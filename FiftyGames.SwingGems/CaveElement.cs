using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SwingGems;

internal class CaveElement
{
	private Vector2 m_Position;

	private Vector2 adjustedPosition;

	private Texture2D m_Sprite;

	private Rectangle dimensions;

	private BoundingBox collisionBox;

	private Texture2D DEBUGTexture;

	private bool isFloor;

	private float textureHeight;

	public CaveElement(Vector2 position, ContentManager inContentManager, Random inRand, CaveImages inCaveImageHolder, bool inIsFloor, bool startingZoneOverRide)
	{
		m_Position = position;
		if (startingZoneOverRide)
		{
			m_Sprite = inCaveImageHolder.roof1;
		}
		else
		{
			switch (inRand.Next(8))
			{
			case 0:
				m_Sprite = inCaveImageHolder.roof1;
				break;
			case 1:
				m_Sprite = inCaveImageHolder.roof2;
				break;
			case 2:
				m_Sprite = inCaveImageHolder.roof3;
				break;
			case 3:
				m_Sprite = inCaveImageHolder.roof4;
				break;
			case 4:
				m_Sprite = inCaveImageHolder.roof5;
				break;
			case 5:
				m_Sprite = inCaveImageHolder.roof6;
				break;
			case 6:
				m_Sprite = inCaveImageHolder.roof7;
				break;
			case 7:
				m_Sprite = inCaveImageHolder.roof8;
				break;
			}
		}
		DEBUGTexture = inCaveImageHolder.DEBUGTexture;
		isFloor = inIsFloor;
		textureHeight = m_Sprite.Height;
		Update(0f);
	}

	public float getXPosPlusWid()
	{
		return m_Position.X + (float)m_Sprite.Width;
	}

	public void Update(float positionIncrement)
	{
		m_Position.X -= positionIncrement;
		adjustedPosition = m_Position;
		if (isFloor)
		{
			adjustedPosition.Y -= textureHeight;
		}
		collisionBox = new BoundingBox(new Vector3(adjustedPosition, 0f), new Vector3(adjustedPosition.X + (float)m_Sprite.Width, adjustedPosition.Y + (float)m_Sprite.Height, 0f));
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(m_Sprite, adjustedPosition, null, Color.White, 0f, Vector2.Zero, 1f, isFloor ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
	}

	public BoundingBox getBoundingBox()
	{
		return collisionBox;
	}
}
