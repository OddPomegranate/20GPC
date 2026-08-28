using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.RRInSpace;

internal class Ship
{
	private Player m_Player;

	private Vector2 m_Position;

	private bool m_Alive;

	private float m_Rotation;

	private Vector2 m_Velocity;

	private Texture2D m_PlayerSprite;

	private Texture2D m_PlayerSpriteOverlay;

	private Vector2 m_origin = new Vector2(39f, 30f);

	private Vector2 m_LastPosition;

	private Texture2D m_thrusterLSprite;

	private Texture2D m_thrusterRSprite;

	private float leftThrust;

	private float rightThrust;

	private int alphaFlickerLowerLimit = 128;

	private Color alphaMaskColor = Color.White;

	private float collisionRadius = 30f;

	private int m_selfIndex;

	private Random randomgenerator;

	private BoundingBox pointBox;

	private int currentCheckpointIndex;

	private int lapsRemaining;

	private bool isPlaced;

	private int place;

	private PlayerManager pManager;

	private bool isThrottleL;

	private int leftDirectionFlag;

	private bool isThrottleR;

	private int rightDirectionFlag;

	private Cue thrusterCue;

	private Cue wallCue;

	private Cue thrusterIgniteCue;

	public Ship(Player player, PlayerManager inPManager, Vector2 position, float scale, Texture2D playerSprite, Texture2D playerOverlaySprite, Texture2D thrusterL, Texture2D thrusterR, int selfIndex, int totalLaps)
	{
		m_Player = player;
		pManager = inPManager;
		m_PlayerSprite = playerSprite;
		m_PlayerSpriteOverlay = playerOverlaySprite;
		m_Position = position;
		m_thrusterLSprite = thrusterL;
		m_thrusterRSprite = thrusterR;
		m_Alive = true;
		m_selfIndex = selfIndex;
		randomgenerator = new Random();
		lapsRemaining = totalLaps;
	}

	public bool isAPressed()
	{
		return m_Player.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed;
	}

	public void Update(Track track, Ship[] players)
	{
		Vector3 vector = new Vector3(m_Position, 0f);
		pointBox.Min = vector;
		pointBox.Max = vector;
		pointBox.Max.X++;
		pointBox.Max.Y++;
		if (m_Alive)
		{
			leftThrust = m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y;
			rightThrust = m_Player.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y;
		}
		else
		{
			leftThrust = 0f;
			rightThrust = 0f;
		}
		if (leftThrust == 0f && rightThrust == 0f && thrusterCue != null && !thrusterCue.IsStopped && !thrusterCue.IsStopping && !thrusterCue.IsDisposed)
		{
			thrusterCue.Stop(AudioStopOptions.AsAuthored);
		}
		if (leftThrust == 0f)
		{
			if (isThrottleL)
			{
				isThrottleL = false;
				leftDirectionFlag = 0;
			}
		}
		else
		{
			if (!isThrottleL)
			{
				isThrottleL = true;
				if (thrusterCue != null)
				{
					if (thrusterCue.IsStopped || thrusterCue.IsStopping)
					{
						thrusterCue.Stop(AudioStopOptions.AsAuthored);
						thrusterCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterLoop");
						thrusterCue.Play();
						isThrottleL = false;
					}
				}
				else
				{
					thrusterCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterLoop");
					thrusterCue.Play();
				}
			}
			if (leftDirectionFlag == 0)
			{
				leftDirectionFlag = ((leftThrust > 0f) ? 1 : (-1));
				thrusterIgniteCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterPress");
				thrusterIgniteCue.Play();
			}
			else
			{
				if (leftDirectionFlag == 1 && leftThrust < 0f)
				{
					thrusterIgniteCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterPress");
					thrusterIgniteCue.Play();
					leftDirectionFlag = -1;
				}
				if (leftDirectionFlag == -1 && leftThrust > 0f)
				{
					thrusterIgniteCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterPress");
					thrusterIgniteCue.Play();
					leftDirectionFlag = 1;
				}
			}
			if (leftDirectionFlag == 1 && leftThrust < 0f)
			{
				leftDirectionFlag = 0;
			}
			if (leftDirectionFlag == -1 && leftThrust > 0f)
			{
				leftDirectionFlag = 0;
			}
		}
		if (rightThrust == 0f)
		{
			if (isThrottleR)
			{
				isThrottleR = false;
				rightDirectionFlag = 0;
			}
		}
		else
		{
			if (!isThrottleR)
			{
				isThrottleR = true;
				if (thrusterCue != null)
				{
					if (thrusterCue.IsStopped || thrusterCue.IsStopping)
					{
						thrusterCue.Stop(AudioStopOptions.AsAuthored);
						thrusterCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterLoop");
						thrusterCue.Play();
					}
				}
				else
				{
					thrusterCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterLoop");
					thrusterCue.Play();
				}
			}
			if (rightDirectionFlag == 0)
			{
				rightDirectionFlag = ((rightThrust > 0f) ? 1 : (-1));
				thrusterIgniteCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterPress");
				thrusterIgniteCue.Play();
			}
			else
			{
				if (rightDirectionFlag == 1 && rightThrust < 0f)
				{
					thrusterIgniteCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterPress");
					thrusterIgniteCue.Play();
					rightDirectionFlag = -1;
				}
				if (rightDirectionFlag == -1 && rightThrust > 0f)
				{
					thrusterIgniteCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ThrusterPress");
					thrusterIgniteCue.Play();
					rightDirectionFlag = 1;
				}
			}
			if (rightDirectionFlag == 1 && rightThrust < 0f)
			{
				rightDirectionFlag = 0;
			}
			if (rightDirectionFlag == -1 && rightThrust > 0f)
			{
				rightDirectionFlag = 0;
			}
		}
		m_Rotation -= rightThrust / 25f;
		m_Rotation += leftThrust / 25f;
		m_Velocity += new Vector2((rightThrust + leftThrust) * (float)Math.Cos(m_Rotation), (rightThrust + leftThrust) * (float)Math.Sin(m_Rotation)) / 30f;
		m_Position += m_Velocity;
		m_Velocity *= 0.99f;
		foreach (Blocker blocker in track.getBlockers())
		{
			if ((blocker.getPosition() + blocker.getOrigin() - m_Position).Length() < collisionRadius)
			{
				float length = m_Velocity.Length();
				m_Velocity = AngleToV2(Vector2.Dot(blocker.getPosition(), m_Position) + (float)Math.PI, length);
				m_Position = m_LastPosition;
				playWallCollision();
			}
		}
		foreach (Ship ship in players)
		{
			if (ship.m_selfIndex != m_selfIndex && (ship.m_Position - m_Position).Length() < collisionRadius)
			{
				_ = Vector2.Zero;
				_ = Vector2.Zero;
				Vector2 value = m_Velocity - ship.m_Velocity;
				Vector2 vector2 = Vector2.Normalize(ship.m_Position - m_Position);
				float num = Vector2.Dot(value, vector2);
				if (num < 1f)
				{
					num = 1f;
				}
				vector2 = Vector2.Multiply(vector2, (float)Math.Sqrt(num));
				ship.m_Velocity += vector2;
				m_Velocity -= vector2;
				RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles ShipCollide").Play();
			}
		}
		if (m_Alive)
		{
			foreach (Checkpoint checkpoint in track.getCheckpoints())
			{
				if (checkpoint.getCollisionBox().Intersects(pointBox) && currentCheckpointIndex == checkpoint.getCheckpointIndex())
				{
					currentCheckpointIndex++;
					checkpoint.pointFlash();
					if (currentCheckpointIndex > 3)
					{
						currentCheckpointIndex = 0;
					}
					if (checkpoint.getCheckpointIndex() == 0)
					{
						lapsRemaining--;
					}
					if (lapsRemaining == 0)
					{
						m_Alive = false;
					}
				}
			}
		}
		m_LastPosition = m_Position;
		_ = m_Alive;
	}

