using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace FiftyGames;

// PORTING NOTE: The original Xbox 360 build of this game persisted player
// profiles and game data through Microsoft.Xna.Framework.Storage
// (StorageDevice / StorageContainer), which was an Xbox-only API with no
// MonoGame / desktop equivalent - it has been removed entirely. This PC
// port instead stores the same data as plain files under the current
// user's local application data folder. The public surface of this class
// (the state enum, DeviceState, and the Load/Save/Delete overloads) has
// been kept the same as the original so that callers elsewhere (Menu,
// PlayerManager, FiftyGames) did not need to change.
public class StorageManager : GameComponent
{
	public enum StorageDeviceState
	{
		NoDevice,
		NotSelected,
		Selecting,
		Disconnected,
		Full,
		Ready,
		Working
	}

	private enum StorageAction
	{
		LoadGameData,
		LoadProfile,
		LoadSettingsFromProfile,
		SaveGameData,
		SaveProfile,
		SaveFullProfile,
		DeleteProfile
	}

	private StorageDeviceState _deviceState;

	private Queue<StorageAction> _storageQueue;

	private Queue<Player> _profileQueue;

	private PlayerManager _playerManager;

	private SoundManager _soundManager;

	private MinigameMeta[] _minigameData;

	private MinigameMeta[] _sortedMinigameList;

	private byte _titleSafeOffsetLeft;

	private byte _titleSafeOffsetTop;

	private byte _titleSafeOffsetWidth;

	private byte _titleSafeOffsetHeight;

	private readonly string _saveDirectory;

	public Stopwatch timer;

	public Rectangle SavedTitleSafe => new Rectangle(_titleSafeOffsetLeft, _titleSafeOffsetTop, 1024 + _titleSafeOffsetWidth, 576 + _titleSafeOffsetHeight);

	public StorageDeviceState DeviceState => _deviceState;

	public StorageManager(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref MinigameMeta[] minigameMeta)
		: base(game)
	{
		_deviceState = StorageDeviceState.NotSelected;
		_storageQueue = new Queue<StorageAction>();
		_profileQueue = new Queue<Player>();
		_playerManager = playerManager;
		_soundManager = soundManager;
		_minigameData = minigameMeta;
		_titleSafeOffsetLeft = 128;
		_titleSafeOffsetTop = 72;
		_titleSafeOffsetWidth = 0;
		_titleSafeOffsetHeight = 0;
		_saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TwentyGamesSaveData");
	}

	public override void Update(GameTime gameTime)
	{
		switch (_deviceState)
		{
		case StorageDeviceState.Ready:
			if (_storageQueue.Count != 0)
			{
				_deviceState = StorageDeviceState.Working;
				ProcessStorage();
			}
			break;
		case StorageDeviceState.NoDevice:
			if (_storageQueue.Count != 0)
			{
				_storageQueue.Clear();
			}
			if (_profileQueue.Count != 0)
			{
				_profileQueue.Clear();
			}
			break;
		}
		base.Update(gameTime);
	}

	public bool SelectStorageDevice(Player selectPlayer, ref MinigameMeta[] sortedMinigameList)
	{
		_storageQueue.Clear();
		_profileQueue.Clear();
		_sortedMinigameList = sortedMinigameList;
		bool result;
		try
		{
			Directory.CreateDirectory(_saveDirectory);
			_deviceState = StorageDeviceState.Ready;
			_storageQueue.Enqueue(StorageAction.LoadGameData);
			_storageQueue.Enqueue(StorageAction.LoadSettingsFromProfile);
			_profileQueue.Enqueue(selectPlayer);
			result = true;
			GameConsole.PrintString("StorageManager: Using local save folder \"" + _saveDirectory + "\".");
		}
		catch
		{
			_deviceState = StorageDeviceState.NoDevice;
			_storageQueue.Clear();
			_profileQueue.Clear();
			result = false;
			GameConsole.PrintString("StorageManager: Could not access the local save folder.");
		}
		return result;
	}

	private static string SanitizeFileName(string name)
	{
		char[] invalid = Path.GetInvalidFileNameChars();
		StringBuilder stringBuilder = new StringBuilder(name.Length);
		foreach (char c in name)
		{
			stringBuilder.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
		}
		return stringBuilder.Length == 0 ? "_" : stringBuilder.ToString();
	}

	private string ProfilePath(string playerName)
	{
		return Path.Combine(_saveDirectory, SanitizeFileName(playerName) + ".profile");
	}

	private string GameDataPath()
	{
		return Path.Combine(_saveDirectory, "GameData.dat");
	}

