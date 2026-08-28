using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class SliderJoint : Joint
{
	public Vector2 LocalAnchorA;

	public Vector2 LocalAnchorB;

	private float _bias;

	private float _gamma;

	private float _impulse;

	private float _mass;

	private Vector2 _u;

	public float MaxLength { get; set; }

	public float MinLength { get; set; }

	public float Frequency { get; set; }

	public float DampingRatio { get; set; }

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

	internal SliderJoint()
	{
		base.JointType = JointType.Slider;
	}

	public SliderJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB, float minLength, float maxlength)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Slider;
		LocalAnchorA = localAnchorA;
		LocalAnchorB = localAnchorB;
		MaxLength = maxlength;
		MinLength = minLength;
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
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 vector2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		_u = bodyB.Sweep.C + vector2 - bodyA.Sweep.C - vector;
		float num = _u.Length();
		if (!(num < MaxLength) || !(num > MinLength))
		{
			if (num > 0.005f)
			{
				_u *= 1f / num;
			}
			else
			{
				_u = Vector2.Zero;
			}
			float num2 = MathUtils.Cross(vector, _u);
			float num3 = MathUtils.Cross(vector2, _u);
			float num4 = bodyA.InvMass + bodyA.InvI * num2 * num2 + bodyB.InvMass + bodyB.InvI * num3 * num3;
			_mass = ((num4 != 0f) ? (1f / num4) : 0f);
			if (Frequency > 0f)
			{
				float num5 = num - MaxLength;
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
				Vector2 vector3 = _impulse * _u;
				bodyA.LinearVelocityInternal -= bodyA.InvMass * vector3;
				bodyA.AngularVelocityInternal -= bodyA.InvI * MathUtils.Cross(vector, vector3);
				bodyB.LinearVelocityInternal += bodyB.InvMass * vector3;
				bodyB.AngularVelocityInternal += bodyB.InvI * MathUtils.Cross(vector2, vector3);
			}
			else
			{
				_impulse = 0f;
			}
		}
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 vector2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		float num = (bodyB.Sweep.C + vector2 - bodyA.Sweep.C - vector).Length();
		if (!(num < MaxLength) || !(num > MinLength))
		{
			Vector2 vector3 = bodyA.LinearVelocityInternal + MathUtils.Cross(bodyA.AngularVelocityInternal, vector);
			Vector2 vector4 = bodyB.LinearVelocityInternal + MathUtils.Cross(bodyB.AngularVelocityInternal, vector2);
			float num2 = Vector2.Dot(_u, vector4 - vector3);
			float num3 = (0f - _mass) * (num2 + _bias + _gamma * _impulse);
			_impulse += num3;
			Vector2 vector5 = num3 * _u;
			bodyA.LinearVelocityInternal -= bodyA.InvMass * vector5;
			bodyA.AngularVelocityInternal -= bodyA.InvI * MathUtils.Cross(vector, vector5);
			bodyB.LinearVelocityInternal += bodyB.InvMass * vector5;
			bodyB.AngularVelocityInternal += bodyB.InvI * MathUtils.Cross(vector2, vector5);
		}
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
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 vector2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
		Vector2 u = bodyB.Sweep.C + vector2 - bodyA.Sweep.C - vector;
		float num = u.Length();
		if (num < MaxLength && num > MinLength)
		{
			return true;
		}
		if (num == 0f)
		{
			return true;
		}
		u /= num;
		float a = num - MaxLength;
		a = MathUtils.Clamp(a, -0.2f, 0.2f);
		float num2 = (0f - _mass) * a;
		_u = u;
		Vector2 vector3 = num2 * _u;
		bodyA.Sweep.C -= bodyA.InvMass * vector3;
		bodyA.Sweep.A -= bodyA.InvI * MathUtils.Cross(vector, vector3);
		bodyB.Sweep.C += bodyB.InvMass * vector3;
		bodyB.Sweep.A += bodyB.InvI * MathUtils.Cross(vector2, vector3);
		bodyA.SynchronizeTransform();
		bodyB.SynchronizeTransform();
		return Math.Abs(a) < 0.005f;
	}
}
