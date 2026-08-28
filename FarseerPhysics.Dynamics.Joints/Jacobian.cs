using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints;

internal struct Jacobian
{
	public float AngularA;

	public float AngularB;

	public Vector2 LinearA;

	public Vector2 LinearB;

	public void SetZero()
	{
		LinearA = Vector2.Zero;
		AngularA = 0f;
		LinearB = Vector2.Zero;
		AngularB = 0f;
	}

	public void Set(Vector2 x1, float a1, Vector2 x2, float a2)
	{
		LinearA = x1;
		AngularA = a1;
		LinearB = x2;
		AngularB = a2;
	}

	public float Compute(Vector2 x1, float a1, Vector2 x2, float a2)
	{
		return Vector2.Dot(LinearA, x1) + AngularA * a1 + Vector2.Dot(LinearB, x2) + AngularB * a2;
	}
}
