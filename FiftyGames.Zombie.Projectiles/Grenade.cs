using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FiftyGames.Zombie.DynamicLights;
using FiftyGames.Zombie.Entitys;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Projectiles;

internal class Grenade : Projectile
{
	private int _damage;

	private Texture2D _sprite;

	private double _startTime;

	private double _timerDuration;

	private List<Body> _sprayBalls;

	private bool _deployedSpray;

	private double _totalElapsed;

	private Texture2D _explosion;

	private ParticleParameters _runtimeCustomParticleParams;

	private bool _hit;

	private Texture2D _grenade;

	private ZombiePlayer _owner;

	public int Damage => _damage;

	public Grenade(Vector2 position, Vector2 vector, int damage, ZombiePlayer owner)
	{
		_owner = owner;
		_damage = damage;
		_sprite = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Node");
		_body = BodyFactory.CreateCircle(ZombieUtils.World(), ConvertUnits.ToSimUnits(10), 0f);
		_body.Position = ConvertUnits.ToSimUnits(position);
		_body.BodyType = BodyType.Dynamic;
		_body.Friction = 0f;
		_body.Restitution = 0.4f;
		_body.Mass = 1f;
		_body.LinearDamping = 3f;
		_body.UserData = this;
		_body.OnCollision += _body_OnCollision;
		vector.Normalize();
		_body.ApplyForce(vector * 1000f);
		_body.ApplyAngularImpulse(1f);
		_startTime = ZombieUtils.Stopwatch.Elapsed.TotalSeconds;
		_timerDuration = 3.0;
		_sprayBalls = new List<Body>();
		_deployedSpray = false;
		_explosion = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ParticleSprites/Explosion");
		CustomParticleDescriptor customParticleDescriptor = new CustomParticleDescriptor(ZombieUtils.ContentManager());
		_runtimeCustomParticleParams = customParticleDescriptor.ToParticleParameters();
		_runtimeCustomParticleParams.SpritePath = "Zombie/ParticleSprites/Explosion";
		_grenade = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Grenade");
	}

