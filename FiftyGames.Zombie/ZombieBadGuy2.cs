using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FiftyGames.Zombie.Entitys;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class ZombieBadGuy2 : BadGuy
{
	private ParticleParameters _runtimeCustomParticleParams;

	public static AISettings Settings { get; set; }

	public ZombieBadGuy2()
	{
		Init(Settings.Health, Settings.Damage, 40, 100, new Vector2(41f, 41f), 10, 50, "Zombie/OtherBadGuys/Zombie4");
		_body.Mass = 10f;
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart1"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart2"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart3"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart4"));
		CustomParticleDescriptor customParticleDescriptor = new CustomParticleDescriptor(ZombieUtils.ContentManager());
		_runtimeCustomParticleParams = customParticleDescriptor.ToParticleParameters();
		base.UsePath = Settings.UsePath;
	}

	protected override bool OnHitOtherFixtureObject(Fixture other, Contact contact)
	{
		return base.OnHitOtherFixtureObject(other, contact);
	}

	protected override void OnDeath()
	{
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
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[0], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 20, 1000));
		_runtimeCustomParticleParams.MinDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) - MathHelper.ToRadians(30f);
		_runtimeCustomParticleParams.MaxDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) + MathHelper.ToRadians(30f);
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[1], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 20, 1000));
		_runtimeCustomParticleParams.MinDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) - MathHelper.ToRadians(40f);
		_runtimeCustomParticleParams.MaxDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) + MathHelper.ToRadians(40f);
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[2], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 10, 1000));
		_runtimeCustomParticleParams.MaxSpeed = 5f;
		_runtimeCustomParticleParams.MinDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) - MathHelper.ToRadians(160f);
		_runtimeCustomParticleParams.MaxDirection = (float)Math.Atan2(_lastDirectionOfDamage.X, _lastDirectionOfDamage.Y) + MathHelper.ToRadians(160f);
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[3], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 15, 1000));
	}

	public override int GetKillPoints()
	{
		return Settings.KillPoints;
	}

	public override void Update()
	{
		int closestPlayer = GetClosestPlayer();
		if (closestPlayer != -1)
		{
			MoveTowardsPoint(ZombieUtils.Players[closestPlayer].Position, Settings.Speed);
			_rotation = GeometryHelper.TurnToFace(_position, ZombieUtils.Players[closestPlayer].Position, _rotation, Settings.TurnSpeed);
		}
		else
		{
			Vector2 defaultZombieGotoPosition = ZombieUtils.DefaultZombieGotoPosition;
			MoveTowardsPoint(defaultZombieGotoPosition, Settings.Speed);
			_rotation = GeometryHelper.TurnToFace(_position, defaultZombieGotoPosition, _rotation, Settings.TurnSpeed);
		}
		if (_body != null)
		{
			_position = ConvertUnits.ToDisplayUnits(_body.Position);
			_collisionCircle.Position = _position;
		}
		base.Update();
	}

	protected override void Draw()
	{
		base.Draw();
	}
}
