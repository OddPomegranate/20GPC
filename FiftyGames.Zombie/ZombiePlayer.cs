using System;
using System.Collections.Generic;
using System.IO;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FiftyGames.Zombie.DynamicLights;
using FiftyGames.Zombie.Guns;
using FiftyGames.Zombie.Pickups;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Zombie;

internal class ZombiePlayer : Entity
{
	private struct BulletHitInfo
	{
		public Vector2 _position;

		public Vector2 _normal;

		public object _objData;
	}

	private Player _frameworkPlayer;

	private Texture2D _bodyBackground;

	private Texture2D _bodyForeground;

	private Texture2D _handsSmall;

	private Texture2D _handsBig;

	private Texture2D _muzzleDebugPositionTexture;

	private Texture2D _onePixelTexture;

	private List<Texture2D> _explosionSprites;

	private Color _color;

	private Circle _playerCircle;

	private List<VertexPositionColor> _collisionCircleVerts;

	private List<Line> _collisionLines;

	private List<Vector2> _hitPositions;

	private List<BulletHitInfo> _bulletHits;

	private ParticleParameters _runtimeCustomParticleParams;

	private Texture2D _particleSprite;

	private Texture2D _bloodShoot;

	private List<Gun> _gunList;

	private Gun _currentGun;

	private bool _isShooting;

	private int _armsSpriteIndex = 4;

	private bool _isHoldingBigGun = true;

	private bool _godMode;

	private int _score;

	private int _deltaSinceLastHit;

	public float _wobbleRotation;

	public int _playerIndex;

	public double _lastShootTime;

	private Vector2 _lastDirectionOfDamage;

	private Queue<VertexPositionColor> _shootLine;

	private Random rand;

	private int lastHitTime;

	private bool wasAtTheSamePlaceLastFrame = true;

	private Vector2 lastStepSoundPosition;

	private static Vertices vertices;

	private GamePadState _previousGamePadState;

	private CustomParticleDescriptor cpd;

	public List<Gun> CurrentInventory => _gunList;

	public Gun CurrentGun
	{
		get
		{
			return _currentGun;
		}
		set
		{
			_currentGun = value;
		}
	}

	public List<Line> CollisionLines => _collisionLines;

	public double LastShootTime
	{
		get
		{
			return _lastShootTime;
		}
		set
		{
			_lastShootTime = value;
		}
	}

	public Color Color => _color;

	public Player FrameworkPlayer => _frameworkPlayer;

	public Vector2 LastDirectionOfDamage
	{
		get
		{
			return _lastDirectionOfDamage;
		}
		set
		{
			_lastDirectionOfDamage = value;
		}
	}

	public int Score
	{
		get
		{
			return _score;
		}
		set
		{
			_score = value;
		}
	}

