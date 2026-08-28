using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class RevoluteJoint : Joint
{
	public Vector2 LocalAnchorA;

	public Vector2 LocalAnchorB;

	private bool _enableLimit;

	private bool _enableMotor;

	private Vector3 _impulse;

	private LimitState _limitState;

	private float _lowerAngle;

	private Mat33 _mass;

	private float _maxMotorTorque;

	private float _motorImpulse;

	private float _motorMass;

	private float _motorSpeed;

	private float _referenceAngle;

	private float _tmpFloat1;

	private Vector2 _tmpVector1;

	private Vector2 _tmpVector2;

	private float _upperAngle;

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

	public float ReferenceAngle
	{
		get
		{
			return _referenceAngle;
		}
		set
		{
			WakeBodies();
			_referenceAngle = value;
		}
	}

	public float JointAngle => base.BodyB.Sweep.A - base.BodyA.Sweep.A - ReferenceAngle;

	public float JointSpeed => base.BodyB.AngularVelocityInternal - base.BodyA.AngularVelocityInternal;

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
			return _lowerAngle;
		}
		set
		{
			WakeBodies();
			_lowerAngle = value;
		}
	}

	public float UpperLimit
	{
		get
		{
			return _upperAngle;
		}
		set
		{
			WakeBodies();
			_upperAngle = value;
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

	public float MaxMotorTorque
	{
		get
		{
			return _maxMotorTorque;
		}
		set
		{
			WakeBodies();
			_maxMotorTorque = value;
		}
	}

	public float MotorTorque
	{
		get
		{
			return _motorImpulse;
		}
		set
		{
			WakeBodies();
			_motorImpulse = value;
		}
	}

	internal RevoluteJoint()
	{
		base.JointType = JointType.Revolute;
	}

	public RevoluteJoint(Body bodyA, Body bodyB, Vector2 localAnchorA, Vector2 localAnchorB)
		: base(bodyA, bodyB)
	{
		base.JointType = JointType.Revolute;
		LocalAnchorA = localAnchorA;
		LocalAnchorB = localAnchorB;
		ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
		_impulse = Vector3.Zero;
		_limitState = LimitState.Inactive;
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		Vector2 vector = new Vector2(_impulse.X, _impulse.Y);
		return inv_dt * vector;
	}

	public override float GetReactionTorque(float inv_dt)
	{
		return inv_dt * _impulse.Z;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		if (!_enableMotor)
		{
			_ = _enableLimit;
		}
		Vector2 a = MathUtils.Multiply(ref bodyA.Xf.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 a2 = MathUtils.Multiply(ref bodyB.Xf.R, LocalAnchorB - bodyB.LocalCenter);
		float invMass = bodyA.InvMass;
		float invMass2 = bodyB.InvMass;
		float invI = bodyA.InvI;
		float invI2 = bodyB.InvI;
		_mass.Col1.X = invMass + invMass2 + a.Y * a.Y * invI + a2.Y * a2.Y * invI2;
		_mass.Col2.X = (0f - a.Y) * a.X * invI - a2.Y * a2.X * invI2;
		_mass.Col3.X = (0f - a.Y) * invI - a2.Y * invI2;
		_mass.Col1.Y = _mass.Col2.X;
		_mass.Col2.Y = invMass + invMass2 + a.X * a.X * invI + a2.X * a2.X * invI2;
		_mass.Col3.Y = a.X * invI + a2.X * invI2;
		_mass.Col1.Z = _mass.Col3.X;
		_mass.Col2.Z = _mass.Col3.Y;
		_mass.Col3.Z = invI + invI2;
		_motorMass = invI + invI2;
		if (_motorMass > 0f)
		{
			_motorMass = 1f / _motorMass;
		}
		if (!_enableMotor)
		{
			_motorImpulse = 0f;
		}
		if (_enableLimit)
		{
			float num = bodyB.Sweep.A - bodyA.Sweep.A - ReferenceAngle;
			if (Math.Abs(_upperAngle - _lowerAngle) < (float)Math.PI / 45f)
			{
				_limitState = LimitState.Equal;
			}
			else if (num <= _lowerAngle)
			{
				if (_limitState != LimitState.AtLower)
				{
					_impulse.Z = 0f;
				}
				_limitState = LimitState.AtLower;
			}
			else if (num >= _upperAngle)
			{
				if (_limitState != LimitState.AtUpper)
				{
					_impulse.Z = 0f;
				}
				_limitState = LimitState.AtUpper;
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
		if (Settings.EnableWarmstarting)
		{
			_impulse *= step.dtRatio;
			_motorImpulse *= step.dtRatio;
			Vector2 b = new Vector2(_impulse.X, _impulse.Y);
			bodyA.LinearVelocityInternal -= invMass * b;
			MathUtils.Cross(ref a, ref b, out _tmpFloat1);
			bodyA.AngularVelocityInternal -= invI * (_tmpFloat1 + _motorImpulse + _impulse.Z);
			bodyB.LinearVelocityInternal += invMass2 * b;
			MathUtils.Cross(ref a2, ref b, out _tmpFloat1);
			bodyB.AngularVelocityInternal += invI2 * (_tmpFloat1 + _motorImpulse + _impulse.Z);
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
		float invMass = bodyA.InvMass;
		float invMass2 = bodyB.InvMass;
		float invI = bodyA.InvI;
		float invI2 = bodyB.InvI;
		if (_enableMotor && _limitState != LimitState.Equal)
		{
			float num3 = num2 - num - _motorSpeed;
			float num4 = _motorMass * (0f - num3);
			float motorImpulse = _motorImpulse;
			float num5 = step.dt * _maxMotorTorque;
			_motorImpulse = MathUtils.Clamp(_motorImpulse + num4, 0f - num5, num5);
			num4 = _motorImpulse - motorImpulse;
			num -= invI * num4;
			num2 += invI2 * num4;
		}
		if (_enableLimit && _limitState != LimitState.Inactive)
		{
			Vector2 a = MathUtils.Multiply(ref bodyA.Xf.R, LocalAnchorA - bodyA.LocalCenter);
			Vector2 a2 = MathUtils.Multiply(ref bodyB.Xf.R, LocalAnchorB - bodyB.LocalCenter);
			MathUtils.Cross(num2, ref a2, out _tmpVector2);
			MathUtils.Cross(num, ref a, out _tmpVector1);
			Vector2 vector = linearVelocityInternal2 + _tmpVector2 - linearVelocityInternal - _tmpVector1;
			float z = num2 - num;
			Vector3 vector2 = new Vector3(vector.X, vector.Y, z);
			Vector3 vector3 = _mass.Solve33(-vector2);
			if (_limitState == LimitState.Equal)
			{
				_impulse += vector3;
			}
			else if (_limitState == LimitState.AtLower)
			{
				float num6 = _impulse.Z + vector3.Z;
				if (num6 < 0f)
				{
					Vector2 vector4 = _mass.Solve22(-vector);
					vector3.X = vector4.X;
					vector3.Y = vector4.Y;
					vector3.Z = 0f - _impulse.Z;
					_impulse.X += vector4.X;
					_impulse.Y += vector4.Y;
					_impulse.Z = 0f;
				}
			}
			else if (_limitState == LimitState.AtUpper)
			{
				float num7 = _impulse.Z + vector3.Z;
				if (num7 > 0f)
				{
					Vector2 vector5 = _mass.Solve22(-vector);
					vector3.X = vector5.X;
					vector3.Y = vector5.Y;
					vector3.Z = 0f - _impulse.Z;
					_impulse.X += vector5.X;
					_impulse.Y += vector5.Y;
					_impulse.Z = 0f;
				}
			}
			Vector2 b = new Vector2(vector3.X, vector3.Y);
			linearVelocityInternal -= invMass * b;
			MathUtils.Cross(ref a, ref b, out _tmpFloat1);
			num -= invI * (_tmpFloat1 + vector3.Z);
			linearVelocityInternal2 += invMass2 * b;
			MathUtils.Cross(ref a2, ref b, out _tmpFloat1);
			num2 += invI2 * (_tmpFloat1 + vector3.Z);
		}
		else
		{
			_tmpVector1 = LocalAnchorA - bodyA.LocalCenter;
			_tmpVector2 = LocalAnchorB - bodyB.LocalCenter;
			Vector2 a3 = MathUtils.Multiply(ref bodyA.Xf.R, ref _tmpVector1);
			Vector2 a4 = MathUtils.Multiply(ref bodyB.Xf.R, ref _tmpVector2);
			MathUtils.Cross(num2, ref a4, out _tmpVector2);
			MathUtils.Cross(num, ref a3, out _tmpVector1);
			Vector2 vector6 = linearVelocityInternal2 + _tmpVector2 - linearVelocityInternal - _tmpVector1;
			Vector2 b2 = _mass.Solve22(-vector6);
			_impulse.X += b2.X;
			_impulse.Y += b2.Y;
			linearVelocityInternal -= invMass * b2;
			MathUtils.Cross(ref a3, ref b2, out _tmpFloat1);
			num -= invI * _tmpFloat1;
			linearVelocityInternal2 += invMass2 * b2;
			MathUtils.Cross(ref a4, ref b2, out _tmpFloat1);
			num2 += invI2 * _tmpFloat1;
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
		float num = 0f;
		if (_enableLimit && _limitState != LimitState.Inactive)
		{
			float num2 = bodyB.Sweep.A - bodyA.Sweep.A - ReferenceAngle;
			float num3 = 0f;
			if (_limitState == LimitState.Equal)
			{
				float num4 = MathUtils.Clamp(num2 - _lowerAngle, (float)Math.PI * -2f / 45f, (float)Math.PI * 2f / 45f);
				num3 = (0f - _motorMass) * num4;
				num = Math.Abs(num4);
			}
			else if (_limitState == LimitState.AtLower)
			{
				float num5 = num2 - _lowerAngle;
				num = 0f - num5;
				num5 = MathUtils.Clamp(num5 + (float)Math.PI / 90f, (float)Math.PI * -2f / 45f, 0f);
				num3 = (0f - _motorMass) * num5;
			}
			else if (_limitState == LimitState.AtUpper)
			{
				float num6 = num2 - _upperAngle;
				num = num6;
				num6 = MathUtils.Clamp(num6 - (float)Math.PI / 90f, 0f, (float)Math.PI * 2f / 45f);
				num3 = (0f - _motorMass) * num6;
			}
			bodyA.Sweep.A -= bodyA.InvI * num3;
			bodyB.Sweep.A += bodyB.InvI * num3;
			bodyA.SynchronizeTransform();
			bodyB.SynchronizeTransform();
		}
		Vector2 a = MathUtils.Multiply(ref bodyA.Xf.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 a2 = MathUtils.Multiply(ref bodyB.Xf.R, LocalAnchorB - bodyB.LocalCenter);
		Vector2 vector = bodyB.Sweep.C + a2 - bodyA.Sweep.C - a;
		float num7 = vector.Length();
		float invMass = bodyA.InvMass;
		float invMass2 = bodyB.InvMass;
		float invI = bodyA.InvI;
		float invI2 = bodyB.InvI;
		if (vector.LengthSquared() > 0.0025f)
		{
			Vector2 vector2 = vector;
			vector2.Normalize();
			float num8 = invMass + invMass2;
			float num9 = 1f / num8;
			Vector2 vector3 = num9 * -vector;
			bodyA.Sweep.C -= 0.5f * invMass * vector3;
			bodyB.Sweep.C += 0.5f * invMass2 * vector3;
			vector = bodyB.Sweep.C + a2 - bodyA.Sweep.C - a;
		}
		Mat22 A = new Mat22(new Vector2(invMass + invMass2, 0f), new Vector2(0f, invMass + invMass2));
		Mat22 B = new Mat22(new Vector2(invI * a.Y * a.Y, (0f - invI) * a.X * a.Y), new Vector2((0f - invI) * a.X * a.Y, invI * a.X * a.X));
		Mat22 B2 = new Mat22(new Vector2(invI2 * a2.Y * a2.Y, (0f - invI2) * a2.X * a2.Y), new Vector2((0f - invI2) * a2.X * a2.Y, invI2 * a2.X * a2.X));
		Mat22.Add(ref A, ref B, out var R);
		Mat22.Add(ref R, ref B2, out var R2);
		Vector2 b = R2.Solve(-vector);
		bodyA.Sweep.C -= bodyA.InvMass * b;
		MathUtils.Cross(ref a, ref b, out _tmpFloat1);
		bodyA.Sweep.A -= bodyA.InvI * _tmpFloat1;
		bodyB.Sweep.C += bodyB.InvMass * b;
		MathUtils.Cross(ref a2, ref b, out _tmpFloat1);
		bodyB.Sweep.A += bodyB.InvI * _tmpFloat1;
		bodyA.SynchronizeTransform();
		bodyB.SynchronizeTransform();
		if (num7 <= 0.005f)
		{
			return num <= (float)Math.PI / 90f;
		}
		return false;
	}
}
