using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts;

public sealed class ContactConstraintPoint
{
	public Vector2 LocalPoint;

	public float NormalImpulse;

	public float NormalMass;

	public float TangentImpulse;

	public float TangentMass;

	public float VelocityBias;

	public Vector2 rA;

	public Vector2 rB;
}
