using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SwingGems;

internal class Cave
{
	private List<CaveElement> m_caveRoof = new List<CaveElement>();

	private List<CaveElement> m_caveFloor = new List<CaveElement>();

	private Random randomGenerator;

	private ContentManager contentManager;

	private int elementCreateCounter;

	private int elementCreateCounterMax = 300;

	private static CaveImages caveImageHolder;

	public Cave(GraphicsDevice graphicsDevice, ContentManager inContentManager, int randomSeed)
	{
		if (randomSeed != -1)
		{
			randomGenerator = new Random(randomSeed);
		}
		else
		{
			randomGenerator = new Random();
		}
		contentManager = inContentManager;
		randomGenerator.Next();
		byte[] buffer = new byte[300];
		randomGenerator.NextBytes(buffer);
		caveImageHolder = new CaveImages();
		caveImageHolder.roof1 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof1");
		caveImageHolder.roof2 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof2");
		caveImageHolder.roof3 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof3");
		caveImageHolder.roof4 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof4");
		caveImageHolder.roof5 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof5");
		caveImageHolder.roof6 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof6");
		caveImageHolder.roof7 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof7");
		caveImageHolder.roof8 = contentManager.Load<Texture2D>("SwingGems/Sprites/Roof8");
		caveImageHolder.DEBUGTexture = inContentManager.Load<Texture2D>("HeliChopper/Sprites/debugPixel");
		m_caveRoof.Add(new CaveElement(new Vector2(0f, 0f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: false, startingZoneOverRide: true));
		m_caveRoof.Add(new CaveElement(new Vector2(400f, 0f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: false, startingZoneOverRide: true));
		m_caveRoof.Add(new CaveElement(new Vector2(800f, 0f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: false, startingZoneOverRide: true));
		m_caveRoof.Add(new CaveElement(new Vector2(1200f, 0f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: false, startingZoneOverRide: true));
		m_caveFloor.Add(new CaveElement(new Vector2(0f, 720f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: true, startingZoneOverRide: true));
		m_caveFloor.Add(new CaveElement(new Vector2(400f, 720f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: true, startingZoneOverRide: true));
		m_caveFloor.Add(new CaveElement(new Vector2(800f, 720f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: true, startingZoneOverRide: true));
		m_caveFloor.Add(new CaveElement(new Vector2(1200f, 720f), inContentManager, randomGenerator, caveImageHolder, inIsFloor: true, startingZoneOverRide: true));
	}

	public void Update(Gem[] m_Gems, float screenPositionincrement, bool graceActive)
	{
		foreach (CaveElement item in m_caveRoof)
		{
			item.Update(screenPositionincrement);
		}
		foreach (CaveElement item2 in m_caveFloor)
		{
			item2.Update(screenPositionincrement);
		}
		if (m_caveRoof[m_caveRoof.Count - 1].getXPosPlusWid() < 1280f)
		{
			m_caveRoof.Add(new CaveElement(new Vector2(1280f, 0f), contentManager, randomGenerator, caveImageHolder, inIsFloor: false, startingZoneOverRide: false));
		}
		if (m_caveFloor[m_caveFloor.Count - 1].getXPosPlusWid() < 1280f)
		{
			m_caveFloor.Add(new CaveElement(new Vector2(1280f, 720f), contentManager, randomGenerator, caveImageHolder, inIsFloor: true, startingZoneOverRide: false));
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		foreach (CaveElement item in m_caveRoof)
		{
			item.Draw(spriteBatch);
		}
		foreach (CaveElement item2 in m_caveFloor)
		{
			item2.Draw(spriteBatch);
		}
	}

	public List<CaveElement> getRoof()
	{
		return m_caveRoof;
	}

	public List<CaveElement> getFloor()
	{
		return m_caveFloor;
	}
}
