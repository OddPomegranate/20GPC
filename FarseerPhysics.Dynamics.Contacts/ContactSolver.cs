using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts;

public class ContactSolver
{
	public ContactConstraint[] Constraints;

	private int _constraintCount;

	private Contact[] _contacts;

	public void Reset(Contact[] contacts, int contactCount, float impulseRatio, bool warmstarting)
	{
		_contacts = contacts;
		_constraintCount = contactCount;
		if (Constraints == null || Constraints.Length < _constraintCount)
		{
			Constraints = new ContactConstraint[_constraintCount * 2];
			for (int i = 0; i < Constraints.Length; i++)
			{
				Constraints[i] = new ContactConstraint();
			}
		}
		for (int j = 0; j < _constraintCount; j++)
		{
			Contact contact = contacts[j];
			Fixture fixtureA = contact.FixtureA;
			Fixture fixtureB = contact.FixtureB;
			Shape shape = fixtureA.Shape;
			Shape shape2 = fixtureB.Shape;
			float radius = shape.Radius;
			float radius2 = shape2.Radius;
			Body body = fixtureA.Body;
			Body body2 = fixtureB.Body;
			Manifold manifold = contact.Manifold;
			ContactConstraint contactConstraint = Constraints[j];
			contactConstraint.Friction = Settings.MixFriction(fixtureA.Friction, fixtureB.Friction);
			contactConstraint.Restitution = Settings.MixRestitution(fixtureA.Restitution, fixtureB.Restitution);
			contactConstraint.BodyA = body;
			contactConstraint.BodyB = body2;
			contactConstraint.Manifold = manifold;
			contactConstraint.Normal = Vector2.Zero;
			contactConstraint.PointCount = manifold.PointCount;
			contactConstraint.LocalNormal = manifold.LocalNormal;
			contactConstraint.LocalPoint = manifold.LocalPoint;
			contactConstraint.RadiusA = radius;
			contactConstraint.RadiusB = radius2;
			contactConstraint.Type = manifold.Type;
			for (int k = 0; k < contactConstraint.PointCount; k++)
			{
				ManifoldPoint manifoldPoint = manifold.Points[k];
				ContactConstraintPoint contactConstraintPoint = contactConstraint.Points[k];
				if (warmstarting)
				{
					contactConstraintPoint.NormalImpulse = impulseRatio * manifoldPoint.NormalImpulse;
					contactConstraintPoint.TangentImpulse = impulseRatio * manifoldPoint.TangentImpulse;
				}
				else
				{
					contactConstraintPoint.NormalImpulse = 0f;
					contactConstraintPoint.TangentImpulse = 0f;
				}
				contactConstraintPoint.LocalPoint = manifoldPoint.LocalPoint;
				contactConstraintPoint.rA = Vector2.Zero;
				contactConstraintPoint.rB = Vector2.Zero;
				contactConstraintPoint.NormalMass = 0f;
				contactConstraintPoint.TangentMass = 0f;
				contactConstraintPoint.VelocityBias = 0f;
			}
			contactConstraint.K.SetZero();
			contactConstraint.NormalMass.SetZero();
		}
	}

