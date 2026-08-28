using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.HeliChopper;

internal class Copter
{
	private const float COPTERGRAVITY = -0.15f;

	private const float COPTERTHRUST = 0.4f;

	private const float AICOPTERGRAVITY = -0.2f;

	private const float AICOPTERTHRUST = 0.3f;

	private const float AISPEEDCAP = 5f;

	private const float maxTilt = 0.2f;

	private const float tiltMultiplier = 0.06f;

	private const float resCounterMax = 100f;

	private const float offScreenBufferWidth = 140f;

	private Player m_Player;

	private Vector2 m_Position;

	private Color m_Colour;

	private float m_Scale;

	private bool m_Alive;

	private bool m_wasAlive;

	private float m_Rotation;

	private Texture2D m_CopterSprite;

	private Vector2 m_CopterOrigin;

	private Vector2 initialPosition;

	private Texture2D debugTexture;

	private float xMomentum;

	private float copterMomentum;

	private float copterForce;

	private int animationCounter;

	private int animationFrames = 3;

	private int animationTimer;

	private int animationTimerLimit = 1;

	private Vector2 spriteSize = new Vector2(150f, 64f);

	private Vector2 resurrectPosition;

	private bool resurrecting = true;

	private BoundingBox collisionBox;

	private float resCounter;

	private Random randomGen;

	private PlayerManager pManager;

	private Cue throttleSound;

	public bool alive
	{
		get
		{
			return m_Alive;
		}
		set
		{
			m_Alive = value;
		}
	}

	public string getPlayerName()
	{
		if (m_Player != null)
		{
			return m_Player.Name;
		}
		return null;
	}

	public Copter(Player player, Vector2 position, float scale, Texture2D copterSprite, Texture2D indebugTexture, bool alive, bool AIMODE, PlayerManager playerManager)
	{
		randomGen = new Random();
		pManager = playerManager;
		if (AIMODE)
		{
			m_Colour = pManager.AvailableColors[randomGen.Next(pManager.AvailableColors.Count())];
		}
		else
		{
			m_Colour = pManager.GetPlayerColor(player);
			m_Player = player;
		}
		m_Position = position;
		initialPosition = position;
		m_Scale = 1f;
		m_Alive = true;
		m_CopterSprite = copterSprite;
		m_CopterOrigin = new Vector2(spriteSize.X / 2f, spriteSize.Y / 2f);
		debugTexture = indebugTexture;
		m_wasAlive = true;
	}

	public void Update(Cave caveReference, bool graceActive)
	{
		animationTimer--;
		if (animationTimer < 0)
		{
			animationTimer = animationTimerLimit;
			animationCounter++;
			if (animationCounter > animationFrames)
			{
				animationCounter = 0;
			}
		}
		if (!graceActive)
		{
			if (m_Player != null)
			{
				if (m_Alive)
				{
					if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed)
					{
						copterForce = 0.4f;
					}
					else
					{
						copterForce = -0.15f;
					}
					m_Rotation = MathHelper.Clamp(copterMomentum * 0.06f, -0.2f, 0.2f);
				}
				else if (!resurrecting)
				{
					copterForce = -0.15f;
					xMomentum += 0.4f;
					m_Position.X -= xMomentum;
					m_Rotation += 0.2f;
				}
			}
			else
			{
				int index = 18;
				int num = (int)(caveReference.getRoof()[index].getPosition().Y + (caveReference.getFloor()[index].getPosition().Y - caveReference.getRoof()[index].getPosition().Y) / 2f);
				if ((float)num < m_Position.Y - spriteSize.Y / 2f)
				{
					copterForce = 0.3f;
				}
				else
				{
					copterForce = -0.2f;
				}
				m_Rotation = MathHelper.Clamp(copterMomentum * 0.06f, -0.2f, 0.2f);
			}
		}
		if (!graceActive)
		{
			copterMomentum += copterForce;
			m_Position.Y -= copterMomentum;
		}
		if (m_Player == null)
		{
			if (copterMomentum > 5f)
			{
				copterMomentum = 5f;
			}
			if (copterMomentum < -5f)
			{
				copterMomentum = -5f;
			}
		}
		Vector2 vector = new Vector2(m_Position.X - spriteSize.X / 2f, m_Position.Y - spriteSize.Y / 2f);
		collisionBox = new BoundingBox(new Vector3(vector, 0f), new Vector3(vector + spriteSize, 0f));
		if (!graceActive && (m_Alive || !resurrecting))
		{
			for (int i = 0; i < caveReference.getRoof().Count; i++)
			{
				if (Helper.DistanceToPoint(caveReference.getRoof().ElementAt(i).getPosition(), m_Position) < 198f && caveReference.getRoof()[i].getCollisionBox().Intersects(collisionBox))
				{
					m_Alive = false;
					break;
				}
			}
			for (int j = 0; j < caveReference.getFloor().Count; j++)
			{
				if (Helper.DistanceToPoint(caveReference.getFloor().ElementAt(j).getPosition(), m_Position) < 198f && caveReference.getFloor()[j].getCollisionBox().Intersects(collisionBox))
				{
					m_Alive = false;
					break;
				}
			}
		}
		_ = m_Alive;
		if (resurrecting)
		{
			resCounter--;
			if (resCounter < 0f)
			{
				resurrecting = false;
			}
			m_Position.X = (initialPosition.X + 140f) * ((100f - resCounter) / 100f) - 140f;
		}
		if (!m_Alive)
		{
			_ = m_wasAlive;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(m_CopterSprite, m_Position, new Rectangle(140 * animationCounter, 0, 140, 64), m_Colour, m_Rotation, m_CopterOrigin, m_Scale, SpriteEffects.None, 0f);
	}

	public bool deadAndOffScreen()
	{
		if (!m_Alive && (m_Position.Y < 0f || m_Position.Y > 720f))
		{
			return true;
		}
		return false;
	}

	public void resurrectCopter()
	{
		m_Alive = true;
		resurrecting = true;
		m_Position = initialPosition;
		m_Position.X = -140f;
		resurrectPosition = m_Position;
		m_Rotation = 0f;
		resCounter = 100f;
		xMomentum = 0f;
		copterMomentum = 0f;
	}
}
