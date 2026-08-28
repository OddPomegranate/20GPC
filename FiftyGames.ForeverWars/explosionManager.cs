using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class explosionManager
{
	private List<explosionEvent> explosionList = new List<explosionEvent>();

	private Texture2D explosionSheet;

	private Random randomGenerator;

	private gridSystem gridManager;

	public explosionManager(GraphicsDevice graphicsDevice, ContentManager contentManager, Random inRandomGenerator, gridSystem inGridManager)
	{
		explosionSheet = contentManager.Load<Texture2D>("ForeverWars/Sprites/ExplosionSheet");
		randomGenerator = inRandomGenerator;
		gridManager = inGridManager;
	}

	public void addExplosion(Vector2 position, float scale, explosionType typeOfExplosion)
	{
		switch (typeOfExplosion)
		{
		case explosionType.tiny:
			explosionList.Add(new explosionEvent(explosionSheet, randomGenerator, position, explosionType.tiny, gridManager, scale));
			break;
		case explosionType.small:
			explosionList.Add(new explosionEvent(explosionSheet, randomGenerator, position, explosionType.small, gridManager, scale));
			break;
		case explosionType.large:
			explosionList.Add(new explosionEvent(explosionSheet, randomGenerator, position, explosionType.large, gridManager, scale));
			break;
		case explosionType.tinySmoke:
			explosionList.Add(new explosionEvent(explosionSheet, randomGenerator, position, explosionType.tinySmoke, gridManager, scale));
			break;
		case explosionType.smallSmoke:
			explosionList.Add(new explosionEvent(explosionSheet, randomGenerator, position, explosionType.smallSmoke, gridManager, scale));
			break;
		}
	}

	public void Update()
	{
		for (int i = 0; i < explosionList.Count; i++)
		{
			if (explosionList[i].Update())
			{
				explosionList.RemoveAt(i);
				i--;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, RenderTarget2D currentRenderTargetInUse)
	{
		foreach (explosionEvent explosion in explosionList)
		{
			explosion.Draw(spriteBatch);
		}
	}
}
