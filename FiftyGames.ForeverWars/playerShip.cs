using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.ForeverWars;

internal class playerShip
{
	private const int flickerCounterMax = 2;

	private const int firingCounterMax = 4;

	private const int shieldCounterMax = 90;

	private const int playerLivesMax = 4;

	private const int bombRechargeTimerMax = 1200;

	private const int flashCounterMax = 4;

	private Player m_Player;

	private Vector2 m_Position;

	private bool m_Alive;

	private float m_Rotation;

	private Vector2 m_Velocity;

	private Texture2D m_PlayerSprite;

	private Vector2 m_origin = new Vector2(13f, 13f);

	private Vector2 m_LastPosition;

	private Texture2D m_thrusterLSprite;

	private Texture2D m_thrusterRSprite;

	private float leftThrust;

	private float rightThrust;

	private float alphaFlickerLowerLimit;

	private int flickerCounter;

	private bool flickerBool = true;

	private Color alphaMaskColor = Color.White;

	private Color playerColor;

	private float collisionRadius = 30f;

	private int m_selfIndex;

	private Random randomgenerator;

	private PlayerManager pManager;

	private GraphicsDevice graphicsDevice;

	private ContentManager contentManager;

	private int firingCounter;

	private BoundingBox collisionBox;

	private explosionManager explosionManagerRef;

	private Vector2 firingAngle;

	private bool offScreen;

	private bool isShieldsUp = true;

	private int shieldCounter = 90;

	private Texture2D shieldSprite;

	private Vector2 shieldOrigin;

	private int playerLivesRemaining = 4;

	private BoundingSphere shieldCollisionSphere;

	private Cue firingSoundCue;

	private Cue shockWaveSoundCue;

	private bool bombIsChargedValue = true;

	private int bombRechargeTimer;

	private int kills;

	private int flashCounter;

	private List<enemyScoreElement> enemyNameScoreList;

	private bool IsAI;

	private int numberOfBlocksDestroyed;

	private gridSystem gridManager;

	private bool bombIsCharged
	{
		get
		{
			return bombIsChargedValue;
		}
		set
		{
			bombIsChargedValue = value;
			if (bombIsChargedValue)
			{
				bombRechargeTimer = 0;
			}
		}
	}

	public int blocksDestroyed
	{
		get
		{
			return numberOfBlocksDestroyed;
		}
		set
		{
			numberOfBlocksDestroyed = value;
		}
	}

	public bool isOffScreen
	{
		get
		{
			return offScreen;
		}
		set
		{
			offScreen = value;
		}
	}

	public playerShip(GraphicsDevice inGraphicsDevice, ContentManager inContentManager, Player player, PlayerManager inPManager, Vector2 initialPosition, int selfIndex, explosionManager inExplosionManagerRef, gridSystem inGridManager, bool AI)
	{
		IsAI = AI;
		graphicsDevice = inGraphicsDevice;
		contentManager = inContentManager;
		m_Player = player;
		pManager = inPManager;
		gridManager = inGridManager;
		m_PlayerSprite = contentManager.Load<Texture2D>("ForeverWars\\Sprites\\Player");
		shieldSprite = contentManager.Load<Texture2D>("ForeverWars\\Sprites\\PlayerShield");
		m_Position = initialPosition;
		m_Alive = true;
		m_selfIndex = selfIndex;
		randomgenerator = new Random();
		if (!IsAI)
		{
			playerColor = inPManager.GetPlayerColor(player);
		}
		explosionManagerRef = inExplosionManagerRef;
		shieldOrigin = new Vector2(shieldSprite.Width / 2, shieldSprite.Height / 2);
		enemyNameScoreList = new List<enemyScoreElement>();
		blocksDestroyed = 0;
	}

	public void Update(Rectangle inFieldSize, int edgeBorder, List<pBullet> playerBulletList, List<eBullet> enemyBulletList)
	{
		if (m_Alive)
		{
			if (!IsAI)
			{
				Vector2 left = m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left;
				Vector2 right = m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Right;
				right = new Vector2(right.Y * -1f, right.X);
				m_Velocity = left * new Vector2(1f, -1f) * 5f;
				m_Position += m_Velocity;
				m_Position.X = MathHelper.Clamp(m_Position.X, inFieldSize.X + edgeBorder, inFieldSize.Width - edgeBorder);
				m_Position.Y = MathHelper.Clamp(m_Position.Y, inFieldSize.Y + edgeBorder, inFieldSize.Height - edgeBorder);
				if (right.Length() > 0.2f)
				{
					firingCounter--;
					if (firingCounter < 0)
					{
						firingCounter = 4;
						firingAngle = right;
						firingSoundCue = ForeverHelper.soundManager.CreateGameSoundCue("geometryWars TurretFired");
						firingSoundCue.Play();
						playerBulletList.Add(new pBullet(graphicsDevice, contentManager, m_Position, V2ToAngle(right), playerColor, inIsBomb: false, this, gridManager, m_Velocity));
					}
				}
				if (bombIsCharged && (m_Player.GamePadManager.ButtonWasPressed(Buttons.LeftTrigger) || m_Player.GamePadManager.ButtonWasPressed(Buttons.RightTrigger)))
				{
					bombIsCharged = false;
					ForeverHelper.soundManager.CreateGameSoundCue("geometryWars ShockWave").Play();
					playerBulletList.Add(new pBullet(graphicsDevice, contentManager, m_Position, V2ToAngle(right), playerColor, inIsBomb: true, this, gridManager, m_Velocity));
				}
				collisionBox = new BoundingBox(new Vector3(m_Position, 0f), new Vector3(m_Position + Vector2.One, 0f));
				shieldCollisionSphere = new BoundingSphere(new Vector3(m_Position, 0f), shieldSprite.Width / 2);
				for (int i = 0; i < enemyBulletList.Count; i++)
				{
					if (isShieldsUp)
					{
						if (enemyBulletList[i].checkForCollision(shieldCollisionSphere) && enemyBulletList[i].getTypeOfBullet() != typeOfEnemyBullet.LaserBlast)
						{
							enemyBulletList[i].destroyBullet();
							enemyBulletList.RemoveAt(i);
							i--;
						}
					}
					else if (enemyBulletList[i].checkForCollision(new BoundingSphere(new Vector3(m_Position, 0f), 1f)))
					{
						if (enemyBulletList[i].getTypeOfBullet() != typeOfEnemyBullet.LaserBlast)
						{
							enemyBulletList[i].destroyBullet();
							enemyBulletList.RemoveAt(i);
							i--;
						}
						killPlayer();
					}
				}
				if (!bombIsCharged)
				{
					bombRechargeTimer++;
					if (bombRechargeTimer > 1200)
					{
						bombRechargeTimer = 0;
						bombIsCharged = true;
					}
				}
			}
			else
			{
				collisionBox = new BoundingBox(new Vector3(new Vector2(-1000f, -1000f), 0f), new Vector3(new Vector2(-1000f, -1000f) + Vector2.One, 0f));
				shieldCollisionSphere = new BoundingSphere(new Vector3(new Vector2(-1000f, -1000f), 0f), 1f);
			}
		}
		_ = m_Alive;
		if (isShieldsUp)
		{
			shieldCounter--;
			if (shieldCounter < 0)
			{
				isShieldsUp = false;
			}
		}
	}

