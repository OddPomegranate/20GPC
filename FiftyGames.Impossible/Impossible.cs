using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Impossible;

internal class Impossible : Minigame
{
	private const int FirstRunCounterMax = 30;

	private SpriteBatch spriteBatch;

	private PlayerManager playerManager;

	private ContentManager contentManager;

	private BackgroundHandler backgroundHandler;

	private bool graceActive = true;

	private Runner[] m_Runners;

	private Random m_Random;

	private int m_TimePassed;

	private Forground forgroundReference;

	private SpriteFont countDownFont;

	private RenderTarget2D drawTarget;

	private float gameSpeed = 20f;

	private bool isSwipingOn;

	private bool isSwipingOff;

	private float swipingPosition = 450f;

	private bool allDead;

	private SpriteFont scoreFont;

	private towerImages towerImageHolder;

	private int score;

	private int scoreCounter;

	private int scoreCounterLimit = 30;

	private int highScore;

	private int lastPlayerAlive;

	private Vector2 scorePosition = new Vector2(10f, 10f);

	private float scoreZoom = 1f;

	private Texture2D characterSprite;

	private Texture2D blockSprite;

	private Texture2D m_Background;

	private Texture2D swipeSprite;

	private bool firstLoad = true;

	private float titleSafeOffsetScaled;

	private Texture2D whiteOutTexture;

	private bool firstUpdate = true;

	private bool firstRunSinceInitalise = true;

	private int FirstRunCounter = 30;

	public Impossible(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		this.playerManager = playerManager;
		this.contentManager = contentManager;
		ImpossibleHelper.soundManager = soundManager;
		ImpossibleHelper._minigameMeta = minigame;
		ImpossibleHelper._framework = (FiftyGames)game;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		if (firstLoad)
		{
			firstLoad = false;
			titleSafeOffsetScaled = (float)ImpossibleHelper._framework.TitleSafeArea.X / 1280f * 400f;
			scoreFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
			characterSprite = contentManager.Load<Texture2D>("Impossible/Sprites/Character3");
			blockSprite = contentManager.Load<Texture2D>("Impossible/Sprites/blockSprite");
			m_Background = contentManager.Load<Texture2D>("Impossible/Sprites/Background");
			swipeSprite = contentManager.Load<Texture2D>("Impossible/Sprites/HugeBricks");
			whiteOutTexture = new Texture2D(base.GraphicsDevice, 1, 1);
			whiteOutTexture.SetData(new Color[1] { Color.White });
			score = 0;
			towerImageHolder = default(towerImages);
			towerImageHolder.tinyBricks = contentManager.Load<Texture2D>("Impossible/Sprites/TinyBricks");
			towerImageHolder.tinyBottom = contentManager.Load<Texture2D>("Impossible/Sprites/TinyBottom");
			towerImageHolder.medBottom = contentManager.Load<Texture2D>("Impossible/Sprites/MedBottom");
			towerImageHolder.medBricks = contentManager.Load<Texture2D>("Impossible/Sprites/MedBricks");
			towerImageHolder.medBacking = contentManager.Load<Texture2D>("Impossible/Sprites/MedBacking");
			towerImageHolder.medRoof = contentManager.Load<Texture2D>("Impossible/Sprites/MedRoof");
			towerImageHolder.bigBottom = contentManager.Load<Texture2D>("Impossible/Sprites/BigBottom");
			towerImageHolder.bigBricks = contentManager.Load<Texture2D>("Impossible/Sprites/BigBricks");
			towerImageHolder.bigBacking = contentManager.Load<Texture2D>("Impossible/Sprites/BigBacking");
			towerImageHolder.bigRoof = contentManager.Load<Texture2D>("Impossible/Sprites/BigRoof");
			towerImageHolder.hugeBottom = contentManager.Load<Texture2D>("Impossible/Sprites/HugeBottom");
			towerImageHolder.hugeBricks = contentManager.Load<Texture2D>("Impossible/Sprites/HugeBricks");
			towerImageHolder.hugeBacking = contentManager.Load<Texture2D>("Impossible/Sprites/HugeBacking");
			towerImageHolder.hugeRoof = contentManager.Load<Texture2D>("Impossible/Sprites/HugeRoof");
			towerImageHolder.testImage = contentManager.Load<Texture2D>("Impossible/Sprites/testImage");
			spriteBatch = new SpriteBatch(base.GraphicsDevice);
			m_Random = new Random();
		}
		else
		{
			purge();
			if (ImpossibleHelper._minigameMeta.BestScore < (float)score)
			{
				ImpossibleHelper._minigameMeta.SetScore(m_Runners[lastPlayerAlive - 1].getPlayer().Name, score);
			}
		}
		firstUpdate = true;
		drawTarget = new RenderTarget2D(base.GraphicsDevice, 400, 150);
		m_Runners = new Runner[playerManager.NumberOfPlayers];
		for (int i = 0; i < playerManager.NumberOfPlayers; i++)
		{
			m_Runners[i] = new Runner(playerManager.PlayersConnected[i], playerManager, new Vector2(40 + i * 10 + (int)titleSafeOffsetScaled, 75f), 1f, characterSprite, alive: true, blockSprite);
		}
		forgroundReference = new Forground(base.GraphicsDevice, contentManager, gameSpeed, towerImageHolder);
		backgroundHandler = new BackgroundHandler(base.GraphicsDevice, m_Background);
		GC.Collect();
	}

