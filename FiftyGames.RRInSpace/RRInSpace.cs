using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RRInSpace;

internal class RRInSpace : Minigame
{
	private const float whiteOutCounterSpeed = 0.025f;

	private SpriteBatch spriteBatch;

	private PlayerManager playerManager;

	private ContentManager contentManager;

	private Track m_Track;

	private Texture2D m_Background;

	private bool graceActive = true;

	private Ship[] m_Ships;

	private Random m_Random;

	private int nextPlace = 1;

	private SpriteFont hudFont;

	private Texture2D singlePixelTexture;

	private bool gameOver;

	private bool isWhiteOutActive;

	private float whiteOutCounter;

	public RRInSpace(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		this.playerManager = playerManager;
		this.contentManager = contentManager;
		RRinSpaceHelper.soundManager = soundManager;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		gameOver = false;
		m_Random = new Random();
		nextPlace = 1;
		m_Track = new Track(base.GraphicsDevice, contentManager);
		hudFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		singlePixelTexture = new Texture2D(base.GraphicsDevice, 1, 1);
		singlePixelTexture.SetData(new Color[1] { Color.White });
		m_Ships = new Ship[playerManager.NumberOfPlayers];
		m_Background = contentManager.Load<Texture2D>("RRinSpace/Sprites/Background");
		for (int i = 0; i < playerManager.NumberOfPlayers; i++)
		{
			m_Ships[i] = new Ship(playerManager.PlayersConnected[i], playerManager, new Vector2(300 + i * 30, 500 + i * 30), 1f, contentManager.Load<Texture2D>("RRinSpace/Sprites/Ship"), contentManager.Load<Texture2D>("RRinSpace/Sprites/ShipPlayerOverlay"), contentManager.Load<Texture2D>("RRinSpace/Sprites/ThrusterL"), contentManager.Load<Texture2D>("RRinSpace/Sprites/ThrusterR"), i, 4);
		}
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		m_Track.Update();
		Ship[] ships = m_Ships;
		foreach (Ship ship in ships)
		{
			ship.Update(m_Track, m_Ships);
		}
		Ship[] ships2 = m_Ships;
		foreach (Ship ship2 in ships2)
		{
			if (!ship2.getAlive() && !ship2.getPlaced())
			{
				ship2.setPlaced(placing: true);
				ship2.setPlace(nextPlace);
				nextPlace++;
			}
		}
		if (nextPlace > m_Ships.Length)
		{
			gameOver = true;
		}
		if (gameOver && !isWhiteOutActive)
		{
			Ship[] ships3 = m_Ships;
			foreach (Ship ship3 in ships3)
			{
				if (ship3.isAPressed())
				{
					isWhiteOutActive = true;
				}
			}
		}
		if (isWhiteOutActive)
		{
			whiteOutCounter += 0.025f;
			if (whiteOutCounter > 1f && gameOver)
			{
				gameOver = false;
				LoadContent();
			}
			if (whiteOutCounter > 2f)
			{
				whiteOutCounter = 0f;
				isWhiteOutActive = false;
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
		_ = Vector2.Zero;
		base.GraphicsDevice.Clear(Color.CornflowerBlue);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);
		spriteBatch.Draw(m_Background, Vector2.Zero, Color.White);
		m_Track.Draw(spriteBatch);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null);
		Ship[] ships = m_Ships;
		foreach (Ship ship in ships)
		{
			ship.Draw(spriteBatch);
		}
		Ship[] ships2 = m_Ships;
		foreach (Ship ship2 in ships2)
		{
			Vector2 position = new Vector2(426 + ((ship2.getPlayerIndex() % 2 == 1) ? 453 : 0), 300 + ((ship2.getPlayerIndex() >= 2) ? 95 : 0));
			string text = "Player " + (ship2.getPlayerIndex() + 1);
			spriteBatch.DrawString(hudFont, text, position, ship2.getHUDColor(), 0f, hudFont.MeasureString(text) / 2f, 1f, SpriteEffects.None, 0f);
			position.Y += 20f;
			if (ship2.getLapsRemaining() > 1)
			{
				text = ship2.getLapsRemaining() + " Laps";
			}
			else if (ship2.getLapsRemaining() == 1)
			{
				text = "Final Lap";
			}
			else
			{
				switch (ship2.getPlace())
				{
				case 1:
					text = "First";
					break;
				case 2:
					text = "Second";
					break;
				case 3:
					text = "Third";
					break;
				case 4:
					text = "Fourth";
					break;
				}
			}
			spriteBatch.DrawString(hudFont, text, position, ship2.getHUDColor(), 0f, hudFont.MeasureString(text) / 2f, 1f, SpriteEffects.None, 0f);
		}
		if (gameOver)
		{
			string text2 = "Press A to Play Again";
			Vector2 position2 = new Vector2(640f, 360f);
			spriteBatch.DrawString(hudFont, text2, position2, Color.White, 0f, hudFont.MeasureString(text2) / 2f, 1f, SpriteEffects.None, 0f);
		}
		if (isWhiteOutActive)
		{
			spriteBatch.Draw(singlePixelTexture, new Rectangle(0, 0, 1280, 720), Color.White * ((whiteOutCounter < 1f) ? whiteOutCounter : (1f - (whiteOutCounter - 1f))));
		}
		spriteBatch.End();
		base.Draw(gameTime);
	}
}
