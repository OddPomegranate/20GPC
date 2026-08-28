using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class DefenceParticle : Projectile
{
	public DefenceParticle(Ship owner)
		: base(owner, owner.Position, new Vector2(13f, 0f))
	{
		_physVolume.Radius = 2f;
		_shotDelay = 0;
		_damage = 3;
		_force = 30f;
		_splashRadius = 10f;
		_splashDamage = 0;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ProjectileDefence");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		if (_position.X + _velocity.X < 0f || _position.X + _velocity.X > 1280f || _position.Y + _velocity.X < 552f)
		{
			_alive = false;
		}
		base.Update(gameTime);
	}
}
