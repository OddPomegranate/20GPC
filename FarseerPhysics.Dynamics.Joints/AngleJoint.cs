using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class AngleJoint : Joint
{
	public float BiasFactor;

	public float MaxImpulse;

	public float Softness;

	private float _bias;

	private float _jointError;

	private float _massFactor;

	private float _targetAngle;

	public float TargetAngle
	{
		get
		{
			return _targetAngle;
		}
		set
		{
			if (value != _targetAngle)
			{
				_targetAngle = value;
				WakeBodies();
			}
		}
	}

	public override Vector2 WorldAnchorA => base.BodyA.Position;

	public override Vector2 WorldAnchorB
	{
		get
		{
			return base.BodyB.Position;
		}
		set
		{
		}
	}

	internal AngleJoint()
	{
		base.JointType = JointType.Angle;
	}

	public AngleJoint(Body bodyA, Body bodyB)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Angle;
		TargetAngle = 0f;
		BiasFactor = 0.2f;
		Softness = 0f;
		MaxImpulse = float.MaxValue;
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		return Vector2.Zero;
	}

	public override float GetReactionTorque(float inv_dt)
	{
		return 0f;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		_jointError = base.BodyB.Sweep.A - base.BodyA.Sweep.A - TargetAngle;
		_bias = (0f - BiasFactor) * step.inv_dt * _jointError;
		_massFactor = (1f - Softness) / (base.BodyA.InvI + base.BodyB.InvI);
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		float value = (_bias - base.BodyB.AngularVelocity + base.BodyA.AngularVelocity) * _massFactor;
		base.BodyA.AngularVelocity -= base.BodyA.InvI * (float)Math.Sign(value) * Math.Min(Math.Abs(value), MaxImpulse);
		base.BodyB.AngularVelocity += base.BodyB.InvI * (float)Math.Sign(value) * Math.Min(Math.Abs(value), MaxImpulse);
	}

	internal override bool SolvePositionConstraints()
	{
		return true;
	}
}
