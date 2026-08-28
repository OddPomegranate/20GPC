using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics;

public class Fixture : IDisposable
{
	private static int _fixtureIdCounter;

	public AfterCollisionEventHandler AfterCollision;

	public BeforeCollisionEventHandler BeforeCollision;

	public OnCollisionEventHandler OnCollision;

	public OnSeparationEventHandler OnSeparation;

	public FixtureProxy[] Proxies;

	public int ProxyCount;

	internal Category _collidesWith;

	internal Category _collisionCategories;

	internal short _collisionGroup;

	internal Dictionary<int, bool> _collisionIgnores;

	private float _friction;

	private float _restitution;

	public short CollisionGroup
	{
		get
		{
			return _collisionGroup;
		}
		set
		{
			if (_collisionGroup != value)
			{
				_collisionGroup = value;
				Refilter();
			}
		}
	}

	public Category CollidesWith
	{
		get
		{
			return _collidesWith;
		}
		set
		{
			if (_collidesWith != value)
			{
				_collidesWith = value;
				Refilter();
			}
		}
	}

	public Category CollisionCategories
	{
		get
		{
			return _collisionCategories;
		}
		set
		{
			if (_collisionCategories != value)
			{
				_collisionCategories = value;
				Refilter();
			}
		}
	}

	public ShapeType ShapeType => Shape.ShapeType;

	public Shape Shape { get; internal set; }

	public bool IsSensor { get; set; }

	public Body Body { get; internal set; }

	public object UserData { get; set; }

	public float Friction
	{
		get
		{
			return _friction;
		}
		set
		{
			_friction = value;
		}
	}

	public float Restitution
	{
		get
		{
			return _restitution;
		}
		set
		{
			_restitution = value;
		}
	}

	public int FixtureId { get; private set; }

	public bool IsDisposed { get; set; }

	internal Fixture()
	{
	}

	public Fixture(Body body, Shape shape)
		: this(body, shape, null)
	{
	}