	public void InitializeVelocityConstraints()
	{
		for (int i = 0; i < _constraintCount; i++)
		{
			ContactConstraint contactConstraint = Constraints[i];
			float radiusA = contactConstraint.RadiusA;
			float radiusB = contactConstraint.RadiusB;
			Body bodyA = contactConstraint.BodyA;
			Body bodyB = contactConstraint.BodyB;
			Manifold manifold = contactConstraint.Manifold;
			Vector2 linearVelocity = bodyA.LinearVelocity;
			Vector2 linearVelocity2 = bodyB.LinearVelocity;
			float angularVelocity = bodyA.AngularVelocity;
			float angularVelocity2 = bodyB.AngularVelocity;
			FarseerPhysics.Collision.Collision.GetWorldManifold(ref manifold, ref bodyA.Xf, radiusA, ref bodyB.Xf, radiusB, out contactConstraint.Normal, out var points);
			Vector2 vector = new Vector2(contactConstraint.Normal.Y, 0f - contactConstraint.Normal.X);
			for (int j = 0; j < contactConstraint.PointCount; j++)
			{
				ContactConstraintPoint contactConstraintPoint = contactConstraint.Points[j];
				contactConstraintPoint.rA = points[j] - bodyA.Sweep.C;
				contactConstraintPoint.rB = points[j] - bodyB.Sweep.C;
				float num = contactConstraintPoint.rA.X * contactConstraint.Normal.Y - contactConstraintPoint.rA.Y * contactConstraint.Normal.X;
				float num2 = contactConstraintPoint.rB.X * contactConstraint.Normal.Y - contactConstraintPoint.rB.Y * contactConstraint.Normal.X;
				num *= num;
				num2 *= num2;
				float num3 = bodyA.InvMass + bodyB.InvMass + bodyA.InvI * num + bodyB.InvI * num2;
				contactConstraintPoint.NormalMass = 1f / num3;
				float num4 = contactConstraintPoint.rA.X * vector.Y - contactConstraintPoint.rA.Y * vector.X;
				float num5 = contactConstraintPoint.rB.X * vector.Y - contactConstraintPoint.rB.Y * vector.X;
				num4 *= num4;
				num5 *= num5;
				float num6 = bodyA.InvMass + bodyB.InvMass + bodyA.InvI * num4 + bodyB.InvI * num5;
				contactConstraintPoint.TangentMass = 1f / num6;
				contactConstraintPoint.VelocityBias = 0f;
				float num7 = contactConstraint.Normal.X * (linearVelocity2.X + (0f - angularVelocity2) * contactConstraintPoint.rB.Y - linearVelocity.X - (0f - angularVelocity) * contactConstraintPoint.rA.Y) + contactConstraint.Normal.Y * (linearVelocity2.Y + angularVelocity2 * contactConstraintPoint.rB.X - linearVelocity.Y - angularVelocity * contactConstraintPoint.rA.X);
				if (num7 < -1f)
				{
					contactConstraintPoint.VelocityBias = (0f - contactConstraint.Restitution) * num7;
				}
			}
			if (contactConstraint.PointCount != 2)
			{
				continue;
			}
			ContactConstraintPoint contactConstraintPoint2 = contactConstraint.Points[0];
			ContactConstraintPoint contactConstraintPoint3 = contactConstraint.Points[1];
			float invMass = bodyA.InvMass;
			float invI = bodyA.InvI;
			float invMass2 = bodyB.InvMass;
			float invI2 = bodyB.InvI;
			float num8 = contactConstraintPoint2.rA.X * contactConstraint.Normal.Y - contactConstraintPoint2.rA.Y * contactConstraint.Normal.X;
			float num9 = contactConstraintPoint2.rB.X * contactConstraint.Normal.Y - contactConstraintPoint2.rB.Y * contactConstraint.Normal.X;
			float num10 = contactConstraintPoint3.rA.X * contactConstraint.Normal.Y - contactConstraintPoint3.rA.Y * contactConstraint.Normal.X;
			float num11 = contactConstraintPoint3.rB.X * contactConstraint.Normal.Y - contactConstraintPoint3.rB.Y * contactConstraint.Normal.X;
			float num12 = invMass + invMass2 + invI * num8 * num8 + invI2 * num9 * num9;
			float num13 = invMass + invMass2 + invI * num10 * num10 + invI2 * num11 * num11;
			float num14 = invMass + invMass2 + invI * num8 * num10 + invI2 * num9 * num11;
			if (num12 * num12 < 100f * (num12 * num13 - num14 * num14))
			{
				contactConstraint.K.Col1.X = num12;
				contactConstraint.K.Col1.Y = num14;
				contactConstraint.K.Col2.X = num14;
				contactConstraint.K.Col2.Y = num13;
				float x = contactConstraint.K.Col1.X;
				float x2 = contactConstraint.K.Col2.X;
				float y = contactConstraint.K.Col1.Y;
				float y2 = contactConstraint.K.Col2.Y;
				float num15 = x * y2 - x2 * y;
				if (num15 != 0f)
				{
					num15 = 1f / num15;
				}
				contactConstraint.NormalMass.Col1.X = num15 * y2;
				contactConstraint.NormalMass.Col1.Y = (0f - num15) * y;
				contactConstraint.NormalMass.Col2.X = (0f - num15) * x2;
				contactConstraint.NormalMass.Col2.Y = num15 * x;
			}
			else
			{
				contactConstraint.PointCount = 1;
			}
		}
	}

