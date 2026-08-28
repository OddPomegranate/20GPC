using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SwingGems;

internal class SwingGems : Minigame
{
	private const float screenPositionIncrementSpeed = 2f;

	private const float screenPanningSpeedMaxSP = 30f;

	private const float screenPanningSpeedMaxMP = 10f;

	private const float screenPanningAccelMax = 0.3f;

	private const float leadingEdgeOfScreenWidth = 800f;

	private const int resetPauseTimerMax = 60;

	private const int resetTimerMax = 60;

	private SpriteBatch spriteBatch;

	private PlayerManager playerManager;

	private ContentManager contentManager;

	private MinigameMeta _minigame;

	private Texture2D m_Background;

	private Cave caveManager;

	private bool graceActive = true;

	private Gem[] m_Gems;

	private Random m_Random;

	private int m_TimePassed;

	private int countDownStart;

	private SpriteFont countDownFont;

	private BackgroundHandler backgroundhandler;

	private float screenPositionIncrement;

	private bool isMultiplayer;

	private float screenPanningSpeedMax;

	private float screenPanningSpeed;

	private bool playerNearingEdge;

	private float closestPlayerDistance;

	private float closestPlayerSpeed;

	private bool allDead;

	private Texture2D pixelSprite;

	private int resetPauseTimer;

	private int resetTimer;

	private bool resetting;

	private Rectangle whiteOutRectangle = new Rectangle(0, 0, 1280, 720);

	private bool isCleared;

	private SpriteFont scoreFont;

	private int score;

	private Texture2D whiteOutTexture;

	private bool firstUpdate;

	private bool firstLoad = true;

	private string lastPlayerAlive;

	public SwingGems(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		this.playerManager = playerManager;
		this.contentManager = contentManager;
		SwingGemsHelper.soundManager = soundManager;
		SwingGemsHelper._minigameMeta = minigame;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		if (firstLoad)
		{
			firstLoad = false;
		}
		else if (SwingGemsHelper._minigameMeta.BestScore < (float)(score / 10))
		{
			SwingGemsHelper._minigameMeta.SetScore(lastPlayerAlive, score / 10);
		}
		pixelSprite = new Texture2D(base.GraphicsDevice, 1, 1);
		pixelSprite.SetData(new Color[1] { Color.White });
		m_Random = new Random(100);
		scoreFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		m_Gems = new Gem[playerManager.NumberOfPlayers];
		backgroundhandler = new BackgroundHandler(base.GraphicsDevice, contentManager, m_Random);
		whiteOutTexture = new Texture2D(base.GraphicsDevice, 1, 1);
		whiteOutTexture.SetData(new Color[1] { Color.White });
		firstUpdate = true;
		for (int i = 0; i < playerManager.NumberOfPlayers; i++)
		{
			m_Gems[i] = new Gem(playerManager.PlayersConnected[i], playerManager, new Vector2(360 + i * 20, base.GraphicsDevice.Viewport.Height / 3), 1f, contentManager.Load<Texture2D>("SwingGems/Sprites/Diamond"), contentManager.Load<Texture2D>("SwingGems/Sprites/Claw"), alive: true, base.GraphicsDevice, contentManager, new Rectangle(0, 0, 1280, 720), m_Random);
		}
		if (playerManager.NumberOfPlayers == 1)
		{
			isMultiplayer = false;
			screenPanningSpeedMax = 30f;
		}
		else
		{
			isMultiplayer = true;
			screenPanningSpeedMax = 10f;
		}
		countDownFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		m_Background = contentManager.Load<Texture2D>("HeliChopper/Sprites/Background");
		lastPlayerAlive = "";
		caveManager = new Cave(base.GraphicsDevice, contentManager, -1);
	}

	protected override void UnloadContent()
	{
	}

	public override void Quit()
	{
		base.Quit();
	}

