using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Shockwave : Projectile
{
	public Shockwave(Ship owner)
		: base(owner, owner.Position, new Vector2(0f, -4f))
	{
		_physVolume.Radius = 2f;
		_shotDelay = 1000;
		_damage = 0;
		_force = 4f;
		_splashRadius = 120f;
		_splashDamage = 0;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ProjectileShockwave");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		_velocity.Y -= 0.1f;
		base.Update(gameTime);
	}
}