	public void purge()
	{
		drawTarget.Dispose();
		drawTarget = null;
	}

	public override void Quit()
	{
		purge();
	}

	protected override void UnloadContent()
	{
		contentManager.Unload();
	}

	public override void Update(GameTime gameTime)
	{
		m_TimePassed++;
		backgroundHandler.Update(gameSpeed);
		if (!isSwipingOn && !isSwipingOff)
		{
			scoreCounter--;
			if (scoreCounter < 1)
			{
				scoreCounter = scoreCounterLimit;
				score++;
			}
			if (firstUpdate)
			{
				firstUpdate = false;
				score = 0;
			}
		}
		forgroundReference.Update(gameSpeed, initFlag: false);
		if (firstRunSinceInitalise)
		{
			FirstRunCounter--;
			if (FirstRunCounter < 1)
			{
				firstRunSinceInitalise = false;
			}
		}
		else
		{
			Runner[] runners = m_Runners;
			foreach (Runner runner in runners)
			{
				runner.Update(forgroundReference);
			}
		}
		int num = 0;
		int num2 = 0;
		Runner[] runners2 = m_Runners;
		foreach (Runner runner2 in runners2)
		{
			num2++;
			if (runner2.getAlive())
			{
				num++;
				if (runner2.getAlive())
				{
					lastPlayerAlive = num2;
				}
			}
		}
		if (!isSwipingOn)
		{
			allDead = true;
			Runner[] runners3 = m_Runners;
			foreach (Runner runner3 in runners3)
			{
				if (runner3.getAlive())
				{
					allDead = false;
				}
			}
			if (allDead)
			{
				allDead = false;
				isSwipingOn = true;
				highScore = score;
			}
		}
		updateSwipe();
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.CornflowerBlue);
		base.GraphicsDevice.SetRenderTarget(drawTarget);
		base.GraphicsDevice.Clear(Color.CornflowerBlue);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
		Vector2 zero = Vector2.Zero;
		backgroundHandler.Draw(spriteBatch);
		forgroundReference.Draw(spriteBatch);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
		Runner[] runners = m_Runners;
		foreach (Runner runner in runners)
		{
			runner.Draw(spriteBatch);
		}
		if (isSwipingOn || isSwipingOff)
		{
			for (int j = 0; j < 450 / swipeSprite.Width + 1; j++)
			{
				for (int k = 0; k < 150 / swipeSprite.Height + 1; k++)
				{
					zero.X = (float)(j * swipeSprite.Width) + swipingPosition;
					zero.Y = k * swipeSprite.Height;
					spriteBatch.Draw(swipeSprite, zero, Color.White);
				}
			}
		}
		spriteBatch.End();
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
		spriteBatch.Draw(drawTarget, new Rectangle(0, 0, 1280, 720), Color.White);
		ImpossibleHelper.drawStringBacking(spriteBatch, scoreFont, whiteOutTexture, "Distance: " + score + "m", new Vector2(ImpossibleHelper._framework.TitleSafeArea.X + ImpossibleHelper._framework.TitleSafeArea.Width / 2, ImpossibleHelper._framework.TitleSafeArea.Y + 20), 0f, 1f);
		string text = "Highscore: " + ImpossibleHelper._minigameMeta.BestScore + "m";
		ImpossibleHelper.drawStringBacking(spriteBatch, scoreFont, whiteOutTexture, text, new Vector2((float)ImpossibleHelper._framework.TitleSafeArea.X + scoreFont.MeasureString(text.ToString()).X / 2f, ImpossibleHelper._framework.TitleSafeArea.Y + 20), 0f, 1f);
		if ((isSwipingOn || isSwipingOff) && ImpossibleHelper._minigameMeta.BestScore <= (float)score)
		{
			ImpossibleHelper.drawStringBacking(spriteBatch, scoreFont, whiteOutTexture, "NEW HIGHSCORE", new Vector2(ImpossibleHelper._framework.TitleSafeArea.X + ImpossibleHelper._framework.TitleSafeArea.Width / 2, ImpossibleHelper._framework.TitleSafeArea.Y + 60), 0f, 1f);
		}
		spriteBatch.End();
		base.Draw(gameTime);
	}

	public void updateSwipe()
	{
		if (isSwipingOn)
		{
			swipingPosition -= 10f;
			if (swipingPosition < 1f)
			{
				isSwipingOn = false;
				isSwipingOff = true;
				LoadContent();
			}
		}
		else if (isSwipingOff)
		{
			swipingPosition -= 10f;
			if (swipingPosition < (float)(-(450 + swipeSprite.Width)))
			{
				isSwipingOff = false;
				swipingPosition = 450f;
			}
		}
	}
}
