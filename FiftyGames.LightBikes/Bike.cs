using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.LightBikes;

internal class Bike
{
	private const float controllerForcedDeadZone = 0.3f;

	private const int deathLimit = 90;

	private const int particleLimit = 20;

	private const float particleSpeed = 3f;

	private const int AISIGHTRANGE = 20;

	private const int AIREACTIONCHANCE = 3;

	private const int startDistanceFromBorder = 15;

	private Player m_Player;

	private Vector2 m_Position;

	private int gridXPosition;

	private int gridYPosition;

	private int initialDirection;

	private Color m_Colour;

	private float m_Scale;

	private bool m_Alive;

	private float m_Rotation;

	private Texture2D m_pixelSprite;

	private Vector2 m_Origin;

	private int initialGridX;

	private int initialGridY;

	private int deathCounter;

	private bool leftPressed;

	private bool RightPressed;

	private bool leftDigitalPressed;

	private bool RightDigitalPressed;

	private int directionFlag;

	private Random m_Random;

	private float tempAngle;

	private List<Vector4> particalList = new List<Vector4>();

	private bool AIMODE;

	private int maxGridx;

	private int maxGridy;

	private PlayerManager pManager;

	private List<Color> validColors;

	private List<int> AIdirectionList = new List<int>();

	private int playerIndex;

	public Bike(Player player, Vector2 position, float scale, Texture2D pixel, int gridX, int gridY, Random inRand, PlayerManager inPManager, bool inAimode, Grid gridManager, int inPlayerIndex, List<Color> validColorList)
	{
		playerIndex = inPlayerIndex;
		m_Random = inRand;
		AIMODE = inAimode;
		pManager = inPManager;
		validColors = validColorList;
		if (!AIMODE)
		{
			m_Player = player;
			m_Colour = pManager.GetPlayerColor(player);
		}
		else
		{
			m_Player = null;
			m_Colour = validColorList[m_Random.Next(validColorList.Count())];
		}
		m_Position = position;
		maxGridx = gridManager.gridArray.GetLength(0);
		maxGridy = gridManager.gridArray.GetLength(1);
		switch (playerIndex)
		{
		case 0:
			gridXPosition = 15;
			initialGridX = 15;
			gridYPosition = maxGridy / 2;
			initialGridY = maxGridy / 2;
			directionFlag = 1;
			break;
		case 1:
			gridXPosition = maxGridx / 2;
			initialGridX = maxGridx / 2;
			gridYPosition = 15;
			initialGridY = 15;
			directionFlag = 2;
			break;
		case 2:
			gridXPosition = maxGridx - 15;
			initialGridX = maxGridx - 15;
			gridYPosition = maxGridy / 2;
			initialGridY = maxGridy / 2;
			directionFlag = 3;
			break;
		case 3:
			gridXPosition = maxGridx / 2;
			initialGridX = maxGridx / 2;
			gridYPosition = maxGridy - 15;
			initialGridY = maxGridy - 15;
			directionFlag = 0;
			break;
		}
		m_Scale = 1f;
		m_Alive = true;
		initialDirection = directionFlag;
		m_pixelSprite = pixel;
		m_Origin = new Vector2((float)m_pixelSprite.Width / 2f, (float)m_pixelSprite.Height / 2f);
		if (AIMODE)
		{
			resetBike();
		}
	}

	public void doAIProcessing(Grid inGrid)
	{
		bool flag = m_Random.NextDouble() > 0.5;
		if (AIdirectionList.Count != 0)
		{
			if (AIdirectionList[0] == -1)
			{
				directionFlag--;
			}
			else if (AIdirectionList[0] == 1)
			{
				directionFlag++;
			}
			AIdirectionList.RemoveAt(0);
		}
		else
		{
			if (AIcheckForCollision(20, directionFlag, inGrid) == -1)
			{
				return;
			}
			int directionToCheck = wrapDirection(directionFlag + ((!flag) ? 1 : (-1)));
			AIcheckForCollision(20, directionToCheck, inGrid);
			if (AIcheckForCollision(20, directionToCheck, inGrid) == -1)
			{
				if (m_Random.Next(3) == 1)
				{
					directionFlag += ((!flag) ? 1 : (-1));
				}
				return;
			}
			directionToCheck = wrapDirection(directionFlag + (flag ? 1 : (-1)));
			AIcheckForCollision(20, directionToCheck, inGrid);
			if (AIcheckForCollision(20, directionToCheck, inGrid) == -1)
			{
				if (m_Random.Next(3) == 1)
				{
					directionFlag += (flag ? 1 : (-1));
				}
			}
			else if (AIcheckForCollision(20, directionFlag, inGrid) < 3 && m_Random.Next(3) == 1)
			{
				int num = AIcheckForCollision(20, wrapDirection(directionFlag - 1), inGrid);
				int num2 = AIcheckForCollision(20, wrapDirection(directionFlag + 1), inGrid);
				if (num == -1)
				{
					num += 20;
				}
				if (num2 == -1)
				{
					num2 += 20;
				}
				if (num < num2)
				{
					directionFlag++;
				}
				else
				{
					directionFlag--;
				}
			}
		}
	}

