using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.HeliChopper;

internal class HeliChopper : Minigame
{
	private const float restartCounterMax = 300f;

	private SpriteBatch spriteBatch;

	private PlayerManager playerManager;

	private ContentManager contentManager;

	private Texture2D m_Background;

	private Cave caveManager;

	private bool graceActive = true;

	private Copter[] m_Copters;

	private Random m_Random;

	private int m_TimePassed;

	private int countDownTimer = 3;

	private int countDownStart;

	private SpriteFont countDownFont;

	private SpriteFont HudFont;

	private bool firstRun = true;

	private bool AIMODE;

	private bool restart = true;

	private float restartCounter;

	private bool restartlock;

	private bool allDead;

	private int score;

	private Texture2D whiteOutTexture;

	private string lastPlayerAlive;

	public HeliChopper(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		this.playerManager = playerManager;
		this.contentManager = contentManager;
		HeliHelper.soundManager = soundManager;
		AIMODE = demoMode;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		m_Random = new Random();
		whiteOutTexture = new Texture2D(base.GraphicsDevice, 1, 1);
		whiteOutTexture.SetData(new Color[1] { Color.White });
		if (AIMODE)
		{
			m_Copters = new Copter[1];
			m_Copters[0] = new Copter(null, new Vector2(200f, base.GraphicsDevice.Viewport.Height / 2), 1f, contentManager.Load<Texture2D>("HeliChopper/Sprites/HelicopterAnimated"), contentManager.Load<Texture2D>("HeliChopper/Sprites/MaskCopter"), alive: true, AIMODE, playerManager);
		}
		else
		{
			m_Copters = new Copter[playerManager.NumberOfPlayers];
			for (int i = 0; i < playerManager.NumberOfPlayers; i++)
			{
				m_Copters[i] = new Copter(playerManager.PlayersConnected[i], new Vector2(200 + i * 130, base.GraphicsDevice.Viewport.Height / 2), 1f, contentManager.Load<Texture2D>("HeliChopper/Sprites/HelicopterAnimated"), contentManager.Load<Texture2D>("HeliChopper/Sprites/debugPixel"), alive: true, AIMODE, playerManager);
			}
		}
		countDownFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		HudFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		m_Background = contentManager.Load<Texture2D>("HeliChopper/Sprites/Background");
		restartGame();
		caveManager = new Cave(base.GraphicsDevice, contentManager.Load<Texture2D>("HeliChopper/Sprites/Land5"), contentManager.Load<Texture2D>("HeliChopper/Sprites/debugPixel"));
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		m_TimePassed++;
		caveManager.Update(m_Copters, restart, AIMODE);
		Copter[] copters = m_Copters;
		foreach (Copter copter in copters)
		{
			copter.Update(caveManager, restart);
		}
		if (!restart)
		{
			allDead = true;
			Copter[] copters2 = m_Copters;
			foreach (Copter copter2 in copters2)
			{
				if (!copter2.deadAndOffScreen())
				{
					lastPlayerAlive = copter2.getPlayerName();
					allDead = false;
				}
			}
			bool flag = false;
			Copter[] copters3 = m_Copters;
			foreach (Copter copter3 in copters3)
			{
				if (copter3.alive)
				{
					flag = true;
				}
			}
			if (flag && !AIMODE)
			{
				score++;
			}
		}
		if (allDead)
		{
			restartGame();
			allDead = false;
		}
		if (restart)
		{
			restartCounter--;
			if (restartCounter < 0f)
			{
				restart = false;
				score = 0;
			}
		}
		base.Update(gameTime);
	}

	public override void Quit()
	{
		base.Quit();
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.CornflowerBlue);
		spriteBatch.Begin();
		Vector2 zero = Vector2.Zero;
		for (int i = 0; i < base.GraphicsDevice.Viewport.Width; i++)
		{
			zero.X = i;
			spriteBatch.Draw(m_Background, zero, Color.White);
		}
		caveManager.Draw(spriteBatch);
		Copter[] copters = m_Copters;
		foreach (Copter copter in copters)
		{
			copter.Draw(spriteBatch);
		}
		if (!AIMODE)
		{
			string text = "Distance: " + score / 10 + "m";
			Vector2 position = new Vector2((float)(_titleSafeArea.X + _titleSafeArea.Width) - HudFont.MeasureString(text).X, (float)_titleSafeArea.Y + HudFont.MeasureString(text).Y + 5f);
			HeliHelper.drawStringBacking(spriteBatch, HudFont, whiteOutTexture, text, position, 0f, 1f);
			if ((float)(score / 10) > _minigameMeta.BestScore)
			{
				text = "New Highscore!";
				position += new Vector2(0f, HudFont.MeasureString(text).Y);
				HeliHelper.drawStringBacking(spriteBatch, HudFont, whiteOutTexture, text, position, 0f, 1f);
			}
		}
		if (restart && !AIMODE)
		{
			spriteBatch.DrawString(countDownFont, ((int)(4f * (restartCounter / 300f))).ToString(), new Vector2(642f, base.GraphicsDevice.Viewport.Height / 2 + 2), Color.Black);
			spriteBatch.DrawString(countDownFont, ((int)(4f * (restartCounter / 300f))).ToString(), new Vector2(640f, base.GraphicsDevice.Viewport.Height / 2 + 2), Color.Black);
			spriteBatch.DrawString(countDownFont, ((int)(4f * (restartCounter / 300f))).ToString(), new Vector2(638f, base.GraphicsDevice.Viewport.Height / 2 - 2), Color.Black);
			spriteBatch.DrawString(countDownFont, ((int)(4f * (restartCounter / 300f))).ToString(), new Vector2(640f, base.GraphicsDevice.Viewport.Height / 2 - 2), Color.Black);
			spriteBatch.DrawString(countDownFont, ((int)(4f * (restartCounter / 300f))).ToString(), new Vector2(640f, base.GraphicsDevice.Viewport.Height / 2), Color.Red);
		}
		spriteBatch.End();
		base.Draw(gameTime);
	}

	public void restartGame()
	{
		restart = true;
		restartCounter = 300f;
		if (!AIMODE && (float)(score / 10) > _minigameMeta.BestScore)
		{
			_minigameMeta.SetScore(lastPlayerAlive, score / 10);
		}
		lastPlayerAlive = "";
		Copter[] copters = m_Copters;
		foreach (Copter copter in copters)
		{
			copter.resurrectCopter();
		}
	}
}
