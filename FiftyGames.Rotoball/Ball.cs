using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Rotoball;

internal class Ball
{
	private enum collisionYDirection
	{
		none,
		up,
		down
	}

	private enum collisionXDirection
	{
		none,
		left,
		right
	}

	private const int lastPlayerDelayCounterMax = 10;

	private const int XboundryCollisionTimerMax = 15;

	private const int YboundryCollisionTimerMax = 15;

	private Texture2D ballSprite;

	private Vector2 position;

	private Vector2 velocity;

	private Vector2 origin;

	public bool goal;

	public bool teamAScore;

	public bool foul;

	private bool isAttached;

	private bool shootlock;

	private Pawn attachedPawnReference;

	private float ballCollisionRadius;

	private float direction;

	private float calculatedDirection;

	private BoundingBox boundingBox;

	private Vector2 startPosition;

	private collisionYDirection lastDirectionYCollided;

	private collisionXDirection lastDirectionXCollided;

	private BoundingBox applyRollBox;

	private int lastPlayerDelayCounter;

	private Pawn lastPawn;

	private int XboundryCollisionTimer;

	private int YboundryCollisionTimer;

	public Ball(Vector2 initialPosition, Texture2D ballImage)
	{
		isAttached = false;
		position = initialPosition;
		startPosition = position;
		attachedPawnReference = null;
		ballSprite = ballImage;
		origin = new Vector2(ballSprite.Width / 2, ballSprite.Height / 2);
		ballCollisionRadius = ballSprite.Width / 2;
		applyRollBox = new BoundingBox(new Vector3(177f, 154f, 0f), new Vector3(1103f, 567f, 0f));
	}