	public void Update(Grid inGrid)
	{
		if (m_Alive)
		{
			if (!AIMODE)
			{
				if (m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X < -0.3f && !leftPressed)
				{
					leftPressed = true;
					directionFlag--;
				}
				if (m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X > -0.3f && leftPressed)
				{
					leftPressed = false;
				}
				if (m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X > 0.3f && !RightPressed)
				{
					RightPressed = true;
					directionFlag++;
				}
				if (m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X < 0.3f && RightPressed)
				{
					RightPressed = false;
				}
				if (m_Player.GamePadManager.GamePadStateCurrent.DPad.Left == ButtonState.Pressed && !leftDigitalPressed)
				{
					leftDigitalPressed = true;
					directionFlag--;
				}
				if (m_Player.GamePadManager.GamePadStateCurrent.DPad.Left == ButtonState.Released && leftDigitalPressed)
				{
					leftDigitalPressed = false;
				}
				if (m_Player.GamePadManager.GamePadStateCurrent.DPad.Right == ButtonState.Pressed && !RightDigitalPressed)
				{
					RightDigitalPressed = true;
					directionFlag++;
				}
				if (m_Player.GamePadManager.GamePadStateCurrent.DPad.Right == ButtonState.Released && RightDigitalPressed)
				{
					RightDigitalPressed = false;
				}
			}
			if (AIMODE)
			{
				doAIProcessing(inGrid);
			}
			directionFlag = wrapDirection(directionFlag);
			inGrid.setGridElement(gridXPosition, gridYPosition, m_Colour);
			switch (directionFlag)
			{
			case 0:
				gridYPosition--;
				break;
			case 1:
				gridXPosition++;
				break;
			case 2:
				gridYPosition++;
				break;
			case 3:
				gridXPosition--;
				break;
			}
			m_Rotation = (float)directionFlag * ((float)Math.PI / 2f);
			m_Position = new Vector2(inGrid.getScreenPosition().X + (float)(gridXPosition * inGrid.getPixelGap()), inGrid.getScreenPosition().Y + (float)(gridYPosition * inGrid.getPixelGap()));
			if (inGrid.getWall(gridXPosition, gridYPosition) || gridXPosition < 1 || gridXPosition > inGrid.getGridWidth() - 1 || gridYPosition < 1 || gridYPosition > inGrid.getGridHeight() - 1)
			{
				m_Alive = false;
				LightBikesHelper.soundManager.CreateGameSoundCue("theGameOfLifeCycles Die").Play();
				deathCounter = 90;
				for (int i = 0; i < 20; i++)
				{
					particalList.Add(new Vector4((float)(m_Random.NextDouble() * 6.2831854820251465), (float)(m_Random.NextDouble() * 3.0), m_Position.X, m_Position.Y));
				}
			}
		}
		else if (deathCounter > 0)
		{
			deathCounter--;
		}
	}

	public bool isAlive()
	{
		return m_Alive;
	}

	public int AIcheckForCollision(int sightRange, int directionToCheck, Grid playingGrid)
	{
		int result = -1;
		switch (directionToCheck)
		{
		case 0:
		{
			for (int num2 = gridYPosition; num2 > gridYPosition - sightRange; num2--)
			{
				if (playingGrid.getWall(gridXPosition, num2))
				{
					result = gridYPosition - num2;
					break;
				}
			}
			break;
		}
		case 1:
		{
			for (int i = gridXPosition; i < gridXPosition + sightRange; i++)
			{
				if (playingGrid.getWall(i, gridYPosition))
				{
					result = i - gridXPosition;
					break;
				}
			}
			break;
		}
		case 2:
		{
			for (int j = gridYPosition; j < gridYPosition + sightRange; j++)
			{
				if (playingGrid.getWall(gridXPosition, j))
				{
					result = j - gridYPosition;
					break;
				}
			}
			break;
		}
		case 3:
		{
			for (int num = gridXPosition; num > gridXPosition - sightRange; num--)
			{
				if (playingGrid.getWall(num, gridYPosition))
				{
					result = gridXPosition - num;
					break;
				}
			}
			break;
		}
		}
		return result;
	}

	public int wrapDirection(int inDir)
	{
		if (inDir < 0)
		{
			return 3;
		}
		if (inDir > 3)
		{
			return 0;
		}
		return inDir;
	}

	public void resetBike()
	{
		m_Alive = true;
		if (AIMODE)
		{
			gridXPosition = initialGridX;
			gridYPosition = initialGridY;
			m_Colour = validColors[m_Random.Next(validColors.Count())];
			directionFlag = m_Random.Next(4);
		}
		else
		{
			directionFlag = initialDirection;
			gridXPosition = initialGridX;
			gridYPosition = initialGridY;
		}
		particalList.Clear();
	}

	public Player getPlayer()
	{
		return m_Player;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		_ = Vector2.Zero;
		if (!m_Alive && deathCounter > 0)
		{
			for (int i = 0; i < 20; i++)
			{
				Vector2 vector = AngleToV2(particalList.ElementAt(i).X, particalList.ElementAt(i).Y * ((float)deathCounter / 90f) + 1f);
				particalList[i] = new Vector4(particalList.ElementAt(i).X, particalList.ElementAt(i).Y, particalList.ElementAt(i).Z + vector.X, particalList.ElementAt(i).W + vector.Y);
				spriteBatch.Draw(position: new Vector2(particalList.ElementAt(i).Z, particalList.ElementAt(i).W), texture: m_pixelSprite, sourceRectangle: null, color: m_Colour, rotation: m_Rotation, origin: m_Origin, scale: 3f, effects: SpriteEffects.None, layerDepth: 0f);
			}
		}
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
