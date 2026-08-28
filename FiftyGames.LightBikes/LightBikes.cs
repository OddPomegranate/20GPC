using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.LightBikes;

internal class LightBikes : Minigame
{
	private const int resetPauseTimerMax = 60;

	private const int resetTimerMax = 60;

	private const int forceResetCounterMax = 90;

	private const int fastFinishSpeed = 3;

	private const float soundFilterSpeed = 0.01f;

	private const float soundFilterMin = 1f;

	private const float soundFilterMax = 6f;

	private const int numberOfSecondsInitialCountDown = 3;

	private SpriteBatch spriteBatch;

	private PlayerManager playerManager;

	private ContentManager contentManager;

	private MinigameMeta _minigame;

	private Texture2D m_Background;

	private int m_TimePassed;

	private Bike[] m_Bikes;

	private Grid gridManager;

	private Random m_Random;

	private Texture2D pixelSprite;

	private bool AIMODE;

	private int resetPauseTimer;

	private int resetTimer;

	private bool resetting;

	private Rectangle whiteOutRectangle = new Rectangle(0, 0, 1280, 720);

	private bool isCleared;

	private int forceResetCounter = 90;

	private bool forceResetCounterActive;

	private int winnerIndex = -1;

	private Player winnerPlayerRef;

	private SpriteFont hudFont;

	private bool fastFinish;

	private bool forcedRestart;

	private Texture2D whiteOutTexture;

	private RenderTarget2D finalOutRT;

	private float soundFilter;

	private bool soundFilterDirection;

	private bool firstUpdate = true;

	private float gameClockTimeMilliseconds;

	private int countDownSeconds = 3;

	private bool firstRound = true;

	public LightBikes(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		this.playerManager = playerManager;
		this.contentManager = contentManager;
		LightBikesHelper.soundManager = soundManager;
		AIMODE = demoMode;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		fastFinish = false;
		m_Random = new Random();
		forcedRestart = false;
		hudFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		whiteOutTexture = new Texture2D(base.GraphicsDevice, 1, 1);
		whiteOutTexture.SetData(new Color[1] { Color.White });
		pixelSprite = contentManager.Load<Texture2D>("LightBikes/Sprites/Pixel");
		gridManager = new Grid(base.GraphicsDevice, contentManager.Load<Texture2D>("LightBikes/Sprites/gridPixel"), contentManager.Load<Effect>("LightBikes/Shaders/GridShader"));
		finalOutRT = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		List<Color> list = new List<Color>(playerManager.AvailableColors);
		for (int i = 0; i < playerManager.NumberOfPlayers; i++)
		{
			Color playerColor = playerManager.GetPlayerColor(playerManager.PlayersConnected[i]);
			list.Remove(playerColor);
		}
		if (AIMODE)
		{
			m_Bikes = new Bike[4];
			for (int j = 0; j < 4; j++)
			{
				m_Bikes[j] = new Bike(null, new Vector2(360 + j * 20, base.GraphicsDevice.Viewport.Height / 2), 1f, contentManager.Load<Texture2D>("LightBikes/Sprites/Pixel"), 40 + j * 30, 100, m_Random, playerManager, inAimode: true, gridManager, j, list);
			}
			return;
		}
		m_Bikes = new Bike[4];
		for (int k = 0; k < playerManager.NumberOfPlayers; k++)
		{
			m_Bikes[k] = new Bike(playerManager.PlayersConnected[k], new Vector2(360 + k * 20, base.GraphicsDevice.Viewport.Height / 2), 1f, contentManager.Load<Texture2D>("LightBikes/Sprites/Pixel"), 40 + k * 30, 100, m_Random, playerManager, inAimode: false, gridManager, k, null);
		}
		for (int l = 0; l < 4; l++)
		{
			if (m_Bikes[l] == null)
			{
				m_Bikes[l] = new Bike(null, new Vector2(360 + l * 20, base.GraphicsDevice.Viewport.Height / 2), 1f, contentManager.Load<Texture2D>("LightBikes/Sprites/Pixel"), 40 + l * 30, 100, m_Random, playerManager, inAimode: true, gridManager, l, list);
			}
		}
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		m_TimePassed++;
		if (firstRound)
		{
			if (AIMODE)
			{
				firstRound = false;
			}
			else if (firstUpdate)
			{
				firstUpdate = false;
				Bike[] bikes = m_Bikes;
				foreach (Bike bike in bikes)
				{
					bike.Update(gridManager);
				}
			}
			else
			{
				gameClockTimeMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
				if (gameClockTimeMilliseconds >= 1000f)
				{
					gameClockTimeMilliseconds -= 1000f;
					countDownSeconds--;
				}
				if (countDownSeconds < 1)
				{
					firstRound = false;
				}
			}
		}
		if (soundFilterDirection)
		{
			soundFilter += 0.01f;
			if (soundFilter > 6f)
			{
				soundFilterDirection = false;
				soundFilter -= 0.01f;
			}
		}
		else
		{
			soundFilter -= 0.01f;
			if (soundFilter < 1f)
			{
				soundFilterDirection = true;
				soundFilter += 0.01f;
			}
		}
		_soundManager.SetGlobalVariable("Filterness", soundFilter);
		gridManager.Update();
		if (!firstRound && !resetting)
		{
			if (!fastFinish)
			{
				Bike[] bikes2 = m_Bikes;
				foreach (Bike bike2 in bikes2)
				{
					bike2.Update(gridManager);
				}
			}
			else
			{
				for (int k = 0; k < 3; k++)
				{
					Bike[] bikes3 = m_Bikes;
					foreach (Bike bike3 in bikes3)
					{
						bike3.Update(gridManager);
					}
				}
			}
		}
		if (!resetting)
		{
			if (lastPlayerAlive() != -1 && !forceResetCounterActive)
			{
				winnerPlayerRef = m_Bikes[lastPlayerAlive()].getPlayer();
				forceResetCounterActive = true;
				winnerIndex = lastPlayerAlive();
			}
			if (forceResetCounterActive)
			{
				forceResetCounter--;
				if (forceResetCounter < 0)
				{
					forceResetCounterActive = false;
					resetting = true;
					isCleared = false;
					resetPauseTimer = 60;
					resetTimer = 60;
				}
			}
		}
		if (!resetting)
		{
			bool flag = true;
			Bike[] bikes4 = m_Bikes;
			foreach (Bike bike4 in bikes4)
			{
				if (bike4.isAlive())
				{
					flag = false;
				}
			}
			if (flag && !forceResetCounterActive)
			{
				resetting = true;
				isCleared = false;
				resetPauseTimer = 60;
				resetTimer = 60;
			}
		}
		else
		{
			if (resetPauseTimer > 0)
			{
				resetPauseTimer--;
			}
			else
			{
				resetTimer--;
			}
			if (resetTimer < 30 && !isCleared)
			{
				fastFinish = false;
				isCleared = true;
				gridManager.clearGrid();
				forceResetCounter = 90;
				winnerPlayerRef = null;
				forceResetCounterActive = false;
				forceResetCounter = 90;
				winnerIndex = -1;
				Bike[] bikes5 = m_Bikes;
				foreach (Bike bike5 in bikes5)
				{
					bike5.resetBike();
				}
				Bike[] bikes6 = m_Bikes;
				foreach (Bike bike6 in bikes6)
				{
					bike6.Update(gridManager);
				}
			}
			if (resetTimer < 0)
			{
				resetting = false;
			}
		}
		if (!AIMODE)
		{
			bool flag2 = false;
			Bike[] bikes7 = m_Bikes;
			foreach (Bike bike7 in bikes7)
			{
				if (bike7.isAlive() && bike7.getPlayer() != null)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				fastFinish = true;
			}
		}
		if (fastFinish && !resetting)
		{
			Bike[] bikes8 = m_Bikes;
			foreach (Bike bike8 in bikes8)
			{
				if (bike8.getPlayer() != null && bike8.getPlayer().GamePadManager.ButtonIsHeld(Buttons.A))
				{
					forceResetCounterActive = false;
					resetting = true;
					isCleared = false;
					resetPauseTimer = 60;
					resetTimer = 60;
				}
			}
		}
		base.Update(gameTime);
	}