	private bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (fixtureB.Body.UserData is Entity entity)
		{
			if (entity is ZombiePlayer)
			{
				_hit = false;
				return false;
			}
			_hit = true;
			return true;
		}
		if (fixtureB.Body.UserData is Line)
		{
			return true;
		}
		return false;
	}

	public override void Update(GameTime gameTime)
	{
		_totalElapsed += gameTime.TotalGameTime.Milliseconds;
		if ((!(ZombieUtils.Stopwatch.Elapsed.TotalSeconds - _startTime > _timerDuration) && !_hit) || _deployedSpray || _body == null)
		{
			return;
		}
		_runtimeCustomParticleParams.Origin = new Vector2(128f, 128f);
		_runtimeCustomParticleParams.MinAlpha = 0.6f;
		_runtimeCustomParticleParams.MaxAlpha = 1f;
		_runtimeCustomParticleParams.MinScale = new Vector2(0.1f, 0.1f);
		_runtimeCustomParticleParams.MaxScale = new Vector2(0.2f, 0.2f);
		_runtimeCustomParticleParams.MinRotation = -0.3f;
		_runtimeCustomParticleParams.MaxRotation = 0.3f;
		_runtimeCustomParticleParams.Change = 30;
		_runtimeCustomParticleParams.Directional = false;
		_runtimeCustomParticleParams.Multiplicative = 0f;
		_runtimeCustomParticleParams.MinScaleChange1 = Vector2.One * 0.025f;
		_runtimeCustomParticleParams.MaxScaleChange1 = Vector2.One * 0.125f;
		_runtimeCustomParticleParams.MinScaleChange2 = Vector2.One * 0.025f;
		_runtimeCustomParticleParams.MaxScaleChange2 = Vector2.One * 0.15f;
		_runtimeCustomParticleParams.MinAlphaChange1 = -0.075f;
		_runtimeCustomParticleParams.MaxAlphaChange1 = -0.075f;
		_runtimeCustomParticleParams.MinAlphaChange2 = -0.1f;
		_runtimeCustomParticleParams.MaxAlphaChange2 = -0.1f;
		_runtimeCustomParticleParams.MinSpeed = 30f;
		_runtimeCustomParticleParams.MaxSpeed = 125f;
		_runtimeCustomParticleParams.MinDirection = 0f;
		_runtimeCustomParticleParams.MaxDirection = 1000f;
		_runtimeCustomParticleParams.MinColor = new Vector3(1f, 0.5f, 0.5f);
		_runtimeCustomParticleParams.MaxColor = Vector3.One;
		_runtimeCustomParticleParams.MinColorChange1 = Vector3.Zero;
		_runtimeCustomParticleParams.MinColorChange2 = Vector3.Zero;
		_runtimeCustomParticleParams.MaxColorChange1 = Vector3.Zero;
		_runtimeCustomParticleParams.MaxColorChange2 = Vector3.Zero;
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_explosion, _runtimeCustomParticleParams), ZombieUtils.Random.Next(1, 100), BlendState.Additive, ConvertUnits.ToDisplayUnits(_body.Position), 1000, 40, 1000));
		ZombieUtils.DynamicLightMaskManager.Add(new ExplosionDynamicLight(ZombieUtils.ContentManager(), ConvertUnits.ToDisplayUnits(_body.Position)));
		ZombieUtils.PlaySound("Explosion");
		for (int i = 0; i < ZombieUtils.Players.Count; i++)
		{
			((ZombiePlayer)ZombieUtils.Players[i]).FrameworkPlayer.GamePadManager.StartVibration(500);
		}
		for (int j = 0; j < ZombieUtils.BadGuys.Count; j++)
		{
			if (ZombieUtils.BadGuys[j].PhysBody == null || !(Vector2.Distance(ConvertUnits.ToDisplayUnits(_body.Position), ConvertUnits.ToDisplayUnits(ZombieUtils.BadGuys[j].PhysBody.Position)) < (float)ZombieUtils.MiscSettings.GrenadeKillRadius))
			{
				continue;
			}
			bool behindWall = false;
			ZombieUtils.World().RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
			{
				if (f.Body.UserData != null && f.Body.UserData is Line)
				{
					behindWall = true;
				}
				return -1f;
			}, ZombieUtils.BadGuys[j].PhysBody.Position, _body.Position);
			if (!behindWall)
			{
				float num = Vector2.Distance(_body.Position, ZombieUtils.BadGuys[j].PhysBody.Position);
				Vector2 vector = ZombieUtils.BadGuys[j].PhysBody.Position - _body.Position;
				vector.Normalize();
				ZombieUtils.BadGuys[j].PhysBody.LinearVelocity = 1000f * num * vector;
				ZombieUtils.BadGuys[j].TakeDamage(_damage, vector);
				if (ZombieUtils.BadGuys[j].Health <= 0f)
				{
					_owner.Score += ZombieUtils.BadGuys[j].GetKillPoints();
				}
			}
		}
		for (int num2 = 0; num2 < ZombieUtils.Players.Count; num2++)
		{
			ZombiePlayerBadGuy zombiePlayerBadGuy = ZombieUtils.Players[num2] as ZombiePlayerBadGuy;
			if (ZombieUtils.Players[num2].PhysBody == null || !(Vector2.Distance(ConvertUnits.ToDisplayUnits(_body.Position), ConvertUnits.ToDisplayUnits(ZombieUtils.Players[num2].PhysBody.Position)) < (float)ZombieUtils.MiscSettings.GrenadeKillRadius))
			{
				continue;
			}
			bool behindWall2 = false;
			ZombieUtils.World().RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
			{
				if (f.Body.UserData != null && f.Body.UserData is Line)
				{
					behindWall2 = true;
				}
				return -1f;
			}, ZombieUtils.Players[num2].PhysBody.Position, _body.Position);
			if (!behindWall2)
			{
				float num3 = Vector2.Distance(_body.Position, ZombieUtils.Players[num2].PhysBody.Position);
				Vector2 vector2 = ZombieUtils.Players[num2].PhysBody.Position - _body.Position;
				vector2.Normalize();
				ZombieUtils.Players[num2].PhysBody.LinearVelocity = 1000f * num3 * vector2;
				if (ZombieUtils.Players[num2] == _owner || zombiePlayerBadGuy != null)
				{
					float num4 = Vector2.Distance(ZombieUtils.Players[num2].PhysBody.Position, _body.Position);
					float damage = (float)_damage * (3f / num4);
					ZombieUtils.Players[num2].TakeDamage(damage, vector2);
				}
				if (zombiePlayerBadGuy == null)
				{
					((ZombiePlayer)ZombieUtils.Players[num2]).LastDirectionOfDamage = ZombieUtils.Players[num2].Position;
				}
			}
		}
		ZombieUtils.ShudderTimer += ZombieUtils.MiscSettings.ExplosionShudderTimer;
		_deployedSpray = true;
		base.IsAlive = false;
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		if (base.IsAlive)
		{
			spriteBatch.Begin();
			Vector2 vector = base.Position + offset;
			spriteBatch.Draw(destinationRectangle: new Rectangle((int)vector.X, (int)vector.Y, _grenade.Width, _grenade.Height), texture: _grenade, sourceRectangle: null, color: Color.White, rotation: GeometryHelper.V2ToAngle(_body.LinearVelocity), origin: new Vector2(_grenade.Width / 2, _grenade.Height / 2), effects: SpriteEffects.None, layerDepth: 0f);
			spriteBatch.End();
		}
	}
}
