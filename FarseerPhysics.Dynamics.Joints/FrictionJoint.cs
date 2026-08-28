using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class FrictionJoint : Joint
{
	public Vector2 LocalAnchorA;

	public Vector2 LocalAnchorB;

	private float _angularImpulse;

	private float _angularMass;

	private Vector2 _linearImpulse;

	private Mat22 _linearMass;

	public override Vector2 WorldAnchorA => base.BodyA.GetWorldPoint(LocalAnchorA);

	public override Vector2 WorldAnchorB
	{
		get
		{
			return base.BodyB.GetWorldPoint(LocalAnchorB);
		}
		set
		{
		}
	}

	public float MaxForce { get; set; }

	public float MaxTorque { get; set; }

	internal FrictionJoint()
	{
		base.JointType = JointType.Friction;
	}

	public FrictionJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Friction;
		LocalAnchorA = localAnchorA;
		LocalAnchorB = localAnchorB;
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		return inv_dt * _linearImpulse;
	}

	public override float GetReactionTorque(float inv_dt)
	{
		return inv_dt * _angularImpulse;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 a2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		float invMass = bodyA.InvMass;
		float invMass2 = bodyB.InvMass;
		float invI = bodyA.InvI;
		float invI2 = bodyB.InvI;
		Mat22 A = default(Mat22);
		A.Col1.X = invMass + invMass2;
		A.Col2.X = 0f;
		A.Col1.Y = 0f;
		A.Col2.Y = invMass + invMass2;
		Mat22 B = default(Mat22);
		B.Col1.X = invI * a.Y * a.Y;
		B.Col2.X = (0f - invI) * a.X * a.Y;
		B.Col1.Y = (0f - invI) * a.X * a.Y;
		B.Col2.Y = invI * a.X * a.X;
		Mat22 B2 = default(Mat22);
		B2.Col1.X = invI2 * a2.Y * a2.Y;
		B2.Col2.X = (0f - invI2) * a2.X * a2.Y;
		B2.Col1.Y = (0f - invI2) * a2.X * a2.Y;
		B2.Col2.Y = invI2 * a2.X * a2.X;
		Mat22.Add(ref A, ref B, out var R);
		Mat22.Add(ref R, ref B2, out var R2);
		_linearMass = R2.Inverse;
		_angularMass = invI + invI2;
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
			bodyB.LinearVelocityInternal += invMass2 * vector;
			bodyB.AngularVelocityInternal += invI2 * (MathUtils.Cross(a2, vector) + _angularImpulse);
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
		Body bodyB = base.BodyB;
		Vector2 linearVelocityInternal = bodyA.LinearVelocityInternal;
		float angularVelocityInternal = bodyA.AngularVelocityInternal;
		Vector2 linearVelocityInternal2 = bodyB.LinearVelocityInternal;
		float angularVelocityInternal2 = bodyB.AngularVelocityInternal;
		float invMass = bodyA.InvMass;
		float invMass2 = bodyB.InvMass;
		float invI = bodyA.InvI;
		float invI2 = bodyB.InvI;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 a2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		float num = angularVelocityInternal2 - angularVelocityInternal;
		float num2 = (0f - _angularMass) * num;
		float angularImpulse = _angularImpulse;
		float num3 = step.dt * MaxTorque;
		_angularImpulse = MathUtils.Clamp(_angularImpulse + num2, 0f - num3, num3);
		num2 = _angularImpulse - angularImpulse;
		angularVelocityInternal -= invI * num2;
		angularVelocityInternal2 += invI2 * num2;
		Vector2 v = linearVelocityInternal2 + MathUtils.Cross(angularVelocityInternal2, a2) - linearVelocityInternal - MathUtils.Cross(angularVelocityInternal, a);
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
		linearVelocityInternal2 += invMass2 * vector;
		angularVelocityInternal2 += invI2 * MathUtils.Cross(a2, vector);
		bodyA.LinearVelocityInternal = linearVelocityInternal;
		bodyA.AngularVelocityInternal = angularVelocityInternal;
		bodyB.LinearVelocityInternal = linearVelocityInternal2;
		bodyB.AngularVelocityInternal = angularVelocityInternal2;
	}

	internal override bool SolvePositionConstraints()
	{
		return true;
	}
}
