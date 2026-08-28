using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.SuperHighway;

internal class SuperHighway : Minigame
{
	private const int BlurAmount = 10;

	private const float BlurIntensity = 0.75f;

	private const int MaxCars = 50;

	private const int MaxDebris = 200;

	private const int DebrisAmount = 6;

	private const int StartingStraight = 180;

	private const int PreventSpawnChance = 5;

	private SpriteBatch _spriteBatch;

	private LineRender _lineRender;

	private GameInterface _gameInterface;

	private RenderTarget2D _interfaceCanvas;

	private RenderTarget2D[] _effectCanvas;

	private Road _road;

	private List<Car> _cars;

	private List<Debris> _debris;

	private int _frame;

	private int DifficultyIncreaseTime = 1000;

	private Random _ranGen;

	private int _spawnTime;

	private List<PlayerCar> _order;

	private bool _gameOver;

	private int _lastScore;

	private Cue _startEngine;

	public SuperHighway(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		_lineRender = new LineRender();
		_lineRender.Load(_contentManager);
		_lineRender.GraphicsDevice = game.GraphicsDevice;
		_lineRender.BackBufferSize = new Rectangle(0, 0, 640, 360);
		_interfaceCanvas = new RenderTarget2D(game.GraphicsDevice, _lineRender.BackBufferSize.Width, _lineRender.BackBufferSize.Height);
		_effectCanvas = new RenderTarget2D[10];
		for (int i = 0; i < _effectCanvas.Length; i++)
		{
			_effectCanvas[i] = new RenderTarget2D(game.GraphicsDevice, _lineRender.BackBufferSize.Width, _lineRender.BackBufferSize.Height);
		}
		string[] cueNames = new string[1] { "hyperChase Crash" };
		_soundManager.PreloadSounds(cueNames);
	}

	public override void Initialize()
	{
		base.Initialize();
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_gameInterface = new GameInterface();
		_road = new Road();
		_cars = new List<Car>();
		if (!_demoMode)
		{
			for (int i = 0; i < _playerManager.NumberOfPlayers; i++)
			{
				_cars.Add(new PlayerCar(_playerManager.PlayersConnected[i], new Vector2(0.2f + 0.2f * (float)i, 0.6f)));
			}
		}
		_debris = new List<Debris>();
		_ranGen = new Random();
		_spawnTime = 20;
		_frame = 0;
		_lastScore = 0;
		_order = new List<PlayerCar>();
		_startEngine = _soundManager.CreateGameSoundCue("hyperChase Engine");
		_startEngine.SetVariable("Speed", 0f);
		_startEngine.Play();
		_gameOver = false;
	}

