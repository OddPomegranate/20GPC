using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts;

public sealed class ContactConstraint
{
	public Body BodyA;

	public Body BodyB;

	public float Friction;

	public Mat22 K;

	public Vector2 LocalNormal;

	public Vector2 LocalPoint;

	public Manifold Manifold;

	public Vector2 Normal;

	public Mat22 NormalMass;

	public int PointCount;

	public ContactConstraintPoint[] Points = new ContactConstraintPoint[Settings.MaxPolygonVertices];

	public float RadiusA;

	public float RadiusB;

	public float Restitution;

	public ManifoldType Type;

	public ContactConstraint()
	{
		for (int i = 0; i < 2; i++)
		{
			Points[i] = new ContactConstraintPoint();
		}
	}
}