	private void playWallCollision()
	{
		wallCue = RRinSpaceHelper.soundManager.CreateGameSoundCue("raceInCircles WallCollide");
		wallCue.Play();
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		alphaMaskColor.A = (byte)randomgenerator.Next(alphaFlickerLowerLimit, 255);
		if (leftThrust > 0f)
		{
			spriteBatch.Draw(m_thrusterLSprite, m_Position, null, alphaMaskColor, m_Rotation, m_origin, new Vector2(leftThrust, 1f), SpriteEffects.None, 0f);
		}
		else if (leftThrust < 0f)
		{
			Vector2 origin = m_origin;
			origin.X -= origin.X / 2f;
			spriteBatch.Draw(m_thrusterLSprite, m_Position, null, alphaMaskColor, m_Rotation, origin, new Vector2(0f - leftThrust, 1f), SpriteEffects.FlipHorizontally, 0f);
		}
		alphaMaskColor.A = (byte)randomgenerator.Next(alphaFlickerLowerLimit, 255);
		if (rightThrust > 0f)
		{
			spriteBatch.Draw(m_thrusterRSprite, m_Position, null, alphaMaskColor, m_Rotation, m_origin, new Vector2(rightThrust, 1f), SpriteEffects.None, 0f);
		}
		else if (rightThrust < 0f)
		{
			Vector2 origin = m_origin;
			origin.X -= origin.X / 2f;
			spriteBatch.Draw(m_thrusterRSprite, m_Position, null, alphaMaskColor, m_Rotation, origin, new Vector2(0f - rightThrust, 1f), SpriteEffects.FlipHorizontally, 0f);
		}
		spriteBatch.Draw(m_PlayerSprite, m_Position, null, Color.White, m_Rotation, m_origin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(m_PlayerSpriteOverlay, m_Position, null, pManager.GetPlayerColor(m_Player), m_Rotation, m_origin, 1f, SpriteEffects.None, 0f);
	}

	public void setPlace(int inPlace)
	{
		place = inPlace;
	}

	public int getPlace()
	{
		return place;
	}

	public bool getAlive()
	{
		return m_Alive;
	}

	public bool getPlaced()
	{
		return isPlaced;
	}

	public void setPlaced(bool placing)
	{
		isPlaced = placing;
	}

	public int getLapsRemaining()
	{
		return lapsRemaining;
	}

	public int getPlayerIndex()
	{
		return m_selfIndex;
	}

	public Color getColor()
	{
		return pManager.GetPlayerColor(m_Player);
	}

	public Color getHUDColor()
	{
		return m_Player.Colour(0.5f, 0.5f);
	}

	public BoundingBox getPointBox()
	{
		return pointBox;
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
