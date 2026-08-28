using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common;

public struct Mat22
{
	public Vector2 Col1;

	public Vector2 Col2;

	public float Angle => (float)Math.Atan2(Col1.Y, Col1.X);

	public Mat22 Inverse
	{
		get
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
			return new Mat22
			{
				Col1 = 
				{
					X = num * y2,
					Y = (0f - num) * y
				},
				Col2 = 
				{
					X = (0f - num) * x2,
					Y = num * x
				}
			};
		}
	}

	public Mat22(Vector2 c1, Vector2 c2)
	{
		Col1 = c1;
		Col2 = c2;
	}

	public Mat22(float a11, float a12, float a21, float a22)
	{
		Col1 = new Vector2(a11, a21);
		Col2 = new Vector2(a12, a22);
	}

	public Mat22(float angle)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		Col1 = new Vector2(num, num2);
		Col2 = new Vector2(0f - num2, num);
	}

	public void Set(Vector2 c1, Vector2 c2)
	{
		Col1 = c1;
		Col2 = c2;
	}

	public void Set(float angle)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		Col1.X = num;
		Col2.X = 0f - num2;
		Col1.Y = num2;
		Col2.Y = num;
	}

	public void SetIdentity()
	{
		Col1.X = 1f;
		Col2.X = 0f;
		Col1.Y = 0f;
		Col2.Y = 1f;
	}

	public void SetZero()
	{
		Col1.X = 0f;
		Col2.X = 0f;
		Col1.Y = 0f;
		Col2.Y = 0f;
	}

	public Vector2 Solve(Vector2 b)
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

	public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
	{
		R.Col1 = A.Col1 + B.Col1;
		R.Col2 = A.Col2 + B.Col2;
	}
}