	public Color getPlayerColor()
	{
		return pManager.GetPlayerColor(m_Player);
	}

	public int getKills()
	{
		return kills;
	}

	public void activateShield()
	{
		isShieldsUp = true;
		shieldCounter = 90;
	}

	public int getLivesRemaining()
	{
		return playerLivesRemaining;
	}

	public bool getBombIsCharged()
	{
		return bombIsCharged;
	}

	public int getBombChargeValue()
	{
		return bombRechargeTimer;
	}

	public int getBombChargeMaxValue()
	{
		return 1200;
	}

	public Player getPlayerRef()
	{
		return m_Player;
	}

	public void addKill(string enemyName)
	{
		kills++;
		bool flag = false;
		for (int i = 0; i < enemyNameScoreList.Count; i++)
		{
			if (enemyNameScoreList[i].getName() == enemyName)
			{
				flag = true;
				enemyNameScoreList[i].incrementKills();
			}
		}
		if (!flag)
		{
			enemyNameScoreList.Add(new enemyScoreElement(enemyName));
		}
	}

	public List<enemyScoreElement> getKillList()
	{
		return enemyNameScoreList;
	}

	public void killPlayer()
	{
		if (!isShieldsUp)
		{
			playerLivesRemaining--;
			if (playerLivesRemaining > 0)
			{
				activateShield();
				bombIsCharged = true;
				explosionManagerRef.addExplosion(m_Position, 1f, explosionType.small);
				ForeverHelper.soundManager.CreateGameSoundCue("geometryWars Explosion Small").Play();
				m_Player.GamePadManager.StartVibration(1000, 1f, 1f, 100, 100);
			}
		}
		if (playerLivesRemaining < 1 && m_Alive)
		{
			m_Alive = false;
			bombIsCharged = true;
			explosionManagerRef.addExplosion(m_Position, 1f, explosionType.large);
			ForeverHelper.soundManager.CreateGameSoundCue("geometryWars Explosion Large").Play();
			m_Player.GamePadManager.StartVibration(1000, 1f);
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (IsAI || !m_Alive)
		{
			return;
		}
		if (isShieldsUp)
		{
			spriteBatch.Draw(shieldSprite, m_Position, null, playerColor, m_Rotation + (float)(shieldCounter / 90), shieldOrigin, 1f, SpriteEffects.None, 0f);
		}
		if (isShieldsUp)
		{
			flickerCounter--;
			if (flickerCounter < 0)
			{
				flickerBool = !flickerBool;
				flickerCounter = 2;
			}
			if (firingAngle.Length() < 0.2f)
			{
				spriteBatch.Draw(m_PlayerSprite, m_Position, null, playerColor * (flickerBool ? 1f : alphaFlickerLowerLimit), m_Rotation, m_origin, 1f, SpriteEffects.None, 0f);
			}
			else
			{
				spriteBatch.Draw(m_PlayerSprite, m_Position, null, playerColor * (flickerBool ? 1f : alphaFlickerLowerLimit), V2ToAngle(firingAngle), m_origin, 1f, SpriteEffects.None, 0f);
			}
		}
		else if (firingAngle.Length() < 0.2f)
		{
			spriteBatch.Draw(m_PlayerSprite, m_Position, null, playerColor, m_Rotation, m_origin, 1f, SpriteEffects.None, 0f);
		}
		else
		{
			spriteBatch.Draw(m_PlayerSprite, m_Position, null, playerColor, V2ToAngle(firingAngle), m_origin, 1f, SpriteEffects.None, 0f);
		}
	}

	public int getPlayerIndex()
	{
		return m_selfIndex;
	}

	public bool getAlive()
	{
		return m_Alive;
	}

	public Vector2 getPosition()
	{
		return m_Position;
	}

	public Color getColor()
	{
		return pManager.GetPlayerColor(m_Player);
	}

	public float V2ToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.X, vector.Y);
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