	public void WarmStart()
	{
		for (int i = 0; i < _constraintCount; i++)
		{
			ContactConstraint contactConstraint = Constraints[i];
			float y = contactConstraint.Normal.Y;
			float num = 0f - contactConstraint.Normal.X;
			for (int j = 0; j < contactConstraint.PointCount; j++)
			{
				ContactConstraintPoint contactConstraintPoint = contactConstraint.Points[j];
				float num2 = contactConstraintPoint.NormalImpulse * contactConstraint.Normal.X + contactConstraintPoint.TangentImpulse * y;
				float num3 = contactConstraintPoint.NormalImpulse * contactConstraint.Normal.Y + contactConstraintPoint.TangentImpulse * num;
				contactConstraint.BodyA.AngularVelocityInternal -= contactConstraint.BodyA.InvI * (contactConstraintPoint.rA.X * num3 - contactConstraintPoint.rA.Y * num2);
				contactConstraint.BodyA.LinearVelocityInternal.X -= contactConstraint.BodyA.InvMass * num2;
				contactConstraint.BodyA.LinearVelocityInternal.Y -= contactConstraint.BodyA.InvMass * num3;
				contactConstraint.BodyB.AngularVelocityInternal += contactConstraint.BodyB.InvI * (contactConstraintPoint.rB.X * num3 - contactConstraintPoint.rB.Y * num2);
				contactConstraint.BodyB.LinearVelocityInternal.X += contactConstraint.BodyB.InvMass * num2;
				contactConstraint.BodyB.LinearVelocityInternal.Y += contactConstraint.BodyB.InvMass * num3;
			}
		}
	}

