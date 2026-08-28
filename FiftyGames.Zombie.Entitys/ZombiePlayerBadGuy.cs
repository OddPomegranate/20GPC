using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Entitys;

internal class ZombiePlayerBadGuy : BadGuy
{
	private ParticleParameters _runtimeCustomParticleParams;

	private Player _frameworkPlayer;

	private Texture2D _zombieBodyOverlay;

	private Texture2D _zombieBodyUnderlay;

	private Texture2D _zombieHead;

	private Texture2D _zombieLHandOverlay;

	private Texture2D _zombieLHandUnderlay;

	private Texture2D _zombieRHandOverlay;

	private Texture2D _zombieRHandUnderlay;

	public Player FrameworkPlayer => _frameworkPlayer;

	public ZombiePlayerBadGuy(Player frameworkPlayer, Vector2 position)
	{
		_frameworkPlayer = frameworkPlayer;
		Init(ZombieUtils.MiscSettings.PlayerZombieHealth, ZombieUtils.MiscSettings.PlayerZombieDamage, 40, 100, new Vector2(41f, 41f), 10, 50, "Zombie/Zombie");
		_body.Mass = 20f;
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart1"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart2"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart3"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Zombie/ZombiePart4"));
		_zombieBodyOverlay = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerZombieBodyOverlay");
		_zombieBodyUnderlay = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayZombieBodyUnderlay");
		_zombieHead = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerZombieHead");
		_zombieLHandOverlay = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerZombieLHandOverlay");
		_zombieLHandUnderlay = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerZombieLHandUnderlay");
		_zombieRHandOverlay = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerZombieRHandOverlay");
		_zombieRHandUnderlay = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/ZombiePlayer/PlayerZombieRHandUnderlay");
		CustomParticleDescriptor customParticleDescriptor = new CustomParticleDescriptor(ZombieUtils.ContentManager());
		_runtimeCustomParticleParams = customParticleDescriptor.ToParticleParameters();
		_position = position;
		_body.Position = ConvertUnits.ToSimUnits(_position);
		EnableBody();
	}

	public override int GetKillPoints()
	{
		return 0;
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

	public override void Update()
	{
		Vector2 vector = new Vector2(_frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y, _frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.X);
		Vector2 vector2 = new Vector2(_frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X, _frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y * -1f);
		if (vector.Length() > 0.5f)
		{
			_rotation = (float)Math.Atan2(_frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y * -1f, _frameworkPlayer.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.X);
		}
		float num = ZombieUtils.MiscSettings.PlayerZombieSpeed;
		Vector2 vector3 = vector2 * num;
		vector3 = (vector3 - _body.LinearVelocity) * _body.Mass;
		_body.LinearDamping = 1f;
		_body.ApplyLinearImpulse(ConvertUnits.ToSimUnits(vector3));
		_position = ConvertUnits.ToDisplayUnits(_body.Position);
		base.Update();
	}

	protected override void Draw()
	{
		if (base.IsAlive)
		{
			Vector2 vector = _position + ZombieUtils.Offset;
			Vector2 origin = new Vector2(30f, 25f);
			ZombieUtils.SpriteBatch.Begin();
			ZombieUtils.SpriteBatch.Draw(_zombieBodyUnderlay, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation, origin, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.Draw(_zombieBodyOverlay, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation, origin, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.Draw(_zombieLHandUnderlay, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation, origin, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.Draw(_zombieLHandOverlay, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation, origin, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.Draw(_zombieRHandUnderlay, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation, origin, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.Draw(_zombieRHandOverlay, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation, origin, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.Draw(_zombieHead, new Rectangle((int)vector.X, (int)vector.Y, 89, 55), null, Color.White, _rotation, origin, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.End();
		}
	}
}
