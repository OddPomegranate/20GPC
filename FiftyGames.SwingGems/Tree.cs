using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SwingGems;

internal class Tree
{
	private const float maxTreePeek = 400f;

	private const float depthToSpeedOffset = 1.2f;

	private Texture2D sprite;

	private Vector2 position;

	private float depthLevel;

	public static int sortParam(Tree t1, Tree t2)
	{
		if (t1.depthLevel == t2.depthLevel)
		{
			return 0;
		}
		if (t1.depthLevel > t2.depthLevel)
		{
			return 1;
		}
		return -1;
	}

	public Tree(Texture2D treeSprite, float xPosition, Random inRandomGenerator, float minDepth, float maxDepth)
	{
		sprite = treeSprite;
		position = new Vector2(xPosition, 720f - (float)(inRandomGenerator.NextDouble() * 400.0 * (double)depthLevel));
		depthLevel = MathHelper.Lerp(minDepth, maxDepth, (float)inRandomGenerator.NextDouble());
	}

	public bool Update(float framePositionStep)
	{
		position.X -= framePositionStep * (depthLevel * 1.2f);
		if (position.X < -600f)
		{
			return true;
		}
		return false;
	}

	public float getDepth()
	{
		return depthLevel;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(sprite, position, null, Color.White, 0f, Vector2.UnitY * sprite.Height, depthLevel, SpriteEffects.None, 0f);
	}

	public Vector2 getPosition()
	{
		return position;
	}

	public void setPosition(Vector2 inPosition)
	{
		position = inPosition;
	}
}