	public override void Update(GameTime gameTime)
	{
		m_TimePassed++;
		allDead = false;
		if (!resetting && firstUpdate)
		{
			firstUpdate = false;
			score = 0;
		}
		if (playerNearingEdge)
		{
			screenPanningSpeed = MathHelper.Lerp(screenPanningSpeedMax, 0f, (1280f - closestPlayerDistance) / 800f);
			if (closestPlayerDistance > 1280f)
			{
				screenPanningSpeed += Math.Abs(1280f - closestPlayerDistance) * 2f;
			}
		}
		else
		{
			screenPanningSpeed = 0f;
		}
		screenPositionIncrement = 2f + screenPanningSpeed;
		if (resetting && isCleared)
		{
			backgroundhandler.Update(0f);
		}
		else
		{
			backgroundhandler.Update(screenPositionIncrement);
		}
		if (resetting && isCleared)
		{
			caveManager.Update(m_Gems, 0f, graceActive);
		}
		else
		{
			caveManager.Update(m_Gems, screenPositionIncrement, graceActive);
		}
		bool flag = false;
		float num = 0f;
		float num2 = 0f;
		if (!isCleared)
		{
			num = 0f;
			Gem[] gems = m_Gems;
			foreach (Gem gem in gems)
			{
				gem.Update(caveManager, screenPositionIncrement);
				if (gem.getXPosition() > 480f && gem.getXPosition() > num)
				{
					flag = true;
					num = gem.getXPosition();
					num2 = gem.getXSpeed();
				}
			}
		}
		if (flag)
		{
			playerNearingEdge = true;
			closestPlayerDistance = num;
			closestPlayerSpeed = num2;
		}
		else
		{
			playerNearingEdge = false;
			closestPlayerDistance = 0f;
			closestPlayerSpeed = 0f;
		}
		if (!resetting)
		{
			allDead = true;
			Gem[] gems2 = m_Gems;
			foreach (Gem gem2 in gems2)
			{
				if (gem2.getAlive())
				{
					lastPlayerAlive = gem2.getPlayer().Name;
					allDead = false;
				}
			}
			if (allDead)
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
				isCleared = true;
				resetGame();
				Gem[] gems3 = m_Gems;
				foreach (Gem gem3 in gems3)
				{
					gem3.Update(caveManager, 0f);
				}
			}
			if (resetTimer < 0)
			{
				resetting = false;
				isCleared = false;
			}
		}
		if (!resetting)
		{
			score += (int)screenPositionIncrement;
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.CornflowerBlue);
		spriteBatch.Begin();
		_ = Vector2.Zero;
		backgroundhandler.Draw(spriteBatch);
		caveManager.Draw(spriteBatch);
		Gem[] gems = m_Gems;
		foreach (Gem gem in gems)
		{
			gem.Draw(spriteBatch);
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
			if (SwingGemsHelper._minigameMeta.BestScore <= (float)(score / 10))
			{
				SwingGemsHelper.drawStringBacking(spriteBatch, scoreFont, whiteOutTexture, "NEW HIGHSCORE", new Vector2(_titleSafeArea.X + _titleSafeArea.Width / 2, _titleSafeArea.Y + 60), 0f, 1f);
			}
		}
		string text = "Distance: " + score / 10 + "m";
		Vector2 position = new Vector2(_titleSafeArea.X + _titleSafeArea.Width / 2, _titleSafeArea.Y + 20);
		new Rectangle((int)(position.X - scoreFont.MeasureString(text).X / 2f), (int)(position.Y - scoreFont.MeasureString(text).Y / 2f), (int)scoreFont.MeasureString(text).X, (int)scoreFont.MeasureString(text).Y);
		SwingGemsHelper.drawStringBacking(spriteBatch, scoreFont, whiteOutTexture, text, position, 0f, 1f);
		spriteBatch.End();
		base.Draw(gameTime);
	}

	public void resetGame()
	{
		screenPanningSpeed = 0f;
		LoadContent();
		playerNearingEdge = false;
		closestPlayerDistance = 0f;
		closestPlayerSpeed = 0f;
		score = 0;
	}
}
