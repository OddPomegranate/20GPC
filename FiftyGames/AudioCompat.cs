using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FiftyGames;

// PORTING NOTE (updated): The original Xbox 360 / Windows XNA build of this
// game used the XACT audio pipeline (AudioEngine / WaveBank / SoundBank /
// Cue, compiled from .xgs/.xwb/.xsb project files). MonoGame.Framework.
// DesktopGL actually still ships real AudioEngine/WaveBank/SoundBank/Cue
// classes in Microsoft.Xna.Framework.Audio (an earlier version of this file
// wrongly assumed they'd been removed) - but they can't be used here for two
// separate reasons:
//   1. MonoGame's WaveBank reader only understands little-endian (PC/Windows
//      authored) .xwb files. This game's Content/Sound/*.xwb files were
//      compiled for Xbox 360, which XACT stores big-endian ("DNBW" instead
//      of "WBND") - MonoGame's reader has no byte-swapping and would misread
//      every offset/count in the header.
//   2. Even for a correctly-read header, every entry in both banks is
//      encoded with the XMA codec (Xbox 360's hardware audio codec).
//      MonoGame's DesktopGL/OpenAL backend only decodes PCM and MS-ADPCM -
//      XMA throws NotSupportedException there (XMA decode is only wired up
//      on Xbox/UWP platform backends).
//
// So the original .xwb/.xsb/.xgs files can't be loaded at runtime at all on
// this platform. Instead, the audio was extracted and converted ONCE
// (outside the game, not at runtime): each wave bank entry's raw XMA bytes
// were pulled out by hand-parsing the (big-endian) .xwb headers, wrapped in
// a minimal RIFF/WAVE container declaring the XMA2 codec (the exact
// technique vgmstream's ffmpeg_make_riff_xma2() uses), and decoded to
// ordinary 16-bit PCM .wav via ffmpeg's xma2 decoder. Cue names were
// recovered the same way, by hand-parsing the (also big-endian) .xsb cue
// tables. The results live under Content/Sound/Extracted/:
//   - WaveBank/<NNNN>.wav, MusicWaveBank/<NNNN>.wav - one decoded .wav per
//     original wave bank entry, named by its numeric index in that bank.
//   - cue_map.json - { "wavebank_names": [...], "cues": { "<cue name>":
//     [[wavebankIndex, streamIndex], ...] } }, i.e. exactly the name -> wave
//     lookup the original .xsb encoded, so GetCue("music Menu") etc. keep
//     working unchanged from every game file's point of view.
//
// This file re-implements the same AudioEngine/WaveBank/SoundBank/Cue
// surface the ~50 minigame files already call by name, now backed by real
// MonoGame SoundEffect/SoundEffectInstance loaded from those decoded .wav
// files instead of always-throwing stubs. No changes were needed in any of
// the individual minigame files - only SoundManager.cs additionally tags
// game-sound cues with the "Game" category (see CreateGameSoundCue) so the
// existing menu-music-ducks-game-sounds behavior still works.
//
// Known simplifications vs. real XACT:
//   - Per-sound authored volume trim (decibels) and RPC curves (e.g. the
//     "Filterness" low-pass filter variable LightBikes/Sumo set) are not
//     reimplemented - SetVariable/SetGlobalVariable are no-ops, same as
//     before. MonoGame's SoundEffectInstance has no generic real-time
//     filter to hook a variable like that up to anyway.
//   - Only the 3 category names this game actually uses ("Default",
//     "Music", "Game") are meaningful; volume is a straight multiply of
//     every category a Cue is subscribed to.
//   - When a cue name has multiple recorded variations (XACT's random
//     sound-variation feature), one is picked uniformly at random rather
//     than using the original authored weighting (which wasn't extracted).
//   - Music cues loop by default (IsLooped = true); SFX cues don't. XACT's
//     authored loop regions weren't extracted, but every music cue in this
//     game is meant to loop for as long as that minigame/menu is active.
namespace Microsoft.Xna.Framework.Audio;

public enum AudioStopOptions
{
	AsAuthored,
	Immediate
}

public sealed class AudioCategory
{
	private readonly List<Cue> _cues = new List<Cue>();

	public float Volume { get; private set; } = 1f;

	internal void Register(Cue cue)
	{
		if (!_cues.Contains(cue))
		{
			_cues.Add(cue);
		}
	}

	internal void Unregister(Cue cue)
	{
		_cues.Remove(cue);
	}

	public void SetVolume(float volume)
	{
		Volume = volume;
		// Copy: a cue's Dispose() unregisters itself, which would otherwise
		// mutate _cues while we're iterating it here.
		foreach (var cue in _cues.ToArray())
		{
			cue.ApplyVolume();
		}
	}
}

public sealed class Cue : IDisposable
{
	private readonly SoundEffectInstance _instance;
	private readonly List<AudioCategory> _categories = new List<AudioCategory>();

