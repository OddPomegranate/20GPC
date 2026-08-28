using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace FiftyGames;

public class SoundManager : GameComponent
{
	private AudioEngine _audioEngine;

	private WaveBank _waveBank;

	private WaveBank _waveBankMusic;

	private SoundBank _soundBank;

	private string _currentSong;

	private Cue _menuMusic;

	private Cue _gameMusic;

	private List<Cue> _preloadedSounds;

	private List<Cue> _pausedSounds;

	private List<Cue> _menuSounds;

	private List<Cue> _gameSounds;

	private Stack<Cue> _usedCues;

	private float _musicVolume;

	private float _effectVolume;

	public float EffectVolume
	{
		get
		{
			float result = 0f;
			if (base.Enabled)
			{
				result = _effectVolume;
			}
			return result;
		}
		set
		{
			if (base.Enabled)
			{
				_effectVolume = value;
				_audioEngine.GetCategory("Default").SetVolume(_effectVolume);
				GameConsole.PrintString("SoundManager: Sound effect volume set to " + _effectVolume + ".");
			}
		}
	}

	public float MusicVolume
	{
		get
		{
			float result = 0f;
			if (base.Enabled)
			{
				result = _musicVolume;
			}
			return result;
		}
		set
		{
			if (base.Enabled)
			{
				_musicVolume = value;
				_audioEngine.GetCategory("Music").SetVolume(_musicVolume);
				GameConsole.PrintString("SoundManager: Music volume set to " + _musicVolume + ".");
			}
		}
	}

	public SoundManager(Game game)
		: base(game)
	{
		_currentSong = string.Empty;
		_gameMusic = null;
		_usedCues = new Stack<Cue>();
		_menuSounds = new List<Cue>();
		_gameSounds = new List<Cue>();
		_pausedSounds = new List<Cue>();
		_musicVolume = 0.6f;
		_effectVolume = 1f;
	}

	public override void Initialize()
	{
		try
		{
			_audioEngine = new AudioEngine("Content/Sound/Sound.xgs");
			_waveBank = new WaveBank(_audioEngine, "Content/Sound/Wave Bank.xwb");
			_waveBankMusic = new WaveBank(_audioEngine, "Content/Sound/Music Wave Bank.xwb");
			_soundBank = new SoundBank(_audioEngine, "Content/Sound/Sound Bank.xsb");
			_audioEngine.GetCategory("Music").SetVolume(_musicVolume);
			_audioEngine.GetCategory("Default").SetVolume(_effectVolume);
			GameConsole.PrintString("SoundManager: Sound manager initialisation successful.");
		}
		catch
		{
			base.Enabled = false;
			GameConsole.PrintString("SoundManager: Sound manager initialisation failed. Sound is disabled.");
		}
		base.Initialize();
	}

	public override void Update(GameTime gameTime)
	{
		if (base.Enabled)
		{
			if (_preloadedSounds != null)
			{
				for (int i = 0; i < _preloadedSounds.Count; i++)
				{
					if (_preloadedSounds[i].IsPrepared)
					{
						_preloadedSounds[i].Play();
						_preloadedSounds[i].Stop(AudioStopOptions.Immediate);
						_usedCues.Push(_preloadedSounds[i]);
						_preloadedSounds.RemoveAt(i);
						i--;
					}
				}
				if (_preloadedSounds.Count == 0)
				{
					_preloadedSounds = null;
				}
			}
			for (int j = 0; j < _gameSounds.Count; j++)
			{
				if (_gameSounds[j].IsStopped || _gameSounds[j].IsDisposed)
				{
					_usedCues.Push(_gameSounds[j]);
					_gameSounds.RemoveAt(j);
					j--;
				}
			}
			for (int k = 0; k < _menuSounds.Count; k++)
			{
				if (_menuSounds[k].IsStopped || _menuSounds[k].IsDisposed)
				{
					_usedCues.Push(_menuSounds[k]);
					_menuSounds.RemoveAt(k);
					k--;
				}
			}
		}
		if (base.Enabled)
		{
			_audioEngine.Update();
		}
		base.Update(gameTime);
	}

	// FIX (music-doesn't-stop bug): ChangeToMenuMusic/ChangeToGameMusic used to
	// just overwrite the _menuMusic/_gameMusic field with a brand-new Cue and
	// Play() it, never stopping whatever cue was already playing under the old
	// field value. Since AudioCompat's music cues always loop (see the class's
	// own header comment), the abandoned cue's underlying SoundEffectInstance
	// just kept looping forever with nothing left referencing it to stop it --
	// reported as: exiting a game back to the menu leaves the game's music
	// playing under the menu music, swapping games layers the new game's music
	// over the old one, and re-entering the same game layers a second copy of
	// the same track. StopCurrentMusic() (same Stop-then-null pattern already
	// used by OnEnabledChanged below) is now called before starting any new
	// music cue, so at most one music cue is ever playing at a time.
	private void StopCurrentMusic()
	{
		if (_menuMusic != null && !_menuMusic.IsStopped)
		{
			_menuMusic.Stop(AudioStopOptions.Immediate);
		}
		_menuMusic = null;
		if (_gameMusic != null && !_gameMusic.IsStopped)
		{
			_gameMusic.Stop(AudioStopOptions.Immediate);
		}
		_gameMusic = null;
	}

	public void ChangeToMenuMusic()
	{
		if (base.Enabled)
		{
			StopCurrentMusic();
			_menuMusic = _soundBank.GetCue("music Menu");
			_menuMusic.Play();
			GameConsole.PrintString("SoundManager: Music changed to 'music Menu'.");
			_audioEngine.GetCategory("Game").SetVolume(0f);
			GameConsole.PrintString("SoundManager: Game sounds muted.");
		}
	}

