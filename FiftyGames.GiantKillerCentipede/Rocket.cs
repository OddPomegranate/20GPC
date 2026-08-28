using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Rocket : Projectile
{
	public Rocket(Ship owner)
		: base(owner, owner.Position, new Vector2(0f, -1f))
	{
		_physVolume.Radius = 4f;
		_shotDelay = 500;
		_damage = 22;
		_force = 18f;
		_splashRadius = 50f;
		_splashDamage = 10;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ProjectileRocket");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		_velocity.Y -= 0.1f;
		base.Update(gameTime);
	}
}
