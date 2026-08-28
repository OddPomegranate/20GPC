using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Sumo;

internal class Sumo : Minigame
{
	private const int BlurAmount = 10;

	private const float BlurIntensity = 0.75f;

	private SpriteBatch _spriteBatch;

	private Texture2D _background;

	private Texture2D _foreground;

	private Vector2 _backgroundPosition;

	private Wrestler _wrestler;

	private SpriteFont _font;

	private int _playerOneWins;

	private int _playerTwoWins;

	private RenderTarget2D[] _effectCanvas;

	private float _timePassed;

	private string _winner;

	private Cue musicCue;

	public Sumo(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		_playerOneWins = 0;
		_playerTwoWins = 0;
		_timePassed = 0f;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		_effectCanvas = new RenderTarget2D[10];
		for (int i = 0; i < _effectCanvas.Length; i++)
		{
			_effectCanvas[i] = new RenderTarget2D(base.GraphicsDevice, base.GraphicsDevice.Viewport.Width, base.GraphicsDevice.Viewport.Height);
		}
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_background = _contentManager.Load<Texture2D>("Sumo/Sprites/Ring");
		_foreground = _contentManager.Load<Texture2D>("Sumo/Sprites/RingOverlay");
		_backgroundPosition.X = (float)base.GraphicsDevice.Viewport.Width / 2f - (float)_background.Width / 2f;
		_backgroundPosition.Y = (float)base.GraphicsDevice.Viewport.Height / 2f - (float)_background.Height / 2f;
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_wrestler = new Wrestler(_playerManager.PlayersConnected[0], _playerManager.PlayersConnected[1], _contentManager.Load<Texture2D>("Sumo/Sprites/Sumos"), _contentManager.Load<Texture2D>("Sumo/Sprites/ArmsOverlay"), _contentManager.Load<Texture2D>("Sumo/Sprites/ArmsUnderlay"), _contentManager.Load<Texture2D>("Sumo/Sprites/SumoOverlay"), _contentManager.Load<Texture2D>("Sumo/Sprites/SumoUnderlay"), _font, new Vector2((float)base.GraphicsDevice.Viewport.Width / 2f, (float)base.GraphicsDevice.Viewport.Height / 2f), 1.5f);
		base.LoadContent();
	}

	public override void Quit()
	{
		for (int i = 0; i < _effectCanvas.Length; i++)
		{
			if (_effectCanvas != null)
			{
				_effectCanvas[i].Dispose();
				_effectCanvas[i] = null;
			}
		}
		_effectCanvas = null;
		base.Quit();
	}

