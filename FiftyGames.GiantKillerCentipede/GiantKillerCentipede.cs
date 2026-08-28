using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.GiantKillerCentipede;

internal class GiantKillerCentipede : Minigame
{
	private const int MushroomSpawnChance = 14;

	private const int MaxMushrooms = 50;

	private const int ShockwaveDropChance = 10;

	private const int RocketDropChance = 6;

	private const int HeatseekerDropChance = 9;

	private const int GrenadeDropChance = 3;

	private const int LaserDropChance = 5;

	private const int ShieldDropChance = 18;

	private const int NothingDropChance = 30;

	private const int MaxParticles = 1000;

	private const int DefenceActivationDelay = 1000;

	private const int EndRestartDelay = 2000;

	private const int MaxBodySegments = 85;

	private SpriteBatch _spriteBatch;

	private Background _background;

	private List<Mushroom> _shrooms;

	private List<Centipede> _centipedes;

	private List<Ship> _ships;

	private List<Projectile> _projectiles;

	private List<Powerup> _powerups;

	private List<Particle> _particles;

	private int _totalDropChance;

	private int _powerupDropChance;

	private int _defenceTimer;

	private Random _ranGen;

	private Texture2D _retryTex;

	private bool _gameOver;

	private int _eventTimer;

	private List<PlayerIndex> _centipedePlayers;

	private List<bool> _preference;

	private int _bodySegments;

	public GiantKillerCentipede(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		_ranGen = new Random();
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_retryTex = _contentManager.Load<Texture2D>("GiantKillerCentipede/Image/Retry");
		_background = new Background(new Rectangle(0, 0, 1280, 720));
		_background.Load(_contentManager, _ranGen);
		_projectiles = new List<Projectile>();
		_shrooms = new List<Mushroom>();
		_centipedes = new List<Centipede>();
		_ships = new List<Ship>(3);
		_powerups = new List<Powerup>();
		_powerupDropChance = 51;
		_totalDropChance = _powerupDropChance + 30;
		_particles = new List<Particle>(1000);
		_centipedePlayers = new List<PlayerIndex>(4);
		_preference = new List<bool>(4);
		for (int i = 0; i < _playerManager.NumberOfPlayers; i++)
		{
			_preference.Add(item: true);
		}
		string[] cueNames = new string[15]
		{
			"centipede BigExplosion", "centipede Die", "centipede Eat", "centipede Explosion", "centipede FireBullet", "centipede FireLaser", "centipede FireNuke", "centipede FireRocket", "centipede FireShockwave", "centipede PickupGrenade",
			"centipede PickupHeatseeker", "centipede PickupLaser", "centipede PickupRocket", "centipede PickupShield", "centipede PickupShockwave"
		};
		_soundManager.PreloadSounds(cueNames);
		SetupNewGame();
	}

