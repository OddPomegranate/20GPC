using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FiftyGames.Shooter;
using FiftyGames.Zombie.Utils;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Entities;
using Shooter.ISHelpers;
using Shooter.World_Ridgid_Bodies;

namespace Shooter.Guns;

internal class GLGrenade : Shot
{
	private Vector2 _currentPosition;

	private float _speed;

	private Random _random;

	private Texture2D _rocketTexture;

	private Texture2D _rocketParticleEffect;

	private ParticleParameters _particleParams;

	private CustomParticleDescriptor _particleDescriptor;

	private ParticleEmitter _explosionEmitter;

	private Body _body;

	private Gun _gun;

	private bool _explodedAtEnd;

	private ShooterPlayer _playerHit;

	private bool _instantExplosion;

	private ShooterPlayer _instantExplosionPlayerHit;

	private Vector2 _playerHitPosition;

	public GLGrenade(Gun owner, World world, ContentManager contentManager, Texture2D rocketTexture, Texture2D smokeParticles, Random random, Vector2 start, Vector2 end, float speed, bool instantExplosion, ShooterPlayer playerHit)
	{
		_instantExplosion = instantExplosion;
		_instantExplosionPlayerHit = playerHit;
		if (playerHit != null)
		{
			_playerHitPosition = playerHit.DisplayPosition;
		}
		_rocketTexture = rocketTexture;
		_rocketParticleEffect = smokeParticles;
		_start = start;
		_end = end;
		_direction = end - start;
		_direction.Normalize();
		_end -= _direction * 3f;
		_speed = speed;
		_currentPosition = start;
		_random = random;
		_speed = speed;
		_gun = owner;
		base.IsDead = false;
		_explodedAtEnd = false;
		_ = _start - _direction * 120f;
		_particleDescriptor = new CustomParticleDescriptor(contentManager);
		_particleParams = _particleDescriptor.ToParticleParameters();
		_particleParams.MinColor = new Vector3(0f, 0f, 0f);
		_particleParams.MaxColor = new Vector3(0.5f, 0.5f, 0.5f);
		_particleParams.MinColorChange1 = new Vector3(0f, 0f, 0f);
		_particleParams.MaxColorChange1 = new Vector3(0.05f, 0.05f, 0.05f);
		_particleParams.MinSpeed = 0f;
		_particleParams.MaxSpeed = 2f;
		_particleParams.MaxScale = new Vector2(0.5f, 0.5f);
		_particleParams.MinScale = new Vector2(0.2f, 0.2f);
		_explosionEmitter = new ParticleEmitter(new RuntimeParticleDescriptor(_rocketParticleEffect, _particleParams), 1, BlendState.AlphaBlend, _end, 100, 100, 100);
		if (!instantExplosion)
		{
			_body = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(10), 10f);
			_body.OnCollision += body_OnCollision;
			_body.UserData = this;
		}
	}

	private bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (fixtureB.Body.UserData != null)
		{
			if (fixtureB.Body.UserData is ShooterPlayer playerHit)
			{
				_playerHit = playerHit;
				OnExplode();
				_explodedAtEnd = false;
			}
			else if (fixtureB.Body.UserData is WorldRidgidBody)
			{
				_playerHit = null;
				OnExplode();
				_explodedAtEnd = false;
			}
		}
		return false;
	}

	public override void Update(GameTime gameTime)
	{
		if (_instantExplosion)
		{
			_playerHit = _instantExplosionPlayerHit;
			_explodedAtEnd = true;
			OnExplode();
		}
		else
		{
			if (_body == null)
			{
				return;
			}
			if (base.IsDead && !_body.IsDisposed)
			{
				_body.Dispose();
			}
			else if (!_body.IsDisposed)
			{
				float num = Vector2.Distance(_currentPosition, _end);
				float num2 = Vector2.Distance(_currentPosition + _direction * _speed, _end);
				if (num > num2)
				{
					_currentPosition += _direction * _speed;
					_body.Position = ConvertUnits.ToSimUnits(_currentPosition);
					return;
				}
				_currentPosition = _end;
				_body.Dispose();
				_explodedAtEnd = true;
				_playerHit = null;
				OnExplode();
				base.IsDead = true;
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_rocketTexture, _currentPosition, null, Color.DarkGray, GeometryHelper.V2ToAngle(_direction), new Vector2(16f, 16f), 0.5f, SpriteEffects.None, 0f);
		spriteBatch.End();
	}

	private void OnExplode()
	{
		base.IsDead = true;
		ParticleEngine.AddEmitter(_explosionEmitter);
		List<ShooterPlayer> allPlayers = _gun.GetOwnerPlayer().GetAllPlayers();
		foreach (ShooterPlayer item in allPlayers)
		{
			if (item.IsAlive && item != _playerHit)
			{
				float num = Vector2.Distance(_currentPosition, item.DisplayPosition) - 35f;
				float num2 = 70f;
				Konsole.SumbitString(num.ToString());
				if (!(num < num2))
				{
					continue;
				}
				Vector2 destinationPoint = _currentPosition;
				if (_explodedAtEnd)
				{
					destinationPoint = _end;
				}
				List<object> list = new List<object>();
				list.Add(this);
				list.Add(_gun.GetOwnerPlayer());
				Body hitObject = null;
				if (!item.IsRayCollisionFromPlayerTo(destinationPoint, list, out hitObject))
				{
					_ = Vector2.Zero;
					Vector2 vector = (_explodedAtEnd ? _direction : (_currentPosition - item.DisplayPosition));
					vector.Normalize();
					float value = MathHelper.Lerp(1f, 0f, num / num2);
					value = MathHelper.Clamp(value, 0f, 1f);
					Konsole.SumbitString(value.ToString());
					int damage = (int)((float)_gun.Settings.DamageOnHit * value);
					Konsole.SumbitString(damage.ToString());
					item.OnTakeDamage(_gun.GetOwnerPlayer(), damage);
					if (!item.IsAlive)
					{
						_gun.GetOwnerPlayer().OnKilledOpponent();
					}
					else
					{
						item.Body.ApplyLinearImpulse(-vector * _gun.Settings.ForceOnWorldObjects * value);
					}
				}
			}
			else if (item.IsAlive)
			{
				Vector2 vector2 = _currentPosition - item.DisplayPosition;
				vector2.Normalize();
				item.OnTakeDamage(_gun.GetOwnerPlayer(), _gun.Settings.DamageOnHit);
				if (!item.IsAlive)
				{
					_gun.GetOwnerPlayer().OnKilledOpponent();
				}
				else
				{
					item.Body.ApplyLinearImpulse(-vector2 * _gun.Settings.ForceOnWorldObjects);
				}
			}
		}
	}
}