	public override void Quit()
	{
		finalOutRT.Dispose();
		finalOutRT = null;
		gridManager.purge();
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.Black);
		base.GraphicsDevice.SetRenderTarget(finalOutRT);
		base.GraphicsDevice.Clear(Color.Black);
		gridManager.DrawBackground(base.GraphicsDevice, spriteBatch, finalOutRT);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
		_ = Vector2.Zero;
		gridManager.Draw(spriteBatch);
		Bike[] bikes = m_Bikes;
		foreach (Bike bike in bikes)
		{
			bike.Draw(spriteBatch);
		}
		if (resetting)
		{
			_ = (float)resetTimer / 30f;
			float num = (float)resetTimer / 2f / 30f;
			if (resetTimer < 30)
			{
				spriteBatch.Draw(pixelSprite, whiteOutRectangle, Color.White * ((float)resetTimer / 30f));
			}
			else
			{
				num = num * -1f + 1f;
				spriteBatch.Draw(pixelSprite, whiteOutRectangle, Color.White * num * 2f);
			}
		}
		if (!AIMODE)
		{
			if (!resetting && fastFinish && winnerIndex == -1)
			{
				string text = "Press A to restart.";
				spriteBatch.DrawString(hudFont, text, new Vector2(640f, 360f), Color.White, 0f, hudFont.MeasureString(text) / 2f, 1f, SpriteEffects.None, 0f);
			}
			if (winnerIndex != -1)
			{
				string text2 = ((m_Bikes[winnerIndex].getPlayer() != null) ? (winnerPlayerRef.Name + " is the winner!") : ("Player " + winnerIndex + " wins!"));
				spriteBatch.DrawString(hudFont, text2, new Vector2(640f, 360f), Color.White, 0f, hudFont.MeasureString(text2) / 2f, 1f, SpriteEffects.None, 0f);
			}
		}
		if (firstRound)
		{
			LightBikesHelper.drawStringBacking(position: new Vector2(_titleSafeArea.X + _titleSafeArea.Width / 2, _titleSafeArea.Y + _titleSafeArea.Height / 2), spriteBatch: spriteBatch, spriteFont: hudFont, singlePixelTexture: whiteOutTexture, text: countDownSeconds.ToString(), rotation: 0f, scale: 1f);
		}
		spriteBatch.End();
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		spriteBatch.Draw(finalOutRT, new Vector2(_titleSafeArea.X + _titleSafeArea.Width / 2, _titleSafeArea.Y + _titleSafeArea.Height / 2), null, Color.White, 0f, new Vector2(640f, 360f), new Vector2((float)_titleSafeArea.Width / 1280f, (float)_titleSafeArea.Height / 720f), SpriteEffects.None, 0f);
		spriteBatch.End();
		base.Draw(gameTime);
	}

	public int lastPlayerAlive()
	{
		int num = 0;
		Bike[] bikes = m_Bikes;
		foreach (Bike bike in bikes)
		{
			if (bike.isAlive())
			{
				num++;
			}
		}
		if (num != 1)
		{
			return -1;
		}
		for (int j = 0; j < m_Bikes.Length; j++)
		{
			if (m_Bikes[j].isAlive())
			{
				return j;
			}
		}
		return -1;
	}
}
