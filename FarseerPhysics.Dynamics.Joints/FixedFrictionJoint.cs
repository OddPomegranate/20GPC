using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class FixedFrictionJoint : Joint
{
	public Vector2 LocalAnchorA;

	public float MaxForce;

	public float MaxTorque;

	private float _angularImpulse;

	private float _angularMass;

	private Vector2 _linearImpulse;

	private Mat22 _linearMass;

	public override Vector2 WorldAnchorA => base.BodyA.GetWorldPoint(LocalAnchorA);

	public override Vector2 WorldAnchorB
	{
		get
		{
			return Vector2.Zero;
		}
		set
		{
		}
	}

	public FixedFrictionJoint(Body body, Vector2 localAnchorA)
		: base(body)
	{
		base.JointType = JointType.FixedFriction;
		LocalAnchorA = localAnchorA;
		float num = (float)Math.Sqrt(2.0 * (double)(body.Inertia / body.Mass));
		MaxForce = body.Mass * 10f;
		MaxTorque = body.Mass * num * 10f;
	}

	public override Vector2 GetReactionForce(float invDT)
	{
		return invDT * _linearImpulse;
	}

	public override float GetReactionTorque(float invDT)
	{
		return invDT * _angularImpulse;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		bodyA.GetTransform(out var transform);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		float invMass = bodyA.InvMass;
		float invI = bodyA.InvI;
		Mat22 A = default(Mat22);
		A.Col1.X = invMass;
		A.Col2.X = 0f;
		A.Col1.Y = 0f;
		A.Col2.Y = invMass;
		Mat22 B = default(Mat22);
		B.Col1.X = invI * a.Y * a.Y;
		B.Col2.X = (0f - invI) * a.X * a.Y;
		B.Col1.Y = (0f - invI) * a.X * a.Y;
		B.Col2.Y = invI * a.X * a.X;
		Mat22.Add(ref A, ref B, out var R);
		_linearMass = R.Inverse;
		_angularMass = invI;
		if (_angularMass > 0f)
		{
			_angularMass = 1f / _angularMass;
		}
		if (Settings.EnableWarmstarting)
		{
			_linearImpulse *= step.dtRatio;
			_angularImpulse *= step.dtRatio;
			Vector2 vector = new Vector2(_linearImpulse.X, _linearImpulse.Y);
			bodyA.LinearVelocityInternal -= invMass * vector;
			bodyA.AngularVelocityInternal -= invI * (MathUtils.Cross(a, vector) + _angularImpulse);
		}
		else
		{
			_linearImpulse = Vector2.Zero;
			_angularImpulse = 0f;
		}
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Vector2 linearVelocityInternal = bodyA.LinearVelocityInternal;
		float angularVelocityInternal = bodyA.AngularVelocityInternal;
		float invMass = bodyA.InvMass;
		float invI = bodyA.InvI;
		bodyA.GetTransform(out var transform);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		float num = 0f - angularVelocityInternal;
		float num2 = (0f - _angularMass) * num;
		float angularImpulse = _angularImpulse;
		float num3 = step.dt * MaxTorque;
		_angularImpulse = MathUtils.Clamp(_angularImpulse + num2, 0f - num3, num3);
		num2 = _angularImpulse - angularImpulse;
		angularVelocityInternal -= invI * num2;
		Vector2 v = -linearVelocityInternal - MathUtils.Cross(angularVelocityInternal, a);
		Vector2 vector = -MathUtils.Multiply(ref _linearMass, v);
		Vector2 linearImpulse = _linearImpulse;
		_linearImpulse += vector;
		float num4 = step.dt * MaxForce;
		if (_linearImpulse.LengthSquared() > num4 * num4)
		{
			_linearImpulse.Normalize();
			_linearImpulse *= num4;
		}
		vector = _linearImpulse - linearImpulse;
		linearVelocityInternal -= invMass * vector;
		angularVelocityInternal -= invI * MathUtils.Cross(a, vector);
		bodyA.LinearVelocityInternal = linearVelocityInternal;
		bodyA.AngularVelocityInternal = angularVelocityInternal;
	}

	internal override bool SolvePositionConstraints()
	{
		return true;
	}
}
