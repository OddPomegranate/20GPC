using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Grenade : Projectile
{
	protected const float Friction = 0.1f;

	protected const float Spin = 0.1f;

	protected int _fuse;

	public int Fuse => _fuse;

	public Grenade(Ship owner)
		: base(owner, owner.Position, new Vector2(0f, -12f))
	{
		_physVolume.Radius = 4f;
		_shotDelay = 1000;
		_damage = 65;
		_force = 25f;
		_splashRadius = 150f;
		_splashDamage = 40;
		_fuse = 3000;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ProjectileGrenade");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		Vector2 vector = -_velocity;
		vector.Normalize();
		_velocity += vector * 0.1f;
		float num = 0.1f * _velocity.LengthSquared();
		if (num > 0.1f)
		{
			num = 0.1f;
		}
		_rotation += num;
		if (_fuse != 0)
		{
			_fuse -= gameTime.ElapsedGameTime.Milliseconds;
			if (_fuse < 0)
			{
				_fuse = 0;
			}
		}
		base.Update(gameTime);
	}
}
