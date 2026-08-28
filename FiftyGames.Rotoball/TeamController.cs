using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Rotoball;

internal class TeamController
{
	private const float sweepLimit = 100f;

	private const float sweepSpeed = 2f;

	private int teamAScore;

	private int teamBScore;

	private bool isGoal;

	private bool isFoul;

	private bool sweepIn;

	private bool sweepOut;

	private float sweepRotation;

	private float sweepCount;

	private Texture2D sweepSprite;

	private SpriteFont sweepFont;

	private List<PlayerController> teamA = new List<PlayerController>();

	private List<PlayerController> teamB = new List<PlayerController>();

	private List<Pawn> pawns = new List<Pawn>();

	private List<BoundingBox> pitchBoundariesXAligned = new List<BoundingBox>();

	private List<BoundingBox> pitchBoundariesYAligned = new List<BoundingBox>();

	private List<BoundingBox> pitchBoundariesYAlignedGoal = new List<BoundingBox>();

	private Texture2D testSprite;

	private Ball ball;

	public TeamController(PlayerController[] inPlayers, int noOfPlayers, ContentManager inContentManager)
	{
		testSprite = inContentManager.Load<Texture2D>("Impossible/Sprites/testImage");
		sweepSprite = inContentManager.Load<Texture2D>("Rotoball/Sprites/BigBall");
		sweepFont = inContentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		ball = new Ball(new Vector2(640f, 360f), inContentManager.Load<Texture2D>("Rotoball/Sprites/Ball"));
		if (noOfPlayers == 2)
		{
			teamA.Add(inPlayers[0]);
			teamA[0].setCurrentSelection(0);
			teamA[0].setTeamIndex(1);
			teamB.Add(inPlayers[1]);
			teamB[0].setCurrentSelection(3);
			teamB[0].setTeamIndex(2);
		}
		else
		{
			teamA.Add(inPlayers[0]);
			teamA[0].setCurrentSelection(0);
			teamA[0].setTeamIndex(1);
			teamA.Add(inPlayers[1]);
			teamA[1].setCurrentSelection(1);
			teamA[1].setTeamIndex(1);
			teamB.Add(inPlayers[2]);
			teamB[0].setCurrentSelection(3);
			teamB[0].setTeamIndex(2);
			if (inPlayers.Length > 3)
			{
				teamB.Add(inPlayers[3]);
				teamB[1].setCurrentSelection(4);
				teamB[1].setTeamIndex(2);
			}
		}
		for (int i = 0; i < 3; i++)
		{
			pawns.Add(new Pawn(1, inContentManager, new Vector2((i != 1) ? 320 : 182, 180 + i * 180), i));
		}
		for (int j = 0; j < 3; j++)
		{
			pawns.Add(new Pawn(2, inContentManager, new Vector2((j != 1) ? 960 : 1098, 180 + j * 180), j + 3));
		}
		if (teamA.Count == 1)
		{
			pawns[0].attatchToPawn(1);
			pawns[3].attatchToPawn(1);
		}
		else
		{
			pawns[0].attatchToPawn(1);
			pawns[3].attatchToPawn(1);
			pawns[1].attatchToPawn(2);
			if (inPlayers.Length > 3)
			{
				pawns[4].attatchToPawn(2);
			}
		}
		Vector2 value = new Vector2(124f, 100f);
		Vector2 vector = new Vector2(1030f, 9f);
		pitchBoundariesXAligned.Add(new BoundingBox(new Vector3(value, 0f), new Vector3(value.X + vector.X, value.Y + vector.Y, 0f)));
		value = new Vector2(124f, 615f);
		vector = new Vector2(1030f, 5f);
		pitchBoundariesXAligned.Add(new BoundingBox(new Vector3(value, 0f), new Vector3(value.X + vector.X, value.Y + vector.Y, 0f)));
		value = new Vector2(124f, 100f);
		vector = new Vector2(5f, 512f);
		pitchBoundariesYAligned.Add(new BoundingBox(new Vector3(value, 0f), new Vector3(value.X + vector.X, value.Y + vector.Y, 0f)));
		value = new Vector2(1152f, 104f);
		vector = new Vector2(9f, 512f);
		pitchBoundariesYAligned.Add(new BoundingBox(new Vector3(value, 0f), new Vector3(value.X + vector.X, value.Y + vector.Y, 0f)));
		value = new Vector2(126f, 270f);
		vector = new Vector2(9f, 180f);
		pitchBoundariesYAlignedGoal.Add(new BoundingBox(new Vector3(value, 0f), new Vector3(value.X + vector.X, value.Y + vector.Y, 0f)));
		value = new Vector2(1148f, 270f);
		vector = new Vector2(9f, 180f);
		pitchBoundariesYAlignedGoal.Add(new BoundingBox(new Vector3(value, 0f), new Vector3(value.X + vector.X, value.Y + vector.Y, 0f)));
	}

