using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Rotoball;

internal class Pawn
{
	private const int boundingBoxBuffer = 25;

	private const float additionalBumpForce = 2f;

	private int teamIndex;

	public float collisionRadius;

	private int controllingIndex;

	private int controllingPlayerReference;

	private Texture2D backgroundSprite;

	private Texture2D overlaySprite;

	private Vector2 overlayOrigin;

	private float currentDirection;

	public Vector2 position;

	private Vector2 startPosition;

	private Vector2 origin;

	private Vector2 velocity;

	private Vector2 currentControllingThumb;

	private Color controllingPlayerColor;

	private int selfIndex;

	private bool hasBallStatus;

	public PlayerController controllingPlayer;

	private BoundingBox boundingBox;

	private SpriteFont overlayFont;

	private string overlayText;

	public Pawn(int inTeamIndex, ContentManager inContentManager, Vector2 initialPosition, int index)
	{
		overlayFont = inContentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		switch (index)
		{
		case 0:
		case 3:
			overlayText = "X";
			break;
		case 1:
		case 4:
			overlayText = "Y";
			break;
		case 2:
		case 5:
			overlayText = "B";
			break;
		}
		selfIndex = index;
		teamIndex = inTeamIndex;
		position = initialPosition;
		startPosition = position;
		if (teamIndex == 1)
		{
			backgroundSprite = inContentManager.Load<Texture2D>("Rotoball/Sprites/TeamAPlayerBack");
		}
		else
		{
			backgroundSprite = inContentManager.Load<Texture2D>("Rotoball/Sprites/TeamBPlayerBack");
		}
		collisionRadius = backgroundSprite.Width / 2;
		overlaySprite = inContentManager.Load<Texture2D>("Rotoball/Sprites/PlayerFront");
		overlayOrigin = new Vector2(overlaySprite.Width / 2, overlaySprite.Height / 2);
		origin = new Vector2(backgroundSprite.Width / 2, backgroundSprite.Height / 2);
	}

	public void Update(List<PlayerController> selfTeam, List<Pawn> pawnList, Ball ball, List<BoundingBox> inBoxX, List<BoundingBox> inBoxY, List<BoundingBox> goalBox)
	{
		int index = 0;
		controllingPlayer = selfTeam[index];
		if (selfTeam.Count != 1)
		{
			index = ((controllingIndex != 0) ? (controllingIndex - 1) : 0);
		}
		else if (controllingIndex != 0)
		{
			index = 0;
		}
		if (controllingIndex != 0 && selfTeam[index].getPlayerReference().GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Length() > 0.2f)
		{
			velocity += selfTeam[index].getPlayerReference().GamePadManager.GamePadStateCurrent.ThumbSticks.Left * new Vector2(1f, -1f);
		}
		controllingPlayerColor = selfTeam[index].getColor();
		position += velocity;
		velocity *= 0.8f;
		foreach (Pawn pawn in pawnList)
		{
			if (pawn.selfIndex != selfIndex && (pawn.position - position).Length() < collisionRadius * 2f)
			{
				_ = Vector2.Zero;
				_ = Vector2.Zero;
				Vector2 value = velocity - pawn.velocity;
				Vector2 vector = Vector2.Normalize(pawn.position - position);
				float num = Vector2.Dot(value, vector);
				num *= 2f;
				if (num < 1f)
				{
					num = 1f;
				}
				vector = Vector2.Multiply(vector, (float)Math.Sqrt(num));
				pawn.velocity += vector;
				velocity -= vector;
			}
		}
		if (!hasBallStatus && (ball.getPosition() - position).Length() < collisionRadius + ball.getBallRadius())
		{
			hasBallStatus = true;
			ball.attachBall(this);
		}
		if (controllingIndex == 0)
		{
			controllingPlayer = null;
		}
		else
		{
			controllingPlayer = selfTeam[index];
		}
		boundingBox = new BoundingBox(new Vector3(position - origin + -(Vector2.UnitX * 25f) + -(Vector2.UnitY * 25f), 0f), new Vector3(position.X - origin.X + (float)backgroundSprite.Width + 25f, position.Y - origin.Y + (float)backgroundSprite.Height + 25f, 0f));
		position.X = MathHelper.Clamp(position.X, 124f + (float)(backgroundSprite.Width / 2) + 25f, 1158f - (float)(backgroundSprite.Width / 2) - 25f);
		position.Y = MathHelper.Clamp(position.Y, 105f + (float)(backgroundSprite.Height / 2) + 25f, 616f - (float)(backgroundSprite.Height / 2) - 25f);
	}

	public void Draw(SpriteBatch spritebatch)
	{
		spritebatch.Draw(backgroundSprite, position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
		spritebatch.Draw(overlaySprite, position, null, (controllingIndex == 0) ? Color.DarkGray : controllingPlayerColor, 0f, overlayOrigin, 1f, SpriteEffects.None, 0f);
		spritebatch.DrawString(overlayFont, overlayText, position, Color.White, 0f, overlayFont.MeasureString(overlayText) / 2f, 1f, SpriteEffects.None, 0f);
	}

	public int getTeamIndex()
	{
		return teamIndex;
	}

	public void setControllingIndex(int index)
	{
		if (index != 0 && index != 1 && index != 2)
		{
			int num = 0;
			_ = 1 / num;
		}
		else
		{
			controllingIndex = index;
		}
	}

	public int getControllingIndex()
	{
		return controllingIndex;
	}

	public void detatchFromPawn()
	{
		controllingIndex = 0;
		controllingPlayerReference = 0;
	}

	public Vector2 getVelocity()
	{
		return velocity;
	}

	public bool attatchToPawn(int playerIndex)
	{
		if (controllingIndex != 0)
		{
			return true;
		}
		controllingIndex = playerIndex;
		controllingPlayerReference = playerIndex;
		return false;
	}

	public void setBallStatus(bool hasBallFlag)
	{
		hasBallStatus = hasBallFlag;
	}

	public void stealBall(bool hasBallFlag, Vector2 currentBallPosition)
	{
		hasBallStatus = hasBallFlag;
		RotoballHelper.soundManager.CreateGameSoundCue("rotoBall Steal").Play();
		velocity = -AngleToV2((float)Math.Atan2(currentBallPosition.Y - position.Y, currentBallPosition.X - position.X), 3f);
	}

	public bool getBallStatus()
	{
		return hasBallStatus;
	}

	public void reset()
	{
		position = startPosition;
		velocity = Vector2.Zero;
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