	public void SolveVelocityConstraints()
	{
		for (int i = 0; i < _constraintCount; i++)
		{
			ContactConstraint contactConstraint = Constraints[i];
			float num = contactConstraint.BodyA.AngularVelocityInternal;
			float num2 = contactConstraint.BodyB.AngularVelocityInternal;
			float y = contactConstraint.Normal.Y;
			float num3 = 0f - contactConstraint.Normal.X;
			float friction = contactConstraint.Friction;
			for (int j = 0; j < contactConstraint.PointCount; j++)
			{
				ContactConstraintPoint contactConstraintPoint = contactConstraint.Points[j];
				float num4 = contactConstraintPoint.TangentMass * (0f - ((contactConstraint.BodyB.LinearVelocityInternal.X + (0f - num2) * contactConstraintPoint.rB.Y - contactConstraint.BodyA.LinearVelocityInternal.X - (0f - num) * contactConstraintPoint.rA.Y) * y + (contactConstraint.BodyB.LinearVelocityInternal.Y + num2 * contactConstraintPoint.rB.X - contactConstraint.BodyA.LinearVelocityInternal.Y - num * contactConstraintPoint.rA.X) * num3));
				float num5 = friction * contactConstraintPoint.NormalImpulse;
				float num6 = Math.Max(0f - num5, Math.Min(contactConstraintPoint.TangentImpulse + num4, num5));
				num4 = num6 - contactConstraintPoint.TangentImpulse;
				float num7 = num4 * y;
				float num8 = num4 * num3;
				contactConstraint.BodyA.LinearVelocityInternal.X -= contactConstraint.BodyA.InvMass * num7;
				contactConstraint.BodyA.LinearVelocityInternal.Y -= contactConstraint.BodyA.InvMass * num8;
				num -= contactConstraint.BodyA.InvI * (contactConstraintPoint.rA.X * num8 - contactConstraintPoint.rA.Y * num7);
				contactConstraint.BodyB.LinearVelocityInternal.X += contactConstraint.BodyB.InvMass * num7;
				contactConstraint.BodyB.LinearVelocityInternal.Y += contactConstraint.BodyB.InvMass * num8;
				num2 += contactConstraint.BodyB.InvI * (contactConstraintPoint.rB.X * num8 - contactConstraintPoint.rB.Y * num7);
				contactConstraintPoint.TangentImpulse = num6;
			}
			if (contactConstraint.PointCount == 1)
			{
				ContactConstraintPoint contactConstraintPoint2 = contactConstraint.Points[0];
				float num9 = (0f - contactConstraintPoint2.NormalMass) * ((contactConstraint.BodyB.LinearVelocityInternal.X + (0f - num2) * contactConstraintPoint2.rB.Y - contactConstraint.BodyA.LinearVelocityInternal.X - (0f - num) * contactConstraintPoint2.rA.Y) * contactConstraint.Normal.X + (contactConstraint.BodyB.LinearVelocityInternal.Y + num2 * contactConstraintPoint2.rB.X - contactConstraint.BodyA.LinearVelocityInternal.Y - num * contactConstraintPoint2.rA.X) * contactConstraint.Normal.Y - contactConstraintPoint2.VelocityBias);
				float num10 = Math.Max(contactConstraintPoint2.NormalImpulse + num9, 0f);
				num9 = num10 - contactConstraintPoint2.NormalImpulse;
				float num11 = num9 * contactConstraint.Normal.X;
				float num12 = num9 * contactConstraint.Normal.Y;
				contactConstraint.BodyA.LinearVelocityInternal.X -= contactConstraint.BodyA.InvMass * num11;
				contactConstraint.BodyA.LinearVelocityInternal.Y -= contactConstraint.BodyA.InvMass * num12;
				num -= contactConstraint.BodyA.InvI * (contactConstraintPoint2.rA.X * num12 - contactConstraintPoint2.rA.Y * num11);
				contactConstraint.BodyB.LinearVelocityInternal.X += contactConstraint.BodyB.InvMass * num11;
				contactConstraint.BodyB.LinearVelocityInternal.Y += contactConstraint.BodyB.InvMass * num12;
				num2 += contactConstraint.BodyB.InvI * (contactConstraintPoint2.rB.X * num12 - contactConstraintPoint2.rB.Y * num11);
				contactConstraintPoint2.NormalImpulse = num10;
			}
			else
			{
				ContactConstraintPoint contactConstraintPoint3 = contactConstraint.Points[0];
				ContactConstraintPoint contactConstraintPoint4 = contactConstraint.Points[1];
				float normalImpulse = contactConstraintPoint3.NormalImpulse;
				float normalImpulse2 = contactConstraintPoint4.NormalImpulse;
				float num13 = (contactConstraint.BodyB.LinearVelocityInternal.X + (0f - num2) * contactConstraintPoint3.rB.Y - contactConstraint.BodyA.LinearVelocityInternal.X - (0f - num) * contactConstraintPoint3.rA.Y) * contactConstraint.Normal.X + (contactConstraint.BodyB.LinearVelocityInternal.Y + num2 * contactConstraintPoint3.rB.X - contactConstraint.BodyA.LinearVelocityInternal.Y - num * contactConstraintPoint3.rA.X) * contactConstraint.Normal.Y;
				float num14 = (contactConstraint.BodyB.LinearVelocityInternal.X + (0f - num2) * contactConstraintPoint4.rB.Y - contactConstraint.BodyA.LinearVelocityInternal.X - (0f - num) * contactConstraintPoint4.rA.Y) * contactConstraint.Normal.X + (contactConstraint.BodyB.LinearVelocityInternal.Y + num2 * contactConstraintPoint4.rB.X - contactConstraint.BodyA.LinearVelocityInternal.Y - num * contactConstraintPoint4.rA.X) * contactConstraint.Normal.Y;
				float num15 = num13 - contactConstraintPoint3.VelocityBias - (contactConstraint.K.Col1.X * normalImpulse + contactConstraint.K.Col2.X * normalImpulse2);
				float num16 = num14 - contactConstraintPoint4.VelocityBias - (contactConstraint.K.Col1.Y * normalImpulse + contactConstraint.K.Col2.Y * normalImpulse2);
				float num17 = 0f - (contactConstraint.NormalMass.Col1.X * num15 + contactConstraint.NormalMass.Col2.X * num16);
				float num18 = 0f - (contactConstraint.NormalMass.Col1.Y * num15 + contactConstraint.NormalMass.Col2.Y * num16);
				if (num17 >= 0f && num18 >= 0f)
				{
					float num19 = num17 - normalImpulse;
					float num20 = num18 - normalImpulse2;
					float num21 = num19 * contactConstraint.Normal.X;
					float num22 = num19 * contactConstraint.Normal.Y;
					float num23 = num20 * contactConstraint.Normal.X;
					float num24 = num20 * contactConstraint.Normal.Y;
					float num25 = num21 + num23;
					float num26 = num22 + num24;
					contactConstraint.BodyA.LinearVelocityInternal.X -= contactConstraint.BodyA.InvMass * num25;
					contactConstraint.BodyA.LinearVelocityInternal.Y -= contactConstraint.BodyA.InvMass * num26;
					num -= contactConstraint.BodyA.InvI * (contactConstraintPoint3.rA.X * num22 - contactConstraintPoint3.rA.Y * num21 + (contactConstraintPoint4.rA.X * num24 - contactConstraintPoint4.rA.Y * num23));
					contactConstraint.BodyB.LinearVelocityInternal.X += contactConstraint.BodyB.InvMass * num25;
					contactConstraint.BodyB.LinearVelocityInternal.Y += contactConstraint.BodyB.InvMass * num26;
					num2 += contactConstraint.BodyB.InvI * (contactConstraintPoint3.rB.X * num22 - contactConstraintPoint3.rB.Y * num21 + (contactConstraintPoint4.rB.X * num24 - contactConstraintPoint4.rB.Y * num23));
					contactConstraintPoint3.NormalImpulse = num17;
					contactConstraintPoint4.NormalImpulse = num18;
				}
				else
				{
					num17 = (0f - contactConstraintPoint3.NormalMass) * num15;
					num18 = 0f;
					num13 = 0f;
					num14 = contactConstraint.K.Col1.Y * num17 + num16;
					if (num17 >= 0f && num14 >= 0f)
					{
						float num27 = num17 - normalImpulse;
						float num28 = num18 - normalImpulse2;
						float num29 = num27 * contactConstraint.Normal.X;
						float num30 = num27 * contactConstraint.Normal.Y;
						float num31 = num28 * contactConstraint.Normal.X;
						float num32 = num28 * contactConstraint.Normal.Y;
						float num33 = num29 + num31;
						float num34 = num30 + num32;
						contactConstraint.BodyA.LinearVelocityInternal.X -= contactConstraint.BodyA.InvMass * num33;
						contactConstraint.BodyA.LinearVelocityInternal.Y -= contactConstraint.BodyA.InvMass * num34;
						num -= contactConstraint.BodyA.InvI * (contactConstraintPoint3.rA.X * num30 - contactConstraintPoint3.rA.Y * num29 + (contactConstraintPoint4.rA.X * num32 - contactConstraintPoint4.rA.Y * num31));
						contactConstraint.BodyB.LinearVelocityInternal.X += contactConstraint.BodyB.InvMass * num33;
						contactConstraint.BodyB.LinearVelocityInternal.Y += contactConstraint.BodyB.InvMass * num34;
						num2 += contactConstraint.BodyB.InvI * (contactConstraintPoint3.rB.X * num30 - contactConstraintPoint3.rB.Y * num29 + (contactConstraintPoint4.rB.X * num32 - contactConstraintPoint4.rB.Y * num31));
						contactConstraintPoint3.NormalImpulse = num17;
						contactConstraintPoint4.NormalImpulse = num18;
					}
					else
					{
						num17 = 0f;
						num18 = (0f - contactConstraintPoint4.NormalMass) * num16;
						num13 = contactConstraint.K.Col2.X * num18 + num15;
						num14 = 0f;
						if (num18 >= 0f && num13 >= 0f)
						{
							float num35 = num17 - normalImpulse;
							float num36 = num18 - normalImpulse2;
							float num37 = num35 * contactConstraint.Normal.X;
							float num38 = num35 * contactConstraint.Normal.Y;
							float num39 = num36 * contactConstraint.Normal.X;
							float num40 = num36 * contactConstraint.Normal.Y;
							float num41 = num37 + num39;
							float num42 = num38 + num40;
							contactConstraint.BodyA.LinearVelocityInternal.X -= contactConstraint.BodyA.InvMass * num41;
							contactConstraint.BodyA.LinearVelocityInternal.Y -= contactConstraint.BodyA.InvMass * num42;
							num -= contactConstraint.BodyA.InvI * (contactConstraintPoint3.rA.X * num38 - contactConstraintPoint3.rA.Y * num37 + (contactConstraintPoint4.rA.X * num40 - contactConstraintPoint4.rA.Y * num39));
							contactConstraint.BodyB.LinearVelocityInternal.X += contactConstraint.BodyB.InvMass * num41;
							contactConstraint.BodyB.LinearVelocityInternal.Y += contactConstraint.BodyB.InvMass * num42;
							num2 += contactConstraint.BodyB.InvI * (contactConstraintPoint3.rB.X * num38 - contactConstraintPoint3.rB.Y * num37 + (contactConstraintPoint4.rB.X * num40 - contactConstraintPoint4.rB.Y * num39));
							contactConstraintPoint3.NormalImpulse = num17;
							contactConstraintPoint4.NormalImpulse = num18;
						}
						else
						{
							num17 = 0f;
							num18 = 0f;
							num13 = num15;
							num14 = num16;
							if (num13 >= 0f && num14 >= 0f)
							{
								float num43 = num17 - normalImpulse;
								float num44 = num18 - normalImpulse2;
								float num45 = num43 * contactConstraint.Normal.X;
								float num46 = num43 * contactConstraint.Normal.Y;
								float num47 = num44 * contactConstraint.Normal.X;
								float num48 = num44 * contactConstraint.Normal.Y;
								float num49 = num45 + num47;
								float num50 = num46 + num48;
								contactConstraint.BodyA.LinearVelocityInternal.X -= contactConstraint.BodyA.InvMass * num49;
								contactConstraint.BodyA.LinearVelocityInternal.Y -= contactConstraint.BodyA.InvMass * num50;
								num -= contactConstraint.BodyA.InvI * (contactConstraintPoint3.rA.X * num46 - contactConstraintPoint3.rA.Y * num45 + (contactConstraintPoint4.rA.X * num48 - contactConstraintPoint4.rA.Y * num47));
								contactConstraint.BodyB.LinearVelocityInternal.X += contactConstraint.BodyB.InvMass * num49;
								contactConstraint.BodyB.LinearVelocityInternal.Y += contactConstraint.BodyB.InvMass * num50;
								num2 += contactConstraint.BodyB.InvI * (contactConstraintPoint3.rB.X * num46 - contactConstraintPoint3.rB.Y * num45 + (contactConstraintPoint4.rB.X * num48 - contactConstraintPoint4.rB.Y * num47));
								contactConstraintPoint3.NormalImpulse = num17;
								contactConstraintPoint4.NormalImpulse = num18;
							}
						}
					}
				}
			}
			contactConstraint.BodyA.AngularVelocityInternal = num;
			contactConstraint.BodyB.AngularVelocityInternal = num2;
		}
	}

