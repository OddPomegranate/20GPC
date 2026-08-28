using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Bullet : Projectile
{
	public Bullet(Ship owner)
		: base(owner, owner.Position, new Vector2(0f, -8f))
	{
		_physVolume.Radius = 2f;
		_shotDelay = 80;
		_damage = 3;
		_force = 9f;
		_splashRadius = 10f;
		_splashDamage = 0;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ProjectileBullet");
		base.Load(contentLoader);
	}
}