	private void LoadProfile(Player loadPlayer, bool settingsOnly)
	{
		float musicVolume = loadPlayer.MusicVolume;
		float effectVolume = loadPlayer.EffectVolume;
		byte sortMode = loadPlayer.SortMode;
		byte colorIndex = loadPlayer.ColorIndex;
		bool allowsVibration = loadPlayer.AllowsVibration;
		string path = ProfilePath(loadPlayer.Name);
		if (File.Exists(path))
		{
			Stream stream = File.Open(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(stream, Encoding.BigEndianUnicode);
			try
			{
				loadPlayer.MusicVolume = (float)Math.Round((double)(int)binaryReader.ReadByte() * 0.1, 1);
				loadPlayer.EffectVolume = (float)Math.Round((double)(int)binaryReader.ReadByte() * 0.1, 1);
				loadPlayer.SortMode = binaryReader.ReadByte();
				if (!settingsOnly)
				{
					loadPlayer.ColorIndex = binaryReader.ReadByte();
					if (!_playerManager.SelectColor(loadPlayer, loadPlayer.ColorIndex))
					{
						_playerManager.SelectNextColor(loadPlayer);
					}
					loadPlayer.AllowsVibration = binaryReader.ReadBoolean();
					GameConsole.PrintString("StorageManager: Loaded settings from profile " + loadPlayer.Name + ".");
				}
				else
				{
					GameConsole.PrintString("StorageManager: Loaded profile " + loadPlayer.Name + ".");
				}
			}
			catch
			{
				loadPlayer.MusicVolume = musicVolume;
				loadPlayer.EffectVolume = effectVolume;
				loadPlayer.SortMode = sortMode;
				loadPlayer.ColorIndex = colorIndex;
				loadPlayer.AllowsVibration = allowsVibration;
				GameConsole.PrintString("StorageManager: Failed to load profile " + loadPlayer.Name + ".");
			}
			finally
			{
				binaryReader.Close();
				binaryReader.Dispose();
				stream.Dispose();
			}
		}
		loadPlayer.WaitingForProfileLoad = false;
	}

	private void SaveProfile(Player savePlayer, bool saveSettings)
	{
		byte sortMode = savePlayer.SortMode;
		byte colorIndex = savePlayer.ColorIndex;
		bool allowsVibration = savePlayer.AllowsVibration;
		if (!saveSettings)
		{
			LoadProfile(savePlayer, settingsOnly: true);
		}
		Directory.CreateDirectory(_saveDirectory);
		Stream stream = File.Open(ProfilePath(savePlayer.Name), FileMode.Create, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.BigEndianUnicode);
		try
		{
			binaryWriter.Write((byte)Math.Round(savePlayer.MusicVolume * 10f, 0));
			binaryWriter.Write((byte)Math.Round(savePlayer.EffectVolume * 10f, 0));
			binaryWriter.Write(sortMode);
			binaryWriter.Write(colorIndex);
			binaryWriter.Write(allowsVibration);
			GameConsole.PrintString("StorageManager: Saved profile " + savePlayer.Name + ".");
		}
		catch
		{
			GameConsole.PrintString("StorageManager: Failed to save profile " + savePlayer.Name + ". File write was interrupted.");
		}
		finally
		{
			binaryWriter.Close();
			binaryWriter.Dispose();
			stream.Dispose();
		}
	}

	private void LoadGameData()
	{
		string path = GameDataPath();
		if (!File.Exists(path))
		{
			return;
		}
		byte[] array = new byte[_minigameData.Length];
		string[] array2 = new string[_minigameData.Length];
		float[] array3 = new float[_minigameData.Length];
		byte titleSafeOffsetLeft = _titleSafeOffsetLeft;
		byte titleSafeOffsetTop = _titleSafeOffsetTop;
		byte titleSafeOffsetWidth = _titleSafeOffsetWidth;
		byte titleSafeOffsetHeight = _titleSafeOffsetHeight;
		Stream stream = File.Open(path, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(stream, Encoding.BigEndianUnicode);
		try
		{
			for (int i = 0; i != _minigameData.Length; i++)
			{
				_minigameData[i].Rating = binaryReader.ReadByte();
				_minigameData[i].BestWinner = binaryReader.ReadString();
				_minigameData[i].BestScore = binaryReader.ReadSingle();
			}
			_titleSafeOffsetLeft = binaryReader.ReadByte();
			_titleSafeOffsetTop = binaryReader.ReadByte();
			_titleSafeOffsetWidth = binaryReader.ReadByte();
			_titleSafeOffsetHeight = binaryReader.ReadByte();
			GameConsole.PrintString("StorageManager: Game data loaded.");
		}
		catch
		{
			for (int j = 0; j != _minigameData.Length; j++)
			{
				_minigameData[j].Rating = array[j];
				_minigameData[j].BestWinner = array2[j];
				_minigameData[j].BestScore = array3[j];
			}
			_titleSafeOffsetLeft = titleSafeOffsetLeft;
			_titleSafeOffsetTop = titleSafeOffsetTop;
			_titleSafeOffsetWidth = titleSafeOffsetWidth;
			_titleSafeOffsetHeight = titleSafeOffsetHeight;
			GameConsole.PrintString("StorageManager: Failed to load game data.");
		}
		finally
		{
			binaryReader.Close();
			binaryReader.Dispose();
			stream.Dispose();
		}
		for (int k = 0; k != _sortedMinigameList.Length; k++)
		{
			for (int l = 0; l != _minigameData.Length; l++)
			{
				if (_sortedMinigameList[k].MinigameID == _minigameData[l].MinigameID)
				{
					_sortedMinigameList[k].Rating = _minigameData[l].Rating;
					_sortedMinigameList[k].BestWinner = _minigameData[l].BestWinner;
					_sortedMinigameList[k].BestScore = _minigameData[l].BestScore;
				}
			}
		}
	}

	private void SaveGameData()
	{
		Directory.CreateDirectory(_saveDirectory);
		Stream stream = File.Open(GameDataPath(), FileMode.Create, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.BigEndianUnicode);
		try
		{
			for (int i = 0; i != _minigameData.Length; i++)
			{
				binaryWriter.Write(_minigameData[i].Rating);
				binaryWriter.Write(_minigameData[i].BestWinner);
				binaryWriter.Write(_minigameData[i].BestScore);
			}
			binaryWriter.Write(_titleSafeOffsetLeft);
			binaryWriter.Write(_titleSafeOffsetTop);
			binaryWriter.Write(_titleSafeOffsetWidth);
			binaryWriter.Write(_titleSafeOffsetHeight);
			GameConsole.PrintString("StorageManager: Saved game data.");
		}
		catch
		{
			GameConsole.PrintString("StorageManager: Failed to save game data. File write was interrupted.");
		}
		finally
		{
			binaryWriter.Close();
			binaryWriter.Dispose();
			stream.Dispose();
		}
	}

	public void Load(ref MinigameMeta[] sortedMinigameList)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			_storageQueue.Enqueue(StorageAction.LoadGameData);
			GameConsole.PrintString("StorageManager: Load game data request queued.");
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Load Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Load Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Load Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Load Cancelled");
		}
		_sortedMinigameList = sortedMinigameList;
	}

