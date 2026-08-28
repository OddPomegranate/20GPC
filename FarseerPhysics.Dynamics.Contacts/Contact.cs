using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts;

public class Contact
{
	private enum ContactType
	{
		NotSupported,
		Polygon,
		PolygonAndCircle,
		Circle,
		EdgeAndPolygon,
		EdgeAndCircle,
		LoopAndPolygon,
		LoopAndCircle
	}

	private static EdgeShape _edge = new EdgeShape();

	private static ContactType[,] _registers = new ContactType[4, 4]
	{
		{
			ContactType.Circle,
			ContactType.EdgeAndCircle,
			ContactType.PolygonAndCircle,
			ContactType.LoopAndCircle
		},
		{
			ContactType.EdgeAndCircle,
			ContactType.NotSupported,
			ContactType.EdgeAndPolygon,
			ContactType.NotSupported
		},
		{
			ContactType.PolygonAndCircle,
			ContactType.EdgeAndPolygon,
			ContactType.Polygon,
			ContactType.LoopAndPolygon
		},
		{
			ContactType.LoopAndCircle,
			ContactType.NotSupported,
			ContactType.LoopAndPolygon,
			ContactType.NotSupported
		}
	};

	public Fixture FixtureA;

	public Fixture FixtureB;

	internal ContactFlags Flags;

	public Manifold Manifold;

	internal ContactEdge NodeA = new ContactEdge();

	internal ContactEdge NodeB = new ContactEdge();

	public float TOI;

	internal int TOICount;

	private ContactType _type;

	public bool Enabled
	{
		get
		{
			return (Flags & ContactFlags.Enabled) == ContactFlags.Enabled;
		}
		set
		{
			if (value)
			{
				Flags |= ContactFlags.Enabled;
			}
			else
			{
				Flags &= ~ContactFlags.Enabled;
			}
		}
	}

	public int ChildIndexA { get; internal set; }

	public int ChildIndexB { get; internal set; }

	private Contact(Fixture fA, int indexA, Fixture fB, int indexB)
	{
		Reset(fA, indexA, fB, indexB);
	}

	public void GetManifold(out Manifold manifold)
	{
		manifold = Manifold;
	}

	public void GetWorldManifold(out Vector2 normal, out FixedArray2<Vector2> points)
	{
		Body body = FixtureA.Body;
		Body body2 = FixtureB.Body;
		Shape shape = FixtureA.Shape;
		Shape shape2 = FixtureB.Shape;
		FarseerPhysics.Collision.Collision.GetWorldManifold(ref Manifold, ref body.Xf, shape.Radius, ref body2.Xf, shape2.Radius, out normal, out points);
	}

	public bool IsTouching()
	{
		return (Flags & ContactFlags.Touching) == ContactFlags.Touching;
	}

	public void FlagForFiltering()
	{
		Flags |= ContactFlags.Filter;
	}

	private void Reset(Fixture fA, int indexA, Fixture fB, int indexB)
	{
		Flags = ContactFlags.Enabled;
		FixtureA = fA;
		FixtureB = fB;
		ChildIndexA = indexA;
		ChildIndexB = indexB;
		Manifold.PointCount = 0;
		NodeA.Contact = null;
		NodeA.Prev = null;
		NodeA.Next = null;
		NodeA.Other = null;
		NodeB.Contact = null;
		NodeB.Prev = null;
		NodeB.Next = null;
		NodeB.Other = null;
		TOICount = 0;
	}

