using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zombie;

namespace FiftyGames.RiskyRiskyRisk;

internal class RiskyRiskyRisk : Minigame
{
	private SpriteBatch _spriteBatch;

	private SpriteFont _font;

	private Texture2D _background;

	private Texture2D _arrow;

	private Rectangle _bgSize;

	private Commander[] _commanders;

	private Random _random;

	private Hex[,] _hexagons;

	private int[,] _decays;

	private List<Country> _countries;

	private RenderTarget2D _rtEffect;

	private RenderTarget2D _rtMaster;

	private RenderTarget2D _rtShadow;

	private Effect _effects;

	private FullscreenQuad _fsq;

	private int _numPlayers;

	private int _numCommanders;

	private bool _isWaitingForAI;

	private Vector2 _aiTextSize;

	private BattleManager _battleManager;

	private bool dunGoofed;

	public RiskyRiskyRisk(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_fsq = new FullscreenQuad(base.GraphicsDevice);
		_random = new Random();
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_effects = _contentManager.Load<Effect>("RiskyRiskyRisk/Effects/InnerGlow");
		_rtEffect = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		_rtMaster = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		_rtShadow = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		_background = _contentManager.Load<Texture2D>("RiskyRiskyRisk/Sprites/Background");
		_bgSize = _background.Bounds;
		_arrow = _contentManager.Load<Texture2D>("RiskyRiskyRisk/Sprites/Arrow");
		_aiTextSize = _font.MeasureString("Select number of AI players: 5");
		_countries = new List<Country>();
		_numPlayers = _playerManager.NumberOfPlayers;
		if (_demoMode)
		{
			_numCommanders = 3;
			_isWaitingForAI = false;
			_numPlayers = 0;
		}
		else
		{
			_isWaitingForAI = true;
			_numCommanders = 6;
		}
		if (_demoMode)
		{
			do
			{
				dunGoofed = false;
				ReloadContent();
			}
			while (dunGoofed);
		}
	}

	protected void ReloadContent()
	{
		_commanders = new Commander[_numCommanders];
		bool[] array = new bool[_playerManager.AvailableColors.Length];
		List<int> list = new List<int>();
		list.Add(0);
		list.Add(1);
		list.Add(2);
		list.Add(3);
		list.Add(4);
		List<int> list2 = list;
		Helper.Shuffle(list2, _random);
		for (int i = 0; i != _numCommanders; i++)
		{
			if (!_demoMode && i < _numPlayers)
			{
				_commanders[i] = new Commander(_playerManager.PlayersConnected[i], _playerManager.GetPlayerColor(_playerManager.PlayersConnected[i]), _numCommanders, i, ref _playerManager, ref _soundManager);
				_commanders[i].LoadContent(base.GraphicsDevice, _contentManager);
				array[_playerManager.PlayersConnected[i].ColorIndex] = true;
				continue;
			}
			int num = 0;
			for (int j = 0; j != _playerManager.AvailableColors.Length; j++)
			{
				if (!array[j])
				{
					num = j;
					array[j] = true;
					break;
				}
			}
			switch (list2[i - _numPlayers])
			{
			case 0:
				_commanders[i] = new AIAgressive(_playerManager.AvailableColors[num], _numCommanders, i, ref _playerManager, ref _soundManager);
				break;
			case 1:
				_commanders[i] = new AIExplorer(_playerManager.AvailableColors[num], _numCommanders, i, ref _playerManager, ref _soundManager);
				break;
			case 2:
				_commanders[i] = new AIDefensive(_playerManager.AvailableColors[num], _numCommanders, i, ref _playerManager, ref _soundManager);
				break;
			case 3:
				_commanders[i] = new AISix(_playerManager.AvailableColors[num], _numCommanders, i, ref _playerManager, ref _soundManager);
				break;
			case 4:
				_commanders[i] = new AIClever(_playerManager.AvailableColors[num], _numCommanders, i, ref _playerManager, ref _soundManager);
				break;
			}
		}
		_countries.Clear();
		CreateMap();
		if (!dunGoofed)
		{
			SetUpMap();
		}
	}