	protected override void LoadContent()
	{
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		if (_shrooms.Count < 50 && _ranGen.Next(_shrooms.Count * _shrooms.Count) < 14)
		{
			Mushroom mushroom = new Mushroom(new Vector2(30f + (float)_ranGen.Next(61) * 20f, 40f + (float)_ranGen.Next(26) * 20f), _ranGen);
			mushroom.Load(_contentManager);
			bool flag = false;
			foreach (Centipede centipede3 in _centipedes)
			{
				foreach (BodySegment item in centipede3.Body)
				{
					if (mushroom.CollisionVolume.Intersects(item.CollisionVolume))
					{
						flag = true;
					}
				}
			}
			foreach (Mushroom shroom in _shrooms)
			{
				if (mushroom.CollisionVolume.Intersects(shroom.CollisionVolume))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				_shrooms.Add(mushroom);
			}
		}
		if (_centipedes.Count != 0 && _centipedes[0].Player != null && _centipedes[0].Player.GamePadManager.ButtonWasPressed(Buttons.Back))
		{
			_preference[(int)_centipedes[0].Player.PlayerIndex] = !_preference[(int)_centipedes[0].Player.PlayerIndex];
			for (int i = 0; i < _centipedes.Count; i++)
			{
				_centipedes[i].ElegableCentipede = _preference[(int)_centipedes[i].Player.PlayerIndex];
				_centipedes[i].ShowPreferenceBubble();
			}
			GameConsole.PrintString(_preference[(int)_centipedes[0].Player.PlayerIndex].ToString());
		}
		bool flag2 = false;
		for (int j = 0; j < _centipedes.Count; j++)
		{
			if (_centipedes[j].Body.Count != 0)
			{
				_centipedes[j].Update(gameTime, _gameOver && !_demoMode);
				for (int k = 0; k < _centipedes[j].Body.Count; k++)
				{
					if (_centipedes[j].Body[k].Health != 0)
					{
						if (_centipedes[j].Body[k].Position.Y > 552f)
						{
							flag2 = true;
						}
						for (int l = 0; l < _centipedes.Count; l++)
						{
							for (int m = 0; m < _centipedes[l].Body.Count; m++)
							{
								if ((j != l || k != m) && _centipedes[j].Body[k].CollisionVolume.Intersects(_centipedes[l].Body[m].CollisionVolume))
								{
									Vector2 vector = _centipedes[j].Body[k].Position - _centipedes[l].Body[m].Position;
									vector.Normalize();
									Vector2 vector2 = Vector2.Lerp(_centipedes[j].Body[k].Position, _centipedes[l].Body[m].Position, 0.5f);
									_centipedes[j].Body[k].Position = vector2 + vector * _centipedes[j].Body[k].CollisionVolume.Radius;
									_centipedes[l].Body[m].Position = vector2 - vector * _centipedes[l].Body[m].CollisionVolume.Radius;
								}
							}
						}
						foreach (Mushroom shroom2 in _shrooms)
						{
							if (!_centipedes[j].Body[k].CollisionVolume.Intersects(shroom2.CollisionVolume))
							{
								continue;
							}
							Vector2 vector3 = _centipedes[j].Body[k].Position - shroom2.Position;
							vector3.Normalize();
							float num = _centipedes[j].Body[k].CollisionVolume.Radius + shroom2.CollisionVolume.Radius;
							if (_centipedes[j].Body[k].BodyType == BodySegment.BodySegmentType.Head)
							{
								if (_centipedes[j].IsEating)
								{
									shroom2.Damage(8, _centipedes[j]);
									if (_particles.Count < 1000)
									{
										Vector2 position = Vector2.Lerp(_centipedes[j].Body[k].Position, shroom2.Position, 0.5f);
										for (int n = 0; n < 5; n++)
										{
											Particle particle = new Particle(_ranGen, position, 500, 20f, 1f, 2f, Color.SaddleBrown, Color.SaddleBrown * 0.1f);
											particle.Load(_contentManager);
											_particles.Add(particle);
										}
									}
									_soundManager.CreateGameSoundCue("centipede Eat").Play();
								}
								num -= 0.1f;
							}
							_centipedes[j].Body[k].Position = shroom2.Position + vector3 * num;
						}
						for (int num2 = 0; num2 < _ships.Count; num2++)
						{
							if (!_centipedes[j].Body[k].CollisionVolume.Intersects(_ships[num2].CollisionVolume))
							{
								continue;
							}
							if (_ships[num2].Shields == 0)
							{
								if (_ships[num2].Player != null)
								{
									_ships[num2].Player.GamePadManager.StartVibration(800, 1f, 1f, 0f, 0f);
								}
								for (int num3 = 0; num3 < 200; num3++)
								{
									Particle particle2 = new Particle(_ranGen, _ships[num2].Position, 1200, 100f, 2f, 4f, _ships[num2].Colour, Color.Gray * 0.1f);
									particle2.Load(_contentManager);
									_particles.Add(particle2);
								}
								for (int num4 = 0; num4 < 100; num4++)
								{
									Particle particle2 = new Particle(_ranGen, _ships[num2].Position, 500, 50f, 7f, 10f, Color.White, Color.Red * 0.8f);
									particle2.Load(_contentManager);
									_particles.Add(particle2);
								}
								for (int num5 = 0; num5 < 80; num5++)
								{
									Particle particle2 = new Particle(_ranGen, _ships[num2].Position, 400, 25f, 7f, 10f, Color.White, Color.Yellow * 0.8f);
									particle2.Load(_contentManager);
									_particles.Add(particle2);
								}
								if (_demoMode)
								{
									if (_centipedes.Count > 11)
									{
										_projectiles.Add(new Grenade(_ships[num2]));
									}
									ShipBot shipBot = new ShipBot(new Vector2(1280 * _ranGen.Next(2), 600f), _ranGen, ref _shrooms, ref _centipedes, ref _powerups);
									shipBot.Load(_contentManager);
									shipBot.SoundManager = _soundManager;
									_ships.Add(shipBot);
								}
								_ships.RemoveAt(num2);
								num2--;
							}
							else
							{
								_ships[num2].Damage();
							}
						}
						continue;
					}
					_bodySegments--;
					if (_centipedes[j].Body[k].Position.Y > _centipedes[j].Body[k].CollisionVolume.Radius)
					{
						Mushroom mushroom2 = new Mushroom(_centipedes[j].Body[k].Position, _ranGen);
						mushroom2.Load(_contentManager);
						_shrooms.Add(mushroom2);
					}
					if (k + 1 < _centipedes[j].Body.Count)
					{
						Centipede centipede;
						if ((object)_centipedes[j].GetType() == typeof(CentipedeBot))
						{
							centipede = new CentipedeBot(_centipedes[j].Body.GetRange(k + 1, _centipedes[j].Body.Count - (k + 1)), ref _shrooms, ref _ships, _ranGen);
							((CentipedeBot)centipede).EatSpeed += 20 * _centipedes.Count;
						}
						else
						{
							centipede = new Centipede(_centipedes[j].Player, _centipedes[j].Body.GetRange(k + 1, _centipedes[j].Body.Count - (k + 1)));
							centipede.ElegableCentipede = _centipedes[j].ElegableCentipede;
						}
						centipede.Load(_contentManager);
						_centipedes.Add(centipede);
					}
					_centipedes[j].Body.RemoveRange(k, _centipedes[j].Body.Count - k);
				}
			}
			else
			{
				_centipedes.RemoveAt(j);
				j--;
				if (_demoMode && _centipedes.Count == 0)
				{
					_centipedes.Add(new CentipedeBot(new Vector2(640f, -96f), 10, ref _shrooms, ref _ships, _ranGen));
					_centipedes[0].Load(_contentManager);
				}
			}
		}
		if (flag2)
		{
			_defenceTimer += gameTime.ElapsedGameTime.Milliseconds;
		}
		else
		{
			_defenceTimer = 0;
		}
		int num6 = 0;
		foreach (Ship ship in _ships)
		{
			if ((object)ship.GetType() != typeof(ShipBot))
			{
				num6++;
			}
			if (ship.Player != null && ship.Player.GamePadManager.ButtonWasPressed(Buttons.Back))
			{
				_preference[(int)ship.Player.PlayerIndex] = !_preference[(int)ship.Player.PlayerIndex];
				ship.ElegableCentipede = _preference[(int)ship.Player.PlayerIndex];
				ship.ShowPreferenceBubble();
			}
			ship.Update(gameTime, _projectiles, _gameOver && !_demoMode);
			if (_defenceTimer > 1000)
			{
				DefenceParticle defenceParticle = new DefenceParticle(ship);
				defenceParticle.Velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 20f, ((float)_ranGen.NextDouble() - 0.1f) * 10f);
				if (defenceParticle.Velocity.Length() < 5f)
				{
					defenceParticle.Velocity.Normalize();
					defenceParticle.Velocity *= 5f;
				}
				defenceParticle.Load(_contentManager);
				_projectiles.Add(defenceParticle);
			}
		}
		if (flag2 && _defenceTimer > 1000)
		{
			_defenceTimer -= 25;
		}
		for (int num7 = 0; num7 < _shrooms.Count; num7++)
		{
			if (_shrooms[num7].Health != 0)
			{
				_shrooms[num7].Update(gameTime);
				continue;
			}
			if (((object)_shrooms[num7].Destroyer.GetType() == typeof(Ship) || (object)_shrooms[num7].Destroyer.GetType() == typeof(ShipBot)) && _ranGen.Next(_totalDropChance) > 30)
			{
				int num8 = _ranGen.Next(_powerupDropChance);
				Powerup.PowerupType type = Powerup.PowerupType.None;
				if (num8 >= 0 && num8 < 10)
				{
					type = Powerup.PowerupType.ShockwaveWeapon;
				}
				else if (num8 >= 10 && num8 < 16)
				{
					type = Powerup.PowerupType.RocketWeapon;
				}
				else if (num8 >= 16 && num8 < 25)
				{
					type = Powerup.PowerupType.HeatseekerWeapon;
				}
				else if (num8 >= 25 && num8 < 28)
				{
					type = Powerup.PowerupType.GrenadeWeapon;
				}
				else if (num8 >= 28 && num8 < 33)
				{
					type = Powerup.PowerupType.LaserWeapon;
				}
				else if (num8 >= 33 && num8 < 51)
				{
					type = Powerup.PowerupType.Shield;
				}
				Powerup powerup = new Powerup(type, _shrooms[num7].Position);
				powerup.Load(_contentManager);
				_powerups.Add(powerup);
			}
			else if (((object)_shrooms[num7].Destroyer.GetType() == typeof(Centipede) || (object)_shrooms[num7].Destroyer.GetType() == typeof(CentipedeBot)) && _bodySegments < 85)
			{
				Centipede centipede2 = (Centipede)_shrooms[num7].Destroyer;
				centipede2.Grow();
				_bodySegments++;
			}
			_shrooms.RemoveAt(num7);
			num7--;
		}
		for (int num9 = 0; num9 < _projectiles.Count; num9++)
		{
			if (_projectiles[num9].IsAlive)
			{
				_projectiles[num9].Update(gameTime);
				if (_projectiles[num9].Position.X < 0f || _projectiles[num9].Position.X > 1280f || _projectiles[num9].Position.Y < 0f || _projectiles[num9].Position.Y > 720f)
				{
					_projectiles[num9].IsAlive = false;
				}
				if ((object)_projectiles[num9].GetType() == typeof(LaserPhoton))
				{
					_projectiles[num9].Position = new Vector2(_projectiles[num9].Owner.Position.X, _projectiles[num9].Position.Y);
				}
				if (_particles.Count < 1000)
				{
					if ((object)_projectiles[num9].GetType() == typeof(Rocket) || (object)_projectiles[num9].GetType() == typeof(Heatseeker))
					{
						Particle particle3 = new Particle(_ranGen, _projectiles[num9].Position, 800, 10f, 1f, 3f, Color.DimGray, Color.DimGray * 0.1f);
						particle3.Load(_contentManager);
						_particles.Add(particle3);
					}
					if ((object)_projectiles[num9].GetType() == typeof(Shockwave))
					{
						for (int num10 = 0; num10 < 21; num10++)
						{
							Vector2 vector4 = new Vector2(-120f + 12f * (float)num10, -10f + (float)Math.Sin((float)Math.PI / 20f * (float)num10) * -80f);
							Particle particle4 = new Particle(_ranGen, _projectiles[num9].Position + vector4, 800, 40f, 1f, 3f, Color.MediumPurple, Color.DarkViolet * 0.1f);
							particle4.Load(_contentManager);
							_particles.Add(particle4);
						}
					}
				}
				PhysicsObject physicsObject = null;
				BoundingSphere boundingSphere = new BoundingSphere(_projectiles[num9].CollisionVolume.Center, _projectiles[num9].SplashRadius);
				if (_projectiles[num9].IsAlive && (object)_projectiles[num9].GetType() != typeof(Shockwave))
				{
					for (int num11 = 0; num11 < _centipedes.Count; num11++)
					{
						for (int num12 = 0; num12 < _centipedes[num11].Body.Count; num12++)
						{
							if (_centipedes[num11].Body[num12].CollisionVolume.Intersects(_projectiles[num9].CollisionVolume))
							{
								if ((object)_projectiles[num9].GetType() != typeof(Grenade))
								{
									physicsObject = _centipedes[num11].Body[num12];
									_centipedes[num11].Body[num12].Damage(_projectiles[num9].ShotDamage);
									_projectiles[num9].IsAlive = false;
									if (_particles.Count < 1000)
									{
										for (int num13 = 0; num13 < 8; num13++)
										{
											Particle particle5 = new Particle(_ranGen, _projectiles[num9].Position, 500, 10f, 1f, 2f, Color.Red, Color.Red * 0.1f);
											particle5.Load(_contentManager);
											_particles.Add(particle5);
										}
									}
								}
								else
								{
									Vector2 vector5 = _projectiles[num9].Position - _centipedes[num11].Body[num12].Position;
									vector5.Normalize();
									float num14 = _projectiles[num9].Velocity.Length() * 0.5f;
									_projectiles[num9].Velocity = vector5 * num14;
									_centipedes[num11].Body[num12].Velocity = vector5 * (0f - num14);
								}
							}
							else if ((object)_projectiles[num9].GetType() == typeof(Heatseeker))
							{
								Heatseeker heatseeker = (Heatseeker)_projectiles[num9];
								Vector2 vector6 = _centipedes[num11].Body[num12].Position - heatseeker.Position;
								Vector2 vector7 = vector6;
								if (heatseeker.Target != null)
								{
									vector7 = heatseeker.Target.Position - heatseeker.Position;
								}
								if (heatseeker.Target == null || (_centipedes[num11].Body[num12] != heatseeker.Target && vector6.Length() < vector7.Length()))
								{
									heatseeker.Target = _centipedes[num11].Body[num12];
									vector7 = vector6;
								}
								vector7.Normalize();
								heatseeker.Velocity += vector7 * 0.01f;
							}
						}
					}
				}
				if (_projectiles[num9].IsAlive && (object)_projectiles[num9].GetType() != typeof(Shockwave))
				{
					for (int num15 = 0; num15 < _shrooms.Count; num15++)
					{
						if (!_projectiles[num9].CollisionVolume.Intersects(_shrooms[num15].CollisionVolume))
						{
							continue;
						}
						if ((object)_projectiles[num9].GetType() != typeof(Grenade))
						{
							physicsObject = _shrooms[num15];
							_shrooms[num15].Damage(_projectiles[num9].ShotDamage, _projectiles[num9].Owner);
							_projectiles[num9].IsAlive = false;
							if (_particles.Count < 1000)
							{
								for (int num16 = 0; num16 < 2; num16++)
								{
									Particle particle6 = new Particle(_ranGen, _projectiles[num9].Position, 500, 20f, 1f, 2f, Color.SaddleBrown, Color.SaddleBrown * 0.1f);
									particle6.Load(_contentManager);
									_particles.Add(particle6);
								}
							}
						}
						else
						{
							Vector2 vector8 = _projectiles[num9].Position - _shrooms[num15].Position;
							vector8.Normalize();
							_projectiles[num9].Velocity = vector8 * _projectiles[num9].Velocity.Length();
						}
					}
				}
				if ((object)_projectiles[num9].GetType() == typeof(Grenade))
				{
					Grenade grenade = (Grenade)_projectiles[num9];
					if (grenade.Fuse == 0)
					{
						physicsObject = grenade;
						grenade.IsAlive = false;
					}
				}
				if ((object)_projectiles[num9].GetType() == typeof(Shockwave))
				{
					physicsObject = _projectiles[num9];
				}
				if (physicsObject == null)
				{
					continue;
				}
				for (int num17 = 0; num17 < _centipedes.Count; num17++)
				{
					for (int num18 = 0; num18 < _centipedes[num17].Body.Count; num18++)
					{
						if (boundingSphere.Intersects(_centipedes[num17].Body[num18].CollisionVolume))
						{
							if (_centipedes[num17].Body[num18] != physicsObject)
							{
								_centipedes[num17].Body[num18].Damage(_projectiles[num9].SplashDamage);
							}
							Vector2 vector9 = new Vector2(0f, -1f);
							if ((object)_projectiles[num9].GetType() != typeof(Bullet))
							{
								vector9 = _centipedes[num17].Body[num18].Position - _projectiles[num9].Position;
								vector9.Normalize();
							}
							_centipedes[num17].Body[num18].Velocity += vector9 * _projectiles[num9].ShotForce;
						}
					}
				}
				for (int num19 = 0; num19 < _shrooms.Count; num19++)
				{
					if (_shrooms[num19] != physicsObject && boundingSphere.Intersects(_shrooms[num19].CollisionVolume))
					{
						_shrooms[num19].Damage(_projectiles[num9].SplashDamage, _projectiles[num9].Owner);
					}
				}
				continue;
			}
			if (_particles.Count < 1000 && _projectiles[num9].Position.X > 0f && _projectiles[num9].Position.X < 1280f && _projectiles[num9].Position.Y > 0f && _projectiles[num9].Position.Y < 720f)
			{
				if ((object)_projectiles[num9].GetType() == typeof(Rocket) || (object)_projectiles[num9].GetType() == typeof(Heatseeker))
				{
					for (int num20 = 0; num20 < 100; num20++)
					{
						Particle particle7 = new Particle(_ranGen, _projectiles[num9].Position, 1200, _projectiles[num9].SplashRadius + 50f, 2f, 4f, Color.DimGray, Color.Gray * 0.1f);
						particle7.Load(_contentManager);
						_particles.Add(particle7);
					}
					for (int num21 = 0; num21 < 50; num21++)
					{
						Particle particle7 = new Particle(_ranGen, _projectiles[num9].Position, 500, _projectiles[num9].SplashRadius, 5f, 8f, Color.White, Color.Red * 0.8f);
						particle7.Load(_contentManager);
						_particles.Add(particle7);
					}
					for (int num22 = 0; num22 < 30; num22++)
					{
						Particle particle7 = new Particle(_ranGen, _projectiles[num9].Position, 400, _projectiles[num9].SplashRadius - 20f, 5f, 8f, Color.White, Color.Yellow * 0.8f);
						particle7.Load(_contentManager);
						_particles.Add(particle7);
					}
					_soundManager.CreateGameSoundCue("centipede Explosion").Play();
				}
				if ((object)_projectiles[num9].GetType() == typeof(Grenade))
				{
					for (int num23 = 0; num23 < 200; num23++)
					{
						Particle particle8 = new Particle(_ranGen, _projectiles[num9].Position, 1200, _projectiles[num9].SplashRadius + 50f, 2f, 4f, Color.DimGray, Color.Gray * 0.1f);
						particle8.Load(_contentManager);
						_particles.Add(particle8);
					}
					for (int num24 = 0; num24 < 100; num24++)
					{
						Particle particle8 = new Particle(_ranGen, _projectiles[num9].Position, 500, _projectiles[num9].SplashRadius, 7f, 10f, Color.White, Color.Red * 0.8f);
						particle8.Load(_contentManager);
						_particles.Add(particle8);
					}
					for (int num25 = 0; num25 < 80; num25++)
					{
						Particle particle8 = new Particle(_ranGen, _projectiles[num9].Position, 400, _projectiles[num9].SplashRadius - 20f, 7f, 10f, Color.White, Color.Yellow * 0.8f);
						particle8.Load(_contentManager);
						_particles.Add(particle8);
					}
					_soundManager.CreateGameSoundCue("centipede BigExplosion").Play();
				}
				if ((object)_projectiles[num9].GetType() == typeof(LaserPhoton))
				{
					Particle particle9 = new Particle(_ranGen, _projectiles[num9].Position, 900, _projectiles[num9].SplashRadius + 50f, 1f, 2f, Color.Turquoise, Color.Turquoise * 0.1f);
					particle9.Load(_contentManager);
					_particles.Add(particle9);
				}
			}
			_projectiles.RemoveAt(num9);
			num9--;
		}
		for (int num26 = 0; num26 < _powerups.Count; num26++)
		{
			if (_powerups[num26].Position.Y < 720f)
			{
				_powerups[num26].Update(gameTime);
				bool flag3 = false;
				foreach (Ship ship2 in _ships)
				{
					if (!flag3 && _powerups[num26].CollisionVolume.Intersects(ship2.CollisionVolume))
					{
						ship2.GivePowerup(_powerups[num26].Type);
						_powerups[num26].Type = Powerup.PowerupType.None;
						flag3 = true;
					}
				}
				if (flag3)
				{
					_powerups.RemoveAt(num26);
					num26--;
				}
			}
			else
			{
				_powerups.RemoveAt(num26);
				num26--;
			}
		}
		for (int num27 = 0; num27 < _particles.Count; num27++)
		{
			if (!_particles[num27].IsUsed)
			{
				_particles[num27].Update(gameTime);
				continue;
			}
			_particles.RemoveAt(num27);
			num27--;
		}
		if (_centipedes.Count == 0 || _ships.Count == 0 || ((object)_centipedes[0].GetType() == typeof(CentipedeBot) && num6 == 0))
		{
			_gameOver = true;
		}
		if (!_gameOver || _demoMode)
		{
			return;
		}
		foreach (Player item2 in _playerManager.PlayersConnected)
		{
			if (_eventTimer > 2000 && item2.GamePadManager.ButtonWasPressed(Buttons.A))
			{
				if (_centipedePlayers.Count == _playerManager.PlayersConnected.Count)
				{
					_centipedePlayers.Clear();
				}
				SetupNewGame();
				_eventTimer = 0;
			}
			_eventTimer += gameTime.ElapsedGameTime.Milliseconds;
		}
		if (_eventTimer % 1000 >= 20)
		{
			return;
		}
		Vector2 position2 = new Vector2(_titleSafeArea.Center.X + (_ranGen.Next(800) - 400), _titleSafeArea.Center.Y + (_ranGen.Next(200) - 100));
		byte[] array = new byte[3];
		for (int num28 = 0; num28 < 100; num28++)
		{
			Particle particle10 = new Particle(_ranGen, position2, 2000, 200f, 7f, 10f, Color.Gray, Color.Gray * 0f);
			particle10.Load(_contentManager);
			_particles.Add(particle10);
		}
		for (int num29 = 0; num29 < 200; num29++)
		{
			_ranGen.NextBytes(array);
			for (int num30 = 0; num30 < array.Length; num30++)
			{
				if (array[num30] < 127)
				{
					array[num30] = 127;
				}
			}
			Color color = new Color(array[0], array[1], array[2]);
			Particle particle10 = new Particle(_ranGen, position2, 1600, 160f, 2f, 4f, color, Color.Lerp(color, Color.White, 0.5f) * 0f);
			particle10.Load(_contentManager);
			_particles.Add(particle10);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		_background.Draw(_spriteBatch);
		foreach (Centipede centipede in _centipedes)
		{
			centipede.Draw(_spriteBatch);
		}
		foreach (Ship ship in _ships)
		{
			ship.Draw(_spriteBatch);
		}
		foreach (Mushroom shroom in _shrooms)
		{
			shroom.Draw(_spriteBatch);
		}
		foreach (Projectile projectile in _projectiles)
		{
			projectile.Draw(_spriteBatch);
		}
		foreach (Powerup powerup in _powerups)
		{
			powerup.Draw(_spriteBatch);
		}
		foreach (Particle particle in _particles)
		{
			particle.Draw(_spriteBatch);
		}
		foreach (Ship ship2 in _ships)
		{
			ship2.DrawPrefenceBubbles(_spriteBatch);
		}
		foreach (Centipede centipede2 in _centipedes)
		{
			centipede2.DrawPrefenceBubbles(_spriteBatch);
		}
		if (_gameOver && !_demoMode)
		{
			_spriteBatch.Draw(_retryTex, new Vector2(_titleSafeArea.Center.X, (float)_titleSafeArea.Center.Y + 100f), null, Color.White, 0f, new Vector2((float)_retryTex.Width * 0.5f, (float)_retryTex.Height * 0.5f), 1f, SpriteEffects.None, 0f);
		}
		_spriteBatch.End();
	}

