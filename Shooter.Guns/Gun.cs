using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Entities;
using Shooter.ISHelpers;
using Shooter.Pickups;
using Shooter.World_Ridgid_Bodies;

namespace Shooter.Guns;

internal class Gun : PhysObject
{
	private struct RayResultData
	{
		public Body body;

		public Vector2 position;
	}

	private Texture2D _texture;

	private Texture2D _bulletTexture;

	private Texture2D _rocketTexture;

	private Texture2D _rocketParticleEffect;

	private Texture2D _muzzleTexture;

	private Texture2D _groundTexture;

	private ContentManager _contentManager;

	private int _millSinceLastShot;

	private int _bulletsLeft;

	private List<VertexPositionColor> _lineVerts;

	private SinglePixelTexture _laserTexture;

	private ShooterPlayer _player;

	public GunSettings Settings { get; set; }

	public Gun(ShooterPlayer owner, World world, ContentManager contentManager, GunSettings gunSettings)
		: base(world)
	{
		Settings = gunSettings;
		_texture = contentManager.Load<Texture2D>("Shooter/Guns/" + Settings.SpritePath);
		_groundTexture = contentManager.Load<Texture2D>("Shooter/Guns/GunsOnPickup/" + Settings.Name + "_ground");
		_bulletTexture = contentManager.Load<Texture2D>("Shooter/Particles/BulletShort");
		_rocketTexture = contentManager.Load<Texture2D>("Shooter/Particles/Grenade");
		_rocketParticleEffect = contentManager.Load<Texture2D>("Shooter/Particles/ParticleSmoke");
		if (Settings.MuzzlePath != "None")
		{
			_muzzleTexture = contentManager.Load<Texture2D>("Shooter/Particles/" + Settings.MuzzlePath);
		}
		_contentManager = contentManager;
		_lineVerts = new List<VertexPositionColor>();
		_bulletsLeft = Settings.MagazineSize;
		_millSinceLastShot = 1000000;
		_laserTexture = new SinglePixelTexture(_bulletTexture.GraphicsDevice);
		_player = owner;
	}