	public ZombiePlayer(Player frameworkPlayer, Vector2 startPosition, int playerIndex)
	{
		_frameworkPlayer = frameworkPlayer;
		_health = ZombieUtils.MiscSettings.PlayerHealth;
		_position = startPosition + new Vector2(playerIndex * 100, 0f);
		_playerIndex = playerIndex;
		_bodyBackground = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerBodyBackground");
		_bodyForeground = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerBodyForeground");
		_handsSmall = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerSmallGunHands");
		_handsBig = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/PlayerBigGunHands");
		_particleSprite = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ParticleSprites/Spark");
		_bloodShoot = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ParticleSprites/BloodShoot");
		_muzzleDebugPositionTexture = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Node");
		_explosionSprites = new List<Texture2D>();
		_explosionSprites.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart1"));
		_explosionSprites.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart2"));
		_explosionSprites.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart3"));
		_explosionSprites.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart4"));
		_explosionSprites.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerPart5"));
		Color value = frameworkPlayer.Colour();
		_color = Color.Lerp(value2: new Color((byte)(value.R + value.G + value.B / 3), (byte)(value.R + value.G + value.B / 3), (byte)(value.R + value.G + value.B / 3)), value1: value, amount: 0.6f);
		_playerCircle = GeometryHelper.GenerateCircle(40, 8, _position);
		_collisionCircleVerts = new List<VertexPositionColor>();
		_collisionLines = new List<Line>();
		GeometryHelper.GetCircleLines(_playerCircle, out _collisionCircleVerts, out _collisionLines);
		_hitPositions = new List<Vector2>();
		_bulletHits = new List<BulletHitInfo>();
		Vertices vertices = new Vertices(_playerCircle.Points);
		for (int i = 0; i < vertices.Count; i++)
		{
			vertices[i] = ConvertUnits.ToSimUnits(vertices[i]);
		}
		_shootLine = new Queue<VertexPositionColor>();
		rand = new Random();
		_lastShootTime = 0.0;
		_gunList = new List<Gun>();
		_gunList.Add(new Pistol(this));
		_currentGun = new Pistol(this);
		_currentGun.AddRounds(10000000);
		cpd = new CustomParticleDescriptor(ZombieUtils.ContentManager());
		_runtimeCustomParticleParams = cpd.ToParticleParameters();
		_onePixelTexture = new Texture2D(ZombieUtils.GraphicsDevice(), 1, 1);
		Color[] data = new Color[1] { Color.White };
		_onePixelTexture.SetData(data);
		_previousGamePadState = _frameworkPlayer.GamePadManager.GamePadStateCurrent;
		if (ZombiePlayer.vertices == null)
		{
			LoadPlayerVerts();
		}
		_body = BodyFactory.CreatePolygon(ZombieUtils.World(), ZombiePlayer.vertices, 0f, ConvertUnits.ToSimUnits(_position));
		_body.BodyType = BodyType.Dynamic;
		_body.Friction = 0f;
		_body.Position = ConvertUnits.ToSimUnits(_position);
		_body.Mass = 2f;
		_body.SleepingAllowed = true;
		_body.UserData = this;
		_body.OnCollision += _body_OnCollision;
	}

