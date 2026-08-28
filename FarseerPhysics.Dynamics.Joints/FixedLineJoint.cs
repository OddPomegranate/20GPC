using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class FixedLineJoint : Joint
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

	public override Vector2 WorldAnchorA => LocalAnchorA;

	public override Vector2 WorldAnchorB
	{
		get
		{
			return base.BodyA.GetWorldPoint(LocalAnchorB);
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

	internal FixedLineJoint()
	{
		base.JointType = JointType.FixedLine;
	}

	public FixedLineJoint(Body body, Vector2 worldAnchor, Vector2 axis)
		: base(body)
	{
		base.JointType = JointType.FixedLine;
		base.BodyB = base.BodyA;
		LocalAnchorA = worldAnchor;
		LocalAnchorB = base.BodyB.GetLocalPoint(worldAnchor);
		LocalXAxis = axis;
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
		Body bodyB = base.BodyB;
		LocalCenterA = Vector2.Zero;
		LocalCenterB = bodyB.LocalCenter;
		bodyB.GetTransform(out var transform);
		Vector2 localAnchorA = LocalAnchorA;
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorB - LocalCenterB);
		Vector2 vector2 = bodyB.Sweep.C + vector - localAnchorA;
		InvMassA = 0f;
		InvIA = 0f;
		InvMassB = bodyB.InvMass;
		InvIB = bodyB.InvI;
		_ay = _localYAxisA;
		_sAy = MathUtils.Cross(vector2 + localAnchorA, _ay);
		_sBy = MathUtils.Cross(vector, _ay);
		_mass = InvMassA + InvMassB + InvIA * _sAy * _sAy + InvIB * _sBy * _sBy;
		if (_mass > 0f)
		{
			_mass = 1f / _mass;
		}
		_springMass = 0f;
		if (Frequency > 0f)
		{
			_ax = LocalXAxis;
			_sAx = MathUtils.Cross(vector2 + localAnchorA, _ax);
			_sBx = MathUtils.Cross(vector, _ax);
			float num = InvMassA + InvMassB + InvIA * _sAx * _sAx + InvIB * _sBx * _sBx;
			if (num > 0f)
			{
				_springMass = 1f / num;
				float num2 = Vector2.Dot(vector2, _ax);
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
			Vector2 vector3 = _impulse * _ay + _springImpulse * _ax;
			float num6 = _impulse * _sBy + _springImpulse * _sBx + _motorImpulse;
			bodyB.LinearVelocityInternal += InvMassB * vector3;
			bodyB.AngularVelocityInternal += InvIB * num6;
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
		Body bodyB = base.BodyB;
		Vector2 zero = Vector2.Zero;
		float num = 0f;
		Vector2 linearVelocityInternal = bodyB.LinearVelocityInternal;
		float angularVelocityInternal = bodyB.AngularVelocityInternal;
		float num2 = Vector2.Dot(_ax, linearVelocityInternal - zero) + _sBx * angularVelocityInternal - _sAx * num;
		float num3 = (0f - _springMass) * (num2 + _bias + _gamma * _springImpulse);
		_springImpulse += num3;
		Vector2 vector = num3 * _ax;
		float num4 = num3 * _sAx;
		float num5 = num3 * _sBx;
		zero -= InvMassA * vector;
		num -= InvIA * num4;
		linearVelocityInternal += InvMassB * vector;
		angularVelocityInternal += InvIB * num5;
		float num6 = angularVelocityInternal - num - _motorSpeed;
		float num7 = (0f - _motorMass) * num6;
		float motorImpulse = _motorImpulse;
		float num8 = step.dt * _maxMotorTorque;
		_motorImpulse = MathUtils.Clamp(_motorImpulse + num7, 0f - num8, num8);
		num7 = _motorImpulse - motorImpulse;
		num -= InvIA * num7;
		angularVelocityInternal += InvIB * num7;
		float num9 = Vector2.Dot(_ay, linearVelocityInternal - zero) + _sBy * angularVelocityInternal - _sAy * num;
		float num10 = _mass * (0f - num9);
		_impulse += num10;
		Vector2 vector2 = num10 * _ay;
		float num11 = num10 * _sBy;
		linearVelocityInternal += InvMassB * vector2;
		angularVelocityInternal += InvIB * num11;
		bodyB.LinearVelocityInternal = linearVelocityInternal;
		bodyB.AngularVelocityInternal = angularVelocityInternal;
	}

	internal override bool SolvePositionConstraints()
	{
		Body bodyB = base.BodyB;
		Vector2 zero = Vector2.Zero;
		Vector2 c = bodyB.Sweep.C;
		float a = bodyB.Sweep.A;
		Mat22 A = new Mat22(0f);
		Mat22 A2 = new Mat22(a);
		Vector2 vector = MathUtils.Multiply(ref A, LocalAnchorA - LocalCenterA);
		Vector2 vector2 = MathUtils.Multiply(ref A2, LocalAnchorB - LocalCenterB);
		Vector2 value = c + vector2 - zero - vector;
		Vector2 vector3 = MathUtils.Multiply(ref A, _localYAxisA);
		float num = MathUtils.Cross(vector2, vector3);
		float num2 = Vector2.Dot(value, vector3);
		float num3 = InvMassA + InvMassB + InvIA * _sAy * _sAy + InvIB * _sBy * _sBy;
		float num4 = ((num3 == 0f) ? 0f : ((0f - num2) / num3));
		Vector2 vector4 = num4 * vector3;
		float num5 = num4 * num;
		c += InvMassB * vector4;
		a += InvIB * num5;
		bodyB.Sweep.C = c;
		bodyB.Sweep.A = a;
		bodyB.SynchronizeTransform();
		return Math.Abs(num2) <= 0.005f;
	}

	public float GetMotorTorque(float invDt)
	{
		return invDt * _motorImpulse;
	}
}
