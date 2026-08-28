using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RRInSpace;

internal class Blocker
{
	private Vector2 position;

	private Vector2 origin;

	private Texture2D spriteImage;

	private BoundingBox collisionBox = default(BoundingBox);

	public Blocker(GraphicsDevice graphicsDevice, ContentManager inContent, Vector2 inPosition)
	{
		position = inPosition;
		spriteImage = inContent.Load<Texture2D>("RRinSpace/Sprites/Blocker");
		origin = new Vector2(spriteImage.Width / 2, spriteImage.Height / 2);
		collisionBox = new BoundingBox(Vector3.Zero, new Vector3(spriteImage.Width, spriteImage.Height, 0f));
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(spriteImage, position, Color.White);
	}

	public Texture2D getSprite()
	{
		return spriteImage;
	}

	public Vector2 getPosition()
	{
		return position;
	}

	public Vector2 getOrigin()
	{
		return origin;
	}
}
