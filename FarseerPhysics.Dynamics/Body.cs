using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics;

public class Body : IDisposable
{
	private static int _bodyIdCounter;

	internal float AngularVelocityInternal;

	public int BodyId;

	public ControllerFilter ControllerFilter;

	internal BodyFlags Flags;

	internal Vector2 Force;

	internal float InvI;

	internal float InvMass;

	internal Vector2 LinearVelocityInternal;

	public PhysicsLogicFilter PhysicsLogicFilter;

	internal float SleepTime;

	internal Sweep Sweep;

	internal float Torque;

	internal World World;

	internal Transform Xf;

	private float _angularDamping;

	private BodyType _bodyType;

	private float _inertia;

	private float _linearDamping;

	private float _mass;

	public float Revolutions => Rotation / (float)Math.PI;

	public BodyType BodyType
	{
		get
		{
			return _bodyType;
		}
		set
		{
			if (_bodyType != value)
			{
				_bodyType = value;
				ResetMassData();
				if (_bodyType == BodyType.Static)
				{
					LinearVelocityInternal = Vector2.Zero;
					AngularVelocityInternal = 0f;
				}
				Awake = true;
				Force = Vector2.Zero;
				Torque = 0f;
				for (int i = 0; i < FixtureList.Count; i++)
				{
					Fixture fixture = FixtureList[i];
					fixture.Refilter();
				}
			}
		}
	}

	public Vector2 LinearVelocity
	{
		get
		{
			return LinearVelocityInternal;
		}
		set
		{
			if (_bodyType != BodyType.Static)
			{
				if (Vector2.Dot(value, value) > 0f)
				{
					Awake = true;
				}
				LinearVelocityInternal = value;
			}
		}
	}

	public float AngularVelocity
	{
		get
		{
			return AngularVelocityInternal;
		}
		set
		{
			if (_bodyType != BodyType.Static)
			{
				if (value * value > 0f)
				{
					Awake = true;
				}
				AngularVelocityInternal = value;
			}
		}
	}

	public float LinearDamping
	{
		get
		{
			return _linearDamping;
		}
		set
		{
			_linearDamping = value;
		}
	}

	public float AngularDamping
	{
		get
		{
			return _angularDamping;
		}
		set
		{
			_angularDamping = value;
		}
	}

	public bool IsBullet
	{
		get
		{
			return (Flags & BodyFlags.Bullet) == BodyFlags.Bullet;
		}
		set
		{
			if (value)
			{
				Flags |= BodyFlags.Bullet;
			}
			else
			{
				Flags &= ~BodyFlags.Bullet;
			}
		}
	}

	public bool SleepingAllowed
	{
		get
		{
			return (Flags & BodyFlags.AutoSleep) == BodyFlags.AutoSleep;
		}
		set
		{
			if (value)
			{
				Flags |= BodyFlags.AutoSleep;
				return;
			}
			Flags &= ~BodyFlags.AutoSleep;
			Awake = true;
		}
	}

