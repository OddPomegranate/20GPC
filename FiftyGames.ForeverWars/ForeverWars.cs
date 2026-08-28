using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.ForeverWars;

internal class ForeverWars : Minigame
{
	private const float cameraMinScale = 1f;

	private const float cameraMaxScale = 2f;

	private const int whiteOutCounterMax = 60;

	private SpriteBatch spriteBatch;

	private PlayerManager playerManager;

	private ContentManager contentManager;

	private MinigameMeta _minigame;

	private Texture2D m_Background;

	private int m_TimePassed;

	private playerShip[] m_PlayerShip;

	private Random m_Random;

	private Texture2D pixelSprite;

	private bool AIMODE;

	private Vector2 virtualScreenDimensions = new Vector2(2000f, 2000f);

	private Vector2 virtualScreenDimensionsBacking = new Vector2(3000f, 3000f);

	private Vector2 screenDimensions = new Vector2(1280f, 720f);

	private Vector2 screenOrigin = new Vector2(640f, 360f);

	private explosionManager mainExplosionManager;

	private Texture2D backgroundSprite;

	private fullScreenQuad finalScreenOutQuad;

	private RenderTarget2D finalScreenOutRT;

	private RenderTarget2D finalScreenOutRTBacking;

	private RenderTarget2D finalScreenOutRTCopy;

	private shipHandler shipManager;

	private Rectangle fieldSize = new Rectangle(0, 0, 2000, 2000);

	private Rectangle fieldSizeBacking = new Rectangle(0, 0, 3000, 3000);

	private int edgeBorder = 3;

	private int explosioncounter = 120;

	private Texture2D arrowSprite;

	private fullScreenQuad quad;

	private Effect borderShader;

	private List<eBullet> enemyBulletList = new List<eBullet>();

	private List<pBullet> playerBulletList = new List<pBullet>();

	private Vector2 cameraPosition;

	private float cameraScale;

	private gridSystem gridManager;

	private SpriteFont hudSpriteFont;

	private int tempInt;

	private int scoreBoardScrollCounter;

	private int scoreBoardScrollTimer;

	private int firstUpdateCounter = 5;

	private int whiteOutCounter;

	private bool whiteOutActive;

	private bool whiteOutRevActive;

	private Texture2D whiteOutTexture;

	private bool gameIsUpdating;

	private int gameClockTimeSeconds;

	private int gameClockTimeMinutes;

	private float gameClockTimeMilliseconds;

	private Cue backgroundMusicCue;

	private bool firstRun = true;

	private bool gameOver;

	private bool hasBeenQuit;

	public ForeverWars(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		this.playerManager = playerManager;
		this.contentManager = contentManager;
		ForeverHelper.soundManager = soundManager;
		AIMODE = demoMode;
		ForeverHelper._minigameMeta = minigame;
		ForeverHelper._titleSafeArea = _titleSafeArea;
	}

