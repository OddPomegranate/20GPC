using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Impossible;

internal class Runner
{
	private const int frameLimit = 2;

	private const int frameTimerLimit = 5;

	private const float gravity = 0.05f;

	private const float jumpImpulse = -1.2f;

	private const int deathCounterLimit = 3;

	private const int jumpCounterLimit = 40;

	private Player m_Player;

	private Vector2 m_Position;

	private Color m_Colour;

	private float m_Scale;

	private bool m_Alive;

	private float m_Rotation;

	private Texture2D m_PlayerSprite;

	private Vector2 m_origin;

	private float xMomentum;

	private int frameCounter;

	private int frameOffset = 5;

	private int frameTimer;

	private Vector2 playerDimensions = new Vector2(5f, 10f);

	private Vector2 tempVector2 = Vector2.Zero;

	private int deathCounter;

	private float verticalMomentum;

	private int jumpCounter;

	private bool isJumping;

	private Texture2D debugSprite;

	private BoundingBox collisionBox = new BoundingBox(Vector3.Zero, new Vector3(5f, 10f, 0f));

	private bool jumpButtonLock = true;

	private PlayerManager pManager;

	private bool jumpSoundLock;

	public Runner(Player player, PlayerManager inPManager, Vector2 position, float scale, Texture2D m_playerSprite, bool alive, Texture2D boundingSprite)
	{
		m_Player = player;
		debugSprite = boundingSprite;
		m_Position = position;
		m_Scale = 1f;
		m_Alive = true;
		pManager = inPManager;
		m_PlayerSprite = m_playerSprite;
		m_origin = new Vector2(2f, 5f);
		m_Colour = pManager.GetPlayerColor(player);
	}

	public Player getPlayer()
	{
		return m_Player;
	}

	public void updateBoundingBoxPosition()
	{
		tempVector2 = new Vector2(2f, 5f);
		collisionBox = new BoundingBox(new Vector3(m_Position - tempVector2, 0f), new Vector3(m_Position + playerDimensions - tempVector2, 0f));
	}

	public void Update(Forground forground)
	{
		if (m_Alive)
		{
			if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed && !jumpButtonLock)
			{
				if (!jumpSoundLock)
				{
					jumpSoundLock = true;
					ImpossibleHelper.soundManager.CreateGameSoundCue("theImpossibleGame Jump").Play();
				}
				jumpCounter++;
				verticalMomentum = -1.2f * ((40f - (float)jumpCounter) / 40f);
				if (jumpCounter > 40)
				{
					jumpCounter = 0;
					jumpButtonLock = true;
					verticalMomentum = 0f;
				}
			}
			else
			{
				verticalMomentum += 0.05f;
			}
			if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Released && !jumpButtonLock && jumpCounter > 0)
			{
				jumpButtonLock = true;
				verticalMomentum = 0f;
				jumpCounter = 0;
			}
			if ((double)verticalMomentum > 0.1)
			{
				jumpButtonLock = true;
			}
			m_Position.Y += verticalMomentum;
			updateBoundingBoxPosition();
			foreach (tower tower in forground.getTowerList())
			{
				if (tower.getCollisionBox().Intersects(collisionBox) && verticalMomentum > 0f)
				{
					if (jumpSoundLock)
					{
						jumpSoundLock = false;
						ImpossibleHelper.soundManager.CreateGameSoundCue("theImpossibleGame Land").Play();
					}
					m_Position.Y -= verticalMomentum;
					verticalMomentum = 0f;
					jumpButtonLock = false;
				}
				if (tower.getSideCollisionBox().Intersects(collisionBox))
				{
					m_Alive = false;
				}
				if (tower.getRoofCollisionBox().Intersects(collisionBox))
				{
					m_Position.Y += Math.Abs(verticalMomentum);
					verticalMomentum = 0f;
					jumpButtonLock = true;
				}
				if (tower.getRoofSideCollisionBox().Intersects(collisionBox))
				{
					m_Alive = false;
				}
			}
		}
		if (!m_Alive)
		{
			deathCounter = 3;
			m_Position.X -= 0.6f;
			m_Position.Y += 0.6f;
			m_Rotation -= 0.2f;
		}
		frameTimer++;
		if (frameTimer > 5)
		{
			frameTimer = 0;
			frameCounter++;
			if (frameCounter > 2)
			{
				frameCounter = 0;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(m_PlayerSprite, m_Position, new Rectangle(frameCounter * frameOffset, 0, 5, 10), m_Colour, m_Rotation, m_origin, m_Scale, SpriteEffects.None, 0f);
	}

	public bool getAlive()
	{
		return m_Alive;
	}
}
