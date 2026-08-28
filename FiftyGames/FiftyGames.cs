using System;
using System.Reflection;
using FiftyGames.ForeverWars;
using FiftyGames.FruitsInARow;
using FiftyGames.GiantKillerCentipede;
using FiftyGames.HammerFight;
using FiftyGames.HeliChopper;
using FiftyGames.Impossible;
using FiftyGames.LightBikes;
using FiftyGames.LunarLander;
using FiftyGames.MicroMachinesGame;
using FiftyGames.PlatformsAreFalling2;
using FiftyGames.RiskyRiskyRisk;
using FiftyGames.Rotoball;
using FiftyGames.RRInSpace;
using FiftyGames.ShooterGame;
using FiftyGames.Sumo;
using FiftyGames.SuperHighway;
using FiftyGames.SwingGems;
using FiftyGames.TheSkyIsFalling;
using FiftyGames.TwoTrackTanks;
using FiftyGames.Zombie;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames;

internal class FiftyGames : Game
{
	private enum GameState
	{
		Initialise,
		Logo,
		Menu,
		Game,
		Pause,
		Quit
	}

	private const int StorageIconTime = 10000;

	private MinigameMeta[] _minigameList = new MinigameMeta[20]
	{
		new MinigameMeta(1, typeof(global::FiftyGames.PlatformsAreFalling2.PlatformsAreFalling2), "Acid Escape", "The toxic sludge is rising, you better climb if you don't want to end up radioactive fish food.", "PlatformsAreFalling", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music PlatformsAreFalling", "points", "PlatformsAreFallingInstructions"),
		new MinigameMeta(2, typeof(global::FiftyGames.MicroMachinesGame.MicroMachinesGame), "Drift Pixel", "Speed and drift your tiny, super maneuverable stunt cars around the block.", "MicroMachines", 1, 4, GameGenre.Race, GameCompetition.FreeForAll, "music MicroMachines", "seconds", "MiniMachinesInstructions"),
		new MinigameMeta(3, typeof(global::FiftyGames.GiantKillerCentipede.GiantKillerCentipede), "Mutant Space Worm", "Defeat the giant killer worm using your not-so-primitive weapons.", "GiantKillerCentipede", 1, 4, GameGenre.Action, GameCompetition.Unilateral, "music Centipede", "", "GiantKillerCentipede"),
		new MinigameMeta(4, typeof(global::FiftyGames.HammerFight.HammerFight), "Dungeon Diamonds", "Smash and grab to get as many diamonds as possible. Watch out for the the other team's wrecking balls!", "HammerFight", 2, 4, GameGenre.Action, GameCompetition.Team, "music HammerFight", "points", "HammerFightInstructions"),
		new MinigameMeta(5, typeof(global::FiftyGames.ForeverWars.ForeverWars), "Forever Wars", "Destroy those pesky space pirates before they destroy you!", "ForeverWars", 1, 4, GameGenre.Action, GameCompetition.CoOp, "music GeometryWars", "Kills", "ForeverWarsInstructions"),
		new MinigameMeta(6, typeof(global::FiftyGames.Sumo.Sumo), "Sumo", "Read your opponent to push them out of the ring... Then honorably tell them to eat it.", "Sumo", 2, 2, GameGenre.Strategy, GameCompetition.OneVsOne, "music Sumo", "", "SumoInstructions"),
		new MinigameMeta(7, typeof(global::FiftyGames.LunarLander.LunarLander), "Astro Pods", "Rush to land your pod first and conquer the moon for your country. Just be careful not to crash!", "LunarLander", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music LunarLander", "points", "LunarLander"),
		new MinigameMeta(8, typeof(global::FiftyGames.HeliChopper.HeliChopper), "Risky Rotor Maneuver", "Try not to crash in the expedition to find the end of the infinite cave.", "HeliChopper", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music Helicopter", "metres", "HeliChopperInstructions"),
		new MinigameMeta(9, typeof(global::FiftyGames.LightBikes.LightBikes), "Pixel Rider", "Don't crash your runaway pixel! your life depends on it!", "LightBikes", 1, 4, GameGenre.Strategy, GameCompetition.FreeForAll, "music TheGameOfLifeCycles", "", "LightBikesInstructions"),
		new MinigameMeta(10, typeof(global::FiftyGames.Zombie.Zombie), "Not My Brains!", "Use whatever you can find to defend yourself from the undead hoards lurking in the shadows.", "Zombie", 1, 4, GameGenre.Action, GameCompetition.CoOp, "music TopZombies", "waves", "ZombieInstructions"),
		new MinigameMeta(11, typeof(global::FiftyGames.RiskyRiskyRisk.RiskyRiskyRisk), "Battle Dice", "Use your dice and your wit to battle your way to complete domiation.", "Risk", 1, 4, GameGenre.Strategy, GameCompetition.FreeForAll, "music RiskyRiskyRisk", "points", "RiskyRiskyRiskInstructions"),
		new MinigameMeta(12, typeof(global::FiftyGames.Rotoball.Rotoball), "Rotoball", "Rotoball! Everybody's doin' it. Everybody's playin' it. Ain't you heard?", "RotoBall", 2, 4, GameGenre.Action, GameCompetition.Team, "music RotoBall", "", "RotoBallInstructions"),
		new MinigameMeta(13, typeof(global::FiftyGames.ShooterGame.ShooterGame), "Gun Lab", "Experimental weapons need testing and you're going to be the subject.", "Shooter", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music TopShooter", "kills", "ShooterInstructions"),
		new MinigameMeta(14, typeof(global::FiftyGames.RRInSpace.RRInSpace), "Space Race", "Race your dual polar-indie turbo-engine Koala shuttle-pod around a hyper-bounce sphere-marked track.", "SpaceRace", 2, 4, GameGenre.Race, GameCompetition.FreeForAll, "music RaceInCircles", "", "SpaceRaceInstructions"),
		new MinigameMeta(15, typeof(global::FiftyGames.SuperHighway.SuperHighway), "Super Highway", "An infinite endurance race to be the last car in one piece!", "SuperHighway", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music HyperChase", "metres", "SuperHighway"),
		new MinigameMeta(16, typeof(global::FiftyGames.SwingGems.SwingGems), "Swing Gems", "Swing your way through a rectangular cave without smashing your fragile body.", "SwingGems", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music SwingSideways", "Metres", "SwingGemsInstructions"),
		new MinigameMeta(17, typeof(global::FiftyGames.Impossible.Impossible), "Sunken Ruin", "Escape the crumbling remains without falling into croc-infested waters!", "TheImpossibleGame", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music TheImpossibleGame", "Meters", "TheImpossibleGameInstructions"),
		new MinigameMeta(18, typeof(global::FiftyGames.TheSkyIsFalling.TheSkyIsFalling), "The Sky is Falling", "Avoid crushing rocks of robot destruction that fall from the sky.", "TheSkyIsFalling", 1, 4, GameGenre.Action, GameCompetition.FreeForAll, "music TheSkyIsFalling", "seconds", "TheSkyIsFallingInstructions"),
		new MinigameMeta(19, typeof(global::FiftyGames.FruitsInARow.FruitsInARow), "Fruits in a Row", "Connect four juicy fruit in a row.", "FruitsInARow", 1, 2, GameGenre.Puzzle, GameCompetition.OneVsOne, "music Connect4", "", "FruitsInARow"),
		new MinigameMeta(20, typeof(global::FiftyGames.TwoTrackTanks.TwoTrackTanks), "Two Track Tanks", "Team up in two-man tanks to destroy the other mad scientist on the block and dominate the trainyard!", "TwoTrackTanks", 2, 4, GameGenre.Action, GameCompetition.Team, "music TwoTrackTanks", "points", "TwoTrackTanks")
	};

	private short[] _demoList = new short[6] { 1, 3, 8, 9, 13, 15 };

	private GameState _currentState;

	private GameState _nextState;

	private ContentManager _minigameContentManager;

	private StorageManager _storageManager;

	private PlayerManager _playerManager;

	private SoundManager _soundManager;

	private GraphicsDeviceManager _graphics;

	// FIX (resizable window + fullscreen): the game keeps rendering everything
	// at a fixed, internal 1280x720 resolution into this render target (every
	// minigame's own layout math, HUD positions, and shader UV math already
	// assume exactly 1280x720 -- see project notes), then Draw() scales/
	// letterboxes that fixed image into whatever the real, user-resizable
	// window or fullscreen backbuffer size actually is. This never needs to
	// be recreated on resize since its size never changes.
	private static RenderTarget2D _masterRenderTarget;

	// Remembers the last windowed size so returning from fullscreen restores
	// it instead of snapping back to the 1280x720 default.
	private int _windowedBackBufferWidth = 1280;

	private int _windowedBackBufferHeight = 720;

	private KeyboardState _previousKeyboardState;

	private GamerServicesComponent _gamerServices;

	private SpriteBatch _spriteBatch;

	private Rectangle _titleSafeRect;

	private AnimationSequence[] _startAnimations;

	private int _currentStartAnimation;

	private Minigame _minigame;

	private short _minigameID;

	private short _selectedMinigame;

	private Menu _mainMenu;

	private MenuComponent _storageIndicator;

	private Texture2D _loadTex;

	private Texture2D _failTex;

	private int _storageFailTimer;

	private Random _ranGen;

	private bool _pauseVibration;

	public Rectangle TitleSafeArea
	{
		get
		{
			return _titleSafeRect;
		}
		set
		{
			_titleSafeRect = value;
			MenuComponent storageIndicator = _storageIndicator;
			Vector2 position = (_storageIndicator.DesiredPosition = new Vector2((float)_titleSafeRect.Right - _storageIndicator.Size.X * 0.5f, (float)_titleSafeRect.Top + _storageIndicator.Size.Y * 0.5f));
			storageIndicator.Position = position;
		}
	}

	public bool VSync
	{
		get
		{
			return _graphics.SynchronizeWithVerticalRetrace;
		}
		set
		{
			_graphics.SynchronizeWithVerticalRetrace = value;
			_graphics.ApplyChanges();
		}
	}

	// FIX (resizable window + fullscreen): exposed so each minigame that does
	// its own internal render-target compositing (see the per-game "SetRenderTarget(null)"
	// -> master RT redirect" fix in those files) can redirect its own final
	// composite into the same fixed 1280x720 surface this class letterboxes
	// to the screen, instead of drawing straight to the real (variable-size)
	// backbuffer and bypassing the scaling entirely.
	public static RenderTarget2D MasterRenderTarget => _masterRenderTarget;

	public FiftyGames()
	{
		_graphics = new GraphicsDeviceManager(this);
		_graphics.PreferredBackBufferHeight = 720;
		_graphics.PreferredBackBufferWidth = 1280;
		_graphics.IsFullScreen = false;
		// FIX (resizable window + fullscreen): HardwareModeSwitch = false makes
		// fullscreen a fast borderless-style switch (resize the backbuffer to
		// the desktop resolution, no display-mode change) instead of exclusive
		// fullscreen, which is smoother and avoids monitor flicker on toggle.
		_graphics.HardwareModeSwitch = false;
		Window.AllowUserResizing = true;
		Window.ClientSizeChanged += Window_ClientSizeChanged;
		_gamerServices = new GamerServicesComponent(this);
		_minigameContentManager = new ColorFixContentManager(base.Services, "Content");
		_soundManager = new SoundManager(this);
		_playerManager = new PlayerManager(this);
		_storageManager = new StorageManager(this, ref _playerManager, ref _soundManager, ref _minigameList);
	}

	protected override void Initialize()
	{
		GameConsole.Initialize();
		base.Components.Add(_playerManager);
		base.Components.Add(_gamerServices);
		base.Components.Add(_storageManager);
		base.Components.Add(_soundManager);
		base.Initialize();
		base.IsMouseVisible = true;
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		// FIX (resizable window + fullscreen): fixed-size internal render target,
		// see the field declaration comment above for why 1280x720 never changes.
		_masterRenderTarget = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		base.Content = new ColorFixContentManager(Services, "Content");
		_titleSafeRect = new Rectangle(128, 72, 1024, 576);
		_ranGen = new Random();
		_selectedMinigame = 0;
		_storageIndicator = new MenuComponent();
		_loadTex = base.Content.Load<Texture2D>("Menu/Sprites/General/LoadIndicator");
		_failTex = base.Content.Load<Texture2D>("Menu/Sprites/General/FailIndicator");
		_storageIndicator.Sprite = _loadTex;
		_storageIndicator.FitComponentToImage();
		_storageIndicator.SpriteOrigin = new Vector2((float)_storageIndicator.Sprite.Width * 0.5f, (float)_storageIndicator.Sprite.Height * 0.5f);
		MenuComponent storageIndicator = _storageIndicator;
		Vector2 position = (_storageIndicator.DesiredPosition = new Vector2((float)_titleSafeRect.Right - _storageIndicator.Size.X * 0.5f, (float)_titleSafeRect.Top + _storageIndicator.Size.Y * 0.5f));
		storageIndicator.Position = position;
		_storageIndicator.PositionAnchor = MenuComponent.Anchor.TopLeft;
		_storageIndicator.DesiredRotation = (float)Math.PI * 2f;
		_storageIndicator.Depth = 1f;
		GameConsole.TextColour = Color.White;
		GameConsole.Font = base.Content.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		GameConsole.BackgroundTexture = base.Content.Load<Texture2D>("Menu/Sprites/General/Console");
		GameConsole.DrawArea = _titleSafeRect;
		GameConsole.CommandInvoked += GameConsoleCommand;
		_currentState = GameState.Initialise;
		_nextState = GameState.Logo;
	}

	private void GameConsoleCommand(GameConsoleCommand command)
	{
		if (!command.IsSet)
		{
			return;
		}
		switch (command.Command)
		{
		case "restart":
			if (command.Arguments.Count != 0 && command.Arguments[0].ToLower() == "game" && _currentState == GameState.Game)
			{
				EndMinigame();
				StartMinigame();
				_soundManager.ChangeToGameMusic(_minigameList[_selectedMinigame].SongName);
			}
			else
			{
				_mainMenu = null;
				_nextState = GameState.Logo;
			}
			break;
		case "launch":
		{
			if (command.Arguments.Count != 0 && short.TryParse(command.Arguments[0], out var result2))
			{
				EndMinigame();
				_minigameID = result2;
				_nextState = GameState.Game;
				if (command.Arguments.Count >= 3 && int.TryParse(command.Arguments[1], out var result3) && int.TryParse(command.Arguments[2], out var result4))
				{
					_playerManager.JoinDebug(result3, _soundManager);
					for (int i = 0; i < result3; i++)
					{
						_playerManager.PlayersConnected[i].GamePadManager = _playerManager.GetGamePad((PlayerIndex)result4);
					}
				}
				else if (command.Arguments.Count == 2 && int.TryParse(command.Arguments[1], out result3))
				{
					_playerManager.JoinDebug(result3, _soundManager);
				}
				else
				{
					_playerManager.JoinDebug(1, _soundManager);
				}
			}
			else
			{
				GameConsole.PrintString("No game selected.");
				GameConsole.PrintString("Syntax: launch <game> [<players>] [<gamePad>]");
			}
			break;
		}
		case "quit":
			if (command.Arguments.Count != 0 && command.Arguments[0].ToLower() == "game" && (_currentState == GameState.Game || _currentState == GameState.Pause))
			{
				_currentState = GameState.Pause;
				_nextState = GameState.Menu;
			}
			else if (_currentState == GameState.Game || _currentState == GameState.Pause)
			{
				Exit();
			}
			break;
		case "music":
		{
			if (command.Arguments.Count >= 2 && command.Arguments[0].ToLower() == "vol" && float.TryParse(command.Arguments[1], out var result5))
			{
				_soundManager.MusicVolume = result5;
			}
			else
			{
				GameConsole.PrintString("Syntax: music vol <level>");
			}
			break;
		}
		case "sound":
		{
			if (command.Arguments.Count >= 2 && command.Arguments[0].ToLower() == "vol" && float.TryParse(command.Arguments[1], out var result))
			{
				_soundManager.EffectVolume = result;
			}
			else
			{
				GameConsole.PrintString("Syntax: sound vol <level>");
			}
			break;
		}
		}
	}

	protected override void Update(GameTime gameTime)
	{
		// FIX (resizable window + fullscreen): F11 or Alt+Enter toggles fullscreen,
		// matching the common convention used by most PC games. Edge-triggered
		// against _previousKeyboardState so holding the key doesn't toggle every frame.
		KeyboardState keyboardState = Keyboard.GetState();
		bool altEnterPressed = (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt)) && keyboardState.IsKeyDown(Keys.Enter) && !((_previousKeyboardState.IsKeyDown(Keys.LeftAlt) || _previousKeyboardState.IsKeyDown(Keys.RightAlt)) && _previousKeyboardState.IsKeyDown(Keys.Enter));
		bool f11Pressed = keyboardState.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11);
		if (altEnterPressed || f11Pressed)
		{
			ToggleFullScreen();
		}
		_previousKeyboardState = keyboardState;
		GameConsole.Update(gameTime);
		_playerManager.Enabled = base.IsActive && !Guide.IsVisible;
		if (_currentState != _nextState)
		{
			ChangeState(gameTime);
		}
		else
		{
			switch (_currentState)
			{
			case GameState.Logo:
				if (_currentStartAnimation != _startAnimations.Length)
				{
					_startAnimations[_currentStartAnimation].Update(gameTime);
					GamePadButtons gamePadButtons = default(GamePadButtons);
					if (_startAnimations[_currentStartAnimation].AnimationFinished || (_playerManager.GetGamePad(PlayerIndex.One).GamePadStateCurrent.Buttons != gamePadButtons && _playerManager.GetGamePad(PlayerIndex.One).GamePadStatePrevious.Buttons == gamePadButtons) || (_playerManager.GetGamePad(PlayerIndex.Two).GamePadStateCurrent.Buttons != gamePadButtons && _playerManager.GetGamePad(PlayerIndex.Two).GamePadStatePrevious.Buttons == gamePadButtons) || (_playerManager.GetGamePad(PlayerIndex.Three).GamePadStateCurrent.Buttons != gamePadButtons && _playerManager.GetGamePad(PlayerIndex.Three).GamePadStatePrevious.Buttons == gamePadButtons) || (_playerManager.GetGamePad(PlayerIndex.Four).GamePadStateCurrent.Buttons != gamePadButtons && _playerManager.GetGamePad(PlayerIndex.Four).GamePadStatePrevious.Buttons == gamePadButtons))
					{
						_soundManager.ClearGameSounds();
						_currentStartAnimation++;
					}
				}
				else
				{
					_nextState = GameState.Menu;
				}
				break;
			case GameState.Menu:
				_minigameID = _mainMenu.Update(gameTime);
				if (_minigameID > 0)
				{
					_nextState = GameState.Game;
				}
				else if (_minigameID == -1)
				{
					if (_minigame != null)
					{
						EndMinigame();
					}
					Exit();
				}
				break;
			case GameState.Game:
				if (_mainMenu.State == Menu.MenuState.Loading)
				{
					_mainMenu.Update(gameTime);
				}
				foreach (Player item in _playerManager.PlayersConnected)
				{
					if (item.GamePadManager.ButtonWasPressed(Buttons.Start) || !item.GamePadManager.GamePadStateCurrent.IsConnected || item.GamerProblem)
					{
						_mainMenu.Pause(item.PlayerIndex, gameTime);
						_pauseVibration = _playerManager.GetPlayer(_mainMenu.PlayerInControl).AllowsVibration;
						_nextState = GameState.Pause;
					}
				}
				if (!base.IsActive || Guide.IsVisible)
				{
					_mainMenu.Pause(gameTime);
					_pauseVibration = _playerManager.GetPlayer(_mainMenu.PlayerInControl).AllowsVibration;
					_nextState = GameState.Pause;
				}
				break;
			case GameState.Pause:
				switch (_mainMenu.Update(gameTime))
				{
				case 1:
					_nextState = GameState.Game;
					break;
				case 2:
					_storageManager.Save(_minigameList[_selectedMinigame]);
					_mainMenu.QuitMinigame(_minigameID, gameTime);
					_nextState = GameState.Menu;
					break;
				}
				break;
			}
		}
		if (_storageManager != null && _storageManager.DeviceState == StorageManager.StorageDeviceState.Working)
		{
			_storageFailTimer = 0;
			if (_storageIndicator.Sprite != _loadTex)
			{
				_storageIndicator.Sprite = _loadTex;
			}
			MenuComponent storageIndicator = _storageIndicator;
			Vector2 position = (_storageIndicator.DesiredPosition = new Vector2((float)_titleSafeRect.Right - _storageIndicator.Size.X * 0.5f, (float)_titleSafeRect.Top + _storageIndicator.Size.Y * 0.5f));
			storageIndicator.Position = position;
			_storageIndicator.DesiredRotation = (float)Math.PI * 2f;
			if (_storageIndicator.Rotation == _storageIndicator.DesiredRotation)
			{
				_storageIndicator.Rotation = 0f;
			}
			_storageIndicator.Update(gameTime);
		}
		else if (_storageManager != null && (_storageManager.DeviceState == StorageManager.StorageDeviceState.Disconnected || _storageManager.DeviceState == StorageManager.StorageDeviceState.Full) && _storageFailTimer < 10000)
		{
			_storageFailTimer += gameTime.ElapsedGameTime.Milliseconds;
			if (_storageIndicator.Sprite != _failTex)
			{
				_storageIndicator.Sprite = _failTex;
			}
			MenuComponent storageIndicator2 = _storageIndicator;
			Vector2 position2 = (_storageIndicator.DesiredPosition = new Vector2((float)_titleSafeRect.Right - _storageIndicator.Size.X * 0.5f, (float)_titleSafeRect.Top + _storageIndicator.Size.Y * 0.5f));
			storageIndicator2.Position = position2;
			MenuComponent storageIndicator3 = _storageIndicator;
			float rotation = (_storageIndicator.DesiredRotation = 0f);
			storageIndicator3.Rotation = rotation;
			_storageIndicator.Update(gameTime);
		}
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		// FIX (resizable window + fullscreen): draw the whole game at its fixed
		// 1280x720 internal resolution into _masterRenderTarget; see the
		// letterbox-present block at the end of this method.
		base.GraphicsDevice.SetRenderTarget(_masterRenderTarget);
		switch (_currentState)
		{
		case GameState.Logo:
			base.GraphicsDevice.Clear(Color.Black);
			if (_currentStartAnimation != _startAnimations.Length)
			{
				_startAnimations[_currentStartAnimation].Draw(_spriteBatch);
			}
			break;
		case GameState.Menu:
			if (_minigame != null)
			{
				_minigame.Draw(gameTime);
			}
			else
			{
				base.GraphicsDevice.Clear(Color.Black);
			}
			if (_mainMenu != null)
			{
				_mainMenu.Draw(_spriteBatch);
			}
			break;
		case GameState.Game:
			if (_minigame != null)
			{
				_minigame.Draw(gameTime);
				if (_mainMenu != null && _mainMenu.State == Menu.MenuState.Loading)
				{
					_mainMenu.State = Menu.MenuState.Pause;
				}
			}
			else
			{
				base.GraphicsDevice.Clear(Color.Black);
			}
			break;
		case GameState.Pause:
			if (_minigame != null)
			{
				_minigame.Draw(gameTime);
			}
			else
			{
				base.GraphicsDevice.Clear(Color.Black);
			}
			if (_mainMenu != null)
			{
				_mainMenu.Draw(_spriteBatch);
			}
			break;
		}
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		if (_storageManager != null && (_storageManager.DeviceState == StorageManager.StorageDeviceState.Working || ((_storageManager.DeviceState == StorageManager.StorageDeviceState.Disconnected || _storageManager.DeviceState == StorageManager.StorageDeviceState.Full) && _storageFailTimer < 10000)))
		{
			_storageIndicator.Draw(_spriteBatch);
		}
		GameConsole.Draw(_spriteBatch);
		_spriteBatch.End();
		// FIX (resizable window + fullscreen): everything above this point drew
		// into the fixed 1280x720 _masterRenderTarget (bound at the very top of
		// this method). Now switch to the REAL backbuffer (whatever size the
		// resized window or fullscreen display actually is) and present that
		// fixed image scaled into it, centered, preserving the 1280x720 aspect
		// ratio with black bars rather than stretching it.
		base.GraphicsDevice.SetRenderTarget(null);
		base.GraphicsDevice.Clear(Color.Black);
		Rectangle letterboxRect = ComputeLetterboxRect();
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp);
		_spriteBatch.Draw(_masterRenderTarget, letterboxRect, Color.White);
		_spriteBatch.End();
		base.Draw(gameTime);
	}

	// FIX (resizable window + fullscreen): largest 1280x720-aspect rectangle
	// that fits centered inside the real backbuffer's current dimensions.
	private Rectangle ComputeLetterboxRect()
	{
		int backBufferWidth = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
		int backBufferHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
		if (backBufferWidth <= 0 || backBufferHeight <= 0)
		{
			return new Rectangle(0, 0, 1280, 720);
		}
		float scale = Math.Min((float)backBufferWidth / 1280f, (float)backBufferHeight / 720f);
		int width = (int)(1280f * scale);
		int height = (int)(720f * scale);
		int x = (backBufferWidth - width) / 2;
		int y = (backBufferHeight - height) / 2;
		return new Rectangle(x, y, width, height);
	}

	// FIX (resizable window + fullscreen): DesktopGL tracks the OS window's
	// client size into the backbuffer automatically while the user drags an
	// edge (see MonoGame issue #5590), but doesn't call ApplyChanges for us --
	// this keeps PreferredBackBufferWidth/Height in sync with it. Guarded so
	// it's a no-op once they already match (ApplyChanges itself re-fires this
	// event, so without that guard this would recurse), skips the transient
	// 0x0 size fired while the window is minimized, and steps aside entirely
	// while fullscreen (ToggleFullScreen owns sizing in that case).
	private void Window_ClientSizeChanged(object sender, EventArgs e)
	{
		if (_graphics.IsFullScreen)
		{
			return;
		}
		int width = Window.ClientBounds.Width;
		int height = Window.ClientBounds.Height;
		if (width <= 0 || height <= 0)
		{
			return;
		}
		if (_graphics.PreferredBackBufferWidth == width && _graphics.PreferredBackBufferHeight == height)
		{
			return;
		}
		_graphics.PreferredBackBufferWidth = width;
		_graphics.PreferredBackBufferHeight = height;
		_graphics.ApplyChanges();
	}

	// FIX (resizable window + fullscreen): toggles between the last-used
	// windowed size and a borderless-style fullscreen at the desktop's
	// current resolution (see HardwareModeSwitch=false in the constructor).
	private void ToggleFullScreen()
	{
		if (_graphics.IsFullScreen)
		{
			_graphics.IsFullScreen = false;
			_graphics.PreferredBackBufferWidth = _windowedBackBufferWidth;
			_graphics.PreferredBackBufferHeight = _windowedBackBufferHeight;
		}
		else
		{
			_windowedBackBufferWidth = _graphics.PreferredBackBufferWidth;
			_windowedBackBufferHeight = _graphics.PreferredBackBufferHeight;
			DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
			_graphics.PreferredBackBufferWidth = displayMode.Width;
			_graphics.PreferredBackBufferHeight = displayMode.Height;
			_graphics.IsFullScreen = true;
		}
		_graphics.ApplyChanges();
	}

	protected override void OnExiting(object sender, ExitingEventArgs args)
	{
		if (_minigame != null)
		{
			EndMinigame();
		}
		base.OnExiting(sender, args);
	}

	private void ChangeState(GameTime gameTime)
	{
		switch (_currentState)
		{
		case GameState.Logo:
			if (_startAnimations != null)
			{
				for (int i = 0; i < _startAnimations.Length; i++)
				{
					_startAnimations[i] = null;
				}
				_startAnimations = null;
			}
			break;
		case GameState.Menu:
			EndMinigame();
			break;
		case GameState.Game:
			_minigame.Enabled = false;
			break;
		case GameState.Pause:
			if (_playerManager.GetPlayer(_mainMenu.PlayerInControl).AllowsVibration != _pauseVibration)
			{
				_storageManager.Save(_playerManager.GetPlayer(_mainMenu.PlayerInControl), saveCurrentSettings: false);
			}
			if (_nextState == GameState.Menu)
			{
				EndMinigame();
			}
			break;
		}
		switch (_nextState)
		{
		case GameState.Logo:
			_currentStartAnimation = 0;
			_startAnimations = new AnimationSequence[2];
			_startAnimations[0] = new IndieSkiesLogo();
			_startAnimations[0].Initialise();
			_startAnimations[0].Load(base.Content, _soundManager);
			_startAnimations[1] = new AutoSave();
			_startAnimations[1].Initialise();
			_startAnimations[1].Load(base.Content, _soundManager);
			_currentState = _nextState;
			break;
		case GameState.Menu:
			if (_mainMenu == null)
			{
				_mainMenu = new Menu(ref _playerManager, ref _storageManager, ref _soundManager, _minigameList, ref _titleSafeRect, this);
				_mainMenu.Load(base.Content);
			}
			if (_minigame == null)
			{
				_minigameID = _demoList[_ranGen.Next(_demoList.Length)];
				GameConsole.PrintString("FiftyGames: Demo game selected: " + _minigameID);
				StartMinigame();
				_soundManager.ChangeToMenuMusic();
			}
			_currentState = _nextState;
			break;
		case GameState.Game:
			if (_currentState == GameState.Menu)
			{
				if (_minigame == null && _mainMenu.State == Menu.MenuState.Loading)
				{
					GameConsole.PrintString("FiftyGames: Game selected: " + _minigameID);
					StartMinigame();
					_soundManager.ChangeToGameMusic(_minigameList[_selectedMinigame].SongName);
					_currentState = _nextState;
				}
				else
				{
					_mainMenu.Update(gameTime);
				}
			}
			else if (_currentState == GameState.Pause)
			{
				_soundManager.ResumeGameSounds();
				_minigame.Enabled = true;
				_currentState = _nextState;
			}
			break;
		case GameState.Pause:
			if (_mainMenu == null)
			{
				_mainMenu = new Menu(ref _playerManager, ref _storageManager, ref _soundManager, _minigameList, ref _titleSafeRect, this);
				_mainMenu.Load(base.Content);
			}
			_soundManager.PauseGameSounds();
			_currentState = _nextState;
			break;
		default:
			_currentState = _nextState;
			break;
		}
	}

	private void StartMinigame()
	{
		bool flag = false;
		for (short num = 0; num < (short)_minigameList.Length; num++)
		{
			if (_minigameList[num].MinigameID == _minigameID)
			{
				_selectedMinigame = num;
				flag = true;
				break;
			}
		}
		if (flag)
		{
			GameConsole.PrintString("FiftyGames: Launching game " + _minigameID + " (" + _minigameList[_selectedMinigame].Name + ") with " + _playerManager.NumberOfPlayers.ToString() + " players");
			ConstructorInfo constructorInfo = _minigameList[_selectedMinigame].Type.GetConstructors()[0];
			object obj = constructorInfo.Invoke(new object[6]
			{
				this,
				_playerManager,
				_soundManager,
				_minigameContentManager,
				_minigameList[_selectedMinigame],
				_nextState == GameState.Menu
			});
			_minigame = (Minigame)obj;
			_minigame.Visible = false;
			base.Components.Add(_minigame);
		}
		else
		{
			GameConsole.PrintString("FiftyGames: Failed to launch game " + _minigameID + ". Game does not exist");
			_nextState = GameState.Menu;
		}
	}

	private void EndMinigame()
	{
		if (_minigame != null)
		{
			_minigame.Quit();
			_minigame.Dispose();
			base.Components.Remove(_minigame);
			_minigame = null;
			_soundManager.ClearGameSounds();
			_minigameContentManager.Unload();
			GC.Collect();
		}
	}
}
