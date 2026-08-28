using System;
using System.Collections.Generic;
using System.IO;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FiftyGames.Zombie.DynamicLights;
using FiftyGames.Zombie.Editors;
using FiftyGames.Zombie.Entitys;
using FiftyGames.Zombie.Guns;
using FiftyGames.Zombie.Pickups;
using FiftyGames.Zombie.Rendering_Helpers;
using FiftyGames.Zombie.Utils;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Zombie;

internal class Zombie : Minigame
{
	private const int FRAME_RATE = 30;

	private MinigameMeta _minigame;

	private SpriteBatch spriteBatch;

	private SpriteFont _font;

	private NavMesh _navMesh;

	private NavMesh _wallMesh;

	private SinglePixelTexture _singlePixel;

	private Texture2D _background;

	private Texture2D _foreground;

	private Texture2D _occluderMap;

	private Texture2D _node;

	private Texture2D _arrow;

	private Texture2D _lightMap;

	private Vector2 _offset = Vector2.Zero;

	private Effect _finalPassEffect;

	private FullscreenQuad _finalPassQuad;

	// TEMPORARY diagnostic (round 2, black-screen investigation): dumps 4
	// intermediate render targets to RenderDiag/Zombie/ once, ~1.5s into the
	// first Draw() call after gameplay starts, to see which stage of the
	// pipeline is actually black. Remove once findings are extracted.
	private int _renderDiagFrameCounter;
	private bool _renderDiagDumped;
	private bool _renderDiagPostPassDumped;

	private void DumpRenderDiagIfDue()
	{
		if (_renderDiagDumped)
		{
			return;
		}
		_renderDiagFrameCounter++;
		if (_renderDiagFrameCounter < 90)
		{
			return;
		}
		_renderDiagDumped = true;
		try
		{
			string dir = System.IO.Path.Combine(AppContext.BaseDirectory, "RenderDiag", "Zombie");
			System.IO.Directory.CreateDirectory(dir);
			void Save(RenderTarget2D rt, string name)
			{
				using System.IO.FileStream stream = System.IO.File.Create(System.IO.Path.Combine(dir, name + ".png"));
				rt.SaveAsPng(stream, rt.Width, rt.Height);
			}
			Save(_aiRT, "1_aiRT");
			Save(_backgroundRT, "2_backgroundRT");
			Save(_finalShadowmap, "3_finalShadowmap");
			Save(_backbufferRT, "4_backbufferRT_final");
		}
		catch
		{
		}
	}

	private RenderTarget2D _finalShadowmap;

	private RenderTarget2D _bluredFinalShadowmap;

	private RenderTarget2D _aiRT;

	private RenderTarget2D _miniMap;

	private RenderTarget2D _backbufferRT;

	private RenderTarget2D _backgroundRT;

	private Light _light;

	private Light _light2;

	private List<Entity> _players;

	private List<BadGuy> _badguys;

	private DecalManager decalManager;

	private DecalManager playerBloodDecalManager;

	private WaveInfoDrawer _waveInfoDrawer;

	private bool _navMeshEditorEnabled;

	private bool _wallMeshEditorEnabled;

	private bool _lightMapEditorEnabled;

	private bool _showForeground;

	private bool _pickupEditorEnabled;

	private bool _waveManagerEnabled;

	private World world;

	private List<Body> _physWalls;

	private Vector4 _screenSideCollision = Vector4.Zero;

	private Line[] _screenLines;

	private Rectangle screen = default(Rectangle);

	private List<OffScreenArrow> _edgePoints = new List<OffScreenArrow>();

	private List<float> _edgeRotations = new List<float>();

	private List<Vector2> _hitLocations = new List<Vector2>();

	private List<Vector2> _hitNormals = new List<Vector2>();

	private List<int> _hitIDs = new List<int>();

	private Monitor monUpdate;

	private Monitor monDraw;

	private Random rand = new Random();

	private float _screenScale = 1f;

	private PickupEditor _pickupEditor;

	private LightMapEditor _lightMapEditor;

	private ShadowHelper2D _shadowHelper;

	private int _previousFrame;

	private int _currentFrame;

	private RotatingLight rotatingLight;

	private Matrix view;

	private Matrix projection;

	private bool _showFarseerDebugMode;

	private bool _showMyDebugInfo;

	private GameState gameState;

	private double countdown = 3000.0;

	private string infoText = "";

	private bool _updatePhysics;

	private bool _hasResetWaveManager;

	private bool _cameraIsAveraging = true;

	private List<WaveData> _loadedWaves = new List<WaveData>();

	private List<Player> _deadPlayers = new List<Player>();

	public Zombie(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		_minigame = minigame;
		string[] cueNames = new string[16]
		{
			"topZombies Step Sand", "topZombies Step Gravel", "topZombies Step Grass", "topZombies Shoot Submachinegun", "topZombies Shoot Shotgun", "topZombies Shoot Rifle", "topZombies Shoot Pistol", "topZombies Shoot Grenadelauncher", "topZombies Shoot Deagle", "topZombies Reload",
			"topZombies Rain", "topZombies Pick Up Gun", "topZombies Hit Zombie", "topZombies Hit Wall", "topZombies Explosion", "topZombies Collect Ammo"
		};
		_soundManager.PreloadSounds(cueNames);
		ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
		ZombieUtils.SoundManager = soundManager;
	}

	public override void Initialize()
	{
		world = new World(Vector2.Zero);
		_physWalls = new List<Body>();
		base.Initialize();
	}