	public Fixture(Body body, Shape shape, object userData)
	{
		if (Settings.UseFPECollisionCategories)
		{
			_collisionCategories = Category.All;
		}
		else
		{
			_collisionCategories = Category.Cat1;
		}
		_collidesWith = Category.All;
		_collisionGroup = 0;
		Friction = 0.2f;
		Restitution = 0f;
		IsSensor = false;
		Body = body;
		UserData = userData;
		Shape = shape.Clone();
		RegisterFixture();
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			Body.DestroyFixture(this);
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	public void RestoreCollisionWith(Fixture fixture)
	{
		if (_collisionIgnores != null && _collisionIgnores.ContainsKey(fixture.FixtureId))
		{
			_collisionIgnores[fixture.FixtureId] = false;
			Refilter();
		}
	}

	public void IgnoreCollisionWith(Fixture fixture)
	{
		if (_collisionIgnores == null)
		{
			_collisionIgnores = new Dictionary<int, bool>();
		}
		if (_collisionIgnores.ContainsKey(fixture.FixtureId))
		{
			_collisionIgnores[fixture.FixtureId] = true;
		}
		else
		{
			_collisionIgnores.Add(fixture.FixtureId, value: true);
		}
		Refilter();
	}

	public bool IsFixtureIgnored(Fixture fixture)
	{
		if (_collisionIgnores == null)
		{
			return false;
		}
		if (_collisionIgnores.ContainsKey(fixture.FixtureId))
		{
			return _collisionIgnores[fixture.FixtureId];
		}
		return false;
	}

	internal void Refilter()
	{
		for (ContactEdge contactEdge = Body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
		{
			Contact contact = contactEdge.Contact;
			Fixture fixtureA = contact.FixtureA;
			Fixture fixtureB = contact.FixtureB;
			if (fixtureA == this || fixtureB == this)
			{
				contact.FlagForFiltering();
			}
		}
		World world = Body.World;
		if (world != null)
		{
			IBroadPhase broadPhase = world.ContactManager.BroadPhase;
			for (int i = 0; i < ProxyCount; i++)
			{
				broadPhase.TouchProxy(Proxies[i].ProxyId);
			}
		}
	}

	private void RegisterFixture()
	{
		Proxies = new FixtureProxy[Shape.ChildCount];
		ProxyCount = 0;
		FixtureId = _fixtureIdCounter++;
		if ((Body.Flags & BodyFlags.Enabled) == BodyFlags.Enabled)
		{
			IBroadPhase broadPhase = Body.World.ContactManager.BroadPhase;
			CreateProxies(broadPhase, ref Body.Xf);
		}
		Body.FixtureList.Add(this);
		if (Shape._density > 0f)
		{
			Body.ResetMassData();
		}
		Body.World.Flags |= WorldFlags.NewFixture;
		if (Body.World.FixtureAdded != null)
		{
			Body.World.FixtureAdded(this);
		}
	}

	public bool TestPoint(ref Vector2 point)
	{
		return Shape.TestPoint(ref Body.Xf, ref point);
	}

	public bool RayCast(out RayCastOutput output, ref RayCastInput input, int childIndex)
	{
		return Shape.RayCast(out output, ref input, ref Body.Xf, childIndex);
	}

	public void GetAABB(out AABB aabb, int childIndex)
	{
		aabb = Proxies[childIndex].AABB;
	}

	public Fixture Clone(Body body)
	{
		Fixture fixture = new Fixture();
		fixture.Body = body;
		fixture.Shape = Shape.Clone();
		fixture.UserData = UserData;
		fixture.Restitution = Restitution;
		fixture.Friction = Friction;
		fixture.IsSensor = IsSensor;
		fixture._collisionGroup = CollisionGroup;
		fixture._collisionCategories = CollisionCategories;
		fixture._collidesWith = CollidesWith;
		if (_collisionIgnores != null)
		{
			fixture._collisionIgnores = new Dictionary<int, bool>();
			foreach (KeyValuePair<int, bool> collisionIgnore in _collisionIgnores)
			{
				fixture._collisionIgnores.Add(collisionIgnore.Key, collisionIgnore.Value);
			}
		}
		fixture.RegisterFixture();
		return fixture;
	}

	public Fixture DeepClone()
	{
		return Clone(Body.Clone());
	}

	internal void Destroy()
	{
		Proxies = null;
		Shape = null;
		BeforeCollision = null;
		OnCollision = null;
		OnSeparation = null;
		AfterCollision = null;
		if (Body.World.FixtureRemoved != null)
		{
			Body.World.FixtureRemoved(this);
		}
		Body.World.FixtureAdded = null;
		Body.World.FixtureRemoved = null;
		OnSeparation = null;
		OnCollision = null;
	}

	internal void CreateProxies(IBroadPhase broadPhase, ref Transform xf)
	{
		ProxyCount = Shape.ChildCount;
		for (int i = 0; i < ProxyCount; i++)
		{
			FixtureProxy proxy = default(FixtureProxy);
			Shape.ComputeAABB(out proxy.AABB, ref xf, i);
			proxy.Fixture = this;
			proxy.ChildIndex = i;
			proxy.ProxyId = broadPhase.AddProxy(ref proxy);
			Proxies[i] = proxy;
		}
	}

	internal void DestroyProxies(IBroadPhase broadPhase)
	{
		for (int i = 0; i < ProxyCount; i++)
		{
			broadPhase.RemoveProxy(Proxies[i].ProxyId);
			Proxies[i].ProxyId = -1;
		}
		ProxyCount = 0;
	}

	internal void Synchronize(IBroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
	{
		if (ProxyCount != 0)
		{
			for (int i = 0; i < ProxyCount; i++)
			{
				FixtureProxy fixtureProxy = Proxies[i];
				Shape.ComputeAABB(out var aabb, ref transform1, fixtureProxy.ChildIndex);
				Shape.ComputeAABB(out var aabb2, ref transform2, fixtureProxy.ChildIndex);
				fixtureProxy.AABB.Combine(ref aabb, ref aabb2);
				Vector2 displacement = transform2.Position - transform1.Position;
				broadPhase.MoveProxy(fixtureProxy.ProxyId, ref fixtureProxy.AABB, displacement);
			}
		}
	}

	internal bool CompareTo(Fixture fixture)
	{
		if (CollidesWith == fixture.CollidesWith && CollisionCategories == fixture.CollisionCategories && CollisionGroup == fixture.CollisionGroup && Friction == fixture.Friction && IsSensor == fixture.IsSensor && Restitution == fixture.Restitution && Shape.CompareTo(fixture.Shape))
		{
			return UserData == fixture.UserData;
		}
		return false;
	}
}
