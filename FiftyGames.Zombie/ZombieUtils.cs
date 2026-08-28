using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Dynamics;
using FiftyGames.Zombie.DynamicLights;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal static class ZombieUtils
{
	private static GraphicsDevice _graphicsDevice;

	private static SpriteBatch _spriteBatch;

	private static ContentManager _contentManager;

	private static World _physWorld;

	private static NavMesh _navMesh;

	private static NavMesh _wallMesh;

	private static List<Entity> _players;

	private static List<BadGuy> _badguys;

	private static Random _random;

	private static SpriteFont _debugFont;

	private static Texture2D _debugDot;

	private static Vector2 _offset;

	private static Stopwatch _stopWatch;

	private static DecalManager _decalManager;

	private static GameTime _gameTime;

	private static SinglePixelTexture _singlePixelTexture;

	private static int _spawnDistance;

	private static DecalManager _playerDecalManager;

	private static SoundManager _soundManager;

	public static bool UseSound = true;

	public static Vector2 DefaultZombieGotoPosition { get; set; }

	public static NavMesh NavMesh
	{
		get
		{
			return _navMesh;
		}
		set
		{
			_navMesh = value;
		}
	}

	public static NavMesh WallMesh
	{
		get
		{
			return _wallMesh;
		}
		set
		{
			_wallMesh = value;
		}
	}

	public static Random Random
	{
		get
		{
			return _random;
		}
		set
		{
			_random = value;
		}
	}

	public static List<Entity> Players
	{
		get
		{
			return _players;
		}
		set
		{
			_players = value;
		}
	}

	public static List<BadGuy> BadGuys
	{
		get
		{
			return _badguys;
		}
		set
		{
			_badguys = value;
		}
	}

	public static Texture2D DebugDot => _debugDot;

	public static Vector2 Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	public static Stopwatch Stopwatch => _stopWatch;

	public static GameTime GameTime
	{
		get
		{
			return _gameTime;
		}
		set
		{
			_gameTime = value;
		}
	}

	public static SpriteBatch SpriteBatch
	{
		get
		{
			return _spriteBatch;
		}
		set
		{
			_spriteBatch = value;
		}
	}

	public static DecalManager DecalManager
	{
		get
		{
			return _decalManager;
		}
		set
		{
			_decalManager = value;
		}
	}

	public static DecalManager PlayerDecalManager
	{
		get
		{
			return _playerDecalManager;
		}
		set
		{
			_playerDecalManager = value;
		}
	}

	public static SinglePixelTexture SinglePixelTexture
	{
		get
		{
			return _singlePixelTexture;
		}
		set
		{
			_singlePixelTexture = value;
		}
	}

	public static int SpawnDistance
	{
		get
		{
			return _spawnDistance;
		}
		set
		{
			_spawnDistance = value;
		}
	}

	public static SoundManager SoundManager
	{
		get
		{
			return _soundManager;
		}
		set
		{
			_soundManager = value;
		}
	}

	public static DynamicLightMaskManager DynamicLightMaskManager { get; set; }

	public static MiscSettings MiscSettings { get; set; }

	public static int ShudderTimer { get; set; }

	public static long ElapsedTime { get; set; }

	public static int TotalBadGuysCreated { get; set; }

	public static List<BadGuy> GlobalBadGuyList { get; set; }

	public static void SetMemebers(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager, World world)
	{
		_graphicsDevice = graphicsDevice;
		_spriteBatch = spriteBatch;
		_contentManager = contentManager;
		_physWorld = world;
		_random = new Random();
		_debugFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_debugDot = contentManager.Load<Texture2D>("Zombie/Node");
		_spawnDistance = 384;
		_stopWatch = Stopwatch.StartNew();
	}

	public static void PlaySound(string name)
	{
		if (_soundManager != null && UseSound)
		{
			Cue cue = _soundManager.CreateGameSoundCue("topZombies " + name);
			cue.Play();
		}
	}

	public static void SetMembersToNull()
	{
		_graphicsDevice = null;
		_spriteBatch = null;
		_contentManager = null;
		_physWorld = null;
		_navMesh = null;
		_wallMesh = null;
		_players = null;
		_badguys = null;
		_random = null;
		_debugFont = null;
		_debugDot = null;
		_offset = Vector2.Zero;
		_stopWatch = null;
		_decalManager = null;
		_gameTime = null;
		_singlePixelTexture = null;
		_spawnDistance = 0;
	}

	public static void RemoveAllBadGuys()
	{
		for (int i = 0; i < GlobalBadGuyList.Count; i++)
		{
			if (GlobalBadGuyList[i] != null)
			{
				GlobalBadGuyList[i].IsAlive = false;
				GlobalBadGuyList[i].Health = 0f;
				GlobalBadGuyList[i].Update();
			}
		}
		GlobalBadGuyList.Clear();
	}

	public static GraphicsDevice GraphicsDevice()
	{
		return _graphicsDevice;
	}

	public static ContentManager ContentManager()
	{
		return _contentManager;
	}

	public static World World()
	{
		return _physWorld;
	}

	public static SpriteFont Font()
	{
		return _debugFont;
	}
}
