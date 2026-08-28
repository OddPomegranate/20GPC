using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Rotoball;

internal class Rotoball : Minigame
{
	private SpriteBatch spriteBatch;

	private PlayerManager playerManager;

	private ContentManager contentManager;

	private Texture2D m_Background;

	private bool graceActive = true;

	private bool noOfPlayersLock;

	private PlayerController[] m_Players;

	private int m_TimePassed;

	private SpriteFont hudFont;

	private TeamController teamController;

	private Texture2D singlePixelTexture;

	private RenderTarget2D finalOutputRenderTarget;

	public Rotoball(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		this.playerManager = playerManager;
		this.contentManager = contentManager;
		RotoballHelper.soundManager = soundManager;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		singlePixelTexture = new Texture2D(base.GraphicsDevice, 1, 1);
		singlePixelTexture.SetData(new Color[1] { Color.White });
		finalOutputRenderTarget = new RenderTarget2D(base.GraphicsDevice, 1280, 720);
		m_Players = new PlayerController[playerManager.NumberOfPlayers];
		hudFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		if (playerManager.NumberOfPlayers == 1)
		{
			noOfPlayersLock = true;
			RotoballHelper.soundManager.ChangeToGameMusic("music Silence");
			return;
		}
		for (int i = 0; i < playerManager.NumberOfPlayers; i++)
		{
			m_Players[i] = new PlayerController(playerManager.PlayersConnected[i], playerManager, i % 2, i);
		}
		teamController = new TeamController(m_Players, playerManager.NumberOfPlayers, contentManager);
		m_Background = contentManager.Load<Texture2D>("Rotoball/Sprites/Background");
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		m_TimePassed++;
		if (!noOfPlayersLock)
		{
			teamController.Update();
		}
		base.Update(gameTime);
	}

	public override void Quit()
	{
		finalOutputRenderTarget.Dispose();
		finalOutputRenderTarget = null;
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		if (noOfPlayersLock)
		{
			base.GraphicsDevice.Clear(Color.Red);
			spriteBatch.Begin();
			spriteBatch.DrawString(hudFont, " This game is for 2 or more players only.\n Some how you broke the menu.\n Congratulations", new Vector2(200f, 200f), Color.White);
			spriteBatch.DrawString(hudFont, " By the way, you lost the game.", new Vector2(200f, 400f), Color.White);
			spriteBatch.End();
		}
		else
		{
			base.GraphicsDevice.SetRenderTarget(finalOutputRenderTarget);
			base.GraphicsDevice.Clear(Color.CornflowerBlue);
			spriteBatch.Begin();
			_ = Vector2.Zero;
			spriteBatch.Draw(m_Background, Vector2.Zero, Color.White);
			teamController.Draw(spriteBatch);
			spriteBatch.End();
			base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
			base.GraphicsDevice.Clear(new Color(102, 204, 51));
			spriteBatch.Begin();
			spriteBatch.Draw(finalOutputRenderTarget, new Vector2(_titleSafeArea.Width / 2 + _titleSafeArea.X, _titleSafeArea.Height / 2 + _titleSafeArea.Y), null, Color.White, 0f, new Vector2(1280f, 720f) * 0.5f, 1f, SpriteEffects.None, 0f);
			teamController.DrawResetOverlay(spriteBatch);
			spriteBatch.End();
		}
		base.Draw(gameTime);
	}

	public void playerNoException()
	{
		noOfPlayersLock = true;
	}
}
