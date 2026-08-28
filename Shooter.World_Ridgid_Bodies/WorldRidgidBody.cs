using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.World_Ridgid_Bodies;

internal class WorldRidgidBody : PhysObject
{
	protected Texture2D _texture;

	public WorldRidgidBody(World world, ContentManager contentManager, string texturePath, Vector2 position)
		: base(world)
	{
		_texture = contentManager.Load<Texture2D>(texturePath);
	}

	public override void Update(GameTime gameTime)
	{
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_texture, base.DisplayPosition, null, Color.White, _body.Rotation, new Vector2(_texture.Width / 2, _texture.Height / 2), 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
	}
}
