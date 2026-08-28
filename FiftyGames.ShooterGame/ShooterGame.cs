using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FiftyGames.Zombie.Utils;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shooter;
using Shooter.Entities;
using Shooter.Guns;
using Shooter.Helpers;
using Shooter.ISHelpers;
using Shooter.Pickups.Items;
using Shooter.World_Ridgid_Bodies;

namespace FiftyGames.ShooterGame;

internal class ShooterGame : Minigame
{
	private const float minZoom = 0.75f;

	private const float maxZoom = 1f;

	private static SoundManager _staticSoundManager;

	private MinigameMeta _minigame;

	private SpriteBatch spriteBatch;

	private Texture2D bottom;

	private Texture2D top;

	private RenderTarget2D ammoHealthRT;

	private Camera camera;

	private Rectangle levelRect;

	private Rectangle screenRect;

	private RenderTarget2D _levelRT;

	private SpriteFont _font;

	private bool _isDebugMode;

	private WallMeshEditor wallMeshEditor;

	private NavMeshEditor _navMeshEditor;

	private WorldBodyEditor _worldRidgidBodyEditor;

	private WorldBodyEditor _worldHealthAmmoEditor;

	private World _world;

	private Matrix _view;

	private Matrix _projection;

	private bool wallEditorEnabled;

	private bool navMeshEditorEnabled;

	private bool farseerDebugOverlayEnabled;

	private bool worldRidgidBodyEditorEnabled;

	private bool worldHealthAmmoEditorEnabled;

	private List<PhysObject> _physObjects;

	private List<PhysObject> _pickups;

	private List<ShooterPlayer> _players;

	private List<ShooterPlayer> _humanPlayers;

	private List<ShooterPlayer> _aiPlayers;

	private List<GunSettings> _gunSettings;

	private Random _random;

	private Rectangle _playerBoundingRect;

	private int _debugGunIndex;

	private Thread aiThread;

	private volatile bool _aiThreadRunning;

	private bool _isFadingOut;

	private float _fadeMills;

	private float _fadeTimer = 2000f;

	private SinglePixelTexture _spt;

	private bool _isDemo;