	protected override void LoadContent()
	{
		ZombieUtils.ElapsedTime = 0L;
		ParticleEngine.InitEngine();
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		ZombieUtils.SetMemebers(base.GraphicsDevice, spriteBatch, _contentManager, world);
		GeometryHelper.InitLineRenderer(base.GraphicsDevice, _contentManager, new Rectangle(0, 0, 1280, 720));
		_shadowHelper = new ShadowHelper2D(ShadowMapSize._1024, 1280, 720);
		_navMesh = new NavMesh(200, new StreamReader("Content/Zombie/Data/waypoints.wpts").BaseStream);
		_wallMesh = new NavMesh(200);
		NavMeshEditor.SetNavMesh(_navMesh);
		WallMeshEditor.SetNavMesh(_wallMesh);
		ZombieUtils.NavMesh = _navMesh;
		ZombieUtils.WallMesh = _wallMesh;
		_singlePixel = new SinglePixelTexture(base.GraphicsDevice);
		ZombieUtils.SinglePixelTexture = _singlePixel;
		ZombieUtils.GlobalBadGuyList = new List<BadGuy>();
		ZombieUtils.DynamicLightMaskManager = new DynamicLightMaskManager();
		Color[] data = new Color[1];
		_singlePixel.GetData(data);
		LoadSettings();
		_badguys = new List<BadGuy>();
		_players = new List<Entity>();
		ZombieUtils.Players = _players;
		ZombieUtils.BadGuys = _badguys;
		decalManager = new DecalManager(base.GraphicsDevice, _contentManager, new Rectangle(0, 0, 1900, 1200), new Rectangle(0, 0, 100, 100), _contentManager.Load<Texture2D>("Zombie/ZombieBlood1"));
		playerBloodDecalManager = new DecalManager(base.GraphicsDevice, _contentManager, new Rectangle(0, 0, 1900, 1200), new Rectangle(0, 0, 100, 100), _contentManager.Load<Texture2D>("Zombie/ParticleSprites/Blood1"));
		ZombieUtils.DecalManager = decalManager;
		ZombieUtils.PlayerDecalManager = playerBloodDecalManager;
		_finalShadowmap = new RenderTarget2D(base.GraphicsDevice, 1280, 720, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		_bluredFinalShadowmap = new RenderTarget2D(base.GraphicsDevice, 1280, 720, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		_backbufferRT = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		_aiRT = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		_miniMap = new RenderTarget2D(base.GraphicsDevice, 1900, 1200);
		_backgroundRT = new RenderTarget2D(base.GraphicsDevice, 1900, 1200);
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_light = new Light(new Vector2(640f, 300f), 10f, _contentManager.Load<Texture2D>("Zombie/FlashLight"), _contentManager.Load<Texture2D>("Zombie/circlelight"), 0f, Vector2.One);
		_light2 = new Light(new Vector2(640f, 300f), 10f, _contentManager.Load<Texture2D>("Zombie/FlashLight"), _contentManager.Load<Texture2D>("Zombie/RealCircleLight"), 0f, Vector2.One);
		_background = _contentManager.Load<Texture2D>("Zombie/BackGround");
		_occluderMap = _contentManager.Load<Texture2D>("Zombie/MidGround");
		_foreground = _contentManager.Load<Texture2D>("Zombie/ForeGround");
		_node = _contentManager.Load<Texture2D>("Zombie/Node");
		_arrow = _contentManager.Load<Texture2D>("Zombie/arrow");
		_finalPassEffect = _contentManager.Load<Effect>("Zombie/FinalPassEffect");
		_finalPassQuad = new FullscreenQuad(base.GraphicsDevice);
		_lightMapEditor = new LightMapEditor(base.GraphicsDevice, _contentManager, _occluderMap);
		_lightMap = _contentManager.Load<Texture2D>("Zombie/Maps/latestBake");
		Color[] data2 = new Color[_lightMap.Width * _lightMap.Height];
		_lightMap.GetData(data2);
		_lightMapEditor.LightMap.SetData(data2);
		_pickupEditor = new PickupEditor();
		_screenLines = new Line[4];
		for (int i = 0; i < 4; i++)
		{
			_screenLines[i] = new Line();
		}
		_screenLines[0].Start = Vector2.Zero;
		_screenLines[0].End = new Vector2(0f, 720f);
		_screenLines[1].Start = Vector2.Zero;
		_screenLines[1].End = new Vector2(1280f, 0f);
		_screenLines[2].Start = new Vector2(1280f, 0f);
		_screenLines[2].End = new Vector2(1280f, 720f);
		_screenLines[3].Start = new Vector2(0f, 720f);
		_screenLines[3].End = new Vector2(1280f, 720f);
		monUpdate = new Monitor(base.GraphicsDevice, _contentManager, 256, 144, 10f, Color.White, 0.2f);
		monDraw = new Monitor(base.GraphicsDevice, _contentManager, 256, 144, 10f, Color.White, 0.2f);
		Konsole.LoadContent(base.GraphicsDevice, _contentManager);
		rotatingLight = new RotatingLight(_contentManager, new Vector2(640f, 480f));
		view = Matrix.Identity;
		projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(1280), ConvertUnits.ToSimUnits(720), 0f, 0f, 1f);
		WaveManager.OnNewWave = OnNewWave;
		WaveManager.OnStart = OnWaveStart;
		gameState = GameState.Countdown;
		_waveInfoDrawer = new WaveInfoDrawer(base.GraphicsDevice, _contentManager);
		ZombieUtils.DefaultZombieGotoPosition = new Vector2(1111f, 550f);
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerBodyBackground");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerBodyForeground");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerSmallGunHands");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerBigGunHands");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ParticleSprites/Spark");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ParticleSprites/BloodShoot");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Node");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart1");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart2");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart3");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart4");
		ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart5");
		base.LoadContent();
	}

	public override void Quit()
	{
		_framework.VSync = true;
		ZombieUtils.SetMembersToNull();
	}

	protected override void UnloadContent()
	{
		base.UnloadContent();
	}

	public override void Update(GameTime gameTime)
	{
		if (_framework.VSync)
		{
			_framework.VSync = false;
		}
		string text = "";
		for (int i = 0; i < _playerManager.PlayersConnected.Count; i++)
		{
			text += _playerManager.PlayersConnected[i].Name;
			if (i + 1 < _playerManager.PlayersConnected.Count)
			{
				text += " + ";
			}
		}
		if (_minigame.BestScore < (float)(WaveManager.CurrentWave - 1))
		{
			_minigame.SetScore(text, WaveManager.CurrentWave);
		}
		ZombieUtils.ElapsedTime += gameTime.ElapsedGameTime.Milliseconds;
		_previousFrame = _currentFrame;
		_currentFrame = Helper.AnimationFrame(30, gameTime.TotalGameTime.Milliseconds, 100);
		if (_currentFrame != _previousFrame)
		{
			ZombieUtils.GameTime = gameTime;
			InputState.SetCurrentStates();
			if (_waveManagerEnabled)
			{
				WaveManager.Update(gameTime, _badguys);
			}
			int num = 10;
			if (InputState.KeyboardStateChanged())
			{
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.C))
				{
					if (!_navMeshEditorEnabled)
					{
						_navMeshEditorEnabled = true;
					}
					else
					{
						_navMeshEditorEnabled = false;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.X))
				{
					if (!_wallMeshEditorEnabled)
					{
						_wallMeshEditorEnabled = true;
					}
					else
					{
						_wallMeshEditorEnabled = false;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.V))
				{
					if (!_showForeground)
					{
						_showForeground = true;
					}
					else
					{
						_showForeground = false;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Z))
				{
					if (!_lightMapEditorEnabled)
					{
						_lightMapEditorEnabled = true;
					}
					else
					{
						_lightMapEditorEnabled = false;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.B))
				{
					if (!_pickupEditorEnabled)
					{
						_pickupEditorEnabled = true;
					}
					else
					{
						_pickupEditorEnabled = false;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.N))
				{
					LoadSettings();
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D))
				{
					if (_showFarseerDebugMode)
					{
						_showFarseerDebugMode = false;
					}
					else
					{
						_showFarseerDebugMode = true;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.W))
				{
					if (_waveManagerEnabled)
					{
						_waveManagerEnabled = false;
					}
					else
					{
						_waveManagerEnabled = true;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.M))
				{
					if (_showMyDebugInfo)
					{
						_showMyDebugInfo = false;
					}
					else
					{
						_showMyDebugInfo = true;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.P))
				{
					if (ZombieUtils.UseSound)
					{
						ZombieUtils.UseSound = false;
					}
					else
					{
						ZombieUtils.UseSound = true;
					}
				}
				if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.A))
				{
					WaveManager.AdvanceWave();
				}
			}
			if (_navMeshEditorEnabled || _wallMeshEditorEnabled || _lightMapEditorEnabled || _pickupEditorEnabled)
			{
				if (Keyboard.GetState().IsKeyDown(Keys.W))
				{
					_offset.Y += num;
				}
				if (Keyboard.GetState().IsKeyDown(Keys.S))
				{
					_offset.Y -= num;
				}
				if (Keyboard.GetState().IsKeyDown(Keys.A))
				{
					_offset.X += num;
				}
				if (Keyboard.GetState().IsKeyDown(Keys.D))
				{
					_offset.X -= num;
				}
			}
			if (_navMeshEditorEnabled)
			{
				NavMeshEditor.Update(_offset);
			}
			if (_wallMeshEditorEnabled)
			{
				WallMeshEditor.Update(_offset, world, _physWalls);
			}
			if (_lightMapEditorEnabled)
			{
				_lightMapEditor.Update(_offset);
			}
			if (_pickupEditorEnabled)
			{
				_pickupEditor.Update();
			}
			Vector2 vector = GetPlayersAveragePosition() - new Vector2(640f, 360f);
			screen = new Rectangle((int)vector.X, (int)vector.Y, 1280, 720);
			int num2 = 0;
			Rectangle titleSafeArea = _titleSafeArea;
			titleSafeArea.X = (int)(0f - _offset.X) + _titleSafeArea.X;
			titleSafeArea.Y = (int)(0f - _offset.Y) + _titleSafeArea.Y;
			_screenLines[0].Start = new Vector2(titleSafeArea.X, titleSafeArea.Y);
			_screenLines[0].End = new Vector2(titleSafeArea.X, titleSafeArea.Y + titleSafeArea.Height);
			_screenLines[1].Start = new Vector2(titleSafeArea.X, titleSafeArea.Y);
			_screenLines[1].End = new Vector2(titleSafeArea.X + titleSafeArea.Width, titleSafeArea.Y);
			_screenLines[2].Start = new Vector2(titleSafeArea.X + titleSafeArea.Width, titleSafeArea.Y);
			_screenLines[2].End = new Vector2(titleSafeArea.X + titleSafeArea.Width, titleSafeArea.Y + titleSafeArea.Height);
			_screenLines[3].Start = new Vector2(titleSafeArea.X + num2, titleSafeArea.Y + titleSafeArea.Height);
			_screenLines[3].End = new Vector2(titleSafeArea.X + titleSafeArea.Width, titleSafeArea.Y + titleSafeArea.Height);
			List<float> list = new List<float>();
			List<Line> list2 = new List<Line>();
			List<Color> list3 = new List<Color>();
			for (int j = 0; j < _players.Count; j++)
			{
				if (!(_players[j] is ZombiePlayerBadGuy) && !titleSafeArea.Contains((int)_players[j].Position.X, (int)_players[j].Position.Y))
				{
					Line line = new Line();
					line.Start = new Vector2(screen.X + 640, screen.Y + 360);
					line.End = _players[j].Position;
					list2.Add(line);
					list3.Add(((ZombiePlayer)_players[j]).FrameworkPlayer.Colour(0.6f, 0.3f));
					Vector2 vector2 = line.Start - line.End;
					vector2.Normalize();
					list.Add((float)Math.Atan2(vector2.Y * -1f, vector2.X) + (float)Math.PI / 2f);
				}
			}
			_edgePoints.Clear();
			_edgeRotations.Clear();
			for (int k = 0; k < 4; k++)
			{
				for (int l = 0; l < list2.Count; l++)
				{
					Vector2 vector3 = GeometryHelper.ProcessIntersection(_screenLines[k], list2[l]);
					_ = list[l];
					Vector2 vector4 = list2[l].Start - vector3;
					vector4.Normalize();
					_edgeRotations.Add((float)Math.Atan2(vector4.Y, vector4.X) - (float)Math.PI / 2f);
					OffScreenArrow item = new OffScreenArrow
					{
						position = vector3,
						color = list3[l]
					};
					_edgePoints.Add(item);
				}
			}
			MoveCamera(new Vector2(640f, 360f) - (new Vector2(screen.X, screen.Y) + new Vector2(640f, 360f)));
			if (_offset.X > 0f)
			{
				_offset.X = 0f;
			}
			if (_offset.Y > 0f)
			{
				_offset.Y = 0f;
			}
			if (_offset.X < -620f)
			{
				_offset.X = -620f;
			}
			if (_offset.Y < -480f)
			{
				_offset.Y = -480f;
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.H))
			{
				_lightMapEditor.BakeToTextureFile(spriteBatch);
			}
			_hitLocations.Clear();
			for (int m = 0; m < _badguys.Count; m++)
			{
				_badguys[m].Update();
			}
			for (int n = 0; n < _players.Count; n++)
			{
				ZombiePlayer zombiePlayer = _players[n] as ZombiePlayer;
				if (zombiePlayer != null)
				{
					zombiePlayer.Update(gameTime, _badguys, _hitLocations);
				}
				else
				{
					ZombiePlayerBadGuy zombiePlayerBadGuy = _players[n] as ZombiePlayerBadGuy;
					zombiePlayerBadGuy.Update();
				}
				if (_players[n].IsAlive)
				{
					continue;
				}
				if (zombiePlayer != null)
				{
					if (zombiePlayer.Health <= 0f)
					{
						ZombieUtils.DefaultZombieGotoPosition = zombiePlayer.Position;
						zombiePlayer.OnExplosion();
						zombiePlayer.Dispose();
						_deadPlayers.Add(zombiePlayer.FrameworkPlayer);
						_players[n] = null;
					}
					else
					{
						_players[n] = null;
					}
				}
				else
				{
					ZombiePlayerBadGuy zombiePlayerBadGuy2 = _players[n] as ZombiePlayerBadGuy;
					int index = ZombieUtils.NavMesh.LineMesh.SpecialNodes[ZombieUtils.Random.Next(0, ZombieUtils.NavMesh.LineMesh.SpecialNodes.Count)];
					_players[n] = new ZombiePlayerBadGuy(zombiePlayerBadGuy2.FrameworkPlayer, ZombieUtils.NavMesh.LineMesh.MeshNodes[index]._position);
				}
			}
			for (int num3 = 0; num3 < _players.Count; num3++)
			{
				if (_players[num3] == null)
				{
					_players.RemoveAt(num3);
					num3--;
				}
			}
			if (_players.Count <= 0 && gameState == GameState.Playing)
			{
				countdown = 3000.0;
				ZombieUtils.DecalManager.RemoveAllDecals();
				ZombieUtils.PlayerDecalManager.RemoveAllDecals();
				_hasResetWaveManager = false;
				ParticleEngine.DestroyAllEmitters();
				gameState = GameState.AllDead;
			}
			ProjectileManager.Update(gameTime);
			InputState.SetPreviousStates();
			if (_updatePhysics)
			{
				world.Step(1f / 30f);
			}
			ZombieUtils.Offset = _offset;
			view = Matrix.CreateTranslation(new Vector3(ConvertUnits.ToSimUnits(_offset), 0f));
		}
		switch (gameState)
		{
		case GameState.Countdown:
			countdown -= gameTime.ElapsedGameTime.Milliseconds;
			infoText = (1 + (int)countdown / 1000).ToString();
			if (countdown < 0.0)
			{
				gameState = GameState.Playing;
				_waveManagerEnabled = true;
				_updatePhysics = true;
				ResetPlayers();
			}
			break;
		case GameState.Playing:
			infoText = "Wave " + WaveManager.CurrentWave;
			if (WaveManager.Completed && _badguys.Count <= 0)
			{
				DestroyPlayers();
				ZombieUtils.DecalManager.RemoveAllDecals();
				ZombieUtils.PlayerDecalManager.RemoveAllDecals();
				gameState = GameState.Won;
			}
			break;
		case GameState.AllDead:
			infoText = "Dead";
			if (countdown == 3000.0)
			{
				DestroyPlayers();
				_deadPlayers.Clear();
				ParticleEngine.DestroyAllEmitters();
				if (ZombieUtils.BadGuys != null)
				{
					for (int num5 = 0; num5 < ZombieUtils.BadGuys.Count; num5++)
					{
						ZombieUtils.BadGuys[num5].IsAlive = false;
						ZombieUtils.BadGuys[num5].Health = 0f;
						ZombieUtils.BadGuys[num5].Update();
					}
					ZombieUtils.BadGuys.Clear();
				}
			}
			if (countdown < 2500.0 && !_hasResetWaveManager)
			{
				ResetWaveManager();
				_waveManagerEnabled = false;
				_hasResetWaveManager = true;
			}
			countdown -= gameTime.ElapsedGameTime.Milliseconds;
			if (countdown < 0.0)
			{
				countdown = 3000.0;
				_updatePhysics = true;
				gameState = GameState.Countdown;
			}
			break;
		case GameState.Won:
		{
			infoText = "Press A";
			for (int num4 = 0; num4 < _playerManager.PlayersConnected.Count; num4++)
			{
				if (_playerManager.GetGamePad(PlayerIndex.One).GamePadStateCurrent.IsButtonDown(Buttons.A))
				{
					ResetWaveManager();
					_waveManagerEnabled = false;
					countdown = 3000.0;
					_updatePhysics = false;
					gameState = GameState.Countdown;
					break;
				}
			}
			break;
		}
		}
		ParticleEngine.Update();
		ZombieUtils.DynamicLightMaskManager.Update(gameTime);
		ZombieUtils.PlayerDecalManager.ApplyChanges();
		ZombieUtils.DecalManager.ApplyChanges();
		base.Update(gameTime);
	}

	private void DestroyPlayers()
	{
		if (_players == null)
		{
			return;
		}
		for (int i = 0; i < _players.Count; i++)
		{
			if (_players[i] is ZombiePlayer)
			{
				ZombiePlayer zombiePlayer = (ZombiePlayer)_players[i];
				zombiePlayer.Dispose();
			}
			else if (_players[i] is ZombiePlayerBadGuy)
			{
				ZombiePlayerBadGuy zombiePlayerBadGuy = (ZombiePlayerBadGuy)_players[i];
				zombiePlayerBadGuy.Health = 0f;
				zombiePlayerBadGuy.CheckDeath();
			}
		}
		_players.Clear();
	}

	private void ResetPlayers()
	{
		_deadPlayers.Clear();
		DestroyPlayers();
		if (_players != null)
		{
			for (int i = 0; i < _playerManager.PlayersConnected.Count; i++)
			{
				ZombiePlayer item = new ZombiePlayer(_playerManager.PlayersConnected[i], new Vector2(1000f, 550f), i);
				_players.Add(item);
			}
		}
	}

	private Vector2 GetPlayersAveragePosition()
	{
		Vector2 result = Vector2.Zero;
		int num = 0;
		for (int i = 0; i < _players.Count; i++)
		{
			if (!(_players[i] is ZombiePlayerBadGuy))
			{
				result += _players[i].Position;
				num++;
			}
		}
		if (_cameraIsAveraging)
		{
			if (_players.Count == 0)
			{
				result = new Vector2(1111f, 550f);
			}
			else
			{
				result /= (float)num;
			}
		}
		else
		{
			result = new Vector2(1111f, 550f);
		}
		return result;
	}

	private void OnNewWave()
	{
		foreach (Player deadPlayer in _deadPlayers)
		{
			ZombiePlayer item = new ZombiePlayer(deadPlayer, new Vector2(1111f, 550f), 0);
			_players.Add(item);
		}
		_deadPlayers.Clear();
		Rectangle rect = new Rectangle(880, 450, 485, 200);
		ZombieUtils.DecalManager.RemoveDecalsInArea(rect);
		ZombieUtils.PlayerDecalManager.RemoveDecalsInArea(rect);
		_cameraIsAveraging = false;
	}

	private void OnWaveStart()
	{
		_cameraIsAveraging = true;
	}

	public void MoveCamera(Vector2 newPosition)
	{
		Vector2 vector = newPosition - _offset;
		vector.Normalize();
		float num = Vector2.Distance(newPosition, _offset);
		float num2 = 4f;
		if (!_cameraIsAveraging)
		{
			num2 = 8f;
		}
		float num3 = num / num2;
		if (!float.IsNaN(vector.X) && !float.IsNaN(vector.Y))
		{
			if (Vector2.Distance(_offset + vector * num3, newPosition) < num3)
			{
				_offset = newPosition;
			}
			else
			{
				_offset += vector * num3;
			}
		}
	}

	private void LoadSettings()
	{
		_waveManagerEnabled = false;
		WaveManager.Init();
		StreamReader streamReader = new StreamReader("Content/Zombie/Data/settings.zs");
		BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
		ZombieBadGuy.Settings = LoadAISettings(binaryReader);
		ZombieBadGuy2.Settings = LoadAISettings(binaryReader);
		CrawlerBadGuy.Settings = LoadAISettings(binaryReader);
		CrawlerBadGuy2.Settings = LoadAISettings(binaryReader);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			WaveData item = default(WaveData);
			List<Pickup> list = new List<Pickup>();
			int num2 = binaryReader.ReadInt32();
			int num3 = binaryReader.ReadInt32();
			if (num3 != 0)
			{
				list.Add(new DeaglePickup(null, Vector2.Zero, num3, num2, dummy: true));
			}
			item.deagleAmmo = num2;
			item.deagleProb = num3;
			num2 = binaryReader.ReadInt32();
			num3 = binaryReader.ReadInt32();
			if (num3 != 0)
			{
				list.Add(new GrenadeLauncherPickup(null, Vector2.Zero, num3, num2, dummy: true));
			}
			item.grenadeLauncherAmmo = num2;
			item.grenadeLauncherProb = num3;
			num2 = binaryReader.ReadInt32();
			num3 = binaryReader.ReadInt32();
			if (num3 != 0)
			{
				list.Add(new M4Pickup(null, Vector2.Zero, num3, num2, dummy: true));
			}
			item.m4Ammo = num2;
			item.m4Prob = num3;
			num2 = binaryReader.ReadInt32();
			num3 = binaryReader.ReadInt32();
			if (num3 != 0)
			{
				list.Add(new ShotgunPickup(null, Vector2.Zero, num3, num2, dummy: true));
			}
			item.shotgunAmmo = num2;
			item.showgunProb = num3;
			num2 = binaryReader.ReadInt32();
			num3 = binaryReader.ReadInt32();
			if (num3 != 0)
			{
				list.Add(new SubmachineGunPickup(null, Vector2.Zero, num3, num2, dummy: true));
			}
			item.submachineGunAmmo = num2;
			item.submachineGunProb = num3;
			int num4 = binaryReader.ReadInt32();
			int numberPerSpawn = binaryReader.ReadInt32();
			int timeBetweenSpawns = binaryReader.ReadInt32();
			int durationOfWave = binaryReader.ReadInt32();
			bool ignoreDuration = binaryReader.ReadBoolean();
			int num5 = binaryReader.ReadInt32();
			int num6 = binaryReader.ReadInt32();
			int num7 = binaryReader.ReadInt32();
			int num8 = binaryReader.ReadInt32();
			int num9 = binaryReader.ReadInt32();
			int maxPickups = binaryReader.ReadInt32();
			item.startingZombies = num4;
			item.numberPerSpawn = numberPerSpawn;
			item.timeBetweenSpawns = timeBetweenSpawns;
			item.durationOfWave = durationOfWave;
			item.ignoreDuration = ignoreDuration;
			item.pickupSpawnTime = num5;
			item.numZombieType1 = num6;
			item.numZombieType2 = num7;
			item.numCrawlerType1 = num8;
			item.numCrawlerType2 = num9;
			item.maxPickups = maxPickups;
			Dictionary<BadGuy, int> dictionary = new Dictionary<BadGuy, int>();
			if (num6 != 0)
			{
				dictionary.Add(new ZombieBadGuy(), num6);
			}
			if (num7 != 0)
			{
				dictionary.Add(new ZombieBadGuy2(), num7);
			}
			if (num8 != 0)
			{
				dictionary.Add(new CrawlerBadGuy(), num8);
			}
			if (num9 != 0)
			{
				dictionary.Add(new CrawlerBadGuy2(), num9);
			}
			Wave wave = new Wave(dictionary, timeBetweenSpawns, num4, durationOfWave, ignoreDuration, numberPerSpawn, list, num5, maxPickups);
			WaveManager.AddWave(wave);
			_loadedWaves.Add(item);
		}
		Deagle.Settings = LoadGunSettings(binaryReader);
		GrenadeLauncher.Settings = LoadGunSettings(binaryReader);
		M4.Settings = LoadGunSettings(binaryReader);
		Pistol.Settings = LoadGunSettings(binaryReader);
		Shotgun.Settings = LoadGunSettings(binaryReader);
		SubmachineGun.Settings = LoadGunSettings(binaryReader);
		MiscSettings miscSettings = new MiscSettings();
		miscSettings.NodeRadiusSpawnLimit = binaryReader.ReadInt32();
		miscSettings.PlayerHealth = binaryReader.ReadInt32();
		miscSettings.PlayerHealthRecoveryAmount = binaryReader.ReadInt32();
		miscSettings.PlayerSpeed = binaryReader.ReadInt32();
		miscSettings.PlayerZombieDamage = binaryReader.ReadInt32();
		miscSettings.PlayerZombieHealth = binaryReader.ReadInt32();
		miscSettings.PlayerZombieSpeed = binaryReader.ReadInt32();
		miscSettings.PlayerHealthTimeUntilRecovery = binaryReader.ReadInt32();
		miscSettings.GrenadeKillRadius = binaryReader.ReadInt32();
		miscSettings.ExplosionShudderTimer = binaryReader.ReadInt32();
		ZombieUtils.MiscSettings = miscSettings;
		ZombieUtils.SpawnDistance = miscSettings.NodeRadiusSpawnLimit;
		binaryReader.Close();
	}

	private void ResetWaveManager()
	{
		_waveManagerEnabled = false;
		WaveManager.Init();
		for (int i = 0; i < _loadedWaves.Count; i++)
		{
			WaveData waveData = _loadedWaves[i];
			List<Pickup> list = new List<Pickup>();
			if (waveData.deagleProb != 0)
			{
				list.Add(new DeaglePickup(null, Vector2.Zero, waveData.deagleProb, waveData.deagleAmmo, dummy: true));
			}
			if (waveData.grenadeLauncherProb != 0)
			{
				list.Add(new GrenadeLauncherPickup(null, Vector2.Zero, waveData.grenadeLauncherProb, waveData.grenadeLauncherAmmo, dummy: true));
			}
			if (waveData.m4Prob != 0)
			{
				list.Add(new M4Pickup(null, Vector2.Zero, waveData.m4Prob, waveData.m4Ammo, dummy: true));
			}
			if (waveData.showgunProb != 0)
			{
				list.Add(new ShotgunPickup(null, Vector2.Zero, waveData.showgunProb, waveData.shotgunAmmo, dummy: true));
			}
			if (waveData.submachineGunProb != 0)
			{
				list.Add(new SubmachineGunPickup(null, Vector2.Zero, waveData.submachineGunProb, waveData.submachineGunAmmo, dummy: true));
			}
			Dictionary<BadGuy, int> dictionary = new Dictionary<BadGuy, int>();
			if (waveData.numZombieType1 != 0)
			{
				dictionary.Add(new ZombieBadGuy(), waveData.numZombieType1);
			}
			if (waveData.numZombieType2 != 0)
			{
				dictionary.Add(new ZombieBadGuy2(), waveData.numZombieType2);
			}
			if (waveData.numCrawlerType1 != 0)
			{
				dictionary.Add(new CrawlerBadGuy(), waveData.numCrawlerType1);
			}
			if (waveData.numCrawlerType2 != 0)
			{
				dictionary.Add(new CrawlerBadGuy2(), waveData.numCrawlerType2);
			}
			Wave wave = new Wave(dictionary, waveData.timeBetweenSpawns, waveData.startingZombies, waveData.durationOfWave, waveData.ignoreDuration, waveData.numberPerSpawn, list, waveData.pickupSpawnTime, waveData.maxPickups);
			WaveManager.AddWave(wave);
		}
	}

	private GunSettings LoadGunSettings(BinaryReader br)
	{
		GunSettings gunSettings = new GunSettings();
		gunSettings.BulletDamage = br.ReadInt32();
		gunSettings.EndOfGunPosition = new Vector2(br.ReadSingle(), br.ReadSingle());
		gunSettings.HasPenertratingPower = br.ReadBoolean();
		gunSettings.IsBigGun = br.ReadBoolean();
		gunSettings.MagazineSize = br.ReadInt32();
		gunSettings.MuzzleType = br.ReadInt32();
		gunSettings.PlayerKickbackImpulseMultiplier = new Vector2(br.ReadSingle(), br.ReadSingle());
		gunSettings.PlayerKickRotation = br.ReadInt32();
		gunSettings.ShootInterval = br.ReadInt32();
		gunSettings.ShotLength = br.ReadInt32();
		gunSettings.ShotsAtOnce = br.ReadInt32();
		gunSettings.SpreadAngle = br.ReadInt32();
		gunSettings.VibrationPerShot = br.ReadInt32();
		return gunSettings;
	}

	private AISettings LoadAISettings(BinaryReader br)
	{
		AISettings aISettings = new AISettings();
		aISettings.Damage = br.ReadInt32();
		aISettings.Health = br.ReadInt32();
		aISettings.Speed = br.ReadInt32();
		aISettings.TurnSpeed = br.ReadSingle();
		aISettings.KillPoints = br.ReadInt32();
		aISettings.UsePath = br.ReadBoolean();
		return aISettings;
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.Black);
		if (_lightMapEditorEnabled)
		{
			_lightMapEditor.BakeLights(spriteBatch, _lightMap);
		}
		base.GraphicsDevice.SetRenderTarget(_finalShadowmap);
		base.GraphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		spriteBatch.Draw(_lightMapEditor.LightMap, _offset, Color.White);
		spriteBatch.End();
		ZombieUtils.DynamicLightMaskManager.Draw(spriteBatch, _offset);
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		_waveInfoDrawer.BeginRTDraw(spriteBatch, infoText);
		for (int i = 0; i < _players.Count; i++)
		{
			float num = ((_players[i] is ZombiePlayer zombiePlayer) ? zombiePlayer._wobbleRotation : 0f);
			_light.Position = _offset + _players[i].Position;
			_light.MaskRotation = _players[i].Rotation + num / 7f + (float)Math.PI / 2f;
			_shadowHelper.StartDrawingOccluders(_light);
			spriteBatch.Begin();
			spriteBatch.Draw(_occluderMap, _offset, Color.White);
			spriteBatch.End();
			_shadowHelper.EndDrawingOccluders(spriteBatch, _finalShadowmap, Color.White, blur: false, BlendState.AlphaBlend);
		}
		if (_players.Count <= 0)
		{
			_light.Position = new Vector2(-200f, -200f);
			_shadowHelper.StartDrawingOccluders(_light);
			_shadowHelper.EndDrawingOccluders(spriteBatch, _finalShadowmap, Color.White, blur: false, BlendState.AlphaBlend);
		}
		base.GraphicsDevice.Clear(Color.Black);
		BlendState blendState = base.GraphicsDevice.BlendState;
		base.GraphicsDevice.SetRenderTarget(_aiRT);
		base.GraphicsDevice.Clear(Color.Transparent);
		base.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		WaveManager.Draw();
		ProjectileManager.Draw(spriteBatch, _offset);
		for (int j = 0; j < _badguys.Count; j++)
		{
			_badguys[j].Draw(drawDebug: false);
		}
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.SetRenderTarget(_backgroundRT);
		base.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		spriteBatch.Draw(_background, Vector2.Zero, Color.White);
		spriteBatch.End();
		_waveInfoDrawer.Draw(gameTime, spriteBatch);
		decalManager.Render(Vector2.Zero);
		playerBloodDecalManager.Render(Vector2.Zero);
		ParticleEngine.Draw(spriteBatch, Vector2.Zero, Vector2.One);
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.BlendState = blendState;
		base.GraphicsDevice.SetRenderTarget(_backbufferRT);
		base.GraphicsDevice.Clear(Color.Black);
		_shadowHelper.ShadowEffect.Parameters["InputTexture"].SetValue(_backgroundRT);
		_shadowHelper.ShadowEffect.Parameters["ShadowMapTexture"].SetValue(_finalShadowmap);
		_shadowHelper.ShadowEffect.Parameters["InLightMap"].SetValue(_aiRT);
		// FIX (round 8, "roof textures" bug): _backgroundRT is a 1900x1200
		// world-space canvas sampled 0..1 by ApplyShadowMapPS -- with no offset
		// it never scrolled and was squashed to fit 1280x720, so the roof/
		// midground overlay (which correctly scrolls with _offset) appeared too
		// small and to slide around with the player relative to it. See the
		// shader's own header comment on BackgroundOffset for the full story.
		_shadowHelper.ShadowEffect.Parameters["BackgroundOffset"]?.SetValue(_offset);
		// FIX (round 7, light-offset bug): removed. ShadowHelper2DEffect.fx's
		// "Offset" parameter used to re-shift the shadow-map sample by _offset a
		// second time, on top of _offset already being baked into where
		// _finalShadowmap was drawn -- displacing the shadow/light circle away
		// from the player. The shader no longer reads this parameter at all (see
		// its own header comment), so setting it here would now be a no-op; left
		// removed rather than converted to a guarded ?.SetValue() since it has no
		// remaining purpose.
		_shadowHelper.ShadowEffect.Parameters["BackgroundDarkness"].SetValue(0.1f);
		_shadowHelper.ShadowEffect.CurrentTechnique = _shadowHelper.ShadowEffect.Techniques["ApplyShadowMap"];
		_shadowHelper.ShadowEffect.CurrentTechnique.Passes[0].Apply();
		_shadowHelper.QuadDrawer.Render(Vector2.One * -1f, Vector2.One);
		spriteBatch.Begin();
		spriteBatch.Draw(_occluderMap, _offset, new Color(0.35f, 0.35f, 0.35f, 1f));
		spriteBatch.End();
		spriteBatch.Begin();
		for (int k = 0; k < _edgePoints.Count; k++)
		{
			Vector2 position = _edgePoints[k].position + _offset;
			spriteBatch.Draw(_arrow, position, null, _edgePoints[k].color, _edgeRotations[k] + (float)Math.PI, new Vector2(40f, 40f), 1f, SpriteEffects.None, 0f);
		}
		spriteBatch.End();
		for (int l = 0; l < _players.Count; l++)
		{
			if (_players[l] is ZombiePlayer zombiePlayer2)
			{
				zombiePlayer2.Draw(spriteBatch, _offset);
			}
			else if (_players[l] is ZombiePlayerBadGuy zombiePlayerBadGuy)
			{
				zombiePlayerBadGuy.Draw(drawDebug: false);
			}
		}
		spriteBatch.Begin();
		if (!_showForeground)
		{
			spriteBatch.Draw(_foreground, _offset, new Color(0.35f, 0.35f, 0.35f, 1f));
		}
		spriteBatch.End();
		if (_navMeshEditorEnabled)
		{
			NavMeshEditor.Draw(spriteBatch, _offset);
		}
		if (_wallMeshEditorEnabled)
		{
			WallMeshEditor.Draw(spriteBatch, _offset);
		}
		if (_pickupEditorEnabled)
		{
			_pickupEditor.Draw();
		}
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		for (int m = 0; m < _hitLocations.Count; m++)
		{
			spriteBatch.Draw(_node, _hitLocations[m] - new Vector2(5f, 5f) + _offset, Color.White);
		}
		if (_showMyDebugInfo)
		{
			int num2 = 100;
			int num3 = 50;
			spriteBatch.DrawString(_font, "Camera Offset: " + _offset, new Vector2(num2, num3), Color.White);
			spriteBatch.DrawString(_font, "CurrentWave: " + WaveManager.CurrentWave, new Vector2(num2, num3 + 20), Color.White);
			spriteBatch.DrawString(_font, "AI Alive: " + (_badguys.Count - WaveManager.BadguyQueue.Count).ToString() + " Time Until Next Wave: " + WaveManager.TimeUntilNextWave + " Limit Reached: " + WaveManager.HasReachedBadguyLimit, new Vector2(num2, num3 + 40), Color.White);
			spriteBatch.DrawString(_font, "Total BadGuy Decals Drawing: " + decalManager.NumberOfDecals, new Vector2(num2, num3 + 60), Color.White);
			spriteBatch.DrawString(_font, "Total Player Decals Drawing: " + playerBloodDecalManager.NumberOfDecals, new Vector2(num2, num3 + 80), Color.White);
			spriteBatch.DrawString(_font, "Particle Emmiters Active: " + ParticleEngine.GetEmitterCount(), new Vector2(num2, num3 + 100), Color.White);
			spriteBatch.DrawString(_font, "Projectiles Active: " + ProjectileManager.Count, new Vector2(num2, num3 + 120), Color.White);
			spriteBatch.DrawString(_font, "Wave Manager Active: " + _waveManagerEnabled, new Vector2(num2, num3 + 140), Color.White);
			spriteBatch.DrawString(_font, "Wave Manager Countdown: " + WaveManager.Countdown, new Vector2(num2, num3 + 160), Color.White);
			spriteBatch.DrawString(_font, "Farseer Bodies: " + ZombieUtils.World().BodyList.Count, new Vector2(num2, num3 + 180), Color.White);
			spriteBatch.DrawString(_font, "Mem: " + GC.GetTotalMemory(forceFullCollection: false), new Vector2(num2, num3 + 200), Color.White);
			spriteBatch.DrawString(_font, "TotalBadGuys: " + ZombieUtils.TotalBadGuysCreated, new Vector2(num2, num3 + 220), Color.White);
			spriteBatch.DrawString(_font, "Using Sound: " + ZombieUtils.UseSound, new Vector2(num2, num3 + 240), Color.White);
			spriteBatch.End();
		}
		else
		{
			spriteBatch.End();
		}
		if (_lightMapEditorEnabled)
		{
			_lightMapEditor.DrawLightEditorOverlay(spriteBatch, _offset);
		}
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		DumpRenderDiagIfDue();
		if (ZombieUtils.ShudderTimer > 0)
		{
			ZombieUtils.ShudderTimer--;
		}
		base.GraphicsDevice.Clear(Color.Black);
		float num4 = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds) * (float)Math.PI / 1000f;
		// NOTE: the CPU-side "shudder" rotation/scale wobble (previously applied via
		// SpriteBatch's position/origin/rotation/scale params) is dropped here -- a raw
		// full-screen quad has no per-draw transform. _screenScale is always 1f and the
		// shudder rotation is 0 whenever ZombieUtils.ShudderTimer is 0 (the common case),
		// so this is a one-time straight full-screen blit either way; only the brief
		// post-damage shudder flourish is lost, in exchange for the screen actually
		// rendering instead of being solid black (see FinalPassEffect.fx header).
		_finalPassEffect.Parameters["Texture"]?.SetValue(_backbufferRT);
		_finalPassEffect.CurrentTechnique.Passes[0].Apply();
		_finalPassQuad.Render(Vector2.One * -1f, Vector2.One);
		if (_renderDiagDumped && !_renderDiagPostPassDumped)
		{
			_renderDiagPostPassDumped = true;
			try
			{
				string dir2 = System.IO.Path.Combine(AppContext.BaseDirectory, "RenderDiag", "Zombie");
				System.IO.Directory.CreateDirectory(dir2);
				RenderTarget2D postPassCaptureRT = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
				base.GraphicsDevice.SetRenderTarget(postPassCaptureRT);
				base.GraphicsDevice.Clear(Color.Black);
				_finalPassEffect.Parameters["Texture"]?.SetValue(_backbufferRT);
				spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _finalPassEffect);
				spriteBatch.Draw(_backbufferRT, new Vector2(640f, 360f), null, Color.White, _screenScale * (num4 * (float)ZombieUtils.ShudderTimer), new Vector2(640f, 360f), _screenScale, SpriteEffects.None, 0f);
				spriteBatch.End();
				base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
				using (System.IO.FileStream stream2 = System.IO.File.Create(System.IO.Path.Combine(dir2, "5_postFinalPassEffect.png")))
				{
					postPassCaptureRT.SaveAsPng(stream2, postPassCaptureRT.Width, postPassCaptureRT.Height);
				}

				// CONTROL DIAGNOSTIC: same draw, but with NO custom effect at all
				// (default SpriteEffect) -- isolates whether the draw call itself
				// works independent of _finalPassEffect.
				RenderTarget2D controlCaptureRT = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
				base.GraphicsDevice.SetRenderTarget(controlCaptureRT);
				base.GraphicsDevice.Clear(Color.Black);
				spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null);
				spriteBatch.Draw(_backbufferRT, new Vector2(640f, 360f), null, Color.White, _screenScale * (num4 * (float)ZombieUtils.ShudderTimer), new Vector2(640f, 360f), _screenScale, SpriteEffects.None, 0f);
				spriteBatch.End();
				base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
				using (System.IO.FileStream stream3 = System.IO.File.Create(System.IO.Path.Combine(dir2, "6_noEffectControl.png")))
				{
					controlCaptureRT.SaveAsPng(stream3, controlCaptureRT.Width, controlCaptureRT.Height);
				}
			}
			catch (Exception diagEx)
			{
				try
				{
					System.IO.File.WriteAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "RenderDiag", "Zombie", "diag_error.txt"), diagEx.ToString());
				}
				catch
				{
				}
			}
		}
	}
}
