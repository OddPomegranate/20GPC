using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision;

public static class SeparationFunction
{
	private static Vector2 _axis;

	private static Vector2 _localPoint;

	private static DistanceProxy _proxyA = new DistanceProxy();

	private static DistanceProxy _proxyB = new DistanceProxy();

	private static Sweep _sweepA;

	private static Sweep _sweepB;

	private static SeparationFunctionType _type;

	public static void Set(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, float t1)
	{
		_localPoint = Vector2.Zero;
		_proxyA = proxyA;
		_proxyB = proxyB;
		int count = cache.Count;
		_sweepA = sweepA;
		_sweepB = sweepB;
		_sweepA.GetTransform(out var xf, t1);
		_sweepB.GetTransform(out var xf2, t1);
		if (count == 1)
		{
			_type = SeparationFunctionType.Points;
			Vector2 v = _proxyA.Vertices[cache.IndexA[0]];
			Vector2 v2 = _proxyB.Vertices[cache.IndexB[0]];
			Vector2 vector = MathUtils.Multiply(ref xf, v);
			Vector2 vector2 = MathUtils.Multiply(ref xf2, v2);
			_axis = vector2 - vector;
			_axis.Normalize();
		}
		else if (cache.IndexA[0] == cache.IndexA[1])
		{
			_type = SeparationFunctionType.FaceB;
			Vector2 vector3 = proxyB.Vertices[cache.IndexB[0]];
			Vector2 vector4 = proxyB.Vertices[cache.IndexB[1]];
			Vector2 vector5 = vector4 - vector3;
			_axis = new Vector2(vector5.Y, 0f - vector5.X);
			_axis.Normalize();
			Vector2 value = MathUtils.Multiply(ref xf2.R, _axis);
			_localPoint = 0.5f * (vector3 + vector4);
			Vector2 vector6 = MathUtils.Multiply(ref xf2, _localPoint);
			Vector2 v3 = proxyA.Vertices[cache.IndexA[0]];
			Vector2 vector7 = MathUtils.Multiply(ref xf, v3);
			float num = Vector2.Dot(vector7 - vector6, value);
			if (num < 0f)
			{
				_axis = -_axis;
				num = 0f - num;
			}
		}
		else
		{
			_type = SeparationFunctionType.FaceA;
			Vector2 vector8 = _proxyA.Vertices[cache.IndexA[0]];
			Vector2 vector9 = _proxyA.Vertices[cache.IndexA[1]];
			Vector2 vector10 = vector9 - vector8;
			_axis = new Vector2(vector10.Y, 0f - vector10.X);
			_axis.Normalize();
			Vector2 value2 = MathUtils.Multiply(ref xf.R, _axis);
			_localPoint = 0.5f * (vector8 + vector9);
			Vector2 vector11 = MathUtils.Multiply(ref xf, _localPoint);
			Vector2 v4 = _proxyB.Vertices[cache.IndexB[0]];
			Vector2 vector12 = MathUtils.Multiply(ref xf2, v4);
			float num2 = Vector2.Dot(vector12 - vector11, value2);
			if (num2 < 0f)
			{
				_axis = -_axis;
				num2 = 0f - num2;
			}
		}
	}

	public static float FindMinSeparation(out int indexA, out int indexB, float t)
	{
		_sweepA.GetTransform(out var xf, t);
		_sweepB.GetTransform(out var xf2, t);
		switch (_type)
		{
		case SeparationFunctionType.Points:
		{
			Vector2 direction3 = MathUtils.MultiplyT(ref xf.R, _axis);
			Vector2 direction4 = MathUtils.MultiplyT(ref xf2.R, -_axis);
			indexA = _proxyA.GetSupport(direction3);
			indexB = _proxyB.GetSupport(direction4);
			Vector2 v3 = _proxyA.Vertices[indexA];
			Vector2 v4 = _proxyB.Vertices[indexB];
			Vector2 vector7 = MathUtils.Multiply(ref xf, v3);
			Vector2 vector8 = MathUtils.Multiply(ref xf2, v4);
			return Vector2.Dot(vector8 - vector7, _axis);
		}
		case SeparationFunctionType.FaceA:
		{
			Vector2 vector4 = MathUtils.Multiply(ref xf.R, _axis);
			Vector2 vector5 = MathUtils.Multiply(ref xf, _localPoint);
			Vector2 direction2 = MathUtils.MultiplyT(ref xf2.R, -vector4);
			indexA = -1;
			indexB = _proxyB.GetSupport(direction2);
			Vector2 v2 = _proxyB.Vertices[indexB];
			Vector2 vector6 = MathUtils.Multiply(ref xf2, v2);
			return Vector2.Dot(vector6 - vector5, vector4);
		}
		case SeparationFunctionType.FaceB:
		{
			Vector2 vector = MathUtils.Multiply(ref xf2.R, _axis);
			Vector2 vector2 = MathUtils.Multiply(ref xf2, _localPoint);
			Vector2 direction = MathUtils.MultiplyT(ref xf.R, -vector);
			indexB = -1;
			indexA = _proxyA.GetSupport(direction);
			Vector2 v = _proxyA.Vertices[indexA];
			Vector2 vector3 = MathUtils.Multiply(ref xf, v);
			return Vector2.Dot(vector3 - vector2, vector);
		}
		default:
			indexA = -1;
			indexB = -1;
			return 0f;
		}
	}

	public static float Evaluate(int indexA, int indexB, float t)
	{
		_sweepA.GetTransform(out var xf, t);
		_sweepB.GetTransform(out var xf2, t);
		switch (_type)
		{
		case SeparationFunctionType.Points:
		{
			MathUtils.MultiplyT(ref xf.R, _axis);
			MathUtils.MultiplyT(ref xf2.R, -_axis);
			Vector2 v3 = _proxyA.Vertices[indexA];
			Vector2 v4 = _proxyB.Vertices[indexB];
			Vector2 vector7 = MathUtils.Multiply(ref xf, v3);
			Vector2 vector8 = MathUtils.Multiply(ref xf2, v4);
			return Vector2.Dot(vector8 - vector7, _axis);
		}
		case SeparationFunctionType.FaceA:
		{
			Vector2 vector4 = MathUtils.Multiply(ref xf.R, _axis);
			Vector2 vector5 = MathUtils.Multiply(ref xf, _localPoint);
			MathUtils.MultiplyT(ref xf2.R, -vector4);
			Vector2 v2 = _proxyB.Vertices[indexB];
			Vector2 vector6 = MathUtils.Multiply(ref xf2, v2);
			return Vector2.Dot(vector6 - vector5, vector4);
		}
		case SeparationFunctionType.FaceB:
		{
			Vector2 vector = MathUtils.Multiply(ref xf2.R, _axis);
			Vector2 vector2 = MathUtils.Multiply(ref xf2, _localPoint);
			MathUtils.MultiplyT(ref xf.R, -vector);
			Vector2 v = _proxyA.Vertices[indexA];
			Vector2 vector3 = MathUtils.Multiply(ref xf, v);
			return Vector2.Dot(vector3 - vector2, vector);
		}
		default:
			return 0f;
		}
	}
}
