using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FiftyGames.MicroMachines.Entities;
using MicroMachinesGame;
using MicroMachinesGame.ISHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.MicroMachinesGame;

internal class MicroMachinesGame : Minigame
{
	private enum GameState
	{
		CountDown,
		Playing,
		Winner,
		Finished
	}

	private const float minZoom = 0.75f;

	private const float maxZoom = 1f;

	private const int countDownMax = 3000;

	private const int FRAME_RATE = 30;

	private GameState _gameState;

	private Texture2D _background;

	private bool _isDebugMode;

	private Rectangle _playerBoundingRect;

	private Rectangle levelRect;

	private Rectangle screenRect;

	private Camera camera;

	private RenderTarget2D _levelRT;

	private World _world;

	private WallMeshEditor wallMeshEditor;

	private Matrix _view;

	private Matrix _projection;

	private bool farseerDebugOverlayEnabled;

	private bool wallEditorEnabled;

	private List<MMPlayer> _players;

	private SpriteBatch _spriteBatch;

	private List<Player> _frameworkPlayers;

	private RenderTarget2D _skidRenderTarget;

	private int countDownMillsTimer = 3000;

	private SpriteFont _font;

	private List<TrackCheckpoint> _checkpoints = new List<TrackCheckpoint>();

	private SinglePixelTexture _scoreboardBackground;

	private SpriteFont _winnerFont;

	private List<MMPlayer> _finishedPlayers = new List<MMPlayer>();

	private static SoundManager _staticSoundManager;

	private bool _hasStartedCountdownTimer;

	private List<NosSkid> _nosSkids = new List<NosSkid>();

	private List<NosSmoke> _nosSmoke = new List<NosSmoke>();

	private int _previousFrame;

	private int _currentFrame;

	public MicroMachinesGame(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		_frameworkPlayers = playerManager.PlayersConnected;
		_players = new List<MMPlayer>();
		_staticSoundManager = soundManager;
		string[] cueNames = new string[5] { "microMachines Win", "microMachines Skid", "microMachines Crash", "microMachines Checkpoint", "microMachines CarEngine" };
		_staticSoundManager.PreloadSounds(cueNames);
	}

	public override void Initialize()
	{
		ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
		base.Initialize();
	}

	public override void Quit()
	{
		base.Quit();
	}