	public override void Update(GameTime gameTime)
	{
		_wrestler.Update();
		float variableValue = (_wrestler.Position - _wrestler.Center).Length() / 10f;
		_soundManager.SetGlobalVariable("Filterness", variableValue);
		_timePassed += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (!_wrestler.Active)
		{
			for (int i = 0; i < _playerManager.NumberOfPlayers; i++)
			{
				if ((_playerManager.PlayersConnected[i].GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed && _playerManager.PlayersConnected[i].GamePadManager.GamePadStatePrevious.Buttons.A == ButtonState.Released) || (_playerManager.PlayersConnected[i].GamePadManager.GamePadStateCurrent.Buttons.X == ButtonState.Pressed && _playerManager.PlayersConnected[i].GamePadManager.GamePadStatePrevious.Buttons.X == ButtonState.Released) || (_playerManager.PlayersConnected[i].GamePadManager.GamePadStateCurrent.Buttons.Y == ButtonState.Pressed && _playerManager.PlayersConnected[i].GamePadManager.GamePadStatePrevious.Buttons.Y == ButtonState.Released) || (_playerManager.PlayersConnected[i].GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Pressed && _playerManager.PlayersConnected[i].GamePadManager.GamePadStatePrevious.Buttons.B == ButtonState.Released) || (_playerManager.PlayersConnected[i].GamePadManager.GamePadStateCurrent.Buttons.RightShoulder == ButtonState.Pressed && _playerManager.PlayersConnected[i].GamePadManager.GamePadStatePrevious.Buttons.RightShoulder == ButtonState.Released) || (_playerManager.PlayersConnected[i].GamePadManager.GamePadStateCurrent.Buttons.LeftShoulder == ButtonState.Pressed && _playerManager.PlayersConnected[i].GamePadManager.GamePadStatePrevious.Buttons.LeftShoulder == ButtonState.Released))
				{
					if (_wrestler.Reset() == 0)
					{
						_playerOneWins++;
					}
					else
					{
						_playerTwoWins++;
					}
					break;
				}
			}
		}
		else if ((_wrestler.Center - _wrestler.Position).Length() > 272f)
		{
			_wrestler.Active = false;
			Cue cue = _soundManager.CreateGameSoundCue("sumo Failure");
			cue.Play();
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		for (int num = _effectCanvas.Length - 2; num >= 0; num--)
		{
			base.GraphicsDevice.SetRenderTarget(_effectCanvas[num + 1]);
			base.GraphicsDevice.Clear(Color.White);
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			_spriteBatch.Draw(_effectCanvas[num], base.GraphicsDevice.Viewport.Bounds, Color.White);
			_spriteBatch.End();
		}
		base.GraphicsDevice.SetRenderTarget(_effectCanvas[0]);
		base.GraphicsDevice.Clear(Color.White);
		_spriteBatch.Begin();
		_spriteBatch.Draw(_background, _backgroundPosition, Color.White);
		_wrestler.Draw(_spriteBatch);
		_spriteBatch.End();
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.Clear(Color.White);
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		for (int i = 0; i != _effectCanvas.Length; i++)
		{
			_spriteBatch.Draw(_effectCanvas[i], _wrestler.Position + (_wrestler.Position - _wrestler.Center) * i * (0.2f * (1f + (float)Math.Sin(_timePassed))), null, Color.White * 0.1f, 0f, _wrestler.Position, 1f + (2f + (float)Math.Sin(_timePassed)) * 0.025f * (float)i, SpriteEffects.None, 0f);
		}
		_spriteBatch.Draw(_effectCanvas[0], new Rectangle(0, 0, 1280, 720), Color.White * 0.4f);
		Helper.DrawOutlinedText(_spriteBatch, _font, _playerManager.PlayersConnected[0].Name, new Vector2((float)base.GraphicsDevice.Viewport.Width / 2f - 400f, (float)base.GraphicsDevice.Viewport.Height / 2f), _playerManager.PlayersConnected[0].Colour(), Color.Black, Helper.OutlineType.Both, centered: true, 1f);
		Helper.DrawOutlinedText(_spriteBatch, _font, _playerOneWins.ToString(), new Vector2((float)base.GraphicsDevice.Viewport.Width / 2f - 400f, (float)base.GraphicsDevice.Viewport.Height / 2f + 60f), _playerManager.PlayersConnected[0].Colour(), Color.Black, Helper.OutlineType.Both, centered: true, 1f);
		Helper.DrawOutlinedText(_spriteBatch, _font, _playerManager.PlayersConnected[1].Name, new Vector2((float)base.GraphicsDevice.Viewport.Width / 2f + 400f, (float)base.GraphicsDevice.Viewport.Height / 2f), _playerManager.PlayersConnected[1].Colour(), Color.Black, Helper.OutlineType.Both, centered: true, 1f);
		Helper.DrawOutlinedText(_spriteBatch, _font, _playerTwoWins.ToString(), new Vector2((float)base.GraphicsDevice.Viewport.Width / 2f + 400f, (float)base.GraphicsDevice.Viewport.Height / 2f + 60f), _playerManager.PlayersConnected[1].Colour(), Color.Black, Helper.OutlineType.Both, centered: true, 1f);
		if (!_wrestler.Active)
		{
			Helper.DrawOutlinedText(_spriteBatch, _font, _playerManager.PlayersConnected[_wrestler.Winner].Name + " wins", new Vector2((float)base.GraphicsDevice.Viewport.Width / 2f, (float)base.GraphicsDevice.Viewport.Height / 2f), _playerManager.PlayersConnected[_wrestler.Winner].Colour(), Color.Black, Helper.OutlineType.Both, (float)Math.Sin(_timePassed * 0.5f) * 0.5f, centered: true, 1f, new Vector2(0.95f, 0.95f) + Vector2.One * 0.05f * ((float)Math.Sin(_timePassed) + 1f));
		}
		_spriteBatch.Draw(_foreground, new Rectangle(0, 0, 1280, 720), Color.White);
		_spriteBatch.End();
		base.Draw(gameTime);
	}
}
