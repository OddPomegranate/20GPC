using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.TwoTrackTanks;

internal class TwoTrackTanks : Minigame
{
	private const float TrainSpeed = 0.2f;

	private const float TrainSpeedVariation = 0.2f;

	private const int GameOverWaitTime = 1000;

	private bool _minigameMetaOver;

	private Random _ranGen;

	private SpriteBatch _spriteBatch;

	private Texture2D _backgroundTex;

	private Texture2D _borderTex;

	private List<Tank> _tanks;

	private List<Projectile> _projectiles;

	private List<Train> _trains;

	private List<PhysicsObject> _obstacles;

	private List<List<Particle>> _particles;

	private Turntable _turntable;

	private int _smokeTimer;

	private int _steamTimer;

	private World _physicsWorld;

	private Tank _winner;

	private int[] _wins;

	private SpriteFont _winnerFont;

	private string _winnerString;

	private int _eventTimer;

	public TwoTrackTanks(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		_ranGen = new Random();
		if (_spriteBatch == null)
		{
			_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		}
		_backgroundTex = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\Background");
		_borderTex = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\Border");
		_winnerFont = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		ConvertUnits.SetDisplayUnitToSimUnitRatio(10f);
		int[] wins = new int[2];
		_wins = wins;
		string[] cueNames = new string[9] { "twoTrackTanks Collide", "twoTrackTanks Explosion", "twoTrackTanks FireCannon", "twoTrackTanks Reload", "twoTrackTanks TankEngine", "twoTrackTanks TrainRollStart", "twoTrackTanks TrainRollStop", "twoTrackTanks TrainWhistle", "twoTrackTanks TurretTurn" };
		_soundManager.PreloadSounds(cueNames);
		SetupNewGame();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void LoadContent()
	{
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		for (int i = 0; i < _tanks.Count; i++)
		{
			if (_tanks[i].Position.X > 1216f)
			{
				_tanks[i].Position = new Vector2(1216f, _tanks[i].Position.Y);
			}
			else if (_tanks[i].Position.X < 64f)
			{
				_tanks[i].Position = new Vector2(64f, _tanks[i].Position.Y);
			}
			if (_tanks[i].Position.Y > 648f)
			{
				_tanks[i].Position = new Vector2(_tanks[i].Position.X, 648f);
			}
			else if (_tanks[i].Position.Y < 72f)
			{
				_tanks[i].Position = new Vector2(_tanks[i].Position.X, 72f);
			}
			if (!_minigameMetaOver)
			{
				_tanks[i].Update(gameTime, _projectiles);
				if (_tanks[i].Health == 0)
				{
					_tanks[0].PhysicsBody.LinearDamping = ConvertUnits.ToSimUnits(0.1f);
					_tanks[1].PhysicsBody.LinearDamping = ConvertUnits.ToSimUnits(0.1f);
					int num = i * -1 + 1;
					_winner = _tanks[num];
					_winnerString = "";
					if (_winner.Driver != null)
					{
						_winnerString += _winner.Driver.Name;
					}
					if (_winner.Gunner != null)
					{
						if (_winnerString != "")
						{
							_winnerString += " & ";
						}
						_winnerString += _winner.Gunner.Name;
					}
					_wins[num]++;
					if (_minigameMeta.BestScore < (float)_wins[num])
					{
						_minigameMeta.SetScore(_winnerString, _wins[num]);
					}
					_minigameMetaOver = true;
					_eventTimer = 0;
				}
			}
			else if (_eventTimer < 1000 * _tanks.Count)
			{
				_eventTimer += gameTime.ElapsedGameTime.Milliseconds;
			}
			else if ((_tanks[i].Driver != null && _tanks[i].Driver.GamePadManager.ButtonWasPressed(Buttons.A)) || (_tanks[i].Gunner != null && _tanks[i].Gunner.GamePadManager.ButtonWasPressed(Buttons.A)))
			{
				SetupNewGame();
			}
			if (_tanks[i].Health == 0 && _steamTimer > 200)
			{
				for (int j = 0; j < 20; j++)
				{
					Particle particle = new Particle(_tanks[i].Position + new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 90f, ((float)_ranGen.NextDouble() - 0.5f) * 150f));
					particle.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 3f, ((float)_ranGen.NextDouble() - 0.5f) * 3f);
					particle.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 2f;
					particle.StartColour = Color.Gray * 0.4f;
					particle.EndColour = Color.Gray * 0f;
					particle.LifeSpan = 1500;
					_particles[0].Add(particle);
				}
			}
			if (_tanks[i].Health == 0 && _smokeTimer > 50)
			{
				for (int k = 0; k < 12; k++)
				{
					Particle particle2 = new Particle(_tanks[i].Position + new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 90f, ((float)_ranGen.NextDouble() - 0.5f) * 90f));
					particle2.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle2.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 2f, ((float)_ranGen.NextDouble() - 0.5f) * 2f);
					particle2.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
					particle2.StartColour = Color.Red * 0.5f;
					particle2.EndColour = Color.Red * 0f;
					particle2.LifeSpan = 500;
					_particles[1].Add(particle2);
				}
				for (int l = 0; l < 8; l++)
				{
					Particle particle3 = new Particle(_tanks[i].Position + new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 60f, ((float)_ranGen.NextDouble() - 0.5f) * 60f));
					particle3.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle3.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 0.5f, ((float)_ranGen.NextDouble() - 0.5f) * 0.5f);
					particle3.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
					particle3.StartColour = Color.Yellow * 0.6f;
					particle3.EndColour = Color.Yellow * 0f;
					particle3.LifeSpan = 460;
					_particles[1].Add(particle3);
				}
				for (int m = 0; m < 4; m++)
				{
					Particle particle4 = new Particle(_tanks[i].Position + new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 40f, ((float)_ranGen.NextDouble() - 0.5f) * 40f));
					particle4.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle4.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 0.1f, ((float)_ranGen.NextDouble() - 0.5f) * 0.1f);
					particle4.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
					particle4.StartColour = Color.White * 0.8f;
					particle4.EndColour = Color.White * 0f;
					particle4.LifeSpan = 300;
					_particles[1].Add(particle4);
				}
			}
			else if ((float)_tanks[i].Health < 50f && _smokeTimer > 50)
			{
				Particle particle5 = new Particle(_tanks[i].Position + Vector2.Transform(new Vector2(0f, 50f), Matrix.CreateRotationZ(_tanks[i].Rotation)) + new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 25f, ((float)_ranGen.NextDouble() - 0.5f) * 25f));
				particle5.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
				particle5.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 0.5f, ((float)_ranGen.NextDouble() - 0.5f) * 0.5f);
				particle5.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
				particle5.StartColour = Color.Lerp(Color.Black, Color.White, (float)_tanks[i].Health / 50f);
				particle5.EndColour = particle5.StartColour * 0f;
				_particles[0].Add(particle5);
			}
		}
		for (int n = 0; n < _projectiles.Count; n++)
		{
			if (_projectiles[n].Destroyed)
			{
				for (int num2 = 0; num2 < 20; num2++)
				{
					Particle particle6 = new Particle(_projectiles[n].Position);
					particle6.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle6.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 2f, ((float)_ranGen.NextDouble() - 0.5f) * 2f);
					particle6.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 2f;
					particle6.StartColour = Color.Gray;
					particle6.EndColour = Color.Gray * 0f;
					particle6.LifeSpan = 1500;
					_particles[0].Add(particle6);
				}
				for (int num3 = 0; num3 < 12; num3++)
				{
					Particle particle7 = new Particle(_projectiles[n].Position + new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 4f, ((float)_ranGen.NextDouble() - 0.5f) * 4f));
					particle7.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle7.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 2f, ((float)_ranGen.NextDouble() - 0.5f) * 2f);
					particle7.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
					particle7.StartColour = Color.Red;
					particle7.EndColour = Color.Red * 0f;
					particle7.LifeSpan = 500;
					_particles[1].Add(particle7);
				}
				for (int num4 = 0; num4 < 8; num4++)
				{
					Particle particle8 = new Particle(_projectiles[n].Position + new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 2f, ((float)_ranGen.NextDouble() - 0.5f) * 2f));
					particle8.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle8.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 0.5f, ((float)_ranGen.NextDouble() - 0.5f) * 0.5f);
					particle8.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
					particle8.StartColour = Color.Yellow;
					particle8.EndColour = Color.Yellow * 0f;
					particle8.LifeSpan = 460;
					_particles[1].Add(particle8);
				}
				for (int num5 = 0; num5 < 4; num5++)
				{
					Particle particle9 = new Particle(_projectiles[n].Position);
					particle9.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
					particle9.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 0.1f, ((float)_ranGen.NextDouble() - 0.5f) * 0.1f);
					particle9.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
					particle9.StartColour = Color.White;
					particle9.EndColour = Color.White * 0f;
					particle9.LifeSpan = 300;
					_particles[1].Add(particle9);
				}
				_soundManager.CreateGameSoundCue("twoTrackTanks Explosion").Play();
				foreach (Tank tank in _tanks)
				{
					Vector2 value = tank.Position - _projectiles[n].Position;
					float num6 = value.Length();
					if (tank != _projectiles[n].Owner && num6 <= 140f && num6 != 0f)
					{
						float num7 = 1f - num6 / 140f;
						tank.Damage((int)(30f * num7));
						tank.PhysicsBody.ApplyLinearImpulse(Vector2.Normalize(value) * ConvertUnits.ToSimUnits(600f) * (1f - num6 / 140f));
						if (tank.Driver != null)
						{
							tank.Driver.GamePadManager.StartVibration(400, num7);
						}
						if (tank.Gunner != null)
						{
							tank.Gunner.GamePadManager.StartVibration(400, num7);
						}
					}
				}
				if (_obstacles.Count < 40)
				{
					for (int num8 = 0; num8 < _trains.Count; num8++)
					{
						Vector2 value2 = _trains[num8].Position - _projectiles[n].Position;
						float num9 = value2.Length();
						if (num9 <= 140f)
						{
							_trains[num8].Derailed = true;
							_trains[num8].PhysicsBody.BodyType = BodyType.Dynamic;
							_trains[num8].PhysicsBody.ApplyLinearImpulse(Vector2.Normalize(value2) * ConvertUnits.ToSimUnits(600f) * (1f - num9 / 140f));
							_turntable.ForceStart();
						}
					}
				}
				foreach (PhysicsObject obstacle in _obstacles)
				{
					if (obstacle.PhysicsBody.BodyType == BodyType.Dynamic)
					{
						Vector2 value3 = obstacle.Position - _projectiles[n].Position;
						float num10 = value3.Length();
						if (obstacle != _projectiles[n].Owner && num10 <= 140f)
						{
							obstacle.PhysicsBody.ApplyLinearImpulse(Vector2.Normalize(value3) * ConvertUnits.ToSimUnits(600f) * (1f - num10 / 140f));
						}
					}
				}
				_projectiles.RemoveAt(n);
				n--;
			}
			else
			{
				_projectiles[n].Update(gameTime);
			}
		}
		for (int num11 = 0; num11 < _trains.Count; num11++)
		{
			_trains[num11].Update(gameTime);
			if (_trains[num11].IsEngine && _steamTimer > 200 && _trains[num11].Position.Y > 72f && _trains[num11].Position.Y < 648f)
			{
				Particle particle10 = new Particle(_trains[num11].Position + Vector2.Transform(new Vector2(40f, 0f), Matrix.CreateRotationZ(_trains[num11].Rotation)));
				particle10.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks\\Image\\ParticleSmoke");
				particle10.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 0.5f, ((float)_ranGen.NextDouble() - 0.5f) * 0.5f);
				particle10.RotationalVelocity = ((float)_ranGen.NextDouble() - 0.5f) * 1f;
				particle10.StartColour = Color.Linen * 0.6f;
				particle10.EndColour = Color.Linen * 0f;
				_particles[0].Add(particle10);
			}
			if ((_trains[num11].Direction.X > 0f && _trains[num11].Position.X > 1780f) || (_trains[num11].Direction.X < 0f && _trains[num11].Position.X < -500f) || (_trains[num11].Direction.Y > 0f && _trains[num11].Position.Y > 1220f) || (_trains[num11].Direction.Y < 0f && _trains[num11].Position.Y < -500f))
			{
				_trains[num11].PhysicsBody.Dispose();
				_trains.RemoveAt(num11);
				num11--;
			}
			else if (_trains[num11].Derailed)
			{
				_physicsWorld.RemoveJoint(_trains[num11].PhysicsBody.JointList.Joint);
				_trains[num11].AnimationFrames = 1;
				_trains[num11].PhysicsBody.FixedRotation = false;
				_trains[num11].PhysicsBody.LinearDamping = ConvertUnits.ToSimUnits(0.1f);
				_trains[num11].PhysicsBody.BodyType = BodyType.Dynamic;
				_trains[num11].PhysicsBody.CollidesWith = Category.All;
				_obstacles.Add(_trains[num11]);
				_trains.RemoveAt(num11);
				num11--;
			}
		}
		if (_trains.Count == 0)
		{
			_turntable.BeingUsed = false;
			_soundManager.CreateGameSoundCue("twoTrackTanks TrainRollStop").Play();
		}
		for (int num12 = 0; num12 < _obstacles.Count; num12++)
		{
			_obstacles[num12].Update(gameTime);
			if (_obstacles[num12].Position.X > 1280f || _obstacles[num12].Position.X < 0f || _obstacles[num12].Position.Y > 720f || _obstacles[num12].Position.Y < 0f)
			{
				_obstacles[num12].PhysicsBody.Dispose();
				_obstacles.RemoveAt(num12);
				num12--;
			}
		}
		for (int num13 = 0; num13 < _particles.Count; num13++)
		{
			for (int num14 = 0; num14 < _particles[num13].Count; num14++)
			{
				if (_particles[num13][num14].IsExausted)
				{
					_particles[num13].RemoveAt(num14);
					num14--;
				}
				else
				{
					_particles[num13][num14].Update(gameTime);
				}
			}
		}
		_turntable.Update(gameTime);
		if (_turntable.HasStopped && _trains.Count == 0)
		{
			_turntable.BeingUsed = true;
			_soundManager.CreateGameSoundCue("twoTrackTanks TrainRollStart").Play();
			_soundManager.CreateGameSoundCue("twoTrackTanks TrainWhistle").Play();
			Vector2 vector = ((!_turntable.IsVertical) ? new Vector2((float)_ranGen.Next(2) * 1280f, 360f) : new Vector2(640f, (float)_ranGen.Next(2) * 720f));
			Vector2 vector2 = Vector2.Normalize(new Vector2(640f, 360f) - vector);
			float rotation = (float)Math.Atan2(vector2.Y, vector2.X);
			float speed = 0.2f + ((float)_ranGen.NextDouble() - 0.5f) * 0.2f;
			Train train = new Train();
			train.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks/Image/TrainEngine");
			train.AnimationFrames = 4;
			train.AnimationFrameSize = new Point(111, 61);
			train.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(train.AnimationFrameSize.X - 4), ConvertUnits.ToSimUnits(train.AnimationFrameSize.Y - 4), 8f);
			train.Position = vector;
			train.Rotation = rotation;
			train.Direction = vector2;
			train.Speed = speed;
			train.PhysicsBody.LinearDamping = ConvertUnits.ToSimUnits(0.03f);
			train.PhysicsBody.FixedRotation = true;
			train.PhysicsBody.BodyType = BodyType.Kinematic;
			train.PhysicsBody.CollisionCategories = Category.Cat2;
			train.PhysicsBody.CollidesWith = Category.Cat1 | Category.Cat2 | Category.Cat3;
			train.SoundManager = _soundManager;
			JointFactory.CreateFixedPrismaticJoint(_physicsWorld, train.PhysicsBody, vector, vector2);
			_trains.Add(train);
			for (int num15 = 0; num15 < _ranGen.Next(4); num15++)
			{
				Train train2 = new Train(_trains[_trains.Count - 1]);
				train2.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks/Image/TrainCarriage0");
				train2.Origin += new Vector2(-10f, 0f);
				train2.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(train2.Sprite.Width - 28), ConvertUnits.ToSimUnits(train2.Sprite.Height - 25), 8f);
				train2.Position = vector + vector2 * -120f * (num15 + 1);
				train2.Direction = vector2;
				train2.Rotation = rotation;
				train2.Speed = speed;
				train2.PhysicsBody.LinearDamping = ConvertUnits.ToSimUnits(0.03f);
				train2.PhysicsBody.FixedRotation = true;
				train2.PhysicsBody.BodyType = BodyType.Kinematic;
				train2.PhysicsBody.CollisionCategories = Category.Cat2;
				train2.PhysicsBody.CollidesWith = Category.Cat1 | Category.Cat2 | Category.Cat3;
				train2.SoundManager = _soundManager;
				JointFactory.CreateFixedPrismaticJoint(_physicsWorld, train2.PhysicsBody, vector, vector2);
				_trains.Add(train2);
			}
		}
		if (_smokeTimer > 50)
		{
			_smokeTimer = 0;
		}
		_smokeTimer += gameTime.ElapsedGameTime.Milliseconds;
		if (_steamTimer > 200)
		{
			_steamTimer = 0;
		}
		_steamTimer += gameTime.ElapsedGameTime.Milliseconds;
		_physicsWorld.Step(gameTime.ElapsedGameTime.Milliseconds);
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		_spriteBatch.Draw(_backgroundTex, new Vector2(0f, 0f), Color.White);
		_turntable.Draw(_spriteBatch);
		foreach (Tank tank in _tanks)
		{
			tank.Draw(_spriteBatch);
		}
		foreach (PhysicsObject obstacle in _obstacles)
		{
			obstacle.Draw(_spriteBatch);
		}
		if (_trains.Count != 0)
		{
			for (int num = _trains.Count - 1; num != -1; num--)
			{
				_trains[num].Draw(_spriteBatch);
			}
		}
		foreach (Projectile projectile in _projectiles)
		{
			projectile.Draw(_spriteBatch);
		}
		foreach (Tank tank2 in _tanks)
		{
			tank2.DrawTurret(_spriteBatch);
		}
		_spriteBatch.Draw(_borderTex, new Vector2(0f, 0f), Color.White);
		foreach (Particle item in _particles[0])
		{
			item.Draw(_spriteBatch);
		}
		try
		{
			if (!_minigameMetaOver)
			{
				foreach (Tank tank3 in _tanks)
				{
					tank3.DrawReticle(_spriteBatch);
				}
			}
		}
		catch
		{
			Initialize();
			SetupNewGame();
		}
		_spriteBatch.End();
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
		foreach (Particle item2 in _particles[1])
		{
			item2.Draw(_spriteBatch);
		}
		_spriteBatch.End();
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		if (_minigameMetaOver)
		{
			Helper.DrawOutlinedText(_spriteBatch, _winnerFont, _winnerString, new Vector2(_titleSafeArea.Center.X, (float)_titleSafeArea.Center.Y - 20f), Color.White, Color.Black, Helper.OutlineType.Both, 0f, centered: true, 2f, Vector2.One);
			if (_winner.Driver == null || _winner.Gunner == null)
			{
				Helper.DrawOutlinedText(_spriteBatch, _winnerFont, "is the champion!", new Vector2(_titleSafeArea.Center.X, (float)_titleSafeArea.Center.Y + 20f), Color.White, Color.Black, Helper.OutlineType.Both, 0f, centered: true, 2f, Vector2.One);
			}
			else
			{
				Helper.DrawOutlinedText(_spriteBatch, _winnerFont, "are the champions!", new Vector2(_titleSafeArea.Center.X, (float)_titleSafeArea.Center.Y + 20f), Color.White, Color.Black, Helper.OutlineType.Both, 0f, centered: true, 2f, Vector2.One);
			}
		}
		_spriteBatch.End();
		base.Draw(gameTime);
	}

	protected override void OnEnabledChanged(object sender, EventArgs args)
	{
		base.OnEnabledChanged(sender, args);
	}

	private void SetupNewGame()
	{
		_smokeTimer = 0;
		_steamTimer = 0;
		_physicsWorld = new World(Vector2.Zero, new AABB(Vector2.Zero, ConvertUnits.ToSimUnits(new Vector2(1280f, 720f))));
		_tanks = new List<Tank>();
		switch (_playerManager.NumberOfPlayers)
		{
		case 1:
			_tanks.Add(new Tank(_playerManager.PlayersConnected[0], _ranGen));
			break;
		case 2:
			_tanks.Add(new Tank(_playerManager.PlayersConnected[0], _ranGen));
			_tanks.Add(new Tank(_playerManager.PlayersConnected[1], _ranGen));
			break;
		case 3:
			_tanks.Add(new Tank(_playerManager.PlayersConnected[0], _playerManager.PlayersConnected[1], _ranGen));
			_tanks.Add(new Tank(_playerManager.PlayersConnected[2], _ranGen));
			break;
		case 4:
			_tanks.Add(new Tank(_playerManager.PlayersConnected[0], _playerManager.PlayersConnected[1], _ranGen));
			_tanks.Add(new Tank(_playerManager.PlayersConnected[2], _playerManager.PlayersConnected[3], _ranGen));
			break;
		}
		_tanks[0].Load(_contentManager, _physicsWorld, _soundManager);
		_tanks[0].Position = new Vector2(240f, 560f);
		_tanks[0].Rotation = (float)Math.PI / 2f;
		_tanks[1].Load(_contentManager, _physicsWorld, _soundManager);
		_tanks[1].Position = new Vector2(1040f, 160f);
		_tanks[1].Rotation = -(float)Math.PI / 2f;
		_particles = new List<List<Particle>>();
		_particles.Add(new List<Particle>());
		_particles.Add(new List<Particle>());
		_turntable = new Turntable();
		_turntable.Load(_contentManager, _physicsWorld);
		_turntable.Position = new Vector2(640f, 360f);
		_obstacles = new List<PhysicsObject>();
		PhysicsObject physicsObject = new PhysicsObject();
		physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(1280f), ConvertUnits.ToSimUnits(72f), 1000f);
		physicsObject.Position = new Vector2(640f, 36f);
		physicsObject.PhysicsBody.BodyType = BodyType.Static;
		physicsObject.PhysicsBody.CollisionCategories = Category.Cat4;
		physicsObject.PhysicsBody.CollidesWith = Category.All;
		physicsObject.SoundManager = _soundManager;
		_obstacles.Add(physicsObject);
		physicsObject = new PhysicsObject();
		physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(1280f), ConvertUnits.ToSimUnits(72f), 1000f);
		physicsObject.Position = new Vector2(640f, 684f);
		physicsObject.PhysicsBody.BodyType = BodyType.Static;
		physicsObject.PhysicsBody.CollisionCategories = Category.Cat4;
		physicsObject.PhysicsBody.CollidesWith = Category.All;
		physicsObject.SoundManager = _soundManager;
		_obstacles.Add(physicsObject);
		physicsObject = new PhysicsObject();
		physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(64f), ConvertUnits.ToSimUnits(720f), 1000f);
		physicsObject.Position = new Vector2(32f, 360f);
		physicsObject.PhysicsBody.BodyType = BodyType.Static;
		physicsObject.PhysicsBody.CollisionCategories = Category.Cat4;
		physicsObject.PhysicsBody.CollidesWith = Category.All;
		physicsObject.SoundManager = _soundManager;
		_obstacles.Add(physicsObject);
		physicsObject = new PhysicsObject();
		physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(64f), ConvertUnits.ToSimUnits(720f), 1000f);
		physicsObject.Position = new Vector2(1248f, 360f);
		physicsObject.PhysicsBody.BodyType = BodyType.Static;
		physicsObject.PhysicsBody.CollisionCategories = Category.Cat4;
		physicsObject.PhysicsBody.CollidesWith = Category.All;
		physicsObject.SoundManager = _soundManager;
		_obstacles.Add(physicsObject);
		for (int i = 0; i < 2; i++)
		{
			physicsObject = new PhysicsObject();
			physicsObject.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks/Image/Stopper");
			physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(physicsObject.Sprite.Width - 4), ConvertUnits.ToSimUnits(physicsObject.Sprite.Height - 4), 100f);
			physicsObject.PhysicsBody.BodyType = BodyType.Static;
			physicsObject.PhysicsBody.CollisionCategories = Category.Cat2;
			physicsObject.PhysicsBody.CollidesWith = Category.Cat1 | Category.Cat2 | Category.Cat4;
			physicsObject.SoundManager = _soundManager;
			_obstacles.Add(physicsObject);
		}
		_obstacles[_obstacles.Count - 2].Position = new Vector2(500f, 182f);
		_obstacles[_obstacles.Count - 2].Rotation = -(float)Math.PI / 2f;
		_obstacles[_obstacles.Count - 1].Position = new Vector2(780f, 539f);
		_obstacles[_obstacles.Count - 1].Rotation = (float)Math.PI / 2f;
		for (int j = 0; j < 16; j++)
		{
			physicsObject = new PhysicsObject();
			physicsObject.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks/Image/Container");
			physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(physicsObject.Sprite.Width - 4), ConvertUnits.ToSimUnits(physicsObject.Sprite.Height - 4), 1000f);
			physicsObject.PhysicsBody.BodyType = BodyType.Static;
			physicsObject.PhysicsBody.CollisionCategories = Category.Cat2;
			physicsObject.PhysicsBody.CollidesWith = Category.All;
			physicsObject.SoundManager = _soundManager;
			float[] array = new float[3] { 1f, 1f, 1f };
			if (_ranGen.Next(2) == 0)
			{
				array[0] -= (float)_ranGen.NextDouble();
				array[1] -= (float)_ranGen.NextDouble();
				array[2] *= 0.5f;
			}
			else if (_ranGen.Next(2) == 0)
			{
				array[0] -= (float)_ranGen.NextDouble();
				array[1] *= 0.5f;
				array[2] -= (float)_ranGen.NextDouble();
			}
			else if (_ranGen.Next(2) == 0)
			{
				array[0] *= 0.5f;
				array[1] -= (float)_ranGen.NextDouble();
				array[2] -= (float)_ranGen.NextDouble();
			}
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = ((array[k] < 0.5f) ? 0.5f : array[k]);
			}
			physicsObject.Colour = Color.Lerp(new Color(array[0], array[1], array[2]), Color.White, (float)_ranGen.NextDouble());
			_obstacles.Add(physicsObject);
		}
		_obstacles[_obstacles.Count - 16].Position = new Vector2(97f, 131f);
		_obstacles[_obstacles.Count - 16].Rotation = -1.549747f;
		_obstacles[_obstacles.Count - 15].Position = new Vector2(-5f, 278f);
		_obstacles[_obstacles.Count - 15].Rotation = 0.1549966f;
		_obstacles[_obstacles.Count - 14].Position = new Vector2(34f, 434f);
		_obstacles[_obstacles.Count - 14].Rotation = -0.1037927f;
		_obstacles[_obstacles.Count - 13].Position = new Vector2(65f, 548f);
		_obstacles[_obstacles.Count - 13].Rotation = 0.09495161f;
		_obstacles[_obstacles.Count - 12].Position = new Vector2(63f, 610f);
		_obstacles[_obstacles.Count - 12].Rotation = 0.04758306f;
		_obstacles[_obstacles.Count - 11].Position = new Vector2(1216f, 539f);
		_obstacles[_obstacles.Count - 11].Rotation = 0f;
		_obstacles[_obstacles.Count - 10].Position = new Vector2(60f, 489f);
		_obstacles[_obstacles.Count - 10].Rotation = 0f;
		_obstacles[_obstacles.Count - 9].Position = new Vector2(91f, 250f);
		_obstacles[_obstacles.Count - 9].Rotation = -1.394087f;
		_obstacles[_obstacles.Count - 8].Position = new Vector2(24f, 192f);
		_obstacles[_obstacles.Count - 8].Rotation = -1.850677f;
		_obstacles[_obstacles.Count - 7].Position = new Vector2(9f, 106f);
		_obstacles[_obstacles.Count - 7].Rotation = -0.09495181f;
		_obstacles[_obstacles.Count - 6].Position = new Vector2(1252f, 246f);
		_obstacles[_obstacles.Count - 6].Rotation = -1.538022f;
		_obstacles[_obstacles.Count - 5].Position = new Vector2(1217f, 604f);
		_obstacles[_obstacles.Count - 5].Rotation = 2.887224f;
		_obstacles[_obstacles.Count - 4].Position = new Vector2(1189f, 243f);
		_obstacles[_obstacles.Count - 4].Rotation = -1.502411f;
		_obstacles[_obstacles.Count - 3].Position = new Vector2(1219f, 101f);
		_obstacles[_obstacles.Count - 3].Rotation = 3.105894f;
		_obstacles[_obstacles.Count - 2].Position = new Vector2(1223f, 159f);
		_obstacles[_obstacles.Count - 2].Rotation = -3.075025f;
		_obstacles[_obstacles.Count - 1].Position = new Vector2(1216f, 460f);
		_obstacles[_obstacles.Count - 1].Rotation = -2.677945f;
		for (int l = 0; l < 8; l++)
		{
			physicsObject = new PhysicsObject();
			physicsObject.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks/Image/Container");
			physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(physicsObject.Sprite.Width - 4), ConvertUnits.ToSimUnits(physicsObject.Sprite.Height - 4), 8f);
			physicsObject.PhysicsBody.LinearDamping = ConvertUnits.ToSimUnits(0.1f);
			physicsObject.PhysicsBody.BodyType = BodyType.Dynamic;
			physicsObject.PhysicsBody.CollisionCategories = Category.Cat2;
			physicsObject.PhysicsBody.CollidesWith = Category.All;
			physicsObject.SoundManager = _soundManager;
			float[] array2 = new float[3] { 1f, 1f, 1f };
			if (_ranGen.Next(2) == 0)
			{
				array2[0] -= (float)_ranGen.NextDouble();
				array2[1] *= 0.5f;
				array2[2] *= 0.5f;
			}
			else if (_ranGen.Next(2) == 0)
			{
				array2[1] -= (float)_ranGen.NextDouble();
				array2[0] *= 0.5f;
				array2[2] *= 0.5f;
			}
			else if (_ranGen.Next(2) == 0)
			{
				array2[2] -= (float)_ranGen.NextDouble();
				array2[0] *= 0.5f;
				array2[1] *= 0.5f;
			}
			for (int m = 0; m < array2.Length; m++)
			{
				array2[m] = ((array2[m] < 0.5f) ? 0.5f : array2[m]);
			}
			physicsObject.Colour = Color.Lerp(new Color(array2[0], array2[1], array2[2]), Color.White, (float)_ranGen.NextDouble());
			_obstacles.Add(physicsObject);
		}
		_obstacles[_obstacles.Count - 8].Position = new Vector2(1033f, 306f);
		_obstacles[_obstacles.Count - 8].Rotation = -2.313967f;
		_obstacles[_obstacles.Count - 7].Position = new Vector2(976f, 472f);
		_obstacles[_obstacles.Count - 7].Rotation = 3.132247f;
		_obstacles[_obstacles.Count - 6].Position = new Vector2(414f, 435f);
		_obstacles[_obstacles.Count - 6].Rotation = 0.372068f;
		_obstacles[_obstacles.Count - 5].Position = new Vector2(312f, 277f);
		_obstacles[_obstacles.Count - 5].Rotation = -0.6262618f;
		_obstacles[_obstacles.Count - 4].Position = new Vector2(581f, 548f);
		_obstacles[_obstacles.Count - 4].Rotation = 3.008063f;
		_obstacles[_obstacles.Count - 3].Position = new Vector2(866f, 286f);
		_obstacles[_obstacles.Count - 3].Rotation = 0.9827935f;
		_obstacles[_obstacles.Count - 2].Position = new Vector2(742f, 144f);
		_obstacles[_obstacles.Count - 2].Rotation = -0.1853479f;
		_obstacles[_obstacles.Count - 1].Position = new Vector2(232f, 429f);
		_obstacles[_obstacles.Count - 1].Rotation = -0.7496989f;
		for (int n = 0; n < 2; n++)
		{
			physicsObject = new PhysicsObject();
			physicsObject.Sprite = _contentManager.Load<Texture2D>("TwoTrackTanks/Image/TrainCarriage1");
			physicsObject.Origin += new Vector2(-10f, 0f);
			physicsObject.PhysicsBody = BodyFactory.CreateRectangle(_physicsWorld, ConvertUnits.ToSimUnits(physicsObject.Sprite.Width - 28), ConvertUnits.ToSimUnits(physicsObject.Sprite.Height - 25), 6f);
			physicsObject.PhysicsBody.LinearDamping = ConvertUnits.ToSimUnits(0.03f);
			physicsObject.PhysicsBody.BodyType = BodyType.Dynamic;
			physicsObject.PhysicsBody.CollisionCategories = Category.Cat2;
			physicsObject.PhysicsBody.CollidesWith = Category.All;
			physicsObject.SoundManager = _soundManager;
			_obstacles.Add(physicsObject);
		}
		_obstacles[_obstacles.Count - 2].Position = new Vector2(270f, 180f);
		_obstacles[_obstacles.Count - 2].Rotation = (float)Math.PI;
		JointFactory.CreateFixedPrismaticJoint(_physicsWorld, _obstacles[_obstacles.Count - 2].PhysicsBody, _obstacles[_obstacles.Count - 2].PhysicsBody.Position, Vector2.UnitX);
		_obstacles[_obstacles.Count - 1].Position = new Vector2(1040f, 540f);
		_obstacles[_obstacles.Count - 1].Rotation = (float)Math.PI;
		JointFactory.CreateFixedPrismaticJoint(_physicsWorld, _obstacles[_obstacles.Count - 1].PhysicsBody, _obstacles[_obstacles.Count - 1].PhysicsBody.Position, Vector2.UnitX);
		_trains = new List<Train>();
		_projectiles = new List<Projectile>();
		_minigameMetaOver = false;
	}
}