	public override void Initialize()
	{
		if (hasBeenQuit)
		{
			return;
		}
		if (firstRun)
		{
			firstRun = false;
			base.Initialize();
			string[] cueNames = new string[15]
			{
				"geometryWars EnemyFire", "geometryWars EnemyHit", "geometryWars Explosion Large", "geometryWars Explosion Medium", "geometryWars Explosion Small", "geometryWars Laser", "geometryWars LaserCannon", "geometryWars PlayerDie", "geometryWars PlayerFire", "geometryWars PlayerHit",
				"geometryWars PlayerHitSub", "geometryWars RocketFired", "geometryWars RocketNoise", "geometryWars ShockWave", "geometryWars TurretFired"
			};
			_soundManager.PreloadSounds(cueNames);
			spriteBatch = new SpriteBatch(base.GraphicsDevice);
			quad = new fullScreenQuad(base.GraphicsDevice);
			gridManager = new gridSystem(base.GraphicsDevice, contentManager);
			m_Random = new Random();
			mainExplosionManager = new explosionManager(base.GraphicsDevice, contentManager, m_Random, gridManager);
			finalScreenOutQuad = new fullScreenQuad(base.GraphicsDevice);
			finalScreenOutRTCopy = new RenderTarget2D(base.GraphicsDevice, (int)screenDimensions.X, (int)screenDimensions.Y);
			finalScreenOutRT = new RenderTarget2D(base.GraphicsDevice, fieldSize.Width, fieldSize.Height);
			finalScreenOutRTBacking = new RenderTarget2D(base.GraphicsDevice, fieldSizeBacking.Width, fieldSizeBacking.Height);
			whiteOutTexture = new Texture2D(base.GraphicsDevice, 1, 1);
			whiteOutTexture.SetData(new Color[1] { Color.White });
		}
		else
		{
			shipManager.Dispose();
		}
		gameOver = false;
		gameClockTimeSeconds = 0;
		gameClockTimeMinutes = 0;
		gameClockTimeMilliseconds = 0f;
		enemyBulletList.Clear();
		playerBulletList.Clear();
		borderShader = contentManager.Load<Effect>("ForeverWars/Effects/borderShader");
		arrowSprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/Arrow");
		backgroundSprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/Background");
		hudSpriteFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		shipManager = new shipHandler(base.GraphicsDevice, contentManager, mainExplosionManager, m_Random, gridManager, AIMODE);
		pixelSprite = contentManager.Load<Texture2D>("LightBikes/Sprites/Pixel");
		List<Color> list = new List<Color>(playerManager.AvailableColors);
		for (int i = 0; i < playerManager.NumberOfPlayers; i++)
		{
			Color playerColor = playerManager.GetPlayerColor(playerManager.PlayersConnected[i]);
			list.Remove(playerColor);
		}
		if (AIMODE)
		{
			m_PlayerShip = new playerShip[1];
			Vector2 initialPosition = new Vector2(1100f, 1100f);
			m_PlayerShip[0] = new playerShip(base.GraphicsDevice, contentManager, null, playerManager, initialPosition, 0, mainExplosionManager, gridManager, AI: true);
		}
		else
		{
			m_PlayerShip = new playerShip[playerManager.NumberOfPlayers];
			for (int j = 0; j < playerManager.NumberOfPlayers; j++)
			{
				Vector2 initialPosition2 = new Vector2(1000 + ((j % 2 == 1) ? (-100) : 100), 1000 + ((j > 1) ? (-100) : 100));
				m_PlayerShip[j] = new playerShip(base.GraphicsDevice, contentManager, playerManager.PlayersConnected[j], playerManager, initialPosition2, j, mainExplosionManager, gridManager, AI: false);
			}
		}
		gridManager.purgeWarpBuffer();
		firstUpdateCounter = 5;
	}

	protected override void LoadContent()
	{
	}

	protected override void UnloadContent()
	{
	}

	public void purge()
	{
		finalScreenOutRT.Dispose();
		finalScreenOutRT = null;
		finalScreenOutRTBacking.Dispose();
		finalScreenOutRTBacking = null;
		finalScreenOutRTCopy.Dispose();
		finalScreenOutRTCopy = null;
		shipManager.Dispose();
		gridManager.Dispose();
	}

	public override void Quit()
	{
		purge();
	}