	public void StoreImpulses()
	{
		for (int i = 0; i < _constraintCount; i++)
		{
			ContactConstraint contactConstraint = Constraints[i];
			Manifold manifold = contactConstraint.Manifold;
			for (int j = 0; j < contactConstraint.PointCount; j++)
			{
				ManifoldPoint value = manifold.Points[j];
				ContactConstraintPoint contactConstraintPoint = contactConstraint.Points[j];
				value.NormalImpulse = contactConstraintPoint.NormalImpulse;
				value.TangentImpulse = contactConstraintPoint.TangentImpulse;
				manifold.Points[j] = value;
			}
			contactConstraint.Manifold = manifold;
			_contacts[i].Manifold = manifold;
		}
	}

	public bool SolvePositionConstraints(float baumgarte)
	{
		float num = 0f;
		for (int i = 0; i < _constraintCount; i++)
		{
			ContactConstraint contactConstraint = Constraints[i];
			Body bodyA = contactConstraint.BodyA;
			Body bodyB = contactConstraint.BodyB;
			float num2 = bodyA.Mass * bodyA.InvMass;
			float num3 = bodyA.Mass * bodyA.InvI;
			float num4 = bodyB.Mass * bodyB.InvMass;
			float num5 = bodyB.Mass * bodyB.InvI;
			for (int j = 0; j < contactConstraint.PointCount; j++)
			{
				Solve(contactConstraint, j, out var normal, out var point, out var separation);
				float num6 = point.X - bodyA.Sweep.C.X;
				float num7 = point.Y - bodyA.Sweep.C.Y;
				float num8 = point.X - bodyB.Sweep.C.X;
				float num9 = point.Y - bodyB.Sweep.C.Y;
				num = Math.Min(num, separation);
				float num10 = Math.Max(-0.2f, Math.Min(baumgarte * (separation + 0.005f), 0f));
				float num11 = num6 * normal.Y - num7 * normal.X;
				float num12 = num8 * normal.Y - num9 * normal.X;
				float num13 = num2 + num4 + num3 * num11 * num11 + num5 * num12 * num12;
				float num14 = ((num13 > 0f) ? ((0f - num10) / num13) : 0f);
				float num15 = num14 * normal.X;
				float num16 = num14 * normal.Y;
				bodyA.Sweep.C.X -= num2 * num15;
				bodyA.Sweep.C.Y -= num2 * num16;
				bodyA.Sweep.A -= num3 * (num6 * num16 - num7 * num15);
				bodyB.Sweep.C.X += num4 * num15;
				bodyB.Sweep.C.Y += num4 * num16;
				bodyB.Sweep.A += num5 * (num8 * num16 - num9 * num15);
				bodyA.SynchronizeTransform();
				bodyB.SynchronizeTransform();
			}
		}
		return num >= -0.0075f;
	}

