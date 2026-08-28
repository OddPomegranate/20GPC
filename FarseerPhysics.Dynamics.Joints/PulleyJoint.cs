using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class PulleyJoint : Joint
{
	public Vector2 GroundAnchorA;

	public Vector2 GroundAnchorB;

	public Vector2 LocalAnchorA;

	public Vector2 LocalAnchorB;

	public float MinPulleyLength = 2f;

	private float _ant;

	private float _impulse;

	private float _lengthA;

	private float _lengthB;

	private float _limitImpulse1;

	private float _limitImpulse2;

	private float _limitMass1;

	private float _limitMass2;

	private LimitState _limitState1;

	private LimitState _limitState2;

	private float _maxLengthA;

	private float _maxLengthB;

	private float _pulleyMass;

	private LimitState _state;

	private Vector2 _u1;

	private Vector2 _u2;

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

	public float LengthA
	{
		get
		{
			return (base.BodyA.GetWorldPoint(LocalAnchorA) - GroundAnchorA).Length();
		}
		set
		{
			_lengthA = value;
		}
	}

	public float LengthB
	{
		get
		{
			return (base.BodyB.GetWorldPoint(LocalAnchorB) - GroundAnchorB).Length();
		}
		set
		{
			_lengthB = value;
		}
	}

	public float Ratio { get; set; }

	public float MaxLengthA
	{
		get
		{
			return _maxLengthA;
		}
		set
		{
			_maxLengthA = value;
		}
	}

	public float MaxLengthB
	{
		get
		{
			return _maxLengthB;
		}
		set
		{
			_maxLengthB = value;
		}
	}

	internal PulleyJoint()
	{
		base.JointType = JointType.Pulley;
	}

	public PulleyJoint(Body bodyA, Body bodyB, Vector2 groundAnchorA, Vector2 groundAnchorB, Vector2 localAnchorA, Vector2 localAnchorB, float ratio)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Pulley;
		GroundAnchorA = groundAnchorA;
		GroundAnchorB = groundAnchorB;
		LocalAnchorA = localAnchorA;
		LocalAnchorB = localAnchorB;
		_lengthA = (base.BodyA.GetWorldPoint(localAnchorA) - groundAnchorA).Length();
		_lengthB = (base.BodyB.GetWorldPoint(localAnchorB) - groundAnchorB).Length();
		Ratio = ratio;
		float num = _lengthA + Ratio * _lengthB;
		MaxLengthA = num - Ratio * MinPulleyLength;
		MaxLengthB = (num - MinPulleyLength) / Ratio;
		_ant = _lengthA + Ratio * _lengthB;
		MaxLengthA = Math.Min(MaxLengthA, _ant - Ratio * MinPulleyLength);
		MaxLengthB = Math.Min(MaxLengthB, (_ant - MinPulleyLength) / Ratio);
		_impulse = 0f;
		_limitImpulse1 = 0f;
		_limitImpulse2 = 0f;
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		Vector2 vector = _impulse * _u2;
		return inv_dt * vector;
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
		Vector2 vector3 = bodyA.Sweep.C + vector;
		Vector2 vector4 = bodyB.Sweep.C + vector2;
		Vector2 groundAnchorA = GroundAnchorA;
		Vector2 groundAnchorB = GroundAnchorB;
		_u1 = vector3 - groundAnchorA;
		_u2 = vector4 - groundAnchorB;
		float num = _u1.Length();
		float num2 = _u2.Length();
		if (num > 0.005f)
		{
			_u1 *= 1f / num;
		}
		else
		{
			_u1 = Vector2.Zero;
		}
		if (num2 > 0.005f)
		{
			_u2 *= 1f / num2;
		}
		else
		{
			_u2 = Vector2.Zero;
		}
		float num3 = _ant - num - Ratio * num2;
		if (num3 > 0f)
		{
			_state = LimitState.Inactive;
			_impulse = 0f;
		}
		else
		{
			_state = LimitState.AtUpper;
		}
		if (num < MaxLengthA)
		{
			_limitState1 = LimitState.Inactive;
			_limitImpulse1 = 0f;
		}
		else
		{
			_limitState1 = LimitState.AtUpper;
		}
		if (num2 < MaxLengthB)
		{
			_limitState2 = LimitState.Inactive;
			_limitImpulse2 = 0f;
		}
		else
		{
			_limitState2 = LimitState.AtUpper;
		}
		float num4 = MathUtils.Cross(vector, _u1);
		float num5 = MathUtils.Cross(vector2, _u2);
		_limitMass1 = bodyA.InvMass + bodyA.InvI * num4 * num4;
		_limitMass2 = bodyB.InvMass + bodyB.InvI * num5 * num5;
		_pulleyMass = _limitMass1 + Ratio * Ratio * _limitMass2;
		_limitMass1 = 1f / _limitMass1;
		_limitMass2 = 1f / _limitMass2;
		_pulleyMass = 1f / _pulleyMass;
		if (Settings.EnableWarmstarting)
		{
			_impulse *= step.dtRatio;
			_limitImpulse1 *= step.dtRatio;
			_limitImpulse2 *= step.dtRatio;
			Vector2 vector5 = (0f - (_impulse + _limitImpulse1)) * _u1;
			Vector2 vector6 = ((0f - Ratio) * _impulse - _limitImpulse2) * _u2;
			bodyA.LinearVelocityInternal += bodyA.InvMass * vector5;
			bodyA.AngularVelocityInternal += bodyA.InvI * MathUtils.Cross(vector, vector5);
			bodyB.LinearVelocityInternal += bodyB.InvMass * vector6;
			bodyB.AngularVelocityInternal += bodyB.InvI * MathUtils.Cross(vector2, vector6);
		}
		else
		{
			_impulse = 0f;
			_limitImpulse1 = 0f;
			_limitImpulse2 = 0f;
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
		if (_state == LimitState.AtUpper)
		{
			Vector2 value = bodyA.LinearVelocityInternal + MathUtils.Cross(bodyA.AngularVelocityInternal, a);
			Vector2 value2 = bodyB.LinearVelocityInternal + MathUtils.Cross(bodyB.AngularVelocityInternal, a2);
			float num = 0f - Vector2.Dot(_u1, value) - Ratio * Vector2.Dot(_u2, value2);
			float num2 = _pulleyMass * (0f - num);
			float impulse = _impulse;
			_impulse = Math.Max(0f, _impulse + num2);
			num2 = _impulse - impulse;
			Vector2 vector = (0f - num2) * _u1;
			Vector2 vector2 = (0f - Ratio) * num2 * _u2;
			bodyA.LinearVelocityInternal += bodyA.InvMass * vector;
			bodyA.AngularVelocityInternal += bodyA.InvI * MathUtils.Cross(a, vector);
			bodyB.LinearVelocityInternal += bodyB.InvMass * vector2;
			bodyB.AngularVelocityInternal += bodyB.InvI * MathUtils.Cross(a2, vector2);
		}
		if (_limitState1 == LimitState.AtUpper)
		{
			Vector2 value3 = bodyA.LinearVelocityInternal + MathUtils.Cross(bodyA.AngularVelocityInternal, a);
			float num3 = 0f - Vector2.Dot(_u1, value3);
			float num4 = (0f - _limitMass1) * num3;
			float limitImpulse = _limitImpulse1;
			_limitImpulse1 = Math.Max(0f, _limitImpulse1 + num4);
			num4 = _limitImpulse1 - limitImpulse;
			Vector2 vector3 = (0f - num4) * _u1;
			bodyA.LinearVelocityInternal += bodyA.InvMass * vector3;
			bodyA.AngularVelocityInternal += bodyA.InvI * MathUtils.Cross(a, vector3);
		}
		if (_limitState2 == LimitState.AtUpper)
		{
			Vector2 value4 = bodyB.LinearVelocityInternal + MathUtils.Cross(bodyB.AngularVelocityInternal, a2);
			float num5 = 0f - Vector2.Dot(_u2, value4);
			float num6 = (0f - _limitMass2) * num5;
			float limitImpulse2 = _limitImpulse2;
			_limitImpulse2 = Math.Max(0f, _limitImpulse2 + num6);
			num6 = _limitImpulse2 - limitImpulse2;
			Vector2 vector4 = (0f - num6) * _u2;
			bodyB.LinearVelocityInternal += bodyB.InvMass * vector4;
			bodyB.AngularVelocityInternal += bodyB.InvI * MathUtils.Cross(a2, vector4);
		}
	}

	internal override bool SolvePositionConstraints()
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		Vector2 groundAnchorA = GroundAnchorA;
		Vector2 groundAnchorB = GroundAnchorB;
		float num = 0f;
		if (_state == LimitState.AtUpper)
		{
			bodyA.GetTransform(out var transform);
			bodyB.GetTransform(out var transform2);
			Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
			Vector2 vector2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - bodyB.LocalCenter);
			Vector2 vector3 = bodyA.Sweep.C + vector;
			Vector2 vector4 = bodyB.Sweep.C + vector2;
			_u1 = vector3 - groundAnchorA;
			_u2 = vector4 - groundAnchorB;
			float num2 = _u1.Length();
			float num3 = _u2.Length();
			if (num2 > 0.005f)
			{
				_u1 *= 1f / num2;
			}
			else
			{
				_u1 = Vector2.Zero;
			}
			if (num3 > 0.005f)
			{
				_u2 *= 1f / num3;
			}
			else
			{
				_u2 = Vector2.Zero;
			}
			float num4 = _ant - num2 - Ratio * num3;
			num = Math.Max(num, 0f - num4);
			num4 = MathUtils.Clamp(num4 + 0.005f, -0.2f, 0f);
			float num5 = (0f - _pulleyMass) * num4;
			Vector2 vector5 = (0f - num5) * _u1;
			Vector2 vector6 = (0f - Ratio) * num5 * _u2;
			bodyA.Sweep.C += bodyA.InvMass * vector5;
			bodyA.Sweep.A += bodyA.InvI * MathUtils.Cross(vector, vector5);
			bodyB.Sweep.C += bodyB.InvMass * vector6;
			bodyB.Sweep.A += bodyB.InvI * MathUtils.Cross(vector2, vector6);
			bodyA.SynchronizeTransform();
			bodyB.SynchronizeTransform();
		}
		if (_limitState1 == LimitState.AtUpper)
		{
			bodyA.GetTransform(out var transform3);
			Vector2 vector7 = MathUtils.Multiply(ref transform3.R, LocalAnchorA - bodyA.LocalCenter);
			Vector2 vector8 = bodyA.Sweep.C + vector7;
			_u1 = vector8 - groundAnchorA;
			float num6 = _u1.Length();
			if (num6 > 0.005f)
			{
				_u1 *= 1f / num6;
			}
			else
			{
				_u1 = Vector2.Zero;
			}
			float num7 = MaxLengthA - num6;
			num = Math.Max(num, 0f - num7);
			num7 = MathUtils.Clamp(num7 + 0.005f, -0.2f, 0f);
			float num8 = (0f - _limitMass1) * num7;
			Vector2 vector9 = (0f - num8) * _u1;
			bodyA.Sweep.C += bodyA.InvMass * vector9;
			bodyA.Sweep.A += bodyA.InvI * MathUtils.Cross(vector7, vector9);
			bodyA.SynchronizeTransform();
		}
		if (_limitState2 == LimitState.AtUpper)
		{
			bodyB.GetTransform(out var transform4);
			Vector2 vector10 = MathUtils.Multiply(ref transform4.R, LocalAnchorB - bodyB.LocalCenter);
			Vector2 vector11 = bodyB.Sweep.C + vector10;
			_u2 = vector11 - groundAnchorB;
			float num9 = _u2.Length();
			if (num9 > 0.005f)
			{
				_u2 *= 1f / num9;
			}
			else
			{
				_u2 = Vector2.Zero;
			}
			float num10 = MaxLengthB - num9;
			num = Math.Max(num, 0f - num10);
			num10 = MathUtils.Clamp(num10 + 0.005f, -0.2f, 0f);
			float num11 = (0f - _limitMass2) * num10;
			Vector2 vector12 = (0f - num11) * _u2;
			bodyB.Sweep.C += bodyB.InvMass * vector12;
			bodyB.Sweep.A += bodyB.InvI * MathUtils.Cross(vector10, vector12);
			bodyB.SynchronizeTransform();
		}
		return num < 0.005f;
	}
}