	public bool Awake
	{
		get
		{
			return (Flags & BodyFlags.Awake) == BodyFlags.Awake;
		}
		set
		{
			if (value)
			{
				if ((Flags & BodyFlags.Awake) == 0)
				{
					Flags |= BodyFlags.Awake;
					SleepTime = 0f;
				}
			}
			else
			{
				Flags &= ~BodyFlags.Awake;
				SleepTime = 0f;
				LinearVelocityInternal = Vector2.Zero;
				AngularVelocityInternal = 0f;
				Force = Vector2.Zero;
				Torque = 0f;
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return (Flags & BodyFlags.Enabled) == BodyFlags.Enabled;
		}
		set
		{
			if (value == Enabled)
			{
				return;
			}
			if (value)
			{
				Flags |= BodyFlags.Enabled;
				IBroadPhase broadPhase = World.ContactManager.BroadPhase;
				for (int i = 0; i < FixtureList.Count; i++)
				{
					FixtureList[i].CreateProxies(broadPhase, ref Xf);
				}
				return;
			}
			Flags &= ~BodyFlags.Enabled;
			IBroadPhase broadPhase2 = World.ContactManager.BroadPhase;
			for (int j = 0; j < FixtureList.Count; j++)
			{
				FixtureList[j].DestroyProxies(broadPhase2);
			}
			ContactEdge contactEdge = ContactList;
			while (contactEdge != null)
			{
				ContactEdge contactEdge2 = contactEdge;
				contactEdge = contactEdge.Next;
				World.ContactManager.Destroy(contactEdge2.Contact);
			}
			ContactList = null;
		}
	}

	public bool FixedRotation
	{
		get
		{
			return (Flags & BodyFlags.FixedRotation) == BodyFlags.FixedRotation;
		}
		set
		{
			if (value)
			{
				Flags |= BodyFlags.FixedRotation;
			}
			else
			{
				Flags &= ~BodyFlags.FixedRotation;
			}
			ResetMassData();
		}
	}

	public List<Fixture> FixtureList { get; internal set; }

	public JointEdge JointList { get; internal set; }

	public ContactEdge ContactList { get; internal set; }

	public object UserData { get; set; }

	public Vector2 Position
	{
		get
		{
			return Xf.Position;
		}
		set
		{
			SetTransform(ref value, Rotation);
		}
	}

	public float Rotation
	{
		get
		{
			return Sweep.A;
		}
		set
		{
			SetTransform(ref Xf.Position, value);
		}
	}

	public bool IsStatic
	{
		get
		{
			return _bodyType == BodyType.Static;
		}
		set
		{
			if (value)
			{
				BodyType = BodyType.Static;
			}
			else
			{
				BodyType = BodyType.Dynamic;
			}
		}
	}

	public bool IgnoreGravity
	{
		get
		{
			return (Flags & BodyFlags.IgnoreGravity) == BodyFlags.IgnoreGravity;
		}
		set
		{
			if (value)
			{
				Flags |= BodyFlags.IgnoreGravity;
			}
			else
			{
				Flags &= ~BodyFlags.IgnoreGravity;
			}
		}
	}

	public Vector2 WorldCenter => Sweep.C;

	public Vector2 LocalCenter
	{
		get
		{
			return Sweep.LocalCenter;
		}
		set
		{
			if (_bodyType == BodyType.Dynamic)
			{
				Vector2 c = Sweep.C;
				Sweep.LocalCenter = value;
				Sweep.C0 = (Sweep.C = MathUtils.Multiply(ref Xf, ref Sweep.LocalCenter));
				Vector2 vector = Sweep.C - c;
				LinearVelocityInternal += new Vector2((0f - AngularVelocityInternal) * vector.Y, AngularVelocityInternal * vector.X);
			}
		}
	}

	public float Mass
	{
		get
		{
			return _mass;
		}
		set
		{
			if (_bodyType == BodyType.Dynamic)
			{
				_mass = value;
				if (_mass <= 0f)
				{
					_mass = 1f;
				}
				InvMass = 1f / _mass;
			}
		}
	}

	public float Inertia
	{
		get
		{
			return _inertia + Mass * Vector2.Dot(Sweep.LocalCenter, Sweep.LocalCenter);
		}
		set
		{
			if (_bodyType == BodyType.Dynamic && value > 0f && (Flags & BodyFlags.FixedRotation) == 0)
			{
				_inertia = value - Mass * Vector2.Dot(LocalCenter, LocalCenter);
				InvI = 1f / _inertia;
			}
		}
	}

	public float Restitution
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				num += fixture.Restitution;
			}
			return num / (float)FixtureList.Count;
		}
		set
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.Restitution = value;
			}
		}
	}

	public float Friction
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				num += fixture.Friction;
			}
			return num / (float)FixtureList.Count;
		}
		set
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.Friction = value;
			}
		}
	}

	public Category CollisionCategories
	{
		set
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.CollisionCategories = value;
			}
		}
	}

	public Category CollidesWith
	{
		set
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.CollidesWith = value;
			}
		}
	}

	public short CollisionGroup
	{
		set
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.CollisionGroup = value;
			}
		}
	}

	public bool IsSensor
	{
		set
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.IsSensor = value;
			}
		}
	}

	public bool IgnoreCCD
	{
		get
		{
			return (Flags & BodyFlags.IgnoreCCD) == BodyFlags.IgnoreCCD;
		}
		set
		{
			if (value)
			{
				Flags |= BodyFlags.IgnoreCCD;
			}
			else
			{
				Flags &= ~BodyFlags.IgnoreCCD;
			}
		}
	}

	public bool IsDisposed { get; set; }

	public event OnCollisionEventHandler OnCollision
	{
		add
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.OnCollision = (OnCollisionEventHandler)Delegate.Combine(fixture.OnCollision, value);
			}
		}
		remove
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.OnCollision = (OnCollisionEventHandler)Delegate.Remove(fixture.OnCollision, value);
			}
		}
	}

	public event OnSeparationEventHandler OnSeparation
	{
		add
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.OnSeparation = (OnSeparationEventHandler)Delegate.Combine(fixture.OnSeparation, value);
			}
		}
		remove
		{
			for (int i = 0; i < FixtureList.Count; i++)
			{
				Fixture fixture = FixtureList[i];
				fixture.OnSeparation = (OnSeparationEventHandler)Delegate.Remove(fixture.OnSeparation, value);
			}
		}
	}

	internal Body()
	{
		FixtureList = new List<Fixture>(32);
	}

	public Body(World world)
		: this(world, null)
	{
	}

	public Body(World world, object userData)
	{
		FixtureList = new List<Fixture>(32);
		BodyId = _bodyIdCounter++;
		World = world;
		UserData = userData;
		FixedRotation = false;
		IsBullet = false;
		SleepingAllowed = true;
		Awake = true;
		BodyType = BodyType.Static;
		Enabled = true;
		Xf.R.Set(0f);
		world.AddBody(this);
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			World.RemoveBody(this);
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	public void ResetDynamics()
	{
		Torque = 0f;
		AngularVelocityInternal = 0f;
		Force = Vector2.Zero;
		LinearVelocityInternal = Vector2.Zero;
	}

	public Fixture CreateFixture(Shape shape)
	{
		return new Fixture(this, shape);
	}

	public Fixture CreateFixture(Shape shape, object userData)
	{
		return new Fixture(this, shape, userData);
	}

	public void DestroyFixture(Fixture fixture)
	{
		ContactEdge contactEdge = ContactList;
		while (contactEdge != null)
		{
			Contact contact = contactEdge.Contact;
			contactEdge = contactEdge.Next;
			Fixture fixtureA = contact.FixtureA;
			Fixture fixtureB = contact.FixtureB;
			if (fixture == fixtureA || fixture == fixtureB)
			{
				World.ContactManager.Destroy(contact);
			}
		}
		if ((Flags & BodyFlags.Enabled) == BodyFlags.Enabled)
		{
			IBroadPhase broadPhase = World.ContactManager.BroadPhase;
			fixture.DestroyProxies(broadPhase);
		}
		FixtureList.Remove(fixture);
		fixture.Destroy();
		fixture.Body = null;
		ResetMassData();
	}

	public void SetTransform(ref Vector2 position, float rotation)
	{
		SetTransformIgnoreContacts(ref position, rotation);
		World.ContactManager.FindNewContacts();
	}

	public void SetTransform(Vector2 position, float rotation)
	{
		SetTransform(ref position, rotation);
	}

	public void SetTransformIgnoreContacts(ref Vector2 position, float angle)
	{
		Xf.R.Set(angle);
		Xf.Position = position;
		Sweep.C0 = (Sweep.C = new Vector2(Xf.Position.X + Xf.R.Col1.X * Sweep.LocalCenter.X + Xf.R.Col2.X * Sweep.LocalCenter.Y, Xf.Position.Y + Xf.R.Col1.Y * Sweep.LocalCenter.X + Xf.R.Col2.Y * Sweep.LocalCenter.Y));
		Sweep.A0 = (Sweep.A = angle);
		IBroadPhase broadPhase = World.ContactManager.BroadPhase;
		for (int i = 0; i < FixtureList.Count; i++)
		{
			FixtureList[i].Synchronize(broadPhase, ref Xf, ref Xf);
		}
	}

	public void GetTransform(out Transform transform)
	{
		transform = Xf;
	}

	public void ApplyForce(Vector2 force, Vector2 point)
	{
		ApplyForce(ref force, ref point);
	}

	public void ApplyForce(ref Vector2 force)
	{
		ApplyForce(ref force, ref Xf.Position);
	}

	public void ApplyForce(Vector2 force)
	{
		ApplyForce(ref force, ref Xf.Position);
	}

	public void ApplyForce(ref Vector2 force, ref Vector2 point)
	{
		if (_bodyType == BodyType.Dynamic)
		{
			if (!Awake)
			{
				Awake = true;
			}
			Force += force;
			Torque += (point.X - Sweep.C.X) * force.Y - (point.Y - Sweep.C.Y) * force.X;
		}
	}

	public void ApplyTorque(float torque)
	{
		if (_bodyType == BodyType.Dynamic)
		{
			if (!Awake)
			{
				Awake = true;
			}
			Torque += torque;
		}
	}

	public void ApplyLinearImpulse(Vector2 impulse)
	{
		ApplyLinearImpulse(ref impulse);
	}

	public void ApplyLinearImpulse(Vector2 impulse, Vector2 point)
	{
		ApplyLinearImpulse(ref impulse, ref point);
	}

	public void ApplyLinearImpulse(ref Vector2 impulse)
	{
		if (_bodyType == BodyType.Dynamic)
		{
			if (!Awake)
			{
				Awake = true;
			}
			LinearVelocityInternal += InvMass * impulse;
		}
	}

	public void ApplyLinearImpulse(ref Vector2 impulse, ref Vector2 point)
	{
		if (_bodyType == BodyType.Dynamic)
		{
			if (!Awake)
			{
				Awake = true;
			}
			LinearVelocityInternal += InvMass * impulse;
			AngularVelocityInternal += InvI * ((point.X - Sweep.C.X) * impulse.Y - (point.Y - Sweep.C.Y) * impulse.X);
		}
	}

	public void ApplyAngularImpulse(float impulse)
	{
		if (_bodyType == BodyType.Dynamic)
		{
			if (!Awake)
			{
				Awake = true;
			}
			AngularVelocityInternal += InvI * impulse;
		}
	}

	public void ResetMassData()
	{
		_mass = 0f;
		InvMass = 0f;
		_inertia = 0f;
		InvI = 0f;
		Sweep.LocalCenter = Vector2.Zero;
		if (BodyType == BodyType.Kinematic)
		{
			Sweep.C0 = (Sweep.C = Xf.Position);
			return;
		}
		Vector2 zero = Vector2.Zero;
		foreach (Fixture fixture in FixtureList)
		{
			if (fixture.Shape._density != 0f)
			{
				MassData massData = fixture.Shape.MassData;
				_mass += massData.Mass;
				zero += massData.Mass * massData.Centroid;
				_inertia += massData.Inertia;
			}
		}
		if (BodyType == BodyType.Static)
		{
			Sweep.C0 = (Sweep.C = Xf.Position);
			return;
		}
		if (_mass > 0f)
		{
			InvMass = 1f / _mass;
			zero *= InvMass;
		}
		else
		{
			_mass = 1f;
			InvMass = 1f;
		}
		if (_inertia > 0f && (Flags & BodyFlags.FixedRotation) == 0)
		{
			_inertia -= _mass * Vector2.Dot(zero, zero);
			InvI = 1f / _inertia;
		}
		else
		{
			_inertia = 0f;
			InvI = 0f;
		}
		Vector2 c = Sweep.C;
		Sweep.LocalCenter = zero;
		Sweep.C0 = (Sweep.C = MathUtils.Multiply(ref Xf, ref Sweep.LocalCenter));
		Vector2 vector = Sweep.C - c;
		LinearVelocityInternal += new Vector2((0f - AngularVelocityInternal) * vector.Y, AngularVelocityInternal * vector.X);
	}

	public Vector2 GetWorldPoint(ref Vector2 localPoint)
	{
		return new Vector2(Xf.Position.X + Xf.R.Col1.X * localPoint.X + Xf.R.Col2.X * localPoint.Y, Xf.Position.Y + Xf.R.Col1.Y * localPoint.X + Xf.R.Col2.Y * localPoint.Y);
	}

	public Vector2 GetWorldPoint(Vector2 localPoint)
	{
		return GetWorldPoint(ref localPoint);
	}

	public Vector2 GetWorldVector(ref Vector2 localVector)
	{
		return new Vector2(Xf.R.Col1.X * localVector.X + Xf.R.Col2.X * localVector.Y, Xf.R.Col1.Y * localVector.X + Xf.R.Col2.Y * localVector.Y);
	}

	public Vector2 GetWorldVector(Vector2 localVector)
	{
		return GetWorldVector(ref localVector);
	}

	public Vector2 GetLocalPoint(ref Vector2 worldPoint)
	{
		return new Vector2((worldPoint.X - Xf.Position.X) * Xf.R.Col1.X + (worldPoint.Y - Xf.Position.Y) * Xf.R.Col1.Y, (worldPoint.X - Xf.Position.X) * Xf.R.Col2.X + (worldPoint.Y - Xf.Position.Y) * Xf.R.Col2.Y);
	}

	public Vector2 GetLocalPoint(Vector2 worldPoint)
	{
		return GetLocalPoint(ref worldPoint);
	}

	public Vector2 GetLocalVector(ref Vector2 worldVector)
	{
		return new Vector2(worldVector.X * Xf.R.Col1.X + worldVector.Y * Xf.R.Col1.Y, worldVector.X * Xf.R.Col2.X + worldVector.Y * Xf.R.Col2.Y);
	}

	public Vector2 GetLocalVector(Vector2 worldVector)
	{
		return GetLocalVector(ref worldVector);
	}

	public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint)
	{
		return GetLinearVelocityFromWorldPoint(ref worldPoint);
	}

	public Vector2 GetLinearVelocityFromWorldPoint(ref Vector2 worldPoint)
	{
		return LinearVelocityInternal + new Vector2((0f - AngularVelocityInternal) * (worldPoint.Y - Sweep.C.Y), AngularVelocityInternal * (worldPoint.X - Sweep.C.X));
	}

	public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint)
	{
		return GetLinearVelocityFromLocalPoint(ref localPoint);
	}

	public Vector2 GetLinearVelocityFromLocalPoint(ref Vector2 localPoint)
	{
		return GetLinearVelocityFromWorldPoint(GetWorldPoint(ref localPoint));
	}

	public Body DeepClone()
	{
		Body body = Clone();
		for (int i = 0; i < FixtureList.Count; i++)
		{
			FixtureList[i].Clone(body);
		}
		return body;
	}

	public Body Clone()
	{
		Body body = new Body();
		body.World = World;
		body.UserData = UserData;
		body.LinearDamping = LinearDamping;
		body.LinearVelocityInternal = LinearVelocityInternal;
		body.AngularDamping = AngularDamping;
		body.AngularVelocityInternal = AngularVelocityInternal;
		body.Position = Position;
		body.Rotation = Rotation;
		body._bodyType = _bodyType;
		body.Flags = Flags;
		World.AddBody(body);
		return body;
	}

	internal void SynchronizeFixtures()
	{
		Transform transform = default(Transform);
		float num = (float)Math.Cos(Sweep.A0);
		float num2 = (float)Math.Sin(Sweep.A0);
		transform.R.Col1.X = num;
		transform.R.Col2.X = 0f - num2;
		transform.R.Col1.Y = num2;
		transform.R.Col2.Y = num;
		transform.Position.X = Sweep.C0.X - (transform.R.Col1.X * Sweep.LocalCenter.X + transform.R.Col2.X * Sweep.LocalCenter.Y);
		transform.Position.Y = Sweep.C0.Y - (transform.R.Col1.Y * Sweep.LocalCenter.X + transform.R.Col2.Y * Sweep.LocalCenter.Y);
		IBroadPhase broadPhase = World.ContactManager.BroadPhase;
		for (int i = 0; i < FixtureList.Count; i++)
		{
			FixtureList[i].Synchronize(broadPhase, ref transform, ref Xf);
		}
	}

	internal void SynchronizeTransform()
	{
		Xf.R.Set(Sweep.A);
		float num = Xf.R.Col1.X * Sweep.LocalCenter.X + Xf.R.Col2.X * Sweep.LocalCenter.Y;
		float num2 = Xf.R.Col1.Y * Sweep.LocalCenter.X + Xf.R.Col2.Y * Sweep.LocalCenter.Y;
		Xf.Position.X = Sweep.C.X - num;
		Xf.Position.Y = Sweep.C.Y - num2;
	}

	internal bool ShouldCollide(Body other)
	{
		if (_bodyType != BodyType.Dynamic && other._bodyType != BodyType.Dynamic)
		{
			return false;
		}
		for (JointEdge jointEdge = JointList; jointEdge != null; jointEdge = jointEdge.Next)
		{
			if (jointEdge.Other == other && !jointEdge.Joint.CollideConnected)
			{
				return false;
			}
		}
		return true;
	}

	internal void Advance(float alpha)
	{
		Sweep.Advance(alpha);
		Sweep.C = Sweep.C0;
		Sweep.A = Sweep.A0;
		SynchronizeTransform();
	}

	public void IgnoreCollisionWith(Body other)
	{
		for (int i = 0; i < FixtureList.Count; i++)
		{
			Fixture fixture = FixtureList[i];
			for (int j = 0; j < other.FixtureList.Count; j++)
			{
				Fixture fixture2 = other.FixtureList[j];
				fixture.IgnoreCollisionWith(fixture2);
			}
		}
	}

	public void RestoreCollisionWith(Body other)
	{
		for (int i = 0; i < FixtureList.Count; i++)
		{
			Fixture fixture = FixtureList[i];
			for (int j = 0; j < other.FixtureList.Count; j++)
			{
				Fixture fixture2 = other.FixtureList[j];
				fixture.RestoreCollisionWith(fixture2);
			}
		}
	}
}