	protected override void LoadContent()
	{
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		if (_frame > 180)
		{
			if (_startEngine.IsPlaying)
			{
				_startEngine.Stop(AudioStopOptions.Immediate);
			}
			if (_frame % _spawnTime == 0 && _ranGen.Next(5) == 0 && _cars.Count < 50)
			{
				Vector2 position = new Vector2((float)_ranGen.NextDouble(), _ranGen.Next(2));
				Vector2 velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 0.001f, (float)_frame / 100000f);
				if (position.Y == 0f)
				{
					velocity.Y += (float)_ranGen.NextDouble() * 1E-05f;
				}
				else
				{
					velocity.Y = 0f - velocity.Y - (float)_ranGen.NextDouble() * 1E-05f;
				}
				position.Y += 0.01f;
				_cars.Add(new ObstacleCar(position, velocity, ((float)_ranGen.NextDouble() - 0.5f) * ((float)_frame / 10000000f)));
			}
			int num = 0;
			foreach (Car car2 in _cars)
			{
				car2.Update(gameTime);
				foreach (Car car3 in _cars)
				{
					if (car2 == car3 || !car2.IsAlive || !car3.IsAlive || !(car2.Position.Y < 0.6f) || !(car2.Position.Y > 0.1f) || !car2.CollisionVolume.Intersects(car3.CollisionVolume))
					{
						continue;
					}
					PlayerCar playerCar = null;
					if (!_gameOver && (object)car2.GetType() == typeof(PlayerCar))
					{
						playerCar = (PlayerCar)car2;
						playerCar.Player.GamePadManager.StartVibration(800, 1f, 1f, 0f, 0f);
						playerCar.Score = _frame - 180;
						_order.Add(playerCar);
					}
					if (!_gameOver && (object)car3.GetType() == typeof(PlayerCar))
					{
						playerCar = (PlayerCar)car3;
						playerCar.Player.GamePadManager.StartVibration(800, 1f, 1f, 0f, 0f);
						playerCar.Score = _frame - 180;
						_order.Add(playerCar);
					}
					if (playerCar != null)
					{
						_soundManager.CreateGameSoundCue("hyperChase Crash").Play();
					}
					car2.IsAlive = false;
					car3.IsAlive = false;
					if (_debris.Count < 200)
					{
						for (int i = 0; i < 6; i++)
						{
							_debris.Add(new Debris(car2, _ranGen));
						}
						for (int j = 0; j < 6; j++)
						{
							_debris.Add(new Debris(car3, _ranGen));
						}
					}
				}
				if (!_gameOver && (object)car2.GetType() == typeof(PlayerCar) && car2.IsAlive)
				{
					num++;
				}
			}
			if (!_gameOver && num == 0)
			{
				_gameOver = true;
				_lastScore = _frame;
				if ((float)_lastScore > _minigameMeta.BestScore)
				{
					string text = string.Empty;
					for (int k = 0; k < _order.Count; k++)
					{
						if (_order[k].Score == _lastScore)
						{
							text = ((!(text == string.Empty)) ? (text + ", " + _order[k].Player.Name) : _order[k].Player.Name);
						}
					}
					_minigameMeta.SetScore(text, _lastScore);
				}
			}
			for (int l = 0; l < _cars.Count; l++)
			{
				Car car = _cars[l];
				if (!car.IsAlive || (car.Position.Y < 0.01f && car.Velocity.Y < 0f) || (car.Position.Y > 0.65f && car.Velocity.Y > 0f))
				{
					Cue engineSound = car.EngineSound;
					if (engineSound != null && engineSound.IsPlaying)
					{
						engineSound.Stop(AudioStopOptions.Immediate);
					}
					_cars.Remove(car);
				}
			}
		}
		else
		{
			_startEngine.SetVariable("Speed", (float)_frame / 180f * 75f);
			foreach (PlayerCar car4 in _cars)
			{
				if (!car4.IsAlive && (double)car4.Position.Y > 0.44999999925494194)
				{
					car4.Position -= new Vector2(0f, 0.001f);
				}
				else if (!car4.IsAlive && (double)car4.Position.Y <= 0.44999999925494194)
				{
					car4.Spawn();
					car4.EngineSound = _soundManager.CreateGameSoundCue("hyperChase Engine");
				}
			}
		}
		for (int m = 0; m < _debris.Count; m++)
		{
			_debris[m].Update(gameTime);
			if (_debris[m]._position.Z > 1.4f)
			{
				_debris.RemoveAt(m);
			}
		}
		_road.Update(gameTime);
		_frame++;
		if (_demoMode && (_frame > 2000 || _frame < 1000))
		{
			_frame = 1000;
		}
		if (_frame % DifficultyIncreaseTime == 0 && _spawnTime > 1)
		{
			_spawnTime--;
		}
		if (_demoMode || !_gameOver || !((float)(_frame - _lastScore) > 120f))
		{
			return;
		}
		foreach (Player item in _playerManager.PlayersConnected)
		{
			if (item.GamePadManager.ButtonWasPressed(Buttons.A))
			{
				Initialize();
			}
		}
	}

	public override void Draw(GameTime gameTime)
	{
		for (int num = _effectCanvas.Length - 2; num >= 0; num--)
		{
			base.GraphicsDevice.SetRenderTarget(_effectCanvas[num + 1]);
			base.GraphicsDevice.Clear(Color.Black);
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			_spriteBatch.Draw(_effectCanvas[num], _lineRender.BackBufferSize, Color.White * 0.75f);
			_spriteBatch.End();
		}
		base.GraphicsDevice.SetRenderTarget(_effectCanvas[0]);
		base.GraphicsDevice.Clear(Color.Black);
		_road.Draw(_lineRender);
		foreach (Debris item in _debris)
		{
			_road.DrawDebris(_lineRender, item);
		}
		foreach (Car car in _cars)
		{
			_road.DrawCar(_lineRender, car);
		}
		base.GraphicsDevice.SetRenderTarget(_interfaceCanvas);
		base.GraphicsDevice.Clear(Color.Transparent);
		if (!_demoMode)
		{
			string text = (_frame - 180).ToString();
			if (_frame >= 180)
			{
				if (_gameOver)
				{
					_gameInterface.DrawRetry(_lineRender, new Vector3(320f, 260f, 0f));
				}
				else
				{
					_gameInterface.DrawString(_lineRender, text, new Vector2(320f - 7.5f * (float)text.Length, (float)(_titleSafeArea.Top / 2) + 5f), 1f, Color.White);
				}
			}
			if ((float)_frame < 240f)
			{
				for (int i = 0; i < 4; i++)
				{
					if (_frame > 36 * i)
					{
						_gameInterface.DrawLight(_lineRender, new Vector3(200f + 60f * (float)i, 120f, 0f), Color.Red);
						_gameInterface.DrawLight(_lineRender, new Vector3(200f + 60f * (float)i, 180f, 0f), Color.Red);
						_gameInterface.DrawLight(_lineRender, new Vector3(200f + 60f * (float)i, 240f, 0f), Color.Red);
					}
				}
				if (_frame > 144)
				{
					_gameInterface.DrawLight(_lineRender, new Vector3(440f, 120f, 0f), Color.Green);
					_gameInterface.DrawLight(_lineRender, new Vector3(440f, 180f, 0f), Color.Green);
					_gameInterface.DrawLight(_lineRender, new Vector3(440f, 240f, 0f), Color.Green);
				}
			}
			else
			{
				for (int j = 0; j < _order.Count; j++)
				{
					text = _order[j].Score.ToString();
					if (_gameOver)
					{
						if (_order[j].Score != _order[_order.Count - 1].Score || _frame % 60 < 30)
						{
							_gameInterface.DrawString(_lineRender, text, new Vector2(320f - 7.5f * (float)text.Length, (float)(_titleSafeArea.Top / 2) + 5f + 25f * (float)(_order.Count - 1) - (float)(25 * j)), 1f, _order[j].Colour);
						}
					}
					else
					{
						_gameInterface.DrawString(_lineRender, text, new Vector2(320f - 7.5f * (float)text.Length, (float)(_titleSafeArea.Top / 2) + 5f + 25f * (float)_order.Count - (float)(25 * j)), 1f, _order[j].Colour);
					}
				}
			}
		}
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.Clear(Color.Black);
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
		for (int k = 0; k != _effectCanvas.Length; k++)
		{
			_spriteBatch.Draw(_effectCanvas[k], new Rectangle(0, 0, 1280, 720), Color.White * ((float)_frame / 180f));
		}
		_spriteBatch.End();
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		for (int l = 0; l != 4; l++)
		{
			_spriteBatch.Draw(_interfaceCanvas, new Rectangle(0, 0, 1280, 720), Color.White);
		}
		_spriteBatch.End();
	}
}