	public void Update()
	{
		if (ball.goal && !isGoal)
		{
			isGoal = true;
			if (ball.foul)
			{
				isFoul = true;
			}
			else if (ball.teamAScore)
			{
				teamAScore++;
			}
			else
			{
				teamBScore++;
			}
		}
		if (!isGoal)
		{
			foreach (Pawn pawn in pawns)
			{
				pawn.Update((pawn.getTeamIndex() == 1) ? teamA : teamB, pawns, ball, pitchBoundariesXAligned, pitchBoundariesYAligned, pitchBoundariesYAlignedGoal);
			}
			foreach (PlayerController item in teamA)
			{
				item.Update(pawns);
			}
			foreach (PlayerController item2 in teamB)
			{
				item2.Update(pawns);
			}
		}
		ball.Update(pitchBoundariesXAligned, pitchBoundariesYAligned, pitchBoundariesYAlignedGoal);
		if (isGoal)
		{
			if (!sweepIn && !sweepOut)
			{
				sweepIn = true;
			}
			else if (sweepIn && sweepCount > 100f)
			{
				sweepCount = 100f;
				sweepIn = false;
				sweepOut = true;
				ball.resetBall();
				foreach (Pawn pawn2 in pawns)
				{
					pawn2.reset();
				}
			}
			else if (sweepCount < 0f)
			{
				sweepOut = false;
				sweepCount = 0f;
				sweepRotation = 0f;
				isGoal = false;
				isFoul = false;
			}
		}
		if (sweepIn)
		{
			sweepCount += 2f;
			sweepRotation += 2f;
		}
		else if (sweepOut)
		{
			sweepCount -= 2f;
			sweepRotation += 2f;
		}
	}

	public void Draw(SpriteBatch spritebatch)
	{
		foreach (Pawn pawn in pawns)
		{
			pawn.Draw(spritebatch);
		}
		ball.Draw(spritebatch);
		string text = teamAScore.ToString("D2") + " | " + teamBScore.ToString("D2");
		spritebatch.DrawString(sweepFont, text, new Vector2(640f, 76f), Color.White, 0f, sweepFont.MeasureString(text) * new Vector2(0.5f, 0f), 1f, SpriteEffects.None, 0f);
	}

	public void DrawResetOverlay(SpriteBatch spritebatch)
	{
		if (sweepIn || sweepOut)
		{
			spritebatch.Draw(sweepSprite, new Vector2(640f, 360f), null, Color.White, sweepRotation * 0.02f, new Vector2(sweepSprite.Width / 2, sweepSprite.Height / 2), sweepCount * 0.06f, SpriteEffects.None, 0f);
			if (isFoul)
			{
				spritebatch.DrawString(sweepFont, "FOUL!", new Vector2(640f, 360f), Color.White, 0f, sweepFont.MeasureString("FOUL!") * 0.5f, sweepCount * 0.06f, SpriteEffects.None, 0f);
			}
			else
			{
				spritebatch.DrawString(sweepFont, "GOAL!", new Vector2(640f, 360f), Color.White, 0f, sweepFont.MeasureString("GOAL!") * 0.5f, sweepCount * 0.06f, SpriteEffects.None, 0f);
			}
		}
	}
}