	private void SetupNewGame()
	{
		_shrooms.Clear();
		for (int i = 0; i < 40; i++)
		{
			Mushroom mushroom = new Mushroom(new Vector2(30f + (float)_ranGen.Next(61) * 20f, 40f + (float)_ranGen.Next(26) * 20f), _ranGen);
			mushroom.Load(_contentManager);
			bool flag = false;
			for (int j = 0; j < _shrooms.Count; j++)
			{
				if (mushroom != _shrooms[j] && mushroom.CollisionVolume.Intersects(_shrooms[j].CollisionVolume))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				_shrooms.Add(mushroom);
			}
		}
		if (!_demoMode)
		{
			_centipedes.Clear();
			_ships.Clear();
			int[] array = new int[4] { -1, -1, -1, -1 };
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int k = 0; k < array.Length; k++)
			{
				while (array[k] == -1)
				{
					if (_playerManager.PlayersConnected[num2].ColorIndex == num)
					{
						num2++;
						if (num2 == _playerManager.PlayersConnected.Count)
						{
							num2 = 0;
						}
						num++;
					}
					else if (array[num3] == num)
					{
						num3++;
						if (num3 == 4)
						{
							num3 = 0;
						}
						num++;
					}
					else
					{
						array[k] = num;
					}
					if (num == _playerManager.AvailableColors.Length)
					{
						num = 0;
					}
				}
			}
			for (int l = 0; l < _preference.Count; l++)
			{
				if (!_preference[l] && !_centipedePlayers.Contains(_playerManager.PlayersConnected[l].PlayerIndex))
				{
					_centipedePlayers.Add(_playerManager.PlayersConnected[l].PlayerIndex);
				}
			}
			int num4 = -1;
			int num5 = 0;
			for (int m = 0; m < _preference.Count; m++)
			{
				if (!_preference[m])
				{
					num5++;
				}
			}
			if (num5 != _playerManager.NumberOfPlayers)
			{
				num4 = _ranGen.Next(_playerManager.NumberOfPlayers);
				while (_centipedePlayers.Contains(_playerManager.PlayersConnected[num4].PlayerIndex))
				{
					num4++;
					if (num4 >= _playerManager.PlayersConnected.Count)
					{
						num4 = 0;
					}
				}
				_centipedes.Add(new Centipede(_playerManager.PlayersConnected[num4], new Vector2(640f, -96f), 10));
				_centipedes[0].Load(_contentManager);
				_bodySegments = 10;
				_centipedePlayers.Add(_playerManager.PlayersConnected[num4].PlayerIndex);
			}
			else
			{
				_centipedes.Add(new CentipedeBot(new Vector2(640f, -96f), 10, ref _shrooms, ref _ships, _ranGen));
				_centipedes[0].Load(_contentManager);
				_bodySegments = 10;
			}
			for (int n = 0; n < _playerManager.PlayersConnected.Count; n++)
			{
				if (num4 == -1 || n != num4)
				{
					Ship ship = new Ship(_playerManager.PlayersConnected[n], new Vector2(540f + 100f * (float)_ships.Count, 600f), _ranGen);
					ship.Load(_contentManager);
					ship.SoundManager = _soundManager;
					_ships.Add(ship);
				}
			}
			int num6 = 0;
			while (_ships.Count != 4)
			{
				Ship ship2 = new ShipBot(new Vector2(540f + 100f * (float)_ships.Count, 600f), _ranGen, ref _shrooms, ref _centipedes, ref _powerups);
				ship2.Load(_contentManager);
				ship2.SoundManager = _soundManager;
				ship2.Colour = _playerManager.AvailableColors[array[num6]];
				_ships.Add(ship2);
				num6++;
			}
		}
		else
		{
			_centipedes.Add(new CentipedeBot(new Vector2(640f, -96f), 10, ref _shrooms, ref _ships, _ranGen));
			_centipedes[0].Load(_contentManager);
			_bodySegments = 10;
			for (int num7 = 0; num7 < 4; num7++)
			{
				_ships.Add(new ShipBot(new Vector2(340f + (float)(200 * _ships.Count), 600f), _ranGen, ref _shrooms, ref _centipedes, ref _powerups));
				_ships[num7].Load(_contentManager);
				_ships[num7].SoundManager = _soundManager;
			}
		}
		_powerups.Clear();
		_projectiles.Clear();
		_particles.Clear();
		_defenceTimer = 0;
		_gameOver = false;
	}
}
