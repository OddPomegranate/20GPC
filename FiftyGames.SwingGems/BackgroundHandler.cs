using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SwingGems;

internal class BackgroundHandler
{
	private const float forgroundDepthMultiplier = 2f;

	private const int treeRandomChance = 1;

	private const int treeRandomChangeMax = 100;

	private const float minForgroundDepth = 1f;

	private const float maxForgroundDepth = 1.9f;

	private const float speedmultiplier = 0.3f;

	private Texture2D backgroundSprite;

	private Texture2D tree1;

	private Texture2D tree2;

	private Texture2D tree3;

	private Vector2 screenSize = new Vector2(400f, 150f);

	private Random randomGen;

	private List<float> backgroundList = new List<float>();

	private List<Tree> forgroundList = new List<Tree>();

	private List<Tree> tempForgroundList;

	public BackgroundHandler(GraphicsDevice graphicsDevice, ContentManager contentManager, Random inRand)
	{
		backgroundSprite = contentManager.Load<Texture2D>("SwingGems/Sprites/Background");
		tree1 = contentManager.Load<Texture2D>("SwingGems/Sprites/Tree1");
		tree2 = contentManager.Load<Texture2D>("SwingGems/Sprites/Tree2");
		tree3 = contentManager.Load<Texture2D>("SwingGems/Sprites/Tree3");
		randomGen = inRand;
		for (int i = 0; (float)i < 1280f / ((float)backgroundSprite.Width - 1f) + 1f; i++)
		{
			backgroundList.Add(i * backgroundSprite.Width - 1);
		}
		for (int j = 0; (float)j < 4.169381f; j++)
		{
			addForgroundTree(j * 307);
		}
	}

	public void Update(float framePositionIncrement)
	{
		float num = framePositionIncrement * 0.3f;
		for (int i = 0; i < backgroundList.Count; i++)
		{
			backgroundList[i] -= num;
		}
		if (backgroundList[0] < (float)(-backgroundSprite.Width))
		{
			backgroundList.RemoveAt(0);
		}
		if (backgroundList[backgroundList.Count - 1] + (float)backgroundSprite.Width < 1281f)
		{
			backgroundList.Add(backgroundList[backgroundList.Count - 1] + (float)backgroundSprite.Width);
		}
		for (int j = 0; j < forgroundList.Count; j++)
		{
			if (forgroundList[j].Update(num))
			{
				forgroundList.RemoveAt(j);
			}
		}
		if (randomGen.Next(100) < 1)
		{
			addForgroundTree(1280f);
		}
		forgroundList.Sort(Tree.sortParam);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		foreach (float background in backgroundList)
		{
			float x = background;
			spriteBatch.Draw(backgroundSprite, new Vector2(x, 0f), Color.White);
		}
		foreach (Tree forground in forgroundList)
		{
			forground.Draw(spriteBatch);
		}
	}

	public void addForgroundTree(float xPosition)
	{
		switch (randomGen.Next(3))
		{
		case 0:
			forgroundList.Add(new Tree(tree1, xPosition, randomGen, 1f, 1.9f));
			break;
		case 1:
			forgroundList.Add(new Tree(tree2, xPosition, randomGen, 1f, 1.9f));
			break;
		case 2:
			forgroundList.Add(new Tree(tree3, xPosition, randomGen, 1f, 1.9f));
			break;
		}
	}
}
