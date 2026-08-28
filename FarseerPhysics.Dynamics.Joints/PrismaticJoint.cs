using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class PrismaticJoint : Joint
{
	public Vector2 LocalAnchorA;

	public Vector2 LocalAnchorB;

	private Mat33 _K;

	private float _a1;

	private float _a2;

	private Vector2 _axis;

	private bool _enableLimit;

	private bool _enableMotor;

	private Vector3 _impulse;

	private LimitState _limitState;

	private Vector2 _localXAxis1;

	private Vector2 _localYAxis1;

	private float _lowerTranslation;

	private float _maxMotorForce;

	private float _motorImpulse;

	private float _motorMass;

	private float _motorSpeed;

	private Vector2 _perp;

	private float _refAngle;

	private float _s1;

	private float _s2;

	private float _upperTranslation;

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
			Vector2 value = base.BodyB.GetWorldPoint(LocalAnchorB) - base.BodyA.GetWorldPoint(LocalAnchorA);
			Vector2 worldVector = base.BodyA.GetWorldVector(ref _localXAxis1);
			return Vector2.Dot(value, worldVector);
		}
	}

	public float JointSpeed
	{
		get
		{
			base.BodyA.GetTransform(out var transform);
			base.BodyB.GetTransform(out var transform2);
			Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - base.BodyA.LocalCenter);
			Vector2 vector2 = MathUtils.Multiply(ref transform2.R, LocalAnchorB - base.BodyB.LocalCenter);
			Vector2 vector3 = base.BodyA.Sweep.C + vector;
			Vector2 vector4 = base.BodyB.Sweep.C + vector2;
			Vector2 value = vector4 - vector3;
			Vector2 worldVector = base.BodyA.GetWorldVector(ref _localXAxis1);
			Vector2 linearVelocityInternal = base.BodyA.LinearVelocityInternal;
			Vector2 linearVelocityInternal2 = base.BodyB.LinearVelocityInternal;
			float angularVelocityInternal = base.BodyA.AngularVelocityInternal;
			float angularVelocityInternal2 = base.BodyB.AngularVelocityInternal;
			return Vector2.Dot(value, MathUtils.Cross(angularVelocityInternal, worldVector)) + Vector2.Dot(worldVector, linearVelocityInternal2 + MathUtils.Cross(angularVelocityInternal2, vector2) - linearVelocityInternal - MathUtils.Cross(angularVelocityInternal, vector));
		}
	}

	public bool LimitEnabled
	{
		get
		{
			return _enableLimit;
		}
		set
		{
			WakeBodies();
			_enableLimit = value;
		}
	}

	public float LowerLimit
	{
		get
		{
			return _lowerTranslation;
		}
		set
		{
			WakeBodies();
			_lowerTranslation = value;
		}
	}

	public float UpperLimit
	{
		get
		{
			return _upperTranslation;
		}
		set
		{
			WakeBodies();
			_upperTranslation = value;
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
			WakeBodies();
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
			WakeBodies();
			_motorSpeed = value;
		}
	}

	public float MaxMotorForce
	{
		get
		{
			return _maxMotorForce;
		}
		set
		{
			WakeBodies();
			_maxMotorForce = value;
		}
	}

	public float MotorForce
	{
		get
		{
			return _motorImpulse;
		}
		set
		{
			_motorImpulse = value;
		}
	}

	public Vector2 LocalXAxis1
	{
		get
		{
			return _localXAxis1;
		}
		set
		{
			_localXAxis1 = base.BodyA.GetLocalVector(value);
			_localYAxis1 = MathUtils.Cross(1f, _localXAxis1);
		}
	}

	public float ReferenceAngle
	{
		get
		{
			return _refAngle;
		}
		set
		{
			_refAngle = value;
		}
	}

	internal PrismaticJoint()
	{
		base.JointType = JointType.Prismatic;
	}

	public PrismaticJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB, Vector2 axis)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Prismatic;
		LocalAnchorA = localAnchorA;
		LocalAnchorB = localAnchorB;
		_localXAxis1 = base.BodyA.GetLocalVector(axis);
		_localYAxis1 = MathUtils.Cross(1f, _localXAxis1);
		_refAngle = base.BodyB.Rotation - base.BodyA.Rotation;
		_limitState = LimitState.Inactive;
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		return inv_dt * (_impulse.X * _perp + (_motorImpulse + _impulse.Z) * _axis);
	}

	public override float GetReactionTorque(float inv_dt)
	{
		return inv_dt * _impulse.Y;
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
		_axis = MathUtils.Multiply(ref transform.R, _localXAxis1);
		_a1 = MathUtils.Cross(vector3 + vector, _axis);
		_a2 = MathUtils.Cross(vector2, _axis);
		_motorMass = InvMassA + InvMassB + InvIA * _a1 * _a1 + InvIB * _a2 * _a2;
		if (_motorMass > 1.1920929E-07f)
		{
			_motorMass = 1f / _motorMass;
		}
		_perp = MathUtils.Multiply(ref transform.R, _localYAxis1);
		_s1 = MathUtils.Cross(vector3 + vector, _perp);
		_s2 = MathUtils.Cross(vector2, _perp);
		float invMassA = InvMassA;
		float invMassB = InvMassB;
		float invIA = InvIA;
		float invIB = InvIB;
		float x = invMassA + invMassB + invIA * _s1 * _s1 + invIB * _s2 * _s2;
		float num = invIA * _s1 + invIB * _s2;
		float num2 = invIA * _s1 * _a1 + invIB * _s2 * _a2;
		float y = invIA + invIB;
		float num3 = invIA * _a1 + invIB * _a2;
		float z = invMassA + invMassB + invIA * _a1 * _a1 + invIB * _a2 * _a2;
		_K.Col1 = new Vector3(x, num, num2);
		_K.Col2 = new Vector3(num, y, num3);
		_K.Col3 = new Vector3(num2, num3, z);
		if (_enableLimit)
		{
			float num4 = Vector2.Dot(_axis, vector3);
			if (Math.Abs(_upperTranslation - _lowerTranslation) < 0.01f)
			{
				_limitState = LimitState.Equal;
			}
			else if (num4 <= _lowerTranslation)
			{
				if (_limitState != LimitState.AtLower)
				{
					_limitState = LimitState.AtLower;
					_impulse.Z = 0f;
				}
			}
			else if (num4 >= _upperTranslation)
			{
				if (_limitState != LimitState.AtUpper)
				{
					_limitState = LimitState.AtUpper;
					_impulse.Z = 0f;
				}
			}
			else
			{
				_limitState = LimitState.Inactive;
				_impulse.Z = 0f;
			}
		}
		else
		{
			_limitState = LimitState.Inactive;
		}
		if (!_enableMotor)
		{
			_motorImpulse = 0f;
		}
		if (Settings.EnableWarmstarting)
		{
			_impulse *= step.dtRatio;
			_motorImpulse *= step.dtRatio;
			Vector2 vector4 = _impulse.X * _perp + (_motorImpulse + _impulse.Z) * _axis;
			float num5 = _impulse.X * _s1 + _impulse.Y + (_motorImpulse + _impulse.Z) * _a1;
			float num6 = _impulse.X * _s2 + _impulse.Y + (_motorImpulse + _impulse.Z) * _a2;
			bodyA.LinearVelocityInternal -= InvMassA * vector4;
			bodyA.AngularVelocityInternal -= InvIA * num5;
			bodyB.LinearVelocityInternal += InvMassB * vector4;
			bodyB.AngularVelocityInternal += InvIB * num6;
		}
		else
		{
			_impulse = Vector3.Zero;
			_motorImpulse = 0f;
		}
	}

	internal override void SolveVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		Vector2 linearVelocityInternal = bodyA.LinearVelocityInternal;
		float num = bodyA.AngularVelocityInternal;
		Vector2 linearVelocityInternal2 = bodyB.LinearVelocityInternal;
		float num2 = bodyB.AngularVelocityInternal;
		if (_enableMotor && _limitState != LimitState.Equal)
		{
			float num3 = Vector2.Dot(_axis, linearVelocityInternal2 - linearVelocityInternal) + _a2 * num2 - _a1 * num;
			float num4 = _motorMass * (_motorSpeed - num3);
			float motorImpulse = _motorImpulse;
			float num5 = step.dt * _maxMotorForce;
			_motorImpulse = MathUtils.Clamp(_motorImpulse + num4, 0f - num5, num5);
			num4 = _motorImpulse - motorImpulse;
			Vector2 vector = num4 * _axis;
			float num6 = num4 * _a1;
			float num7 = num4 * _a2;
			linearVelocityInternal -= InvMassA * vector;
			num -= InvIA * num6;
			linearVelocityInternal2 += InvMassB * vector;
			num2 += InvIB * num7;
		}
		Vector2 vector2 = new Vector2(Vector2.Dot(_perp, linearVelocityInternal2 - linearVelocityInternal) + _s2 * num2 - _s1 * num, num2 - num);
		if (_enableLimit && _limitState != LimitState.Inactive)
		{
			float z = Vector2.Dot(_axis, linearVelocityInternal2 - linearVelocityInternal) + _a2 * num2 - _a1 * num;
			Vector3 vector3 = new Vector3(vector2.X, vector2.Y, z);
			Vector3 impulse = _impulse;
			Vector3 vector4 = _K.Solve33(-vector3);
			_impulse += vector4;
			if (_limitState == LimitState.AtLower)
			{
				_impulse.Z = Math.Max(_impulse.Z, 0f);
			}
			else if (_limitState == LimitState.AtUpper)
			{
				_impulse.Z = Math.Min(_impulse.Z, 0f);
			}
			Vector2 b = -vector2 - (_impulse.Z - impulse.Z) * new Vector2(_K.Col3.X, _K.Col3.Y);
			Vector2 vector5 = _K.Solve22(b) + new Vector2(impulse.X, impulse.Y);
			_impulse.X = vector5.X;
			_impulse.Y = vector5.Y;
			vector4 = _impulse - impulse;
			Vector2 vector6 = vector4.X * _perp + vector4.Z * _axis;
			float num8 = vector4.X * _s1 + vector4.Y + vector4.Z * _a1;
			float num9 = vector4.X * _s2 + vector4.Y + vector4.Z * _a2;
			linearVelocityInternal -= InvMassA * vector6;
			num -= InvIA * num8;
			linearVelocityInternal2 += InvMassB * vector6;
			num2 += InvIB * num9;
		}
		else
		{
			Vector2 vector7 = _K.Solve22(-vector2);
			_impulse.X += vector7.X;
			_impulse.Y += vector7.Y;
			Vector2 vector8 = vector7.X * _perp;
			float num10 = vector7.X * _s1 + vector7.Y;
			float num11 = vector7.X * _s2 + vector7.Y;
			linearVelocityInternal -= InvMassA * vector8;
			num -= InvIA * num10;
			linearVelocityInternal2 += InvMassB * vector8;
			num2 += InvIB * num11;
		}
		bodyA.LinearVelocityInternal = linearVelocityInternal;
		bodyA.AngularVelocityInternal = num;
		bodyB.LinearVelocityInternal = linearVelocityInternal2;
		bodyB.AngularVelocityInternal = num2;
	}

	internal override bool SolvePositionConstraints()
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		Vector2 c = bodyA.Sweep.C;
		float a = bodyA.Sweep.A;
		Vector2 c2 = bodyB.Sweep.C;
		float a2 = bodyB.Sweep.A;
		float val = 0f;
		bool flag = false;
		float num = 0f;
		Mat22 A = new Mat22(a);
		Mat22 A2 = new Mat22(a2);
		Vector2 vector = MathUtils.Multiply(ref A, LocalAnchorA - LocalCenterA);
		Vector2 vector2 = MathUtils.Multiply(ref A2, LocalAnchorB - LocalCenterB);
		Vector2 vector3 = c2 + vector2 - c - vector;
		if (_enableLimit)
		{
			_axis = MathUtils.Multiply(ref A, _localXAxis1);
			_a1 = MathUtils.Cross(vector3 + vector, _axis);
			_a2 = MathUtils.Cross(vector2, _axis);
			float num2 = Vector2.Dot(_axis, vector3);
			if (Math.Abs(_upperTranslation - _lowerTranslation) < 0.01f)
			{
				num = MathUtils.Clamp(num2, -0.2f, 0.2f);
				val = Math.Abs(num2);
				flag = true;
			}
			else if (num2 <= _lowerTranslation)
			{
				num = MathUtils.Clamp(num2 - _lowerTranslation + 0.005f, -0.2f, 0f);
				val = _lowerTranslation - num2;
				flag = true;
			}
			else if (num2 >= _upperTranslation)
			{
				num = MathUtils.Clamp(num2 - _upperTranslation - 0.005f, 0f, 0.2f);
				val = num2 - _upperTranslation;
				flag = true;
			}
		}
		_perp = MathUtils.Multiply(ref A, _localYAxis1);
		_s1 = MathUtils.Cross(vector3 + vector, _perp);
		_s2 = MathUtils.Cross(vector2, _perp);
		Vector2 vector4 = new Vector2(Vector2.Dot(_perp, vector3), a2 - a - ReferenceAngle);
		val = Math.Max(val, Math.Abs(vector4.X));
		float num3 = Math.Abs(vector4.Y);
		Vector3 vector5 = default(Vector3);
		if (flag)
		{
			float invMassA = InvMassA;
			float invMassB = InvMassB;
			float invIA = InvIA;
			float invIB = InvIB;
			float x = invMassA + invMassB + invIA * _s1 * _s1 + invIB * _s2 * _s2;
			float num4 = invIA * _s1 + invIB * _s2;
			float num5 = invIA * _s1 * _a1 + invIB * _s2 * _a2;
			float y = invIA + invIB;
			float num6 = invIA * _a1 + invIB * _a2;
			float z = invMassA + invMassB + invIA * _a1 * _a1 + invIB * _a2 * _a2;
			_K.Col1 = new Vector3(x, num4, num5);
			_K.Col2 = new Vector3(num4, y, num6);
			_K.Col3 = new Vector3(num5, num6, z);
			Vector3 b = new Vector3(0f - vector4.X, 0f - vector4.Y, 0f - num);
			vector5 = _K.Solve33(b);
		}
		else
		{
			float invMassA2 = InvMassA;
			float invMassB2 = InvMassB;
			float invIA2 = InvIA;
			float invIB2 = InvIB;
			float x2 = invMassA2 + invMassB2 + invIA2 * _s1 * _s1 + invIB2 * _s2 * _s2;
			float num7 = invIA2 * _s1 + invIB2 * _s2;
			float y2 = invIA2 + invIB2;
			_K.Col1 = new Vector3(x2, num7, 0f);
			_K.Col2 = new Vector3(num7, y2, 0f);
			Vector2 vector6 = _K.Solve22(-vector4);
			vector5.X = vector6.X;
			vector5.Y = vector6.Y;
			vector5.Z = 0f;
		}
		Vector2 vector7 = vector5.X * _perp + vector5.Z * _axis;
		float num8 = vector5.X * _s1 + vector5.Y + vector5.Z * _a1;
		float num9 = vector5.X * _s2 + vector5.Y + vector5.Z * _a2;
		c -= InvMassA * vector7;
		a -= InvIA * num8;
		c2 += InvMassB * vector7;
		a2 += InvIB * num9;
		bodyA.Sweep.C = c;
		bodyA.Sweep.A = a;
		bodyB.Sweep.C = c2;
		bodyB.Sweep.A = a2;
		bodyA.SynchronizeTransform();
		bodyB.SynchronizeTransform();
		if (val <= 0.005f)
		{
			return num3 <= (float)Math.PI / 90f;
		}
		return false;
	}
}