	public override void Update(GameTime gameTime)
	{
		_millSinceLastShot += gameTime.ElapsedGameTime.Milliseconds;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, ShooterPlayer player)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_texture, position, null, Color.White, rotation, new Vector2(Settings.OffsetX, Settings.OffsetY), 1f, SpriteEffects.None, 0f);
		if (_millSinceLastShot < 17 && _muzzleTexture != null)
		{
			spriteBatch.Draw(_muzzleTexture, player.GetRelativePosition(Settings.MuzzleOffsetX, Settings.MuzzleOffsetY), null, Color.White, rotation, new Vector2(Settings.MuzzleSpriteOffsetX, Settings.MuzzleSpriteOffsetY), 1f, SpriteEffects.None, 0f);
		}
		spriteBatch.End();
	}

	public void Reload()
	{
		_bulletsLeft = Settings.MagazineSize;
	}

	public bool CanShoot()
	{
		float num = 60000f / (float)Settings.ShotsPerMin;
		if ((float)_millSinceLastShot > num && _bulletsLeft > 0)
		{
			return true;
		}
		return false;
	}

	public bool Shoot(Vector2 direction, Random random, ShooterPlayer player)
	{
		bool result = false;
		player.OnFireGun();
		float num = 60000f / (float)Settings.ShotsPerMin;
		if ((float)_millSinceLastShot > num && _bulletsLeft > 0)
		{
			Vector2 relativePosition = player.GetRelativePosition(Settings.MuzzleOffsetX, Settings.MuzzleOffsetY);
			_lineVerts.Clear();
			PerformRay(relativePosition, direction, isDirection: true);
			Body hitBody = null;
			if (!player.IsRayCollisionFromPlayerTo(player.GetRelativePosition(Settings.MuzzleOffsetX, Settings.MuzzleOffsetY), out hitBody))
			{
				float num2 = GeometryHelper.V2ToAngle(direction);
				List<Vector2> list = new List<Vector2>();
				float num3 = num2 - MathHelper.ToRadians((float)Settings.ProjectileSpread / 2f);
				float num4 = num2 + MathHelper.ToRadians((float)Settings.ProjectileSpread / 2f);
				float num5 = num4 - num3;
				float num6 = num5 / (float)Settings.ProjectileCount;
				float num7 = num3;
				if (Settings.RandomSpray)
				{
					for (int i = 0; i < Settings.ProjectileCount; i++)
					{
						float angle = MathHelper.Lerp(num3, num4, (float)random.NextDouble());
						Vector2 vector = GeometryHelper.AngleToV2(angle, 1f);
						RayResultData rayResultData = PerformRay(relativePosition, vector, isDirection: true);
						if (Vector2.Distance(ConvertUnits.ToDisplayUnits(rayResultData.position), relativePosition) < (float)Settings.ShotLength)
						{
							list.Add(rayResultData.position);
						}
						else
						{
							list.Add(ConvertUnits.ToSimUnits(relativePosition + vector * Settings.ShotLength));
						}
					}
				}
				else
				{
					for (int j = 0; j < Settings.ProjectileCount; j++)
					{
						Vector2 vector2 = GeometryHelper.AngleToV2(num7, 1f);
						RayResultData rayResultData2 = PerformRay(relativePosition, vector2, isDirection: true);
						if (Vector2.Distance(ConvertUnits.ToDisplayUnits(rayResultData2.position), relativePosition) < (float)Settings.ShotLength)
						{
							list.Add(rayResultData2.position);
						}
						else
						{
							list.Add(ConvertUnits.ToSimUnits(relativePosition + vector2 * Settings.ShotLength));
						}
						num7 += num6;
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					switch (Settings.ProjectileType)
					{
					case 0:
						ProjectileManager.AddShot(new Rocket(this, _world, _contentManager, _rocketTexture, _rocketParticleEffect, random, relativePosition, ConvertUnits.ToDisplayUnits(list[k]), 1.02f, instantExplosion: false, null));
						break;
					case 1:
						ProjectileManager.AddShot(new Grenade(this, _world, _contentManager, _rocketTexture, _rocketParticleEffect, random, relativePosition, ConvertUnits.ToDisplayUnits(list[k]), 20f, instantExplosion: false, null));
						break;
					case 2:
						ProjectileManager.AddShot(new LaserShot(_laserTexture, relativePosition, ConvertUnits.ToDisplayUnits(list[k])));
						break;
					case 3:
						ProjectileManager.AddShot(new GunShot(_bulletTexture, random, relativePosition, ConvertUnits.ToDisplayUnits(list[k]), 80f));
						break;
					case 4:
						ProjectileManager.AddShot(new GLGrenade(this, _world, _contentManager, _rocketTexture, _rocketParticleEffect, random, relativePosition, ConvertUnits.ToDisplayUnits(list[k]), 20f, instantExplosion: false, null));
						break;
					}
					if (Settings.ProjectileType != 2 && Settings.ProjectileType != 3)
					{
						continue;
					}
					Vector2 direction2 = ConvertUnits.ToDisplayUnits(list[k]);
					RayResultData rayResultData3 = PerformRay(relativePosition, direction2, isDirection: false);
					if (rayResultData3.body == null || rayResultData3.body.UserData == null)
					{
						continue;
					}
					if (rayResultData3.body.UserData is WorldRidgidBody)
					{
						WorldRidgidBody worldRidgidBody = rayResultData3.body.UserData as WorldRidgidBody;
						worldRidgidBody.Body.ApplyLinearImpulse(direction * 10f);
					}
					else
					{
						if (!(rayResultData3.body.UserData is ShooterPlayer))
						{
							continue;
						}
						ShooterPlayer shooterPlayer = rayResultData3.body.UserData as ShooterPlayer;
						if (shooterPlayer.IsAlive)
						{
							shooterPlayer.OnTakeDamage(player, Settings.DamageOnHit);
							if (!shooterPlayer.IsAlive)
							{
								player.OnKilledOpponent();
							}
						}
					}
				}
			}
			else if (hitBody != null && hitBody.UserData != null && hitBody.UserData != null)
			{
				_ = hitBody.UserData;
				for (int l = 0; l < Settings.ProjectileCount; l++)
				{
					switch (Settings.ProjectileType)
					{
					case 0:
						ProjectileManager.AddShot(new Rocket(this, _world, _contentManager, _rocketTexture, _rocketParticleEffect, random, GetOwnerPlayer().DisplayPosition, GetOwnerPlayer().DisplayPosition + direction, 1.02f, instantExplosion: true, null));
						break;
					case 1:
						ProjectileManager.AddShot(new Grenade(this, _world, _contentManager, _rocketTexture, _rocketParticleEffect, random, GetOwnerPlayer().DisplayPosition, GetOwnerPlayer().DisplayPosition + direction, 20f, instantExplosion: true, null));
						break;
					case 2:
						ProjectileManager.AddShot(new LaserShot(_laserTexture, GetOwnerPlayer().DisplayPosition, GetOwnerPlayer().DisplayPosition + direction));
						break;
					case 3:
						ProjectileManager.AddShot(new GunShot(_bulletTexture, random, GetOwnerPlayer().DisplayPosition, GetOwnerPlayer().DisplayPosition + direction, 80f));
						break;
					case 4:
						ProjectileManager.AddShot(new GLGrenade(this, _world, _contentManager, _rocketTexture, _rocketParticleEffect, random, GetOwnerPlayer().DisplayPosition, GetOwnerPlayer().DisplayPosition + direction, 20f, instantExplosion: true, null));
						break;
					}
					if (Settings.ProjectileType != 2 && Settings.ProjectileType != 3)
					{
						continue;
					}
					Vector2 direction3 = ConvertUnits.ToDisplayUnits(GetOwnerPlayer().DisplayPosition + direction * 50f);
					RayResultData rayResultData4 = PerformRay(GetOwnerPlayer().DisplayPosition, direction3, isDirection: false);
					Konsole.SumbitString(rayResultData4.body.UserData.ToString());
					if (rayResultData4.body == null || rayResultData4.body.UserData == null)
					{
						continue;
					}
					if (rayResultData4.body.UserData is WorldRidgidBody)
					{
						WorldRidgidBody worldRidgidBody2 = rayResultData4.body.UserData as WorldRidgidBody;
						worldRidgidBody2.Body.ApplyLinearImpulse(direction * 10f);
					}
					else
					{
						if (!(rayResultData4.body.UserData is ShooterPlayer))
						{
							continue;
						}
						ShooterPlayer shooterPlayer2 = rayResultData4.body.UserData as ShooterPlayer;
						Konsole.SumbitString(shooterPlayer2.GetID().ToString());
						if (shooterPlayer2.IsAlive)
						{
							shooterPlayer2.OnTakeDamage(player, Settings.DamageOnHit);
							if (!shooterPlayer2.IsAlive)
							{
								player.OnKilledOpponent();
							}
						}
					}
				}
			}
			_bulletsLeft--;
			if (Settings.ProjectileType != 2)
			{
				player.PullBackArms();
			}
			player.Body.ApplyLinearImpulse(direction * (0f - Settings.Recoil));
			_millSinceLastShot = 0;
			int num8 = random.Next(1, 3);
			if (num8 == 1)
			{
				player.Body.Rotation = player._lastLookAngle + MathHelper.ToRadians(Settings.SpreadDegrees / 2f);
			}
			else
			{
				player.Body.Rotation = player._lastLookAngle - MathHelper.ToRadians(Settings.SpreadDegrees / 2f);
			}
			result = true;
		}
		return result;
	}

	private RayResultData PerformRay(Vector2 start, Vector2 direction, bool isDirection)
	{
		List<Vector2> hitPos = new List<Vector2>();
		List<Body> bodies = new List<Body>();
		if (isDirection)
		{
			_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
			{
				if (f.Body.UserData != null && f.Body.UserData is Pickup pickup && !pickup.IsActive())
				{
					return -1f;
				}
				hitPos.Add(p);
				bodies.Add(f.Body);
				return 1f;
			}, ConvertUnits.ToSimUnits(start), ConvertUnits.ToSimUnits(start + direction * 10000f));
		}
		else
		{
			_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
			{
				if (f.Body.UserData != null && f.Body.UserData is Pickup pickup && !pickup.IsActive())
				{
					return -1f;
				}
				hitPos.Add(p);
				bodies.Add(f.Body);
				return 1f;
			}, ConvertUnits.ToSimUnits(start), ConvertUnits.ToSimUnits(direction));
		}
		float num = 1000000f;
		Vector2 position = Vector2.Zero;
		Body body = null;
		for (int num2 = 0; num2 < hitPos.Count; num2++)
		{
			float num3 = Vector2.Distance(ConvertUnits.ToSimUnits(start), hitPos[num2]);
			if (num3 < num)
			{
				num = num3;
				position = hitPos[num2];
				body = bodies[num2];
			}
		}
		return new RayResultData
		{
			body = body,
			position = position
		};
	}

	public int GetAmmoRemaining()
	{
		return _bulletsLeft;
	}

	public ShooterPlayer GetOwnerPlayer()
	{
		return _player;
	}

	public Texture2D GetGroundTexture()
	{
		return _groundTexture;
	}
}
