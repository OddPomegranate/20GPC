using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Impossible;

internal class BackgroundHandler
{
	private const int caveIncrementLimit = 60;

	private Texture2D backgroundSprite;

	private Vector2 screenSize = new Vector2(400f, 150f);

	private List<Vector2> backgroundElements = new List<Vector2>();

	private int caveUnitLength;

	private int caveIncrementCounter;

	public BackgroundHandler(GraphicsDevice graphicsDevice, Texture2D caveElementImage)
	{
		backgroundSprite = caveElementImage;
		caveUnitLength = (int)(RobsMath.TruncF(screenSize.X / (float)backgroundSprite.Width) + 2f);
		for (int i = 0; i < caveUnitLength; i++)
		{
			backgroundElements.Add(new Vector2(i * backgroundSprite.Width, 0f));
		}
	}

	public void Update(float gameSpeed)
	{
		caveIncrementCounter--;
		if (caveIncrementCounter < 0)
		{
			caveIncrementCounter = (int)RobsMath.TruncF(60f / gameSpeed);
			for (int i = 0; i < backgroundElements.Count; i++)
			{
				backgroundElements[i] -= Vector2.UnitX;
			}
			if (backgroundElements.ElementAt(0).X < (float)(-backgroundSprite.Width))
			{
				backgroundElements.RemoveAt(0);
				backgroundElements.Add(new Vector2(backgroundElements.ElementAt(backgroundElements.Count - 1).X + (float)backgroundSprite.Width, 0f));
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < backgroundElements.Count; i++)
		{
			spriteBatch.Draw(backgroundSprite, backgroundElements[i], null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
	}

	private void addNewCaveElement(bool graceActive)
	{
	}
}
