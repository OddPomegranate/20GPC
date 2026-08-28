using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames;

internal class Menu
{
	public enum MenuState
	{
		Start,
		Main,
		Connect,
		Games,
		Settings,
		Screen,
		Instruction,
		Loading,
		Credits,
		Quit,
		Confirm,
		Pause,
		Disconnect,
		SignOut
	}

	private enum ConfirmAction
	{
		QuitGame,
		ClearScores,
		ClearRatings,
		ContinueWithoutSaving,
		ValidBuy,
		InvalidBuy
	}

	private const int DefaultRepeatTime = 200;

	private const int MinRepeatTime = 60;

	private const int HoldLimit = 600;

	private MenuState _menuState;

	private MenuState _nextState;

	private PlayerIndex _leader;

	private PlayerIndex _playerInControl;

	private Player _leaderPlayer;

	private int _selectedIndex;

	private int[] _holdTimers;

	private int[] _holdRepeatTime;

	private FiftyGames _game;

	private Rectangle _contentArea;

	private PlayerManager _playerManager;

	private StorageManager _storageManager;

	private SoundManager _soundManager;

	private MinigameMeta[] _minigameMeta;

	private ContentManager _contentLoader;

	private MenuComponent _overlay;

	private MenuComponent _logo;

	private MenuComponent _logoShadow;

	private TextComponent _start;

	private ListButton[] _menuMain;

	private TextComponent[] _promptsMain;

	private ListButton[] _menuSettings;

	private TextComponent[] _promptsSettings;

	private MenuComponent[] _screenCorners;

	private MenuComponent _screenCentre;

	private TextComponent _screenAreaText;

	private TextComponent _screenBrightnessText;

	private TextComponent[] _promptsScreen;

	private ConnectPanel[] _menuConnect;

	private float panelSpacing;

	private MinigameMeta.SortMode _sortMode;

	private MenuComponent _gameImage;

	private MenuComponent _gameLockImage;

	private MenuComponent _gameCompetitionImage;

	private TextComponent _gameCompetition;

	private TextComponent _gamePlayerLimit;

	private TextComponent _gameHighscore;

	private TextComponent _gameGenre;

	private TextComponent _gameDescription;

	private ListButton[] _menuGames;

	private StarRating[] _starGames;

	private float _scrollOffset;

	private TextComponent[] _promptsGames;

	private TextComponent _sortModeText;

	private MenuComponent _instructionImage;

	private TextComponent[] _promptsInstruction;

	private TextComponent _loading;

	private MenuComponent _creditsLogo;

	private TextComponent[] _creditsHeaders;

	private TextComponent[] _creditsNames;

	private TextComponent _promptCredits;

	private ConfirmAction _confirmAction;

	private ListButton[] _menuConfirm;

	private TextComponent _confirmText;

	private TextComponent[] _promptsConfirm;

	private MenuComponent _pauseBackground;

	private TextComponent _pauseHeader;

	private TextComponent _pauseName;

	private ListButton[] _menuPause;

	private Texture2D _AButtonSprite;

	private Texture2D _StartButtonSprite;

	private Texture2D _BackButtonSprite;

	private TextComponent[] _promptsPause;

	private bool[] _demoLock;

	private int _selectedGame;

	private bool _buying;

	public PlayerIndex Leader => _leader;

	public PlayerIndex PlayerInControl => _playerInControl;

	public MenuState State
	{
		get
		{
			return _menuState;
		}
		set
		{
			_nextState = value;
		}
	}

	public Menu(ref PlayerManager playerManager, ref StorageManager storageManager, ref SoundManager soundManager, MinigameMeta[] minigameMeta, ref Rectangle contentArea, Game game)
	{
		_game = (FiftyGames)game;
		_playerManager = playerManager;
		_storageManager = storageManager;
		_soundManager = soundManager;
		_contentArea = contentArea;
		_minigameMeta = new MinigameMeta[minigameMeta.Length];
		for (int i = 0; i < minigameMeta.Length; i++)
		{
			_minigameMeta[i] = new MinigameMeta(minigameMeta[i]);
		}
		_holdTimers = new int[4];
		_holdRepeatTime = new int[4];
		for (int j = 0; j != _holdTimers.Length; j++)
		{
			_holdTimers[j] = 0;
			_holdRepeatTime[j] = 400;
		}
		_scrollOffset = 0f;
		_menuState = (_nextState = MenuState.Start);
		_demoLock = new bool[20]
		{
			false, false, true, true, false, true, false, true, true, false,
			true, true, true, true, true, true, true, true, true, true
		};
	}