	private static void Solve(ContactConstraint cc, int index, out Vector2 normal, out Vector2 point, out float separation)
	{
		normal = Vector2.Zero;
		switch (cc.Type)
		{
		case ManifoldType.Circles:
		{
			Vector2 worldPoint5 = cc.BodyA.GetWorldPoint(ref cc.LocalPoint);
			Vector2 worldPoint6 = cc.BodyB.GetWorldPoint(ref cc.Points[0].LocalPoint);
			float num = (worldPoint5.X - worldPoint6.X) * (worldPoint5.X - worldPoint6.X) + (worldPoint5.Y - worldPoint6.Y) * (worldPoint5.Y - worldPoint6.Y);
			if (num > 1.4210855E-14f)
			{
				Vector2 vector = worldPoint6 - worldPoint5;
				float num2 = 1f / (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
				normal.X = vector.X * num2;
				normal.Y = vector.Y * num2;
			}
			else
			{
				normal.X = 1f;
				normal.Y = 0f;
			}
			point = 0.5f * (worldPoint5 + worldPoint6);
			separation = (worldPoint6.X - worldPoint5.X) * normal.X + (worldPoint6.Y - worldPoint5.Y) * normal.Y - cc.RadiusA - cc.RadiusB;
			break;
		}
		case ManifoldType.FaceA:
		{
			normal = cc.BodyA.GetWorldVector(ref cc.LocalNormal);
			Vector2 worldPoint3 = cc.BodyA.GetWorldPoint(ref cc.LocalPoint);
			Vector2 worldPoint4 = cc.BodyB.GetWorldPoint(ref cc.Points[index].LocalPoint);
			separation = (worldPoint4.X - worldPoint3.X) * normal.X + (worldPoint4.Y - worldPoint3.Y) * normal.Y - cc.RadiusA - cc.RadiusB;
			point = worldPoint4;
			break;
		}
		case ManifoldType.FaceB:
		{
			normal = cc.BodyB.GetWorldVector(ref cc.LocalNormal);
			Vector2 worldPoint = cc.BodyB.GetWorldPoint(ref cc.LocalPoint);
			Vector2 worldPoint2 = cc.BodyA.GetWorldPoint(ref cc.Points[index].LocalPoint);
			separation = (worldPoint2.X - worldPoint.X) * normal.X + (worldPoint2.Y - worldPoint.Y) * normal.Y - cc.RadiusA - cc.RadiusB;
			point = worldPoint2;
			normal = -normal;
			break;
		}
		default:
			point = Vector2.Zero;
			separation = 0f;
			break;
		}
	}
}
