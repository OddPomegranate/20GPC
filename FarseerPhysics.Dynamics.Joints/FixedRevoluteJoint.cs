using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class FixedRevoluteJoint : Joint
{
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

	private float _upperAngle;

	private Vector2 _worldAnchor;

	public override Vector2 WorldAnchorA => base.BodyA.GetWorldPoint(LocalAnchorA);

	public override Vector2 WorldAnchorB
	{
		get
		{
			return _worldAnchor;
		}
		set
		{
			_worldAnchor = value;
		}
	}

	public Vector2 LocalAnchorA { get; set; }

	public float ReferenceAngle { get; set; }

	public float JointAngle => base.BodyA.Sweep.A - ReferenceAngle;

	public float JointSpeed => base.BodyA.AngularVelocityInternal;

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

	public FixedRevoluteJoint(Body body, Vector2 bodyAnchor, Vector2 worldAnchor)
		: base(body)
	{
		base.JointType = JointType.FixedRevolute;
		LocalAnchorA = bodyAnchor;
		_worldAnchor = worldAnchor;
		ReferenceAngle = 0f - base.BodyA.Rotation;
		_impulse = Vector3.Zero;
		_limitState = LimitState.Inactive;
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		return inv_dt * new Vector2(_impulse.X, _impulse.Y);
	}

	public override float GetReactionTorque(float inv_dt)
	{
		return inv_dt * _impulse.Z;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		if (!_enableMotor)
		{
			_ = _enableLimit;
		}
		bodyA.GetTransform(out var transform);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 worldAnchor = _worldAnchor;
		float invMass = bodyA.InvMass;
		float invI = bodyA.InvI;
		_mass.Col1.X = invMass + 0f + a.Y * a.Y * invI + worldAnchor.Y * worldAnchor.Y * 0f;
		_mass.Col2.X = (0f - a.Y) * a.X * invI - worldAnchor.Y * worldAnchor.X * 0f;
		_mass.Col3.X = (0f - a.Y) * invI - worldAnchor.Y * 0f;
		_mass.Col1.Y = _mass.Col2.X;
		_mass.Col2.Y = invMass + 0f + a.X * a.X * invI + worldAnchor.X * worldAnchor.X * 0f;
		_mass.Col3.Y = a.X * invI + worldAnchor.X * 0f;
		_mass.Col1.Z = _mass.Col3.X;
		_mass.Col2.Z = _mass.Col3.Y;
		_mass.Col3.Z = invI + 0f;
		_motorMass = invI + 0f;
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
			float num = 0f - bodyA.Sweep.A - ReferenceAngle;
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
			Vector2 vector = new Vector2(_impulse.X, _impulse.Y);
			bodyA.LinearVelocityInternal -= invMass * vector;
			bodyA.AngularVelocityInternal -= invI * (MathUtils.Cross(a, vector) + _motorImpulse + _impulse.Z);
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
		Vector2 linearVelocityInternal = bodyA.LinearVelocityInternal;
		float num = bodyA.AngularVelocityInternal;
		Vector2 zero = Vector2.Zero;
		float invMass = bodyA.InvMass;
		float invI = bodyA.InvI;
		if (_enableMotor && _limitState != LimitState.Equal)
		{
			float num2 = 0f - num - _motorSpeed;
			float num3 = _motorMass * (0f - num2);
			float motorImpulse = _motorImpulse;
			float num4 = step.dt * _maxMotorTorque;
			_motorImpulse = MathUtils.Clamp(_motorImpulse + num3, 0f - num4, num4);
			num3 = _motorImpulse - motorImpulse;
			num -= invI * num3;
		}
		if (_enableLimit && _limitState != LimitState.Inactive)
		{
			bodyA.GetTransform(out var transform);
			Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
			Vector2 worldAnchor = _worldAnchor;
			Vector2 vector = zero + MathUtils.Cross(0f, worldAnchor) - linearVelocityInternal - MathUtils.Cross(num, a);
			float z = 0f - num;
			Vector3 vector2 = new Vector3(vector.X, vector.Y, z);
			Vector3 vector3 = _mass.Solve33(-vector2);
			if (_limitState == LimitState.Equal)
			{
				_impulse += vector3;
			}
			else if (_limitState == LimitState.AtLower)
			{
				float num5 = _impulse.Z + vector3.Z;
				if (num5 < 0f)
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
				float num6 = _impulse.Z + vector3.Z;
				if (num6 > 0f)
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
			Vector2 vector6 = new Vector2(vector3.X, vector3.Y);
			linearVelocityInternal -= invMass * vector6;
			num -= invI * (MathUtils.Cross(a, vector6) + vector3.Z);
		}
		else
		{
			bodyA.GetTransform(out var transform2);
			Vector2 a2 = MathUtils.Multiply(ref transform2.R, LocalAnchorA - bodyA.LocalCenter);
			Vector2 worldAnchor2 = _worldAnchor;
			Vector2 vector7 = zero + MathUtils.Cross(0f, worldAnchor2) - linearVelocityInternal - MathUtils.Cross(num, a2);
			Vector2 vector8 = _mass.Solve22(-vector7);
			_impulse.X += vector8.X;
			_impulse.Y += vector8.Y;
			linearVelocityInternal -= invMass * vector8;
			num -= invI * MathUtils.Cross(a2, vector8);
		}
		bodyA.LinearVelocityInternal = linearVelocityInternal;
		bodyA.AngularVelocityInternal = num;
	}

	internal override bool SolvePositionConstraints()
	{
		Body bodyA = base.BodyA;
		float num = 0f;
		if (_enableLimit && _limitState != LimitState.Inactive)
		{
			float num2 = 0f - bodyA.Sweep.A - ReferenceAngle;
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
			bodyA.SynchronizeTransform();
		}
		bodyA.GetTransform(out var transform);
		Vector2 vector = MathUtils.Multiply(ref transform.R, LocalAnchorA - bodyA.LocalCenter);
		Vector2 worldAnchor = _worldAnchor;
		Vector2 vector2 = Vector2.Zero + worldAnchor - bodyA.Sweep.C - vector;
		float num7 = vector2.Length();
		float invMass = bodyA.InvMass;
		float invI = bodyA.InvI;
		if (vector2.LengthSquared() > 0.0025f)
		{
			Vector2 vector3 = vector2;
			vector3.Normalize();
			float num8 = invMass + 0f;
			float num9 = 1f / num8;
			Vector2 vector4 = num9 * -vector2;
			bodyA.Sweep.C -= 0.5f * invMass * vector4;
			vector2 = Vector2.Zero + worldAnchor - bodyA.Sweep.C - vector;
		}
		Mat22 A = new Mat22(new Vector2(invMass + 0f, 0f), new Vector2(0f, invMass + 0f));
		Mat22 B = new Mat22(new Vector2(invI * vector.Y * vector.Y, (0f - invI) * vector.X * vector.Y), new Vector2((0f - invI) * vector.X * vector.Y, invI * vector.X * vector.X));
		Mat22 B2 = new Mat22(new Vector2(0f * worldAnchor.Y * worldAnchor.Y, -0f * worldAnchor.X * worldAnchor.Y), new Vector2(-0f * worldAnchor.X * worldAnchor.Y, 0f * worldAnchor.X * worldAnchor.X));
		Mat22.Add(ref A, ref B, out var R);
		Mat22.Add(ref R, ref B2, out var R2);
		Vector2 vector5 = R2.Solve(-vector2);
		bodyA.Sweep.C -= bodyA.InvMass * vector5;
		bodyA.Sweep.A -= bodyA.InvI * MathUtils.Cross(vector, vector5);
		bodyA.SynchronizeTransform();
		if (num7 <= 0.005f)
		{
			return num <= (float)Math.PI / 90f;
		}
		return false;
	}
}