	public ShooterGame(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		_playerManager = playerManager;
		_staticSoundManager = soundManager;
		_contentManager = contentManager;
		_minigame = minigame;
		_isDemo = demoMode;
		string[] cueNames = new string[20]
		{
			"topShooter Walk", "topShooter Shoot Submachinegun", "topShooter Shoot Sniper", "topShooter Shoot Shotgun", "topShooter Shoot RPG", "topShooter Shoot Rifle", "topShooter Shoot P90", "topShooter Shoot Mac10", "topShooter Shoot M9", "topShooter Shoot M4",
			"topShooter Shoot M249", "topShooter Shoot Laser", "topShooter Shoot Grenade", "topShooter Shoot Flak", "topShooter Shoot Deagle", "topShooter Shoot AR", "topShooter Pickup", "topShooter Flak Explosion", "topShooter Explosion", "topShooter End Laser"
		};
		_staticSoundManager.PreloadSounds(cueNames);
		ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		ParticleEngine.InitEngine();
		_physObjects = new List<PhysObject>();
		_pickups = new List<PhysObject>();
		_players = new List<ShooterPlayer>();
		_humanPlayers = new List<ShooterPlayer>();
		_aiPlayers = new List<ShooterPlayer>();
		_gunSettings = new List<GunSettings>();
		ammoHealthRT = new RenderTarget2D(base.GraphicsDevice, 64, 64);
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		bottom = _contentManager.Load<Texture2D>("Shooter/Grounds/Bottom");
		top = _contentManager.Load<Texture2D>("Shooter/Grounds/Top");
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		levelRect = bottom.Bounds;
		screenRect = new Rectangle(0, 0, 1280, 720);
		camera = new Camera(screenRect, levelRect, 0.75f, 1f);
		GeometryHelper.InitLineRenderer(base.GraphicsDevice, _contentManager, levelRect);
		_levelRT = new RenderTarget2D(base.GraphicsDevice, bottom.Bounds.Width, bottom.Bounds.Height);
		_world = new World(Vector2.Zero);
		wallMeshEditor = new WallMeshEditor(base.GraphicsDevice, _contentManager, _world);
		_navMeshEditor = new NavMeshEditor(base.GraphicsDevice, _contentManager);
		_worldRidgidBodyEditor = new WorldBodyEditor(_world, base.GraphicsDevice, _contentManager, "Content/Shooter/Data/WorldRidgidBodyPositions.wbp");
		_worldRidgidBodyEditor.RegisterWorldBody(new Barrel(_world, _contentManager, new Vector2(-640f, -480f)));
		_worldRidgidBodyEditor.RegisterWorldBody(new Crate(_world, _contentManager, new Vector2(-640f, -480f)));
		_worldRidgidBodyEditor.RegisterWorldBody(new MetalCrate(_world, _contentManager, new Vector2(-640f, -480f)));
		_worldRidgidBodyEditor.LoadPositions(_physObjects);
		_worldHealthAmmoEditor = new WorldBodyEditor(_world, base.GraphicsDevice, _contentManager, "Content/Shooter/Data/WorldHealthAmmoPositions.wbp");
		_worldHealthAmmoEditor.RegisterWorldBody(new Ammo(_world, _contentManager, new Vector2(-640f, -480f)));
		_worldHealthAmmoEditor.RegisterWorldBody(new Health(_world, _contentManager, new Vector2(-640f, -480f)));
		_worldHealthAmmoEditor.LoadPositions(_pickups);
		_view = Matrix.Identity;
		Konsole.LoadContent(base.GraphicsDevice, _contentManager);
		_projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(camera.GetRect().Width), ConvertUnits.ToSimUnits(camera.GetRect().Height), 0f, 0f, 1f);
		_random = new Random();
		LoadGunsSettings();
		int num = 0;
		if (!_isDemo)
		{
			for (int i = 0; i < _playerManager.PlayersConnected.Count; i++)
			{
				ShooterPlayer item = new HumanPlayer(num, _playerManager.PlayersConnected[i], _world, _random, _contentManager, _navMeshEditor.NavMesh, _players, _gunSettings, ammoHealthRT);
				_physObjects.Add(item);
				_players.Add(item);
				_humanPlayers.Add(item);
				num++;
			}
			for (int j = 0; j < 4 - _playerManager.PlayersConnected.Count; j++)
			{
				ShooterPlayer item2 = new AIPlayer(num, _world, _random, _contentManager, new NavMesh(120, new StreamReader("Content/Shooter/Data/waypoints.wpts").BaseStream, base.GraphicsDevice, _contentManager), _players, _humanPlayers, _aiPlayers, _gunSettings, _pickups, ammoHealthRT);
				_physObjects.Add(item2);
				_players.Add(item2);
				_aiPlayers.Add(item2);
				num++;
			}
		}
		else
		{
			for (int k = 0; k < 8; k++)
			{
				ShooterPlayer item3 = new AIPlayer(num, _world, _random, _contentManager, new NavMesh(120, new StreamReader("Content/Shooter/Data/waypoints.wpts").BaseStream, base.GraphicsDevice, _contentManager), _players, _humanPlayers, _aiPlayers, _gunSettings, _pickups, ammoHealthRT);
				_physObjects.Add(item3);
				_players.Add(item3);
				_aiPlayers.Add(item3);
				num++;
			}
		}
		_aiThreadRunning = true;
		aiThread = new Thread(PathWorker);
		aiThread.IsBackground = true;
		aiThread.Start();
		for (int l = 0; l < _players.Count; l++)
		{
			_players[l].SetCurrentGun(new Gun(_players[l], _world, _contentManager, _gunSettings[_debugGunIndex]));
		}
		_isFadingOut = false;
		_spt = new SinglePixelTexture(base.GraphicsDevice);
		base.LoadContent();
	}

	private void PathWorker()
	{
		// PORTING NOTE: Thread.SetProcessorAffinity was an Xbox 360-only
		// XNA extension for pinning a thread to a specific hardware core;
		// it doesn't exist on desktop .NET and isn't needed there, so it
		// has simply been removed.
		//
		// PORTING NOTE: this loop used to run forever (while (true)) and rely
		// on Quit() calling Thread.Abort() to kill it -- Thread.Abort() was
		// removed from .NET Core/.NET 5+ entirely (it always throws
		// PlatformNotSupportedException: "Thread abort is not supported on
		// this platform."). Replaced with a cooperative _aiThreadRunning flag
		// that Quit() clears instead, so the loop exits on its own.
		while (_aiThreadRunning)
		{
			for (int i = 0; i < _aiPlayers.Count; i++)
			{
				AIPlayer aIPlayer = _aiPlayers[i] as AIPlayer;
				aIPlayer.ThreadWork();
			}
		}
	}

	public override void Quit()
	{
		if (aiThread != null)
		{
			// PORTING NOTE: Thread.Abort() doesn't exist on .NET Core/.NET 5+
			// (throws PlatformNotSupportedException unconditionally). Signal
			// the cooperative flag PathWorker() checks instead, and give the
			// thread a short window to exit on its own; it's a background
			// thread (see LoadContent) so the process can still exit cleanly
			// even if it somehow doesn't finish in time.
			_aiThreadRunning = false;
			aiThread.Join(500);
			aiThread = null;
		}
		ProjectileManager.DeleteAllShots();
		ParticleEngine.DestroyAllEmitters();
	}

	protected override void UnloadContent()
	{
		base.UnloadContent();
	}

	private void LoadGunsSettings()
	{
		_gunSettings.Clear();
		StreamReader streamReader = new StreamReader("Content/Shooter/Data/gunsSettings.ss");
		BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			_gunSettings.Add(GunSettings.LoadFromStream(binaryReader.BaseStream));
		}
		binaryReader.Close();
	}

	public override void Update(GameTime gameTime)
	{
		InputState.SetCurrentStates();
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
		if (InputState.IsKeyDown(Keys.Z))
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
		if (InputState.IsKeyDown(Keys.X))
		{
			if (navMeshEditorEnabled)
			{
				navMeshEditorEnabled = false;
			}
			else
			{
				navMeshEditorEnabled = true;
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
		if (InputState.IsKeyDown(Keys.V))
		{
			if (worldRidgidBodyEditorEnabled)
			{
				worldRidgidBodyEditorEnabled = false;
			}
			else
			{
				worldRidgidBodyEditorEnabled = true;
			}
		}
		if (InputState.IsKeyDown(Keys.B))
		{
			if (worldHealthAmmoEditorEnabled)
			{
				worldHealthAmmoEditorEnabled = false;
			}
			else
			{
				worldHealthAmmoEditorEnabled = true;
			}
		}
		if (InputState.IsKeyDown(Keys.Up) && _debugGunIndex < _gunSettings.Count - 1)
		{
			_debugGunIndex++;
			for (int i = 0; i < _players.Count; i++)
			{
				_players[i].SetCurrentGun(new Gun(_players[i], _world, _contentManager, _gunSettings[_debugGunIndex]));
			}
		}
		if (InputState.IsKeyDown(Keys.Down) && _debugGunIndex > 0)
		{
			_debugGunIndex--;
			for (int j = 0; j < _players.Count; j++)
			{
				_players[j].SetCurrentGun(new Gun(_players[j], _world, _contentManager, _gunSettings[_debugGunIndex]));
			}
		}
		if (InputState.IsKeyDown(Keys.L))
		{
			LoadGunsSettings();
			for (int k = 0; k < _players.Count; k++)
			{
				_players[k].SetCurrentGun(new Gun(_players[k], _world, _contentManager, _gunSettings[_debugGunIndex]));
			}
		}
		float num = 10000f;
		float num2 = 10000f;
		float num3 = 0f;
		float num4 = 0f;
		foreach (ShooterPlayer player in _players)
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
		int num5 = 200;
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
		if (_isDemo)
		{
			camera.MoveTo(destination, 20f);
			camera.ZoomTo(zoomLevel, 20f);
			camera.Update(gameTime);
		}
		else
		{
			camera.MoveTo(destination, 5f);
			camera.ZoomTo(zoomLevel, 50f);
			camera.Update(gameTime);
		}
		Vector2 mouseCoords = InputState.GetMouseCoords();
		if (!_isDebugMode)
		{
			mouseCoords.X = (float)camera.GetRect().Width / 1280f * mouseCoords.X;
			mouseCoords.Y = (float)camera.GetRect().Height / 720f * mouseCoords.Y;
			mouseCoords += camera.GetPosition();
		}
		else
		{
			mouseCoords.X = (float)bottom.Bounds.Width / 1280f * mouseCoords.X;
			mouseCoords.Y = (float)bottom.Bounds.Height / 720f * mouseCoords.Y;
		}
		if (wallEditorEnabled)
		{
			wallMeshEditor.Update(mouseCoords, _world);
		}
		if (navMeshEditorEnabled)
		{
			_navMeshEditor.Update(mouseCoords);
		}
		if (worldRidgidBodyEditorEnabled)
		{
			_worldRidgidBodyEditor.Update(mouseCoords);
		}
		if (worldHealthAmmoEditorEnabled)
		{
			_worldHealthAmmoEditor.Update(mouseCoords);
		}
		if (_isDebugMode)
		{
			_view = Matrix.Identity;
			_projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(bottom.Bounds.Width), ConvertUnits.ToSimUnits(bottom.Bounds.Height), 0f, 0f, 1f);
		}
		else
		{
			_view = Matrix.CreateTranslation(new Vector3(ConvertUnits.ToSimUnits(-camera.GetPosition()), 0f));
			_projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(camera.GetRect().Width), ConvertUnits.ToSimUnits(camera.GetRect().Height), 0f, 0f, 1f);
		}
		_world.Step(1f / 60f);
		for (int l = 0; l < _physObjects.Count; l++)
		{
			_physObjects[l].Update(gameTime);
		}
		for (int m = 0; m < _pickups.Count; m++)
		{
			_pickups[m].Update(gameTime);
		}
		foreach (ShooterPlayer player2 in _players)
		{
			if (player2.HasWon)
			{
				_isFadingOut = true;
			}
		}
		if (_isFadingOut)
		{
			_fadeMills += gameTime.ElapsedGameTime.Milliseconds;
			_ = _fadeMills / _fadeTimer;
			if (_fadeMills >= _fadeTimer)
			{
				_fadeMills = 0f;
				_isFadingOut = false;
				foreach (ShooterPlayer player3 in _players)
				{
					player3.Reset();
				}
			}
		}
		ProjectileManager.Update(gameTime);
		ParticleEngine.Update();
		InputState.SetPreviousStates();
	}

	public override void Draw(GameTime gameTime)
	{
		foreach (ShooterPlayer player in _players)
		{
			player.GenerateMaskedBar(spriteBatch);
		}
		base.GraphicsDevice.SetRenderTarget(_levelRT);
		base.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		spriteBatch.Draw(bottom, Vector2.Zero, Color.White);
		spriteBatch.End();
		ParticleEngine.Draw(spriteBatch, Vector2.Zero, Vector2.One);
		ProjectileManager.Draw(spriteBatch);
		spriteBatch.Begin();
		spriteBatch.Draw(top, Vector2.Zero, Color.White);
		spriteBatch.End();
		if (_isDebugMode)
		{
			camera.DrawDebugRect();
			Camera.DrawDebugRect(_playerBoundingRect);
		}
		for (int i = 0; i < _pickups.Count; i++)
		{
			_pickups[i].Draw(spriteBatch);
		}
		for (int j = 0; j < _physObjects.Count; j++)
		{
			_physObjects[j].Draw(spriteBatch);
		}
		if (worldRidgidBodyEditorEnabled)
		{
			_worldRidgidBodyEditor.Draw(spriteBatch);
		}
		if (worldHealthAmmoEditorEnabled)
		{
			_worldHealthAmmoEditor.Draw(spriteBatch);
		}
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		if (_isDebugMode)
		{
			spriteBatch.Draw(_levelRT, screenRect, Color.White);
			spriteBatch.DrawString(_font, "Zoom: " + camera.GetZoom() + " " + camera.GetRect().ToString() + " " + camera.GetPosition().ToString(), new Vector2(100f, 650f), Color.White);
		}
		else
		{
			spriteBatch.Draw(_levelRT, screenRect, camera.GetRect(), Color.White);
		}
		spriteBatch.End();
		if (!_isDebugMode)
		{
			if (wallEditorEnabled)
			{
				wallMeshEditor.Draw(spriteBatch, -camera.GetPosition(), camera.GetRect());
			}
			if (navMeshEditorEnabled)
			{
				_navMeshEditor.Draw(spriteBatch, -camera.GetPosition(), camera.GetRect());
			}
		}
		else
		{
			if (wallEditorEnabled)
			{
				wallMeshEditor.Draw(spriteBatch, Vector2.Zero, bottom.Bounds);
			}
			if (navMeshEditorEnabled)
			{
				_navMeshEditor.Draw(spriteBatch, Vector2.Zero, bottom.Bounds);
			}
			foreach (AIPlayer aiPlayer in _aiPlayers)
			{
				aiPlayer.DrawDebug(spriteBatch);
			}
			spriteBatch.Begin();
			int num = 0;
			int num2 = 10;
			for (int k = 0; k < _players.Count; k++)
			{
				spriteBatch.DrawString(_font, _players[k].GetCurrentGun().Settings.Name, new Vector2(0f, num), Color.White);
				num += num2;
				spriteBatch.DrawString(_font, _players[k].GetCurrentGun().GetAmmoRemaining().ToString(), new Vector2(0f, num), Color.White);
				num += num2;
				spriteBatch.DrawString(_font, _players[k].GetHealth().ToString(), new Vector2(0f, num), Color.White);
				num += num2 * 2;
			}
			spriteBatch.End();
		}
		if (_isFadingOut)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(_spt, new Rectangle(0, 0, 1280, 720), Color.White * (_fadeMills / _fadeTimer));
			spriteBatch.End();
		}
		base.Draw(gameTime);
	}

	public static Cue PlayCue(string cueName)
	{
		if (_staticSoundManager != null)
		{
			Cue cue = _staticSoundManager.CreateGameSoundCue("topShooter " + cueName);
			cue.Play();
			return cue;
		}
		return null;
	}

	public static void DisposeRenderTarget(RenderTarget2D renderTarget)
	{
		if (renderTarget != null && !renderTarget.IsDisposed)
		{
			renderTarget.Dispose();
		}
	}
}
