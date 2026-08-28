using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class explosionEvent
{
	private const int largeExplosionNumberOfSubExplosionsMax = 4;

	private const int largeExplosionSubCounterMax = 5;

	private const float LargeExplosionSubRadi = 100f;

	private const float LargeExplosionSubRadiAdditional = 30f;

	private List<explosionElement> explosionElementList = new List<explosionElement>();

	private Texture2D explosionSprite;

	private Random randomGen;

	private int largeExplosionNumberOfSubExplosions;

	private int largeExplosionSubCounter = 5;

	private Vector2 position;

	private explosionType typeOfExplosion;

	private gridSystem gridManager;

	private float scale;

	public explosionEvent(Texture2D explosionSheet, Random inRand, Vector2 InPosition, explosionType InTypeOfExplosion, gridSystem inGridManager, float inScale)
	{
		gridManager = inGridManager;
		randomGen = inRand;
		position = InPosition;
		explosionSprite = explosionSheet;
		typeOfExplosion = InTypeOfExplosion;
		scale = inScale;
		if (inScale == 0f)
		{
			scale = 1f;
		}
		switch (typeOfExplosion)
		{
		case explosionType.tiny:
			explosionElementList.Add(new explosionElement(explosionSheet, InPosition, (float)inRand.NextDouble() * ((float)Math.PI * 2f), 0.1f * scale, explosionColor.Yellow, explosionType.tiny, inRand.NextDouble() < 0.5, gridManager));
			break;
		case explosionType.small:
			explosionElementList.Add(new explosionElement(explosionSheet, InPosition, (float)inRand.NextDouble() * ((float)Math.PI * 2f), 0.8f, explosionColor.Yellow, explosionType.small, inRand.NextDouble() < 0.5, gridManager));
			break;
		case explosionType.large:
		{
			explosionElementList.Add(new explosionElement(explosionSheet, InPosition, (float)inRand.NextDouble() * ((float)Math.PI * 2f), 1f, explosionColor.Yellow, explosionType.large, inRand.NextDouble() < 0.5, gridManager));
			for (int i = 0; i < 4; i++)
			{
				float num = (float)i / 4f * ((float)Math.PI * 2f);
				Vector2 inPosition = position + AngleToV2(num + (float)inRand.NextDouble() * ((float)Math.PI * 2f), 100f + 30f * (float)largeExplosionNumberOfSubExplosions);
				explosionElementList.Add(new explosionElement(explosionSprite, inPosition, (float)randomGen.NextDouble() * ((float)Math.PI * 2f), 1f, explosionColor.Yellow, explosionType.large, inRand.NextDouble() < 0.5, gridManager));
			}
			break;
		}
		case explosionType.tinySmoke:
			explosionElementList.Add(new explosionElement(explosionSheet, InPosition, (float)inRand.NextDouble() * ((float)Math.PI * 2f), 0.1f * scale, explosionColor.Grey, explosionType.tiny, inRand.NextDouble() < 0.5, gridManager));
			break;
		case explosionType.smallSmoke:
			explosionElementList.Add(new explosionElement(explosionSheet, InPosition, (float)inRand.NextDouble() * ((float)Math.PI * 2f), 0.7f * scale, explosionColor.Grey, explosionType.small, inRand.NextDouble() < 0.5, gridManager));
			break;
		}
	}

	public bool Update()
	{
		if (typeOfExplosion == explosionType.large)
		{
			largeExplosionSubCounter--;
			if (largeExplosionSubCounter < 0)
			{
				largeExplosionSubCounter = 5;
				if (largeExplosionNumberOfSubExplosions <= 4)
				{
					largeExplosionNumberOfSubExplosions++;
					for (int i = 0; i < 8; i++)
					{
						float angle = (float)i / 8f * ((float)Math.PI * 2f);
						Vector2 inPosition = position + AngleToV2(angle, 100f + 30f * (float)largeExplosionNumberOfSubExplosions);
						explosionElementList.Add(new explosionElement(explosionSprite, inPosition, (float)randomGen.NextDouble() * ((float)Math.PI * 2f), 0.3f, explosionColor.Grey, explosionType.smallSmoke, randomGen.NextDouble() < 0.5, gridManager));
					}
				}
			}
		}
		for (int j = 0; j < explosionElementList.Count; j++)
		{
			if (explosionElementList[j].Update())
			{
				explosionElementList.RemoveAt(j);
				j--;
			}
		}
		if (explosionElementList.Count == 0)
		{
			return true;
		}
		return false;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		foreach (explosionElement explosionElement in explosionElementList)
		{
			explosionElement.Draw(spriteBatch);
		}
	}

	public float V2ToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.Y, vector.X);
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
