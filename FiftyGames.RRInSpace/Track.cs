using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RRInSpace;

internal class Track
{
	private List<Blocker> m_blockers = new List<Blocker>();

	private List<Checkpoint> m_checkpoints = new List<Checkpoint>();

	private GraphicsDevice graphicsDeviceReferance;

	private Random randomGenerator;

	private int innerRingTrackLimit = 14;

	private int outerRingTrackLimit = 33;

	private Vector2 focalPoint1 = new Vector2(415f, 347f);

	private Vector2 focalPoint2 = new Vector2(865f, 347f);

	private float innerRingDiam = 110f;

	private float outerRingDiam = 270f;

	private float outerSegmentLowerLimit = (float)Math.PI * 13f / 40f;

	private float outerSegmentUpperLimit = (float)Math.PI * 27f / 40f;

	public Track(GraphicsDevice graphicsDevice, ContentManager inContent)
	{
		graphicsDeviceReferance = graphicsDevice;
		randomGenerator = new Random();
		for (int i = 0; i < innerRingTrackLimit; i++)
		{
			float angle = (float)i / (float)innerRingTrackLimit * ((float)Math.PI * 2f);
			Vector2 vector = AngleToV2(angle, innerRingDiam);
			m_blockers.Add(new Blocker(graphicsDevice, inContent, focalPoint1 + vector));
		}
		for (int j = 0; j < outerRingTrackLimit; j++)
		{
			float angle = (float)j / (float)outerRingTrackLimit * ((float)Math.PI * 2f);
			if (angle < outerSegmentLowerLimit || angle > outerSegmentUpperLimit)
			{
				Vector2 vector = AngleToV2(angle - (float)Math.PI / 2f, outerRingDiam);
				m_blockers.Add(new Blocker(graphicsDevice, inContent, focalPoint1 + vector));
			}
		}
		for (int k = 0; k < innerRingTrackLimit; k++)
		{
			float angle = (float)k / (float)innerRingTrackLimit * ((float)Math.PI * 2f);
			Vector2 vector = AngleToV2(angle, innerRingDiam);
			m_blockers.Add(new Blocker(graphicsDevice, inContent, focalPoint2 + vector));
		}
		for (int l = 0; l < outerRingTrackLimit; l++)
		{
			float angle = (float)l / (float)outerRingTrackLimit * ((float)Math.PI * 2f);
			if (angle < outerSegmentLowerLimit || angle > outerSegmentUpperLimit)
			{
				Vector2 vector = AngleToV2(angle - (float)Math.PI / 2f + (float)Math.PI, outerRingDiam);
				m_blockers.Add(new Blocker(graphicsDevice, inContent, focalPoint2 + vector));
			}
		}
		m_checkpoints.Add(new Checkpoint(graphicsDevice, inContent, new Vector2(410f, 466f), startFlag: true, 0));
		m_checkpoints.Add(new Checkpoint(graphicsDevice, inContent, new Vector2(880f, 86f), startFlag: false, 1));
		m_checkpoints.Add(new Checkpoint(graphicsDevice, inContent, new Vector2(880f, 466f), startFlag: false, 2));
		m_checkpoints.Add(new Checkpoint(graphicsDevice, inContent, new Vector2(410f, 86f), startFlag: false, 3));
	}

	public void Update()
	{
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		foreach (Checkpoint checkpoint in m_checkpoints)
		{
			checkpoint.Draw(spriteBatch);
		}
		foreach (Blocker blocker in m_blockers)
		{
			blocker.Draw(spriteBatch);
		}
	}

	public List<Blocker> getBlockers()
	{
		return m_blockers;
	}

	public List<Checkpoint> getCheckpoints()
	{
		return m_checkpoints;
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