	public void Load(ref Player player, bool loadCurrentSettings)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			if (loadCurrentSettings)
			{
				_storageQueue.Enqueue(StorageAction.LoadSettingsFromProfile);
				GameConsole.PrintString("StorageManager: Load profile (" + player.Name + ") settings request queued.");
			}
			else
			{
				_storageQueue.Enqueue(StorageAction.LoadProfile);
				GameConsole.PrintString("StorageManager: Load profile (" + player.Name + ") request queued.");
			}
			_profileQueue.Enqueue(player);
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Load Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Load Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Load Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Load Cancelled");
		}
	}

	public void Save(Rectangle titleSafeRect)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			_titleSafeOffsetLeft = ((titleSafeRect.Left == 256) ? byte.MaxValue : ((byte)titleSafeRect.Left));
			_titleSafeOffsetTop = (byte)titleSafeRect.Top;
			_titleSafeOffsetWidth = ((titleSafeRect.Width == 1280) ? byte.MaxValue : ((byte)(titleSafeRect.Width - 1024)));
			_titleSafeOffsetHeight = (byte)(titleSafeRect.Height - 576);
			_storageQueue.Enqueue(StorageAction.SaveGameData);
			GameConsole.PrintString("StorageManager: Save game data (title safe) request queued.");
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Save Cancelled");
		}
	}

	public void Save(MinigameMeta minigameData)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			for (int i = 0; i != _minigameData.Length; i++)
			{
				if (_minigameData[i].MinigameID == minigameData.MinigameID)
				{
					_minigameData[i].Rating = minigameData.Rating;
					_minigameData[i].BestScore = minigameData.BestScore;
					_minigameData[i].BestWinner = minigameData.BestWinner;
				}
			}
			_storageQueue.Enqueue(StorageAction.SaveGameData);
			GameConsole.PrintString("StorageManager: Save game data (" + minigameData.Name + ") request queued.");
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Save Cancelled");
		}
	}

	public void Save(MinigameMeta[] minigameData)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			for (int i = 0; i < minigameData.Length; i++)
			{
				for (int j = 0; j != _minigameData.Length; j++)
				{
					if (_minigameData[j].MinigameID == minigameData[i].MinigameID)
					{
						_minigameData[j].Rating = minigameData[i].Rating;
						_minigameData[j].BestScore = minigameData[i].BestScore;
						_minigameData[j].BestWinner = minigameData[i].BestWinner;
					}
				}
			}
			_storageQueue.Enqueue(StorageAction.SaveGameData);
			GameConsole.PrintString("StorageManager: Save game data (minigame list) request queued.");
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Save Cancelled");
		}
	}

	public void Save(Player player, bool saveCurrentSettings)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			if (saveCurrentSettings)
			{
				_storageQueue.Enqueue(StorageAction.SaveFullProfile);
				GameConsole.PrintString("StorageManager: Save profile (" + player.Name + ") request queued.");
			}
			else
			{
				_storageQueue.Enqueue(StorageAction.SaveProfile);
				GameConsole.PrintString("StorageManager: Save profile (" + player.Name + ") settings request queued.");
			}
			_profileQueue.Enqueue(player);
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Save Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Save Cancelled");
		}
	}

	public void Delete(ref Player player)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			_storageQueue.Enqueue(StorageAction.DeleteProfile);
			GameConsole.PrintString("StorageManager: Delete profile (" + player.Name + ") request queued.");
			_profileQueue.Enqueue(player);
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Delete Cancelled");
		}
	}

	public void Delete(MinigameMeta minigameData, bool ratings)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			for (int i = 0; i != _minigameData.Length; i++)
			{
				if (_minigameData[i].MinigameID == minigameData.MinigameID)
				{
					_minigameData[i].Rating = 0;
					_minigameData[i].BestScore = 0f;
					_minigameData[i].BestWinner = "";
				}
			}
			_storageQueue.Enqueue(StorageAction.SaveGameData);
			GameConsole.PrintString("StorageManager: Delete game data (" + minigameData.Name + (ratings ? " ratings" : " scores") + ") request queued.");
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Delete Cancelled");
		}
	}

	public void Delete(MinigameMeta[] minigameData, bool ratings)
	{
		if (_deviceState == StorageDeviceState.Ready || _deviceState == StorageDeviceState.Working || _deviceState == StorageDeviceState.Selecting)
		{
			for (int i = 0; i < minigameData.Length; i++)
			{
				for (int j = 0; j != _minigameData.Length; j++)
				{
					if (_minigameData[j].MinigameID == minigameData[i].MinigameID)
					{
						_minigameData[j].Rating = 0;
						_minigameData[j].BestScore = 0f;
						_minigameData[j].BestWinner = "";
					}
				}
			}
			_storageQueue.Enqueue(StorageAction.SaveGameData);
			GameConsole.PrintString("StorageManager: Delete game data (minigame" + (ratings ? " ratings" : " scores") + ") request queued.");
		}
		else if (_deviceState == StorageDeviceState.NoDevice)
		{
			GameConsole.PrintString("StorageManager: No storage Device. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Disconnected)
		{
			GameConsole.PrintString("StorageManager: Storage device missing. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.Full)
		{
			GameConsole.PrintString("StorageManager: Not enough space available on storage device. Delete Cancelled");
		}
		else if (_deviceState == StorageDeviceState.NotSelected)
		{
			GameConsole.PrintString("StorageManager: No storage device has been selected. Delete Cancelled");
		}
	}

	private void ProcessStorage()
	{
		while (_storageQueue.Count != 0)
		{
			switch (_storageQueue.Dequeue())
			{
			case StorageAction.LoadGameData:
				LoadGameData();
				break;
			case StorageAction.LoadProfile:
			{
				Player player = _profileQueue.Dequeue();
				LoadProfile(player, settingsOnly: false);
				break;
			}
			case StorageAction.LoadSettingsFromProfile:
			{
				Player player = _profileQueue.Dequeue();
				LoadProfile(player, settingsOnly: false);
				_soundManager.MusicVolume = player.MusicVolume;
				_soundManager.EffectVolume = player.EffectVolume;
				break;
			}
			case StorageAction.SaveGameData:
				SaveGameData();
				break;
			case StorageAction.SaveProfile:
			{
				Player player = _profileQueue.Dequeue();
				if (player.Gamer != null && player.Name != "Default")
				{
					SaveProfile(player, saveSettings: false);
				}
				break;
			}
			case StorageAction.SaveFullProfile:
			{
				Player player = _profileQueue.Dequeue();
				if (player.Gamer != null && player.Name != "Default")
				{
					SaveProfile(player, saveSettings: true);
				}
				break;
			}
			case StorageAction.DeleteProfile:
			{
				Player player = _profileQueue.Dequeue();
				string path = ProfilePath(player.Name);
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				break;
			}
			}
		}
		_deviceState = StorageDeviceState.Ready;
	}
}