	protected override void LoadContent()
	{
		_background = _contentManager.Load<Texture2D>("MicroMachines/TheLevel");
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_winnerFont = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		GeometryHelper.InitLineRenderer(base.GraphicsDevice, _contentManager, new Rectangle(0, 0, 1920, 1080));
		levelRect = _background.Bounds;
		screenRect = new Rectangle(0, 0, 1280, 720);
		_skidRenderTarget = new RenderTarget2D(base.GraphicsDevice, 1920, 1280, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.PreserveContents);
		camera = new Camera(screenRect, levelRect, 0.75f, 1f);
		GeometryHelper.InitLineRenderer(base.GraphicsDevice, _contentManager, levelRect);
		_levelRT = new RenderTarget2D(base.GraphicsDevice, levelRect.Width, levelRect.Height);
		_world = new World(Vector2.Zero);
		wallMeshEditor = new WallMeshEditor(base.GraphicsDevice, _contentManager, _world);
		_players = new List<MMPlayer>();
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_view = Matrix.Identity;
		_projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(camera.GetRect().Width), ConvertUnits.ToSimUnits(camera.GetRect().Height), 0f, 0f, 1f);
		base.GraphicsDevice.SetRenderTarget(_skidRenderTarget);
		base.GraphicsDevice.Clear(Color.Transparent);
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		int num = 0;
		foreach (Player frameworkPlayer in _frameworkPlayers)
		{
			_players.Add(new MMPlayer(_world, frameworkPlayer, _contentManager, _spriteBatch, _skidRenderTarget, num, _frameworkPlayers.Count == 1, _finishedPlayers, _nosSkids, _nosSmoke));
			num++;
		}
		_checkpoints.Add(new TrackCheckpoint(new Vector2(1000f, 72f), new Vector2(1000f, 165f), isHorizontal: false, _world, 0, base.GraphicsDevice));
		_checkpoints.Add(new TrackCheckpoint(new Vector2(935f, 550f), new Vector2(1076f, 550f), isHorizontal: true, _world, 3, base.GraphicsDevice));
		_checkpoints.Add(new TrackCheckpoint(new Vector2(1625f, 541f), new Vector2(1717f, 541f), isHorizontal: true, _world, 1, base.GraphicsDevice));
		_checkpoints.Add(new TrackCheckpoint(new Vector2(1092f, 813f), new Vector2(1092f, 945f), isHorizontal: false, _world, 2, base.GraphicsDevice));
		_scoreboardBackground = new SinglePixelTexture(base.GraphicsDevice);
	}

	protected override void UnloadContent()
	{
		_contentManager.Unload();
		_contentManager.RootDirectory = "Content";
	}

	public void ClearSkidRT()
	{
		base.GraphicsDevice.SetRenderTarget(_skidRenderTarget);
		base.GraphicsDevice.Clear(Color.Transparent);
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
	}

	public override void Update(GameTime gameTime)
	{
		_previousFrame = _currentFrame;
		_currentFrame = Helper.AnimationFrame(30, gameTime.TotalGameTime.Milliseconds, 100);
		InputState.SetCurrentStates();
		if (_currentFrame != _previousFrame)
		{
			if (InputState.IsKeyDown(Keys.Space))
			{
				if (_isDebugMode)
				{
					_isDebugMode = false;
				}
				else
				{
					_isDebugMode = true;
				}
			}
			if (InputState.IsKeyDown(Keys.X))
			{
				if (wallEditorEnabled)
				{
					wallEditorEnabled = false;
				}
				else
				{
					wallEditorEnabled = true;
				}
			}
			if (InputState.IsKeyDown(Keys.C))
			{
				if (farseerDebugOverlayEnabled)
				{
					farseerDebugOverlayEnabled = false;
				}
				else
				{
					farseerDebugOverlayEnabled = true;
				}
			}
			float num = 10000f;
			float num2 = 10000f;
			float num3 = 0f;
			float num4 = 0f;
			foreach (MMPlayer player in _players)
			{
				if (player.IsAlive)
				{
					Vector2 displayPosition = player.DisplayPosition;
					if (displayPosition.X < num)
					{
						num = displayPosition.X;
					}
					if (displayPosition.Y < num2)
					{
						num2 = displayPosition.Y;
					}
					if (displayPosition.X > num3)
					{
						num3 = displayPosition.X;
					}
					if (displayPosition.Y > num4)
					{
						num4 = displayPosition.Y;
					}
				}
			}
			int num5 = 300;
			num -= (float)num5;
			num2 -= (float)num5;
			num3 += (float)num5;
			num4 += (float)num5;
			float num6 = num3 - num;
			float num7 = num4 - num2;
			_playerBoundingRect = new Rectangle((int)num, (int)num2, (int)num6, (int)num7);
			Vector2 destination = new Vector2(_playerBoundingRect.Center.X, _playerBoundingRect.Center.Y);
			float num8 = 1f - num6 / 1920f;
			float num9 = 1f - num7 / 1080f;
			float zoomLevel = ((!(num8 < num9)) ? MathHelper.Lerp(0.75f, 1.5f, num9) : MathHelper.Lerp(0.75f, 1.5f, num8));
			camera.MoveTo(destination, 5f);
			camera.ZoomTo(zoomLevel, 50f);
			camera.Update(gameTime);
			Vector2 mouseCoords = InputState.GetMouseCoords();
			if (!_isDebugMode)
			{
				mouseCoords.X = (float)camera.GetRect().Width / 1280f * mouseCoords.X;
				mouseCoords.Y = (float)camera.GetRect().Height / 720f * mouseCoords.Y;
				mouseCoords += camera.GetPosition();
			}
			else
			{
				mouseCoords.X = (float)levelRect.Width / 1280f * mouseCoords.X;
				mouseCoords.Y = (float)levelRect.Height / 720f * mouseCoords.Y;
			}
			if (wallEditorEnabled)
			{
				wallMeshEditor.Update(mouseCoords, _world);
			}
			if (_isDebugMode)
			{
				_view = Matrix.Identity;
				_projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(levelRect.Width), ConvertUnits.ToSimUnits(levelRect.Height), 0f, 0f, 1f);
			}
			else
			{
				_view = Matrix.CreateTranslation(new Vector3(ConvertUnits.ToSimUnits(-camera.GetPosition()), 0f));
				_projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(camera.GetRect().Width), ConvertUnits.ToSimUnits(camera.GetRect().Height), 0f, 0f, 1f);
			}
			for (int i = 0; i < _nosSkids.Count; i++)
			{
				_nosSkids[i].Update(gameTime);
				if (!_nosSkids[i].IsAlive)
				{
					_nosSkids.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < _nosSmoke.Count; j++)
			{
				_nosSmoke[j].Update(gameTime);
				if (!_nosSmoke[j].IsAlive)
				{
					_nosSmoke.RemoveAt(j);
					j--;
				}
			}
			switch (_gameState)
			{
			case GameState.CountDown:
				countDownMillsTimer -= gameTime.ElapsedGameTime.Milliseconds * 2;
				if (countDownMillsTimer < 0)
				{
					countDownMillsTimer = 3000;
					_gameState = GameState.Playing;
				}
				break;
			case GameState.Playing:
			{
				int num11 = 0;
				foreach (MMPlayer player2 in _players)
				{
					player2.Update(gameTime);
					if (!player2.IsRacing)
					{
						num11++;
					}
				}
				if (num11 == _players.Count)
				{
					_gameState = GameState.Winner;
				}
				if (!_hasStartedCountdownTimer && num11 > 0)
				{
					_hasStartedCountdownTimer = true;
					countDownMillsTimer = 20000;
				}
				else if (_hasStartedCountdownTimer)
				{
					countDownMillsTimer -= gameTime.ElapsedGameTime.Milliseconds * 2;
					if (countDownMillsTimer < 0)
					{
						countDownMillsTimer = 3000;
						_hasStartedCountdownTimer = false;
						_gameState = GameState.Winner;
					}
				}
				break;
			}
			case GameState.Winner:
				countDownMillsTimer = 3000;
				_hasStartedCountdownTimer = false;
				foreach (MMPlayer player3 in _players)
				{
					RaceStats raceStats = player3.GetRaceStats();
					if (_minigameMeta.BestScore < (float)raceStats.bestLap)
					{
						_minigameMeta.SetScore(raceStats.player.Name, (float)raceStats.bestLap / 1000f);
					}
				}
				foreach (Player item in _playerManager.PlayersConnected)
				{
					if (item.GamePadManager.ButtonWasPressed(Buttons.A))
					{
						_gameState = GameState.Finished;
						break;
					}
				}
				break;
			case GameState.Finished:
			{
				foreach (MMPlayer player4 in _players)
				{
					player4.OnDeath();
					player4.DestroyBody();
				}
				_players.Clear();
				int num10 = 0;
				foreach (Player frameworkPlayer in _frameworkPlayers)
				{
					_players.Add(new MMPlayer(_world, frameworkPlayer, _contentManager, _spriteBatch, _skidRenderTarget, num10, _frameworkPlayers.Count == 1, _finishedPlayers, _nosSkids, _nosSmoke));
					num10++;
				}
				_finishedPlayers.Clear();
				ClearSkidRT();
				_gameState = GameState.CountDown;
				break;
			}
			}
			foreach (TrackCheckpoint checkpoint in _checkpoints)
			{
				checkpoint.Update(gameTime);
			}
			_world.Step(1f / 30f);
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.SetRenderTarget(_skidRenderTarget);
		_spriteBatch.Begin();
		for (int i = 0; i < _players.Count; i++)
		{
			if (_players[i].IsBoosting)
			{
				_players[i].DrawSkid(fullAlpha: true);
			}
			else if (_players[i].IsSkiding)
			{
				_players[i].DrawSkid(fullAlpha: false);
			}
		}
		_spriteBatch.End();
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.SetRenderTarget(_levelRT);
		base.GraphicsDevice.Clear(Color.Black);
		_spriteBatch.Begin();
		_spriteBatch.Draw(_background, Vector2.Zero, Color.White);
		_spriteBatch.Draw(_skidRenderTarget, Vector2.Zero, Color.White);
		_spriteBatch.End();
		for (int j = 0; j < _nosSkids.Count; j++)
		{
			_nosSkids[j].Draw(_spriteBatch);
		}
		for (int k = 0; k < _nosSmoke.Count; k++)
		{
			_nosSmoke[k].Draw(_spriteBatch);
		}
		foreach (TrackCheckpoint checkpoint in _checkpoints)
		{
			checkpoint.Draw(_spriteBatch);
		}
		foreach (MMPlayer player in _players)
		{
			player.Draw(_spriteBatch);
		}
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		_spriteBatch.Begin();
		if (_isDebugMode)
		{
			_spriteBatch.Draw(_levelRT, screenRect, Color.White);
		}
		else
		{
			_spriteBatch.Draw(_levelRT, screenRect, camera.GetRect(), Color.White);
		}
		_spriteBatch.End();
		switch (_gameState)
		{
		case GameState.CountDown:
		{
			_spriteBatch.Begin();
			string text2 = (countDownMillsTimer / 1000 + 1).ToString();
			Helper.DrawOutlinedText(_spriteBatch, _font, text2, new Vector2(640f, 360f) - _font.MeasureString(text2) / 2f, Color.White, Color.Black);
			_spriteBatch.End();
			break;
		}
		case GameState.Playing:
			if (_hasStartedCountdownTimer)
			{
				_spriteBatch.Begin();
				string text = (countDownMillsTimer / 1000 + 1).ToString();
				Helper.DrawOutlinedText(_spriteBatch, _font, text, new Vector2(640f, 360f) - _font.MeasureString(text) / 2f, Color.White, Color.White);
				_spriteBatch.End();
			}
			break;
		case GameState.Winner:
		{
			_spriteBatch.Begin();
			_spriteBatch.Draw(_scoreboardBackground, screenRect, Color.Black * 0.7f);
			int top = _titleSafeArea.Top;
			Vector2 vector = _winnerFont.MeasureString("00.000");
			int num = (int)((float)_titleSafeArea.Height / vector.Y);
			num += (_titleSafeArea.Height - num * 9) / 9;
			int num2 = (int)((float)_titleSafeArea.Width / vector.X);
			num2 += (_titleSafeArea.Width - num2 * 5) / 5;
			int left = _titleSafeArea.Left;
			Helper.DrawOutlinedText(_spriteBatch, _winnerFont, "Player", new Vector2(left, top), Color.White, Color.Black);
			top += num;
			for (int l = 0; l < 5; l++)
			{
				Helper.DrawOutlinedText(_spriteBatch, _winnerFont, "Lap " + (l + 1) + ": ", new Vector2(left, top), Color.White, Color.Black);
				top += num;
			}
			Helper.DrawOutlinedText(_spriteBatch, _winnerFont, "Average: ", new Vector2(left, top), Color.White, Color.Black);
			top += num;
			Helper.DrawOutlinedText(_spriteBatch, _winnerFont, "Best: ", new Vector2(left, top), Color.White, Color.Black);
			top += num;
			Helper.DrawOutlinedText(_spriteBatch, _winnerFont, "Total: ", new Vector2(left, top), Color.White, Color.Black);
			left += num2;
			for (int m = 0; m < _finishedPlayers.Count; m++)
			{
				top -= num * 8;
				RaceStats raceStats = _finishedPlayers[m].GetRaceStats();
				float num3 = 1.7f;
				_spriteBatch.Draw(raceStats.player.Texture, new Rectangle(left, top, (int)((float)raceStats.player.Texture.Width * num3), (int)((float)raceStats.player.Texture.Height * num3)), Color.White);
				_spriteBatch.Draw(raceStats.player.TextureOverlay, new Rectangle(left, top, (int)((float)raceStats.player.Texture.Width * num3), (int)((float)raceStats.player.Texture.Height * num3)), raceStats.player.Color);
				_spriteBatch.Draw(raceStats.player.Texture, new Rectangle(left, top, (int)((float)raceStats.player.Texture.Width * num3), (int)((float)raceStats.player.Texture.Height * num3)), raceStats.player.Color * 0.3f);
				for (int n = 0; n < raceStats.lapTimes.Length; n++)
				{
					top += num;
					Helper.DrawOutlinedText(_spriteBatch, _winnerFont, ((float)raceStats.lapTimes[n] / 1000f).ToString("F3"), new Vector2(left, top), Color.White, Color.Black);
				}
				top += num;
				Helper.DrawOutlinedText(_spriteBatch, _winnerFont, ((float)raceStats.averageLapTime / 1000f).ToString("F3"), new Vector2(left, top), Color.White, Color.Black);
				top += num;
				Helper.DrawOutlinedText(_spriteBatch, _winnerFont, ((float)raceStats.bestLap / 1000f).ToString("F3"), new Vector2(left, top), Color.White, Color.Black);
				top += num;
				Helper.DrawOutlinedText(_spriteBatch, _winnerFont, ((float)raceStats.totalTime / 1000f).ToString("F3"), new Vector2(left, top), Color.White, Color.Black);
				left += num2;
			}
			_spriteBatch.End();
			break;
		}
		}
		if (!_isDebugMode)
		{
			if (wallEditorEnabled)
			{
				wallMeshEditor.Draw(_spriteBatch, -camera.GetPosition(), camera.GetRect());
			}
		}
		else if (wallEditorEnabled)
		{
			wallMeshEditor.Draw(_spriteBatch, Vector2.Zero, new Rectangle(0, 0, 1920, 1080));
		}
		InputState.SetPreviousStates();
		base.Draw(gameTime);
	}

	protected override void OnEnabledChanged(object sender, EventArgs args)
	{
		base.OnEnabledChanged(sender, args);
	}

	public static Cue PlaySound(string name)
	{
		Cue cue = _staticSoundManager.CreateGameSoundCue("microMachines " + name);
		cue.Play();
		return cue;
	}
}