	public override void Update(GameTime gameTime)
	{
		gameIsUpdating = true;
		m_TimePassed++;
		ForeverSoundManager.checkforRocketsInFlight(enemyBulletList);
		if (!allPlayersDead())
		{
			gameClockTimeMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
			if (gameClockTimeMilliseconds >= 1000f)
			{
				gameClockTimeMilliseconds -= 1000f;
				gameClockTimeSeconds++;
			}
			if (gameClockTimeSeconds > 59)
			{
				gameClockTimeSeconds -= 60;
				gameClockTimeMinutes++;
			}
		}
		if (whiteOutActive)
		{
			if (whiteOutRevActive)
			{
				whiteOutCounter--;
				if (whiteOutCounter < 1)
				{
					whiteOutActive = false;
				}
			}
			else
			{
				whiteOutCounter++;
				if (whiteOutCounter > 59)
				{
					whiteOutRevActive = true;
					Initialize();
				}
			}
		}
		if (!gameOver && allPlayersDead())
		{
			gameOver = false;
			for (int i = 0; i < m_PlayerShip.Length; i++)
			{
				if (ForeverHelper._minigameMeta.BestScore < (float)m_PlayerShip[i].getKills() && m_PlayerShip[i].getPlayerRef().Name != "")
				{
					ForeverHelper._minigameMeta.SetScore(m_PlayerShip[i].getPlayerRef().Name, m_PlayerShip[i].getKills());
				}
			}
		}
		gridManager.Update();
		mainExplosionManager.Update();
		shipManager.Update(m_PlayerShip, enemyBulletList, playerBulletList);
		for (int j = 0; j < enemyBulletList.Count; j++)
		{
			if (enemyBulletList[j].Update(enemyBulletList, m_PlayerShip, playerBulletList))
			{
				enemyBulletList[j].destroyBullet();
				enemyBulletList.RemoveAt(j);
				j--;
			}
		}
		for (int k = 0; k < playerBulletList.Count; k++)
		{
			if (playerBulletList[k].Update())
			{
				playerBulletList.RemoveAt(k);
				k--;
			}
		}
		playerShip[] array = m_PlayerShip;
		foreach (playerShip playerShip2 in array)
		{
			playerShip2.Update(fieldSize, edgeBorder, playerBulletList, enemyBulletList);
		}
		updateCameraPosition(m_PlayerShip, virtualScreenDimensions);
		playerShip[] array2 = m_PlayerShip;
		foreach (playerShip playerShip3 in array2)
		{
			if (playerShip3.getAlive())
			{
				Vector2 vector = getCameraPosition();
				Vector2 vector2 = playerShip3.getPosition() - vector;
				playerShip3.isOffScreen = Math.Abs(vector2.X) > screenOrigin.X || Math.Abs(vector2.Y) > screenOrigin.Y;
			}
			else
			{
				playerShip3.isOffScreen = false;
			}
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.SetRenderTarget(finalScreenOutRTBacking);
		base.GraphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin();
		shipManager.Draw(spriteBatch, new Vector2(500f, 500f));
		spriteBatch.End();
		base.GraphicsDevice.SetRenderTarget(finalScreenOutRT);
		base.GraphicsDevice.SetRenderTarget(finalScreenOutRTCopy);
		base.GraphicsDevice.Clear(Color.Transparent);
		base.GraphicsDevice.SetRenderTarget(finalScreenOutRT);
		base.GraphicsDevice.Clear(Color.Transparent);
		borderShader.Parameters["borderThickness"].SetValue(edgeBorder);
		borderShader.Parameters["maxWidth"].SetValue(fieldSize.Width);
		borderShader.Parameters["maxHeight"].SetValue(fieldSize.Height);
		borderShader.Parameters["InputTexture"].SetValue(finalScreenOutRTCopy);
		_ = base.GraphicsDevice.BlendState;
		base.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
		borderShader.CurrentTechnique.Passes[0].Apply();
		quad.Render(-Vector2.One, Vector2.One);
		base.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		spriteBatch.Begin();
		_ = Vector2.Zero;
		mainExplosionManager.Draw(spriteBatch, null);
		playerShip[] array = m_PlayerShip;
		foreach (playerShip playerShip2 in array)
		{
			playerShip2.Draw(spriteBatch);
		}
		if (!AIMODE && shipManager.getClosestBossPosition() != new Vector2(-1f, -1f) && Vector2.Distance(getCameraPosition(), shipManager.getClosestBossPosition()) > 500f)
		{
			float num = ForeverHelper.TurnToFace(getCameraPosition(), shipManager.getClosestBossPosition(), 0f, 8f);
			bool flag = lastPlayerAlive();
			if (m_PlayerShip.Length != 1)
			{
				spriteBatch.Draw(arrowSprite, getCameraPosition() + (flag ? ForeverHelper.AngleToV2(num, 60f) : Vector2.Zero), null, Color.White, num, new Vector2(arrowSprite.Width / 2, arrowSprite.Height / 2), 1f, SpriteEffects.None, 0f);
			}
			else
			{
				spriteBatch.Draw(arrowSprite, getCameraPosition() + (m_PlayerShip[0].getAlive() ? ForeverHelper.AngleToV2(num, 60f) : Vector2.Zero), null, Color.White, num, new Vector2(arrowSprite.Width / 2, arrowSprite.Height / 2), 1f, SpriteEffects.None, 0f);
			}
		}
		foreach (eBullet enemyBullet in enemyBulletList)
		{
			enemyBullet.Draw(spriteBatch);
		}
		foreach (pBullet playerBullet in playerBulletList)
		{
			playerBullet.Draw(spriteBatch);
		}
		playerShip[] array2 = m_PlayerShip;
		foreach (playerShip playerShip3 in array2)
		{
			if (playerShip3.isOffScreen)
			{
				Vector2 vector = getCameraPosition();
				Vector2 position = new Vector2(MathHelper.Clamp(playerShip3.getPosition().X, vector.X - screenOrigin.X, vector.X + screenOrigin.X), MathHelper.Clamp(playerShip3.getPosition().Y, vector.Y - screenOrigin.Y, vector.Y + screenOrigin.Y));
				Vector2 vector2 = playerShip3.getPosition() - getCameraPosition();
				vector2.Normalize();
				float rotation = ForeverHelper.V2ToAngle(vector2);
				spriteBatch.Draw(arrowSprite, position, null, playerShip3.getColor(), rotation, new Vector2(arrowSprite.Width, arrowSprite.Height / 2), 1f, SpriteEffects.None, 0f);
			}
		}
		spriteBatch.End();
		if (firstUpdateCounter > 0)
		{
			firstUpdateCounter--;
			gridManager.purgeWarpBuffer();
		}
		gridManager.Draw(spriteBatch, getCameraPosition(), screenOrigin);
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		spriteBatch.Begin();
		spriteBatch.Draw(finalScreenOutRTBacking, screenOrigin - Vector2.One * 500f, null, Color.White, 0f, getCameraPosition(), 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(finalScreenOutRT, screenOrigin, null, Color.White, 0f, getCameraPosition(), 1f, SpriteEffects.None, 0f);
		if (!AIMODE)
		{
			if (allPlayersDead())
			{
				drawScoreBoard(spriteBatch);
				playerShip[] array3 = m_PlayerShip;
				foreach (playerShip playerShip4 in array3)
				{
					if (playerShip4.getPlayerRef().GamePadManager.ButtonIsHeld(Buttons.A))
					{
						restartGame();
					}
				}
			}
			else
			{
				DrawHud(spriteBatch);
			}
		}
		spriteBatch.End();
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		if (whiteOutActive)
		{
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			spriteBatch.Draw(whiteOutTexture, new Rectangle(0, 0, 1280, 720), Color.White * ((float)whiteOutCounter / 60f));
			spriteBatch.End();
		}
		gameIsUpdating = false;
		base.Draw(gameTime);
	}

	public void restartGame()
	{
		if (!whiteOutActive)
		{
			GC.Collect();
			whiteOutActive = true;
			whiteOutRevActive = false;
			whiteOutCounter = 0;
		}
	}

	public bool allPlayersDead()
	{
		int num = 0;
		playerShip[] array = m_PlayerShip;
		foreach (playerShip playerShip2 in array)
		{
			if (playerShip2.getAlive())
			{
				num++;
			}
		}
		return num < 1;
	}

	public bool lastPlayerAlive()
	{
		int num = 0;
		playerShip[] array = m_PlayerShip;
		foreach (playerShip playerShip2 in array)
		{
			if (playerShip2.getAlive())
			{
				num++;
			}
		}
		return num == 1;
	}

	public void DrawHud(SpriteBatch spriteBatch)
	{
		string text = gameClockTimeMinutes + ":" + gameClockTimeSeconds.ToString("D2");
		spriteBatch.DrawString(position: new Vector2(_titleSafeArea.X + _titleSafeArea.Width / 2, (float)_titleSafeArea.Y + hudSpriteFont.MeasureString(text).Y), spriteFont: hudSpriteFont, text: text, color: Color.White, rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
		for (int i = 0; i < m_PlayerShip.Length; i++)
		{
			bool flag = i % 2 != 1;
			bool flag2 = i < 2;
			float num = (flag ? _titleSafeArea.X : (_titleSafeArea.X + _titleSafeArea.Width));
			float num2 = ((!flag2) ? (_titleSafeArea.Y + 120) : (_titleSafeArea.Y + _titleSafeArea.Height));
			string name = m_PlayerShip[i].getPlayerRef().Name;
			spriteBatch.DrawString(hudSpriteFont, name, new Vector2(num - (flag ? 0f : hudSpriteFont.MeasureString(name).X), num2 - 120f), m_PlayerShip[i].getPlayerColor());
			string text2 = "";
			for (int j = 0; j < m_PlayerShip[i].getLivesRemaining(); j++)
			{
				text2 += "X";
			}
			name = (flag ? ("Lives: " + text2) : (text2 + " :Lives"));
			spriteBatch.DrawString(hudSpriteFont, name, new Vector2(num - (flag ? 0f : hudSpriteFont.MeasureString(name).X), num2 - 100f), m_PlayerShip[i].getPlayerColor());
			if (!m_PlayerShip[i].getBombIsCharged())
			{
				string text3 = "";
				for (int k = 0; k < 5; k++)
				{
					_ = (float)m_PlayerShip[i].getBombChargeValue() / (float)m_PlayerShip[i].getBombChargeMaxValue();
					text3 = ((!((float)k + 1f < (float)m_PlayerShip[i].getBombChargeValue() / (float)m_PlayerShip[i].getBombChargeMaxValue() * 5f)) ? (text3 + " ") : (text3 + "-"));
				}
				name = (flag ? ("Bomb  charging: [" + text3 + "]") : ("[" + text3 + "] :Bomb  charging"));
				spriteBatch.DrawString(hudSpriteFont, name, new Vector2(num - (flag ? 0f : hudSpriteFont.MeasureString(name).X), num2 - 75f), m_PlayerShip[i].getPlayerColor());
			}
			else
			{
				name = "Bomb ready.";
				spriteBatch.DrawString(hudSpriteFont, name, new Vector2(num - (flag ? 0f : hudSpriteFont.MeasureString(name).X), num2 - 75f), m_PlayerShip[i].getPlayerColor());
			}
			name = ((i < 2) ? ("Kills: " + m_PlayerShip[i].getKills()) : (m_PlayerShip[i].getKills() + " :Kills"));
			spriteBatch.DrawString(hudSpriteFont, name, new Vector2(num - (flag ? 0f : hudSpriteFont.MeasureString(name).X), num2 - 50f), m_PlayerShip[i].getPlayerColor());
		}
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
		shipManager.DrawHudElements(spriteBatch, hudSpriteFont);
	}

	private void updateCameraPosition(playerShip[] playerList, Vector2 maxScreenPosition)
	{
		List<Vector2> list = new List<Vector2>();
		Vector2 zero = Vector2.Zero;
		int num = 0;
		playerShip[] array = m_PlayerShip;
		foreach (playerShip playerShip2 in array)
		{
			if (playerShip2.getAlive())
			{
				num++;
			}
		}
		if (num < 1)
		{
			return;
		}
		for (int j = 0; j < playerList.Length; j++)
		{
			if (playerList[j].getAlive())
			{
				list.Add(playerList[j].getPosition());
			}
		}
		if (playerList.Count() == 0)
		{
			cameraPosition = maxScreenPosition * 0.5f;
		}
		foreach (Vector2 item in list)
		{
			zero += item;
		}
		if (cameraPosition == Vector2.Zero)
		{
			cameraPosition = zero / list.Count();
		}
		else
		{
			cameraPosition = new Vector2(MathHelper.Lerp((zero / list.Count()).X, cameraPosition.X, 0.01f), MathHelper.Lerp((zero / list.Count()).Y, cameraPosition.Y, 0.01f));
		}
	}

	public Vector2 getCameraPosition()
	{
		cameraPosition = new Vector2(MathHelper.Clamp(cameraPosition.X, 640 - _titleSafeArea.X, 2000f - (float)(640 - (1280 - (_titleSafeArea.Width + _titleSafeArea.X)))), MathHelper.Clamp(cameraPosition.Y, 360 - _titleSafeArea.Y, 2000f - (float)(360 - (720 - (_titleSafeArea.Height + _titleSafeArea.Y)))));
		return cameraPosition;
	}

	public void drawScoreBoard(SpriteBatch spriteBatch)
	{
		float num = _titleSafeArea.Y + _titleSafeArea.Height / 2 - 288 + 10;
		_ = Vector2.Zero;
		if (gameIsUpdating)
		{
			scoreBoardScrollTimer--;
			if (scoreBoardScrollTimer < 0)
			{
				scoreBoardScrollCounter++;
				scoreBoardScrollTimer = 100;
				if (scoreBoardScrollCounter > shipManager.getDummyShipList().Count - 1)
				{
					scoreBoardScrollCounter = 0;
				}
			}
		}
		string text = gameClockTimeMinutes + ":" + gameClockTimeSeconds.ToString("D2");
		spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * 1f, num + 100f), spriteFont: hudSpriteFont, text: text, color: Color.White, rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
		text = "Total Kills :";
		spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * 2f, num + 100f), spriteFont: hudSpriteFont, text: text, color: Color.White, rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
		text = "Total Blocks \n Destroyed :";
		spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * 2f, num + 150f), spriteFont: hudSpriteFont, text: text, color: Color.White, rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
		for (int i = 0; i < 5; i++)
		{
			int index = ((i + scoreBoardScrollCounter > shipManager.getDummyShipList().Count - 1) ? (i + scoreBoardScrollCounter - shipManager.getDummyShipList().Count) : (i + scoreBoardScrollCounter));
			text = shipManager.getDummyShipList()[index].shipModule.getName();
			spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * 2f, num + 65f * (float)(4 + i)), spriteFont: hudSpriteFont, text: text, color: Color.White, rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
			Vector2 position = new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * 1f, num + 65f * (float)(4 + i));
			shipManager.drawShip(spriteBatch, text, position, -(float)Math.PI / 4f, 0.3f);
		}
		for (int j = 0; j < m_PlayerShip.Length; j++)
		{
			text = m_PlayerShip[j].getPlayerRef().Name;
			spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * (float)(j + 3), num), spriteFont: hudSpriteFont, text: text, color: m_PlayerShip[j].getPlayerColor(), rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
			if (ForeverHelper._minigameMeta.BestScore <= (float)m_PlayerShip[j].getKills())
			{
				Vector2 position = new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * (float)(j + 3), num + hudSpriteFont.MeasureString(text).Y);
				text = "HIGHSCORE";
				spriteBatch.DrawString(hudSpriteFont, text, position, m_PlayerShip[j].getPlayerColor(), 0f, hudSpriteFont.MeasureString(text) / 2f, 1f, SpriteEffects.None, 0f);
			}
			text = m_PlayerShip[j].getKills().ToString();
			spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * (float)(j + 3), num + 100f), spriteFont: hudSpriteFont, text: text, color: m_PlayerShip[j].getPlayerColor(), rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
			text = m_PlayerShip[j].blocksDestroyed.ToString();
			spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * (float)(j + 3), num + 150f), spriteFont: hudSpriteFont, text: text, color: m_PlayerShip[j].getPlayerColor(), rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
			for (int k = 0; k < 5; k++)
			{
				int index2 = ((k + scoreBoardScrollCounter > shipManager.getDummyShipList().Count - 1) ? (k + scoreBoardScrollCounter - shipManager.getDummyShipList().Count) : (k + scoreBoardScrollCounter));
				text = "0";
				for (int l = 0; l < m_PlayerShip[j].getKillList().Count; l++)
				{
					if (m_PlayerShip[j].getKillList()[l].getName() == shipManager.getDummyShipList()[index2].shipModule.getName())
					{
						text = m_PlayerShip[j].getKillList()[l].getKills().ToString();
					}
				}
				spriteBatch.DrawString(position: new Vector2(1280f / (float)(m_PlayerShip.Length + 3) * (float)(j + 3), num + 65f * (float)(4 + k)), spriteFont: hudSpriteFont, text: text, color: m_PlayerShip[j].getPlayerColor(), rotation: 0f, origin: hudSpriteFont.MeasureString(text) / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
			}
		}
	}
}
