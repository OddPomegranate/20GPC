using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common;

public struct Sweep
{
	public float A;

	public float A0;

	public float Alpha0;

	public Vector2 C;

	public Vector2 C0;

	public Vector2 LocalCenter;

	public void GetTransform(out Transform xf, float beta)
	{
		xf = default(Transform);
		xf.Position.X = (1f - beta) * C0.X + beta * C.X;
		xf.Position.Y = (1f - beta) * C0.Y + beta * C.Y;
		float angle = (1f - beta) * A0 + beta * A;
		xf.R.Set(angle);
		xf.Position -= MathUtils.Multiply(ref xf.R, ref LocalCenter);
	}

	public void Advance(float alpha)
	{
		float num = (alpha - Alpha0) / (1f - Alpha0);
		C0.X = (1f - num) * C0.X + num * C.X;
		C0.Y = (1f - num) * C0.Y + num * C.Y;
		A0 = (1f - num) * A0 + num * A;
		Alpha0 = alpha;
	}

	public void Normalize()
	{
		float num = (float)Math.PI * 2f * (float)Math.Floor(A0 / ((float)Math.PI * 2f));
		A0 -= num;
		A -= num;
	}
}