	public void Load(ContentManager contentLoader)
	{
		_contentLoader = contentLoader;
		_overlay = new MenuComponent();
		_overlay.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_overlay.FitComponentToImage();
		MenuComponent overlay = _overlay;
		Vector2 position = (_overlay.DesiredPosition = new Vector2(640f, 360f));
		overlay.Position = position;
		_overlay.PositionAnchor = MenuComponent.Anchor.Centre;
		MenuComponent overlay2 = _overlay;
		Vector2 size = (_overlay.DesiredSize = new Vector2(1280f, 720f));
		overlay2.Size = size;
		MenuComponent overlay3 = _overlay;
		Color colour = (_overlay.DesiredColour = Color.Black * 0f);
		overlay3.Colour = colour;
		_overlay.Depth = 0f;
		_logo = new MenuComponent();
		_logo.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Logo");
		_logo.FitComponentToImage();
		MenuComponent logo = _logo;
		Vector2 position2 = (_logo.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f));
		logo.Position = position2;
		_logo.Depth = 0.002f;
		_logoShadow = new MenuComponent();
		_logoShadow.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/LogoShadow");
		_logoShadow.FitComponentToImage();
		MenuComponent logoShadow = _logoShadow;
		Vector2 position3 = (_logoShadow.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f));
		logoShadow.Position = position3;
		_logoShadow.Depth = 0.001f;
		_start = new TextComponent();
		_start.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_start.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_start.FitComponentToImage();
		_start.Position = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y);
		_start.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y);
		_start.IsOutlined = true;
		TextComponent start = _start;
		Color colour2 = (_start.DesiredColour = Color.Black * 0f);
		start.Colour = colour2;
		_start.Text = "Press Start";
		_start.FitComponentToText(5f);
		_start.Depth = 0.01f;
		_menuMain = new ListButton[5];
		for (int i = 0; i < _menuMain.Length; i++)
		{
			_menuMain[i] = new ListButton();
			_menuMain[i].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
			_menuMain[i].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_menuMain[i].FitComponentToImage();
			ListButton obj = _menuMain[i];
			Vector2 position4 = (_menuMain[i].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)i));
			obj.Position = position4;
			ListButton obj2 = _menuMain[i];
			Vector2 size2 = (_menuMain[i].DesiredSize = new Vector2(200f, 34f));
			obj2.Size = size2;
			ListButton obj3 = _menuMain[i];
			Color colour3 = (_menuMain[i].DesiredColour = new Color(102, 102, 255) * 0.8f);
			obj3.Colour = colour3;
			_menuMain[i].Depth = 0.01f;
		}
		_menuMain[0].Text = "Play";
		_menuMain[1].Text = "Settings";
		_menuMain[2].Text = "Buy";
		_menuMain[3].Text = "Credits";
		_menuMain[4].Text = "Dashboard";
		_promptsMain = new TextComponent[2];
		_promptsMain[0] = new TextComponent();
		_promptsMain[0].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_promptsMain[0].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsMain[0].FitComponentToImage();
		TextComponent obj4 = _promptsMain[0];
		Vector2 position5 = (_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f));
		obj4.Position = position5;
		_promptsMain[0].TextAnchor = MenuComponent.Anchor.MiddleRight;
		TextComponent obj5 = _promptsMain[0];
		Vector2 size3 = (_promptsMain[0].DesiredSize = new Vector2(30f, 30f));
		obj5.Size = size3;
		_promptsMain[0].Text = "Select     ";
		_promptsMain[0].Depth = 0.2f;
		_promptsMain[1] = new TextComponent();
		_promptsMain[1].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptsMain[1].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsMain[1].FitComponentToImage();
		TextComponent obj6 = _promptsMain[1];
		Vector2 position6 = (_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[1].Size.Y * 0.5f));
		obj6.Position = position6;
		_promptsMain[1].TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent obj7 = _promptsMain[1];
		Vector2 size4 = (_promptsMain[1].DesiredSize = new Vector2(30f, 30f));
		obj7.Size = size4;
		_promptsMain[1].Text = "     Back";
		_promptsMain[1].Depth = 0.2f;
		_menuSettings = new ListButton[5];
		for (int j = 0; j < _menuSettings.Length; j++)
		{
			_menuSettings[j] = new ListButton();
			_menuSettings[j].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
			_menuSettings[j].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_menuSettings[j].FitComponentToImage();
			ListButton obj8 = _menuSettings[j];
			Vector2 position7 = (_menuSettings[j].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)j));
			obj8.Position = position7;
			_menuSettings[j].TextAnchor = MenuComponent.Anchor.MiddleLeft;
			ListButton obj9 = _menuSettings[j];
			Vector2 size5 = (_menuSettings[j].DesiredSize = new Vector2(400f, 34f));
			obj9.Size = size5;
			ListButton obj10 = _menuSettings[j];
			Color colour4 = (_menuSettings[j].DesiredColour = new Color(102, 102, 255) * 0.8f);
			obj10.Colour = colour4;
			_menuSettings[j].Depth = 0.01f;
		}
		_menuSettings[0].Text = "Music volume: ";
		_menuSettings[1].Text = "Sound volume: ";
		_menuSettings[2].Text = "Screen setup...";
		_menuSettings[3].Text = "Clear ratings";
		_menuSettings[4].Text = "Clear highscores";
		_promptsSettings = new TextComponent[2];
		_promptsSettings[0] = new TextComponent();
		_promptsSettings[0].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_promptsSettings[0].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsSettings[0].FitComponentToImage();
		TextComponent obj11 = _promptsSettings[0];
		Vector2 position8 = (_promptsSettings[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsSettings[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsSettings[0].Size.Y * 0.5f));
		obj11.Position = position8;
		_promptsSettings[0].TextAnchor = MenuComponent.Anchor.MiddleRight;
		TextComponent obj12 = _promptsSettings[0];
		Vector2 size6 = (_promptsSettings[0].DesiredSize = new Vector2(30f, 30f));
		obj12.Size = size6;
		_promptsSettings[0].Text = "Select     ";
		_promptsSettings[0].Depth = 0.2f;
		_promptsSettings[1] = new TextComponent();
		_promptsSettings[1].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptsSettings[1].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsSettings[1].FitComponentToImage();
		TextComponent obj13 = _promptsSettings[1];
		Vector2 position9 = (_promptsSettings[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsSettings[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsSettings[1].Size.Y * 0.5f));
		obj13.Position = position9;
		_promptsSettings[1].TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent obj14 = _promptsSettings[1];
		Vector2 size7 = (_promptsSettings[1].DesiredSize = new Vector2(30f, 30f));
		obj14.Size = size7;
		_promptsSettings[1].Text = "     Back";
		_promptsSettings[1].Depth = 0.2f;
		_screenCentre = new MenuComponent();
		_screenCentre.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Screen/ScreenCentre");
		_screenCentre.FitComponentToImage();
		MenuComponent screenCentre = _screenCentre;
		Vector2 position10 = (_screenCentre.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y));
		screenCentre.Position = position10;
		_screenCentre.Depth = 0.01f;
		_screenCorners = new MenuComponent[4];
		for (int k = 0; k < _screenCorners.Length; k++)
		{
			_screenCorners[k] = new MenuComponent();
			_screenCorners[k].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Screen/ScreenCorner");
			_screenCorners[k].FitComponentToImage();
			_screenCorners[k].PositionAnchor = MenuComponent.Anchor.TopLeft;
			MenuComponent obj15 = _screenCorners[k];
			float rotation = (_screenCorners[k].DesiredRotation = (float)Math.PI / 2f * (float)k);
			obj15.Rotation = rotation;
			_screenCorners[k].Depth = 0.02f;
		}
		MenuComponent obj16 = _screenCorners[0];
		Vector2 position11 = (_screenCorners[0].DesiredPosition = new Vector2((float)_contentArea.Left - 10f + 1280f, (float)_contentArea.Top - 10f));
		obj16.Position = position11;
		MenuComponent obj17 = _screenCorners[1];
		Vector2 position12 = (_screenCorners[1].DesiredPosition = new Vector2((float)_contentArea.Right + 10f + 1280f, (float)_contentArea.Top - 10f));
		obj17.Position = position12;
		MenuComponent obj18 = _screenCorners[2];
		Vector2 position13 = (_screenCorners[2].DesiredPosition = new Vector2((float)_contentArea.Right + 10f + 1280f, (float)_contentArea.Bottom + 10f));
		obj18.Position = position13;
		MenuComponent obj19 = _screenCorners[3];
		Vector2 position14 = (_screenCorners[3].DesiredPosition = new Vector2((float)_contentArea.Left - 10f + 1280f, (float)_contentArea.Bottom + 10f));
		obj19.Position = position14;
		_screenAreaText = new TextComponent();
		_screenAreaText.Font = _contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		TextComponent screenAreaText = _screenAreaText;
		Vector2 position15 = (_screenAreaText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Top + 10f));
		screenAreaText.Position = position15;
		_screenAreaText.PositionAnchor = MenuComponent.Anchor.TopCentre;
		_screenAreaText.Text = "Position the corners of the viewport as close to the corners\nof your screen as possible while keeping them clearly visible.\nUse the thumbsticks to move and resize the viewport.";
		_screenAreaText.FitComponentToText(5f);
		_screenAreaText.Depth = 0.015f;
		_screenBrightnessText = new TextComponent();
		_screenBrightnessText.Font = _contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		TextComponent screenBrightnessText = _screenBrightnessText;
		Vector2 position16 = (_screenBrightnessText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Bottom - 10f));
		screenBrightnessText.Position = position16;
		_screenBrightnessText.PositionAnchor = MenuComponent.Anchor.BottomCentre;
		_screenBrightnessText.Text = "Adjust the brightness and contrast of your screen until\nthe text on the white and black rectangles are barely visible.";
		_screenBrightnessText.FitComponentToText(5f);
		_screenBrightnessText.Depth = 0.015f;
		_promptsScreen = new TextComponent[5];
		for (int l = 0; l < _promptsScreen.Length; l++)
		{
			_promptsScreen[l] = new TextComponent();
			_promptsScreen[l].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
			_promptsScreen[l].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_promptsScreen[l].FitComponentToImage();
			TextComponent obj20 = _promptsScreen[l];
			Vector2 size8 = (_promptsScreen[l].DesiredSize = new Vector2(30f, 30f));
			obj20.Size = size8;
			TextComponent obj21 = _promptsScreen[l];
			Vector2 position17 = (_promptsScreen[l].DesiredPosition = new Vector2(_screenCentre.Position.X - _screenCentre.Size.X * 0.5f + _screenCentre.Size.X * (0.2f * (float)l) + _promptsScreen[l].Size.X * 0.5f, _screenCentre.Position.Y + _screenCentre.Size.Y * 0.5f + _promptsScreen[l].Size.Y * 0.5f + 4f));
			obj21.Position = position17;
			_promptsScreen[l].TextAnchor = MenuComponent.Anchor.MiddleLeft;
			_promptsScreen[l].Depth = 0.2f;
		}
		_promptsScreen[0].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/LeftThumbstick");
		_promptsScreen[0].Text = "     Move";
		_promptsScreen[1].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/RightThumbstick");
		_promptsScreen[1].Text = "     Resize";
		_promptsScreen[2].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_promptsScreen[2].Text = "     Accept";
		_promptsScreen[3].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptsScreen[3].Text = "     Cancel";
		_promptsScreen[4].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/Y");
		_promptsScreen[4].Text = "     Reset";
		_menuConnect = new ConnectPanel[4];
		panelSpacing = ((float)_contentArea.Width - 320f) / (float)(_menuConnect.Length - 1);
		for (int m = 0; m < _menuConnect.Length; m++)
		{
			_menuConnect[m] = new ConnectPanel();
			ConnectPanel obj22 = _menuConnect[m];
			Vector2 position18 = (_menuConnect[m].DesiredPosition = new Vector2((float)_contentArea.Left + 160f + panelSpacing * (float)m + 1280f, (float)_contentArea.Center.Y + 100f));
			obj22.Position = position18;
			_menuConnect[m].Depth = 0.01f;
			_menuConnect[m].Load(contentLoader);
			ConnectPanel obj23 = _menuConnect[m];
			Vector2 size9 = (_menuConnect[m].DesiredSize = new Vector2(200f, 260f));
			obj23.Size = size9;
			ConnectPanel obj24 = _menuConnect[m];
			Color colour5 = (_menuConnect[m].DesiredColour = new Color(102, 102, 255) * 0.5f);
			obj24.Colour = colour5;
			_menuConnect[m].PlayerIndex = (PlayerIndex)m;
		}
		Vector2 vector28 = new Vector2((float)_contentArea.Center.X + 74f + 1280f, (float)_contentArea.Top + 40f);
		_menuGames = new ListButton[_minigameMeta.Length];
		_starGames = new StarRating[_minigameMeta.Length];
		for (int n = 0; n != _minigameMeta.Length; n++)
		{
			_menuGames[n] = new ListButton();
			_menuGames[n].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
			_menuGames[n].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_menuGames[n].FitComponentToImage();
			ListButton obj25 = _menuGames[n];
			Vector2 position19 = (_menuGames[n].DesiredPosition = vector28 + new Vector2(0f, 17f));
			obj25.Position = position19;
			_menuGames[n].PositionAnchor = MenuComponent.Anchor.MiddleRight;
			_menuGames[n].TextAnchor = MenuComponent.Anchor.MiddleLeft;
			ListButton obj26 = _menuGames[n];
			Vector2 size10 = (_menuGames[n].DesiredSize = new Vector2(550f, 34f));
			obj26.Size = size10;
			ListButton obj27 = _menuGames[n];
			Color colour6 = (_menuGames[n].DesiredColour = new Color(102, 102, 255) * 0.8f);
			obj27.Colour = colour6;
			_menuGames[n].Text = _minigameMeta[n].Name;
			_menuGames[n].Depth = 0.01f;
			_starGames[n] = new StarRating();
			_starGames[n].Depth = 0.01f;
			_starGames[n].Load(contentLoader);
			StarRating obj28 = _starGames[n];
			Vector2 position20 = (_starGames[n].DesiredPosition = _menuGames[n].Position);
			obj28.Position = position20;
			_starGames[n].Depth = 0.201f;
			_starGames[n].Rating = _minigameMeta[n].Rating;
		}
		Vector2 vector31 = new Vector2((float)_contentArea.Center.X + 76f + 1280f, (float)_contentArea.Top + 100f);
		_gameImage = new MenuComponent();
		_gameImage.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/GameBanners/null");

		_gameImage.FitComponentToImage();
		MenuComponent gameImage = _gameImage;
		Vector2 position22 = (_gameImage.DesiredPosition = vector31);
		gameImage.Position = position22;
		_gameImage.PositionAnchor = MenuComponent.Anchor.TopLeft;
		MenuComponent gameImage2 = _gameImage;
		Vector2 size11 = (_gameImage.DesiredSize = new Vector2(400f, 110f));
		gameImage2.Size = size11;
		_gameImage.Depth = 0.01f;
		_gameLockImage = new MenuComponent();
		_gameLockImage.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/GameBanners/Lock");
		_gameLockImage.FitComponentToImage();
		MenuComponent gameLockImage = _gameLockImage;
		Vector2 position23 = (_gameLockImage.DesiredPosition = vector31);
		gameLockImage.Position = position23;
		_gameLockImage.PositionAnchor = MenuComponent.Anchor.TopLeft;
		MenuComponent gameLockImage2 = _gameLockImage;
		Vector2 size12 = (_gameLockImage.DesiredSize = new Vector2(400f, 110f));
		gameLockImage2.Size = size12;
		_gameLockImage.Depth = 0.02f;
		_gameDescription = new TextComponent();
		_gameDescription.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_gameDescription.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_gameDescription.FitComponentToImage();
		TextComponent gameDescription = _gameDescription;
		Vector2 size13 = (_gameDescription.DesiredSize = new Vector2(400f, 32f));
		gameDescription.Size = size13;
		TextComponent gameDescription2 = _gameDescription;
		Vector2 position24 = (_gameDescription.DesiredPosition = _gameImage.Position + new Vector2(0f, _gameImage.Size.Y + 2f));
		gameDescription2.Position = position24;
		_gameDescription.PositionAnchor = MenuComponent.Anchor.TopLeft;
		_gameDescription.TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent gameDescription3 = _gameDescription;
		Vector2 size14 = (_gameDescription.DesiredSize = new Vector2(400f, 34f));
		gameDescription3.Size = size14;
		_gameDescription.FitTextToWidth(0f);
		_gameDescription.FitComponentToText(0f);
		TextComponent gameDescription4 = _gameDescription;
		Vector2 size15 = (_gameDescription.DesiredSize = new Vector2(400f, _gameDescription.Size.Y));
		gameDescription4.Size = size15;
		TextComponent gameDescription5 = _gameDescription;
		Color colour7 = (_gameDescription.DesiredColour = new Color(102, 102, 255) * 0.8f);
		gameDescription5.Colour = colour7;
		_gameDescription.Depth = 0.01f;
		_gameGenre = new TextComponent();
		_gameGenre.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_gameGenre.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_gameGenre.FitComponentToImage();
		TextComponent gameGenre = _gameGenre;
		Vector2 position25 = (_gameGenre.DesiredPosition = _gameDescription.Position + new Vector2(0f, _gameDescription.Size.Y + 2f));
		gameGenre.Position = position25;
		_gameGenre.PositionAnchor = MenuComponent.Anchor.TopLeft;
		_gameGenre.Text = "Genre";
		TextComponent gameGenre2 = _gameGenre;
		Vector2 size16 = (_gameGenre.DesiredSize = new Vector2(400f, 32f));
		gameGenre2.Size = size16;
		TextComponent gameGenre3 = _gameGenre;
		colour = (_gameGenre.DesiredColour = new Color(102, 102, 255) * 0.8f);
		gameGenre3.Colour = colour;
		_gameGenre.Depth = 0.01f;
		_gameCompetitionImage = new MenuComponent();
		_gameCompetitionImage.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Competition/FreeForAll");
		_gameCompetitionImage.FitComponentToImage();
		MenuComponent gameCompetitionImage = _gameCompetitionImage;
		position = (_gameCompetitionImage.DesiredPosition = _gameGenre.Position + new Vector2(0f, _gameGenre.Size.Y + 2f));
		gameCompetitionImage.Position = position;
		_gameCompetitionImage.PositionAnchor = MenuComponent.Anchor.TopLeft;
		MenuComponent gameCompetitionImage2 = _gameCompetitionImage;
		position = (_gameCompetitionImage.DesiredSize = new Vector2(200f, 80f));
		gameCompetitionImage2.Size = position;
		_gameCompetitionImage.Depth = 0.02f;
		_gamePlayerLimit = new TextComponent();
		_gamePlayerLimit.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_gamePlayerLimit.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_gamePlayerLimit.FitComponentToImage();
		TextComponent gamePlayerLimit = _gamePlayerLimit;
		position = (_gamePlayerLimit.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, 0f));
		gamePlayerLimit.Position = position;
		_gamePlayerLimit.PositionAnchor = MenuComponent.Anchor.TopLeft;
		TextComponent gamePlayerLimit2 = _gamePlayerLimit;
		position = (_gamePlayerLimit.DesiredSize = new Vector2(240f, _gameCompetitionImage.Size.Y * 0.5f));
		gamePlayerLimit2.Size = position;
		TextComponent gamePlayerLimit3 = _gamePlayerLimit;
		colour = (_gamePlayerLimit.DesiredColour = new Color(102, 102, 255) * 0.8f);
		gamePlayerLimit3.Colour = colour;
		_gamePlayerLimit.Depth = 0.01f;
		_gameCompetition = new TextComponent();
		_gameCompetition.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_gameCompetition.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_gameCompetition.FitComponentToImage();
		TextComponent gameCompetition = _gameCompetition;
		position = (_gameCompetition.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, _gameCompetitionImage.Size.Y));
		gameCompetition.Position = position;
		_gameCompetition.PositionAnchor = MenuComponent.Anchor.BottomLeft;
		TextComponent gameCompetition2 = _gameCompetition;
		position = (_gameCompetition.DesiredSize = new Vector2(240f, _gameCompetitionImage.Size.Y * 0.5f));
		gameCompetition2.Size = position;
		TextComponent gameCompetition3 = _gameCompetition;
		colour = (_gameCompetition.DesiredColour = new Color(102, 102, 255) * 0.8f);
		gameCompetition3.Colour = colour;
		_gameCompetition.Depth = 0.01f;
		_gameHighscore = new TextComponent();
		_gameHighscore.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_gameHighscore.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_gameHighscore.FitComponentToImage();
		TextComponent gameHighscore = _gameHighscore;
		position = (_gameHighscore.DesiredPosition = _gameCompetitionImage.Position + new Vector2(0f, _gameCompetitionImage.Size.Y + 2f));
		gameHighscore.Position = position;
		_gameHighscore.PositionAnchor = MenuComponent.Anchor.TopLeft;
		_gameHighscore.TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent gameHighscore2 = _gameHighscore;
		position = (_gameHighscore.DesiredSize = new Vector2(400f, 34f));
		gameHighscore2.Size = position;
		_gameHighscore.FitTextToWidth(0f);
		_gameHighscore.FitComponentToText(0f);
		TextComponent gameHighscore3 = _gameHighscore;
		position = (_gameHighscore.DesiredSize = new Vector2(400f, _gameHighscore.Size.Y));
		gameHighscore3.Size = position;
		TextComponent gameHighscore4 = _gameHighscore;
		colour = (_gameHighscore.DesiredColour = new Color(102, 102, 255) * 0.8f);
		gameHighscore4.Colour = colour;
		_gameHighscore.Depth = 0.01f;
		_sortModeText = new TextComponent();
		_sortModeText.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_sortModeText.FitComponentToText(0f);
		_sortModeText.PositionAnchor = MenuComponent.Anchor.BottomRight;
		_sortModeText.TextAnchor = MenuComponent.Anchor.BottomRight;
		TextComponent sortModeText = _sortModeText;
		position = (_sortModeText.DesiredPosition = new Vector2(_gameImage.Position.X + _gameImage.Size.X, (float)_contentArea.Bottom - 40f));
		sortModeText.Position = position;
		_sortModeText.Text = "";
		TextComponent sortModeText2 = _sortModeText;
		colour = (_sortModeText.DesiredTextColour = Color.LightGray);
		sortModeText2.TextColour = colour;
		_sortModeText.Depth = 0.01f;
		_promptsGames = new TextComponent[4];
		for (int num2 = 0; num2 < _promptsGames.Length; num2++)
		{
			_promptsGames[num2] = new TextComponent();
			_promptsGames[num2].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
			_promptsGames[num2].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_promptsGames[num2].FitComponentToImage();
			TextComponent obj29 = _promptsGames[num2];
			position = (_promptsGames[num2].DesiredSize = new Vector2(30f, 30f));
			obj29.Size = position;
			TextComponent obj30 = _promptsGames[num2];
			position = (_promptsGames[num2].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * (0.25f * (float)num2) + _promptsGames[num2].Size.X * 0.5f + 1280f, _gameHighscore.Size.Y + 2f + _promptsGames[num2].Size.Y * 0.5f));
			obj30.Position = position;
			_promptsGames[num2].TextAnchor = MenuComponent.Anchor.MiddleLeft;
			_promptsGames[num2].Depth = 0.2f;
		}
		_promptsGames[0].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_promptsGames[0].Text = "     Play";
		_promptsGames[1].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptsGames[1].Text = "     Back";
		_promptsGames[2].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/X");
		_promptsGames[2].Text = "     Sort";
		_promptsGames[3].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/Y");
		_promptsGames[3].Text = "     Rate";
		_instructionImage = new MenuComponent();
		_instructionImage.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Instructions/null");
		_instructionImage.FitComponentToImage();
		MenuComponent instructionImage = _instructionImage;
		position = (_instructionImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y));
		instructionImage.Position = position;
		_instructionImage.Depth = 0.01f;
		_promptsInstruction = new TextComponent[2];
		_promptsInstruction[0] = new TextComponent();
		_promptsInstruction[0].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_promptsInstruction[0].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsInstruction[0].FitComponentToImage();
		TextComponent obj31 = _promptsInstruction[0];
		position = (_promptsInstruction[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsInstruction[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsInstruction[0].Size.Y * 0.5f));
		obj31.Position = position;
		_promptsInstruction[0].TextAnchor = MenuComponent.Anchor.MiddleRight;
		TextComponent obj32 = _promptsInstruction[0];
		position = (_promptsInstruction[0].DesiredSize = new Vector2(30f, 30f));
		obj32.Size = position;
		_promptsInstruction[0].Text = "Play     ";
		_promptsInstruction[0].Depth = 0.2f;
		_promptsInstruction[1] = new TextComponent();
		_promptsInstruction[1].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptsInstruction[1].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsInstruction[1].FitComponentToImage();
		TextComponent obj33 = _promptsInstruction[1];
		position = (_promptsInstruction[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsInstruction[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsInstruction[1].Size.Y * 0.5f));
		obj33.Position = position;
		_promptsInstruction[1].TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent obj34 = _promptsInstruction[1];
		position = (_promptsInstruction[1].DesiredSize = new Vector2(30f, 30f));
		obj34.Size = position;
		_promptsInstruction[1].Text = "     Back";
		_promptsInstruction[1].Depth = 0.2f;
		_loading = new TextComponent();
		_loading.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_loading.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_loading.FitComponentToImage();
		TextComponent loading = _loading;
		position = (_loading.DesiredPosition = new Vector2((float)_contentArea.Right - 10f, (float)_contentArea.Bottom - 10f));
		loading.Position = position;
		_loading.PositionAnchor = MenuComponent.Anchor.BottomRight;
		_loading.IsOutlined = true;
		TextComponent loading2 = _loading;
		colour = (_loading.DesiredColour = Color.Black * 0f);
		loading2.Colour = colour;
		_loading.Text = "Now Loading...";
		_loading.FitComponentToText(5f);
		_loading.Depth = 0.01f;
		_creditsLogo = new MenuComponent();
		_creditsLogo.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/CreditsLogo");
		_creditsLogo.FitComponentToImage();
		MenuComponent creditsLogo = _creditsLogo;
		position = (_creditsLogo.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 160f));
		creditsLogo.Position = position;
		_creditsLogo.PositionAnchor = MenuComponent.Anchor.Centre;
		MenuComponent creditsLogo2 = _creditsLogo;
		colour = (_creditsLogo.DesiredColour = Color.White);
		creditsLogo2.Colour = colour;
		_creditsLogo.Depth = 0.01f;
		_creditsHeaders = new TextComponent[2];
		for (int num3 = 0; num3 != _creditsHeaders.Length; num3++)
		{
			_creditsHeaders[num3] = new TextComponent();
			_creditsHeaders[num3].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
			_creditsHeaders[num3].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_creditsHeaders[num3].FitComponentToImage();
			_creditsHeaders[num3].PositionAnchor = MenuComponent.Anchor.Centre;
			_creditsHeaders[num3].TextAnchor = MenuComponent.Anchor.Centre;
			TextComponent obj35 = _creditsHeaders[num3];
			position = (_creditsHeaders[num3].DesiredSize = new Vector2(560f, 36f));
			obj35.Size = position;
			TextComponent obj36 = _creditsHeaders[num3];
			colour = (_creditsHeaders[num3].DesiredColour = Color.Black * 0.8f);
			obj36.Colour = colour;
			_creditsHeaders[num3].Depth = 0.01f;
		}
		_creditsHeaders[0].Text = "Development";
		TextComponent obj37 = _creditsHeaders[0];
		position = (_creditsHeaders[0].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f));
		obj37.Position = position;
		_creditsHeaders[1].Text = "Special Thanks";
		TextComponent obj38 = _creditsHeaders[1];
		position = (_creditsHeaders[1].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 90f));
		obj38.Position = position;
		_creditsNames = new TextComponent[5];
		for (int num4 = 0; num4 != _creditsNames.Length; num4++)
		{
			_creditsNames[num4] = new TextComponent();
			_creditsNames[num4].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
			_creditsNames[num4].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_creditsNames[num4].FitComponentToImage();
			_creditsNames[num4].PositionAnchor = MenuComponent.Anchor.Centre;
			_creditsNames[num4].TextAnchor = MenuComponent.Anchor.Centre;
			TextComponent obj39 = _creditsNames[num4];
			position = (_creditsNames[num4].DesiredSize = new Vector2(560f, 36f));
			obj39.Size = position;
			TextComponent obj40 = _creditsNames[num4];
			colour = (_creditsNames[num4].DesiredColour = Color.Black * 0.8f);
			obj40.Colour = colour;
			_creditsNames[num4].Depth = 0.01f;
		}
		_creditsNames[0].Text = "David Jones, Kevin Chandler, Laurie Brown,";
		TextComponent obj41 = _creditsNames[0];
		position = (_creditsNames[0].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y));
		obj41.Position = position;
		_creditsNames[1].Text = "Theo Chin, Robert Shilling";
		TextComponent obj42 = _creditsNames[1];
		position = (_creditsNames[1].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y * 2f));
		obj42.Position = position;
		_creditsNames[2].Text = "Andrew Roper, Lenny Joseph-Mathurin,";
		TextComponent obj43 = _creditsNames[2];
		position = (_creditsNames[2].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y));
		obj43.Position = position;
		_creditsNames[3].Text = "Chris Barnes, Jake Woodruff,";
		TextComponent obj44 = _creditsNames[3];
		position = (_creditsNames[3].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 2f));
		obj44.Position = position;
		_creditsNames[4].Text = "Matt Floyd, David Harris";
		TextComponent obj45 = _creditsNames[4];
		position = (_creditsNames[4].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 3f));
		obj45.Position = position;
		_promptCredits = new TextComponent();
		_promptCredits.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptCredits.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptCredits.FitComponentToImage();
		TextComponent promptCredits = _promptCredits;
		position = (_promptCredits.DesiredPosition = new Vector2((float)_contentArea.Left + _promptCredits.Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptCredits.Size.Y * 0.5f));
		promptCredits.Position = position;
		_promptCredits.TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent promptCredits2 = _promptCredits;
		position = (_promptCredits.DesiredSize = new Vector2(30f, 30f));
		promptCredits2.Size = position;
		_promptCredits.Text = "     Back";
		_promptCredits.Depth = 0.2f;
		_menuConfirm = new ListButton[2];
		for (int num5 = 0; num5 < _menuConfirm.Length; num5++)
		{
			_menuConfirm[num5] = new ListButton();
			_menuConfirm[num5].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
			_menuConfirm[num5].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_menuConfirm[num5].FitComponentToImage();
			ListButton obj46 = _menuConfirm[num5];
			position = (_menuConfirm[num5].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)num5));
			obj46.Position = position;
			ListButton obj47 = _menuConfirm[num5];
			position = (_menuConfirm[num5].DesiredSize = new Vector2(200f, 34f));
			obj47.Size = position;
			ListButton obj48 = _menuConfirm[num5];
			colour = (_menuConfirm[num5].DesiredColour = new Color(102, 102, 255) * 0.8f);
			obj48.Colour = colour;
			_menuConfirm[num5].Depth = 0.01f;
		}
		_menuConfirm[0].Text = "Yes";
		_menuConfirm[1].Text = "No";
		_confirmText = new TextComponent();
		_confirmText.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		TextComponent confirmText = _confirmText;
		position = (_confirmText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f));
		confirmText.Position = position;
		_confirmText.PositionAnchor = MenuComponent.Anchor.BottomCentre;
		_confirmText.Depth = 0.01f;
		_promptsConfirm = new TextComponent[2];
		_promptsConfirm[0] = new TextComponent();
		_promptsConfirm[0].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_promptsConfirm[0].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsConfirm[0].FitComponentToImage();
		TextComponent obj49 = _promptsConfirm[0];
		position = (_promptsConfirm[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsConfirm[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsConfirm[0].Size.Y * 0.5f));
		obj49.Position = position;
		_promptsConfirm[0].TextAnchor = MenuComponent.Anchor.MiddleRight;
		TextComponent obj50 = _promptsConfirm[0];
		position = (_promptsConfirm[0].DesiredSize = new Vector2(30f, 30f));
		obj50.Size = position;
		_promptsConfirm[0].Text = "Select     ";
		_promptsConfirm[0].Depth = 0.2f;
		_promptsConfirm[1] = new TextComponent();
		_promptsConfirm[1].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptsConfirm[1].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsConfirm[1].FitComponentToImage();
		TextComponent obj51 = _promptsConfirm[1];
		position = (_promptsConfirm[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsConfirm[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsConfirm[1].Size.Y * 0.5f));
		obj51.Position = position;
		_promptsConfirm[1].TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent obj52 = _promptsConfirm[1];
		position = (_promptsConfirm[1].DesiredSize = new Vector2(30f, 30f));
		obj52.Size = position;
		_promptsConfirm[1].Text = "     Back";
		_promptsConfirm[1].Depth = 0.2f;
		_pauseBackground = new MenuComponent();
		_pauseBackground.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		MenuComponent pauseBackground = _pauseBackground;
		position = (_pauseBackground.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y));
		pauseBackground.Position = position;
		MenuComponent pauseBackground2 = _pauseBackground;
		position = (_pauseBackground.DesiredSize = new Vector2(250f, 230f));
		pauseBackground2.Size = position;
		MenuComponent pauseBackground3 = _pauseBackground;
		colour = (_pauseBackground.DesiredColour = Color.Black * 0.8f);
		pauseBackground3.Colour = colour;
		_pauseHeader = new TextComponent();
		_pauseHeader.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_pauseHeader.Text = "Paused";
		TextComponent pauseHeader = _pauseHeader;
		position = (_pauseHeader.DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.4f));
		pauseHeader.Position = position;
		_pauseHeader.Depth = 0.01f;
		_pauseHeader.MoveSpeed = 1f;
		_pauseName = new TextComponent();
		_pauseName.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_pauseName.Text = "";
		TextComponent pauseName = _pauseName;
		position = (_pauseName.DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.3f));
		pauseName.Position = position;
		_pauseName.TextAnchor = MenuComponent.Anchor.TopCentre;
		_pauseName.Depth = 0.01f;
		_pauseName.MoveSpeed = 1f;
		_menuPause = new ListButton[3];
		for (int num6 = 0; num6 < _menuPause.Length; num6++)
		{
			_menuPause[num6] = new ListButton();
			_menuPause[num6].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
			_menuPause[num6].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			_menuPause[num6].FitComponentToImage();
			ListButton obj53 = _menuPause[num6];
			position = (_menuPause[num6].DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.02f + 36f * (float)num6));
			obj53.Position = position;
			ListButton obj54 = _menuPause[num6];
			position = (_menuPause[num6].DesiredSize = new Vector2(200f, 34f));
			obj54.Size = position;
			_menuPause[num6].Depth = 0.02f;
			_menuPause[num6].MoveSpeed = 1f;
		}
		_menuPause[0].Text = "Resume";
		_menuPause[1].Text = "Vibration: ";
		_menuPause[2].Text = "Quit";
		_AButtonSprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_StartButtonSprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/Start");
		_BackButtonSprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/Back");
		_promptsPause = new TextComponent[2];
		_promptsPause[0] = new TextComponent();
		_promptsPause[0].Sprite = _AButtonSprite;
		_promptsPause[0].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsPause[0].FitComponentToImage();
		TextComponent obj55 = _promptsPause[0];
		position = (_promptsPause[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsPause[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsPause[0].Size.Y * 0.5f));
		obj55.Position = position;
		_promptsPause[0].TextAnchor = MenuComponent.Anchor.MiddleRight;
		TextComponent obj56 = _promptsPause[0];
		position = (_promptsPause[0].DesiredSize = new Vector2(30f, 30f));
		obj56.Size = position;
		_promptsPause[0].Text = "Select     ";
		_promptsPause[0].IsOutlined = true;
		TextComponent obj57 = _promptsPause[0];
		colour = (_promptsPause[0].DesiredColour = Color.White * 0f);
		obj57.Colour = colour;
		TextComponent obj58 = _promptsPause[0];
		colour = (_promptsPause[0].DesiredOutlineColour = Color.Black * 0f);
		obj58.OutlineColour = colour;
		_promptsPause[0].Depth = 0.2f;
		_promptsPause[1] = new TextComponent();
		_promptsPause[1].Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_promptsPause[1].Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_promptsPause[1].FitComponentToImage();
		TextComponent obj59 = _promptsPause[1];
		position = (_promptsPause[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsPause[1].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsPause[1].Size.Y * 0.5f));
		obj59.Position = position;
		_promptsPause[1].TextAnchor = MenuComponent.Anchor.MiddleLeft;
		TextComponent obj60 = _promptsPause[1];
		position = (_promptsPause[1].DesiredSize = new Vector2(30f, 30f));
		obj60.Size = position;
		_promptsPause[1].Text = "     Resume";
		_promptsPause[1].IsOutlined = true;
		TextComponent obj61 = _promptsPause[1];
		colour = (_promptsPause[1].DesiredColour = Color.White * 0f);
		obj61.Colour = colour;
		TextComponent obj62 = _promptsPause[1];
		colour = (_promptsPause[1].DesiredOutlineColour = Color.Black * 0f);
		obj62.OutlineColour = colour;
		_promptsPause[1].Depth = 0.2f;
	}

	public short Update(GameTime gameTime)
	{
		short result = 0;
		if (_menuState != MenuState.Pause && _menuState != MenuState.Disconnect && !_playerManager.GetGamePad(_playerInControl).GamePadStateCurrent.IsConnected)
		{
			if (_playerManager.GetGamePad(_leader).GamePadStateCurrent.IsConnected)
			{
				_playerInControl = _leader;
			}
			else if (_playerManager.NumberOfPlayers != 0)
			{
				for (int i = 0; i != _playerManager.PlayersConnected.Count; i++)
				{
					if (_playerManager.PlayersConnected[i].GamePadManager.GamePadStateCurrent.IsConnected)
					{
						_playerInControl = _playerManager.PlayersConnected[i].PlayerIndex;
					}
				}
			}
			else
			{
				for (int j = 0; j != 4; j++)
				{
					if (_playerManager.GetGamePad((PlayerIndex)j).GamePadStateCurrent.IsConnected)
					{
						_playerInControl = (PlayerIndex)j;
					}
				}
			}
		}
		if (_menuState != _nextState)
		{
			ChangeState(_nextState, gameTime);
		}
		else
		{
			switch (_menuState)
			{
			case MenuState.Start:
			{
				for (int num34 = 0; num34 != 4; num34++)
				{
					if (!_playerManager.GetGamePad((PlayerIndex)num34).ButtonWasPressed(Buttons.Start) && !_playerManager.GetGamePad((PlayerIndex)num34).ButtonWasPressed(Buttons.A))
					{
						continue;
					}
					_leader = (_playerInControl = (PlayerIndex)num34);
					GameConsole.PrintString("Menu: Game pad " + (num34 + 1) + " is the leader.");
					if (!Guide.IsVisible)
					{
						_leaderPlayer = new Player(_leader, _playerManager, _soundManager);
						if (_leaderPlayer.Gamer == null)
						{
							_leaderPlayer.Name = "Default";
						}
						_leaderPlayer.GamePadManager = _playerManager.GetGamePad((PlayerIndex)num34);
						_storageManager.SelectStorageDevice(_leaderPlayer, ref _minigameMeta);
						_nextState = MenuState.Main;
						GameConsole.PrintString("Menu: Settings profile is " + _leaderPlayer.Name + ".");
					}
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
				}
				_logo.Update(gameTime);
				_logoShadow.Update(gameTime);
				_start.Update(gameTime);
				break;
			}
			case MenuState.Main:
			{
				if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadDown))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadDown))
					{
						_selectedIndex++;
						if (_selectedIndex == _menuMain.Length)
						{
							_selectedIndex = 0;
						}
						if (!Guide.IsTrialMode && _selectedIndex == 2)
						{
							_selectedIndex++;
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadUp))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadUp))
					{
						_selectedIndex--;
						if (_selectedIndex == -1)
						{
							_selectedIndex = _menuMain.Length - 1;
						}
						if (!Guide.IsTrialMode && _selectedIndex == 2)
						{
							_selectedIndex--;
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					_holdTimers[(int)_playerInControl] = 0;
					_holdRepeatTime[(int)_playerInControl] = 200;
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.A))
				{
					switch (_selectedIndex)
					{
					case 0:
						_nextState = MenuState.Connect;
						break;
					case 1:
						_nextState = MenuState.Settings;
						break;
					case 2:
						if (!Guide.IsVisible && Guide.IsTrialMode && Gamer.SignedInGamers[_playerInControl] != null && Gamer.SignedInGamers[_playerInControl].Privileges.AllowPurchaseContent)
						{
							Guide.ShowMarketplace(_playerInControl);
							_buying = true;
						}
						break;
					case 3:
						_nextState = MenuState.Credits;
						break;
					case 4:
						_confirmAction = ConfirmAction.QuitGame;
						_confirmText.Text = "Are you sure you want to quit?";
						_confirmText.FitComponentToText(5f);
						_nextState = MenuState.Confirm;
						break;
					}
					_soundManager.CreateMenuSoundCue("menu Click").Play();
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.B))
				{
					_nextState = MenuState.Start;
				}
				_logo.Update(gameTime);
				if (_buying && !Guide.IsVisible)
				{
					_buying = false;
					if (!Guide.IsTrialMode)
					{
						_selectedIndex = 0;
					}
				}
				if (!Guide.IsTrialMode)
				{
					_menuMain[3].DesiredPosition = _menuMain[2].Position;
					_menuMain[4].DesiredPosition = _menuMain[2].Position + new Vector2(0f, 36f);
				}
				for (int num7 = 0; num7 != _menuMain.Length; num7++)
				{
					_menuMain[num7].IsHighlighted = _selectedIndex == num7;
					_menuMain[num7].Update(gameTime);
				}
				_menuMain[2].Enabled = (Gamer.SignedInGamers[_playerInControl] != null && Gamer.SignedInGamers[_playerInControl].Privileges.AllowPurchaseContent) || _menuMain[2].IsHighlighted;
				break;
			}
			case MenuState.Connect:
			{
				int num8 = 0;
				for (int num9 = 0; num9 < 4; num9++)
				{
					if (_menuConnect[num9].IsReady)
					{
						num8++;
					}
				}
				bool flag = num8 == _playerManager.PlayersConnected.Count;
				for (int num10 = 0; num10 != 4; num10++)
				{
					if (_menuConnect[num10].IsActive && !_menuConnect[num10].IsReady && (_playerManager.GetGamePad((PlayerIndex)num10).ButtonIsHeld(Buttons.LeftThumbstickRight) || _playerManager.GetGamePad((PlayerIndex)num10).ButtonIsHeld(Buttons.DPadRight)))
					{
						if (_holdTimers[num10] > 600 || _playerManager.GetGamePad((PlayerIndex)num10).ButtonWasPressed(Buttons.LeftThumbstickRight) || _playerManager.GetGamePad((PlayerIndex)num10).ButtonWasPressed(Buttons.DPadRight))
						{
							_playerManager.SelectNextColor(_playerManager.GetPlayer((PlayerIndex)num10));
							_soundManager.CreateMenuSoundCue("menu Tick").Play();
							_holdTimers[num10] -= _holdRepeatTime[num10];
						}
						if (_holdRepeatTime[num10] > 60)
						{
							_holdRepeatTime[num10]--;
							if (_holdRepeatTime[num10] < 60)
							{
								_holdRepeatTime[num10] = 60;
							}
						}
						_holdTimers[num10] += gameTime.ElapsedGameTime.Milliseconds;
					}
					else if (_menuConnect[num10].IsActive && !_menuConnect[num10].IsReady && (_playerManager.GetGamePad((PlayerIndex)num10).ButtonIsHeld(Buttons.LeftThumbstickLeft) || _playerManager.GetGamePad((PlayerIndex)num10).ButtonIsHeld(Buttons.DPadLeft)))
					{
						if (_holdTimers[num10] > 600 || _playerManager.GetGamePad((PlayerIndex)num10).ButtonWasPressed(Buttons.LeftThumbstickLeft) || _playerManager.GetGamePad((PlayerIndex)num10).ButtonWasPressed(Buttons.DPadLeft))
						{
							_playerManager.SelectPreviousColor(_playerManager.GetPlayer((PlayerIndex)num10));
							_soundManager.CreateMenuSoundCue("menu Tick").Play();
							_holdTimers[num10] -= _holdRepeatTime[num10];
						}
						if (_holdRepeatTime[num10] > 60)
						{
							_holdRepeatTime[num10]--;
							if (_holdRepeatTime[num10] < 60)
							{
								_holdRepeatTime[num10] = 60;
							}
						}
						_holdTimers[num10] += gameTime.ElapsedGameTime.Milliseconds;
					}
					else
					{
						_holdTimers[num10] = 0;
						_holdRepeatTime[num10] = 200;
					}
				}
				for (int num11 = 0; num11 < _playerManager.NumberOfPlayers; num11++)
				{
					if (_playerManager.PlayersConnected.Count != 0 && _playerManager.PlayersConnected[num11].GamePadManager.ButtonWasPressed(Buttons.Start) && flag)
					{
						for (int num12 = 0; num12 < _playerManager.NumberOfPlayers; num12++)
						{
							_storageManager.Save(_playerManager.PlayersConnected[num12], saveCurrentSettings: false);
						}
						if (_playerManager.GetPlayer(_leader) != null)
						{
							_leaderPlayer.ColorIndex = _playerManager.GetPlayer(_leader).ColorIndex;
						}
						_playerManager.ConnectState = false;
						SignedInGamer.SignedIn -= Connect_GamerSignedIn;
						SignedInGamer.SignedOut -= Connect_GamerSignedOut;
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						_nextState = MenuState.Games;
					}
					if (!_playerManager.PlayersConnected[num11].GamePadManager.GamePadStateCurrent.IsConnected)
					{
						_playerManager.PlayerLeave(_playerManager.PlayersConnected[num11].PlayerIndex);
					}
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.B) && !_menuConnect[(int)_playerInControl].IsActive)
				{
					_playerManager.KickAllPlayers();
					for (int num13 = 0; num13 != _menuConnect.Length; num13++)
					{
						_menuConnect[num13].IsReady = false;
						_menuConnect[num13].IsActive = false;
					}
					_playerManager.ConnectState = false;
					SignedInGamer.SignedIn -= Connect_GamerSignedIn;
					SignedInGamer.SignedOut -= Connect_GamerSignedOut;
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
					_nextState = MenuState.Main;
				}
				_logo.Update(gameTime);
				ConnectPanel[] menuConnect = _menuConnect;
				foreach (ConnectPanel connectPanel in menuConnect)
				{
					connectPanel.EveryoneReady = flag;
					connectPanel.Update(gameTime, _playerManager, _storageManager, _soundManager);
				}
				break;
			}
			case MenuState.Games:
			{
				for (int num25 = 0; num25 != _playerManager.NumberOfPlayers; num25++)
				{
					if (_playerManager.PlayersConnected[num25].GamerProblem)
					{
						_playerManager.PlayersConnected[num25].Name = "Player " + (num25 + 1);
					}
					if (_playerManager.PlayersConnected[num25].GamePadManager.ButtonIsHeld(Buttons.LeftThumbstickDown) || _playerManager.PlayersConnected[num25].GamePadManager.ButtonIsHeld(Buttons.DPadDown))
					{
						if (_holdTimers[num25] > 600 || _playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.LeftThumbstickDown) || _playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.DPadDown))
						{
							_selectedIndex++;
							if (_selectedIndex == _menuGames.Length)
							{
								_selectedIndex = 0;
							}
							UpdateGameMeta();
							_soundManager.CreateMenuSoundCue("menu Tick").Play();
							_holdTimers[num25] -= _holdRepeatTime[num25];
						}
						if (_holdRepeatTime[num25] > 60)
						{
							_holdRepeatTime[num25]--;
							if (_holdRepeatTime[num25] < 60)
							{
								_holdRepeatTime[num25] = 60;
							}
						}
						_holdTimers[num25] += gameTime.ElapsedGameTime.Milliseconds;
					}
					else if (_playerManager.PlayersConnected[num25].GamePadManager.ButtonIsHeld(Buttons.LeftThumbstickUp) || _playerManager.PlayersConnected[num25].GamePadManager.ButtonIsHeld(Buttons.DPadUp))
					{
						if (_holdTimers[num25] > 600 || _playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.LeftThumbstickUp) || _playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.DPadUp))
						{
							_selectedIndex--;
							if (_selectedIndex == -1)
							{
								_selectedIndex = _menuGames.Length - 1;
							}
							UpdateGameMeta();
							_soundManager.CreateMenuSoundCue("menu Tick").Play();
							_holdTimers[num25] -= _holdRepeatTime[num25];
						}
						if (_holdRepeatTime[num25] > 60)
						{
							_holdRepeatTime[num25]--;
							if (_holdRepeatTime[num25] < 60)
							{
								_holdRepeatTime[num25] = 60;
							}
						}
						_holdTimers[num25] += gameTime.ElapsedGameTime.Milliseconds;
					}
					else
					{
						_holdTimers[num25] = 0;
						_holdRepeatTime[num25] = 200;
					}
					if (_playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.A) && _playerManager.NumberOfPlayers >= _minigameMeta[_selectedIndex].MinimumPlayers && _playerManager.NumberOfPlayers <= _minigameMeta[_selectedIndex].MaximumPlayers)
					{
						_soundManager.CreateMenuSoundCue("menu Click").Play();
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						if (Guide.IsTrialMode && _demoLock[_selectedIndex])
						{
							_selectedGame = _selectedIndex;
							_confirmText.FitComponentToText(5f);
							_nextState = MenuState.Confirm;
							if (Gamer.SignedInGamers[_playerInControl] != null && Gamer.SignedInGamers[_playerInControl].Privileges.AllowPurchaseContent)
							{
								_confirmText.Text = "Game locked in demo mode. Buy full version and play?";
								_confirmAction = ConfirmAction.ValidBuy;
								_selectedIndex = 0;
							}
							else if (Gamer.SignedInGamers[_playerInControl] == null || !Gamer.SignedInGamers[_playerInControl].Privileges.AllowPurchaseContent)
							{
								_confirmText.Text = "Game locked in demo mode. Sign in to unlock.";
								_confirmAction = ConfirmAction.InvalidBuy;
								_selectedIndex = 0;
							}
						}
						else
						{
							_nextState = MenuState.Instruction;
						}
					}
					if (_playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.B))
					{
						_storageManager.Save(_minigameMeta);
						_storageManager.Save(_leaderPlayer, saveCurrentSettings: true);
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						_nextState = MenuState.Connect;
					}
					if (_playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.X))
					{
						_sortMode++;
						_sortMode = ((_sortMode <= MinigameMeta.SortMode.Genre) ? _sortMode : MinigameMeta.SortMode.Unsorted);
						MinigameMeta.Sort(ref _minigameMeta, MinigameMeta.SortMode.Unsorted);
						MinigameMeta.Sort(ref _minigameMeta, _sortMode);
						switch (_sortMode)
						{
						case MinigameMeta.SortMode.Unsorted:
							_sortModeText.Text = "";
							break;
						case MinigameMeta.SortMode.Rating:
							_sortModeText.Text = "Sorted by rating.";
							break;
						case MinigameMeta.SortMode.Name:
							_sortModeText.Text = "Sorted by name.";
							break;
						case MinigameMeta.SortMode.MinPlayers:
							_sortModeText.Text = "Sorted by minimum players.";
							break;
						case MinigameMeta.SortMode.MaxPlayers:
							_sortModeText.Text = "Sorted by maximum players.";
							break;
						case MinigameMeta.SortMode.Genre:
							_sortModeText.Text = "Sorted by genre.";
							break;
						case MinigameMeta.SortMode.Competition:
							_sortModeText.Text = "Sorted by competition type.";
							break;
						default:
							_sortModeText.Text = "";
							break;
						}
						for (int num26 = 0; num26 != _minigameMeta.Length; num26++)
						{
							_menuGames[num26].Text = _minigameMeta[num26].Name;
							_starGames[num26].Rating = _minigameMeta[num26].Rating;
						}
						_leaderPlayer.SortMode = (byte)_sortMode;
						UpdateGameMeta();
						_soundManager.CreateMenuSoundCue("menu Click").Play();
					}
					if (_playerManager.PlayersConnected[num25].GamePadManager.ButtonWasPressed(Buttons.Y))
					{
						_minigameMeta[_selectedIndex].Rating++;
						if (_minigameMeta[_selectedIndex].Rating > 5)
						{
							_minigameMeta[_selectedIndex].Rating = 0;
						}
						_soundManager.CreateMenuSoundCue("menu Click").Play();
					}
				}
				Vector2 vector62 = new Vector2((float)_contentArea.Center.X + 74f, (float)_contentArea.Top + 40f);
				if (_selectedIndex * 36 < (int)((float)_contentArea.Height * 0.5f))
				{
					_scrollOffset = 0f;
				}
				else if ((_minigameMeta.Length - 1 - _selectedIndex) * 36 < (int)((float)_contentArea.Height * 0.5f))
				{
					_scrollOffset = (float)_contentArea.Bottom - vector62.Y - 36f * (float)(_minigameMeta.Length - 1) - 27f;
				}
				else
				{
					_scrollOffset = (float)_contentArea.Center.Y - vector62.Y - 36f * (float)_selectedIndex;
				}
				for (int num27 = 0; num27 != _menuGames.Length; num27++)
				{
					if (_selectedIndex == num27 || (_playerManager.NumberOfPlayers >= _minigameMeta[num27].MinimumPlayers && _playerManager.NumberOfPlayers <= _minigameMeta[num27].MaximumPlayers && (!Guide.IsTrialMode || !_demoLock[num27])))
					{
						_menuGames[num27].Enabled = true;
					}
					else
					{
						_menuGames[num27].Enabled = false;
						_menuGames[num27].DesiredSize = new Vector2(550f, 34f);
						_menuGames[num27].DesiredColour = new Color(102, 102, 255) * 0.4f;
					}
					_menuGames[num27].IsHighlighted = _selectedIndex == num27;
					_menuGames[num27].DesiredPosition = vector62 + new Vector2(0f, 17f + 36f * (float)num27 + _scrollOffset);
					_menuGames[num27].Update(gameTime);
					StarRating obj41 = _starGames[num27];
					Vector2 position = (_starGames[num27].DesiredPosition = _menuGames[num27].Position);
					obj41.Position = position;
					_starGames[num27].Rating = _minigameMeta[num27].Rating;
					_starGames[num27].Update(gameTime);
				}
				if (_playerManager.NumberOfPlayers >= _minigameMeta[_selectedIndex].MinimumPlayers && _playerManager.NumberOfPlayers <= _minigameMeta[_selectedIndex].MaximumPlayers)
				{
					_gamePlayerLimit.DesiredTextColour = Color.White;
				}
				else
				{
					_gamePlayerLimit.DesiredTextColour = Color.Red;
				}
				_gameImage.Update(gameTime);
				_gameLockImage.Update(gameTime);
				_gameDescription.Update(gameTime);
				_gameHighscore.Update(gameTime);
				_gameCompetitionImage.Update(gameTime);
				_gamePlayerLimit.Update(gameTime);
				_gameCompetition.Update(gameTime);
				_gameGenre.Update(gameTime);
				break;
			}
			case MenuState.Instruction:
			{
				for (int num28 = 0; num28 < _playerManager.NumberOfPlayers; num28++)
				{
					if (_playerManager.PlayersConnected[num28].GamePadManager.ButtonWasReleased(Buttons.A))
					{
						_storageManager.Save(_minigameMeta);
						_storageManager.Save(_leaderPlayer, saveCurrentSettings: true);
						result = _minigameMeta[_selectedIndex].MinigameID;
						_soundManager.CreateMenuSoundCue("menu Click").Play();
						_nextState = MenuState.Loading;
					}
					if (_playerManager.PlayersConnected[num28].GamePadManager.ButtonWasPressed(Buttons.B))
					{
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						_nextState = MenuState.Games;
					}
					if (_playerManager.PlayersConnected[num28].GamerProblem)
					{
						_playerManager.PlayerLeave(_playerInControl);
					}
				}
				_instructionImage.Update(gameTime);
				break;
			}
			case MenuState.Settings:
			{
				if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadDown))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadDown))
					{
						_selectedIndex++;
						if (_selectedIndex == _menuSettings.Length)
						{
							_selectedIndex = 0;
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadUp))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadUp))
					{
						_selectedIndex--;
						if (_selectedIndex == -1)
						{
							_selectedIndex = _menuSettings.Length - 1;
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickLeft) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadLeft))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickLeft) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadLeft))
					{
						switch (_selectedIndex)
						{
						case 0:
						{
							float num = _soundManager.MusicVolume - 0.1f;
							_soundManager.MusicVolume = ((num < 0f) ? 1f : ((float)Math.Round(num, 1)));
							break;
						}
						case 1:
						{
							float num = _soundManager.EffectVolume - 0.1f;
							_soundManager.EffectVolume = ((num < 0f) ? 1f : ((float)Math.Round(num, 1)));
							break;
						}
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickRight) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadRight))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickRight) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadRight))
					{
						switch (_selectedIndex)
						{
						case 0:
						{
							float num2 = _soundManager.MusicVolume + 0.1f;
							_soundManager.MusicVolume = ((num2 > 1f) ? 0f : ((float)Math.Round(num2, 1)));
							break;
						}
						case 1:
						{
							float num2 = _soundManager.EffectVolume + 0.1f;
							_soundManager.EffectVolume = ((num2 > 1f) ? 0f : ((float)Math.Round(num2, 1)));
							break;
						}
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					_holdTimers[(int)_playerInControl] = 0;
					_holdRepeatTime[(int)_playerInControl] = 200;
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.A))
				{
					switch (_selectedIndex)
					{
					case 0:
						_soundManager.MusicVolume += 0.1f;
						_soundManager.MusicVolume = ((_soundManager.MusicVolume > 1f) ? 0f : ((float)Math.Round(_soundManager.MusicVolume, 1)));
						break;
					case 1:
						_soundManager.EffectVolume += 0.1f;
						_soundManager.EffectVolume = ((_soundManager.EffectVolume > 1f) ? 0f : ((float)Math.Round(_soundManager.EffectVolume, 1)));
						break;
					case 2:
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						_nextState = MenuState.Screen;
						break;
					case 3:
						_confirmAction = ConfirmAction.ClearRatings;
						_confirmText.Text = "Are you sure you want to clear your star ratings? This cannot be undone.";
						_confirmText.FitComponentToText(0f);
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						_nextState = MenuState.Confirm;
						break;
					case 4:
						_confirmAction = ConfirmAction.ClearScores;
						_confirmText.Text = "Are you sure you want to clear your highscores? This cannot be undone.";
						_confirmText.FitComponentToText(0f);
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						_nextState = MenuState.Confirm;
						break;
					}
					_soundManager.CreateMenuSoundCue("menu Click").Play();
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.B))
				{
					_leaderPlayer.MusicVolume = _soundManager.MusicVolume;
					_leaderPlayer.EffectVolume = _soundManager.EffectVolume;
					_storageManager.Save(_leaderPlayer, saveCurrentSettings: true);
					_storageManager.Save(_contentArea);
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
					_nextState = MenuState.Main;
				}
				_logo.Update(gameTime);
				_menuSettings[0].Text = "Music Volume: " + _soundManager.MusicVolume * 10f;
				_menuSettings[1].Text = "Effect Volume: " + _soundManager.EffectVolume * 10f;
				for (int m = 0; m != _menuSettings.Length; m++)
				{
					_menuSettings[m].IsHighlighted = _selectedIndex == m;
					_menuSettings[m].Update(gameTime);
				}
				break;
			}
			case MenuState.Screen:
			{
				_contentArea.X += (int)Math.Round(_playerManager.GetGamePad(_playerInControl).GamePadStateCurrent.ThumbSticks.Left.X);
				_contentArea.Y -= (int)Math.Round(_playerManager.GetGamePad(_playerInControl).GamePadStateCurrent.ThumbSticks.Left.Y);
				_contentArea.Width += (int)(Math.Round(_playerManager.GetGamePad(_playerInControl).GamePadStateCurrent.ThumbSticks.Right.X) * 2.0);
				_contentArea.Height -= (int)(Math.Round(_playerManager.GetGamePad(_playerInControl).GamePadStateCurrent.ThumbSticks.Right.Y) * 2.0);
				if (_contentArea.Width < 1024)
				{
					_contentArea.Width = 1024;
				}
				else
				{
					_contentArea.X -= (int)Math.Round(_playerManager.GetGamePad(_playerInControl).GamePadStateCurrent.ThumbSticks.Right.X);
				}
				if (_contentArea.Height < 576)
				{
					_contentArea.Height = 576;
				}
				else
				{
					_contentArea.Y += (int)Math.Round(_playerManager.GetGamePad(_playerInControl).GamePadStateCurrent.ThumbSticks.Right.Y);
				}
				Vector2 position;
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.A))
				{
					_game.TitleSafeArea = _contentArea;
					MenuComponent logo = _logo;
					position = (_logo.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f));
					logo.Position = position;
					MenuComponent logoShadow = _logoShadow;
					position = (_logoShadow.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f));
					logoShadow.Position = position;
					TextComponent start = _start;
					position = (_start.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, _contentArea.Center.Y));
					start.Position = position;
					for (int num17 = 0; num17 < _menuMain.Length; num17++)
					{
						ListButton obj9 = _menuMain[num17];
						position = (_menuMain[num17].DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Center.Y + 36f * (float)num17));
						obj9.Position = position;
					}
					if (!Guide.IsTrialMode)
					{
						ListButton obj10 = _menuMain[3];
						position = (_menuMain[3].DesiredPosition = _menuMain[2].Position);
						obj10.Position = position;
						ListButton obj11 = _menuMain[4];
						position = (_menuMain[4].DesiredPosition = _menuMain[2].Position + new Vector2(0f, 36f));
						obj11.Position = position;
					}
					TextComponent obj12 = _promptsMain[0];
					position = (_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f));
					obj12.Position = position;
					TextComponent obj13 = _promptsMain[1];
					position = (_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[1].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsMain[1].Size.Y * 0.5f));
					obj13.Position = position;
					for (int num18 = 0; num18 < _menuSettings.Length; num18++)
					{
						ListButton obj14 = _menuSettings[num18];
						position = (_menuSettings[num18].DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Center.Y + 36f * (float)num18));
						obj14.Position = position;
					}
					TextComponent obj15 = _promptsSettings[0];
					position = (_promptsSettings[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsSettings[0].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsSettings[0].Size.Y * 0.5f));
					obj15.Position = position;
					TextComponent obj16 = _promptsSettings[1];
					position = (_promptsSettings[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsSettings[1].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsSettings[1].Size.Y * 0.5f));
					obj16.Position = position;
					panelSpacing = ((float)_contentArea.Width - 320f) / (float)(_menuConnect.Length - 1);
					for (int num19 = 0; num19 < _menuConnect.Length; num19++)
					{
						ConnectPanel obj17 = _menuConnect[num19];
						position = (_menuConnect[num19].DesiredPosition = new Vector2((float)_contentArea.Left + 160f + panelSpacing * (float)num19 + 1280f, (float)_contentArea.Center.Y + 100f));
						obj17.Position = position;
					}
					Vector2 vector21 = new Vector2((float)_contentArea.Center.X + 74f + 1280f, (float)_contentArea.Top + 40f);
					for (int num20 = 0; num20 != _minigameMeta.Length; num20++)
					{
						ListButton obj18 = _menuGames[num20];
						position = (_menuGames[num20].DesiredPosition = vector21 + new Vector2(0f, 17f));
						obj18.Position = position;
						StarRating obj19 = _starGames[num20];
						position = (_starGames[num20].DesiredPosition = _menuGames[num20].Position);
						obj19.Position = position;
					}
					MenuComponent gameImage = _gameImage;
					position = (_gameImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 76f + 1280f, (float)_contentArea.Top + 40f));
					gameImage.Position = position;
					TextComponent gameDescription = _gameDescription;
					position = (_gameDescription.DesiredPosition = _gameImage.Position + new Vector2(0f, _gameImage.Size.Y + 2f));
					gameDescription.Position = position;
					TextComponent gameGenre = _gameGenre;
					position = (_gameGenre.DesiredPosition = _gameDescription.Position + new Vector2(0f, _gameDescription.Size.Y + 2f));
					gameGenre.Position = position;
					MenuComponent gameCompetitionImage = _gameCompetitionImage;
					position = (_gameCompetitionImage.DesiredPosition = _gameGenre.Position + new Vector2(0f, _gameGenre.Size.Y + 2f));
					gameCompetitionImage.Position = position;
					TextComponent gamePlayerLimit = _gamePlayerLimit;
					position = (_gamePlayerLimit.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, 0f));
					gamePlayerLimit.Position = position;
					TextComponent gameCompetition = _gameCompetition;
					position = (_gameCompetition.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, _gameCompetitionImage.Size.Y));
					gameCompetition.Position = position;
					TextComponent gameHighscore = _gameHighscore;
					position = (_gameHighscore.DesiredPosition = _gameCompetitionImage.Position + new Vector2(0f, _gameCompetitionImage.Size.Y + 2f));
					gameHighscore.Position = position;
					TextComponent sortModeText = _sortModeText;
					position = (_sortModeText.DesiredPosition = new Vector2(_gameImage.Position.X + _gameImage.Size.X, (float)_contentArea.Bottom - 40f));
					sortModeText.Position = position;
					for (int num21 = 0; num21 < _promptsGames.Length; num21++)
					{
						TextComponent obj20 = _promptsGames[num21];
						position = (_promptsGames[num21].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * (0.25f * (float)num21) + _promptsGames[num21].Size.X * 0.5f + 1280f, _gameHighscore.Size.Y + 2f + _promptsGames[num21].Size.Y * 0.5f));
						obj20.Position = position;
					}
					MenuComponent instructionImage2 = _instructionImage;
					position = (_instructionImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y));
					instructionImage2.Position = position;
					TextComponent obj21 = _promptsInstruction[0];
					position = (_promptsInstruction[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsInstruction[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsInstruction[0].Size.Y * 0.5f));
					obj21.Position = position;
					TextComponent obj22 = _promptsInstruction[1];
					position = (_promptsInstruction[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsInstruction[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsInstruction[1].Size.Y * 0.5f));
					obj22.Position = position;
					MenuComponent creditsLogo = _creditsLogo;
					position = (_creditsLogo.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 160f));
					creditsLogo.Position = position;
					TextComponent obj23 = _creditsHeaders[0];
					position = (_creditsHeaders[0].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f));
					obj23.Position = position;
					TextComponent obj24 = _creditsHeaders[1];
					position = (_creditsHeaders[1].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 90f));
					obj24.Position = position;
					TextComponent obj25 = _creditsNames[0];
					position = (_creditsNames[0].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y));
					obj25.Position = position;
					TextComponent obj26 = _creditsNames[1];
					position = (_creditsNames[1].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y * 2f));
					obj26.Position = position;
					TextComponent obj27 = _creditsNames[2];
					position = (_creditsNames[2].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y));
					obj27.Position = position;
					TextComponent obj28 = _creditsNames[3];
					position = (_creditsNames[3].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 2f));
					obj28.Position = position;
					TextComponent obj29 = _creditsNames[4];
					position = (_creditsNames[4].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 3f));
					obj29.Position = position;
					TextComponent promptCredits = _promptCredits;
					position = (_promptCredits.DesiredPosition = new Vector2((float)_contentArea.Left + _promptCredits.Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptCredits.Size.Y * 0.5f));
					promptCredits.Position = position;
					for (int num22 = 0; num22 < _menuConfirm.Length; num22++)
					{
						ListButton obj30 = _menuConfirm[num22];
						position = (_menuConfirm[num22].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)num22));
						obj30.Position = position;
					}
					TextComponent confirmText = _confirmText;
					position = (_confirmText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f));
					confirmText.Position = position;
					TextComponent obj31 = _promptsConfirm[0];
					position = (_promptsConfirm[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsConfirm[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsConfirm[0].Size.Y * 0.5f));
					obj31.Position = position;
					TextComponent obj32 = _promptsConfirm[1];
					position = (_promptsConfirm[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsConfirm[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsConfirm[1].Size.Y * 0.5f));
					obj32.Position = position;
					MenuComponent pauseBackground = _pauseBackground;
					position = (_pauseBackground.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y));
					pauseBackground.Position = position;
					TextComponent pauseHeader = _pauseHeader;
					position = (_pauseHeader.DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.4f));
					pauseHeader.Position = position;
					TextComponent pauseName = _pauseName;
					position = (_pauseName.DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.3f));
					pauseName.Position = position;
					for (int num23 = 0; num23 < _menuPause.Length; num23++)
					{
						ListButton obj33 = _menuPause[num23];
						position = (_menuPause[num23].DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.02f + 36f * (float)num23));
						obj33.Position = position;
					}
					TextComponent obj34 = _promptsPause[0];
					position = (_promptsPause[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsPause[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsPause[0].Size.Y * 0.5f));
					obj34.Position = position;
					TextComponent obj35 = _promptsPause[1];
					position = (_promptsPause[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsPause[1].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsPause[1].Size.Y * 0.5f));
					obj35.Position = position;
					_soundManager.CreateMenuSoundCue("menu Click").Play();
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
					_nextState = MenuState.Settings;
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.B))
				{
					_contentArea = _game.TitleSafeArea;
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
					_nextState = MenuState.Settings;
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.Y))
				{
					_soundManager.CreateMenuSoundCue("menu Click").Play();
					_contentArea = new Rectangle(128, 72, 1024, 576);
				}
				_contentArea.Width = ((_contentArea.Width > 1280) ? 1280 : _contentArea.Width);
				_contentArea.Height = ((_contentArea.Height > 720) ? 720 : _contentArea.Height);
				_contentArea.X = ((_contentArea.Left >= 0) ? _contentArea.X : 0);
				_contentArea.X = ((_contentArea.Right > 1280) ? (1280 - _contentArea.Width) : _contentArea.X);
				_contentArea.Y = ((_contentArea.Top >= 0) ? _contentArea.Y : 0);
				_contentArea.Y = ((_contentArea.Bottom > 720) ? (720 - _contentArea.Height) : _contentArea.Y);
				MenuComponent screenCentre = _screenCentre;
				position = (_screenCentre.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y));
				screenCentre.Position = position;
				MenuComponent obj36 = _screenCorners[0];
				position = (_screenCorners[0].DesiredPosition = new Vector2((float)_contentArea.Left - 10f, (float)_contentArea.Top - 10f));
				obj36.Position = position;
				MenuComponent obj37 = _screenCorners[1];
				position = (_screenCorners[1].DesiredPosition = new Vector2((float)_contentArea.Right + 10f, (float)_contentArea.Top - 10f));
				obj37.Position = position;
				MenuComponent obj38 = _screenCorners[2];
				position = (_screenCorners[2].DesiredPosition = new Vector2((float)_contentArea.Right + 10f, (float)_contentArea.Bottom + 10f));
				obj38.Position = position;
				MenuComponent obj39 = _screenCorners[3];
				position = (_screenCorners[3].DesiredPosition = new Vector2((float)_contentArea.Left - 10f, (float)_contentArea.Bottom + 10f));
				obj39.Position = position;
				for (int num24 = 0; num24 < _promptsScreen.Length; num24++)
				{
					TextComponent obj40 = _promptsScreen[num24];
					position = (_promptsScreen[num24].DesiredPosition = new Vector2(_screenCentre.Position.X - _screenCentre.Size.X * 0.5f + _screenCentre.Size.X * (0.2f * (float)num24) + _promptsScreen[num24].Size.X * 0.5f, _screenCentre.Position.Y + _screenCentre.Size.Y * 0.5f + _promptsScreen[num24].Size.Y * 0.5f + 4f));
					obj40.Position = position;
				}
				TextComponent screenAreaText = _screenAreaText;
				position = (_screenAreaText.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + 10f));
				screenAreaText.Position = position;
				TextComponent screenBrightnessText = _screenBrightnessText;
				position = (_screenBrightnessText.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Bottom - 10f));
				screenBrightnessText.Position = position;
				break;
			}
			case MenuState.Credits:
			{
				if (_leaderPlayer.GamePadManager.ButtonWasPressed(Buttons.B))
				{
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
					_nextState = MenuState.Main;
				}
				_creditsLogo.Update(gameTime);
				for (int num32 = 0; num32 != _creditsHeaders.Length; num32++)
				{
					_creditsHeaders[num32].Update(gameTime);
				}
				for (int num33 = 0; num33 != _creditsNames.Length; num33++)
				{
					_creditsNames[num33].Update(gameTime);
				}
				_promptCredits.Update(gameTime);
				break;
			}
			case MenuState.Confirm:
			{
				if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadDown))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadDown))
					{
						_selectedIndex = _selectedIndex * -1 + 1;
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadUp))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadUp))
					{
						_selectedIndex = _selectedIndex * -1 + 1;
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					_holdTimers[(int)_playerInControl] = 0;
					_holdRepeatTime[(int)_playerInControl] = 200;
				}
				if (_confirmAction == ConfirmAction.InvalidBuy)
				{
					_selectedIndex = 0;
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.A))
				{
					switch (_selectedIndex)
					{
					case 0:
						switch (_confirmAction)
						{
						case ConfirmAction.QuitGame:
							_overlay.ColourBlendSpeed = 0.01f;
							_nextState = MenuState.Quit;
							break;
						case ConfirmAction.ClearScores:
						{
							_storageManager.Delete(_minigameMeta, ratings: false);
							for (int num4 = 0; num4 < _minigameMeta.Length; num4++)
							{
								_minigameMeta[num4].SetScore(string.Empty, 0f);
							}
							_selectedIndex = 4;
							_nextState = MenuState.Settings;
							break;
						}
						case ConfirmAction.ClearRatings:
						{
							_storageManager.Delete(_minigameMeta, ratings: true);
							for (int n = 0; n < _minigameMeta.Length; n++)
							{
								_minigameMeta[n].Rating = 0;
							}
							_selectedIndex = 3;
							_nextState = MenuState.Settings;
							break;
						}
						case ConfirmAction.ContinueWithoutSaving:
						{
							Vector2 position;
							for (int num3 = 0; num3 < _menuMain.Length; num3++)
							{
								ListButton obj = _menuMain[num3];
								position = (_menuMain[num3].DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, _menuMain[num3].Position.Y));
								obj.Position = position;
							}
							TextComponent obj2 = _promptsMain[0];
							position = (_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f - 1280f, _promptsMain[0].Position.Y));
							obj2.Position = position;
							TextComponent obj3 = _promptsMain[1];
							position = (_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[1].Size.X * 0.5f - 1280f, _promptsMain[1].Position.Y));
							obj3.Position = position;
							_nextState = MenuState.Main;
							break;
						}
						case ConfirmAction.ValidBuy:
							if (!Guide.IsVisible && Gamer.SignedInGamers[_playerInControl].Privileges.AllowPurchaseContent)
							{
								Guide.ShowMarketplace(_playerInControl);
							}
							break;
						}
						break;
					case 1:
						switch (_confirmAction)
						{
						case ConfirmAction.QuitGame:
							_nextState = MenuState.Main;
							_selectedIndex = 3;
							break;
						case ConfirmAction.ClearScores:
							_nextState = MenuState.Settings;
							_selectedIndex = 4;
							break;
						case ConfirmAction.ClearRatings:
							_nextState = MenuState.Settings;
							_selectedIndex = 3;
							break;
						case ConfirmAction.ContinueWithoutSaving:
							_storageManager.SelectStorageDevice(_leaderPlayer, ref _minigameMeta);
							_menuState = MenuState.Start;
							break;
						case ConfirmAction.ValidBuy:
							_nextState = MenuState.Games;
							_selectedIndex = _selectedGame;
							break;
						}
						break;
					}
					_soundManager.CreateMenuSoundCue("menu Click").Play();
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
				}
				else if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.B))
				{
					switch (_confirmAction)
					{
					case ConfirmAction.QuitGame:
						_nextState = MenuState.Main;
						_selectedIndex = 4;
						break;
					case ConfirmAction.ClearScores:
						_nextState = MenuState.Settings;
						_selectedIndex = 4;
						break;
					case ConfirmAction.ClearRatings:
						_nextState = MenuState.Settings;
						_selectedIndex = 3;
						break;
					case ConfirmAction.ValidBuy:
						_nextState = MenuState.Games;
						_selectedIndex = _selectedGame;
						break;
					case ConfirmAction.InvalidBuy:
						_nextState = MenuState.Games;
						_selectedIndex = _selectedGame;
						break;
					default:
						_nextState = MenuState.Start;
						break;
					}
					_soundManager.CreateMenuSoundCue("menu Whiz").Play();
				}
				if (_confirmAction == ConfirmAction.ContinueWithoutSaving && _storageManager.DeviceState == StorageManager.StorageDeviceState.Ready)
				{
					Vector2 position;
					for (int num5 = 0; num5 < _menuMain.Length; num5++)
					{
						ListButton obj4 = _menuMain[num5];
						position = (_menuMain[num5].DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, _menuMain[num5].Position.Y));
						obj4.Position = position;
					}
					TextComponent obj5 = _promptsMain[0];
					position = (_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f - 1280f, _promptsMain[0].Position.Y));
					obj5.Position = position;
					TextComponent obj6 = _promptsMain[1];
					position = (_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[1].Size.X * 0.5f - 1280f, _promptsMain[1].Position.Y));
					obj6.Position = position;
					_nextState = MenuState.Main;
				}
				if (_confirmAction == ConfirmAction.ValidBuy && !Guide.IsTrialMode && !Guide.IsVisible)
				{
					MenuComponent instructionImage = _instructionImage;
					Vector2 position = (_instructionImage.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, _instructionImage.Position.Y));
					instructionImage.Position = position;
					TextComponent obj7 = _promptsInstruction[0];
					position = (_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsInstruction[0].Size.X * 0.5f - 1280f, _promptsInstruction[0].Position.Y));
					obj7.Position = position;
					TextComponent obj8 = _promptsInstruction[1];
					position = (_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsInstruction[1].Size.X * 0.5f - 1280f, _promptsInstruction[1].Position.Y));
					obj8.Position = position;
					_selectedIndex = _selectedGame;
					_nextState = MenuState.Instruction;
				}
				if (_confirmAction == ConfirmAction.InvalidBuy && Gamer.SignedInGamers[_playerInControl] != null && Gamer.SignedInGamers[_playerInControl].Privileges.AllowPurchaseContent)
				{
					_confirmText.Text = "Game locked in demo mode. Buy full version and play?";
					_confirmAction = ConfirmAction.ValidBuy;
				}
				else if (_confirmAction == ConfirmAction.ValidBuy && (Gamer.SignedInGamers[_playerInControl] == null || !Gamer.SignedInGamers[_playerInControl].Privileges.AllowPurchaseContent))
				{
					_confirmText.Text = "Game locked in demo mode. Valid signed-in profile is required to unlock.";
					_confirmAction = ConfirmAction.InvalidBuy;
				}
				_confirmText.Update(gameTime);
				for (int num6 = 0; num6 != _menuConfirm.Length; num6++)
				{
					_menuConfirm[num6].IsHighlighted = _selectedIndex == num6;
					_menuConfirm[num6].Update(gameTime);
				}
				break;
			}
			case MenuState.Quit:
				result = -1;
				break;
			case MenuState.Pause:
			{
				if (!_playerManager.GetPlayer(_playerInControl).GamePadManager.GamePadStatePrevious.IsConnected)
				{
					_nextState = MenuState.Disconnect;
				}
				else if (_playerManager.GetPlayer(_playerInControl).GamerProblem)
				{
					_nextState = MenuState.SignOut;
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadDown))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickDown) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadDown))
					{
						_selectedIndex++;
						if (_selectedIndex == _menuPause.Length)
						{
							_selectedIndex = 0;
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else if (_playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonIsHeld(Buttons.DPadUp))
				{
					if (_holdTimers[(int)_playerInControl] > 600 || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.LeftThumbstickUp) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.DPadUp))
					{
						_selectedIndex--;
						if (_selectedIndex == -1)
						{
							_selectedIndex = _menuPause.Length - 1;
						}
						_soundManager.CreateMenuSoundCue("menu Tick").Play();
						_holdTimers[(int)_playerInControl] -= _holdRepeatTime[(int)_playerInControl];
					}
					if (_holdRepeatTime[(int)_playerInControl] > 60)
					{
						_holdRepeatTime[(int)_playerInControl]--;
						if (_holdRepeatTime[(int)_playerInControl] < 60)
						{
							_holdRepeatTime[(int)_playerInControl] = 60;
						}
					}
					_holdTimers[(int)_playerInControl] += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					_holdTimers[(int)_playerInControl] = 0;
					_holdRepeatTime[(int)_playerInControl] = 200;
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasReleased(Buttons.A))
				{
					switch (_selectedIndex)
					{
					case 0:
						result = 1;
						break;
					case 1:
						_playerManager.GetPlayer(_playerInControl).AllowsVibration = !_playerManager.GetPlayer(_playerInControl).AllowsVibration;
						if (_playerManager.GetPlayer(_playerInControl).AllowsVibration)
						{
							_playerManager.GetPlayer(_playerInControl).GamePadManager.StartVibration(300);
						}
						break;
					case 2:
						result = 2;
						break;
					}
					_soundManager.CreateMenuSoundCue("menu Click").Play();
				}
				if (_playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.Start) || _playerManager.GetGamePad(_playerInControl).ButtonWasPressed(Buttons.B))
				{
					result = 1;
				}
				_pauseName.Text = _playerManager.GetPlayer(_playerInControl).Name;
				_menuPause[1].Text = "Vibration: " + (_playerManager.GetPlayer(_playerInControl).AllowsVibration ? "ON" : "OFF");
				_pauseBackground.Update(gameTime);
				_pauseHeader.Update(gameTime);
				_pauseName.Update(gameTime);
				for (int num15 = 0; num15 != _menuPause.Length; num15++)
				{
					_menuPause[num15].IsHighlighted = _selectedIndex == num15;
					_menuPause[num15].Update(gameTime);
				}
				for (int num16 = 0; num16 < _promptsPause.Length; num16++)
				{
					_promptsPause[num16].Update(gameTime);
				}
				break;
			}
			case MenuState.Disconnect:
			{
				int num29 = 0;
				foreach (Player item in _playerManager.PlayersConnected)
				{
					if (item.GamePadManager.GamePadStateCurrent.IsConnected)
					{
						num29++;
					}
					if (item.GamePadManager.ButtonWasPressed(Buttons.Back))
					{
						_soundManager.CreateMenuSoundCue("menu Whiz").Play();
						result = 2;
					}
					if (_playerManager.GetPlayer(_playerInControl).GamePadManager.GamePadStateCurrent.IsConnected && !item.GamePadManager.GamePadStateCurrent.IsConnected)
					{
						_playerInControl = item.PlayerIndex;
					}
				}
				if (!_playerManager.GetPlayer(_playerInControl).GamePadManager.GamePadStatePrevious.IsConnected && _playerManager.GetPlayer(_playerInControl).GamePadManager.GamePadStateCurrent.IsConnected)
				{
					_nextState = MenuState.Pause;
				}
				if (num29 != 0)
				{
					if (num29 == _playerManager.NumberOfPlayers - 1)
					{
						_pauseName.Text = _playerManager.GetPlayer(_playerInControl).Name + " has disconnected.\nPlease reconnect the controller\nor press back to quit.";
					}
					else
					{
						_pauseName.Text = "Players have disconnected.\nPlease reconnect the controller\nor press back to quit.";
					}
				}
				else if (_playerManager.NumberOfPlayers == 1)
				{
					_pauseName.Text = "The controller has been disconnected.\nPlease reconnect the controller.";
				}
				else
				{
					_pauseName.Text = "All players have disconnected.\nPlease reconnect the controllers.";
				}
				float x = _pauseName.Font.MeasureString(_pauseName.Text).X;
				if (x > 520f)
				{
					_pauseBackground.DesiredSize = new Vector2(x + 20f, _pauseBackground.DesiredSize.Y);
				}
				else
				{
					_pauseBackground.DesiredSize = new Vector2(510f, _pauseBackground.DesiredSize.Y);
				}
				_pauseBackground.Update(gameTime);
				_pauseHeader.Update(gameTime);
				_pauseName.Update(gameTime);
				for (int num30 = 0; num30 != _menuPause.Length; num30++)
				{
					_menuPause[num30].IsHighlighted = _selectedIndex == num30;
					_menuPause[num30].Update(gameTime);
				}
				for (int num31 = 0; num31 < _promptsPause.Length; num31++)
				{
					_promptsPause[num31].Update(gameTime);
				}
				break;
			}
			case MenuState.SignOut:
			{
				if (!_playerManager.GetPlayer(_playerInControl).GamePadManager.GamePadStatePrevious.IsConnected)
				{
					_nextState = MenuState.Disconnect;
				}
				else if (!_playerManager.GetPlayer(_playerInControl).GamerProblem)
				{
					_nextState = MenuState.Pause;
				}
				if (_playerManager.GetPlayer(_playerInControl).GamePadManager.ButtonWasPressed(Buttons.Start))
				{
					Player player = _playerManager.GetPlayer(_playerInControl);
					player.Name = "Player " + (int)(_playerInControl + 1);
					player.GamerProblem = false;
					_soundManager.CreateMenuSoundCue("menu Click").Play();
				}
				_pauseName.Text = _playerManager.GetPlayer(_playerInControl).Name + " has signed out.\nPlease sign back in or press start to\ncontinue playing as Player " + (int)(_playerInControl + 1);
				_pauseBackground.Update(gameTime);
				_pauseHeader.Update(gameTime);
				_pauseName.Update(gameTime);
				for (int k = 0; k != _menuPause.Length; k++)
				{
					_menuPause[k].IsHighlighted = _selectedIndex == k;
					_menuPause[k].Update(gameTime);
				}
				for (int l = 0; l < _promptsPause.Length; l++)
				{
					_promptsPause[l].Update(gameTime);
				}
				break;
			}
			}
		}
		return result;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
		if (_menuState != MenuState.Pause && _menuState != MenuState.Disconnect && _menuState != MenuState.SignOut)
		{
			_overlay.Draw(spriteBatch);
			DrawState(_menuState, spriteBatch);
			if (_menuState != _nextState)
			{
				DrawState(_nextState, spriteBatch);
			}
		}
		else
		{
			DrawPause(spriteBatch);
		}
		spriteBatch.End();
	}

	private void DrawState(MenuState state, SpriteBatch spriteBatch)
	{
		switch (state)
		{
		case MenuState.Start:
			_logo.Draw(spriteBatch);
			_logoShadow.Draw(spriteBatch);
			_start.Draw(spriteBatch);
			break;
		case MenuState.Main:
		{
			_logo.Draw(spriteBatch);
			for (int num5 = 0; num5 != _menuMain.Length; num5++)
			{
				if (num5 != 2 || Guide.IsTrialMode)
				{
					_menuMain[num5].Draw(spriteBatch);
				}
			}
			TextComponent[] promptsMain2 = _promptsMain;
			foreach (TextComponent textComponent7 in promptsMain2)
			{
				textComponent7.Draw(spriteBatch);
			}
			break;
		}
		case MenuState.Connect:
		{
			_logo.Draw(spriteBatch);
			ConnectPanel[] menuConnect = _menuConnect;
			foreach (ConnectPanel connectPanel in menuConnect)
			{
				connectPanel.Draw(spriteBatch);
			}
			break;
		}
		case MenuState.Games:
		{
			ListButton[] menuGames = _menuGames;
			foreach (ListButton listButton2 in menuGames)
			{
				listButton2.Draw(spriteBatch);
			}
			StarRating[] starGames = _starGames;
			foreach (StarRating starRating in starGames)
			{
				starRating.Draw(spriteBatch);
			}
			if (Guide.IsTrialMode && _demoLock[_selectedIndex])
			{
				_gameLockImage.Draw(spriteBatch);
			}
			_gameImage.Draw(spriteBatch);
			_gameDescription.Draw(spriteBatch);
			_gameHighscore.Draw(spriteBatch);
			_gameCompetitionImage.Draw(spriteBatch);
			_gamePlayerLimit.Draw(spriteBatch);
			_gameCompetition.Draw(spriteBatch);
			_gameGenre.Draw(spriteBatch);
			_sortModeText.Draw(spriteBatch);
			TextComponent[] promptsGames = _promptsGames;
			foreach (TextComponent textComponent3 in promptsGames)
			{
				textComponent3.Draw(spriteBatch);
			}
			break;
		}
		case MenuState.Settings:
		{
			_logo.Draw(spriteBatch);
			ListButton[] menuSettings = _menuSettings;
			foreach (ListButton listButton3 in menuSettings)
			{
				listButton3.Draw(spriteBatch);
			}
			TextComponent[] promptsSettings = _promptsSettings;
			foreach (TextComponent textComponent6 in promptsSettings)
			{
				textComponent6.Draw(spriteBatch);
			}
			break;
		}
		case MenuState.Screen:
		{
			_logo.Draw(spriteBatch);
			_screenCentre.Draw(spriteBatch);
			_screenAreaText.Draw(spriteBatch);
			_screenBrightnessText.Draw(spriteBatch);
			MenuComponent[] screenCorners = _screenCorners;
			foreach (MenuComponent menuComponent in screenCorners)
			{
				menuComponent.Draw(spriteBatch);
			}
			TextComponent[] promptsScreen = _promptsScreen;
			foreach (TextComponent textComponent8 in promptsScreen)
			{
				textComponent8.Draw(spriteBatch);
			}
			break;
		}
		case MenuState.Instruction:
		{
			_instructionImage.Draw(spriteBatch);
			TextComponent[] promptsMain = _promptsMain;
			foreach (TextComponent textComponent in promptsMain)
			{
				textComponent.Draw(spriteBatch);
			}
			if (_nextState != MenuState.Loading)
			{
				TextComponent[] promptsInstruction = _promptsInstruction;
				foreach (TextComponent textComponent2 in promptsInstruction)
				{
					textComponent2.Draw(spriteBatch);
				}
			}
			break;
		}
		case MenuState.Loading:
			_instructionImage.Draw(spriteBatch);
			_loading.Draw(spriteBatch);
			break;
		case MenuState.Credits:
		{
			_creditsLogo.Draw(spriteBatch);
			TextComponent[] creditsHeaders = _creditsHeaders;
			foreach (TextComponent textComponent4 in creditsHeaders)
			{
				textComponent4.Draw(spriteBatch);
			}
			TextComponent[] creditsNames = _creditsNames;
			foreach (TextComponent textComponent5 in creditsNames)
			{
				textComponent5.Draw(spriteBatch);
			}
			_promptCredits.Draw(spriteBatch);
			break;
		}
		case MenuState.Confirm:
			_confirmText.Draw(spriteBatch);
			if (_confirmAction != ConfirmAction.InvalidBuy)
			{
				ListButton[] menuConfirm = _menuConfirm;
				foreach (ListButton listButton in menuConfirm)
				{
					listButton.Draw(spriteBatch);
				}
				_promptsConfirm[0].Draw(spriteBatch);
			}
			_promptsConfirm[1].Draw(spriteBatch);
			break;
		case MenuState.Quit:
			break;
		}
	}

	private void DrawPause(SpriteBatch spriteBatch)
	{
		_pauseBackground.Draw(spriteBatch);
		_pauseHeader.Draw(spriteBatch);
		_pauseName.Draw(spriteBatch);
		for (int i = 0; i < _menuPause.Length; i++)
		{
			_menuPause[i].Draw(spriteBatch);
		}
		for (int j = 0; j < _promptsPause.Length; j++)
		{
			_promptsPause[j].Draw(spriteBatch);
		}
	}

	private void ChangeState(MenuState nextState, GameTime gameTime)
	{
		switch (_menuState)
		{
		case MenuState.Start:
		{
			_overlay.DesiredColour = Color.Black * 0.8f;
			_overlay.Update(gameTime);
			MenuComponent logoShadow = _logoShadow;
			Vector2 desiredPosition2 = (_logoShadow.Position = _logo.Position);
			logoShadow.DesiredPosition = desiredPosition2;
			_logoShadow.DesiredColour = Color.White * 0f;
			_logoShadow.Update(gameTime);
			_start.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, _contentArea.Center.Y);
			_start.Update(gameTime);
			if (_storageManager.DeviceState == StorageManager.StorageDeviceState.NoDevice)
			{
				_confirmText.Text = "Do you wish to continue without saving?";
				_confirmAction = ConfirmAction.ContinueWithoutSaving;
				_nextState = MenuState.Confirm;
			}
			_selectedIndex = 0;
			break;
		}
		case MenuState.Main:
			if (_nextState == MenuState.Start)
			{
				for (int num2 = 0; num2 != _menuMain.Length; num2++)
				{
					_menuMain[num2].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)num2);
					_menuMain[num2].Update(gameTime);
				}
				if (!Guide.IsTrialMode)
				{
					_menuMain[3].Position = _menuMain[2].Position;
					_menuMain[4].Position = _menuMain[2].Position + new Vector2(0f, 36f);
				}
				_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsMain[0].Update(gameTime);
				_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsMain[1].Update(gameTime);
			}
			else
			{
				for (int num3 = 0; num3 != _menuMain.Length; num3++)
				{
					_menuMain[num3].DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Center.Y + 36f * (float)num3);
					_menuMain[num3].Update(gameTime);
				}
				if (!Guide.IsTrialMode)
				{
					_menuMain[3].Position = _menuMain[2].Position;
					_menuMain[4].Position = _menuMain[2].Position + new Vector2(0f, 36f);
				}
				_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsMain[0].Update(gameTime);
				_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsMain[1].Update(gameTime);
			}
			if (_nextState == MenuState.Confirm || _nextState == MenuState.Credits)
			{
				_logo.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f);
				_logo.Update(gameTime);
				_selectedIndex = 0;
			}
			_selectedIndex = 0;
			break;
		case MenuState.Connect:
			panelSpacing = ((float)_contentArea.Width - 320f) / (float)(_menuConnect.Length - 1);
			if (_nextState == MenuState.Main)
			{
				for (int k = 0; k != _menuConnect.Length; k++)
				{
					_menuConnect[k].DesiredPosition = new Vector2(_menuConnect[0].Size.X * 0.5f + (float)_contentArea.Left + 160f + panelSpacing * (float)k + 1280f, (float)_contentArea.Center.Y + 100f);
					_menuConnect[k].Update(gameTime, _playerManager, _storageManager, _soundManager);
				}
				_selectedIndex = 0;
			}
			else if (_nextState == MenuState.Games)
			{
				if (_playerManager.GetPlayer(_leader) == null)
				{
					_playerInControl = _playerManager.PlayersConnected[0].PlayerIndex;
				}
				_logo.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f);
				_logo.Update(gameTime);
				for (int l = 0; l != _menuConnect.Length; l++)
				{
					_menuConnect[l].DesiredPosition = new Vector2((float)_contentArea.Left + 160f + panelSpacing * (float)l - 1280f, (float)_contentArea.Center.Y + 100f);
					_menuConnect[l].Update(gameTime, _playerManager, _storageManager, _soundManager);
				}
				_selectedIndex = 0;
			}
			break;
		case MenuState.Games:
		{
			Vector2 vector = new Vector2((float)_contentArea.Center.X + 74f, (float)_contentArea.Top + 40f);
			Vector2 desiredPosition = new Vector2((float)_contentArea.Center.X + 76f, (float)_contentArea.Top + 40f);
			if (_nextState == MenuState.Connect)
			{
				vector.X += 1280f;
				desiredPosition.X += 1280f;
				_sortModeText.DesiredPosition = new Vector2((float)_contentArea.Right - 1280f, _contentArea.Bottom);
			}
			else if (_nextState == MenuState.Instruction || _nextState == MenuState.Confirm)
			{
				vector.X -= 1280f;
				desiredPosition.X -= 1280f;
				_sortModeText.DesiredPosition = new Vector2((float)_contentArea.Right + 1280f, _contentArea.Bottom);
			}
			_sortModeText.Update(gameTime);
			for (int j = 0; j != _menuGames.Length; j++)
			{
				_menuGames[j].DesiredPosition = new Vector2(vector.X, _menuGames[j].Position.Y);
				_menuGames[j].Update(gameTime);
				StarRating obj = _starGames[j];
				Vector2 position = (_starGames[j].DesiredPosition = _menuGames[j].Position);
				obj.Position = position;
			}
			_gameImage.DesiredPosition = desiredPosition;
			_gameImage.Update(gameTime);
			_gameLockImage.DesiredPosition = desiredPosition;
			_gameLockImage.Update(gameTime);
			_gameDescription.DesiredPosition = _gameImage.DesiredPosition + new Vector2(0f, _gameImage.Size.Y + 2f);
			_gameDescription.Update(gameTime);
			_gameGenre.DesiredPosition = _gameDescription.DesiredPosition + new Vector2(0f, _gameDescription.Size.Y + 2f);
			_gameGenre.Update(gameTime);
			_gameCompetitionImage.DesiredPosition = _gameGenre.DesiredPosition + new Vector2(0f, _gameGenre.Size.Y + 2f);
			_gameCompetitionImage.Update(gameTime);
			_gamePlayerLimit.DesiredPosition = _gameCompetitionImage.DesiredPosition + new Vector2(_gameCompetitionImage.Size.X, 0f);
			_gamePlayerLimit.Update(gameTime);
			_gameCompetition.DesiredPosition = _gameCompetitionImage.DesiredPosition + new Vector2(_gameCompetitionImage.Size.X, _gameCompetitionImage.Size.Y);
			_gameCompetition.Update(gameTime);
			_gameHighscore.DesiredPosition = _gameCompetitionImage.DesiredPosition + new Vector2(0f, _gameCompetitionImage.Size.Y + 2f);
			_gameHighscore.Update(gameTime);
			_sortModeText.DesiredPosition = new Vector2(_gameImage.Position.X + _gameImage.Size.X, (float)_contentArea.Bottom - 40f);
			_sortModeText.Update(gameTime);
			TextComponent obj2 = _promptsGames[0];
			Vector2 position3 = (_promptsGames[0].DesiredPosition = _gameHighscore.Position + new Vector2(_promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
			obj2.Position = position3;
			_promptsGames[0].Update(gameTime);
			TextComponent obj3 = _promptsGames[1];
			Vector2 position4 = (_promptsGames[1].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * 0.25f + _promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
			obj3.Position = position4;
			_promptsGames[1].Update(gameTime);
			TextComponent obj4 = _promptsGames[2];
			Vector2 position5 = (_promptsGames[2].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * 0.5f + _promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
			obj4.Position = position5;
			_promptsGames[2].Update(gameTime);
			TextComponent obj5 = _promptsGames[3];
			Vector2 position6 = (_promptsGames[3].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * 0.75f + _promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
			obj5.Position = position6;
			_promptsGames[3].Update(gameTime);
			break;
		}
		case MenuState.Settings:
			if (_nextState == MenuState.Main)
			{
				for (int m = 0; m != _menuSettings.Length; m++)
				{
					_menuSettings[m].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)m);
					_menuSettings[m].Update(gameTime);
				}
				_promptsSettings[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsSettings[0].Update(gameTime);
				_promptsSettings[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsSettings[1].Update(gameTime);
				_selectedIndex = 1;
			}
			else if (_nextState == MenuState.Screen || _nextState == MenuState.Confirm)
			{
				_logo.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f);
				_logo.Update(gameTime);
				for (int n = 0; n != _menuSettings.Length; n++)
				{
					_menuSettings[n].DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, (float)_contentArea.Center.Y + 36f * (float)n);
					_menuSettings[n].Update(gameTime);
				}
				_promptsSettings[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsSettings[0].Update(gameTime);
				_promptsSettings[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f - 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsSettings[1].Update(gameTime);
				_selectedIndex = 1;
			}
			break;
		case MenuState.Screen:
		{
			_screenCentre.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y);
			_screenCorners[0].DesiredPosition = new Vector2((float)_contentArea.Left - 10f + 1280f, (float)_contentArea.Top - 10f);
			_screenCorners[1].DesiredPosition = new Vector2((float)_contentArea.Right + 10f + 1280f, (float)_contentArea.Top - 10f);
			_screenCorners[2].DesiredPosition = new Vector2((float)_contentArea.Right + 10f + 1280f, (float)_contentArea.Bottom + 10f);
			_screenCorners[3].DesiredPosition = new Vector2((float)_contentArea.Left - 10f + 1280f, (float)_contentArea.Bottom + 10f);
			_screenCentre.Update(gameTime);
			_screenCorners[0].Update(gameTime);
			_screenCorners[1].Update(gameTime);
			_screenCorners[2].Update(gameTime);
			_screenCorners[3].Update(gameTime);
			_screenAreaText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Top + 10f);
			_screenBrightnessText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Bottom - 10f);
			_screenAreaText.Update(gameTime);
			_screenBrightnessText.Update(gameTime);
			for (int num6 = 0; num6 < _promptsScreen.Length; num6++)
			{
				TextComponent obj8 = _promptsScreen[num6];
				Vector2 position8 = (_promptsScreen[num6].DesiredPosition = new Vector2(_screenCentre.Position.X - _screenCentre.Size.X * 0.5f + _screenCentre.Size.X * (0.2f * (float)num6) + _promptsScreen[num6].Size.X * 0.5f, (float)_contentArea.Center.Y + _screenCentre.Size.Y * 0.5f + _promptsScreen[num6].Size.Y * 0.5f + 4f));
				obj8.Position = position8;
			}
			_selectedIndex = 2;
			break;
		}
		case MenuState.Instruction:
			if (_nextState == MenuState.Games)
			{
				_instructionImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y);
				_instructionImage.Update(gameTime);
			}
			_promptsInstruction[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsInstruction[0].Update(gameTime);
			_promptsInstruction[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsInstruction[1].Update(gameTime);
			break;
		case MenuState.Credits:
		{
			_creditsLogo.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 160f);
			_creditsLogo.Update(gameTime);
			_creditsHeaders[0].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f);
			_creditsHeaders[1].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 90f);
			for (int num4 = 0; num4 != _creditsHeaders.Length; num4++)
			{
				_creditsHeaders[num4].Update(gameTime);
			}
			_creditsNames[0].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y);
			_creditsNames[1].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y * 2f);
			_creditsNames[2].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y);
			_creditsNames[3].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 2f);
			_creditsNames[4].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 3f);
			for (int num5 = 0; num5 != _creditsNames.Length; num5++)
			{
				_creditsNames[num5].Update(gameTime);
			}
			_promptCredits.DesiredPosition = new Vector2((float)_contentArea.Left + _promptCredits.Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptCredits.Size.Y * 0.5f);
			_promptCredits.Update(gameTime);
			_selectedIndex = 3;
			break;
		}
		case MenuState.Confirm:
		{
			_confirmText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f);
			_confirmText.Update(gameTime);
			for (int num = 0; num != _menuConfirm.Length; num++)
			{
				_menuConfirm[num].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)num);
				ListButton obj6 = _menuConfirm[num];
				Vector2 size = (_menuMain[num].DesiredSize = new Vector2(200f, 34f));
				obj6.Size = size;
				ListButton obj7 = _menuConfirm[num];
				Color colour = (_menuMain[num].DesiredColour = new Color(102, 102, 255) * 0.8f);
				obj7.Colour = colour;
				_menuConfirm[num].IsHighlighted = false;
				_menuConfirm[num].Update(gameTime);
			}
			_promptsConfirm[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsConfirm[0].Update(gameTime);
			_promptsConfirm[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsConfirm[1].Update(gameTime);
			break;
		}
		case MenuState.Pause:
		{
			if (_nextState != MenuState.Pause)
			{
				_pauseBackground.ScaleSpeed = 15f;
			}
			_pauseName.DesiredTextColour = _pauseName.TextColour * 0f;
			_pauseName.ColourBlendSpeed = 0.1f;
			for (int i = 0; i < _menuPause.Length; i++)
			{
				_menuPause[i].DesiredColour = _menuPause[i].Colour * 0f;
				_menuPause[i].DesiredTextColour = _menuPause[i].TextColour * 0f;
				_menuPause[i].Enabled = false;
				_menuPause[i].Update(gameTime);
			}
			_promptsPause[0].DesiredColour = Color.White * 0f;
			_promptsPause[0].DesiredTextColour = Color.White * 0f;
			_promptsPause[0].DesiredOutlineColour = Color.Black * 0f;
			_promptsPause[0].Update(gameTime);
			_promptsPause[1].DesiredColour = Color.White * 0f;
			_promptsPause[1].DesiredTextColour = Color.White * 0f;
			_promptsPause[1].DesiredOutlineColour = Color.Black * 0f;
			_promptsPause[1].Update(gameTime);
			_selectedIndex = 2;
			break;
		}
		case MenuState.Disconnect:
			_pauseBackground.ScaleSpeed = 15f;
			_pauseName.DesiredTextColour = _pauseName.TextColour * 0f;
			_pauseName.ColourBlendSpeed = 0.1f;
			_promptsPause[0].DesiredColour = Color.White * 0f;
			_promptsPause[0].DesiredTextColour = Color.White * 0f;
			_promptsPause[0].DesiredOutlineColour = Color.Black * 0f;
			_promptsPause[0].Update(gameTime);
			_promptsPause[1].DesiredColour = Color.White * 0f;
			_promptsPause[1].DesiredTextColour = Color.White * 0f;
			_promptsPause[1].DesiredOutlineColour = Color.Black * 0f;
			_promptsPause[1].Update(gameTime);
			break;
		case MenuState.SignOut:
			_pauseBackground.ScaleSpeed = 15f;
			_pauseName.DesiredTextColour = _pauseName.TextColour * 0f;
			_pauseName.ColourBlendSpeed = 0.1f;
			_promptsPause[0].DesiredColour = Color.White * 0f;
			_promptsPause[0].DesiredTextColour = Color.White * 0f;
			_promptsPause[0].DesiredOutlineColour = Color.Black * 0f;
			_promptsPause[0].Update(gameTime);
			_promptsPause[1].DesiredColour = Color.White * 0f;
			_promptsPause[1].DesiredTextColour = Color.White * 0f;
			_promptsPause[1].DesiredOutlineColour = Color.Black * 0f;
			_promptsPause[1].Update(gameTime);
			break;
		}
		switch (nextState)
		{
		case MenuState.Start:
		{
			_overlay.DesiredColour = Color.Black * 0f;
			_overlay.Update(gameTime);
			_logo.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f);
			_logo.Update(gameTime);
			MenuComponent logoShadow2 = _logoShadow;
			Vector2 desiredPosition4 = (_logoShadow.Position = _logo.Position);
			logoShadow2.DesiredPosition = desiredPosition4;
			_logoShadow.DesiredColour = Color.White;
			_logoShadow.Update(gameTime);
			_start.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y);
			_start.Update(gameTime);
			if (_start.Position.X == _start.DesiredPosition.X)
			{
				_leaderPlayer = null;
				_menuState = _nextState;
			}
			break;
		}
		case MenuState.Main:
		{
			_logo.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f);
			_logo.Update(gameTime);
			if (_storageManager.DeviceState == StorageManager.StorageDeviceState.Working || _storageManager.DeviceState == StorageManager.StorageDeviceState.Selecting)
			{
				break;
			}
			for (int num16 = 0; num16 != _menuMain.Length; num16++)
			{
				_menuMain[num16].DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y + 36f * (float)num16);
				ListButton obj23 = _menuMain[num16];
				Vector2 size2 = (_menuMain[num16].DesiredSize = new Vector2(200f, 34f));
				obj23.Size = size2;
				ListButton obj24 = _menuMain[num16];
				Color colour2 = (_menuMain[num16].DesiredColour = new Color(102, 102, 255) * 0.8f);
				obj24.Colour = colour2;
				_menuMain[num16].IsHighlighted = false;
				_menuMain[num16].Update(gameTime);
			}
			if (!Guide.IsTrialMode)
			{
				_menuMain[3].Position = _menuMain[2].Position;
				_menuMain[4].Position = _menuMain[2].Position + new Vector2(0f, 36f);
			}
			_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsMain[0].Update(gameTime);
			_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsMain[1].Update(gameTime);
			if (_menuMain[0].Position.X != _menuMain[0].DesiredPosition.X)
			{
				break;
			}
			_game.TitleSafeArea = (_contentArea = _storageManager.SavedTitleSafe);
			if (_menuState == MenuState.Start)
			{
				MenuComponent logo = _logo;
				Vector2 position11 = (_logo.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f));
				logo.Position = position11;
				MenuComponent logoShadow3 = _logoShadow;
				Vector2 position12 = (_logoShadow.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f));
				logoShadow3.Position = position12;
				TextComponent start = _start;
				Vector2 position13 = (_start.DesiredPosition = new Vector2((float)_contentArea.Center.X - 1280f, _contentArea.Center.Y));
				start.Position = position13;
				for (int num17 = 0; num17 < _menuMain.Length; num17++)
				{
					ListButton obj25 = _menuMain[num17];
					Vector2 position14 = (_menuMain[num17].DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y + 36f * (float)num17));
					obj25.Position = position14;
				}
				if (!Guide.IsTrialMode)
				{
					ListButton obj26 = _menuMain[3];
					Vector2 position15 = (_menuMain[3].DesiredPosition = _menuMain[2].Position);
					obj26.Position = position15;
					ListButton obj27 = _menuMain[4];
					Vector2 position17 = (_menuMain[4].DesiredPosition = _menuMain[2].Position + new Vector2(0f, 36f));
					obj27.Position = position17;
				}
				TextComponent obj28 = _promptsMain[0];
				Vector2 position18 = (_promptsMain[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f));
				obj28.Position = position18;
				TextComponent obj29 = _promptsMain[1];
				Vector2 position19 = (_promptsMain[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[1].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[1].Size.Y * 0.5f));
				obj29.Position = position19;
				for (int num18 = 0; num18 < _menuSettings.Length; num18++)
				{
					ListButton obj30 = _menuSettings[num18];
					Vector2 position20 = (_menuSettings[num18].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)num18));
					obj30.Position = position20;
				}
				TextComponent obj31 = _promptsSettings[0];
				Vector2 desiredPosition2 = (_promptsSettings[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsSettings[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsSettings[0].Size.Y * 0.5f));
				obj31.Position = desiredPosition2;
				TextComponent obj32 = _promptsSettings[1];
				desiredPosition2 = (_promptsSettings[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsSettings[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsSettings[1].Size.Y * 0.5f));
				obj32.Position = desiredPosition2;
				MenuComponent screenCentre = _screenCentre;
				desiredPosition2 = (_screenCentre.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y));
				screenCentre.Position = desiredPosition2;
				MenuComponent obj33 = _screenCorners[0];
				desiredPosition2 = (_screenCorners[0].DesiredPosition = new Vector2((float)_contentArea.Left - 10f + 1280f, (float)_contentArea.Top - 10f));
				obj33.Position = desiredPosition2;
				MenuComponent obj34 = _screenCorners[1];
				desiredPosition2 = (_screenCorners[1].DesiredPosition = new Vector2((float)_contentArea.Right + 10f + 1280f, (float)_contentArea.Top - 10f));
				obj34.Position = desiredPosition2;
				MenuComponent obj35 = _screenCorners[2];
				desiredPosition2 = (_screenCorners[2].DesiredPosition = new Vector2((float)_contentArea.Right + 10f + 1280f, (float)_contentArea.Bottom + 10f));
				obj35.Position = desiredPosition2;
				MenuComponent obj36 = _screenCorners[3];
				desiredPosition2 = (_screenCorners[3].DesiredPosition = new Vector2((float)_contentArea.Left - 10f + 1280f, (float)_contentArea.Bottom + 10f));
				obj36.Position = desiredPosition2;
				TextComponent screenAreaText = _screenAreaText;
				desiredPosition2 = (_screenAreaText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Top + 10f));
				screenAreaText.Position = desiredPosition2;
				TextComponent screenBrightnessText = _screenBrightnessText;
				desiredPosition2 = (_screenBrightnessText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Bottom - 10f));
				screenBrightnessText.Position = desiredPosition2;
				for (int num19 = 0; num19 < _promptsScreen.Length; num19++)
				{
					TextComponent obj37 = _promptsScreen[num19];
					desiredPosition2 = (_promptsScreen[num19].DesiredPosition = new Vector2(_screenCentre.Position.X - _screenCentre.Size.X * 0.5f + _screenCentre.Size.X * (0.2f * (float)num19) + _promptsScreen[num19].Size.X * 0.5f, _screenCentre.Position.Y + _screenCentre.Size.Y * 0.5f + _promptsScreen[num19].Size.Y * 0.5f + 4f));
					obj37.Position = desiredPosition2;
				}
				panelSpacing = ((float)_contentArea.Width - 320f) / (float)(_menuConnect.Length - 1);
				for (int num20 = 0; num20 < _menuConnect.Length; num20++)
				{
					ConnectPanel obj38 = _menuConnect[num20];
					desiredPosition2 = (_menuConnect[num20].DesiredPosition = new Vector2((float)_contentArea.Left + 160f + panelSpacing * (float)num20 + 1280f, (float)_contentArea.Center.Y + 100f));
					obj38.Position = desiredPosition2;
				}
				Vector2 vector38 = new Vector2((float)_contentArea.Center.X + 74f + 1280f, (float)_contentArea.Top + 40f);
				for (int num21 = 0; num21 != _minigameMeta.Length; num21++)
				{
					ListButton obj39 = _menuGames[num21];
					desiredPosition2 = (_menuGames[num21].DesiredPosition = vector38 + new Vector2(0f, 17f));
					obj39.Position = desiredPosition2;
					StarRating obj40 = _starGames[num21];
					desiredPosition2 = (_starGames[num21].DesiredPosition = _menuGames[num21].Position);
					obj40.Position = desiredPosition2;
				}
				MenuComponent gameImage = _gameImage;
				desiredPosition2 = (_gameImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 76f + 1280f, (float)_contentArea.Top + 40f));
				gameImage.Position = desiredPosition2;
				TextComponent gameDescription = _gameDescription;
				desiredPosition2 = (_gameDescription.DesiredPosition = _gameImage.Position + new Vector2(0f, _gameImage.Size.Y + 2f));
				gameDescription.Position = desiredPosition2;
				TextComponent gameGenre = _gameGenre;
				desiredPosition2 = (_gameGenre.DesiredPosition = _gameDescription.Position + new Vector2(0f, _gameDescription.Size.Y + 2f));
				gameGenre.Position = desiredPosition2;
				MenuComponent gameCompetitionImage = _gameCompetitionImage;
				desiredPosition2 = (_gameCompetitionImage.DesiredPosition = _gameGenre.Position + new Vector2(0f, _gameGenre.Size.Y + 2f));
				gameCompetitionImage.Position = desiredPosition2;
				TextComponent gamePlayerLimit = _gamePlayerLimit;
				desiredPosition2 = (_gamePlayerLimit.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, 0f));
				gamePlayerLimit.Position = desiredPosition2;
				TextComponent gameCompetition = _gameCompetition;
				desiredPosition2 = (_gameCompetition.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, _gameCompetitionImage.Size.Y));
				gameCompetition.Position = desiredPosition2;
				TextComponent gameHighscore = _gameHighscore;
				desiredPosition2 = (_gameHighscore.DesiredPosition = _gameCompetitionImage.Position + new Vector2(0f, _gameCompetitionImage.Size.Y + 2f));
				gameHighscore.Position = desiredPosition2;
				TextComponent sortModeText = _sortModeText;
				desiredPosition2 = (_sortModeText.DesiredPosition = new Vector2(_gameImage.Position.X + _gameImage.Size.X, (float)_contentArea.Bottom - 40f));
				sortModeText.Position = desiredPosition2;
				for (int num22 = 0; num22 < _promptsGames.Length; num22++)
				{
					TextComponent obj41 = _promptsGames[num22];
					desiredPosition2 = (_promptsGames[num22].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * (0.25f * (float)num22) + _promptsGames[num22].Size.X * 0.5f + 1280f, _gameHighscore.Size.Y + 2f + _promptsGames[num22].Size.Y * 0.5f));
					obj41.Position = desiredPosition2;
				}
				MenuComponent instructionImage = _instructionImage;
				desiredPosition2 = (_instructionImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _contentArea.Center.Y));
				instructionImage.Position = desiredPosition2;
				TextComponent obj42 = _promptsInstruction[0];
				desiredPosition2 = (_promptsInstruction[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsInstruction[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsInstruction[0].Size.Y * 0.5f));
				obj42.Position = desiredPosition2;
				TextComponent obj43 = _promptsInstruction[1];
				desiredPosition2 = (_promptsInstruction[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsInstruction[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsInstruction[1].Size.Y * 0.5f));
				obj43.Position = desiredPosition2;
				MenuComponent creditsLogo = _creditsLogo;
				desiredPosition2 = (_creditsLogo.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 160f));
				creditsLogo.Position = desiredPosition2;
				TextComponent obj44 = _creditsHeaders[0];
				desiredPosition2 = (_creditsHeaders[0].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f));
				obj44.Position = desiredPosition2;
				TextComponent obj45 = _creditsHeaders[1];
				desiredPosition2 = (_creditsHeaders[1].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 90f));
				obj45.Position = desiredPosition2;
				TextComponent obj46 = _creditsNames[0];
				desiredPosition2 = (_creditsNames[0].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y));
				obj46.Position = desiredPosition2;
				TextComponent obj47 = _creditsNames[1];
				desiredPosition2 = (_creditsNames[1].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y * 2f));
				obj47.Position = desiredPosition2;
				TextComponent obj48 = _creditsNames[2];
				desiredPosition2 = (_creditsNames[2].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y));
				obj48.Position = desiredPosition2;
				TextComponent obj49 = _creditsNames[3];
				desiredPosition2 = (_creditsNames[3].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 2f));
				obj49.Position = desiredPosition2;
				TextComponent obj50 = _creditsNames[4];
				desiredPosition2 = (_creditsNames[4].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 3f));
				obj50.Position = desiredPosition2;
				TextComponent promptCredits = _promptCredits;
				desiredPosition2 = (_promptCredits.DesiredPosition = new Vector2((float)_contentArea.Left + _promptCredits.Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptCredits.Size.Y * 0.5f));
				promptCredits.Position = desiredPosition2;
				for (int num23 = 0; num23 < _menuConfirm.Length; num23++)
				{
					ListButton obj51 = _menuConfirm[num23];
					desiredPosition2 = (_menuConfirm[num23].DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y + 36f * (float)num23));
					obj51.Position = desiredPosition2;
				}
				TextComponent confirmText = _confirmText;
				desiredPosition2 = (_confirmText.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, (float)_contentArea.Center.Y - 40f));
				confirmText.Position = desiredPosition2;
				TextComponent obj52 = _promptsConfirm[0];
				desiredPosition2 = (_promptsConfirm[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsConfirm[0].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsConfirm[0].Size.Y * 0.5f));
				obj52.Position = desiredPosition2;
				TextComponent obj53 = _promptsConfirm[1];
				desiredPosition2 = (_promptsConfirm[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsConfirm[1].Size.X * 0.5f + 1280f, (float)_contentArea.Bottom - _promptsConfirm[1].Size.Y * 0.5f));
				obj53.Position = desiredPosition2;
				MenuComponent pauseBackground = _pauseBackground;
				desiredPosition2 = (_pauseBackground.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y));
				pauseBackground.Position = desiredPosition2;
				TextComponent pauseHeader = _pauseHeader;
				desiredPosition2 = (_pauseHeader.DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.4f));
				pauseHeader.Position = desiredPosition2;
				TextComponent pauseName = _pauseName;
				desiredPosition2 = (_pauseName.DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.3f));
				pauseName.Position = desiredPosition2;
				for (int num24 = 0; num24 < _menuPause.Length; num24++)
				{
					ListButton obj54 = _menuPause[num24];
					desiredPosition2 = (_menuPause[num24].DesiredPosition = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.02f + 36f * (float)num24));
					obj54.Position = desiredPosition2;
				}
				TextComponent obj55 = _promptsPause[0];
				desiredPosition2 = (_promptsPause[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsPause[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsPause[0].Size.Y * 0.5f));
				obj55.Position = desiredPosition2;
				TextComponent obj56 = _promptsPause[1];
				desiredPosition2 = (_promptsPause[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsPause[1].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsPause[1].Size.Y * 0.5f));
				obj56.Position = desiredPosition2;
			}
			_menuState = _nextState;
			break;
		}
		case MenuState.Connect:
		{
			_logo.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f);
			_logo.Update(gameTime);
			panelSpacing = ((float)_contentArea.Width - 320f) / (float)(_menuConnect.Length - 1);
			for (int num10 = 0; num10 != _menuConnect.Length; num10++)
			{
				_menuConnect[num10].DesiredPosition = new Vector2((float)_contentArea.Left + 160f + panelSpacing * (float)num10, (float)_contentArea.Center.Y + 100f);
				ConnectPanel obj11 = _menuConnect[num10];
				Vector2 desiredPosition2 = (_menuConnect[num10].DesiredSize = new Vector2(200f, 260f));
				obj11.Size = desiredPosition2;
				ConnectPanel obj12 = _menuConnect[num10];
				Color colour = (_menuConnect[num10].DesiredColour = new Color(102, 102, 255) * 0.5f);
				obj12.Colour = colour;
				_menuConnect[num10].Update(gameTime);
			}
			if (_menuConnect[0].Position.X == _menuConnect[0].DesiredPosition.X)
			{
				_playerInControl = _leader;
				_playerManager.ConnectState = true;
				for (int num11 = 0; num11 != _menuGames.Length; num11++)
				{
					ListButton obj13 = _menuGames[num11];
					Vector2 desiredPosition2 = (_menuGames[num11].DesiredPosition = new Vector2((float)_contentArea.Center.X + 74f + 1280f, (float)_contentArea.Top + 57f));
					obj13.Position = desiredPosition2;
				}
				SignedInGamer.SignedIn += Connect_GamerSignedIn;
				SignedInGamer.SignedOut += Connect_GamerSignedOut;
				for (int num12 = 0; num12 != _playerManager.NumberOfPlayers; num12++)
				{
					_menuConnect[(int)_playerManager.PlayersConnected[num12].PlayerIndex].IsReady = true;
				}
				_menuState = _nextState;
			}
			break;
		}
		case MenuState.Games:
			if (_storageManager.DeviceState != StorageManager.StorageDeviceState.Working)
			{
				_sortMode = (MinigameMeta.SortMode)_leaderPlayer.SortMode;
				MinigameMeta.Sort(ref _minigameMeta, _sortMode);
				switch (_sortMode)
				{
				case MinigameMeta.SortMode.Unsorted:
					_sortModeText.Text = "";
					break;
				case MinigameMeta.SortMode.Rating:
					_sortModeText.Text = "Sorted by rating.";
					break;
				case MinigameMeta.SortMode.Name:
					_sortModeText.Text = "Sorted by name.";
					break;
				case MinigameMeta.SortMode.MinPlayers:
					_sortModeText.Text = "Sorted by minimum players.";
					break;
				case MinigameMeta.SortMode.MaxPlayers:
					_sortModeText.Text = "Sorted by maximum players.";
					break;
				case MinigameMeta.SortMode.Genre:
					_sortModeText.Text = "Sorted by genre.";
					break;
				case MinigameMeta.SortMode.Competition:
					_sortModeText.Text = "Sorted by competition type.";
					break;
				default:
					_sortModeText.Text = "";
					break;
				}
				Vector2 vector12 = new Vector2((float)_contentArea.Center.X + 74f, (float)_contentArea.Top + 40f);
				Vector2 desiredPosition2;
				for (int num15 = 0; num15 != _menuGames.Length; num15++)
				{
					_menuGames[num15].Text = _minigameMeta[num15].Name;
					_menuGames[num15].DesiredPosition = new Vector2(vector12.X, _menuGames[num15].Position.Y);
					_menuGames[num15].IsHighlighted = false;
					ListButton obj16 = _menuGames[num15];
					desiredPosition2 = (_menuGames[num15].DesiredSize = new Vector2(550f, 34f));
					obj16.Size = desiredPosition2;
					ListButton obj17 = _menuGames[num15];
					Color colour = (_menuGames[num15].DesiredColour = new Color(102, 102, 255) * 0.8f);
					obj17.Colour = colour;
					_menuGames[num15].Update(gameTime);
					_menuGames[num15].TextColour = Color.White * 0f;
					StarRating obj18 = _starGames[num15];
					desiredPosition2 = (_starGames[num15].DesiredPosition = _menuGames[num15].Position);
					obj18.Position = desiredPosition2;
					_starGames[num15].Colour = Color.White * 0f;
				}
				Vector2 desiredPosition3 = new Vector2((float)_contentArea.Center.X + 76f, (float)_contentArea.Top + 40f);
				_gameImage.DesiredPosition = desiredPosition3;
				_gameImage.Update(gameTime);
				_gameLockImage.DesiredPosition = desiredPosition3;
				_gameLockImage.Update(gameTime);
				_gameDescription.DesiredPosition = _gameImage.DesiredPosition + new Vector2(0f, _gameImage.Size.Y + 2f);
				_gameDescription.Update(gameTime);
				_gameGenre.DesiredPosition = _gameDescription.DesiredPosition + new Vector2(0f, _gameDescription.Size.Y + 2f);
				_gameGenre.Update(gameTime);
				_gameCompetitionImage.DesiredPosition = _gameGenre.DesiredPosition + new Vector2(0f, _gameGenre.Size.Y + 2f);
				_gameCompetitionImage.Update(gameTime);
				_gamePlayerLimit.DesiredPosition = _gameCompetitionImage.DesiredPosition + new Vector2(_gameCompetitionImage.Size.X, 0f);
				_gamePlayerLimit.Update(gameTime);
				_gameCompetition.DesiredPosition = _gameCompetitionImage.DesiredPosition + new Vector2(_gameCompetitionImage.Size.X, _gameCompetitionImage.Size.Y);
				_gameCompetition.Update(gameTime);
				_gameHighscore.DesiredPosition = _gameCompetitionImage.DesiredPosition + new Vector2(0f, _gameCompetitionImage.Size.Y + 2f);
				_gameHighscore.Update(gameTime);
				_sortModeText.DesiredPosition = new Vector2(_gameImage.Position.X + _gameImage.Size.X, (float)_contentArea.Bottom - 40f);
				_sortModeText.Update(gameTime);
				UpdateGameMeta();
				TextComponent obj19 = _promptsGames[0];
				desiredPosition2 = (_promptsGames[0].DesiredPosition = _gameHighscore.Position + new Vector2(_promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
				obj19.Position = desiredPosition2;
				_promptsGames[0].Update(gameTime);
				TextComponent obj20 = _promptsGames[1];
				desiredPosition2 = (_promptsGames[1].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * 0.25f + _promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
				obj20.Position = desiredPosition2;
				_promptsGames[1].Update(gameTime);
				TextComponent obj21 = _promptsGames[2];
				desiredPosition2 = (_promptsGames[2].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * 0.5f + _promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
				obj21.Position = desiredPosition2;
				_promptsGames[2].Update(gameTime);
				TextComponent obj22 = _promptsGames[3];
				desiredPosition2 = (_promptsGames[3].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * 0.75f + _promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
				obj22.Position = desiredPosition2;
				_promptsGames[3].Update(gameTime);
				if (_gameImage.Position.X == _gameImage.DesiredPosition.X)
				{
					_menuState = _nextState;
				}
			}
			break;
		case MenuState.Instruction:
			_instructionImage.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y);
			_instructionImage.Update(gameTime);
			if (_instructionImage.Position.X == _instructionImage.DesiredPosition.X)
			{
				_menuState = _nextState;
			}
			_promptsInstruction[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsInstruction[0].Update(gameTime);
			_promptsInstruction[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsInstruction[1].Update(gameTime);
			break;
		case MenuState.Loading:
			_menuState = _nextState;
			break;
		case MenuState.Settings:
			_logo.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + (float)_logo.Sprite.Height * 0.5f);
			_logo.Update(gameTime);
			if (_storageManager.DeviceState != StorageManager.StorageDeviceState.Working)
			{
				for (int num7 = 0; num7 != _menuSettings.Length; num7++)
				{
					_menuSettings[num7].DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y + 36f * (float)num7);
					ListButton obj9 = _menuSettings[num7];
					Vector2 desiredPosition2 = (_menuSettings[num7].DesiredSize = new Vector2(400f, 34f));
					obj9.Size = desiredPosition2;
					ListButton obj10 = _menuSettings[num7];
					Color colour = (_menuSettings[num7].DesiredColour = new Color(102, 102, 255) * 0.8f);
					obj10.Colour = colour;
					_menuSettings[num7].IsHighlighted = false;
					_menuSettings[num7].Update(gameTime);
				}
				if (_menuSettings[0].Position.X == _menuSettings[0].DesiredPosition.X)
				{
					_menuState = _nextState;
				}
				_promptsSettings[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsSettings[0].Update(gameTime);
				_promptsSettings[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
				_promptsSettings[1].Update(gameTime);
			}
			break;
		case MenuState.Credits:
		{
			_creditsLogo.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y - 160f);
			_creditsLogo.Update(gameTime);
			_creditsHeaders[0].DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y - 40f);
			_creditsHeaders[1].DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y + 90f);
			for (int num8 = 0; num8 != _creditsHeaders.Length; num8++)
			{
				_creditsHeaders[num8].Update(gameTime);
			}
			_creditsNames[0].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y);
			_creditsNames[1].DesiredPosition = _creditsHeaders[0].Position + new Vector2(0f, _creditsHeaders[0].Size.Y * 2f);
			_creditsNames[2].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y);
			_creditsNames[3].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 2f);
			_creditsNames[4].DesiredPosition = _creditsHeaders[1].Position + new Vector2(0f, _creditsHeaders[1].Size.Y * 3f);
			for (int num9 = 0; num9 != _creditsNames.Length; num9++)
			{
				_creditsNames[num9].Update(gameTime);
			}
			_promptCredits.DesiredPosition = new Vector2((float)_contentArea.Left + _promptCredits.Size.X * 0.5f, (float)_contentArea.Bottom - _promptCredits.Size.Y * 0.5f);
			_promptCredits.Update(gameTime);
			if (_creditsLogo.Position.X == _creditsLogo.DesiredPosition.X)
			{
				_menuState = _nextState;
			}
			break;
		}
		case MenuState.Screen:
		{
			_screenCentre.DesiredPosition = new Vector2(_contentArea.Center.X, _contentArea.Center.Y);
			_screenCorners[0].DesiredPosition = new Vector2((float)_contentArea.Left - 10f, (float)_contentArea.Top - 10f);
			_screenCorners[1].DesiredPosition = new Vector2((float)_contentArea.Right + 10f, (float)_contentArea.Top - 10f);
			_screenCorners[2].DesiredPosition = new Vector2((float)_contentArea.Right + 10f, (float)_contentArea.Bottom + 10f);
			_screenCorners[3].DesiredPosition = new Vector2((float)_contentArea.Left - 10f, (float)_contentArea.Bottom + 10f);
			_screenCentre.Update(gameTime);
			_screenCorners[0].Update(gameTime);
			_screenCorners[1].Update(gameTime);
			_screenCorners[2].Update(gameTime);
			_screenCorners[3].Update(gameTime);
			_screenAreaText.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Top + 10f);
			_screenBrightnessText.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Bottom - 10f);
			_screenAreaText.Update(gameTime);
			_screenBrightnessText.Update(gameTime);
			for (int num25 = 0; num25 < _promptsScreen.Length; num25++)
			{
				TextComponent obj57 = _promptsScreen[num25];
				Vector2 desiredPosition2 = (_promptsScreen[num25].DesiredPosition = new Vector2(_screenCentre.Position.X - _screenCentre.Size.X * 0.5f + _screenCentre.Size.X * (0.2f * (float)num25) + _promptsScreen[num25].Size.X * 0.5f, _screenCentre.Position.Y + _screenCentre.Size.Y * 0.5f + _promptsScreen[num25].Size.Y * 0.5f + 4f));
				obj57.Position = desiredPosition2;
			}
			_selectedIndex = 2;
			if (_screenCentre.Position.X == _screenCentre.DesiredPosition.X)
			{
				_menuState = _nextState;
			}
			break;
		}
		case MenuState.Confirm:
		{
			_confirmText.DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y - 40f);
			_confirmText.Update(gameTime);
			for (int num13 = 0; num13 != _menuConfirm.Length; num13++)
			{
				_menuConfirm[num13].DesiredPosition = new Vector2(_contentArea.Center.X, (float)_contentArea.Center.Y + 36f * (float)num13);
				ListButton obj14 = _menuConfirm[num13];
				Vector2 desiredPosition2 = (_menuMain[num13].DesiredSize = new Vector2(200f, 34f));
				obj14.Size = desiredPosition2;
				ListButton obj15 = _menuConfirm[num13];
				Color colour = (_menuMain[num13].DesiredColour = new Color(102, 102, 255) * 0.8f);
				obj15.Colour = colour;
				_menuConfirm[num13].IsHighlighted = false;
				_menuConfirm[num13].Update(gameTime);
			}
			if (_confirmText.Position.X == _confirmText.DesiredPosition.X)
			{
				_menuState = _nextState;
			}
			_promptsConfirm[0].DesiredPosition = new Vector2((float)_contentArea.Right - _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsConfirm[0].Update(gameTime);
			_promptsConfirm[1].DesiredPosition = new Vector2((float)_contentArea.Left + _promptsMain[0].Size.X * 0.5f, (float)_contentArea.Bottom - _promptsMain[0].Size.Y * 0.5f);
			_promptsConfirm[1].Update(gameTime);
			break;
		}
		case MenuState.Quit:
			_overlay.DesiredColour = Color.Black;
			_overlay.Update(gameTime);
			if (_overlay.Colour == _overlay.DesiredColour)
			{
				_menuState = _nextState;
			}
			break;
		case MenuState.Pause:
		{
			float x = _pauseName.Font.MeasureString(_playerManager.GetPlayer(_playerInControl).Name).X;
			if (x > 250f)
			{
				_pauseBackground.DesiredSize = new Vector2(x + 20f, _pauseBackground.DesiredSize.Y);
			}
			else
			{
				_pauseBackground.DesiredSize = new Vector2(250f, _pauseBackground.DesiredSize.Y);
			}
			_pauseBackground.Update(gameTime);
			_pauseName.Update(gameTime);
			for (int num14 = 0; num14 < _menuPause.Length; num14++)
			{
				_menuPause[num14].DesiredTextColour = Color.White;
				_menuPause[num14].Enabled = true;
				_menuPause[num14].Update(gameTime);
			}
			if (_pauseBackground.Size.X == _pauseBackground.DesiredSize.X || _menuState == _nextState)
			{
				_pauseBackground.ScaleSpeed = 20f;
				_pauseName.DesiredTextColour = Color.White;
				_promptsPause[0].Sprite = _AButtonSprite;
				_promptsPause[0].Text = "Select     ";
				_promptsPause[0].DesiredColour = Color.White;
				_promptsPause[0].DesiredTextColour = Color.White;
				_promptsPause[0].DesiredOutlineColour = Color.Black;
				_promptsPause[1].Text = "     Resume";
				_promptsPause[1].DesiredColour = Color.White;
				_promptsPause[1].DesiredTextColour = Color.White;
				_promptsPause[1].DesiredOutlineColour = Color.Black;
				_menuState = _nextState;
			}
			break;
		}
		case MenuState.Disconnect:
			_pauseBackground.DesiredSize = new Vector2(500f, _pauseBackground.DesiredSize.Y);
			_pauseBackground.Update(gameTime);
			_pauseName.Update(gameTime);
			if (_pauseBackground.Size.X == _pauseBackground.DesiredSize.X)
			{
				_pauseBackground.ScaleSpeed = 20f;
				_pauseName.DesiredTextColour = Color.White;
				if (_playerManager.NumberOfPlayers != 1)
				{
					_promptsPause[0].Sprite = _BackButtonSprite;
					_promptsPause[0].Text = "Quit     ";
					_promptsPause[0].DesiredColour = Color.White;
					_promptsPause[0].DesiredTextColour = Color.White;
					_promptsPause[0].DesiredOutlineColour = Color.Black;
				}
				if (_playerManager.GetPlayer(_playerInControl).GamePadManager.GamePadStateCurrent.IsConnected)
				{
					_promptsPause[1].Text = "     Back";
					_promptsPause[1].DesiredColour = Color.White;
					_promptsPause[1].DesiredTextColour = Color.White;
					_promptsPause[1].DesiredOutlineColour = Color.Black;
				}
				_menuState = _nextState;
			}
			break;
		case MenuState.SignOut:
			_pauseBackground.DesiredSize = new Vector2(_pauseName.Font.MeasureString(_pauseName.Text).X + 4f, _pauseBackground.DesiredSize.Y);
			_pauseBackground.Update(gameTime);
			_pauseName.Update(gameTime);
			if (_pauseBackground.Size.X == _pauseBackground.DesiredSize.X)
			{
				_pauseBackground.ScaleSpeed = 20f;
				_pauseName.DesiredTextColour = Color.White;
				_promptsPause[0].Sprite = _StartButtonSprite;
				_promptsPause[0].Text = "Continue as Player " + (int)(_playerInControl + 1) + "     ";
				_promptsPause[0].DesiredColour = Color.White;
				_promptsPause[0].DesiredTextColour = Color.White;
				_promptsPause[0].DesiredOutlineColour = Color.Black;
				_menuState = _nextState;
			}
			break;
		}
	}

	private void Connect_GamerSignedIn(object sender, SignedInEventArgs e)
	{
		SignedInGamer gamer = e.Gamer;
		Player player = _playerManager.GetPlayer(gamer.PlayerIndex);
		if (player != null)
		{
			GameConsole.PrintString("Menu: " + gamer.Gamertag + " signed in on game pad " + (int)gamer.PlayerIndex + ". Assigning to " + player.Name);
			player.Name = gamer.Gamertag;
			if (_storageManager.DeviceState == StorageManager.StorageDeviceState.Ready || _storageManager.DeviceState == StorageManager.StorageDeviceState.Working)
			{
				player.WaitingForProfileLoad = true;
				_storageManager.Load(ref player, loadCurrentSettings: false);
			}
			_menuConnect[(int)player.PlayerIndex].IsReady = false;
			player.GamerProblem = false;
		}
	}

	private void Connect_GamerSignedOut(object sender, SignedOutEventArgs e)
	{
		SignedInGamer gamer = e.Gamer;
		Player player = _playerManager.GetPlayer(gamer.PlayerIndex);
		if (player != null)
		{
			GameConsole.PrintString("Menu: " + gamer.Gamertag + " signed out. Renamed to Player " + (int)(player.PlayerIndex + 1));
			_menuConnect[(int)player.PlayerIndex].IsReady = false;
			player.Name = "Player " + (int)(player.PlayerIndex + 1);
		}
	}

	private void UpdateGameMeta()
	{
		if (_minigameMeta[_selectedIndex].Image != null)
		{
			_gameImage.Sprite = _contentLoader.Load<Texture2D>("Menu\\Sprites\\GameBanners\\" + _minigameMeta[_selectedIndex].Image);
		}
		else
		{
			_gameImage.Sprite = _contentLoader.Load<Texture2D>("Menu\\Sprites\\GameBanners\\null");
		}
		_gameDescription.Text = _minigameMeta[_selectedIndex].Description;
		if (_minigameMeta[_selectedIndex].BestWinner == "" || _minigameMeta[_selectedIndex].BestWinner == null)
		{
			_gameHighscore.Text = "";
		}
		else
		{
			_gameHighscore.Text = _minigameMeta[_selectedIndex].BestWinner + ": " + _minigameMeta[_selectedIndex].BestScore + " " + _minigameMeta[_selectedIndex].ScoreUnit;
		}
		_gameCompetitionImage.Sprite = _contentLoader.Load<Texture2D>("Menu\\Sprites\\Competition\\" + _minigameMeta[_selectedIndex].Competition);
		string text = _minigameMeta[_selectedIndex].MinimumPlayers.ToString();
		text = ((_minigameMeta[_selectedIndex].MaximumPlayers != _minigameMeta[_selectedIndex].MinimumPlayers) ? (text + " - " + _minigameMeta[_selectedIndex].MaximumPlayers + " players") : ((_minigameMeta[_selectedIndex].MinimumPlayers <= 1) ? (text + " player") : (text + " players")));
		_gamePlayerLimit.Text = text;
		switch (_minigameMeta[_selectedIndex].Competition)
		{
		case GameCompetition.CoOp:
			_gameCompetition.Text = "Co-operative";
			break;
		case GameCompetition.Team:
			_gameCompetition.Text = "Team battle";
			break;
		case GameCompetition.FreeForAll:
			_gameCompetition.Text = "Free for all";
			break;
		case GameCompetition.Unilateral:
			_gameCompetition.Text = "One versus all";
			break;
		case GameCompetition.OneVsOne:
			_gameCompetition.Text = "Head to head";
			break;
		}
		_gameGenre.Text = _minigameMeta[_selectedIndex].Genre.ToString();
		if (_minigameMeta[_selectedIndex].InstructionImage != null)
		{
			_instructionImage.Sprite = _contentLoader.Load<Texture2D>("Menu\\Sprites\\Instructions\\" + _minigameMeta[_selectedIndex].InstructionImage);
		}
		else
		{
			_instructionImage.Sprite = _contentLoader.Load<Texture2D>("Menu\\Sprites\\Instructions\\null");
		}
		_gameDescription.FitTextToWidth(0f);
		_gameDescription.FitComponentToText(0f);
		TextComponent gameDescription = _gameDescription;
		Vector2 size = (_gameDescription.DesiredSize = new Vector2(400f, _gameDescription.Size.Y));
		gameDescription.Size = size;
		TextComponent gameGenre = _gameGenre;
		Vector2 position = (_gameGenre.DesiredPosition = _gameDescription.Position + new Vector2(0f, _gameDescription.Size.Y + 2f));
		gameGenre.Position = position;
		MenuComponent gameCompetitionImage = _gameCompetitionImage;
		Vector2 position2 = (_gameCompetitionImage.DesiredPosition = _gameGenre.Position + new Vector2(0f, _gameGenre.Size.Y + 2f));
		gameCompetitionImage.Position = position2;
		TextComponent gamePlayerLimit = _gamePlayerLimit;
		Vector2 position3 = (_gamePlayerLimit.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, 0f));
		gamePlayerLimit.Position = position3;
		TextComponent gameCompetition = _gameCompetition;
		Vector2 position4 = (_gameCompetition.DesiredPosition = _gameCompetitionImage.Position + new Vector2(160f, _gameCompetitionImage.Size.Y));
		gameCompetition.Position = position4;
		TextComponent gameHighscore = _gameHighscore;
		Vector2 position5 = (_gameHighscore.DesiredPosition = _gameCompetitionImage.Position + new Vector2(0f, _gameCompetitionImage.Size.Y + 2f));
		gameHighscore.Position = position5;
		_gameHighscore.FitTextToWidth(0f);
		_gameHighscore.FitComponentToText(0f);
		TextComponent gameHighscore2 = _gameHighscore;
		Vector2 size2 = (_gameHighscore.DesiredSize = new Vector2(400f, _gameHighscore.Size.Y));
		gameHighscore2.Size = size2;
		for (int i = 0; i < _promptsGames.Length; i++)
		{
			TextComponent obj = _promptsGames[i];
			Vector2 position6 = (_promptsGames[i].DesiredPosition = _gameHighscore.Position + new Vector2(_gameHighscore.Size.X * 0.25f * (float)i + _promptsGames[0].Size.X * 0.5f, _gameHighscore.Size.Y + 2f + _promptsGames[0].Size.Y * 0.5f));
			obj.Position = position6;
		}
	}

	public void Pause(GameTime gameTime)
	{
		PlayerIndex playerIndex = _leader;
		if (_playerManager.GetPlayer(_leader) == null)
		{
			if (_playerInControl != _leader && _playerManager.GetPlayer(_playerInControl) != null)
			{
				playerIndex = _playerInControl;
			}
			else if (_playerManager.NumberOfPlayers != 0)
			{
				_playerInControl = _playerManager.PlayersConnected[0].PlayerIndex;
			}
		}
		Pause(playerIndex, gameTime);
	}

	public void Pause(PlayerIndex playerIndex, GameTime gameTime)
	{
		_selectedIndex = 0;
		_playerInControl = playerIndex;
		_pauseBackground.Colour *= 0f;
		_pauseBackground.ScaleSpeed = 2f;
		_pauseHeader.Position = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.4f);
		_pauseHeader.TextColour = Color.White * 0f;
		_pauseName.Position = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * -0.3f);
		_pauseName.TextColour = Color.White * 0f;
		_pauseName.ColourBlendSpeed = 0.16f;
		_pauseName.DesiredTextColour = Color.White;
		_pauseName.Text = _playerManager.GetPlayer(_playerInControl).Name;
		float x = _pauseName.Font.MeasureString(_pauseName.Text).X;
		if (x > 260f)
		{
			_pauseBackground.DesiredSize = new Vector2(x + 20f, _pauseBackground.DesiredSize.Y);
			_pauseBackground.Size = new Vector2(x, 210f);
		}
		else
		{
			_pauseBackground.DesiredSize = new Vector2(260f, _pauseBackground.DesiredSize.Y);
			_pauseBackground.Size = new Vector2(240f, 210f);
		}
		_pauseBackground.Update(gameTime);
		for (int i = 0; i < _menuPause.Length; i++)
		{
			_menuPause[i].Position = _pauseBackground.Position + new Vector2(0f, _pauseBackground.Size.Y * (-0.02f + 0.17f * (float)i));
			_menuPause[i].Colour = new Color(102, 102, 255) * 0f;
			_menuPause[i].TextColour = Color.White * 0f;
			_menuPause[i].DesiredTextColour = Color.White;
			_menuPause[i].Enabled = true;
			_menuPause[i].ColourBlendSpeed = 0.16f;
		}
		for (int j = 0; j < _promptsPause.Length; j++)
		{
			_promptsPause[j].Colour = Color.White * 0f;
			_promptsPause[j].DesiredColour = Color.White;
			_promptsPause[j].TextColour = Color.White * 0f;
			_promptsPause[j].DesiredTextColour = Color.White;
			_promptsPause[j].OutlineColour = Color.Black * 0f;
			_promptsPause[j].DesiredOutlineColour = Color.Black;
			_promptsPause[j].ColourBlendSpeed = 0.16f;
		}
		_menuState = MenuState.Pause;
		if (!_playerManager.GetPlayer(_playerInControl).GamePadManager.GamePadStateCurrent.IsConnected)
		{
			_nextState = MenuState.Disconnect;
		}
		else if (_playerManager.GetPlayer(_playerInControl).GamerProblem)
		{
			_nextState = MenuState.SignOut;
		}
		else
		{
			_nextState = MenuState.Pause;
		}
	}

	public void QuitMinigame(int minigameID, GameTime gameTime)
	{
		float x = (float)_contentArea.Center.X + 74f;
		for (int i = 0; i != _minigameMeta.Length; i++)
		{
			ListButton obj = _menuGames[i];
			Vector2 position = (_menuGames[i].DesiredPosition = new Vector2(x, _menuGames[i].Position.Y));
			obj.Position = position;
			StarRating obj2 = _starGames[i];
			Vector2 position2 = (_starGames[i].DesiredPosition = _menuGames[i].Position);
			obj2.Position = position2;
		}
		MenuComponent gameImage = _gameImage;
		Vector2 position4 = (_gameImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 76f, _gameImage.Position.Y));
		gameImage.Position = position4;
		TextComponent gameDescription = _gameDescription;
		Vector2 position5 = (_gameDescription.DesiredPosition = new Vector2(_gameImage.Position.X, _gameDescription.Position.Y));
		gameDescription.Position = position5;
		TextComponent gameGenre = _gameGenre;
		Vector2 position6 = (_gameGenre.DesiredPosition = new Vector2(_gameDescription.Position.X, _gameGenre.Position.Y));
		gameGenre.Position = position6;
		MenuComponent gameCompetitionImage = _gameCompetitionImage;
		Vector2 position7 = (_gameCompetitionImage.DesiredPosition = new Vector2(_gameGenre.Position.X, _gameCompetitionImage.Position.Y));
		gameCompetitionImage.Position = position7;
		TextComponent gamePlayerLimit = _gamePlayerLimit;
		Vector2 position8 = (_gamePlayerLimit.DesiredPosition = new Vector2(_gameCompetitionImage.Position.X + 160f, _gamePlayerLimit.Position.Y));
		gamePlayerLimit.Position = position8;
		TextComponent gameCompetition = _gameCompetition;
		Vector2 position9 = (_gameCompetition.DesiredPosition = new Vector2(_gameCompetitionImage.Position.X + 160f, _gameCompetition.Position.Y));
		gameCompetition.Position = position9;
		TextComponent gameHighscore = _gameHighscore;
		Vector2 position10 = (_gameHighscore.DesiredPosition = new Vector2(_gameCompetitionImage.Position.X, _gameHighscore.Position.Y));
		gameHighscore.Position = position10;
		MenuComponent instructionImage = _instructionImage;
		Vector2 position11 = (_instructionImage.DesiredPosition = new Vector2((float)_contentArea.Center.X + 1280f, _instructionImage.Position.Y));
		instructionImage.Position = position11;
		for (ushort num = 0; num < (ushort)_minigameMeta.Length; num++)
		{
			if (_minigameMeta[num].MinigameID == minigameID)
			{
				_selectedIndex = num;
				break;
			}
		}
		_storageManager.Load(ref _minigameMeta);
		_menuState = MenuState.Games;
		_nextState = MenuState.Games;
	}
}
