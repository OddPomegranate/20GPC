using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.HeliChopper;

internal class Cave
{
	private const int caveStep = 12;

	private const int caveMaxSpeed = 17;

	private const int caveStartSpeed = 7;

	private const int AICAVESPEEDMAX = 10;

	private List<CaveElement> m_caveRoof = new List<CaveElement>();

	private List<CaveElement> m_caveFloor = new List<CaveElement>();

	private int caveMid;

	private int caveWid;

	private int caveCount;

	private int caveUpDown;

	private int caveMove;

	private int caveSizeChange;

	private int caveSpeed = 15;

	private int speedCounterLimit = 120;

	private int speedCounter;

	private int caveMaxWid;

	private int caveMinWid;

	private Texture2D debugTexture;

	private int caveUnitLength;

	private Vector2 screenAttributes = Vector2.Zero;

	private Texture2D caveImageHolder;

	private Random randomGenerator;

	private int graphicsViewportOver2;

	private bool lastGraceActive;

	public Cave(GraphicsDevice graphicsDevice, Texture2D caveElementImage, Texture2D debug)
	{
		randomGenerator = new Random();
		debugTexture = debug;
		caveSpeed = 7;
		caveMid = graphicsDevice.Viewport.Height / 2;
		caveWid = graphicsDevice.Viewport.Height / 2;
		graphicsViewportOver2 = graphicsDevice.Viewport.Height / 2;
		screenAttributes = new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
		caveImageHolder = caveElementImage;
		caveMaxWid = 500;
		caveMinWid = 200;
		caveUnitLength = (int)(RobsMath.TruncF(screenAttributes.X / (float)caveElementImage.Width) + 2f);
		for (int i = 0; i < caveUnitLength; i++)
		{
			m_caveRoof.Add(new CaveElement(new Vector2(i * caveElementImage.Width, caveMid - caveWid / 2), caveElementImage, inverted: false, debugTexture));
			m_caveFloor.Add(new CaveElement(new Vector2(i * caveElementImage.Width, caveMid + caveWid / 2), caveElementImage, inverted: true, debugTexture));
		}
	}

	public void Update(Copter[] m_Copters, bool graceActive, bool AIMODE)
	{
		if (!graceActive)
		{
			speedCounter--;
			if (speedCounter < 0)
			{
				speedCounter = speedCounterLimit;
				if (caveSpeed < 17)
				{
					caveSpeed++;
				}
			}
		}
		else if (caveSpeed > 7)
		{
			speedCounter -= 5;
			if (speedCounter < 0)
			{
				speedCounter = speedCounterLimit;
				caveSpeed--;
			}
		}
		if (AIMODE && caveSpeed > 10)
		{
			caveSpeed = 10;
		}
		if (lastGraceActive && !graceActive && caveSpeed > 7)
		{
			caveSpeed = 7;
		}
		lastGraceActive = graceActive;
		for (int i = 0; i < m_caveRoof.Count(); i++)
		{
			m_caveRoof.ElementAt(i).setXPosition(m_caveRoof.ElementAt(i).getXPosition() - (float)caveSpeed);
			m_caveFloor.ElementAt(i).setXPosition(m_caveFloor.ElementAt(i).getXPosition() - (float)caveSpeed);
		}
		if (m_caveRoof.ElementAt(0).getXPosition() < (float)(-m_caveRoof.ElementAt(0).getImageWidth()))
		{
			m_caveRoof.RemoveAt(0);
			m_caveFloor.RemoveAt(0);
			addNewCaveElement(graceActive);
		}
		foreach (CaveElement item in m_caveRoof)
		{
			item.setcollisionBox(new BoundingBox(new Vector3(item.getPosition().X - (float)(item.getSprite().Width / 2), item.getPosition().Y - (float)item.getSprite().Height, 0f), new Vector3(new Vector2(item.getPosition().X + (float)(item.getSprite().Width / 2), item.getPosition().Y - 10f), 0f)));
		}
		foreach (CaveElement item2 in m_caveFloor)
		{
			item2.setcollisionBox(new BoundingBox(new Vector3(item2.getPosition().X - (float)(item2.getSprite().Width / 2), item2.getPosition().Y + 10f, 0f), new Vector3(new Vector2(item2.getPosition().X + (float)(item2.getSprite().Width / 2), item2.getPosition().Y + (float)item2.getSprite().Height), 0f)));
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < m_caveRoof.Count(); i++)
		{
			m_caveRoof.ElementAt(i).Draw(spriteBatch);
		}
		for (int j = 0; j < m_caveFloor.Count(); j++)
		{
			m_caveFloor.ElementAt(j).Draw(spriteBatch);
		}
	}

	private int randNumber(int num1, int num2)
	{
		return randomGenerator.Next(num1) - num2;
	}

	public List<CaveElement> getRoof()
	{
		return m_caveRoof;
	}

	public List<CaveElement> getFloor()
	{
		return m_caveFloor;
	}

	private void addNewCaveElement(bool graceActive)
	{
		m_caveRoof.Add(new CaveElement(new Vector2(m_caveRoof.ElementAt(m_caveRoof.Count() - 1).getXPosition() + (float)caveImageHolder.Width, caveMid - caveWid / 2), caveImageHolder, inverted: false, debugTexture));
		m_caveFloor.Add(new CaveElement(new Vector2(m_caveFloor.ElementAt(m_caveFloor.Count() - 1).getXPosition() + (float)caveImageHolder.Width, caveMid + caveWid / 2), caveImageHolder, inverted: true, debugTexture));
		if (!graceActive)
		{
			caveCount--;
			if (caveCount < 1)
			{
				caveUpDown = randNumber(3, 1);
				caveCount = 10;
			}
			if (caveUpDown == -1 && caveMid - caveWid / 2 > 50)
			{
				caveMid -= 12;
			}
			if (caveUpDown == 1 && (float)(caveMid + caveWid / 2) < screenAttributes.Y - 50f)
			{
				caveMid += 12;
			}
			caveSizeChange = randNumber(3, 1);
			if (caveSizeChange == 1 && caveWid < caveMaxWid)
			{
				caveWid += 12;
			}
			if (caveSizeChange == -1 && caveWid > caveMinWid)
			{
				caveWid -= 12;
			}
		}
		else
		{
			if (caveMid > graphicsViewportOver2 && caveMid - caveWid / 2 > 50)
			{
				caveMid -= 12;
			}
			if (caveMid < graphicsViewportOver2 && (float)(caveMid + caveWid / 2) < screenAttributes.Y - 50f)
			{
				caveMid += 12;
			}
			if (caveWid > graphicsViewportOver2)
			{
				caveWid -= 12;
			}
			if (caveWid < graphicsViewportOver2)
			{
				caveWid += 12;
			}
		}
	}
}