	public void Update(List<BoundingBox> inBoxesX, List<BoundingBox> inBoxesY, List<BoundingBox> inBoxGoal)
	{
		bool flag = false;
		if (lastPlayerDelayCounter > 0)
		{
			lastPlayerDelayCounter--;
		}
		if (position.X < 0f || position.X > 1280f || position.Y < 0f || position.Y > 720f)
		{
			foul = true;
			flag = true;
			goal = true;
		}
		if (isAttached)
		{
			if (attachedPawnReference.getVelocity().Length() > 0.3f)
			{
				calculatedDirection = attachedPawnReference.V2ToAngle(attachedPawnReference.getVelocity());
				direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position + attachedPawnReference.AngleToV2(calculatedDirection, 10f), direction, attachedPawnReference.getVelocity().Length() * 0.04f);
				position = attachedPawnReference.position + attachedPawnReference.AngleToV2(direction, attachedPawnReference.collisionRadius + ballCollisionRadius);
			}
			if (attachedPawnReference.controllingPlayer != null)
			{
				if (attachedPawnReference.controllingPlayer.getPlayerReference().GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed && !shootlock)
				{
					shootlock = true;
					RotoballHelper.soundManager.CreateGameSoundCue("rotoBall Kick").Play();
					shootBall();
				}
				else if (attachedPawnReference.controllingPlayer.getPlayerReference().GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Released && shootlock)
				{
					shootlock = false;
				}
			}
			if (attachedPawnReference == null)
			{
				return;
			}
			boundingBox = new BoundingBox(new Vector3(position - origin, 0f), new Vector3(position.X - origin.X + (float)ballSprite.Width, position.Y - origin.Y + (float)ballSprite.Height, 0f));
			foreach (BoundingBox item in inBoxesX)
			{
				if (boundingBox.Intersects(item))
				{
					if (item.Min.Y < 360f)
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position + Vector2.UnitY, direction, MathHelper.Clamp(attachedPawnReference.getVelocity().Length(), 0.1f, 1f) * 0.04f);
					}
					else
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position - Vector2.UnitY, direction, MathHelper.Clamp(attachedPawnReference.getVelocity().Length(), 0.1f, 1f) * 0.04f);
					}
					position = attachedPawnReference.position + attachedPawnReference.AngleToV2(direction, attachedPawnReference.collisionRadius + ballCollisionRadius);
				}
			}
			foreach (BoundingBox item2 in inBoxGoal)
			{
				if (boundingBox.Intersects(item2))
				{
					setGoal();
					shootBall();
					flag = true;
					if (position.X > 640f)
					{
						teamAScore = true;
					}
				}
			}
			if (flag)
			{
				return;
			}
			foreach (BoundingBox item3 in inBoxesY)
			{
				if (boundingBox.Intersects(item3))
				{
					if (item3.Min.X < 640f)
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position + Vector2.UnitX, direction, MathHelper.Clamp(attachedPawnReference.getVelocity().Length(), 0.1f, 1f) * 0.04f);
					}
					else
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position - Vector2.UnitX, direction, MathHelper.Clamp(attachedPawnReference.getVelocity().Length(), 0.1f, 1f) * 0.04f);
					}
					position = attachedPawnReference.position + attachedPawnReference.AngleToV2(direction, attachedPawnReference.collisionRadius + ballCollisionRadius);
				}
			}
			position.X = MathHelper.Clamp(position.X, 128f + (float)(ballSprite.Width / 2), 1152f - (float)(ballSprite.Width / 2));
			position.Y = MathHelper.Clamp(position.Y, 105f + (float)(ballSprite.Height / 2), 616f - (float)(ballSprite.Height / 2));
			foreach (BoundingBox item4 in inBoxesX)
			{
				if (boundingBox.Intersects(item4))
				{
					if (item4.Min.Y < 360f)
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position + Vector2.UnitY, direction, 0.3f);
					}
					else
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position - Vector2.UnitY, direction, 0.3f);
					}
					position = attachedPawnReference.position + attachedPawnReference.AngleToV2(direction, attachedPawnReference.collisionRadius + ballCollisionRadius);
				}
			}
			foreach (BoundingBox item5 in inBoxesY)
			{
				if (boundingBox.Intersects(item5))
				{
					if (item5.Min.X < 640f)
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position + Vector2.UnitX, direction, 0.3f);
					}
					else
					{
						direction = RotoballHelper.TurnToFace(attachedPawnReference.position, attachedPawnReference.position - Vector2.UnitX, direction, 0.3f);
					}
					position = attachedPawnReference.position + attachedPawnReference.AngleToV2(direction, attachedPawnReference.collisionRadius + ballCollisionRadius);
				}
			}
			position.X = MathHelper.Clamp(position.X, 130f + (float)(ballSprite.Width / 2), 1150f - (float)(ballSprite.Width / 2));
			position.Y = MathHelper.Clamp(position.Y, 107f + (float)(ballSprite.Height / 2), 614f - (float)(ballSprite.Height / 2));
			return;
		}
		position += velocity;
		applyRollBox = new BoundingBox(new Vector3(177f, 154f, 0f), new Vector3(1103f, 567f, 0f));
		if ((position.X < 177f || position.X > 1103f || position.Y < 154f || position.Y > 567f) && !goal && velocity.Length() < 0.2f)
		{
			velocity += Vector2.Normalize(position - new Vector2(640f, 360f)) * -0.2f;
		}
		velocity *= 0.98f;
		boundingBox = new BoundingBox(new Vector3(position - origin, 0f), new Vector3(position.X - origin.X + (float)ballSprite.Width, position.Y - origin.Y + (float)ballSprite.Height, 0f));
		bool flag2 = false;
		foreach (BoundingBox item6 in inBoxesX)
		{
			if (boundingBox.Intersects(item6))
			{
				velocity.Y *= -1f;
				position += velocity;
				flag2 = true;
			}
		}
		if (flag2 && !foul)
		{
			XboundryCollisionTimer++;
			if (XboundryCollisionTimer > 15)
			{
				foul = true;
				goal = true;
			}
		}
		else
		{
			XboundryCollisionTimer = 0;
		}
		foreach (BoundingBox item7 in inBoxGoal)
		{
			if (boundingBox.Intersects(item7))
			{
				setGoal();
				if (position.X > 640f)
				{
					teamAScore = true;
				}
			}
		}
		if (goal)
		{
			return;
		}
		flag2 = false;
		foreach (BoundingBox item8 in inBoxesY)
		{
			if (boundingBox.Intersects(item8))
			{
				velocity.X *= -1f;
				position += velocity;
				flag2 = true;
			}
		}
		if (flag2 && !foul)
		{
			YboundryCollisionTimer++;
			if (YboundryCollisionTimer > 15)
			{
				foul = true;
				goal = true;
			}
		}
		else
		{
			YboundryCollisionTimer = 0;
		}
	}

	public void Draw(SpriteBatch spritebatch)
	{
		spritebatch.Draw(ballSprite, position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
	}

	private void newLastPawn(Pawn newLastPawn)
	{
		lastPawn = null;
	}

	public bool attachBall(Pawn newPawn)
	{
		if (lastPlayerDelayCounter < 1 || lastPawn != newPawn)
		{
			if (!isAttached)
			{
				attachedPawnReference = newPawn;
				isAttached = true;
				RotoballHelper.soundManager.CreateGameSoundCue("rotoBall Catch").Play();
			}
			else
			{
				attachedPawnReference.stealBall(hasBallFlag: false, position);
				newLastPawn(attachedPawnReference);
				attachedPawnReference = newPawn;
				isAttached = true;
			}
			direction = RotoballHelper.TurnToFace(attachedPawnReference.position, position + attachedPawnReference.getVelocity() * 3f, direction, 7f);
			float num = RotoballHelper.TurnToFace(attachedPawnReference.position, position + attachedPawnReference.getVelocity() * 3f, direction, 7f);
			float num2 = 0.9f;
			_ = attachedPawnReference.position + attachedPawnReference.AngleToV2(num + num2, attachedPawnReference.collisionRadius + ballCollisionRadius);
			_ = attachedPawnReference.position + attachedPawnReference.AngleToV2(num - num2, attachedPawnReference.collisionRadius + ballCollisionRadius);
			return true;
		}
		return false;
	}

	public void shootBall()
	{
		attachedPawnReference.setBallStatus(hasBallFlag: false);
		newLastPawn(attachedPawnReference);
		isAttached = false;
		velocity = attachedPawnReference.AngleToV2(direction, 10f);
		attachedPawnReference = null;
		position += velocity;
	}

	public void setVelocity(Vector2 inVel)
	{
		velocity = inVel;
	}

	public Vector2 getVelocity()
	{
		return velocity;
	}

	public float getBallRadius()
	{
		return ballCollisionRadius;
	}

	public Vector2 getPosition()
	{
		return position;
	}

	public void setGoal()
	{
		goal = true;
	}

	public void resetBall()
	{
		position = startPosition;
		goal = false;
		velocity = Vector2.Zero;
		teamAScore = false;
		foul = false;
	}

	public float V2ToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.Y, vector.X);
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
