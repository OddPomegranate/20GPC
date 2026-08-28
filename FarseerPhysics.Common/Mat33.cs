using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common;

public struct Mat33(Vector3 c1, Vector3 c2, Vector3 c3)
{
	public Vector3 Col1 = c1;

	public Vector3 Col2 = c2;

	public Vector3 Col3 = c3;

	public void SetZero()
	{
		Col1 = Vector3.Zero;
		Col2 = Vector3.Zero;
		Col3 = Vector3.Zero;
	}

	public Vector3 Solve33(Vector3 b)
	{
		float num = Vector3.Dot(Col1, Vector3.Cross(Col2, Col3));
		if (num != 0f)
		{
			num = 1f / num;
		}
		return new Vector3(num * Vector3.Dot(b, Vector3.Cross(Col2, Col3)), num * Vector3.Dot(Col1, Vector3.Cross(b, Col3)), num * Vector3.Dot(Col1, Vector3.Cross(Col2, b)));
	}

	public Vector2 Solve22(Vector2 b)
	{
		float x = Col1.X;
		float x2 = Col2.X;
		float y = Col1.Y;
		float y2 = Col2.Y;
		float num = x * y2 - x2 * y;
		if (num != 0f)
		{
			num = 1f / num;
		}
		return new Vector2(num * (y2 * b.X - x2 * b.Y), num * (x * b.Y - y * b.X));
	}
}
