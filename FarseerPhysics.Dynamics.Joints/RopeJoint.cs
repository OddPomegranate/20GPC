using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class RopeJoint : Joint
{
	public Vector2 LocalAnchorA;

	public Vector2 LocalAnchorB;

	private float _impulse;

	private float _length;

	private float _mass;

	private Vector2 _rA;

	private Vector2 _rB;

	private LimitState _state;

	private Vector2 _u;

	public float MaxLength { get; set; }

	public LimitState State => _state;

	public sealed override Vector2 WorldAnchorA => base.BodyA.GetWorldPoint(LocalAnchorA);

	public sealed override Vector2 WorldAnchorB
	{
		get
		{
			return base.BodyB.GetWorldPoint(LocalAnchorB);
		}
		set
		{
		}
	}

	internal RopeJoint()
	{
		base.JointType = JointType.Rope;
	}

	public RopeJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Rope;
		LocalAnchorA = localAnchorA;
		LocalAnchorB = localAnchorB;
		MaxLength = (WorldAnchorB - WorldAnchorA).Length();
		_mass = 0f;
		_impulse = 0f;
		_state = LimitState.Inactive;
		_length = 0f;
	}

	public override Vector2 GetReactionForce(float invDt)
	{
		return invDt * _impulse * _u;
	}

	public override float GetReactionTorque(float invDt)
	{
		return 0f;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		_rA = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		_rB = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		_u = bodyB.Sweep.C + _rB - bodyA.Sweep.C - _rA;
		_length = _u.Length();
		float num = _length - MaxLength;
		if (num > 0f)
		{
			_state = LimitState.AtUpper;
		}
		else
		{
			_state = LimitState.Inactive;
		}
		if (_length > 0.005f)
		{
			_u *= 1f / _length;
			float num2 = MathUtils.Cross(_rA, _u);
			float num3 = MathUtils.Cross(_rB, _u);
			float num4 = bodyA.InvMass + bodyA.InvI * num2 * num2 + bodyB.InvMass + bodyB.InvI * num3 * num3;
			_mass = ((num4 != 0f) ? (1f / num4) : 0f);
			if (Settings.EnableWarmstarting)
			{
				_impulse *= step.dtRatio;
				Vector2 vector = _impulse * _u;
				bodyA.LinearVelocity -= bodyA.InvMass * vector;
				bodyA.AngularVelocity -= bodyA.InvI * MathUtils.Cross(_rA, vector);
				bodyB.LinearVelocity += bodyB.InvMass * vector;
				bodyB.AngularVelocity += bodyB.InvI * MathUtils.Cross(_rB, vector);
			}
			else
			{
				_impulse = 0f;
			}
		}
		else
		{
			_u = Vector2.Zero;
			_mass = 0f;
			_impulse = 0f;
		}
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		Vector2 vector = bodyA.LinearVelocity + MathUtils.Cross(bodyA.AngularVelocity, _rA);
		Vector2 vector2 = bodyB.LinearVelocity + MathUtils.Cross(bodyB.AngularVelocity, _rB);
		float num = _length - MaxLength;
		float num2 = Vector2.Dot(_u, vector2 - vector);
		if (num < 0f)
		{
			num2 += step.inv_dt * num;
		}
		float num3 = (0f - _mass) * num2;
		float impulse = _impulse;
		_impulse = Math.Min(0f, _impulse + num3);
		num3 = _impulse - impulse;
		Vector2 vector3 = num3 * _u;
		bodyA.LinearVelocity -= bodyA.InvMass * vector3;
		bodyA.AngularVelocity -= bodyA.InvI * MathUtils.Cross(_rA, vector3);
		bodyB.LinearVelocity += bodyB.InvMass * vector3;
		bodyB.AngularVelocity += bodyB.InvI * MathUtils.Cross(_rB, vector3);
	}

	internal override bool SolvePositionConstraints()
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 vector2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		Vector2 vector3 = bodyB.Sweep.C + vector2 - bodyA.Sweep.C - vector;
		float num = vector3.Length();
		vector3.Normalize();
		float a = num - MaxLength;
		a = MathUtils.Clamp(a, 0f, 0.2f);
		float num2 = (0f - _mass) * a;
		Vector2 vector4 = num2 * vector3;
		bodyA.Sweep.C -= bodyA.InvMass * vector4;
		bodyA.Sweep.A -= bodyA.InvI * MathUtils.Cross(vector, vector4);
		bodyB.Sweep.C += bodyB.InvMass * vector4;
		bodyB.Sweep.A += bodyB.InvI * MathUtils.Cross(vector2, vector4);
		bodyA.SynchronizeTransform();
		bodyB.SynchronizeTransform();
		return num - MaxLength < 0.005f;
	}
}
