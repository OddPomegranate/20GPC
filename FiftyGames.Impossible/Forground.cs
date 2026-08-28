using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Impossible;

internal class Forground
{
	private const int buildingIncrementMax = 1;

	private const int maxBuildingSpace = 150;

	private const int buildingProbLimit = 100;

	private const int buildProbChance = 10;

	private List<tower> m_buildings = new List<tower>();

	private float buildingSpeed = 1f;

	private float MaxBuildingSpeed = 4f;

	private int buildingIncrementCounter;

	private ContentManager contentManagerReferance;

	private int caveUnitLength;

	private Vector2 screenAttributes = Vector2.Zero;

	private towerImages towerImageHolder;

	private Random randomGenerator;

	private GraphicsDevice graphicsDeviceReferance;

	public Forground(GraphicsDevice graphicsDevice, ContentManager inContent, float gameSpeed, towerImages inImageHolder)
	{
		graphicsDeviceReferance = graphicsDevice;
		randomGenerator = new Random();
		contentManagerReferance = inContent;
		towerImageHolder = inImageHolder;
		for (int i = 0; i < 7; i++)
		{
			switch (i)
			{
			case 0:
			case 2:
			case 4:
			case 6:
				m_buildings.Add(new tower(graphicsDevice, contentManagerReferance, towerImageHolder, towerType.StartingBlockB, Vector4.Zero, null));
				break;
			case 1:
			case 3:
			case 5:
			case 7:
				m_buildings.Add(new tower(graphicsDevice, contentManagerReferance, towerImageHolder, towerType.StartingBlockA, Vector4.Zero, null));
				break;
			}
			m_buildings[i].incrementXPosition(i * 56);
		}
		for (int j = 0; j < 200; j++)
		{
			Update(gameSpeed, initFlag: true);
		}
	}

	public void Update(float gameSpeed, bool initFlag)
	{
		buildingIncrementCounter--;
		if (buildingIncrementCounter < 0)
		{
			buildingIncrementCounter = (int)RobsMath.TruncF(1f / gameSpeed);
			foreach (tower building in m_buildings)
			{
				building.decrementXPosition(buildingSpeed);
				if (!initFlag && buildingSpeed < MaxBuildingSpeed)
				{
					buildingSpeed += 0.0001f;
				}
				building.setCollisionBox(new BoundingBox(new Vector3(building.getPosition().X, 132f - building.getBuildingHeight() * 6f, 0f), new Vector3(building.getPosition().X + (float)building.getWidth(), 135f - building.getBuildingHeight() * 6f, 0f)));
				building.setSideCollisionBox(new BoundingBox(new Vector3(building.getPosition().X, 132f - building.getBuildingHeight() * 6f + 1f, 0f), new Vector3(building.getPosition().X + 3f, 451f, 0f)));
				building.setRoofCollisionBox(new BoundingBox(new Vector3(building.getPosition().X, building.calculateRoofHeight(), 0f), new Vector3(building.getPosition().X + (float)building.getWidth(), building.calculateRoofHeight() - 2f, 0f)));
				building.setRoofSideCollisionBox();
			}
		}
		if (m_buildings.ElementAt(0).getPosition().X < (float)(-m_buildings[0].getWidth()))
		{
			m_buildings.RemoveAt(0);
		}
		if ((float)m_buildings[m_buildings.Count - 1].getWidth() + m_buildings[m_buildings.Count - 1].getPosition().X < 400f && (randomGenerator.Next(100) < 10 || m_buildings[m_buildings.Count - 1].getPosition().X < 300f))
		{
			switch (randomGenerator.Next(8))
			{
			case 1:
				m_buildings.Add(new tower(graphicsDeviceReferance, contentManagerReferance, towerImageHolder, towerType.Tiny, m_buildings[m_buildings.Count - 1].getDimensionVector(), randomGenerator));
				break;
			case 2:
			case 3:
				m_buildings.Add(new tower(graphicsDeviceReferance, contentManagerReferance, towerImageHolder, towerType.Medium, m_buildings[m_buildings.Count - 1].getDimensionVector(), randomGenerator));
				break;
			case 4:
			case 5:
				m_buildings.Add(new tower(graphicsDeviceReferance, contentManagerReferance, towerImageHolder, towerType.Big, m_buildings[m_buildings.Count - 1].getDimensionVector(), randomGenerator));
				break;
			case 6:
			case 7:
				m_buildings.Add(new tower(graphicsDeviceReferance, contentManagerReferance, towerImageHolder, towerType.Huge, m_buildings[m_buildings.Count - 1].getDimensionVector(), randomGenerator));
				break;
			default:
				m_buildings.Add(new tower(graphicsDeviceReferance, contentManagerReferance, towerImageHolder, towerType.Huge, m_buildings[m_buildings.Count - 1].getDimensionVector(), randomGenerator));
				break;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		foreach (tower building in m_buildings)
		{
			building.Draw(spriteBatch);
		}
	}

	public List<tower> getTowerList()
	{
		return m_buildings;
	}

	private int randNumber(int num1, int num2)
	{
		return randomGenerator.Next(num1) - num2;
	}

	private void addNewCaveElement(bool graceActive)
	{
	}
}
