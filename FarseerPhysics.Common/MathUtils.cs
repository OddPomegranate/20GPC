using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common;

public static class MathUtils
{
	[StructLayout(LayoutKind.Explicit)]
	private struct FloatConverter
	{
		[FieldOffset(0)]
		public float x;

		[FieldOffset(0)]
		public int i;
	}

	public static float Cross(Vector2 a, Vector2 b)
	{
		return a.X * b.Y - a.Y * b.X;
	}

	public static Vector2 Cross(Vector2 a, float s)
	{
		return new Vector2(s * a.Y, (0f - s) * a.X);
	}

	public static Vector2 Cross(float s, Vector2 a)
	{
		return new Vector2((0f - s) * a.Y, s * a.X);
	}

	public static Vector2 Abs(Vector2 v)
	{
		return new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
	}

	public static Vector2 Multiply(ref Mat22 A, Vector2 v)
	{
		return Multiply(ref A, ref v);
	}

	public static Vector2 Multiply(ref Mat22 A, ref Vector2 v)
	{
		return new Vector2(A.Col1.X * v.X + A.Col2.X * v.Y, A.Col1.Y * v.X + A.Col2.Y * v.Y);
	}

	public static Vector2 MultiplyT(ref Mat22 A, Vector2 v)
	{
		return MultiplyT(ref A, ref v);
	}

	public static Vector2 MultiplyT(ref Mat22 A, ref Vector2 v)
	{
		return new Vector2(v.X * A.Col1.X + v.Y * A.Col1.Y, v.X * A.Col2.X + v.Y * A.Col2.Y);
	}

	public static Vector2 Multiply(ref Transform T, Vector2 v)
	{
		return Multiply(ref T, ref v);
	}

	public static Vector2 Multiply(ref Transform T, ref Vector2 v)
	{
		return new Vector2(T.Position.X + T.R.Col1.X * v.X + T.R.Col2.X * v.Y, T.Position.Y + T.R.Col1.Y * v.X + T.R.Col2.Y * v.Y);
	}

	public static Vector2 MultiplyT(ref Transform T, Vector2 v)
	{
		return MultiplyT(ref T, ref v);
	}

	public static Vector2 MultiplyT(ref Transform T, ref Vector2 v)
	{
		Vector2 v2 = Vector2.Zero;
		v2.X = v.X - T.Position.X;
		v2.Y = v.Y - T.Position.Y;
		return MultiplyT(ref T.R, ref v2);
	}

	public static void MultiplyT(ref Mat22 A, ref Mat22 B, out Mat22 C)
	{
		C = default(Mat22);
		C.Col1.X = A.Col1.X * B.Col1.X + A.Col1.Y * B.Col1.Y;
		C.Col1.Y = A.Col2.X * B.Col1.X + A.Col2.Y * B.Col1.Y;
		C.Col2.X = A.Col1.X * B.Col2.X + A.Col1.Y * B.Col2.Y;
		C.Col2.Y = A.Col2.X * B.Col2.X + A.Col2.Y * B.Col2.Y;
	}

	public static void MultiplyT(ref Transform A, ref Transform B, out Transform C)
	{
		C = default(Transform);
		MultiplyT(ref A.R, ref B.R, out C.R);
		C.Position.X = B.Position.X - A.Position.X;
		C.Position.Y = B.Position.Y - A.Position.Y;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	public static bool IsValid(float x)
	{
		if (float.IsNaN(x))
		{
			return false;
		}
		return !float.IsInfinity(x);
	}

	public static bool IsValid(this Vector2 x)
	{
		if (IsValid(x.X))
		{
			return IsValid(x.Y);
		}
		return false;
	}

	public static float InvSqrt(float x)
	{
		FloatConverter floatConverter = default(FloatConverter);
		floatConverter.x = x;
		float num = 0.5f * x;
		floatConverter.i = 1597463007 - (floatConverter.i >> 1);
		x = floatConverter.x;
		x *= 1.5f - num * x * x;
		return x;
	}

	public static int Clamp(int a, int low, int high)
	{
		return Math.Max(low, Math.Min(a, high));
	}

	public static float Clamp(float a, float low, float high)
	{
		return Math.Max(low, Math.Min(a, high));
	}

	public static Vector2 Clamp(Vector2 a, Vector2 low, Vector2 high)
	{
		return Vector2.Max(low, Vector2.Min(a, high));
	}

	public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
	{
		c = a.X * b.Y - a.Y * b.X;
	}

	public static double VectorAngle(ref Vector2 p1, ref Vector2 p2)
	{
		double num = Math.Atan2(p1.Y, p1.X);
		double num2 = Math.Atan2(p2.Y, p2.X);
		double num3;
		for (num3 = num2 - num; num3 > Math.PI; num3 -= Math.PI * 2.0)
		{
		}
		for (; num3 < -Math.PI; num3 += Math.PI * 2.0)
		{
		}
		return num3;
	}

	public static double VectorAngle(Vector2 p1, Vector2 p2)
	{
		return VectorAngle(ref p1, ref p2);
	}

	public static float Area(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c);
	}

	public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
	{
		return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
	}

	public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c)
	{
		return Collinear(ref a, ref b, ref c, 0f);
	}

	public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance)
	{
		return FloatInRange(Area(ref a, ref b, ref c), 0f - tolerance, tolerance);
	}

	public static void Cross(float s, ref Vector2 a, out Vector2 b)
	{
		b = new Vector2((0f - s) * a.Y, s * a.X);
	}

	public static bool FloatEquals(float value1, float value2)
	{
		return Math.Abs(value1 - value2) <= 1.1920929E-07f;
	}

	public static bool FloatEquals(float value1, float value2, float delta)
	{
		return FloatInRange(value1, value2 - delta, value2 + delta);
	}

	public static bool FloatInRange(float value, float min, float max)
	{
		if (value >= min)
		{
			return value <= max;
		}
		return false;
	}
}