	public void ChangeToGameMusic(string songName)
	{
		if (base.Enabled)
		{
			StopCurrentMusic();
			_currentSong = songName;
			_gameMusic = _soundBank.GetCue(_currentSong);
			_gameMusic.Play();
			GameConsole.PrintString("SoundManager: Music changed to '" + _currentSong + "'.");
			_audioEngine.GetCategory("Game").SetVolume(_effectVolume);
			GameConsole.PrintString("SoundManager: Game sounds reverted to " + _effectVolume + ".");
		}
	}

	public Cue CreateGameSoundCue(string cueFriendlyName)
	{
		Cue cue = null;
		if (base.Enabled)
		{
			if (_usedCues.Count != 0)
			{
				_usedCues.Pop();
			}
			cue = _soundBank.GetCue(cueFriendlyName);
			// Tag game-sound cues with the "Game" category (on top of the
			// "Default" category SoundBank.GetCue already subscribed them
			// to) so ChangeToMenuMusic/ChangeToGameMusic's Game-category
			// mute/unmute still ducks in-game sound effects while leaving
			// menu sound effects (CreateMenuSoundCue) alone.
			cue.Subscribe(_audioEngine.GetCategory("Game"));
			_gameSounds.Add(cue);
		}
		return cue;
	}

	public Cue CreateMenuSoundCue(string cueFriendlyName)
	{
		Cue cue = null;
		if (base.Enabled)
		{
			if (_usedCues.Count != 0)
			{
				_usedCues.Pop();
			}
			cue = _soundBank.GetCue(cueFriendlyName);
			_menuSounds.Add(cue);
		}
		return cue;
	}

	public void PreloadSounds(string[] cueNames)
	{
		if (base.Enabled)
		{
			_preloadedSounds = new List<Cue>();
			for (int i = 0; i < cueNames.Length; i++)
			{
				_preloadedSounds.Add(_soundBank.GetCue(cueNames[i]));
			}
		}
	}

	public void SetGlobalVariable(string variableName, float variableValue)
	{
		if (base.Enabled)
		{
			_audioEngine.SetGlobalVariable(variableName, variableValue);
		}
	}

	public void ClearGameSounds()
	{
		if (!base.Enabled)
		{
			return;
		}
		ResumeGameSounds();
		for (int i = 0; i < _gameSounds.Count; i++)
		{
			if (!_gameSounds[i].IsStopped)
			{
				_gameSounds[i].Stop(AudioStopOptions.Immediate);
			}
			_gameSounds[i].Dispose();
		}
		_gameSounds.Clear();
		_usedCues.Clear();
	}

	public void PauseGameSounds()
	{
		if (!base.Enabled)
		{
			return;
		}
		for (int i = 0; i != _gameSounds.Count; i++)
		{
			if (_gameSounds[i].IsPlaying && !_gameSounds[i].IsPaused && !_gameSounds[i].IsStopped && !_gameSounds[i].IsStopping && !_gameSounds[i].IsDisposed)
			{
				_gameSounds[i].Pause();
				_pausedSounds.Add(_gameSounds[i]);
			}
		}
	}

	public void ResumeGameSounds()
	{
		if (!base.Enabled)
		{
			return;
		}
		for (int i = 0; i != _pausedSounds.Count; i++)
		{
			if (_pausedSounds[i].IsPlaying && _pausedSounds[i].IsPaused && !_pausedSounds[i].IsStopped && !_pausedSounds[i].IsStopping && !_pausedSounds[i].IsDisposed)
			{
				_pausedSounds[i].Resume();
			}
		}
		_pausedSounds.Clear();
	}

	protected override void OnEnabledChanged(object sender, EventArgs args)
	{
		if (!base.Enabled)
		{
			if (_preloadedSounds != null)
			{
				for (int i = 0; i < _preloadedSounds.Count; i++)
				{
					if (!_preloadedSounds[i].IsStopped && !_preloadedSounds[i].IsDisposed)
					{
						_preloadedSounds[i].Stop(AudioStopOptions.Immediate);
						_preloadedSounds[i].Dispose();
					}
				}
				_preloadedSounds.Clear();
			}
			for (int j = 0; j < _pausedSounds.Count; j++)
			{
				if (!_pausedSounds[j].IsStopped && !_pausedSounds[j].IsDisposed)
				{
					_pausedSounds[j].Stop(AudioStopOptions.Immediate);
					_pausedSounds[j].Dispose();
				}
			}
			_pausedSounds.Clear();
			for (int k = 0; k < _gameSounds.Count; k++)
			{
				if (!_gameSounds[k].IsStopped && !_gameSounds[k].IsDisposed)
				{
					_gameSounds[k].Stop(AudioStopOptions.Immediate);
					_gameSounds[k].Dispose();
				}
			}
			_gameSounds.Clear();
			for (int l = 0; l < _menuSounds.Count; l++)
			{
				if (!_menuSounds[l].IsStopped && !_menuSounds[l].IsDisposed)
				{
					_menuSounds[l].Stop(AudioStopOptions.Immediate);
					_menuSounds[l].Dispose();
				}
			}
			_menuSounds.Clear();
			if (_menuMusic != null && !_menuMusic.IsStopped)
			{
				_menuMusic.Stop(AudioStopOptions.Immediate);
				_menuMusic = null;
			}
			if (_gameMusic != null && !_gameMusic.IsStopped)
			{
				_gameMusic.Stop(AudioStopOptions.Immediate);
				_gameMusic = null;
			}
		}
		base.OnEnabledChanged(sender, args);
	}
}