	internal void Update(ContactManager contactManager)
	{
		Manifold oldManifold = Manifold;
		Flags |= ContactFlags.Enabled;
		bool flag = (Flags & ContactFlags.Touching) == ContactFlags.Touching;
		bool flag2 = FixtureA.IsSensor || FixtureB.IsSensor;
		Body body = FixtureA.Body;
		Body body2 = FixtureB.Body;
		bool flag3;
		if (flag2)
		{
			Shape shape = FixtureA.Shape;
			Shape shape2 = FixtureB.Shape;
			flag3 = AABB.TestOverlap(shape, ChildIndexA, shape2, ChildIndexB, ref body.Xf, ref body2.Xf);
			Manifold.PointCount = 0;
		}
		else
		{
			Evaluate(ref Manifold, ref body.Xf, ref body2.Xf);
			flag3 = Manifold.PointCount > 0;
			for (int i = 0; i < Manifold.PointCount; i++)
			{
				ManifoldPoint value = Manifold.Points[i];
				value.NormalImpulse = 0f;
				value.TangentImpulse = 0f;
				ContactID id = value.Id;
				bool flag4 = false;
				for (int j = 0; j < oldManifold.PointCount; j++)
				{
					ManifoldPoint manifoldPoint = oldManifold.Points[j];
					if (manifoldPoint.Id.Key == id.Key)
					{
						value.NormalImpulse = manifoldPoint.NormalImpulse;
						value.TangentImpulse = manifoldPoint.TangentImpulse;
						flag4 = true;
						break;
					}
				}
				if (!flag4)
				{
					value.NormalImpulse = 0f;
					value.TangentImpulse = 0f;
				}
				Manifold.Points[i] = value;
			}
			if (flag3 != flag)
			{
				body.Awake = true;
				body2.Awake = true;
			}
		}
		if (flag3)
		{
			Flags |= ContactFlags.Touching;
		}
		else
		{
			Flags &= ~ContactFlags.Touching;
		}
		if (!flag && flag3)
		{
			if (FixtureA.OnCollision != null)
			{
				Enabled = FixtureA.OnCollision(FixtureA, FixtureB, this);
			}
			if (FixtureB.OnCollision != null)
			{
				Enabled = FixtureB.OnCollision(FixtureB, FixtureA, this);
			}
			if (contactManager.BeginContact != null)
			{
				Enabled = contactManager.BeginContact(this);
			}
			if (!Enabled)
			{
				Flags &= ~ContactFlags.Touching;
			}
		}
		if (flag && !flag3)
		{
			if (FixtureA != null && FixtureA.OnSeparation != null)
			{
				FixtureA.OnSeparation(FixtureA, FixtureB);
			}
			if (FixtureB != null && FixtureB.OnSeparation != null)
			{
				FixtureB.OnSeparation(FixtureB, FixtureA);
			}
			if (contactManager.EndContact != null)
			{
				contactManager.EndContact(this);
			}
		}
		if (!flag2 && contactManager.PreSolve != null)
		{
			contactManager.PreSolve(this, ref oldManifold);
		}
	}

	private void Evaluate(ref Manifold manifold, ref Transform transformA, ref Transform transformB)
	{
		switch (_type)
		{
		case ContactType.Polygon:
			FarseerPhysics.Collision.Collision.CollidePolygons(ref manifold, (PolygonShape)FixtureA.Shape, ref transformA, (PolygonShape)FixtureB.Shape, ref transformB);
			break;
		case ContactType.PolygonAndCircle:
			FarseerPhysics.Collision.Collision.CollidePolygonAndCircle(ref manifold, (PolygonShape)FixtureA.Shape, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
			break;
		case ContactType.EdgeAndCircle:
			FarseerPhysics.Collision.Collision.CollideEdgeAndCircle(ref manifold, (EdgeShape)FixtureA.Shape, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
			break;
		case ContactType.EdgeAndPolygon:
			FarseerPhysics.Collision.Collision.CollideEdgeAndPolygon(ref manifold, (EdgeShape)FixtureA.Shape, ref transformA, (PolygonShape)FixtureB.Shape, ref transformB);
			break;
		case ContactType.LoopAndCircle:
		{
			LoopShape loopShape2 = (LoopShape)FixtureA.Shape;
			loopShape2.GetChildEdge(ref _edge, ChildIndexA);
			FarseerPhysics.Collision.Collision.CollideEdgeAndCircle(ref manifold, _edge, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
			break;
		}
		case ContactType.LoopAndPolygon:
		{
			LoopShape loopShape = (LoopShape)FixtureA.Shape;
			loopShape.GetChildEdge(ref _edge, ChildIndexA);
			FarseerPhysics.Collision.Collision.CollideEdgeAndPolygon(ref manifold, _edge, ref transformA, (PolygonShape)FixtureB.Shape, ref transformB);
			break;
		}
		case ContactType.Circle:
			FarseerPhysics.Collision.Collision.CollideCircles(ref manifold, (CircleShape)FixtureA.Shape, ref transformA, (CircleShape)FixtureB.Shape, ref transformB);
			break;
		}
	}

	internal static Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
	{
		ShapeType shapeType = fixtureA.ShapeType;
		ShapeType shapeType2 = fixtureB.ShapeType;
		Queue<Contact> contactPool = fixtureA.Body.World.ContactPool;
		Contact contact;
		if (contactPool.Count <= 0)
		{
			contact = (((shapeType < shapeType2 && (shapeType != ShapeType.Edge || shapeType2 != ShapeType.Polygon)) || (shapeType2 == ShapeType.Edge && shapeType == ShapeType.Polygon)) ? new Contact(fixtureB, indexB, fixtureA, indexA) : new Contact(fixtureA, indexA, fixtureB, indexB));
		}
		else
		{
			contact = contactPool.Dequeue();
			if ((shapeType >= shapeType2 || (shapeType == ShapeType.Edge && shapeType2 == ShapeType.Polygon)) && (shapeType2 != ShapeType.Edge || shapeType != ShapeType.Polygon))
			{
				contact.Reset(fixtureA, indexA, fixtureB, indexB);
			}
			else
			{
				contact.Reset(fixtureB, indexB, fixtureA, indexA);
			}
		}
		contact._type = _registers[(int)shapeType, (int)shapeType2];
		return contact;
	}

	internal void Destroy()
	{
		FixtureA.Body.World.ContactPool.Enqueue(this);
		Reset(null, 0, null, 0);
	}
}
