using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class DistanceJoint : Joint
{
	public Vector2 LocalAnchorA;

	public Vector2 LocalAnchorB;

	private float _bias;

	private float _gamma;

	private float _impulse;

	private float _mass;

	private float _tmpFloat1;

	private Vector2 _tmpVector1;

	private Vector2 _u;

	public float Length { get; set; }

	public float Frequency { get; set; }

	public float DampingRatio { get; set; }

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

	internal DistanceJoint()
	{
		base.JointType = JointType.Distance;
	}

	public DistanceJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Distance;
		LocalAnchorA = localAnchorA;
		LocalAnchorB = localAnchorB;
		Length = (WorldAnchorB - WorldAnchorA).Length();
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		return inv_dt * _impulse * _u;
	}

	public override float GetReactionTorque(float inv_dt)
	{
		return 0f;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		Vector2 a = MathUtils.Multiply(ref bodyA.Xf.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 a2 = MathUtils.Multiply(ref bodyB.Xf.R, LocalAnchorB - bodyB.LocalCenter);
		_u = bodyB.Sweep.C + a2 - bodyA.Sweep.C - a;
		float num = _u.Length();
		if (num > 0.005f)
		{
			_u *= 1f / num;
		}
		else
		{
			_u = Vector2.Zero;
		}
		MathUtils.Cross(ref a, ref _u, out var c);
		MathUtils.Cross(ref a2, ref _u, out var c2);
		float num2 = bodyA.InvMass + bodyA.InvI * c * c + bodyB.InvMass + bodyB.InvI * c2 * c2;
		_mass = ((num2 != 0f) ? (1f / num2) : 0f);
		if (Frequency > 0f)
		{
			float num3 = num - Length;
			float num4 = (float)Math.PI * 2f * Frequency;
			float num5 = 2f * _mass * DampingRatio * num4;
			float num6 = _mass * num4 * num4;
			_gamma = step.dt * (num5 + step.dt * num6);
			_gamma = ((_gamma != 0f) ? (1f / _gamma) : 0f);
			_bias = num3 * step.dt * num6 * _gamma;
			_mass = num2 + _gamma;
			_mass = ((_mass != 0f) ? (1f / _mass) : 0f);
		}
		if (Settings.EnableWarmstarting)
		{
			_impulse *= step.dtRatio;
			Vector2 b = _impulse * _u;
			bodyA.LinearVelocityInternal -= bodyA.InvMass * b;
			MathUtils.Cross(ref a, ref b, out _tmpFloat1);
			bodyA.AngularVelocityInternal -= bodyA.InvI * _tmpFloat1;
			bodyB.LinearVelocityInternal += bodyB.InvMass * b;
			MathUtils.Cross(ref a2, ref b, out _tmpFloat1);
			bodyB.AngularVelocityInternal += bodyB.InvI * _tmpFloat1;
		}
		else
		{
			_impulse = 0f;
		}
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 a2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		MathUtils.Cross(bodyA.AngularVelocityInternal, ref a, out _tmpVector1);
		Vector2 vector = bodyA.LinearVelocityInternal + _tmpVector1;
		MathUtils.Cross(bodyB.AngularVelocityInternal, ref a2, out _tmpVector1);
		Vector2 vector2 = bodyB.LinearVelocityInternal + _tmpVector1;
		float num = Vector2.Dot(_u, vector2 - vector);
		float num2 = (0f - _mass) * (num + _bias + _gamma * _impulse);
		_impulse += num2;
		Vector2 b = num2 * _u;
		bodyA.LinearVelocityInternal -= bodyA.InvMass * b;
		MathUtils.Cross(ref a, ref b, out _tmpFloat1);
		bodyA.AngularVelocityInternal -= bodyA.InvI * _tmpFloat1;
		bodyB.LinearVelocityInternal += bodyB.InvMass * b;
		MathUtils.Cross(ref a2, ref b, out _tmpFloat1);
		bodyB.AngularVelocityInternal += bodyB.InvI * _tmpFloat1;
	}

	internal override bool SolvePositionConstraints()
	{
		if (Frequency > 0f)
		{
			return true;
		}
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 a2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		Vector2 u = bodyB.Sweep.C + a2 - bodyA.Sweep.C - a;
		float num = u.Length();
		if (num == 0f)
		{
			return true;
		}
		u /= num;
		float a3 = num - Length;
		a3 = MathUtils.Clamp(a3, -0.2f, 0.2f);
		float num2 = (0f - _mass) * a3;
		_u = u;
		Vector2 b = num2 * _u;
		bodyA.Sweep.C -= bodyA.InvMass * b;
		MathUtils.Cross(ref a, ref b, out _tmpFloat1);
		bodyA.Sweep.A -= bodyA.InvI * _tmpFloat1;
		bodyB.Sweep.C += bodyB.InvMass * b;
		MathUtils.Cross(ref a2, ref b, out _tmpFloat1);
		bodyB.Sweep.A += bodyB.InvI * _tmpFloat1;
		bodyA.SynchronizeTransform();
		bodyB.SynchronizeTransform();
		return Math.Abs(a3) < 0.005f;
	}
}
