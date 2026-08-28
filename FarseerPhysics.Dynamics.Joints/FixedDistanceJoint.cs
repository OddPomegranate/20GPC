using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class FixedDistanceJoint : Joint
{
	public Vector2 LocalAnchorA;

	private float _bias;

	private float _gamma;

	private float _impulse;

	private float _mass;

	private Vector2 _u;

	private Vector2 _worldAnchorB;

	public float Length { get; set; }

	public float Frequency { get; set; }

	public float DampingRatio { get; set; }

	public sealed override Vector2 WorldAnchorA => base.BodyA.GetWorldPoint(LocalAnchorA);

	public sealed override Vector2 WorldAnchorB
	{
		get
		{
			return _worldAnchorB;
		}
		set
		{
			_worldAnchorB = value;
		}
	}

	public FixedDistanceJoint(Body body, Vector2 bodyAnchor, Vector2 worldAnchor)
		: base(body)
	{
		base.JointType = JointType.FixedDistance;
		LocalAnchorA = bodyAnchor;
		_worldAnchorB = worldAnchor;
		Length = (WorldAnchorB - WorldAnchorA).Length();
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
		bodyA.GetTransform(out var transform);
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 worldAnchorB = _worldAnchorB;
		_u = worldAnchorB - bodyA.Sweep.C - vector;
		float num = _u.Length();
		if (num > 0.005f)
		{
			_u *= 1f / num;
		}
		else
		{
			_u = Vector2.Zero;
		}
		float num2 = MathUtils.Cross(vector, _u);
		float num3 = MathUtils.Cross(worldAnchorB, _u);
		float num4 = bodyA.InvMass + bodyA.InvI * num2 * num2 + 0f * num3 * num3;
		_mass = ((num4 != 0f) ? (1f / num4) : 0f);
		if (Frequency > 0f)
		{
			float num5 = num - Length;
			float num6 = (float)Math.PI * 2f * Frequency;
			float num7 = 2f * _mass * DampingRatio * num6;
			float num8 = _mass * num6 * num6;
			_gamma = step.dt * (num7 + step.dt * num8);
			_gamma = ((_gamma != 0f) ? (1f / _gamma) : 0f);
			_bias = num5 * step.dt * num8 * _gamma;
			_mass = num4 + _gamma;
			_mass = ((_mass != 0f) ? (1f / _mass) : 0f);
		}
		if (Settings.EnableWarmstarting)
		{
			_impulse *= step.dtRatio;
			Vector2 vector2 = _impulse * _u;
			bodyA.LinearVelocityInternal -= bodyA.InvMass * vector2;
			bodyA.AngularVelocityInternal -= bodyA.InvI * MathUtils.Cross(vector, vector2);
		}
		else
		{
			_impulse = 0f;
		}
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		bodyA.GetTransform(out var transform);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 vector = bodyA.LinearVelocityInternal + MathUtils.Cross(bodyA.AngularVelocityInternal, a);
		Vector2 zero = Vector2.Zero;
		float num = Vector2.Dot(_u, zero - vector);
		float num2 = (0f - _mass) * (num + _bias + _gamma * _impulse);
		_impulse += num2;
		Vector2 vector2 = num2 * _u;
		bodyA.LinearVelocityInternal -= bodyA.InvMass * vector2;
		bodyA.AngularVelocityInternal -= bodyA.InvI * MathUtils.Cross(a, vector2);
	}

	internal override bool SolvePositionConstraints()
	{
		if (Frequency > 0f)
		{
			return true;
		}
		Body bodyA = base.BodyA;
		bodyA.GetTransform(out var transform);
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 worldAnchorB = _worldAnchorB;
		Vector2 u = worldAnchorB - bodyA.Sweep.C - vector;
		float num = u.Length();
		if (num == 0f)
		{
			return true;
		}
		u /= num;
		float a = num - Length;
		a = MathUtils.Clamp(a, -0.2f, 0.2f);
		float num2 = (0f - _mass) * a;
		_u = u;
		Vector2 vector2 = num2 * _u;
		bodyA.Sweep.C -= bodyA.InvMass * vector2;
		bodyA.Sweep.A -= bodyA.InvI * MathUtils.Cross(vector, vector2);
		bodyA.SynchronizeTransform();
		return Math.Abs(a) < 0.005f;
	}
}
