using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class LineJoint : Joint
{
	private Vector2 _ax;

	private Vector2 _ay;

	private float _bias;

	private bool _enableMotor;

	private float _gamma;

	private float _impulse;

	private Vector2 _localXAxis;

	private Vector2 _localYAxisA;

	private float _mass;

	private float _maxMotorTorque;

	private float _motorImpulse;

	private float _motorMass;

	private float _motorSpeed;

	private float _sAx;

	private float _sAy;

	private float _sBx;

	private float _sBy;

	private float _springImpulse;

	private float _springMass;

	public Vector2 LocalAnchorA { get; set; }

	public Vector2 LocalAnchorB { get; set; }

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

	public float JointTranslation
	{
		get
		{
			Body bodyA = base.BodyA;
			Body bodyB = base.BodyB;
			Vector2 worldPoint = bodyA.GetWorldPoint(LocalAnchorA);
			Vector2 worldPoint2 = bodyB.GetWorldPoint(LocalAnchorB);
			Vector2 value = worldPoint2 - worldPoint;
			Vector2 worldVector = bodyA.GetWorldVector(LocalXAxis);
			return Vector2.Dot(value, worldVector);
		}
	}

	public float JointSpeed
	{
		get
		{
			float angularVelocityInternal = base.BodyA.AngularVelocityInternal;
			float angularVelocityInternal2 = base.BodyB.AngularVelocityInternal;
			return angularVelocityInternal2 - angularVelocityInternal;
		}
	}

	public bool MotorEnabled
	{
		get
		{
			return _enableMotor;
		}
		set
		{
			base.BodyA.Awake = true;
			base.BodyB.Awake = true;
			_enableMotor = value;
		}
	}

	public float MotorSpeed
	{
		get
		{
			return _motorSpeed;
		}
		set
		{
			base.BodyA.Awake = true;
			base.BodyB.Awake = true;
			_motorSpeed = value;
		}
	}

	public float MaxMotorTorque
	{
		get
		{
			return _maxMotorTorque;
		}
		set
		{
			base.BodyA.Awake = true;
			base.BodyB.Awake = true;
			_maxMotorTorque = value;
		}
	}

	public float Frequency { get; set; }

	public float DampingRatio { get; set; }

	public Vector2 LocalXAxis
	{
		get
		{
			return _localXAxis;
		}
		set
		{
			_localXAxis = value;
			_localYAxisA = MathUtils.Cross(1f, _localXAxis);
		}
	}

	internal LineJoint()
	{
		base.JointType = JointType.Line;
	}

	public LineJoint(Body bA, Body bB, Vector2 anchor, Vector2 axis)
		: base(bA, bB)
	{
		base.JointType = JointType.Line;
		LocalAnchorA = bA.GetLocalPoint(anchor);
		LocalAnchorB = bB.GetLocalPoint(anchor);
		LocalXAxis = bA.GetLocalVector(axis);
	}

	public override Vector2 GetReactionForce(float invDt)
	{
		return invDt * (_impulse * _ay + _springImpulse * _ax);
	}

	public override float GetReactionTorque(float invDt)
	{
		return invDt * _motorImpulse;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		LocalCenterA = bodyA.LocalCenter;
		LocalCenterB = bodyB.LocalCenter;
		bodyA.GetTransform(out var transform);
		bodyB.GetTransform(out var transform2);
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - LocalCenterA);
		Vector2 vector2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - LocalCenterB);
		Vector2 vector3 = bodyB.Sweep.C + vector2 - bodyA.Sweep.C - vector;
		InvMassA = bodyA.InvMass;
		InvIA = bodyA.InvI;
		InvMassB = bodyB.InvMass;
		InvIB = bodyB.InvI;
		_ay = MathUtils.Multiply(ref transform.R, _localYAxisA);
		_sAy = MathUtils.Cross(vector3 + vector, _ay);
		_sBy = MathUtils.Cross(vector2, _ay);
		_mass = InvMassA + InvMassB + InvIA * _sAy * _sAy + InvIB * _sBy * _sBy;
		if (_mass > 0f)
		{
			_mass = 1f / _mass;
		}
		_springMass = 0f;
		if (Frequency > 0f)
		{
			_ax = MathUtils.Multiply(ref transform.R, LocalXAxis);
			_sAx = MathUtils.Cross(vector3 + vector, _ax);
			_sBx = MathUtils.Cross(vector2, _ax);
			float num = InvMassA + InvMassB + InvIA * _sAx * _sAx + InvIB * _sBx * _sBx;
			if (num > 0f)
			{
				_springMass = 1f / num;
				float num2 = Vector2.Dot(vector3, _ax);
				float num3 = (float)Math.PI * 2f * Frequency;
				float num4 = 2f * _springMass * DampingRatio * num3;
				float num5 = _springMass * num3 * num3;
				_gamma = step.dt * (num4 + step.dt * num5);
				if (_gamma > 0f)
				{
					_gamma = 1f / _gamma;
				}
				_bias = num2 * step.dt * num5 * _gamma;
				_springMass = num + _gamma;
				if (_springMass > 0f)
				{
					_springMass = 1f / _springMass;
				}
			}
		}
		else
		{
			_springImpulse = 0f;
			_springMass = 0f;
		}
		if (_enableMotor)
		{
			_motorMass = InvIA + InvIB;
			if (_motorMass > 0f)
			{
				_motorMass = 1f / _motorMass;
			}
		}
		else
		{
			_motorMass = 0f;
			_motorImpulse = 0f;
		}
		if (Settings.EnableWarmstarting)
		{
			_impulse *= step.dtRatio;
			_springImpulse *= step.dtRatio;
			_motorImpulse *= step.dtRatio;
			Vector2 vector4 = _impulse * _ay + _springImpulse * _ax;
			float num6 = _impulse * _sAy + _springImpulse * _sAx + _motorImpulse;
			float num7 = _impulse * _sBy + _springImpulse * _sBx + _motorImpulse;
			bodyA.LinearVelocityInternal -= InvMassA * vector4;
			bodyA.AngularVelocityInternal -= InvIA * num6;
			bodyB.LinearVelocityInternal += InvMassB * vector4;
			bodyB.AngularVelocityInternal += InvIB * num7;
		}
		else
		{
			_impulse = 0f;
			_springImpulse = 0f;
			_motorImpulse = 0f;
		}
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		Vector2 linearVelocity = bodyA.LinearVelocity;
		float angularVelocityInternal = bodyA.AngularVelocityInternal;
		Vector2 linearVelocityInternal = bodyB.LinearVelocityInternal;
		float angularVelocityInternal2 = bodyB.AngularVelocityInternal;
		float num = Vector2.Dot(_ax, linearVelocityInternal - linearVelocity) + _sBx * angularVelocityInternal2 - _sAx * angularVelocityInternal;
		float num2 = (0f - _springMass) * (num + _bias + _gamma * _springImpulse);
		_springImpulse += num2;
		Vector2 vector = num2 * _ax;
		float num3 = num2 * _sAx;
		float num4 = num2 * _sBx;
		linearVelocity -= InvMassA * vector;
		angularVelocityInternal -= InvIA * num3;
		linearVelocityInternal += InvMassB * vector;
		angularVelocityInternal2 += InvIB * num4;
		float num5 = angularVelocityInternal2 - angularVelocityInternal - _motorSpeed;
		float num6 = (0f - _motorMass) * num5;
		float motorImpulse = _motorImpulse;
		float num7 = step.dt * _maxMotorTorque;
		_motorImpulse = MathUtils.Clamp(_motorImpulse + num6, 0f - num7, num7);
		num6 = _motorImpulse - motorImpulse;
		angularVelocityInternal -= InvIA * num6;
		angularVelocityInternal2 += InvIB * num6;
		float num8 = Vector2.Dot(_ay, linearVelocityInternal - linearVelocity) + _sBy * angularVelocityInternal2 - _sAy * angularVelocityInternal;
		float num9 = _mass * (0f - num8);
		_impulse += num9;
		Vector2 vector2 = num9 * _ay;
		float num10 = num9 * _sAy;
		float num11 = num9 * _sBy;
		linearVelocity -= InvMassA * vector2;
		angularVelocityInternal -= InvIA * num10;
		linearVelocityInternal += InvMassB * vector2;
		angularVelocityInternal2 += InvIB * num11;
		bodyA.LinearVelocityInternal = linearVelocity;
		bodyA.AngularVelocityInternal = angularVelocityInternal;
		bodyB.LinearVelocityInternal = linearVelocityInternal;
		bodyB.AngularVelocityInternal = angularVelocityInternal2;
	}

	internal override bool SolvePositionConstraints()
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		Vector2 c = bodyA.Sweep.C;
		float a = bodyA.Sweep.A;
		Vector2 c2 = bodyB.Sweep.C;
		float a2 = bodyB.Sweep.A;
		Mat22 A = new Mat22(a);
		Mat22 A2 = new Mat22(a2);
		Vector2 vector = MathUtils.Multiply(ref A, LocalAnchorA - LocalCenterA);
		Vector2 vector2 = MathUtils.Multiply(ref A2, LocalAnchorB - LocalCenterB);
		Vector2 vector3 = c2 + vector2 - c - vector;
		Vector2 vector4 = MathUtils.Multiply(ref A, _localYAxisA);
		float num = MathUtils.Cross(vector3 + vector, vector4);
		float num2 = MathUtils.Cross(vector2, vector4);
		float num3 = Vector2.Dot(vector3, vector4);
		float num4 = InvMassA + InvMassB + InvIA * _sAy * _sAy + InvIB * _sBy * _sBy;
		float num5 = ((num4 == 0f) ? 0f : ((0f - num3) / num4));
		Vector2 vector5 = num5 * vector4;
		float num6 = num5 * num;
		float num7 = num5 * num2;
		c -= InvMassA * vector5;
		a -= InvIA * num6;
		c2 += InvMassB * vector5;
		a2 += InvIB * num7;
		bodyA.Sweep.C = c;
		bodyA.Sweep.A = a;
		bodyB.Sweep.C = c2;
		bodyB.Sweep.A = a2;
		bodyA.SynchronizeTransform();
		bodyB.SynchronizeTransform();
		return Math.Abs(num3) <= 0.005f;
	}

	public float GetMotorTorque(float invDt)
	{
		return invDt * _motorImpulse;
	}
}
