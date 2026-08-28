using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Pickups.Guns;

internal class GunPickup : Pickup
{
	public GunPickup(World world, ContentManager contentManager, Vector2 position)
		: base(world, contentManager, position, "")
	{
	}

	public override void Update(GameTime gameTime)
	{
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
	}
}
