using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.PlatformsAreFalling2;

internal class PlatformsAreFalling2(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode) : Minigame(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
{
	private const int ScreenWidth = 880;

	private SpriteBatch _spriteBatch;

	private SpriteFont _font;

	private Texture2D _background;

	private Texture2D _platformSprite;

	private List<Platform> _platforms;

	private Robot[] _allRobots;

	private List<Robot> _robots;

	private int[] _scores;

	private int _highScore;

	private Color[] _colours;

	private Vector2[] _scorePositions;

	private List<int> _winners;

	private Random _random;

	private Floor _floor;

	private bool[] _isPlatformMoving;

	private float _screenOffset;

	private float _screenOffsetPrev;

	private int _screenOffsetMax;

	private Acid _acid;

	private float[] _prevPlatformY;

	private byte _numAliveRobots = 1;

	private int _restartTimer;

	private World _world;

	public override void Initialize()
	{
		_isPlatformMoving = new bool[3];
		_random = new Random();
		ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
		_world = new World(new Vector2(0f, 120f));
		if (!_demoMode)
		{
			ContactManager contactManager = _world.ContactManager;
			contactManager.BeginContact = (BeginContactDelegate)Delegate.Combine(contactManager.BeginContact, new BeginContactDelegate(BeginContact));
			ContactManager contactManager2 = _world.ContactManager;
			contactManager2.EndContact = (EndContactDelegate)Delegate.Combine(contactManager2.EndContact, new EndContactDelegate(EndContact));
		}
		base.Initialize();
	}

	private bool BeginContact(Contact contact)
	{
		if (contact.FixtureA.Body.UserData != null && (int)contact.FixtureA.Body.UserData >= 30)
		{
			_allRobots[(int)contact.FixtureA.Body.UserData - 30].AddContact(ref contact);
		}
		if (contact.FixtureB.Body.UserData != null && (int)contact.FixtureB.Body.UserData >= 30)
		{
			_allRobots[(int)contact.FixtureB.Body.UserData - 30].AddContact(ref contact);
		}
		return true;
	}

	private void EndContact(Contact contact)
	{
		if (contact.FixtureA.Body.UserData != null && (int)contact.FixtureA.Body.UserData >= 30)
		{
			_allRobots[(int)contact.FixtureA.Body.UserData - 30].RemoveContact(ref contact);
		}
		else if (contact.FixtureB.Body.UserData != null && (int)contact.FixtureB.Body.UserData >= 30)
		{
			_allRobots[(int)contact.FixtureB.Body.UserData - 30].RemoveContact(ref contact);
		}
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_winners = new List<int>(4);
		_acid = new Acid(880, base.Game.GraphicsDevice);
		_platformSprite = _contentManager.Load<Texture2D>("PlatformsAreFalling/Sprites/Block");
		_platforms = new List<Platform>();
		_background = _contentManager.Load<Texture2D>("PlatformsAreFalling/Sprites/Background");
		_prevPlatformY = new float[3] { 380f, 180f, -80f };
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		ReloadContent();
	}

	private void ReloadContent()
	{
		_highScore = 0;
		_winners.Clear();
		_isPlatformMoving[0] = false;
		_isPlatformMoving[1] = false;
		_isPlatformMoving[2] = false;
		_platforms.Clear();
		_acid.Reset();
		_restartTimer = 0;
		_screenOffset = 0f;
		_screenOffsetPrev = 0f;
		_prevPlatformY[0] = 380f;
		_prevPlatformY[1] = 180f;
		_prevPlatformY[2] = 80f;
		_screenOffsetMax = 0;
		_world.Clear();
		_floor = new Floor(880);
		_floor.LoadContent(_contentManager, _world, new Vector2(640f, 660f));
		_acid.Color = new Color(48, 213, 0) * 0.75f;
		if (!_demoMode)
		{
			_allRobots = new Robot[_playerManager.NumberOfPlayers];
			_robots = new List<Robot>(_playerManager.NumberOfPlayers);
			_scores = new int[_playerManager.NumberOfPlayers];
			_scorePositions = new Vector2[_playerManager.NumberOfPlayers];
			_colours = new Color[_playerManager.NumberOfPlayers];
			for (int i = 0; i < _playerManager.NumberOfPlayers; i++)
			{
				_allRobots[i] = new Robot(_playerManager.PlayersConnected[i], 880, _playerManager.NumberOfPlayers, i, ref _playerManager, ref _soundManager);
				_robots.Add(_allRobots[i]);
				ref Color reference = ref _colours[i];
				reference = _playerManager.GetPlayerColor(_playerManager.PlayersConnected[i]);
				ref Vector2 reference2 = ref _scorePositions[i];
				reference2 = new Vector2(1080 * (i % 2) + 100, _titleSafeArea.Top + 50 + 200 * (i / 2));
				_robots[i].LoadContent(_contentManager, _world);
			}
		}
	}

	public override void Update(GameTime gameTime)
	{
		_world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 1f / 30f));
		_acid.Update(_screenOffset);
		_isPlatformMoving[0] = false;
		_isPlatformMoving[1] = false;
		_isPlatformMoving[2] = false;
		for (ushort num = 0; num != _platforms.Count; num++)
		{
			_isPlatformMoving[_platforms[num].Zone] = _platforms[num].Active;
			_platforms[num].Update(_platforms, _acid.Position.Y, ref _prevPlatformY);
		}
		if (!_demoMode)
		{
			if (_robots.Count != 0)
			{
				for (byte b = 0; b != 3; b++)
				{
					if (!_isPlatformMoving[b])
					{
						_platforms.Add(new Platform(ref _random, _platformSprite, 880, _screenOffsetMax, b, ref _prevPlatformY, _world));
					}
				}
				if (_robots.Count != 0 && _numAliveRobots > 0)
				{
					_screenOffsetPrev = _screenOffset;
					_screenOffset = 0f;
				}
				_numAliveRobots = 0;
				for (int i = 0; i < _robots.Count; i++)
				{
					_robots[i].Update(_acid.Position.Y, gameTime);
					_scores[_robots[i].PlayerNum] = _robots[i].Score;
					if (_robots[i].IsAlive)
					{
						_numAliveRobots++;
						_screenOffset += (int)_robots[i].Position.Y;
					}
					else if (_robots[i].FullyDead)
					{
						_robots.RemoveAt(i);
						i--;
					}
				}
				if (_robots.Count > 0)
				{
					if (_numAliveRobots > 0)
					{
						_screenOffset /= (int)_numAliveRobots;
						_screenOffset = Math.Min(0f, _screenOffset - 360f);
						_screenOffsetMax = (int)Math.Min(_screenOffsetMax, _screenOffset);
					}
					else
					{
						_screenOffset = _screenOffsetPrev;
					}
				}
			}
			else
			{
				_screenOffset = _screenOffsetPrev;
				for (int j = 0; j != _scores.Length; j++)
				{
					if (_scores[j] > _highScore)
					{
						_winners.Clear();
						_winners.Add(j);
						_highScore = _scores[j];
					}
					else if (_scores[j] == _highScore)
					{
						_winners.Add(j);
					}
				}
				_restartTimer += gameTime.ElapsedGameTime.Milliseconds;
				if (_restartTimer > 3000)
				{
					foreach (int winner in _winners)
					{
						if ((float)_allRobots[winner].Score > _minigameMeta.BestScore)
						{
							_minigameMeta.SetScore(_allRobots[winner].Name, _allRobots[winner].Score);
						}
					}
					ReloadContent();
				}
			}
		}
		else
		{
			_screenOffset -= 0.45f;
			for (byte b2 = 0; b2 != 3; b2++)
			{
				if (!_isPlatformMoving[b2])
				{
					_platforms.Add(new Platform(ref _random, _platformSprite, 880, _screenOffsetMax, b2, ref _prevPlatformY, _world));
				}
			}
			if (_acid.Position.Y < _screenOffset)
			{
				_acid.Color = new Color(_acid.Color.R - 3, _acid.Color.G - 3, _acid.Color.B - 3, _acid.Color.A + 3);
				if (_acid.Color.R == 0 && _acid.Color.G == 0 && _acid.Color.B == 0)
				{
					ReloadContent();
				}
			}
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.Gray);
		_spriteBatch.Begin();
		_spriteBatch.Draw(_background, new Rectangle(200, 0, 1080, 720), null, Color.White);
		_floor.Draw(_spriteBatch, _screenOffset);
		foreach (Platform platform in _platforms)
		{
			platform.Draw(_spriteBatch, _screenOffset);
		}
		if (!_demoMode)
		{
			foreach (Robot robot in _robots)
			{
				robot.Draw(_spriteBatch, _screenOffset, _numAliveRobots);
			}
		}
		_acid.Draw(_spriteBatch, _screenOffset);
		_spriteBatch.Draw(_background, new Rectangle(0, 0, 200, 720), null, Color.Black);
		_spriteBatch.Draw(_background, new Rectangle(1080, 0, 200, 720), null, Color.Black);
		if (!_demoMode)
		{
			for (int i = 0; i != _scores.Length; i++)
			{
				if (_robots.Count != 0)
				{
					Helper.DrawOutlinedText(_spriteBatch, _font, _scores[i].ToString(), new Vector2(_scorePositions[i].X, _scorePositions[i].Y), _colours[i], Color.Black, Helper.OutlineType.Orthogonal, centered: true, 1f, new Vector2(1.5f, 1.5f));
				}
				else if (_winners.Contains(i))
				{
					if (gameTime.TotalGameTime.Milliseconds / 200 % 2 != 0)
					{
						Helper.DrawOutlinedText(_spriteBatch, _font, _scores[i].ToString(), new Vector2(640f, (i + 1) * 100), _colours[i], Color.Black, Helper.OutlineType.Orthogonal, centered: true, 1f, new Vector2(1.5f, 1.5f));
					}
				}
				else
				{
					Helper.DrawOutlinedText(_spriteBatch, _font, _scores[i].ToString(), new Vector2(640f, (i + 1) * 100), _colours[i], Color.Black, Helper.OutlineType.Orthogonal, centered: true, 1f, new Vector2(1.5f, 1.5f));
				}
			}
		}
		_spriteBatch.End();
		base.Draw(gameTime);
	}
}
