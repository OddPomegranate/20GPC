using System;
using System.Diagnostics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics;

public class Island
{
	private const float LinTolSqr = 0.0001f;

	private const float AngTolSqr = 0.0012184697f;

	public Body[] Bodies;

	public int BodyCount;

	public int ContactCount;

	public int JointCount;

	private int _bodyCapacity;

	private int _contactCapacity;

	private ContactManager _contactManager;

	private ContactSolver _contactSolver = new ContactSolver();

	private Contact[] _contacts;

	private int _jointCapacity;

	private Joint[] _joints;

	public float JointUpdateTime;

	private Stopwatch _watch = new Stopwatch();

	private float _tmpTime;

	public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
	{
		_bodyCapacity = bodyCapacity;
		_contactCapacity = contactCapacity;
		_jointCapacity = jointCapacity;
		BodyCount = 0;
		ContactCount = 0;
		JointCount = 0;
		_contactManager = contactManager;
		if (Bodies == null || Bodies.Length < bodyCapacity)
		{
			Bodies = new Body[bodyCapacity];
		}
		if (_contacts == null || _contacts.Length < contactCapacity)
		{
			_contacts = new Contact[contactCapacity * 2];
		}
		if (_joints == null || _joints.Length < jointCapacity)
		{
			_joints = new Joint[jointCapacity * 2];
		}
	}

	public void Clear()
	{
		BodyCount = 0;
		ContactCount = 0;
		JointCount = 0;
	}

	public void Solve(ref TimeStep step, ref Vector2 gravity)
	{
		for (int i = 0; i < BodyCount; i++)
		{
			Body body = Bodies[i];
			if (body.BodyType == BodyType.Dynamic)
			{
				if (body.IgnoreGravity)
				{
					body.LinearVelocityInternal.X += step.dt * (body.InvMass * body.Force.X);
					body.LinearVelocityInternal.Y += step.dt * (body.InvMass * body.Force.Y);
					body.AngularVelocityInternal += step.dt * body.InvI * body.Torque;
				}
				else
				{
					body.LinearVelocityInternal.X += step.dt * (gravity.X + body.InvMass * body.Force.X);
					body.LinearVelocityInternal.Y += step.dt * (gravity.Y + body.InvMass * body.Force.Y);
					body.AngularVelocityInternal += step.dt * body.InvI * body.Torque;
				}
				body.LinearVelocityInternal *= MathUtils.Clamp(1f - step.dt * body.LinearDamping, 0f, 1f);
				body.AngularVelocityInternal *= MathUtils.Clamp(1f - step.dt * body.AngularDamping, 0f, 1f);
			}
		}
		int num = -1;
		for (int j = 0; j < ContactCount; j++)
		{
			Fixture fixtureA = _contacts[j].FixtureA;
			Fixture fixtureB = _contacts[j].FixtureB;
			Body body2 = fixtureA.Body;
			Body body3 = fixtureB.Body;
			if (body2.BodyType != BodyType.Static && body3.BodyType != BodyType.Static)
			{
				num++;
				Contact contact = _contacts[num];
				_contacts[num] = _contacts[j];
				_contacts[j] = contact;
			}
		}
		_contactSolver.Reset(_contacts, ContactCount, step.dtRatio, Settings.EnableWarmstarting);
		_contactSolver.InitializeVelocityConstraints();
		if (Settings.EnableWarmstarting)
		{
			_contactSolver.WarmStart();
		}
		if (Settings.EnableDiagnostics)
		{
			_watch.Start();
			_tmpTime = 0f;
		}
		for (int k = 0; k < JointCount; k++)
		{
			if (_joints[k].Enabled)
			{
				_joints[k].InitVelocityConstraints(ref step);
			}
		}
		if (Settings.EnableDiagnostics)
		{
			_tmpTime += _watch.ElapsedTicks;
		}
		for (int l = 0; l < Settings.VelocityIterations; l++)
		{
			if (Settings.EnableDiagnostics)
			{
				_watch.Start();
			}
			for (int m = 0; m < JointCount; m++)
			{
				Joint joint = _joints[m];
				if (joint.Enabled)
				{
					joint.SolveVelocityConstraints(ref step);
					joint.Validate(step.inv_dt);
				}
			}
			if (Settings.EnableDiagnostics)
			{
				_watch.Stop();
				_tmpTime += _watch.ElapsedTicks;
				_watch.Reset();
			}
			_contactSolver.SolveVelocityConstraints();
		}
		_contactSolver.StoreImpulses();
		for (int n = 0; n < BodyCount; n++)
		{
			Body body4 = Bodies[n];
			if (body4.BodyType != BodyType.Static)
			{
				float num2 = step.dt * body4.LinearVelocityInternal.X;
				float num3 = step.dt * body4.LinearVelocityInternal.Y;
				float num4 = num2 * num2 + num3 * num3;
				if (num4 > 4f)
				{
					float num5 = (float)Math.Sqrt(num4);
					float num6 = 2f / num5;
					body4.LinearVelocityInternal.X *= num6;
					body4.LinearVelocityInternal.Y *= num6;
				}
				float num7 = step.dt * body4.AngularVelocityInternal;
				if (num7 * num7 > 2.4674013f)
				{
					float num8 = (float)Math.PI / 2f / Math.Abs(num7);
					body4.AngularVelocityInternal *= num8;
				}
				body4.Sweep.C0.X = body4.Sweep.C.X;
				body4.Sweep.C0.Y = body4.Sweep.C.Y;
				body4.Sweep.A0 = body4.Sweep.A;
				body4.Sweep.C.X += step.dt * body4.LinearVelocityInternal.X;
				body4.Sweep.C.Y += step.dt * body4.LinearVelocityInternal.Y;
				body4.Sweep.A += step.dt * body4.AngularVelocityInternal;
				body4.SynchronizeTransform();
			}
		}
		for (int num9 = 0; num9 < Settings.PositionIterations; num9++)
		{
			bool flag = _contactSolver.SolvePositionConstraints(0.2f);
			bool flag2 = true;
			if (Settings.EnableDiagnostics)
			{
				_watch.Start();
			}
			for (int num10 = 0; num10 < JointCount; num10++)
			{
				Joint joint2 = _joints[num10];
				if (joint2.Enabled)
				{
					bool flag3 = joint2.SolvePositionConstraints();
					flag2 = flag2 && flag3;
				}
			}
			if (Settings.EnableDiagnostics)
			{
				_watch.Stop();
				_tmpTime += _watch.ElapsedTicks;
				_watch.Reset();
			}
			if (flag && flag2)
			{
				break;
			}
		}
		if (Settings.EnableDiagnostics)
		{
			JointUpdateTime = _tmpTime;
		}
		Report(_contactSolver.Constraints);
		if (!Settings.AllowSleep)
		{
			return;
		}
		float num11 = float.MaxValue;
		for (int num12 = 0; num12 < BodyCount; num12++)
		{
			Body body5 = Bodies[num12];
			if (body5.BodyType != BodyType.Static)
			{
				if ((body5.Flags & BodyFlags.AutoSleep) == 0)
				{
					body5.SleepTime = 0f;
					num11 = 0f;
				}
				if ((body5.Flags & BodyFlags.AutoSleep) == 0 || body5.AngularVelocityInternal * body5.AngularVelocityInternal > 0.0012184697f || Vector2.Dot(body5.LinearVelocityInternal, body5.LinearVelocityInternal) > 0.0001f)
				{
					body5.SleepTime = 0f;
					num11 = 0f;
				}
				else
				{
					body5.SleepTime += step.dt;
					num11 = Math.Min(num11, body5.SleepTime);
				}
			}
		}
		if (num11 >= 0.5f)
		{
			for (int num13 = 0; num13 < BodyCount; num13++)
			{
				Body body6 = Bodies[num13];
				body6.Awake = false;
			}
		}
	}