	public override void Update(GameTime gameTime)
	{
		if (_isWaitingForAI)
		{
			if (_playerManager.PlayersConnected[0].GamePadManager.ButtonWasPressed(Buttons.A))
			{
				do
				{
					dunGoofed = false;
					ReloadContent();
				}
				while (dunGoofed);
				_isWaitingForAI = false;
			}
			else if (_playerManager.PlayersConnected[0].GamePadManager.ButtonWasPressed(Buttons.LeftThumbstickUp) || _playerManager.PlayersConnected[0].GamePadManager.ButtonWasPressed(Buttons.DPadUp))
			{
				_numCommanders = Math.Max(Math.Min(_numCommanders + 1, 6), Math.Max(_numPlayers, 2));
			}
			else if (_playerManager.PlayersConnected[0].GamePadManager.ButtonWasPressed(Buttons.LeftThumbstickDown) || _playerManager.PlayersConnected[0].GamePadManager.ButtonWasPressed(Buttons.DPadDown))
			{
				_numCommanders = Math.Max(Math.Min(_numCommanders - 1, 6), Math.Max(_numPlayers, 2));
			}
			return;
		}
		if (!_battleManager.IsGameOver)
		{
			_battleManager.CurrentCommander.Update(_countries, gameTime, _random, ref _battleManager);
		}
		foreach (Country country in _countries)
		{
			country.Update(gameTime);
		}
		_battleManager.Update(gameTime);
		if (_battleManager.IsGameOverFinished)
		{
			if (!_demoMode)
			{
				foreach (Player item in _playerManager.PlayersConnected)
				{
					if (item.GamePadManager.ButtonWasPressed(Buttons.A))
					{
						do
						{
							dunGoofed = false;
							ReloadContent();
						}
						while (dunGoofed);
					}
				}
			}
			else
			{
				do
				{
					dunGoofed = false;
					ReloadContent();
				}
				while (dunGoofed);
			}
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		if (!_isWaitingForAI)
		{
			_spriteBatch.GraphicsDevice.SetRenderTarget(_rtMaster);
			base.GraphicsDevice.Clear(Color.Transparent);
			_spriteBatch.Begin();
			for (int i = 0; i != _countries.Count; i++)
			{
				_countries[i].Draw(_spriteBatch);
				_countries[i].DrawDice(_spriteBatch);
			}
			_spriteBatch.End();
			base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		}
		for (int j = 0; j != 30; j++)
		{
			_spriteBatch.Begin();
			float num = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 750.0);
			_spriteBatch.Draw(_background, new Vector2((j % 10 - 1) * _bgSize.Width, (j / 10 - 1) * _bgSize.Height) + new Vector2((int)(gameTime.TotalGameTime.TotalMilliseconds / 100.0) % _bgSize.Width, num * 10f), null, Color.CornflowerBlue);
			_spriteBatch.End();
		}
		if (_isWaitingForAI)
		{
			_spriteBatch.Begin();
			if (_numCommanders < 6)
			{
				_spriteBatch.Draw(_arrow, new Vector2(640f + _aiTextSize.X / 2f - (float)_arrow.Width + 5f, 330f), Color.White);
			}
			if (_numCommanders > Math.Max(2, _numPlayers))
			{
				_spriteBatch.Draw(_arrow, new Vector2(640f + _aiTextSize.X / 2f - (float)_arrow.Width + 5f, 340f + _aiTextSize.Y), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
			}
			Helper.DrawOutlinedText(_spriteBatch, _font, "Select number of AI players: " + (_numCommanders - _numPlayers), new Vector2(640f - _aiTextSize.X / 2f, 340f), Color.White, Color.Black, Helper.OutlineType.Orthogonal);
			_spriteBatch.End();
		}
		else
		{
			_spriteBatch.Begin();
			_spriteBatch.Draw(_rtShadow, new Vector2(0f, 5f), Color.Black * 0.7f);
			_spriteBatch.Draw(_rtMaster, Vector2.Zero, Color.White);
			if (!_battleManager.IsGameOver)
			{
				for (int k = 0; k < _numCommanders; k++)
				{
					if (_commanders[k].IsMyTurn)
					{
						_commanders[k].Draw(_spriteBatch, _font);
					}
				}
			}
			_battleManager.Draw(_spriteBatch, _font, _titleSafeArea);
			_spriteBatch.End();
		}
		base.Draw(gameTime);
	}

	private void SetUpMap()
	{
		Helper.Shuffle(_countries, _random);
		int num = 0;
		for (int i = 0; i != _numCommanders; i++)
		{
			bool flag;
			do
			{
				flag = false;
				foreach (Country connectedCountry in _countries[num].ConnectedCountries)
				{
					if (connectedCountry.Owner != null || connectedCountry.HexCount < 6)
					{
						flag = true;
						num++;
						break;
					}
				}
			}
			while (flag && num != _countries.Count);
			if (num < _countries.Count)
			{
				_countries[num].Dice = 6;
				_commanders[i].AddCountry(_countries[num]);
				num++;
			}
		}
		for (int j = 0; j != _countries.Count; j++)
		{
			BattleManager.MaxCountrySize = Math.Max(_countries[j].HexCount, BattleManager.MaxCountrySize);
		}
		_battleManager = new BattleManager(_commanders, _numCommanders, _numPlayers, ref _random, _countries.Count, _demoMode, _soundManager, _titleSafeArea);
		_battleManager.LoadContent(_contentManager, _titleSafeArea);
		_countries.Sort(Country.SortSelected);
	}

	private void CreateMap()
	{
		_hexagons = new Hex[32, 20];
		_decays = new int[_hexagons.GetLength(0), _hexagons.GetLength(1)];
		for (ushort num = 0; num != _hexagons.GetLength(1); num++)
		{
			for (ushort num2 = 0; num2 != _hexagons.GetLength(0); num2++)
			{
				_hexagons[num2, num] = new Hex(new Point(num2, num), (num != 0 && num2 != 0 && num != _hexagons.GetLength(1) - 1 && num2 != _hexagons.GetLength(0) - 1 && _random.Next(_hexagons.Length) / 5 != 0) ? true : false, ref _random, 0.7f);
				_hexagons[num2, num].LoadContent(_contentManager);
			}
		}
		Erode(3);
		Erode(2);
		Erode(1);
		Erode(4);
		Erode(4);
		List<Hex> hexes = new List<Hex>();
		for (ushort num3 = 0; num3 != _hexagons.GetLength(1); num3++)
		{
			for (ushort num4 = 0; num4 != _hexagons.GetLength(0); num4++)
			{
				if (_hexagons[num4, num3].IsActive)
				{
					hexes.Add(_hexagons[num4, num3]);
				}
				else
				{
					_hexagons[num4, num3].Filled = true;
				}
			}
		}
		int num5 = 1;
		int depth = 0;
		int size = 0;
		while (hexes.Count != 0)
		{
			FloodFill(ref hexes, hexes[0], num5, depth, ref size, -1, -1);
			if (dunGoofed)
			{
				return;
			}
			num5++;
		}
		int[] array = new int[num5];
		for (ushort num6 = 0; num6 != _hexagons.GetLength(1); num6++)
		{
			for (ushort num7 = 0; num7 != _hexagons.GetLength(0); num7++)
			{
				array[_hexagons[num7, num6].Pass]++;
			}
		}
		num5 = 1;
		for (ushort num8 = 1; num8 != array.Length; num8++)
		{
			if (array[num8] > array[num5])
			{
				num5 = num8;
			}
		}
		if (array[num5] > 290)
		{
			for (ushort num9 = 0; num9 != _hexagons.GetLength(1); num9++)
			{
				for (ushort num10 = 0; num10 != _hexagons.GetLength(0); num10++)
				{
					if (_hexagons[num10, num9].Pass != num5)
					{
						_hexagons[num10, num9].IsActive = false;
					}
					else
					{
						_hexagons[num10, num9].Filled = false;
					}
				}
			}
			int num11 = 1;
			_countries.Clear();
			while (num11 != 0)
			{
				num11 = CreateCountry();
			}
			ConnectCountries();
			for (int i = 0; i != _countries.Count; i++)
			{
				if (_countries[i].HexCount == 0)
				{
					foreach (Country connectedCountry in _countries[i].ConnectedCountries)
					{
						connectedCountry.RemoveConnectedCountry(_countries[i]);
					}
					_countries.RemoveAt(i);
					i--;
				}
				else
				{
					_countries[i].LoadContent(_contentManager);
					_countries[i].CreateTexture(_fsq, _spriteBatch, _effects, _random);
				}
			}
			base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
			_spriteBatch.GraphicsDevice.SetRenderTarget(_rtShadow);
			base.GraphicsDevice.Clear(Color.Transparent);
			_spriteBatch.Begin();
			for (int j = 0; j != _countries.Count; j++)
			{
				_countries[j].Draw(_spriteBatch);
			}
			_spriteBatch.End();
			base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		}
		else
		{
			CreateMap();
		}
	}

	private void ConnectCountries()
	{
		for (int i = 0; i != _countries.Count; i++)
		{
			for (int j = 0; j < _countries[i].HexCount; j++)
			{
				Point point = _countries[i].HexPPos(j);
				int num = point.Y % 2;
				EdgeClip edges = GetEdges(point.X, point.Y, hex: true, num);
				if ((edges & EdgeClip.Left) == 0)
				{
					Connect(_countries[i], _hexagons[point.X - 1, point.Y]);
				}
				if ((edges & EdgeClip.Right) == 0)
				{
					Connect(_countries[i], _hexagons[point.X + 1, point.Y]);
				}
				if ((edges & EdgeClip.UpLeft) == 0)
				{
					Connect(_countries[i], _hexagons[point.X - (1 - num), point.Y - 1]);
				}
				if ((edges & EdgeClip.UpRight) == 0)
				{
					Connect(_countries[i], _hexagons[point.X + num, point.Y - 1]);
				}
				if ((edges & EdgeClip.DownLeft) == 0)
				{
					Connect(_countries[i], _hexagons[point.X - (1 - num), point.Y + 1]);
				}
				if ((edges & EdgeClip.DownRight) == 0)
				{
					Connect(_countries[i], _hexagons[point.X + num, point.Y + 1]);
				}
			}
		}
		for (int k = 0; k != _countries.Count; k++)
		{
			if (_countries[k].HexCount >= 6)
			{
				continue;
			}
			int index = 0;
			for (int l = 0; l != _countries[k].ConnectedCountries.Count; l++)
			{
				if (_countries[k].ConnectedCountries[l].HexCount < _countries[k].ConnectedCountries[index].HexCount)
				{
					index = l;
				}
			}
			_countries[k].ConnectedCountries[index].Combine(_countries[k]);
		}
	}

	private void Connect(Country country, Hex hex)
	{
		if (hex.IsActive && hex.Country != country)
		{
			country.AddConnectedCountry(hex.Country);
			hex.Country.AddConnectedCountry(country);
		}
	}

	private int CreateCountry()
	{
		Country country = new Country();
		List<Hex> hexes = new List<Hex>();
		for (ushort num = 0; num != _hexagons.GetLength(1); num++)
		{
			for (ushort num2 = 0; num2 != _hexagons.GetLength(0); num2++)
			{
				if (!_hexagons[num2, num].Filled)
				{
					hexes.Add(_hexagons[num2, num]);
					_hexagons[num2, num].Pass = 0;
				}
				else
				{
					_hexagons[num2, num].Pass = 0;
				}
			}
		}
		if (hexes.Count == 0)
		{
			return 0;
		}
		int num3 = 1;
		int depth = 0;
		int size = 0;
		FloodFill(ref hexes, hexes[0], 1, depth, ref size, _random.Next(5, 8), BattleManager.MaxCountrySize);
		for (ushort num4 = 0; num4 != _hexagons.GetLength(1); num4++)
		{
			for (ushort num5 = 0; num5 != _hexagons.GetLength(0); num5++)
			{
				if (_hexagons[num5, num4].Pass == num3)
				{
					country.AddHex(_hexagons[num5, num4]);
				}
			}
		}
		_countries.Add(country);
		return hexes.Count;
	}

	private EdgeClip GetEdges(int X, int Y, bool hex, int rowOffset)
	{
		EdgeClip edgeClip = ((X == 0) ? EdgeClip.Left : ((X == _hexagons.GetLength(0) - 1) ? EdgeClip.Right : EdgeClip.None));
		edgeClip = (EdgeClip)((int)edgeClip | ((Y == 0) ? 52 : ((Y == _hexagons.GetLength(1) - 1) ? 200 : 0)));
		if (hex)
		{
			edgeClip = (EdgeClip)((int)edgeClip | ((X + rowOffset == 0) ? 80 : ((X + rowOffset == _hexagons.GetLength(0)) ? 160 : 0)));
		}
		return edgeClip;
	}

	private void FloodFill(ref List<Hex> hexes, Hex hex, int pass, int depth, ref int size, int depthMax, int sizeMax)
	{
		if (hex.Filled)
		{
			return;
		}
		if (depth > 200)
		{
			dunGoofed = true;
		}
		else
		{
			if (depth == depthMax)
			{
				return;
			}
			depth++;
			if (size == sizeMax)
			{
				return;
			}
			size++;
			hex.Filled = true;
			hex.Pass = pass;
			hexes.Remove(hex);
			List<int> list = new List<int>(6);
			list.Add(0);
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Add(4);
			list.Add(5);
			List<int> list2 = list;
			Helper.Shuffle(list2, _random);
			int num = hex.PPosition.Y % 2;
			EdgeClip edges = GetEdges(hex.PPosition.X, hex.PPosition.Y, hex: true, num);
			foreach (int item in list2)
			{
				switch (item)
				{
				case 0:
					if ((edges & EdgeClip.Left) == 0)
					{
						FloodFill(ref hexes, _hexagons[hex.PPosition.X - 1, hex.PPosition.Y], pass, depth, ref size, depthMax, sizeMax);
					}
					break;
				case 1:
					if ((edges & EdgeClip.Right) == 0)
					{
						FloodFill(ref hexes, _hexagons[hex.PPosition.X + 1, hex.PPosition.Y], pass, depth, ref size, depthMax, sizeMax);
					}
					break;
				case 2:
					if ((edges & EdgeClip.UpLeft) == 0)
					{
						FloodFill(ref hexes, _hexagons[hex.PPosition.X - (1 - num), hex.PPosition.Y - 1], pass, depth, ref size, depthMax, sizeMax);
					}
					break;
				case 3:
					if ((edges & EdgeClip.UpRight) == 0)
					{
						FloodFill(ref hexes, _hexagons[hex.PPosition.X + num, hex.PPosition.Y - 1], pass, depth, ref size, depthMax, sizeMax);
					}
					break;
				case 4:
					if ((edges & EdgeClip.DownLeft) == 0)
					{
						FloodFill(ref hexes, _hexagons[hex.PPosition.X - (1 - num), hex.PPosition.Y + 1], pass, depth, ref size, depthMax, sizeMax);
					}
					break;
				case 5:
					if ((edges & EdgeClip.DownRight) == 0)
					{
						FloodFill(ref hexes, _hexagons[hex.PPosition.X + num, hex.PPosition.Y + 1], pass, depth, ref size, depthMax, sizeMax);
					}
					break;
				}
			}
		}
	}

	private void Erode(int erosionFalloff)
	{
		for (ushort num = 0; num != _hexagons.GetLength(1); num++)
		{
			for (ushort num2 = 0; num2 != _hexagons.GetLength(0); num2++)
			{
				if (_hexagons[num2, num].IsActive)
				{
					_decays[num2, num] = SurroundingDecays(num2, num, erosionFalloff);
				}
				else
				{
					_decays[num2, num] = 0;
				}
			}
		}
		for (ushort num3 = 0; num3 != _hexagons.GetLength(1); num3++)
		{
			for (ushort num4 = 0; num4 != _hexagons.GetLength(0); num4++)
			{
				if (_hexagons[num4, num3].IsActive)
				{
					_hexagons[num4, num3].IsActive = _random.Next(15) - _decays[num4, num3] >= 0;
				}
				_hexagons[num4, num3].Decay = _decays[num4, num3];
			}
		}
	}

	private int SurroundingDecays(ushort j, ushort i, int erosionFalloff)
	{
		int num = 0;
		EdgeClip edges = GetEdges(j, i, hex: false, 0);
		if ((edges & EdgeClip.Left) == 0)
		{
			num += Math.Max(0, _hexagons[j - 1, i].Decay - erosionFalloff);
		}
		if ((edges & EdgeClip.Right) == 0)
		{
			num += Math.Max(0, _hexagons[j + 1, i].Decay - erosionFalloff);
		}
		if ((edges & EdgeClip.Up) == 0)
		{
			num += Math.Max(0, _hexagons[j, i - 1].Decay - erosionFalloff);
		}
		if ((edges & EdgeClip.Down) == 0)
		{
			num += Math.Max(0, _hexagons[j, i + 1].Decay - erosionFalloff);
		}
		return num;
	}
}
