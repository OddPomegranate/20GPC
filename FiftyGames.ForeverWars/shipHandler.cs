using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class shipHandler
{
	private const int targetNumberOfEnemies = 4;

	private const float percentageRatioOfRareShips = 0.05f;

	private const float percentageRatioOfUncommonShips = 0.1f;

	private const float percentageRatioOfStations = 0.1f;

	private const int hudFadeInCounterMax = 30;

	private const int hudWaitCounterMax = 60;

	private const int tutorialEnemiesCounterMax = 6;

	private const int AIShipDestroyTimerMax = 600;

	private List<shipModule> shipTemplates = new List<shipModule>();

	private List<string> shipFileNamesCommon = new List<string>();

	private List<string> shipFileNamesUncommon = new List<string>();

	private List<string> shipFileNamesRare = new List<string>();

	private List<string> shipFileNamesStation = new List<string>();

	private List<shipDummy> listOfImageShips = new List<shipDummy>();

	private shipData shipSpritesHolder;

	private StreamReader shipConfigStreamReader;

	private explosionManager explosionManagerRef;

	private GraphicsDevice graphicsDevice;

	private ContentManager contentManager;

	private Random randGen;

	private bool isBossFight;

	private string currentBossName = "";

	private gridSystem gridManager;

	private float hudBossWarningCounter = 2f;

	private int hudFadeInCounter = 30;

	private int hudWaitCounter = 60;

	private bool forcedShipSpawn;

	private bool stationExists;

	private int tutorialEnemiesCounter;

	private bool AIModeEnabled;

	private int AIShipDestroyTimer = 600;

	public void forceShipSpawn(string shipName, bool isBoss)
	{
		isBossFight = isBoss;
		forcedShipSpawn = true;
		shipTemplates.Clear();
		currentBossName = addShipConfig(shipTemplates, shipName, new Vector2(300f, 300f), 0f, isBoss, isStation: false, isDummy: false);
	}

	public List<shipDummy> getDummyShipList()
	{
		return listOfImageShips;
	}

	public string addShipConfig(List<shipModule> listToAddTo, string nameOfShipConfigFile, Vector2 initialPosition, float initialRotation, bool isBoss, bool isStation, bool isDummy)
	{
		shipConfigStreamReader = new StreamReader(nameOfShipConfigFile);
		int num = 0;
		int num2 = 0;
		string text = "";
		while (num == 0 || num2 == 0)
		{
			string text2 = shipConfigStreamReader.ReadLine();
			switch (text2.ElementAt(0))
			{
			case '@':
				switch (text2.ElementAt(1))
				{
				case '1':
					num2 = ((!isDummy) ? 1 : (-1));
					break;
				case '2':
					num2 = (isDummy ? (-1) : 2);
					break;
				case '3':
					num2 = (isDummy ? (-1) : 3);
					break;
				case '4':
					num2 = (isDummy ? (-1) : 4);
					break;
				case '5':
					num2 = (isDummy ? (-1) : 5);
					break;
				}
				break;
			case '&':
				switch (text2.ElementAt(1))
				{
				case '1':
					num = 1;
					break;
				case '2':
					num = 2;
					break;
				case '3':
					num = 3;
					break;
				}
				break;
			case '$':
				text = text2.Substring(1);
				break;
			}
		}
		if (!isDummy)
		{
			shipTemplates.Add(new shipModule(graphicsDevice, contentManager, initialPosition, initialRotation, shipSpritesHolder, inIsCore: true, num, num2, typeOfBlock.core, 0, shipConfigStreamReader, explosionManagerRef, randGen, gridManager, text, isBoss, isStation));
		}
		else
		{
			listOfImageShips.Add(new shipDummy(new shipModule(graphicsDevice, contentManager, new Vector2(220f, 220f), 0f, shipSpritesHolder, inIsCore: true, num, -1, typeOfBlock.core, 0, shipConfigStreamReader, explosionManagerRef, randGen, gridManager, text, inIsBoss: false, inIsStation: false), graphicsDevice));
		}
		return text;
	}

	private void loadShipsFromFileAutomatic()
	{
		string[] files = Directory.GetFiles("Content/ForeverWars/Data/Boss/");
		string[] array = files;
		foreach (string item in array)
		{
			shipFileNamesRare.Add(item);
		}
		files = Directory.GetFiles("Content/ForeverWars/Data/Fighters/");
		string[] array2 = files;
		foreach (string item2 in array2)
		{
			shipFileNamesCommon.Add(item2);
		}
		files = Directory.GetFiles("Content/ForeverWars/Data/Shuttles/");
		string[] array3 = files;
		foreach (string item3 in array3)
		{
			shipFileNamesUncommon.Add(item3);
		}
		files = Directory.GetFiles("Content/ForeverWars/Data/Stations/");
		string[] array4 = files;
		foreach (string item4 in array4)
		{
			shipFileNamesStation.Add(item4);
		}
	}

	private void loadShipsFromFileManual()
	{
		shipFileNamesRare.Add("Boss/battleShip.txt");
		shipFileNamesRare.Add("Boss/Barbarian.txt");
		shipFileNamesRare.Add("Boss/cruiser.txt");
		shipFileNamesRare.Add("Boss/Nemesis.txt");
		shipFileNamesRare.Add("Boss/Goliath.txt");
		shipFileNamesCommon.Add("Fighters/fighter1Ship.txt");
		shipFileNamesCommon.Add("Fighters/fighter2Ship.txt");
		shipFileNamesCommon.Add("Fighters/fighter3Ship.txt");
		shipFileNamesCommon.Add("Fighters/fighter4Ship.txt");
		shipFileNamesUncommon.Add("Shuttles/shuttleRocket.txt");
		shipFileNamesUncommon.Add("Shuttles/shuttleRainMaker.txt");
		shipFileNamesUncommon.Add("Shuttles/shuttleFirefly.txt");
		shipFileNamesUncommon.Add("Shuttles/shuttleBelabor.txt");
		shipFileNamesStation.Add("Stations/Delta6.txt");
		shipFileNamesStation.Add("Stations/Epsilon9.txt");
		shipFileNamesStation.Add("Stations/Kilo4.txt");
	}

	public shipHandler(GraphicsDevice inGraphicsDevice, ContentManager inContentManager, explosionManager inExplosionManager, Random inRand, gridSystem inGridManager, bool AIMode)
	{
		gridManager = inGridManager;
		randGen = inRand;
		explosionManagerRef = inExplosionManager;
		contentManager = inContentManager;
		graphicsDevice = inGraphicsDevice;
		shipSpritesHolder = new shipData(contentManager);
		AIModeEnabled = AIMode;
		loadShipsFromFileAutomatic();
		foreach (string item in shipFileNamesCommon)
		{
			addShipConfig(shipTemplates, item, Vector2.Zero, 0f, isBoss: false, isStation: false, isDummy: true);
		}
		foreach (string item2 in shipFileNamesUncommon)
		{
			addShipConfig(shipTemplates, item2, Vector2.Zero, 0f, isBoss: false, isStation: false, isDummy: true);
		}
		foreach (string item3 in shipFileNamesRare)
		{
			addShipConfig(shipTemplates, item3, Vector2.Zero, 0f, isBoss: false, isStation: false, isDummy: true);
		}
		foreach (string item4 in shipFileNamesStation)
		{
			addShipConfig(shipTemplates, item4, Vector2.Zero, 0f, isBoss: false, isStation: false, isDummy: true);
		}
		foreach (shipDummy listOfImageShip in listOfImageShips)
		{
			listOfImageShip.initaliseOnce();
		}
	}

	public void addShip()
	{
		if (tutorialEnemiesCounter < 6)
		{
			tutorialEnemiesCounter++;
		}
		float num = (float)randGen.NextDouble();
		if (num < 0.05f)
		{
			if (tutorialEnemiesCounter >= 6)
			{
				isBossFight = true;
				switch (randGen.Next(4))
				{
				case 0:
					currentBossName = addShipConfig(shipTemplates, shipFileNamesRare[randGen.Next(shipFileNamesRare.Count)], new Vector2((float)randGen.NextDouble() * 3000f, 0f), (float)Math.PI / 2f, isBoss: true, isStation: false, isDummy: false);
					break;
				case 1:
					currentBossName = addShipConfig(shipTemplates, shipFileNamesRare[randGen.Next(shipFileNamesRare.Count)], new Vector2((float)randGen.NextDouble() * 3000f, 3000f), 4.712389f, isBoss: true, isStation: false, isDummy: false);
					break;
				case 2:
					currentBossName = addShipConfig(shipTemplates, shipFileNamesRare[randGen.Next(shipFileNamesRare.Count)], new Vector2(-1000f, (float)randGen.NextDouble() * 3000f), 0f, isBoss: true, isStation: false, isDummy: false);
					break;
				case 3:
					currentBossName = addShipConfig(shipTemplates, shipFileNamesRare[randGen.Next(shipFileNamesRare.Count)], new Vector2(3000f, (float)randGen.NextDouble() * 3000f), (float)Math.PI, isBoss: true, isStation: false, isDummy: false);
					break;
				}
			}
		}
		else if (num < 0.15f)
		{
			switch (randGen.Next(4))
			{
			case 0:
				addShipConfig(shipTemplates, shipFileNamesUncommon[randGen.Next(shipFileNamesUncommon.Count)], new Vector2((float)randGen.NextDouble() * 3000f, -1000f), (float)Math.PI / 2f, isBoss: false, isStation: false, isDummy: false);
				break;
			case 1:
				addShipConfig(shipTemplates, shipFileNamesUncommon[randGen.Next(shipFileNamesUncommon.Count)], new Vector2((float)randGen.NextDouble() * 3000f, 3000f), 4.712389f, isBoss: false, isStation: false, isDummy: false);
				break;
			case 2:
				addShipConfig(shipTemplates, shipFileNamesUncommon[randGen.Next(shipFileNamesUncommon.Count)], new Vector2(-1000f, (float)randGen.NextDouble() * 3000f), 0f, isBoss: false, isStation: false, isDummy: false);
				break;
			case 3:
				addShipConfig(shipTemplates, shipFileNamesUncommon[randGen.Next(shipFileNamesUncommon.Count)], new Vector2(3000f, (float)randGen.NextDouble() * 3000f), (float)Math.PI, isBoss: false, isStation: false, isDummy: false);
				break;
			}
		}
		else if (num < 0.25f && !stationExists)
		{
			stationExists = true;
			switch (randGen.Next(4))
			{
			case 0:
				addShipConfig(shipTemplates, shipFileNamesStation[randGen.Next(shipFileNamesStation.Count)], new Vector2((float)randGen.NextDouble() * 3000f, -1000f), (float)Math.PI / 2f, isBoss: false, isStation: true, isDummy: false);
				break;
			case 1:
				addShipConfig(shipTemplates, shipFileNamesStation[randGen.Next(shipFileNamesStation.Count)], new Vector2((float)randGen.NextDouble() * 3000f, 3000f), 4.712389f, isBoss: false, isStation: true, isDummy: false);
				break;
			case 2:
				addShipConfig(shipTemplates, shipFileNamesStation[randGen.Next(shipFileNamesStation.Count)], new Vector2(-1000f, (float)randGen.NextDouble() * 3000f), 0f, isBoss: false, isStation: true, isDummy: false);
				break;
			case 3:
				addShipConfig(shipTemplates, shipFileNamesStation[randGen.Next(shipFileNamesStation.Count)], new Vector2(3000f, (float)randGen.NextDouble() * 3000f), (float)Math.PI, isBoss: false, isStation: true, isDummy: false);
				break;
			}
		}
		else
		{
			switch (randGen.Next(4))
			{
			case 0:
				addShipConfig(shipTemplates, shipFileNamesCommon[randGen.Next(shipFileNamesCommon.Count)], new Vector2((float)randGen.NextDouble() * 3000f, -1000f), (float)Math.PI / 2f, isBoss: false, isStation: false, isDummy: false);
				break;
			case 1:
				addShipConfig(shipTemplates, shipFileNamesCommon[randGen.Next(shipFileNamesCommon.Count)], new Vector2((float)randGen.NextDouble() * 3000f, 3000f), 4.712389f, isBoss: false, isStation: false, isDummy: false);
				break;
			case 2:
				addShipConfig(shipTemplates, shipFileNamesCommon[randGen.Next(shipFileNamesCommon.Count)], new Vector2(-1000f, (float)randGen.NextDouble() * 3000f), 0f, isBoss: false, isStation: false, isDummy: false);
				break;
			case 3:
				addShipConfig(shipTemplates, shipFileNamesCommon[randGen.Next(shipFileNamesCommon.Count)], new Vector2(3000f, (float)randGen.NextDouble() * 3000f), (float)Math.PI, isBoss: false, isStation: false, isDummy: false);
				break;
			}
		}
	}

	public void Update(playerShip[] playerList, List<eBullet> eBulletList, List<pBullet> pBulletList)
	{
		if (AIModeEnabled)
		{
			AIShipDestroyTimer--;
			if (AIShipDestroyTimer < 0)
			{
				foreach (shipModule shipTemplate in shipTemplates)
				{
					if (Vector2.Distance(shipTemplate.getPosition(), playerList[0].getPosition()) < 200f)
					{
						AIShipDestroyTimer = 600;
						shipTemplate.setToDead(playerList[0], isRootExplosion: true);
					}
				}
			}
		}
		if (!forcedShipSpawn)
		{
			if (isBossFight)
			{
				bool flag = false;
				foreach (shipModule shipTemplate2 in shipTemplates)
				{
					if (shipTemplate2.getIsBoss())
					{
						flag = true;
					}
				}
				if (!flag)
				{
					endBossFight();
					isBossFight = false;
				}
			}
			if (shipTemplates.Count < 4 && !isBossFight)
			{
				addShip();
			}
		}
		stationExists = false;
		for (int i = 0; i < shipTemplates.Count; i++)
		{
			if (shipTemplates[i].getIsStation())
			{
				stationExists = true;
			}
		}
		for (int j = 0; j < shipTemplates.Count; j++)
		{
			if (shipTemplates[j].Update(playerList, Vector2.Zero, 0f, eBulletList, permittedToFire: false, pBulletList))
			{
				shipTemplates.RemoveAt(j);
				j--;
			}
		}
	}

	public void Dispose()
	{
		foreach (shipModule shipTemplate in shipTemplates)
		{
			shipTemplate.Dispose();
		}
		foreach (shipDummy listOfImageShip in listOfImageShips)
		{
			listOfImageShip.Dispose();
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		foreach (shipModule shipTemplate in shipTemplates)
		{
			shipTemplate.Draw(spriteBatch, offset);
		}
	}

	public Vector2 getClosestBossPosition()
	{
		if (isBossFight)
		{
			foreach (shipModule shipTemplate in shipTemplates)
			{
				if (shipTemplate.getIsBoss())
				{
					return shipTemplate.getPosition();
				}
			}
		}
		return new Vector2(-1f, -1f);
	}

	public string getBossName()
	{
		if (isBossFight)
		{
			return currentBossName;
		}
		return "-1";
	}

	public void DrawHudElements(SpriteBatch spriteBatch, SpriteFont spriteFont)
	{
		if (isBossFight)
		{
			if (hudFadeInCounter > 0)
			{
				hudFadeInCounter--;
			}
			else if (hudWaitCounter > 0)
			{
				hudWaitCounter--;
			}
			else if (hudBossWarningCounter > 1f)
			{
				hudBossWarningCounter -= 0.01f;
			}
			float a = (float)hudFadeInCounter / 30f * -1f + 1f;
			string text = "Warning! " + currentBossName + " inbound!";
			spriteBatch.DrawString(spriteFont, text, new Vector2(ForeverHelper._titleSafeArea.X + ForeverHelper._titleSafeArea.Width / 2, MathHelper.Lerp((float)(ForeverHelper._titleSafeArea.Y + ForeverHelper._titleSafeArea.Height) - spriteFont.MeasureString(text).Y, ForeverHelper._titleSafeArea.Y + ForeverHelper._titleSafeArea.Height / 2, hudBossWarningCounter - 1f)), new Color(1f, 0f, 0f, a), 0f, spriteFont.MeasureString(text) / 2f, hudBossWarningCounter, SpriteEffects.None, 0f);
		}
		else
		{
			hudBossWarningCounter = 2f;
			hudFadeInCounter = 30;
			hudWaitCounter = 60;
			currentBossName = "";
		}
	}

	public void drawShip(SpriteBatch spriteBatch, string shipName, Vector2 position, float rotation, float scale)
	{
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null);
		foreach (shipDummy listOfImageShip in listOfImageShips)
		{
			if (listOfImageShip.shipModule.getName() == shipName)
			{
				Texture2D image = listOfImageShip.getImage();
				spriteBatch.Draw(image, position, null, Color.White, rotation, new Vector2((float)image.Width / 2f, (float)image.Height / 2f), scale, SpriteEffects.None, 0f);
			}
		}
		spriteBatch.End();
		spriteBatch.Begin();
	}

	public void endBossFight()
	{
		hudBossWarningCounter = 2f;
		hudFadeInCounter = 30;
		hudWaitCounter = 60;
		currentBossName = "";
	}
}
