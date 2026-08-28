using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames;

internal class ConnectPanel : MenuComponent
{
	private TextComponent _backgroundMessage;

	private TextComponent _nameHeading;

	private TextComponent _colourHeading;

	private MenuComponent _colourPrompt;

	private TextComponent _readyHeading;

	private MenuComponent _readyPrompt;

	private TextComponent _liveHeading;

	private MenuComponent _livePrompt;

	private MenuComponent _loadIndicator;

	private ControllerIndexDisplay _controllerDisplay;

	private Texture2D _colourPromptSprite;

	private Texture2D _startPromptSprite;

	private Texture2D _readyPromptSprite;

	private Texture2D _unreadyPromptSprite;

	private PlayerIndex _playerIndex;

	private bool _connected;

	private bool _loading;

	private bool _active;

	private bool _ready;

	private bool _everyoneReady;

	public PlayerIndex PlayerIndex
	{
		get
		{
			return _playerIndex;
		}
		set
		{
			_playerIndex = value;
		}
	}

	public bool IsActive
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
		}
	}

	public bool IsReady
	{
		get
		{
			return _ready;
		}
		set
		{
			_ready = value;
		}
	}

	public bool EveryoneReady
	{
		get
		{
			return _everyoneReady;
		}
		set
		{
			_everyoneReady = value;
		}
	}

	public ConnectPanel()
	{
		_colourBlendSpeed = 0.14f;
		_backgroundMessage = new TextComponent();
		_nameHeading = new TextComponent();
		_colourHeading = new TextComponent();
		_colourPrompt = new MenuComponent();
		_readyHeading = new TextComponent();
		_readyPrompt = new MenuComponent();
		_liveHeading = new TextComponent();
		_livePrompt = new MenuComponent();
		_loadIndicator = new MenuComponent();
		_controllerDisplay = new ControllerIndexDisplay();
		_size = (_desiredSize = new Vector2(100f, 200f));
		_ready = false;
		_active = false;
		_everyoneReady = false;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/Pixel");
		_backgroundMessage.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_backgroundMessage.Load(contentLoader);
		_backgroundMessage.Depth = _depth + 0.001f;
		TextComponent backgroundMessage = _backgroundMessage;
		Color textColour = (_backgroundMessage.DesiredTextColour = Color.White * 0.5f);
		backgroundMessage.TextColour = textColour;
		_backgroundMessage.Text = "Connect\nController";
		TextComponent backgroundMessage2 = _backgroundMessage;
		Vector2 position = (_backgroundMessage.DesiredPosition = new Vector2(-300f));
		backgroundMessage2.Position = position;
		_nameHeading.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_nameHeading.Load(contentLoader);
		_nameHeading.FitComponentToText(0f);
		_nameHeading.Depth = _depth + 0.003f;
		TextComponent nameHeading = _nameHeading;
		Vector2 position2 = (_nameHeading.DesiredPosition = new Vector2(-300f));
		nameHeading.Position = position2;
		_nameHeading.ColourBlendSpeed = _colourBlendSpeed;
		_controllerDisplay.Depth = _depth + 0.001f;
		_controllerDisplay.Position = new Vector2(-300f);
		_colourHeading.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_colourHeading.Load(contentLoader);
		_colourHeading.Text = "Color";
		_colourHeading.Depth = _depth + 0.001f;
		TextComponent colourHeading = _colourHeading;
		Vector2 position3 = (_colourHeading.DesiredPosition = new Vector2(-300f));
		colourHeading.Position = position3;
		_colourHeading.ColourBlendSpeed = _colourBlendSpeed;
		_colourPromptSprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/LeftThumbstickHorizontal");
		_startPromptSprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/Start");
		_colourPrompt.Position = _position + new Vector2(0f, _size.Y * -0.1f);
		_colourPrompt.Depth = _depth + 0.001f;
		MenuComponent colourPrompt = _colourPrompt;
		Vector2 position4 = (_colourPrompt.DesiredPosition = new Vector2(-300f));
		colourPrompt.Position = position4;
		_readyHeading.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_readyHeading.Load(contentLoader);
		_readyHeading.Text = "Join";
		_readyHeading.Depth = _depth + 0.001f;
		TextComponent readyHeading = _readyHeading;
		Vector2 position5 = (_readyHeading.DesiredPosition = new Vector2(-300f));
		readyHeading.Position = position5;
		_readyHeading.ColourBlendSpeed = _colourBlendSpeed;
		_readyPromptSprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_unreadyPromptSprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_readyPrompt.Depth = _depth + 0.001f;
		MenuComponent readyPrompt = _readyPrompt;
		Vector2 position6 = (_readyPrompt.DesiredPosition = new Vector2(-300f));
		readyPrompt.Position = position6;
		_liveHeading.Font = contentLoader.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_liveHeading.Load(contentLoader);
		_liveHeading.Text = "Sign in";
		_liveHeading.Depth = _depth + 0.001f;
		TextComponent liveHeading = _liveHeading;
		Vector2 position7 = (_liveHeading.DesiredPosition = new Vector2(-300f));
		liveHeading.Position = position7;
		_liveHeading.ColourBlendSpeed = _colourBlendSpeed;
		_livePrompt.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Buttons/X");
		_livePrompt.FitComponentToImage();
		_livePrompt.Size *= new Vector2(0.8f, 0.8f);
		_livePrompt.Depth = _depth + 0.001f;
		MenuComponent livePrompt = _livePrompt;
		Vector2 position8 = (_livePrompt.DesiredPosition = new Vector2(-300f));
		livePrompt.Position = position8;
		_loadIndicator.Sprite = contentLoader.Load<Texture2D>("Menu/Sprites/General/LoadIndicator");
		_loadIndicator.FitComponentToImage();
		_loadIndicator.SpriteOrigin = new Vector2((float)_loadIndicator.Sprite.Width * 0.5f, (float)_loadIndicator.Sprite.Height * 0.5f);
		_loadIndicator.PositionAnchor = Anchor.TopLeft;
		_loadIndicator.DesiredRotation = (float)Math.PI * 2f;
		_loadIndicator.Depth = _depth + 0.003f;
		_loadIndicator.Position = new Vector2(-300f);
		FitComponentToImage();
		_nameHeading.Load(contentLoader);
		_colourHeading.Load(contentLoader);
		_colourPrompt.Load(contentLoader);
		_readyHeading.Load(contentLoader);
		_readyPrompt.Load(contentLoader);
		_controllerDisplay.Load(contentLoader);
	}

	public void Update(GameTime gameTime, PlayerManager playerManager, StorageManager storageManager, SoundManager soundManager)
	{
		base.Update(gameTime);
		if (_connected && !playerManager.GetGamePad(_playerIndex).GamePadStateCurrent.IsConnected)
		{
			playerManager.PlayerLeave(_playerIndex);
		}
		_connected = playerManager.GetGamePad(_playerIndex).GamePadStateCurrent.IsConnected;
		_nameHeading.TextScale = 1f;
		_nameHeading.FitComponentToText(0f);
		if (_nameHeading.Size.X > _size.X)
		{
			_nameHeading.TextScale = 1f - (_nameHeading.Size.X - _size.X) / _nameHeading.Size.X;
		}
		TextComponent nameHeading = _nameHeading;
		Vector2 position = (_nameHeading.DesiredPosition = _position + new Vector2(0f, (0f - _size.Y) * 0.3f));
		nameHeading.Position = position;
		TextComponent backgroundMessage = _backgroundMessage;
		Vector2 position2 = (_backgroundMessage.DesiredPosition = _position);
		backgroundMessage.Position = position2;
		_backgroundMessage.Depth = _depth + 0.001f;
		_controllerDisplay.Connected = _connected;
		TextComponent colourHeading = _colourHeading;
		Vector2 position4 = (_colourHeading.DesiredPosition = _position + new Vector2(0f, _size.Y * -0.15f));
		colourHeading.Position = position4;
		MenuComponent colourPrompt = _colourPrompt;
		Vector2 position5 = (_colourPrompt.DesiredPosition = _position + new Vector2(0f, _size.Y * 0f));
		colourPrompt.Position = position5;
		if (_connected)
		{
			_controllerDisplay.PlayerIndex = _playerIndex;
			if (_active)
			{
				_loading = playerManager.GetPlayer(_playerIndex).WaitingForProfileLoad;
				_nameHeading.Text = playerManager.GetPlayer(_playerIndex).Name;
				TextComponent liveHeading = _liveHeading;
				Vector2 position6 = (_liveHeading.DesiredPosition = _position + new Vector2(_size.X * 0.25f, _size.Y * 0.2f));
				liveHeading.Position = position6;
				MenuComponent livePrompt = _livePrompt;
				Vector2 position7 = (_livePrompt.DesiredPosition = _position + new Vector2(_size.X * 0.25f, _size.Y * 0.35f));
				livePrompt.Position = position7;
				ControllerIndexDisplay controllerDisplay = _controllerDisplay;
				Vector2 position8 = (_controllerDisplay.DesiredPosition = _position + new Vector2(0f, (0f - _size.Y) * 0.45f));
				controllerDisplay.Position = position8;
				if (_loading)
				{
					MenuComponent loadIndicator = _loadIndicator;
					Vector2 position9 = (_loadIndicator.DesiredPosition = _position);
					loadIndicator.Position = position9;
					if (_loadIndicator.Rotation == _loadIndicator.DesiredRotation)
					{
						_loadIndicator.Rotation = 0f;
					}
					_loadIndicator.Update(gameTime);
				}
				else if (_ready)
				{
					_colourHeading.Text = "Play!";
					_colourPrompt.Sprite = _startPromptSprite;
					_colourPrompt.FitComponentToImage();
					_colourPrompt.Size *= new Vector2(0.8f, 0.8f);
					_readyHeading.Text = "Unready";
					TextComponent readyHeading = _readyHeading;
					Vector2 position11 = (_readyHeading.DesiredPosition = _position + new Vector2(0f, _size.Y * 0.2f));
					readyHeading.Position = position11;
					_readyPrompt.Sprite = _unreadyPromptSprite;
					_readyPrompt.FitComponentToImage();
					_readyPrompt.Size *= new Vector2(0.8f, 0.8f);
					MenuComponent readyPrompt = _readyPrompt;
					Vector2 position12 = (_readyPrompt.DesiredPosition = _position + new Vector2(0f, _size.Y * 0.35f));
					readyPrompt.Position = position12;
					_desiredSize.X = 200f;
					_desiredSize.Y = 260f;
					_desiredColour = playerManager.GetPlayerColor(playerManager.GetPlayer(_playerIndex)) * 0.8f;
					if (playerManager.GetGamePad(_playerIndex).ButtonWasPressed(Buttons.B))
					{
						soundManager.CreateMenuSoundCue("menu Whiz").Play();
						_ready = false;
					}
				}
				else
				{
					_colourHeading.Text = "Color";
					_colourPrompt.Sprite = _colourPromptSprite;
					_colourPrompt.FitComponentToImage();
					_colourPrompt.Size *= new Vector2(0.8f, 0.8f);
					_readyHeading.Text = "Ready";
					TextComponent readyHeading2 = _readyHeading;
					Vector2 position13 = (_readyHeading.DesiredPosition = _position + new Vector2(_size.X * -0.25f, _size.Y * 0.2f));
					readyHeading2.Position = position13;
					_readyPrompt.Sprite = _readyPromptSprite;
					_readyPrompt.FitComponentToImage();
					_readyPrompt.Size *= new Vector2(0.8f, 0.8f);
					MenuComponent readyPrompt2 = _readyPrompt;
					Vector2 position14 = (_readyPrompt.DesiredPosition = _position + new Vector2(_size.X * -0.25f, _size.Y * 0.35f));
					readyPrompt2.Position = position14;
					_desiredSize.X = 210f;
					_desiredSize.Y = 270f;
					_desiredColour = playerManager.GetPlayerColor(playerManager.GetPlayer(_playerIndex)) * 1f;
					if (playerManager.GetGamePad(_playerIndex).ButtonWasPressed(Buttons.A))
					{
						soundManager.CreateMenuSoundCue("menu Click").Play();
						_ready = true;
					}
					else if (playerManager.GetGamePad(_playerIndex).ButtonWasPressed(Buttons.B))
					{
						soundManager.CreateMenuSoundCue("menu Whiz").Play();
						playerManager.PlayerLeave(_playerIndex);
						_active = false;
					}
					else if (playerManager.GetGamePad(_playerIndex).ButtonWasPressed(Buttons.X))
					{
						soundManager.CreateMenuSoundCue("menu Click").Play();
						Guide.ShowSignIn(1, onlineOnly: false);
					}
				}
			}
			else
			{
				_ready = false;
				_nameHeading.Text = "";
				_readyHeading.Text = "Join";
				TextComponent readyHeading3 = _readyHeading;
				Vector2 position15 = (_readyHeading.DesiredPosition = _position + new Vector2(0f, _size.Y * 0.15f));
				readyHeading3.Position = position15;
				_readyPrompt.Sprite = _readyPromptSprite;
				_readyPrompt.FitComponentToImage();
				_readyPrompt.Size *= new Vector2(0.8f, 0.8f);
				MenuComponent readyPrompt3 = _readyPrompt;
				Vector2 position16 = (_readyPrompt.DesiredPosition = _position + new Vector2(0f, _size.Y * 0.3f));
				readyPrompt3.Position = position16;
				ControllerIndexDisplay controllerDisplay2 = _controllerDisplay;
				Vector2 position17 = (_controllerDisplay.DesiredPosition = _position + new Vector2(0f, (0f - _size.Y) * 0.1f));
				controllerDisplay2.Position = position17;
				_desiredSize.X = 200f;
				_desiredSize.Y = 260f;
				_desiredColour = new Color(102, 102, 255) * 0.8f;
				if (playerManager.GetGamePad(_playerIndex).ButtonWasPressed(Buttons.A))
				{
					soundManager.CreateMenuSoundCue("menu Click").Play();
					playerManager.PlayerJoin(_playerIndex, storageManager, soundManager);
					_active = true;
				}
			}
		}
		else
		{
			_active = false;
			_ready = false;
			_desiredSize.X = 200f;
			_desiredSize.Y = 260f;
			_desiredColour = new Color(102, 102, 255) * 0.5f;
		}
		if (_desiredColour.G > 202)
		{
			_nameHeading.DesiredTextColour = Color.Black;
			_colourHeading.DesiredTextColour = Color.Black;
			_readyHeading.DesiredTextColour = Color.Black;
			_liveHeading.DesiredTextColour = Color.Black;
		}
		else
		{
			_nameHeading.DesiredTextColour = Color.White;
			_colourHeading.DesiredTextColour = Color.White;
			_readyHeading.DesiredTextColour = Color.White;
			_liveHeading.DesiredTextColour = Color.White;
		}
		_nameHeading.Update(gameTime);
		_colourHeading.Update(gameTime);
		_readyHeading.Update(gameTime);
		_liveHeading.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_connected)
		{
			if (_loading)
			{
				_loadIndicator.Draw(spriteBatch);
			}
			else if (_active)
			{
				_nameHeading.Draw(spriteBatch);
				if (!_ready)
				{
					_colourHeading.Draw(spriteBatch);
					_colourPrompt.Draw(spriteBatch);
					_liveHeading.Draw(spriteBatch);
					_livePrompt.Draw(spriteBatch);
				}
				else if (_everyoneReady)
				{
					_colourHeading.Draw(spriteBatch);
					_colourPrompt.Draw(spriteBatch);
				}
				_readyHeading.Draw(spriteBatch);
				_readyPrompt.Draw(spriteBatch);
			}
			else
			{
				_readyHeading.Draw(spriteBatch);
				_readyPrompt.Draw(spriteBatch);
			}
			_controllerDisplay.Draw(spriteBatch);
		}
		else
		{
			_backgroundMessage.Draw(spriteBatch);
		}
	}
}
