using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FiftyGames.Zombie.Entitys;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class CrawlerBadGuy2 : BadGuy
{
	private ParticleParameters _runtimeCustomParticleParams;

	public static AISettings Settings { get; set; }

	public CrawlerBadGuy2()
	{
		Init(Settings.Health, Settings.Damage, 20, 52, new Vector2(18f, 25f), 9, 4, "Zombie/OtherBadGuys/Zombie3");
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Crawler/CrawlerBody"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Crawler/CrawlerLeg"));
		_deathTextures.Add(ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Crawler/CrawlerTenticle"));
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
		_runtimeCustomParticleParams.MinAlpha = 0.6f;
		_runtimeCustomParticleParams.MaxAlpha = 1f;
		_runtimeCustomParticleParams.MinScale = new Vector2(0.7f, 0.7f);
		_runtimeCustomParticleParams.MaxScale = new Vector2(1.2f, 1.2f);
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
		_runtimeCustomParticleParams.MaxSpeed = 6f;
		_runtimeCustomParticleParams.MinColor = Vector3.One;
		_runtimeCustomParticleParams.MaxColor = Vector3.One;
		_runtimeCustomParticleParams.MinColorChange1 = Vector3.Zero;
		_runtimeCustomParticleParams.MinColorChange2 = Vector3.Zero;
		_runtimeCustomParticleParams.MaxColorChange1 = Vector3.Zero;
		_runtimeCustomParticleParams.MaxColorChange2 = Vector3.Zero;
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[0], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 5, 1000));
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[1], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 5, 1000));
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[2], _runtimeCustomParticleParams), rand.Next(1, 100), BlendState.AlphaBlend, _position, 1000, 5, 1000));
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
