using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.HeliChopper;

internal class CaveElement
{
	private Vector2 m_Position;

	private Texture2D m_Sprite;

	private bool m_Inverted;

	private BoundingBox collisionBox;

	private Texture2D debugSprite;

	public CaveElement(Vector2 position, Texture2D sprite, bool inverted, Texture2D indebugSprite)
	{
		m_Position = position;
		m_Sprite = sprite;
		m_Inverted = inverted;
		debugSprite = indebugSprite;
	}

	public void Update()
	{
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(m_Sprite, m_Position, null, Color.White, 0f, m_Inverted ? new Vector2(m_Sprite.Width / 2, 0f) : new Vector2(m_Sprite.Width / 2, m_Sprite.Height), 1f, (!m_Inverted) ? SpriteEffects.FlipVertically : SpriteEffects.None, 1f);
	}

	public Vector2 getPosition()
	{
		return m_Position;
	}

	public Texture2D getSprite()
	{
		return m_Sprite;
	}

	public Vector2 getOrigin()
	{
		if (!m_Inverted)
		{
			return new Vector2(m_Sprite.Width / 2, m_Sprite.Height);
		}
		return new Vector2(m_Sprite.Width / 2, 0f);
	}

	public float getXPosition()
	{
		return m_Position.X;
	}

	public void setXPosition(float inX)
	{
		m_Position.X = inX;
	}

	public int getImageWidth()
	{
		return m_Sprite.Width;
	}

	public BoundingBox getCollisionBox()
	{
		return collisionBox;
	}

	public void setcollisionBox(BoundingBox inBox)
	{
		collisionBox = inBox;
	}
}