	internal void SolveTOI(ref TimeStep subStep)
	{
		_contactSolver.Reset(_contacts, ContactCount, subStep.dtRatio, warmstarting: false);
		for (int i = 0; i < Settings.TOIPositionIterations; i++)
		{
			if (_contactSolver.SolvePositionConstraints(0.75f))
			{
				break;
			}
			if (i == Settings.TOIPositionIterations - 1)
			{
				i = i;
			}
		}
		for (int j = 0; j < BodyCount; j++)
		{
			Body body = Bodies[j];
			body.Sweep.A0 = body.Sweep.A;
			body.Sweep.C0 = body.Sweep.C;
		}
		_contactSolver.InitializeVelocityConstraints();
		for (int k = 0; k < Settings.TOIVelocityIterations; k++)
		{
			_contactSolver.SolveVelocityConstraints();
		}
		for (int l = 0; l < BodyCount; l++)
		{
			Body body2 = Bodies[l];
			if (body2.BodyType == BodyType.Static)
			{
				continue;
			}
			float num = subStep.dt * body2.LinearVelocityInternal.X;
			float num2 = subStep.dt * body2.LinearVelocityInternal.Y;
			float num3 = num * num + num2 * num2;
			if (num3 > 4f)
			{
				float num4 = 1f / (float)Math.Sqrt(num3);
				float num5 = 2f * subStep.inv_dt;
				body2.LinearVelocityInternal.X = num5 * (num * num4);
				body2.LinearVelocityInternal.Y = num5 * (num2 * num4);
			}
			float num6 = subStep.dt * body2.AngularVelocity;
			if (num6 * num6 > 2.4674013f)
			{
				if ((double)num6 < 0.0)
				{
					body2.AngularVelocityInternal = (0f - subStep.inv_dt) * ((float)Math.PI / 2f);
				}
				else
				{
					body2.AngularVelocityInternal = subStep.inv_dt * ((float)Math.PI / 2f);
				}
			}
			body2.Sweep.C.X += subStep.dt * body2.LinearVelocityInternal.X;
			body2.Sweep.C.Y += subStep.dt * body2.LinearVelocityInternal.Y;
			body2.Sweep.A += subStep.dt * body2.AngularVelocityInternal;
			body2.SynchronizeTransform();
		}
		Report(_contactSolver.Constraints);
	}

	public void Add(Body body)
	{
		Bodies[BodyCount++] = body;
	}

	public void Add(Contact contact)
	{
		_contacts[ContactCount++] = contact;
	}

	public void Add(Joint joint)
	{
		_joints[JointCount++] = joint;
	}

	private void Report(ContactConstraint[] constraints)
	{
		if (_contactManager == null)
		{
			return;
		}
		for (int i = 0; i < ContactCount; i++)
		{
			Contact contact = _contacts[i];
			if (contact.FixtureA.AfterCollision != null)
			{
				contact.FixtureA.AfterCollision(contact.FixtureA, contact.FixtureB, contact);
			}
			if (contact.FixtureB.AfterCollision != null)
			{
				contact.FixtureB.AfterCollision(contact.FixtureB, contact.FixtureA, contact);
			}
			if (_contactManager.PostSolve != null)
			{
				ContactConstraint impulse = constraints[i];
				_contactManager.PostSolve(contact, impulse);
			}
		}
	}
}
