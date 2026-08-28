using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common;

public struct Transform(ref Vector2 position, ref Mat22 r)
{
	public Vector2 Position = position;

	public Mat22 R = r;

	public float Angle => (float)Math.Atan2(R.Col1.Y, R.Col1.X);

	public void SetIdentity()
	{
		Position = Vector2.Zero;
		R.SetIdentity();
	}

	public void Set(Vector2 position, float angle)
	{
		Position = position;
		R.Set(angle);
	}
}