	// PORTING FIX: real-time parameters set via SetVariable() (below) used to be a
	// pure no-op - the original XACT project drove engine/skid loop volume off a
	// "Speed"/"SkidVolume" RPC curve (0-100) so cars idled quietly and got louder
	// under way; without it every drivingCue/engine cue played at flat full volume
	// from the instant it was created. With up to 4 players each starting their own
	// full-volume engine loop in the same frame (Drift Pixel/MicroMachines - see
	// MMPlayer.cs), that's audible as a loud, layered wash of engine noise right at
	// race start. The exact original curve shape wasn't recoverable, so this applies
	// a reasonable linear approximation (with a soft idle floor so a stationary
	// engine still hums rather than going silent) instead of doing nothing.
	private readonly Dictionary<string, float> _variables = new Dictionary<string, float>();

	public string Name { get; }

	public bool IsDisposed { get; private set; }

	public bool IsPrepared => true;

	public bool IsPlaying => !IsDisposed && _instance != null && _instance.State == SoundState.Playing;

	public bool IsPaused => !IsDisposed && _instance != null && _instance.State == SoundState.Paused;

	public bool IsStopped => IsDisposed || _instance == null || _instance.State == SoundState.Stopped;

	public bool IsStopping => false;

	internal Cue(string name, SoundEffectInstance instance, bool loop)
	{
		Name = name;
		_instance = instance;
		if (_instance != null)
		{
			_instance.IsLooped = loop;
		}
	}

	internal void Subscribe(AudioCategory category)
	{
		if (category == null || IsDisposed || _categories.Contains(category))
		{
			return;
		}
		_categories.Add(category);
		category.Register(this);
		ApplyVolume();
	}

	internal void ApplyVolume()
	{
		if (IsDisposed || _instance == null)
		{
			return;
		}
		float volume = 1f;
		foreach (var category in _categories)
		{
			volume *= category.Volume;
		}
		volume *= GetVariableVolumeScale();
		if (volume < 0f) volume = 0f;
		if (volume > 1f) volume = 1f;
		try
		{
			_instance.Volume = volume;
		}
		catch (ObjectDisposedException)
		{
			// Instance may already have been reclaimed by SoundEffectInstancePool.
		}
	}

	public void Play()
	{
		if (IsDisposed || _instance == null)
		{
			return;
		}
		ApplyVolume();
		try
		{
			_instance.Play();
		}
		catch (Exception)
		{
			// Out of hardware voices, disposed instance, etc. - matches the
			// original "sound is best-effort" behavior instead of crashing
			// gameplay over a dropped sound effect.
		}
	}

	public void Stop(AudioStopOptions options)
	{
		if (IsDisposed || _instance == null)
		{
			return;
		}
		try
		{
			_instance.Stop(options == AudioStopOptions.Immediate);
		}
		catch (ObjectDisposedException)
		{
		}
	}

	public void Pause()
	{
		if (!IsDisposed && IsPlaying)
		{
			_instance.Pause();
		}
	}

	public void Resume()
	{
		if (!IsDisposed && IsPaused)
		{
			_instance.Resume();
		}
	}

	public void SetVariable(string name, float value)
	{
		// Most real-time RPC parameters (e.g. "Filterness") still aren't
		// reimplemented - see the file-level porting note. "Speed" and
		// "SkidVolume" (both authored 0-100, same convention every call site
		// uses - MMPlayer.cs, SuperHighway/Car.cs, TwoTrackTanks/Tank.cs) are
		// approximated as a volume scale instead of being dropped entirely.
		_variables[name] = value;
		ApplyVolume();
	}

	// Linear approximation of this cue's original engine/skid RPC curve: 0 at
	// the variable's minimum maps to a quiet idle rather than silence, 100
	// maps to full volume. Cues with no recognized variable set are unaffected
	// (scale stays 1).
	private float GetVariableVolumeScale()
	{
		const float idleFloor = 0.15f;
		float scale = 1f;
		if (_variables.TryGetValue("Speed", out float speed))
		{
			scale *= idleFloor + (1f - idleFloor) * Clamp01(speed / 100f);
		}
		if (_variables.TryGetValue("SkidVolume", out float skidVolume))
		{
			scale *= idleFloor + (1f - idleFloor) * Clamp01(skidVolume / 100f);
		}
		return scale;
	}

	private static float Clamp01(float value)
	{
		if (value < 0f) return 0f;
		if (value > 1f) return 1f;
		return value;
	}

	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}
		foreach (var category in _categories)
		{
			category.Unregister(this);
		}
		_categories.Clear();
		if (_instance != null && !_instance.IsDisposed)
		{
			try
			{
				_instance.Stop(true);
			}
			catch (Exception)
			{
			}
			_instance.Dispose();
		}
		IsDisposed = true;
	}
}

public sealed class AudioEngine : IDisposable
{
	private readonly Dictionary<string, AudioCategory> _categories = new Dictionary<string, AudioCategory>(StringComparer.OrdinalIgnoreCase);

	internal readonly Dictionary<string, WaveBank> Wavebanks = new Dictionary<string, WaveBank>(StringComparer.OrdinalIgnoreCase);

	public AudioEngine(string settingsFile)
	{
		// settingsFile (Sound.xgs) itself is never parsed - the only
		// categories this game ever asks for are the fixed strings
		// "Default", "Music" and "Game" (see GetCategory), created on
		// first use.
	}