	private bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (fixtureB.Body.UserData is Pickup pickup)
		{
			pickup.OnPlayerTouch(this);
		}
		return true;
	}

	public static void LoadPlayerVerts()
	{
		NavMesh navMesh = new NavMesh(100);
		StreamReader streamReader = new StreamReader("Content/Zombie/Data/playerEdges.wls");
		navMesh.LoadNavMesh(streamReader.BaseStream);
		vertices = new Vertices();
		for (int i = 0; i < navMesh.LineMesh.MeshNodes.Count; i++)
		{
			vertices.Add(ConvertUnits.ToSimUnits(navMesh.LineMesh.MeshNodes[i]._position - new Vector2(40f, 30f)));
		}
	}

	public void Dispose()
	{
		if (_body != null)
		{
			_body.Dispose();
		}
		_body = null;
		_isAlive = false;
		_health = 0f;
	}

	public override void TakeDamage(float damage, Vector2 fromDirection)
	{
		lastHitTime = ZombieUtils.GameTime.TotalGameTime.Seconds;
		base.TakeDamage(damage, fromDirection);
	}

	private void CheckBadGuyHit(GameTime gameTime)
	{
		if (_health <= 0f)
		{
			_isAlive = false;
			Dispose();
			return;
		}
		for (ContactEdge contactEdge = _body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
		{
			if (contactEdge.Contact.IsTouching() && contactEdge.Other.UserData is BadGuy badguy)
			{
				if (!_godMode)
				{
					OnPlayerHit(gameTime, badguy);
				}
				lastHitTime = gameTime.TotalGameTime.Seconds;
				if (_health <= 0f)
				{
					_isAlive = false;
					Dispose();
				}
			}
		}
	}

	public void Update(GameTime gameTime, List<BadGuy> badguys, List<Vector2> hitLocations)
	{
		_deltaSinceLastHit = gameTime.TotalGameTime.Seconds - lastHitTime;
		if (_deltaSinceLastHit > ZombieUtils.MiscSettings.PlayerHealthTimeUntilRecovery && _health < (float)ZombieUtils.MiscSettings.PlayerHealth)
		{
			if (_health + (float)ZombieUtils.MiscSettings.PlayerHealthRecoveryAmount <= (float)ZombieUtils.MiscSettings.PlayerHealth)
			{
				_health += ZombieUtils.MiscSettings.PlayerHealthRecoveryAmount;
			}
			else
			{
				_health = ZombieUtils.MiscSettings.PlayerHealth;
			}
		}
		if (InputState.KeyboardStateChanged() && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.G))
		{
			if (_godMode)
			{
				_godMode = false;
			}
			else
			{
				_godMode = true;
			}
		}
		Vector2 vector = new Vector2(_frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y, _frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.X);
		Vector2 vector2 = new Vector2(_frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X, _frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y * -1f);
		if (_frameworkPlayer.GamePadManager.GamePadStateCurrent.Triggers.Right > 0.4f)
		{
			int num = 0;
			_wobbleRotation = MathHelper.ToRadians(num);
		}
		if (vector.Length() > 0.5f)
		{
			_rotation = (float)Math.Atan2(_frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y * -1f, _frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.X);
		}
		Matrix matrix = Matrix.CreateTranslation(new Vector3(-50f, -18f, 0f)) * Matrix.CreateRotationZ(_rotation);
		_isHoldingBigGun = _currentGun.IsBigGun;
		if (gameTime.TotalGameTime.TotalMilliseconds - LastShootTime > (double)_currentGun.ShootInterval)
		{
			if (_frameworkPlayer.GamePadManager.GamePadStateCurrent.Triggers.Right > 0.4f)
			{
				LastShootTime = gameTime.TotalGameTime.TotalMilliseconds;
				ZombieUtils.DynamicLightMaskManager.Add(new MuzzleDynamicLight(ZombieUtils.ContentManager(), _position));
				Vector2 vector3 = GeometryHelper.AngleToV2(_rotation, 100f);
				vector3.Normalize();
				_body.ApplyLinearImpulse(ConvertUnits.ToSimUnits(vector3 * (_currentGun.PlayerKickbackImpulseMultiplier * -1f) * 100f));
				List<Shot> list = _currentGun.Shoot(_position - new Vector2(matrix.Translation.X, matrix.Translation.Y), _rotation + _wobbleRotation);
				_isShooting = true;
				if (list.Count > 0)
				{
					_armsSpriteIndex = 4;
				}
				_shootLine.Clear();
				_bulletHits.Clear();
				for (int i = 0; i < list.Count; i++)
				{
					_bulletHits.Clear();
					ZombieUtils.World().RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
					{
						if (f.Body.UserData != null)
						{
							if (f.Body.UserData is Pickup)
							{
								return -1f;
							}
							if ((object)f.Body.UserData.GetType() != typeof(ZombiePlayer))
							{
								BulletHitInfo item = default(BulletHitInfo);
								item._position = ConvertUnits.ToDisplayUnits(p);
								item._normal = n;
								item._objData = f.Body.UserData;
								_bulletHits.Add(item);
								return 1f;
							}
							return -1f;
						}
						return -1f;
					}, ConvertUnits.ToSimUnits(list[i].startPosition + list[i].bulletVector), ConvertUnits.ToSimUnits(list[i].startPosition));
					float num2 = 100000f;
					BulletHitInfo? bulletHitInfo = null;
					for (int num3 = 0; num3 < _bulletHits.Count; num3++)
					{
						float num4 = Vector2.Distance(base.Position, _bulletHits[num3]._position);
						if (num4 < num2)
						{
							num2 = num4;
							bulletHitInfo = _bulletHits[num3];
						}
					}
					List<BulletHitInfo> list2 = new List<BulletHitInfo>();
					if (_currentGun.HasPenertratingPower)
					{
						for (int num5 = 0; num5 < _bulletHits.Count; num5++)
						{
							if (_bulletHits[num5]._objData != null && _bulletHits[num5]._objData is BadGuy)
							{
								list2.Add(_bulletHits[num5]);
							}
						}
					}
					else if (bulletHitInfo.HasValue)
					{
						BulletHitInfo value = bulletHitInfo.Value;
						if (value._objData != null && value._objData is BadGuy)
						{
							list2.Add(value);
						}
					}
					Vector2? vector4 = null;
					if (bulletHitInfo.HasValue)
					{
						BulletHitInfo value2 = bulletHitInfo.Value;
						if (value2._objData != null)
						{
							if (list2.Count <= 0)
							{
								_shootLine.Enqueue(new VertexPositionColor(new Vector3(base.Position - new Vector2(matrix.Translation.X, matrix.Translation.Y), 0f), list[i].startColor));
								_shootLine.Enqueue(new VertexPositionColor(new Vector3(bulletHitInfo.Value._position, 0f), list[i].endColor));
							}
							for (int num6 = 0; num6 < list2.Count; num6++)
							{
								if (value2._objData is BadGuy)
								{
									BadGuy badGuy = (BadGuy)list2[num6]._objData;
									Line line = new Line();
									line.Start = base.Position;
									line.End = list2[num6]._position;
									Vector2[] lineIntersectionsWithCollisionCircle = badGuy.GetLineIntersectionsWithCollisionCircle(line);
									float num7 = 100000f;
									for (int num8 = 0; num8 < lineIntersectionsWithCollisionCircle.Length; num8++)
									{
										float num9 = Vector2.Distance(base.Position, lineIntersectionsWithCollisionCircle[num8]);
										if (num9 < num7)
										{
											num7 = num9;
											vector4 = lineIntersectionsWithCollisionCircle[num8];
										}
									}
									if (vector4.HasValue)
									{
										Vector2 fromDirection = vector4.Value - base.Position;
										fromDirection.Normalize();
										badGuy.TakeDamage(_currentGun.BulletDamage, fromDirection);
										if (badGuy.Health <= 0f)
										{
											_score += badGuy.GetKillPoints();
										}
									}
								}
								_shootLine.Enqueue(new VertexPositionColor(new Vector3(base.Position - new Vector2(matrix.Translation.X, matrix.Translation.Y), 0f), list[i].startColor));
								float num10 = Vector2.Distance(base.Position, value2._position);
								if (num10 < Vector2.Distance(base.Position, base.Position + list[i].bulletVector))
								{
									if (vector4.HasValue)
									{
										float num11 = (float)rand.NextDouble();
										ZombieUtils.DecalManager.AddNewDecal(vector4.Value, new Vector2(num11, num11), MathHelper.ToRadians(rand.Next(0, 360)));
										_shootLine.Enqueue(new VertexPositionColor(new Vector3(vector4.Value, 0f), list[i].endColor));
										_runtimeCustomParticleParams.Gravity = Vector2.Zero;
										_runtimeCustomParticleParams.MinColor = new Vector3(1f, 1f, 0f);
										_runtimeCustomParticleParams.MaxColor = new Vector3(1f, 1f, 0f);
										_runtimeCustomParticleParams.Change = 20;
										_runtimeCustomParticleParams.MinScale = Vector2.Zero;
										_runtimeCustomParticleParams.MaxScale = new Vector2(2f, 2f);
										ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_bloodShoot, _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, vector4.Value, 1, 1, 1));
										ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_bloodShoot, _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, value2._position, 1, 1, 1));
										ZombieUtils.PlaySound("Hit Zombie");
									}
									else
									{
										_shootLine.Enqueue(new VertexPositionColor(new Vector3(value2._position, 0f), list[i].endColor));
									}
								}
								else
								{
									_shootLine.Enqueue(new VertexPositionColor(new Vector3(base.Position + list[i].bulletVector, 0f), list[i].endColor));
								}
							}
						}
					}
					else
					{
						_shootLine.Enqueue(new VertexPositionColor(new Vector3(base.Position - new Vector2(matrix.Translation.X, matrix.Translation.Y), 0f), list[i].startColor));
						_shootLine.Enqueue(new VertexPositionColor(new Vector3(base.Position + list[i].bulletVector, 0f), list[i].endColor));
					}
					if (bulletHitInfo.HasValue && bulletHitInfo.Value._objData is Line)
					{
						Vector2 vector5 = bulletHitInfo.Value._position - _position;
						vector5.Normalize();
						Vector2 vector6 = bulletHitInfo.Value._normal * Vector2.Dot(vector5, bulletHitInfo.Value._normal);
						Vector2 vector7 = vector5 - vector6;
						Vector2 vector8 = vector7 - vector6;
						vector8.Normalize();
						_runtimeCustomParticleParams = cpd.ToParticleParameters();
						_runtimeCustomParticleParams.Gravity = vector8 * 3f;
						_runtimeCustomParticleParams.MinColor = new Vector3(1f, 1f, 1f);
						_runtimeCustomParticleParams.MaxColor = new Vector3(1f, 1f, 1f);
						_runtimeCustomParticleParams.Change = 20;
						_runtimeCustomParticleParams.Directional = true;
						_runtimeCustomParticleParams.MinScale = Vector2.Zero;
						_runtimeCustomParticleParams.MaxScale = new Vector2(0.9f, 0.9f);
						_runtimeCustomParticleParams.Multiplicative = 0.9f;
						ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_particleSprite, _runtimeCustomParticleParams), 1, BlendState.AlphaBlend, bulletHitInfo.Value._position, 5, 5, 1));
					}
				}
			}
			else
			{
				_isShooting = false;
				if (_armsSpriteIndex > 0)
				{
					_armsSpriteIndex--;
				}
			}
		}
		else
		{
			_isShooting = false;
			if (_armsSpriteIndex > 0)
			{
				_armsSpriteIndex--;
			}
		}
		CheckBadGuyHit(gameTime);
		if (_currentGun.RoundsRemaining <= 0)
		{
			_currentGun = new Pistol(this);
		}
		if (_body != null)
		{
			float num12 = ZombieUtils.MiscSettings.PlayerSpeed;
			Vector2 vector9 = vector2 * num12;
			vector9 = (vector9 - _body.LinearVelocity) * _body.Mass;
			_body.LinearDamping = 5f;
			_body.ApplyLinearImpulse(ConvertUnits.ToSimUnits(vector9));
			_body.Rotation = _rotation;
			Vector2 position = _position;
			Vector2 unitX = Vector2.UnitX;
			Vector2.Transform(unitX, Matrix.CreateRotationZ(_rotation));
			_position = ConvertUnits.ToDisplayUnits(_body.Position);
			_playerCircle.Position = _position;
			if (position != _position)
			{
				if (_health < (float)ZombieUtils.MiscSettings.PlayerHealth && _health > 0f)
				{
					_ = (float)ZombieUtils.MiscSettings.PlayerHealth / _health / 2f;
					float num13 = (float)rand.NextDouble() / (float)_deltaSinceLastHit;
					ZombieUtils.PlayerDecalManager.AddNewDecal(ConvertUnits.ToDisplayUnits(_body.Position), new Vector2(num13, num13), MathHelper.ToRadians(rand.Next(0, 360)));
				}
				if (wasAtTheSamePlaceLastFrame)
				{
					ZombieUtils.PlaySound("Step Gravel");
					wasAtTheSamePlaceLastFrame = false;
					lastStepSoundPosition = _position;
				}
				else if (Vector2.Distance(lastStepSoundPosition, _position) > 100f)
				{
					ZombieUtils.PlaySound("Step Gravel");
					lastStepSoundPosition = _position;
				}
			}
			else
			{
				wasAtTheSamePlaceLastFrame = true;
			}
		}
		if (InputState.KeyboardStateChanged())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D1))
			{
				_currentGun = new Deagle(this);
				_currentGun.AddRounds(Deagle.Settings.MagazineSize);
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D2))
			{
				_currentGun = new GrenadeLauncher(this);
				_currentGun.AddRounds(GrenadeLauncher.Settings.MagazineSize);
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D3))
			{
				_currentGun = new M4(this);
				_currentGun.AddRounds(M4.Settings.MagazineSize);
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D4))
			{
				_currentGun = new Pistol(this);
				_currentGun.AddRounds(Pistol.Settings.MagazineSize);
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D5))
			{
				_currentGun = new Shotgun(this);
				_currentGun.AddRounds(Shotgun.Settings.MagazineSize);
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.D6))
			{
				_currentGun = new SubmachineGun(this);
				_currentGun.AddRounds(SubmachineGun.Settings.MagazineSize);
			}
		}
	}

	private Vector2 GetNearestAIPosition()
	{
		float num = 100f;
		Vector2 result = Vector2.Zero;
		for (int i = 0; i < ZombieUtils.BadGuys.Count; i++)
		{
			float num2 = Vector2.Distance(ZombieUtils.BadGuys[i].Position, base.Position);
			if (num2 < num)
			{
				result = ZombieUtils.BadGuys[i].Position;
			}
		}
		return result;
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		GeometryHelper.LineRenderer.DrawShape(_shootLine.ToArray(), offset);
		_shootLine.Clear();
		Vector2 vector = _position + offset;
		spriteBatch.Begin();
		Vector2 origin = new Vector2(30f, 25f);
		spriteBatch.Draw(_bodyBackground, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation + _wobbleRotation, origin, SpriteEffects.None, 0f);
		if (_isHoldingBigGun)
		{
			spriteBatch.Draw(_handsBig, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), new Rectangle(90 * _armsSpriteIndex, 0, 85, 55), _color, _rotation + _wobbleRotation, origin, SpriteEffects.None, 0f);
		}
		else
		{
			spriteBatch.Draw(_handsSmall, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), new Rectangle(90 * _armsSpriteIndex, 0, 85, 55), _color, _rotation + _wobbleRotation, origin, SpriteEffects.None, 0f);
		}
		spriteBatch.Draw(_bodyForeground, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, _color, _rotation + _wobbleRotation, origin, SpriteEffects.None, 0f);
		spriteBatch.End();
		Vector2 vector2 = GeometryHelper.AngleToV2(_rotation, _armsSpriteIndex * -5);
		_currentGun.Draw(vector + vector2, _rotation, spriteBatch);
		if (_isShooting)
		{
			_currentGun.DrawMuzzle(spriteBatch, vector + vector2, _rotation);
		}
		for (int i = 0; i < _gunList.Count; i++)
		{
			_currentGun.DrawPersistant(spriteBatch);
		}
	}

	public void OnPlayerHit(GameTime gameTime, BadGuy badguy)
	{
		_frameworkPlayer.GamePadManager.StartVibration(1, 0.4f);
		_health -= badguy.DamagePerHit;
		_runtimeCustomParticleParams = cpd.ToParticleParameters();
		Vector2 vector = base.Position - badguy.Position;
		vector.Normalize();
		float num = 0f - (float)Math.Atan2(vector.Y, vector.X) - (float)Math.PI / 2f;
		_runtimeCustomParticleParams.MinDirection = num - 1.6f;
		_runtimeCustomParticleParams.MaxDirection = num + 1.6f;
		_runtimeCustomParticleParams.MinSpeed = 6f;
		_runtimeCustomParticleParams.MaxSpeed = 10f;
		_runtimeCustomParticleParams.Multiplicative = 0.9f;
		_runtimeCustomParticleParams.MinAlpha = 0.1f;
		_runtimeCustomParticleParams.MaxAlpha = 0.55f;
		_runtimeCustomParticleParams.MinAlphaChange1 = 0f;
		_runtimeCustomParticleParams.MinAlphaChange2 = 0f;
		_runtimeCustomParticleParams.MaxAlphaChange1 = -0.1f;
		_runtimeCustomParticleParams.MaxAlphaChange2 = -0.15f;
		_runtimeCustomParticleParams.MinScaleChange1 = Vector2.Zero;
		_runtimeCustomParticleParams.MinScaleChange2 = Vector2.Zero;
		_runtimeCustomParticleParams.MaxScaleChange1 = Vector2.One * 0.01f;
		_runtimeCustomParticleParams.MaxScaleChange2 = Vector2.One * 0.01f;
		_runtimeCustomParticleParams.MinColor = new Vector3(1f, 1f, 1f);
		_runtimeCustomParticleParams.MaxColor = new Vector3(1f, 1f, 1f);
		_runtimeCustomParticleParams.MaxColorChange1 = Vector3.One;
		_runtimeCustomParticleParams.MaxColorChange2 = Vector3.One;
		_runtimeCustomParticleParams.MinColorChange1 = Vector3.One;
		_runtimeCustomParticleParams.MinColorChange2 = Vector3.One;
		_runtimeCustomParticleParams.Change = 40;
		_runtimeCustomParticleParams.MinScale = Vector2.One * 0.4f;
		_runtimeCustomParticleParams.MaxScale = new Vector2(2f);
		if (_body != null)
		{
			ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_bloodShoot, _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, ConvertUnits.ToDisplayUnits(_body.Position), 1000, 15, 60));
			float num2 = (float)rand.NextDouble() * 0.6f + 0.4f;
			ZombieUtils.PlayerDecalManager.AddNewDecal(ConvertUnits.ToDisplayUnits(_body.Position), new Vector2(num2, num2), MathHelper.ToRadians(rand.Next(0, 360)));
		}
	}

	public void OnExplosion()
	{
		_runtimeCustomParticleParams = cpd.ToParticleParameters();
		_runtimeCustomParticleParams.MinDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) - MathHelper.ToRadians(10f);
		_runtimeCustomParticleParams.MaxDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) + MathHelper.ToRadians(10f);
		_runtimeCustomParticleParams.MinAlpha = 0.6f;
		_runtimeCustomParticleParams.MaxAlpha = 1f;
		_runtimeCustomParticleParams.Change = 100;
		_runtimeCustomParticleParams.Directional = true;
		_runtimeCustomParticleParams.Multiplicative = 0.9f;
		_runtimeCustomParticleParams.MinScaleChange1 = Vector2.Zero;
		_runtimeCustomParticleParams.MaxScaleChange1 = new Vector2(0.005f, 0.005f);
		_runtimeCustomParticleParams.MinScaleChange2 = Vector2.Zero;
		_runtimeCustomParticleParams.MaxScaleChange2 = Vector2.Zero;
		_runtimeCustomParticleParams.MinAlphaChange1 = -0.005f;
		_runtimeCustomParticleParams.MaxAlphaChange1 = -0.0025f;
		_runtimeCustomParticleParams.MinAlphaChange2 = -0.005f;
		_runtimeCustomParticleParams.MaxAlphaChange2 = -0.0025f;
		_runtimeCustomParticleParams.MinSpeed = 0.5f;
		_runtimeCustomParticleParams.MaxSpeed = 9f;
		_runtimeCustomParticleParams.MinColor = Vector3.One;
		_runtimeCustomParticleParams.MaxColor = Vector3.One;
		_runtimeCustomParticleParams.MinColorChange1 = Vector3.Zero;
		_runtimeCustomParticleParams.MinColorChange2 = Vector3.Zero;
		_runtimeCustomParticleParams.MaxColorChange1 = Vector3.Zero;
		_runtimeCustomParticleParams.MaxColorChange2 = Vector3.Zero;
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_explosionSprites[0], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 20, 1000));
		_runtimeCustomParticleParams.MinDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) - MathHelper.ToRadians(30f);
		_runtimeCustomParticleParams.MaxDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) + MathHelper.ToRadians(30f);
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_explosionSprites[1], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 20, 1000));
		_runtimeCustomParticleParams.MinDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) - MathHelper.ToRadians(40f);
		_runtimeCustomParticleParams.MaxDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) + MathHelper.ToRadians(40f);
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_explosionSprites[2], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 10, 1000));
		_runtimeCustomParticleParams.MaxSpeed = 5f;
		_runtimeCustomParticleParams.MinDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) - MathHelper.ToRadians(160f);
		_runtimeCustomParticleParams.MaxDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) + MathHelper.ToRadians(160f);
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_explosionSprites[3], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 15, 1000));
	}

	public void SetPosition(Vector2 position)
	{
		_body.Position = ConvertUnits.ToSimUnits(position);
		_position = position;
	}
}
