using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public abstract class Joint
{
	public float Breakpoint = float.MaxValue;

	internal JointEdge EdgeA = new JointEdge();

	internal JointEdge EdgeB = new JointEdge();

	public bool Enabled = true;

	protected float InvIA;

	protected float InvIB;

	protected float InvMassA;

	protected float InvMassB;

	internal bool IslandFlag;

	protected Vector2 LocalCenterA;

	protected Vector2 LocalCenterB;

	public JointType JointType { get; protected set; }

	public Body BodyA { get; set; }

	public Body BodyB { get; set; }

	public abstract Vector2 WorldAnchorA { get; }

	public abstract Vector2 WorldAnchorB { get; set; }

	public object UserData { get; set; }

	public bool Active
	{
		get
		{
			if (BodyA.Enabled)
			{
				return BodyB.Enabled;
			}
			return false;
		}
	}

	public bool CollideConnected { get; set; }

	public event Action<Joint, float> Broke;

	protected Joint()
	{
	}

	protected Joint(Body body, Body bodyB)
	{
		BodyA = body;
		BodyB = bodyB;
		CollideConnected = false;
	}

	protected Joint(Body body)
	{
		BodyA = body;
		CollideConnected = false;
	}

	public abstract Vector2 GetReactionForce(float inv_dt);

	public abstract float GetReactionTorque(float inv_dt);

	protected void WakeBodies()
	{
		BodyA.Awake = true;
		if (BodyB != null)
		{
			BodyB.Awake = true;
		}
	}

	public bool IsFixedType()
	{
		if (JointType != JointType.FixedRevolute && JointType != JointType.FixedDistance && JointType != JointType.FixedPrismatic && JointType != JointType.FixedLine && JointType != JointType.FixedMouse && JointType != JointType.FixedAngle)
		{
			return JointType == JointType.FixedFriction;
		}
		return true;
	}

	internal abstract void InitVelocityConstraints(ref TimeStep step);

	internal void Validate(float invDT)
	{
		if (!Enabled)
		{
			return;
		}
		float num = GetReactionForce(invDT).Length();
		if (!(Math.Abs(num) <= Breakpoint))
		{
			Enabled = false;
			if (Broke != null)
			{
				Broke(this, num);
			}
		}
	}

	internal abstract void SolveVelocityConstraints(ref TimeStep step);

	internal abstract bool SolvePositionConstraints();
}