	public AudioCategory GetCategory(string name)
	{
		if (!_categories.TryGetValue(name, out var category))
		{
			category = new AudioCategory();
			_categories[name] = category;
		}
		return category;
	}

	public void SetGlobalVariable(string name, float value)
	{
		// See file-level porting note - RPC/global variables aren't wired
		// up to anything; this matches the previous shim's no-op behavior.
	}

	public void Update()
	{
	}

	public void Dispose()
	{
		foreach (var waveBank in Wavebanks.Values)
		{
			waveBank.Dispose();
		}
		Wavebanks.Clear();
	}
}

public sealed class WaveBank : IDisposable
{
	private readonly string _assetFolder;
	private readonly Dictionary<int, SoundEffect> _cache = new Dictionary<int, SoundEffect>();

	internal string BankName { get; }

	public WaveBank(AudioEngine audioEngine, string nonStreamingWaveBankFilename)
	{
		BankName = Path.GetFileNameWithoutExtension(nonStreamingWaveBankFilename);
		_assetFolder = Path.Combine(AppContext.BaseDirectory, "Content", "Sound", "Extracted", BankName.Replace(" ", string.Empty));
		if (audioEngine != null)
		{
			audioEngine.Wavebanks[BankName] = this;
		}
	}

	internal SoundEffect GetSoundEffect(int index)
	{
		if (_cache.TryGetValue(index, out var cached))
		{
			return cached;
		}

		var path = Path.Combine(_assetFolder, index.ToString("D4") + ".wav");
		if (!File.Exists(path))
		{
			return null;
		}

		try
		{
			using var stream = File.OpenRead(path);
			var effect = SoundEffect.FromStream(stream);
			_cache[index] = effect;
			return effect;
		}
		catch (Exception ex)
		{
			GameConsole.PrintString($"WaveBank: failed to load '{path}': {ex.Message}");
			return null;
		}
	}

	public void Dispose()
	{
		foreach (var effect in _cache.Values)
		{
			effect.Dispose();
		}
		_cache.Clear();
	}
}

public sealed class SoundBank : IDisposable
{
	private readonly AudioEngine _audioEngine;
	private readonly Dictionary<string, List<(string BankName, int Index)>> _cues;
	private static readonly Random _random = new Random();

	public SoundBank(AudioEngine audioEngine, string soundBankFilename)
	{
		_audioEngine = audioEngine;
		_cues = LoadCueMap();
	}

	private static Dictionary<string, List<(string BankName, int Index)>> LoadCueMap()
	{
		var result = new Dictionary<string, List<(string BankName, int Index)>>(StringComparer.Ordinal);

		var path = Path.Combine(AppContext.BaseDirectory, "Content", "Sound", "Extracted", "cue_map.json");
		if (!File.Exists(path))
		{
			GameConsole.PrintString($"SoundBank: cue map not found at '{path}'; sound cues will be silent.");
			return result;
		}

		try
		{
			using var doc = JsonDocument.Parse(File.ReadAllText(path));
			var root = doc.RootElement;

			var bankNames = new List<string>();
			foreach (var el in root.GetProperty("wavebank_names").EnumerateArray())
			{
				bankNames.Add(el.GetString());
			}

			foreach (var cueProp in root.GetProperty("cues").EnumerateObject())
			{
				var variations = new List<(string BankName, int Index)>();
				foreach (var pair in cueProp.Value.EnumerateArray())
				{
					var arr = pair.EnumerateArray();
					arr.MoveNext();
					int bankIndex = arr.Current.GetInt32();
					arr.MoveNext();
					int streamIndex = arr.Current.GetInt32();
					if (bankIndex >= 0 && bankIndex < bankNames.Count)
					{
						variations.Add((bankNames[bankIndex], streamIndex));
					}
				}
				if (variations.Count > 0)
				{
					result[cueProp.Name] = variations;
				}
			}
		}
		catch (Exception ex)
		{
			GameConsole.PrintString($"SoundBank: failed to parse cue map '{path}': {ex.Message}");
		}

		return result;
	}

	public Cue GetCue(string name)
	{
		if (name != null && _cues.TryGetValue(name, out var variations) && variations.Count > 0)
		{
			var (bankName, index) = variations[_random.Next(variations.Count)];
			if (_audioEngine.Wavebanks.TryGetValue(bankName, out var waveBank))
			{
				var soundEffect = waveBank.GetSoundEffect(index);
				if (soundEffect != null)
				{
					bool isMusic = string.Equals(bankName, "Music Wave Bank", StringComparison.OrdinalIgnoreCase);
					var instance = soundEffect.CreateInstance();
					var cue = new Cue(name, instance, isMusic);
					cue.Subscribe(_audioEngine.GetCategory(isMusic ? "Music" : "Default"));
					return cue;
				}
			}
		}

		// Unknown cue name, missing wave bank, or failed load: behave like
		// the original no-op shim (silent Cue) rather than throwing, so a
		// single bad/missing sound never takes down a minigame.
		return new Cue(name, null, false);
	}

	public void Dispose()
	{
	}
}
