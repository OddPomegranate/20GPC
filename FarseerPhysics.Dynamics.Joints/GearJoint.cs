using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

public class GearJoint : Joint
{
	private Jacobian _J;

	private float _ant;

	private FixedPrismaticJoint _fixedPrismatic1;

	private FixedPrismaticJoint _fixedPrismatic2;

	private FixedRevoluteJoint _fixedRevolute1;

	private FixedRevoluteJoint _fixedRevolute2;

	private float _impulse;

	private float _mass;

	private PrismaticJoint _prismatic1;

	private PrismaticJoint _prismatic2;

	private RevoluteJoint _revolute1;

	private RevoluteJoint _revolute2;

	public override Vector2 WorldAnchorA => base.BodyA.GetWorldPoint(LocalAnchor1);

	public override Vector2 WorldAnchorB
	{
		get
		{
			return base.BodyB.GetWorldPoint(LocalAnchor2);
		}
		set
		{
		}
	}

	public float Ratio { get; set; }

	public Joint JointA { get; set; }

	public Joint JointB { get; set; }

	public Vector2 LocalAnchor1 { get; private set; }

	public Vector2 LocalAnchor2 { get; private set; }

	public GearJoint(Joint jointA, Joint jointB, float ratio)
		: base(jointA.BodyA, jointA.BodyB)
	{
		base.JointType = JointType.Gear;
		JointA = jointA;
		JointB = jointB;
		Ratio = ratio;
		JointType jointType = jointA.JointType;
		JointType jointType2 = jointB.JointType;
		if (jointType != JointType.Revolute)
		{
			_ = 1;
		}
		if (jointType2 != JointType.Revolute)
		{
			_ = 1;
		}
		float num = 0f;
		float num2 = 0f;
		switch (jointType)
		{
		case JointType.Revolute:
			base.BodyA = jointA.BodyB;
			_revolute1 = (RevoluteJoint)jointA;
			LocalAnchor1 = _revolute1.LocalAnchorB;
			num = _revolute1.JointAngle;
			break;
		case JointType.Prismatic:
			base.BodyA = jointA.BodyB;
			_prismatic1 = (PrismaticJoint)jointA;
			LocalAnchor1 = _prismatic1.LocalAnchorB;
			num = _prismatic1.JointTranslation;
			break;
		case JointType.FixedRevolute:
			base.BodyA = jointA.BodyA;
			_fixedRevolute1 = (FixedRevoluteJoint)jointA;
			LocalAnchor1 = _fixedRevolute1.LocalAnchorA;
			num = _fixedRevolute1.JointAngle;
			break;
		case JointType.FixedPrismatic:
			base.BodyA = jointA.BodyA;
			_fixedPrismatic1 = (FixedPrismaticJoint)jointA;
			LocalAnchor1 = _fixedPrismatic1.LocalAnchorA;
			num = _fixedPrismatic1.JointTranslation;
			break;
		}
		switch (jointType2)
		{
		case JointType.Revolute:
			base.BodyB = jointB.BodyB;
			_revolute2 = (RevoluteJoint)jointB;
			LocalAnchor2 = _revolute2.LocalAnchorB;
			num2 = _revolute2.JointAngle;
			break;
		case JointType.Prismatic:
			base.BodyB = jointB.BodyB;
			_prismatic2 = (PrismaticJoint)jointB;
			LocalAnchor2 = _prismatic2.LocalAnchorB;
			num2 = _prismatic2.JointTranslation;
			break;
		case JointType.FixedRevolute:
			base.BodyB = jointB.BodyA;
			_fixedRevolute2 = (FixedRevoluteJoint)jointB;
			LocalAnchor2 = _fixedRevolute2.LocalAnchorA;
			num2 = _fixedRevolute2.JointAngle;
			break;
		case JointType.FixedPrismatic:
			base.BodyB = jointB.BodyA;
			_fixedPrismatic2 = (FixedPrismaticJoint)jointB;
			LocalAnchor2 = _fixedPrismatic2.LocalAnchorA;
			num2 = _fixedPrismatic2.JointTranslation;
			break;
		}
		_ant = num + Ratio * num2;
	}

	public override Vector2 GetReactionForce(float inv_dt)
	{
		Vector2 vector = _impulse * _J.LinearB;
		return inv_dt * vector;
	}

