using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class LaserPhoton : Projectile
{
	public LaserPhoton(Ship owner)
		: base(owner, owner.Position, new Vector2(0f, -24f))
	{
		_physVolume.Radius = 2f;
		_shotDelay = 0;
		_damage = 1;
		_force = 1f;
		_splashRadius = 2f;
		_splashDamage = 0;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ProjectileLaser");
		base.Load(contentLoader);
	}
}
