using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

public class PlayerManager : GameComponent
{
	private Player[] _playersConnected;

	private GamePadManager[] _gamePads;

	private bool _connect;

	private Color[] _availableColors;

	private GraphicsDevice _graphicsDevice;

	public List<Player> PlayersConnected
	{
		get
		{
			List<Player> list = new List<Player>();
			Player[] playersConnected = _playersConnected;
			foreach (Player player in playersConnected)
			{
				if (player != null)
				{
					list.Add(player);
				}
			}
			return list;
		}
	}

	public int NumberOfPlayers
	{
		get
		{
			int num = 0;
			Player[] playersConnected = _playersConnected;
			foreach (Player player in playersConnected)
			{
				if (player != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool ConnectState
	{
		get
		{
			return _connect;
		}
		set
		{
			_connect = value;
		}
	}

	public Color[] AvailableColors => _availableColors;

	public PlayerManager(Game game)
		: base(game)
	{
		_playersConnected = new Player[4];
		_gamePads = new GamePadManager[4];
		for (int i = 0; i != _gamePads.Length; i++)
		{
			_gamePads[i] = new GamePadManager((PlayerIndex)i);
		}
		_availableColors = new Color[9];
		ref Color reference = ref _availableColors[1];
		reference = new Color(255, 0, 0);
		ref Color reference2 = ref _availableColors[5];
		reference2 = new Color(255, 64, 0);
		ref Color reference3 = ref _availableColors[2];
		reference3 = new Color(255, 255, 0);
		ref Color reference4 = ref _availableColors[3];
		reference4 = new Color(0, 255, 0);
		ref Color reference5 = ref _availableColors[6];
		reference5 = new Color(0, 255, 192);
		ref Color reference6 = ref _availableColors[8];
		reference6 = new Color(0, 127, 255);
		ref Color reference7 = ref _availableColors[0];
		reference7 = new Color(0, 0, 255);
		ref Color reference8 = ref _availableColors[4];
		reference8 = new Color(128, 0, 255);
		ref Color reference9 = ref _availableColors[7];
		reference9 = new Color(255, 0, 192);
		_connect = true;
		SignedInGamer.SignedIn += GamerSignedIn;
	}

	public override void Initialize()
	{
		base.UpdateOrder = 0;
		base.Initialize();
	}

	public override void Update(GameTime gameTime)
	{
		for (int i = 0; i != _gamePads.Length; i++)
		{
			_gamePads[i].Update(gameTime);
		}
		if (_connect)
		{
			for (int j = 0; j != _playersConnected.Length; j++)
			{
				if (_playersConnected[j] != null && _playersConnected[j].GamerProblem)
				{
					_playersConnected[j].Name = "Player " + (j + 1);
					_playersConnected[j].GamerProblem = false;
				}
			}
		}
		base.Update(gameTime);
	}

	private void GamerSignedIn(object sender, SignedInEventArgs e)
	{
		SignedInGamer gamer = e.Gamer;
		Player player = GetPlayer(gamer.PlayerIndex);
		if (player == null)
		{
			for (int i = 0; i < _playersConnected.Length; i++)
			{
				if (_playersConnected[i] != null && _playersConnected[i].Name == gamer.Gamertag)
				{
					_playersConnected[i].GamePadManager = _gamePads[(int)gamer.PlayerIndex];
					_playersConnected[i].GamerProblem = false;
					GameConsole.PrintString("PlayerManager: " + gamer.Gamertag + "Signed back in on game pad " + ((int)gamer.PlayerIndex).ToString() + ". Player " + (int)gamer.PlayerIndex + "'s issue is resolved.");
				}
			}
		}
		else if (player.Name == gamer.Gamertag)
		{
			player.GamerProblem = false;
			GameConsole.PrintString("PlayerManager: " + gamer.Gamertag + "Signed back in. Player " + (int)gamer.PlayerIndex + "'s issue is resolved.");
		}
	}

	public Player GetPlayer(PlayerIndex playerIndex)
	{
		Player player = null;
		Player[] playersConnected = _playersConnected;
		foreach (Player player2 in playersConnected)
		{
			if (player2 != null && player2.PlayerIndex == playerIndex)
			{
				player = player2;
			}
		}
		if (player == null)
		{
			player = _playersConnected[(int)playerIndex];
		}
		return player;
	}

	public GamePadManager GetGamePad(PlayerIndex playerIndex)
	{
		return _gamePads[(int)playerIndex];
	}

	public void PlayerJoin(PlayerIndex playerIndex, StorageManager storageManager, SoundManager soundManager)
	{
		_playersConnected[(int)playerIndex] = new Player(playerIndex, this, soundManager);
		_playersConnected[(int)playerIndex].GamePadManager = _gamePads[(int)playerIndex];
		if (_playersConnected[(int)playerIndex].Gamer != null && (storageManager.DeviceState == StorageManager.StorageDeviceState.Ready || storageManager.DeviceState == StorageManager.StorageDeviceState.Working))
		{
			storageManager.Load(ref _playersConnected[(int)playerIndex], loadCurrentSettings: false);
			_playersConnected[(int)playerIndex].WaitingForProfileLoad = true;
		}
		if (!SelectColor(_playersConnected[(int)playerIndex], _playersConnected[(int)playerIndex].ColorIndex))
		{
			SelectNextColor(_playersConnected[(int)playerIndex]);
		}
	}

	public void JoinDebug(int players, SoundManager soundManager)
	{
		for (int i = 0; i != players; i++)
		{
			_playersConnected[i] = new DebugPlayer(this, soundManager);
			for (int j = 0; j < _gamePads.Length; j++)
			{
				if (_gamePads[j].GamePadStateCurrent.IsConnected)
				{
					_playersConnected[i].GamePadManager = _gamePads[j];
				}
			}
			if (!SelectColor(_playersConnected[i], 0))
			{
				SelectNextColor(_playersConnected[i]);
			}
		}
	}

	public void PlayerLeave(PlayerIndex playerIndex)
	{
		for (int i = 0; i < _playersConnected.Length; i++)
		{
			if (_playersConnected[i] != null && _playersConnected[i].PlayerIndex == playerIndex)
			{
				_playersConnected[i] = null;
			}
		}
	}

	public void KickAllPlayers()
	{
		for (int i = 0; i < _playersConnected.Length; i++)
		{
			_playersConnected[i] = null;
		}
	}

	public bool SelectColor(Player player, byte colorIndex)
	{
		bool flag = true;
		Player[] playersConnected = _playersConnected;
		foreach (Player player2 in playersConnected)
		{
			if (player2 != player && player2 != null && player2.ColorIndex == colorIndex)
			{
				flag = false;
			}
		}
		if (flag)
		{
			player.ColorIndex = colorIndex;
		}
		return flag;
	}

	public void SelectNextColor(Player player)
	{
		int num = player.ColorIndex + 1;
		if (num == _availableColors.Length)
		{
			num = 0;
		}
		while (num != player.ColorIndex)
		{
			if (!SelectColor(player, (byte)num))
			{
				num++;
				if (num == _availableColors.Length)
				{
					num = 0;
				}
			}
		}
	}

	public void SelectPreviousColor(Player player)
	{
		int num = player.ColorIndex - 1;
		if (num == -1)
		{
			num = _availableColors.Length - 1;
		}
		while (num != player.ColorIndex)
		{
			if (!SelectColor(player, (byte)num))
			{
				num--;
				if (num == -1)
				{
					num = _availableColors.Length - 1;
				}
			}
		}
	}

	public Color GetPlayerColor(Player player)
	{
		return GetPlayerColor(player, 0.5f, 1f);
	}

	public Color GetPlayerColor(Player player, float luminescence, float saturation)
	{
		Color color = _availableColors[player.ColorIndex];
		if (saturation != 1f)
		{
			float num = (color.R + color.G + color.B) / 3;
			color = Color.Lerp(new Color(num, num, num), color, saturation);
		}
		if (luminescence > 0.5f)
		{
			color = Color.Lerp(color, Color.White, (luminescence - 0.5f) * 2f);
		}
		else if (luminescence < 0.5f)
		{
			color = Color.Lerp(Color.Black, color, luminescence * 2f);
		}
		return color;
	}

	protected override void OnEnabledChanged(object sender, EventArgs args)
	{
		GamePadManager[] gamePads = _gamePads;
		foreach (GamePadManager gamePadManager in gamePads)
		{
			gamePadManager.HideInput = !base.Enabled;
		}
		base.OnEnabledChanged(sender, args);
	}
}
