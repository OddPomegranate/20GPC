using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Heatseeker : Projectile
{
	public const float TurnSpeed = 0.01f;

	private BodySegment _target;

	public BodySegment Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	public Heatseeker(Ship owner)
		: base(owner, owner.Position, new Vector2(0f, -3f))
	{
		_physVolume.Radius = 4f;
		_shotDelay = 600;
		_maxVelocity = 5f;
		_damage = 9;
		_force = 16f;
		_splashRadius = 50f;
		_splashDamage = 4;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ProjectileHeatseeker");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (_target != null && _target.Health == 0)
		{
			_target = null;
		}
		Vector2 vector = _position - (_position + _velocity);
		_rotation = (float)Math.Atan2(vector.Y, vector.X);
	}
}