	public override float GetReactionTorque(float inv_dt)
	{
		base.BodyB.GetTransform(out var transform);
		Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchor2 - base.BodyB.LocalCenter);
		Vector2 b = _impulse * _J.LinearB;
		float num = _impulse * _J.AngularB - MathUtils.Cross(a, b);
		return inv_dt * num;
	}

	internal override void InitVelocityConstraints(ref TimeStep step)
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		float num = 0f;
		_J.SetZero();
		if (_revolute1 != null || _fixedRevolute1 != null)
		{
			_J.AngularA = -1f;
			num += bodyA.InvI;
		}
		else
		{
			Vector2 vector = ((_prismatic1 == null) ? _fixedPrismatic1.LocalXAxis1 : _prismatic1.LocalXAxis1);
			bodyA.GetTransform(out var transform);
			Vector2 a = MathUtils.Multiply(ref transform.R, LocalAnchor1 - bodyA.LocalCenter);
			float num2 = MathUtils.Cross(a, vector);
			_J.LinearA = -vector;
			_J.AngularA = 0f - num2;
			num += bodyA.InvMass + bodyA.InvI * num2 * num2;
		}
		if (_revolute2 != null || _fixedRevolute2 != null)
		{
			_J.AngularB = 0f - Ratio;
			num += Ratio * Ratio * bodyB.InvI;
		}
		else
		{
			Vector2 vector2 = ((_prismatic2 == null) ? _fixedPrismatic2.LocalXAxis1 : _prismatic2.LocalXAxis1);
			bodyB.GetTransform(out var transform2);
			Vector2 a2 = MathUtils.Multiply(ref transform2.R, LocalAnchor2 - bodyB.LocalCenter);
			float num3 = MathUtils.Cross(a2, vector2);
			_J.LinearB = (0f - Ratio) * vector2;
			_J.AngularB = (0f - Ratio) * num3;
			num += Ratio * Ratio * (bodyB.InvMass + bodyB.InvI * num3 * num3);
		}
		_mass = ((num > 0f) ? (1f / num) : 0f);
		if (Settings.EnableWarmstarting)
		{
			bodyA.LinearVelocityInternal += bodyA.InvMass * _impulse * _J.LinearA;
			bodyA.AngularVelocityInternal += bodyA.InvI * _impulse * _J.AngularA;
			bodyB.LinearVelocityInternal += bodyB.InvMass * _impulse * _J.LinearB;
			bodyB.AngularVelocityInternal += bodyB.InvI * _impulse * _J.AngularB;
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
		float num = _J.Compute(bodyA.LinearVelocityInternal, bodyA.AngularVelocityInternal, bodyB.LinearVelocityInternal, bodyB.AngularVelocityInternal);
		float num2 = _mass * (0f - num);
		_impulse += num2;
		bodyA.LinearVelocityInternal += bodyA.InvMass * num2 * _J.LinearA;
		bodyA.AngularVelocityInternal += bodyA.InvI * num2 * _J.AngularA;
		bodyB.LinearVelocityInternal += bodyB.InvMass * num2 * _J.LinearB;
		bodyB.AngularVelocityInternal += bodyB.InvI * num2 * _J.AngularB;
	}

	internal override bool SolvePositionConstraints()
	{
		Body bodyA = base.BodyA;
		Body bodyB = base.BodyB;
		float num = 0f;
		float num2 = 0f;
		if (_revolute1 != null)
		{
			num = _revolute1.JointAngle;
		}
		else if (_fixedRevolute1 != null)
		{
			num = _fixedRevolute1.JointAngle;
		}
		else if (_prismatic1 != null)
		{
			num = _prismatic1.JointTranslation;
		}
		else if (_fixedPrismatic1 != null)
		{
			num = _fixedPrismatic1.JointTranslation;
		}
		if (_revolute2 != null)
		{
			num2 = _revolute2.JointAngle;
		}
		else if (_fixedRevolute2 != null)
		{
			num2 = _fixedRevolute2.JointAngle;
		}
		else if (_prismatic2 != null)
		{
			num2 = _prismatic2.JointTranslation;
		}
		else if (_fixedPrismatic2 != null)
		{
			num2 = _fixedPrismatic2.JointTranslation;
		}
		float num3 = _ant - (num + Ratio * num2);
		float num4 = _mass * (0f - num3);
		bodyA.Sweep.C += bodyA.InvMass * num4 * _J.LinearA;
		bodyA.Sweep.A += bodyA.InvI * num4 * _J.AngularA;
		bodyB.Sweep.C += bodyB.InvMass * num4 * _J.LinearB;
		bodyB.Sweep.A += bodyB.InvI * num4 * _J.AngularB;
		bodyA.SynchronizeTransform();
		bodyB.SynchronizeTransform();
		return true;
	}
}
