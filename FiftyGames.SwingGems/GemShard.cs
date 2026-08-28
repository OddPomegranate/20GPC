using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SwingGems;

internal class GemShard
{
	private Vector2 position;

	private Vector2 momentum;

	private float rotation;

	private Vector2 origin;

	private Random randgen;

	private Color color;

	private Texture2D sprite;

	private bool RotationDirection;

	private float initialMomentumRange = 2f;

	private float scale;

	public GemShard(Color gemColor, Vector2 gemPosition, Vector2 gemMomentum, Texture2D shardSprite, Random inRand, float inScale)
	{
		randgen = inRand;
		color = gemColor;
		scale = inScale;
		position = gemPosition;
		sprite = shardSprite;
		origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
		RotationDirection = randgen.NextDouble() > 0.5;
		momentum = gemMomentum;
		momentum += new Vector2((float)randgen.NextDouble() * (initialMomentumRange * 2f) - initialMomentumRange, (float)randgen.NextDouble() * (initialMomentumRange * 2f) - initialMomentumRange);
	}

	public void Update(float screenIncrementPosition)
	{
		position.X -= screenIncrementPosition;
		if (RotationDirection)
		{
			rotation += 0.1f;
		}
		else
		{
			rotation -= 0.1f;
		}
		momentum.Y += 0.2f;
		position += momentum;
	}

	public void Draw(SpriteBatch spritebatch)
	{
		spritebatch.Draw(sprite, position, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
	}
}
