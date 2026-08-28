using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision;

public static class Collision
{
	private static FatEdge _edgeA;

	private static EPProxy _proxyA = new EPProxy();

	private static EPProxy _proxyB = new EPProxy();

	private static Transform _xf;

	private static Vector2 _limit11;

	private static Vector2 _limit12;

	private static Vector2 _limit21;

	private static Vector2 _limit22;

	private static float _radius;

	private static Vector2[] _tmpNormals = new Vector2[2];

	public static void GetWorldManifold(ref Manifold manifold, ref Transform transformA, float radiusA, ref Transform transformB, float radiusB, out Vector2 normal, out FixedArray2<Vector2> points)
	{
		points = default(FixedArray2<Vector2>);
		normal = Vector2.Zero;
		if (manifold.PointCount == 0)
		{
			normal = Vector2.UnitY;
			return;
		}
		switch (manifold.Type)
		{
		case ManifoldType.Circles:
		{
			Vector2 localPoint3 = manifold.Points[0].LocalPoint;
			float num11 = transformA.Position.X + transformA.R.Col1.X * manifold.LocalPoint.X + transformA.R.Col2.X * manifold.LocalPoint.Y;
			float num12 = transformA.Position.Y + transformA.R.Col1.Y * manifold.LocalPoint.X + transformA.R.Col2.Y * manifold.LocalPoint.Y;
			float num13 = transformB.Position.X + transformB.R.Col1.X * localPoint3.X + transformB.R.Col2.X * localPoint3.Y;
			float num14 = transformB.Position.Y + transformB.R.Col1.Y * localPoint3.X + transformB.R.Col2.Y * localPoint3.Y;
			normal.X = 1f;
			normal.Y = 0f;
			float num15 = (num11 - num13) * (num11 - num13) + (num12 - num14) * (num12 - num14);
			if (num15 > 1.4210855E-14f)
			{
				float num16 = num13 - num11;
				float num17 = num14 - num12;
				float num18 = 1f / (float)Math.Sqrt(num16 * num16 + num17 * num17);
				normal.X = num16 * num18;
				normal.Y = num17 * num18;
			}
			Vector2 zero3 = Vector2.Zero;
			zero3.X = num11 + radiusA * normal.X + (num13 - radiusB * normal.X);
			zero3.Y = num12 + radiusA * normal.Y + (num14 - radiusB * normal.Y);
			points[0] = 0.5f * zero3;
			break;
		}
		case ManifoldType.FaceA:
		{
			normal.X = transformA.R.Col1.X * manifold.LocalNormal.X + transformA.R.Col2.X * manifold.LocalNormal.Y;
			normal.Y = transformA.R.Col1.Y * manifold.LocalNormal.X + transformA.R.Col2.Y * manifold.LocalNormal.Y;
			float num6 = transformA.Position.X + transformA.R.Col1.X * manifold.LocalPoint.X + transformA.R.Col2.X * manifold.LocalPoint.Y;
			float num7 = transformA.Position.Y + transformA.R.Col1.Y * manifold.LocalPoint.X + transformA.R.Col2.Y * manifold.LocalPoint.Y;
			for (int j = 0; j < manifold.PointCount; j++)
			{
				Vector2 localPoint2 = manifold.Points[j].LocalPoint;
				float num8 = transformB.Position.X + transformB.R.Col1.X * localPoint2.X + transformB.R.Col2.X * localPoint2.Y;
				float num9 = transformB.Position.Y + transformB.R.Col1.Y * localPoint2.X + transformB.R.Col2.Y * localPoint2.Y;
				float num10 = (num8 - num6) * normal.X + (num9 - num7) * normal.Y;
				Vector2 zero2 = Vector2.Zero;
				zero2.X = num8 + (radiusA - num10) * normal.X + (num8 - radiusB * normal.X);
				zero2.Y = num9 + (radiusA - num10) * normal.Y + (num9 - radiusB * normal.Y);
				points[j] = 0.5f * zero2;
			}
			break;
		}
		case ManifoldType.FaceB:
		{
			normal.X = transformB.R.Col1.X * manifold.LocalNormal.X + transformB.R.Col2.X * manifold.LocalNormal.Y;
			normal.Y = transformB.R.Col1.Y * manifold.LocalNormal.X + transformB.R.Col2.Y * manifold.LocalNormal.Y;
			float num = transformB.Position.X + transformB.R.Col1.X * manifold.LocalPoint.X + transformB.R.Col2.X * manifold.LocalPoint.Y;
			float num2 = transformB.Position.Y + transformB.R.Col1.Y * manifold.LocalPoint.X + transformB.R.Col2.Y * manifold.LocalPoint.Y;
			for (int i = 0; i < manifold.PointCount; i++)
			{
				Vector2 localPoint = manifold.Points[i].LocalPoint;
				float num3 = transformA.Position.X + transformA.R.Col1.X * localPoint.X + transformA.R.Col2.X * localPoint.Y;
				float num4 = transformA.Position.Y + transformA.R.Col1.Y * localPoint.X + transformA.R.Col2.Y * localPoint.Y;
				float num5 = (num3 - num) * normal.X + (num4 - num2) * normal.Y;
				Vector2 zero = Vector2.Zero;
				zero.X = num3 - radiusA * normal.X + (num3 + (radiusB - num5) * normal.X);
				zero.Y = num4 - radiusA * normal.Y + (num4 + (radiusB - num5) * normal.Y);
				points[i] = 0.5f * zero;
			}
			normal *= -1f;
			break;
		}
		default:
			normal = Vector2.UnitY;
			break;
		}
	}

	public static void GetPointStates(out FixedArray2<PointState> state1, out FixedArray2<PointState> state2, ref Manifold manifold1, ref Manifold manifold2)
	{
		state1 = default(FixedArray2<PointState>);
		state2 = default(FixedArray2<PointState>);
		for (int i = 0; i < manifold1.PointCount; i++)
		{
			ContactID id = manifold1.Points[i].Id;
			state1[i] = PointState.Remove;
			for (int j = 0; j < manifold2.PointCount; j++)
			{
				if (manifold2.Points[j].Id.Key == id.Key)
				{
					state1[i] = PointState.Persist;
					break;
				}
			}
		}
		for (int k = 0; k < manifold2.PointCount; k++)
		{
			ContactID id2 = manifold2.Points[k].Id;
			state2[k] = PointState.Add;
			for (int l = 0; l < manifold1.PointCount; l++)
			{
				if (manifold1.Points[l].Id.Key == id2.Key)
				{
					state2[k] = PointState.Persist;
					break;
				}
			}
		}
	}

	public static void CollideCircles(ref Manifold manifold, CircleShape circleA, ref Transform xfA, CircleShape circleB, ref Transform xfB)
	{
		manifold.PointCount = 0;
		float num = xfA.Position.X + xfA.R.Col1.X * circleA.Position.X + xfA.R.Col2.X * circleA.Position.Y;
		float num2 = xfA.Position.Y + xfA.R.Col1.Y * circleA.Position.X + xfA.R.Col2.Y * circleA.Position.Y;
		float num3 = xfB.Position.X + xfB.R.Col1.X * circleB.Position.X + xfB.R.Col2.X * circleB.Position.Y;
		float num4 = xfB.Position.Y + xfB.R.Col1.Y * circleB.Position.X + xfB.R.Col2.Y * circleB.Position.Y;
		float num5 = (num3 - num) * (num3 - num) + (num4 - num2) * (num4 - num2);
		float num6 = circleA.Radius + circleB.Radius;
		if (!(num5 > num6 * num6))
		{
			manifold.Type = ManifoldType.Circles;
			manifold.LocalPoint = circleA.Position;
			manifold.LocalNormal = Vector2.Zero;
			manifold.PointCount = 1;
			ManifoldPoint value = manifold.Points[0];
			value.LocalPoint = circleB.Position;
			value.Id.Key = 0u;
			manifold.Points[0] = value;
		}
	}

	public static void CollidePolygonAndCircle(ref Manifold manifold, PolygonShape polygonA, ref Transform transformA, CircleShape circleB, ref Transform transformB)
	{
		manifold.PointCount = 0;
		Vector2 vector = new Vector2(transformB.Position.X + transformB.R.Col1.X * circleB.Position.X + transformB.R.Col2.X * circleB.Position.Y, transformB.Position.Y + transformB.R.Col1.Y * circleB.Position.X + transformB.R.Col2.Y * circleB.Position.Y);
		Vector2 vector2 = new Vector2((vector.X - transformA.Position.X) * transformA.R.Col1.X + (vector.Y - transformA.Position.Y) * transformA.R.Col1.Y, (vector.X - transformA.Position.X) * transformA.R.Col2.X + (vector.Y - transformA.Position.Y) * transformA.R.Col2.Y);
		int num = 0;
		float num2 = float.MinValue;
		float num3 = polygonA.Radius + circleB.Radius;
		int count = polygonA.Vertices.Count;
		for (int i = 0; i < count; i++)
		{
			Vector2 vector3 = polygonA.Normals[i];
			Vector2 vector4 = vector2 - polygonA.Vertices[i];
			float num4 = vector3.X * vector4.X + vector3.Y * vector4.Y;
			if (num4 > num3)
			{
				return;
			}
			if (num4 > num2)
			{
				num2 = num4;
				num = i;
			}
		}
		int num5 = num;
		int index = ((num5 + 1 < count) ? (num5 + 1) : 0);
		Vector2 vector5 = polygonA.Vertices[num5];
		Vector2 vector6 = polygonA.Vertices[index];
		if (num2 < 1.1920929E-07f)
		{
			manifold.PointCount = 1;
			manifold.Type = ManifoldType.FaceA;
			manifold.LocalNormal = polygonA.Normals[num];
			manifold.LocalPoint = 0.5f * (vector5 + vector6);
			ManifoldPoint value = manifold.Points[0];
			value.LocalPoint = circleB.Position;
			value.Id.Key = 0u;
			manifold.Points[0] = value;
			return;
		}
		float num6 = (vector2.X - vector5.X) * (vector6.X - vector5.X) + (vector2.Y - vector5.Y) * (vector6.Y - vector5.Y);
		float num7 = (vector2.X - vector6.X) * (vector5.X - vector6.X) + (vector2.Y - vector6.Y) * (vector5.Y - vector6.Y);
		if (num6 <= 0f)
		{
			float num8 = (vector2.X - vector5.X) * (vector2.X - vector5.X) + (vector2.Y - vector5.Y) * (vector2.Y - vector5.Y);
			if (!(num8 > num3 * num3))
			{
				manifold.PointCount = 1;
				manifold.Type = ManifoldType.FaceA;
				manifold.LocalNormal = vector2 - vector5;
				float num9 = 1f / (float)Math.Sqrt(manifold.LocalNormal.X * manifold.LocalNormal.X + manifold.LocalNormal.Y * manifold.LocalNormal.Y);
				manifold.LocalNormal.X = manifold.LocalNormal.X * num9;
				manifold.LocalNormal.Y = manifold.LocalNormal.Y * num9;
				manifold.LocalPoint = vector5;
				ManifoldPoint value2 = manifold.Points[0];
				value2.LocalPoint = circleB.Position;
				value2.Id.Key = 0u;
				manifold.Points[0] = value2;
			}
		}
		else if (num7 <= 0f)
		{
			float num10 = (vector2.X - vector6.X) * (vector2.X - vector6.X) + (vector2.Y - vector6.Y) * (vector2.Y - vector6.Y);
			if (!(num10 > num3 * num3))
			{
				manifold.PointCount = 1;
				manifold.Type = ManifoldType.FaceA;
				manifold.LocalNormal = vector2 - vector6;
				float num11 = 1f / (float)Math.Sqrt(manifold.LocalNormal.X * manifold.LocalNormal.X + manifold.LocalNormal.Y * manifold.LocalNormal.Y);
				manifold.LocalNormal.X = manifold.LocalNormal.X * num11;
				manifold.LocalNormal.Y = manifold.LocalNormal.Y * num11;
				manifold.LocalPoint = vector6;
				ManifoldPoint value3 = manifold.Points[0];
				value3.LocalPoint = circleB.Position;
				value3.Id.Key = 0u;
				manifold.Points[0] = value3;
			}
		}
		else
		{
			Vector2 vector7 = 0.5f * (vector5 + vector6);
			Vector2 vector8 = vector2 - vector7;
			Vector2 vector9 = polygonA.Normals[num5];
			float num12 = vector8.X * vector9.X + vector8.Y * vector9.Y;
			if (!(num12 > num3))
			{
				manifold.PointCount = 1;
				manifold.Type = ManifoldType.FaceA;
				manifold.LocalNormal = polygonA.Normals[num5];
				manifold.LocalPoint = vector7;
				ManifoldPoint value4 = manifold.Points[0];
				value4.LocalPoint = circleB.Position;
				value4.Id.Key = 0u;
				manifold.Points[0] = value4;
			}
		}
	}

	public static void CollidePolygons(ref Manifold manifold, PolygonShape polyA, ref Transform transformA, PolygonShape polyB, ref Transform transformB)
	{
		manifold.PointCount = 0;
		float num = polyA.Radius + polyB.Radius;
		int edgeIndex = 0;
		float num2 = FindMaxSeparation(out edgeIndex, polyA, ref transformA, polyB, ref transformB);
		if (num2 > num)
		{
			return;
		}
		int edgeIndex2 = 0;
		float num3 = FindMaxSeparation(out edgeIndex2, polyB, ref transformB, polyA, ref transformA);
		if (num3 > num)
		{
			return;
		}
		PolygonShape polygonShape;
		PolygonShape poly;
		Transform xf;
		Transform xf2;
		int num4;
		bool flag;
		if (num3 > 0.98f * num2 + 0.001f)
		{
			polygonShape = polyB;
			poly = polyA;
			xf = transformB;
			xf2 = transformA;
			num4 = edgeIndex2;
			manifold.Type = ManifoldType.FaceB;
			flag = true;
		}
		else
		{
			polygonShape = polyA;
			poly = polyB;
			xf = transformA;
			xf2 = transformB;
			num4 = edgeIndex;
			manifold.Type = ManifoldType.FaceA;
			flag = false;
		}
		FindIncidentEdge(out var c, polygonShape, ref xf, num4, poly, ref xf2);
		int count = polygonShape.Vertices.Count;
		int num5 = num4;
		int num6 = ((num4 + 1 < count) ? (num4 + 1) : 0);
		Vector2 vector = polygonShape.Vertices[num5];
		Vector2 vector2 = polygonShape.Vertices[num6];
		float num7 = vector2.X - vector.X;
		float num8 = vector2.Y - vector.Y;
		float num9 = 1f / (float)Math.Sqrt(num7 * num7 + num8 * num8);
		num7 *= num9;
		num8 *= num9;
		Vector2 localNormal = new Vector2(num8, 0f - num7);
		Vector2 localPoint = 0.5f * (vector + vector2);
		Vector2 vector3 = new Vector2(xf.R.Col1.X * num7 + xf.R.Col2.X * num8, xf.R.Col1.Y * num7 + xf.R.Col2.Y * num8);
		float y = vector3.Y;
		float num10 = 0f - vector3.X;
		vector = new Vector2(xf.Position.X + xf.R.Col1.X * vector.X + xf.R.Col2.X * vector.Y, xf.Position.Y + xf.R.Col1.Y * vector.X + xf.R.Col2.Y * vector.Y);
		vector2 = new Vector2(xf.Position.X + xf.R.Col1.X * vector2.X + xf.R.Col2.X * vector2.Y, xf.Position.Y + xf.R.Col1.Y * vector2.X + xf.R.Col2.Y * vector2.Y);
		float num11 = y * vector.X + num10 * vector.Y;
		float offset = 0f - (vector3.X * vector.X + vector3.Y * vector.Y) + num;
		float offset2 = vector3.X * vector2.X + vector3.Y * vector2.Y + num;
		int num12 = ClipSegmentToLine(out var vOut, ref c, -vector3, offset, num5);
		if (num12 < 2)
		{
			return;
		}
		num12 = ClipSegmentToLine(out var vOut2, ref vOut, vector3, offset2, num6);
		if (num12 < 2)
		{
			return;
		}
		manifold.LocalNormal = localNormal;
		manifold.LocalPoint = localPoint;
		int num13 = 0;
		for (int i = 0; i < 2; i++)
		{
			Vector2 v = vOut2[i].V;
			float num14 = y * v.X + num10 * v.Y - num11;
			if (num14 <= num)
			{
				ManifoldPoint value = manifold.Points[num13];
				Vector2 v2 = vOut2[i].V;
				float num15 = v2.X - xf2.Position.X;
				float num16 = v2.Y - xf2.Position.Y;
				value.LocalPoint.X = num15 * xf2.R.Col1.X + num16 * xf2.R.Col1.Y;
				value.LocalPoint.Y = num15 * xf2.R.Col2.X + num16 * xf2.R.Col2.Y;
				value.Id = vOut2[i].ID;
				if (flag)
				{
					ContactFeature features = value.Id.Features;
					value.Id.Features.IndexA = features.IndexB;
					value.Id.Features.IndexB = features.IndexA;
					value.Id.Features.TypeA = features.TypeB;
					value.Id.Features.TypeB = features.TypeA;
				}
				manifold.Points[num13] = value;
				num13++;
			}
		}
		manifold.PointCount = num13;
	}

	public static void CollideEdgeAndCircle(ref Manifold manifold, EdgeShape edgeA, ref Transform transformA, CircleShape circleB, ref Transform transformB)
	{
		manifold.PointCount = 0;
		Vector2 vector = MathUtils.MultiplyT(ref transformA, MathUtils.Multiply(ref transformB, ref circleB._position));
		Vector2 vertex = edgeA.Vertex1;
		Vector2 vertex2 = edgeA.Vertex2;
		Vector2 value = vertex2 - vertex;
		float num = Vector2.Dot(value, vertex2 - vector);
		float num2 = Vector2.Dot(value, vector - vertex);
		float num3 = edgeA.Radius + circleB.Radius;
		ContactFeature features = default(ContactFeature);
		features.IndexB = 0;
		features.TypeB = 0;
		Vector2 vector2;
		Vector2 value2;
		if (num2 <= 0f)
		{
			vector2 = vertex;
			value2 = vector - vector2;
			Vector2.Dot(ref value2, ref value2, out var result);
			if (result > num3 * num3)
			{
				return;
			}
			if (edgeA.HasVertex0)
			{
				Vector2 vertex3 = edgeA.Vertex0;
				Vector2 vector3 = vertex;
				Vector2 value3 = vector3 - vertex3;
				float num4 = Vector2.Dot(value3, vector3 - vector);
				if (num4 > 0f)
				{
					return;
				}
			}
			features.IndexA = 0;
			features.TypeA = 0;
			manifold.PointCount = 1;
			manifold.Type = ManifoldType.Circles;
			manifold.LocalNormal = Vector2.Zero;
			manifold.LocalPoint = vector2;
			ManifoldPoint value4 = new ManifoldPoint
			{
				Id = 
				{
					Key = 0u,
					Features = features
				},
				LocalPoint = circleB.Position
			};
			manifold.Points[0] = value4;
			return;
		}
		if (num <= 0f)
		{
			vector2 = vertex2;
			value2 = vector - vector2;
			Vector2.Dot(ref value2, ref value2, out var result2);
			if (result2 > num3 * num3)
			{
				return;
			}
			if (edgeA.HasVertex3)
			{
				Vector2 vertex4 = edgeA.Vertex3;
				Vector2 vector4 = vertex2;
				Vector2 value5 = vertex4 - vector4;
				float num5 = Vector2.Dot(value5, vector - vector4);
				if (num5 > 0f)
				{
					return;
				}
			}
			features.IndexA = 1;
			features.TypeA = 0;
			manifold.PointCount = 1;
			manifold.Type = ManifoldType.Circles;
			manifold.LocalNormal = Vector2.Zero;
			manifold.LocalPoint = vector2;
			ManifoldPoint value6 = new ManifoldPoint
			{
				Id = 
				{
					Key = 0u,
					Features = features
				},
				LocalPoint = circleB.Position
			};
			manifold.Points[0] = value6;
			return;
		}
		Vector2.Dot(ref value, ref value, out var result3);
		vector2 = 1f / result3 * (num * vertex + num2 * vertex2);
		value2 = vector - vector2;
		Vector2.Dot(ref value2, ref value2, out var result4);
		if (!(result4 > num3 * num3))
		{
			Vector2 vector5 = new Vector2(0f - value.Y, value.X);
			if (Vector2.Dot(vector5, vector - vertex) < 0f)
			{
				vector5 = new Vector2(0f - vector5.X, 0f - vector5.Y);
			}
			vector5.Normalize();
			features.IndexA = 0;
			features.TypeA = 1;
			manifold.PointCount = 1;
			manifold.Type = ManifoldType.FaceA;
			manifold.LocalNormal = vector5;
			manifold.LocalPoint = vertex;
			ManifoldPoint value7 = new ManifoldPoint
			{
				Id = 
				{
					Key = 0u,
					Features = features
				},
				LocalPoint = circleB.Position
			};
			manifold.Points[0] = value7;
		}
	}

	public static void CollideEdgeAndPolygon(ref Manifold manifold, EdgeShape edgeA, ref Transform xfA, PolygonShape polygonB, ref Transform xfB)
	{
		MathUtils.MultiplyT(ref xfA, ref xfB, out _xf);
		_edgeA.V0 = edgeA.Vertex0;
		_edgeA.V1 = edgeA.Vertex1;
		_edgeA.V2 = edgeA.Vertex2;
		_edgeA.V3 = edgeA.Vertex3;
		Vector2 vector = _edgeA.V2 - _edgeA.V1;
		_edgeA.Normal = new Vector2(vector.Y, 0f - vector.X);
		_edgeA.Normal.Normalize();
		_edgeA.HasVertex0 = edgeA.HasVertex0;
		_edgeA.HasVertex3 = edgeA.HasVertex3;
		ref Vector2 reference = ref _proxyA.Vertices[0];
		reference = _edgeA.V1;
		ref Vector2 reference2 = ref _proxyA.Vertices[1];
		reference2 = _edgeA.V2;
		ref Vector2 reference3 = ref _proxyA.Normals[0];
		reference3 = _edgeA.Normal;
		ref Vector2 reference4 = ref _proxyA.Normals[1];
		reference4 = -_edgeA.Normal;
		_proxyA.Centroid = 0.5f * (_edgeA.V1 + _edgeA.V2);
		_proxyA.Count = 2;
		_proxyB.Count = polygonB.Vertices.Count;
		_proxyB.Centroid = MathUtils.Multiply(ref _xf, ref polygonB.MassData.Centroid);
		for (int i = 0; i < polygonB.Vertices.Count; i++)
		{
			ref Vector2 reference5 = ref _proxyB.Vertices[i];
			reference5 = MathUtils.Multiply(ref _xf, polygonB.Vertices[i]);
			ref Vector2 reference6 = ref _proxyB.Normals[i];
			reference6 = MathUtils.Multiply(ref _xf.R, polygonB.Normals[i]);
		}
		_radius = 0.02f;
		_limit11 = Vector2.Zero;
		_limit12 = Vector2.Zero;
		_limit21 = Vector2.Zero;
		_limit22 = Vector2.Zero;
		manifold.PointCount = 0;
		Vector2 v = _edgeA.V0;
		Vector2 v2 = _edgeA.V1;
		Vector2 v3 = _edgeA.V2;
		Vector2 v4 = _edgeA.V3;
		Vector2 centroid = _proxyB.Centroid;
		if (_edgeA.HasVertex0)
		{
			Vector2 vector2 = v2 - v;
			Vector2 vector3 = v3 - v2;
			Vector2 vector4 = new Vector2(vector2.Y, 0f - vector2.X);
			Vector2 vector5 = new Vector2(vector3.Y, 0f - vector3.X);
			vector4.Normalize();
			vector5.Normalize();
			bool flag = MathUtils.Cross(vector4, vector5) >= 0f;
			bool flag2 = Vector2.Dot(vector4, centroid - v) >= 0f;
			bool flag3 = Vector2.Dot(vector5, centroid - v2) >= 0f;
			if (flag)
			{
				if (flag2 || flag3)
				{
					_limit11 = vector5;
					_limit12 = vector4;
				}
				else
				{
					_limit11 = -vector5;
					_limit12 = -vector4;
				}
			}
			else if (flag2 && flag3)
			{
				_limit11 = vector4;
				_limit12 = vector5;
			}
			else
			{
				_limit11 = -vector4;
				_limit12 = -vector5;
			}
		}
		else
		{
			_limit11 = Vector2.Zero;
			_limit12 = Vector2.Zero;
		}
		if (_edgeA.HasVertex3)
		{
			Vector2 vector6 = v3 - v2;
			Vector2 vector7 = v4 - v3;
			Vector2 vector8 = new Vector2(vector6.Y, 0f - vector6.X);
			Vector2 vector9 = new Vector2(vector7.Y, 0f - vector7.X);
			vector8.Normalize();
			vector9.Normalize();
			bool flag4 = MathUtils.Cross(vector8, vector9) >= 0f;
			bool flag5 = Vector2.Dot(vector8, centroid - v2) >= 0f;
			bool flag6 = Vector2.Dot(vector9, centroid - v3) >= 0f;
			if (flag4)
			{
				if (flag5 || flag6)
				{
					_limit21 = vector9;
					_limit22 = vector8;
				}
				else
				{
					_limit21 = -vector9;
					_limit22 = -vector8;
				}
			}
			else if (flag5 && flag6)
			{
				_limit21 = vector8;
				_limit22 = vector9;
			}
			else
			{
				_limit21 = -vector8;
				_limit22 = -vector9;
			}
		}
		else
		{
			_limit21 = Vector2.Zero;
			_limit22 = Vector2.Zero;
		}
		EPAxis ePAxis = ComputeEdgeSeparation();
		if (ePAxis.Type == EPAxisType.Unknown || ePAxis.Separation > _radius)
		{
			return;
		}
		EPAxis ePAxis2 = ComputePolygonSeparation();
		if (ePAxis2.Type != EPAxisType.Unknown && ePAxis2.Separation > _radius)
		{
			return;
		}
		EPAxis ePAxis3 = ((ePAxis2.Type == EPAxisType.Unknown) ? ePAxis : ((!(ePAxis2.Separation > 0.98f * ePAxis.Separation + 0.001f)) ? ePAxis : ePAxis2));
		FixedArray2<ClipVertex> c = default(FixedArray2<ClipVertex>);
		EPProxy ePProxy;
		EPProxy proxy;
		if (ePAxis3.Type == EPAxisType.EdgeA)
		{
			ePProxy = _proxyA;
			proxy = _proxyB;
			manifold.Type = ManifoldType.FaceA;
		}
		else
		{
			ePProxy = _proxyB;
			proxy = _proxyA;
			manifold.Type = ManifoldType.FaceB;
		}
		int index = ePAxis3.Index;
		FindIncidentEdge(ref c, ePProxy, ePAxis3.Index, proxy);
		int count = ePProxy.Count;
		int num = index;
		int num2 = ((index + 1 < count) ? (index + 1) : 0);
		Vector2 vector10 = ePProxy.Vertices[num];
		Vector2 vector11 = ePProxy.Vertices[num2];
		Vector2 vector12 = vector11 - vector10;
		vector12.Normalize();
		Vector2 v5 = MathUtils.Cross(vector12, 1f);
		Vector2 v6 = 0.5f * (vector10 + vector11);
		float num3 = Vector2.Dot(v5, vector10);
		float offset = 0f - Vector2.Dot(vector12, vector10) + _radius;
		float offset2 = Vector2.Dot(vector12, vector11) + _radius;
		int num4 = ClipSegmentToLine(out var vOut, ref c, -vector12, offset, num);
		if (num4 < 2)
		{
			return;
		}
		num4 = ClipSegmentToLine(out var vOut2, ref vOut, vector12, offset2, num2);
		if (num4 < 2)
		{
			return;
		}
		if (ePAxis3.Type == EPAxisType.EdgeA)
		{
			manifold.LocalNormal = v5;
			manifold.LocalPoint = v6;
		}
		else
		{
			manifold.LocalNormal = MathUtils.MultiplyT(ref _xf.R, ref v5);
			manifold.LocalPoint = MathUtils.MultiplyT(ref _xf, ref v6);
		}
		int num5 = 0;
		for (int j = 0; j < 2; j++)
		{
			float num6 = Vector2.Dot(v5, vOut2[j].V) - num3;
			if (num6 <= _radius)
			{
				ManifoldPoint value = manifold.Points[num5];
				if (ePAxis3.Type == EPAxisType.EdgeA)
				{
					value.LocalPoint = MathUtils.MultiplyT(ref _xf, vOut2[j].V);
					value.Id = vOut2[j].ID;
				}
				else
				{
					value.LocalPoint = vOut2[j].V;
					value.Id.Features.TypeA = vOut2[j].ID.Features.TypeB;
					value.Id.Features.TypeB = vOut2[j].ID.Features.TypeA;
					value.Id.Features.IndexA = vOut2[j].ID.Features.IndexB;
					value.Id.Features.IndexB = vOut2[j].ID.Features.IndexA;
				}
				manifold.Points[num5] = value;
				num5++;
			}
		}
		manifold.PointCount = num5;
	}

	private static EPAxis ComputeEdgeSeparation()
	{
		EPAxis result = default(EPAxis);
		result.Type = EPAxisType.Unknown;
		result.Index = -1;
		result.Separation = float.MinValue;
		ref Vector2 reference = ref _tmpNormals[0];
		reference = _edgeA.Normal;
		ref Vector2 reference2 = ref _tmpNormals[1];
		reference2 = -_edgeA.Normal;
		EPAxis ePAxis = default(EPAxis);
		for (int i = 0; i < 2; i++)
		{
			Vector2 vector = _tmpNormals[i];
			bool flag = MathUtils.Cross(vector, _limit11) >= -(float)Math.PI / 90f && MathUtils.Cross(_limit12, vector) >= -(float)Math.PI / 90f;
			bool flag2 = MathUtils.Cross(vector, _limit21) >= -(float)Math.PI / 90f && MathUtils.Cross(_limit22, vector) >= -(float)Math.PI / 90f;
			if (!flag || !flag2)
			{
				continue;
			}
			ePAxis.Type = EPAxisType.EdgeA;
			ePAxis.Index = i;
			ePAxis.Separation = float.MaxValue;
			for (int j = 0; j < _proxyB.Count; j++)
			{
				float num = Vector2.Dot(vector, _proxyB.Vertices[j] - _edgeA.V1);
				if (num < ePAxis.Separation)
				{
					ePAxis.Separation = num;
				}
			}
			if (ePAxis.Separation > _radius)
			{
				return ePAxis;
			}
			if (ePAxis.Separation > result.Separation)
			{
				result = ePAxis;
			}
		}
		return result;
	}

	private static EPAxis ComputePolygonSeparation()
	{
		EPAxis result = default(EPAxis);
		result.Type = EPAxisType.Unknown;
		result.Index = -1;
		result.Separation = float.MinValue;
		for (int i = 0; i < _proxyB.Count; i++)
		{
			Vector2 vector = -_proxyB.Normals[i];
			bool flag = MathUtils.Cross(vector, _limit11) >= -(float)Math.PI / 90f && MathUtils.Cross(_limit12, vector) >= -(float)Math.PI / 90f;
			bool flag2 = MathUtils.Cross(vector, _limit21) >= -(float)Math.PI / 90f && MathUtils.Cross(_limit22, vector) >= -(float)Math.PI / 90f;
			if (flag || flag2)
			{
				float val = Vector2.Dot(vector, _proxyB.Vertices[i] - _edgeA.V1);
				float val2 = Vector2.Dot(vector, _proxyB.Vertices[i] - _edgeA.V2);
				float num = Math.Min(val, val2);
				if (num > _radius)
				{
					result.Type = EPAxisType.EdgeB;
					result.Index = i;
					result.Separation = num;
				}
				if (num > result.Separation)
				{
					result.Type = EPAxisType.EdgeB;
					result.Index = i;
					result.Separation = num;
				}
			}
		}
		return result;
	}

	private static void FindIncidentEdge(ref FixedArray2<ClipVertex> c, EPProxy proxy1, int edge1, EPProxy proxy2)
	{
		int count = proxy2.Count;
		Vector2 value = proxy1.Normals[edge1];
		int num = 0;
		float num2 = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			float num3 = Vector2.Dot(value, proxy2.Normals[i]);
			if (num3 < num2)
			{
				num2 = num3;
				num = i;
			}
		}
		int num4 = num;
		int num5 = ((num4 + 1 < count) ? (num4 + 1) : 0);
		ClipVertex value2 = (c[0] = new ClipVertex
		{
			V = proxy2.Vertices[num4],
			ID = 
			{
				Features = 
				{
					IndexA = (byte)edge1,
					IndexB = (byte)num4,
					TypeA = 1,
					TypeB = 0
				}
			}
		});
		value2.V = proxy2.Vertices[num5];
		value2.ID.Features.IndexA = (byte)edge1;
		value2.ID.Features.IndexB = (byte)num5;
		value2.ID.Features.TypeA = 1;
		value2.ID.Features.TypeB = 0;
		c[1] = value2;
	}

	private static int ClipSegmentToLine(out FixedArray2<ClipVertex> vOut, ref FixedArray2<ClipVertex> vIn, Vector2 normal, float offset, int vertexIndexA)
	{
		vOut = default(FixedArray2<ClipVertex>);
		ClipVertex value = vIn[0];
		ClipVertex value2 = vIn[1];
		int num = 0;
		float num2 = normal.X * value.V.X + normal.Y * value.V.Y - offset;
		float num3 = normal.X * value2.V.X + normal.Y * value2.V.Y - offset;
		if (num2 <= 0f)
		{
			vOut[num++] = value;
		}
		if (num3 <= 0f)
		{
			vOut[num++] = value2;
		}
		if (num2 * num3 < 0f)
		{
			float num4 = num2 / (num2 - num3);
			ClipVertex value3 = vOut[num];
			value3.V.X = value.V.X + num4 * (value2.V.X - value.V.X);
			value3.V.Y = value.V.Y + num4 * (value2.V.Y - value.V.Y);
			value3.ID.Features.IndexA = (byte)vertexIndexA;
			value3.ID.Features.IndexB = value.ID.Features.IndexB;
			value3.ID.Features.TypeA = 0;
			value3.ID.Features.TypeB = 1;
			vOut[num] = value3;
			num++;
		}
		return num;
	}

	private static float EdgeSeparation(PolygonShape poly1, ref Transform xf1, int edge1, PolygonShape poly2, ref Transform xf2)
	{
		int count = poly2.Vertices.Count;
		Vector2 vector = poly1.Normals[edge1];
		float num = xf1.R.Col1.X * vector.X + xf1.R.Col2.X * vector.Y;
		float num2 = xf1.R.Col1.Y * vector.X + xf1.R.Col2.Y * vector.Y;
		Vector2 value = new Vector2(num * xf2.R.Col1.X + num2 * xf2.R.Col1.Y, num * xf2.R.Col2.X + num2 * xf2.R.Col2.Y);
		int index = 0;
		float num3 = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			float num4 = Vector2.Dot(poly2.Vertices[i], value);
			if (num4 < num3)
			{
				num3 = num4;
				index = i;
			}
		}
		Vector2 vector2 = poly1.Vertices[edge1];
		Vector2 vector3 = poly2.Vertices[index];
		return (xf2.Position.X + xf2.R.Col1.X * vector3.X + xf2.R.Col2.X * vector3.Y - (xf1.Position.X + xf1.R.Col1.X * vector2.X + xf1.R.Col2.X * vector2.Y)) * num + (xf2.Position.Y + xf2.R.Col1.Y * vector3.X + xf2.R.Col2.Y * vector3.Y - (xf1.Position.Y + xf1.R.Col1.Y * vector2.X + xf1.R.Col2.Y * vector2.Y)) * num2;
	}

	private static float FindMaxSeparation(out int edgeIndex, PolygonShape poly1, ref Transform xf1, PolygonShape poly2, ref Transform xf2)
	{
		int count = poly1.Vertices.Count;
		float num = xf2.Position.X + xf2.R.Col1.X * poly2.MassData.Centroid.X + xf2.R.Col2.X * poly2.MassData.Centroid.Y - (xf1.Position.X + xf1.R.Col1.X * poly1.MassData.Centroid.X + xf1.R.Col2.X * poly1.MassData.Centroid.Y);
		float num2 = xf2.Position.Y + xf2.R.Col1.Y * poly2.MassData.Centroid.X + xf2.R.Col2.Y * poly2.MassData.Centroid.Y - (xf1.Position.Y + xf1.R.Col1.Y * poly1.MassData.Centroid.X + xf1.R.Col2.Y * poly1.MassData.Centroid.Y);
		Vector2 value = new Vector2(num * xf1.R.Col1.X + num2 * xf1.R.Col1.Y, num * xf1.R.Col2.X + num2 * xf1.R.Col2.Y);
		int num3 = 0;
		float num4 = float.MinValue;
		for (int i = 0; i < count; i++)
		{
			float num5 = Vector2.Dot(poly1.Normals[i], value);
			if (num5 > num4)
			{
				num4 = num5;
				num3 = i;
			}
		}
		float num6 = EdgeSeparation(poly1, ref xf1, num3, poly2, ref xf2);
		int num7 = ((num3 - 1 >= 0) ? (num3 - 1) : (count - 1));
		float num8 = EdgeSeparation(poly1, ref xf1, num7, poly2, ref xf2);
		int num9 = ((num3 + 1 < count) ? (num3 + 1) : 0);
		float num10 = EdgeSeparation(poly1, ref xf1, num9, poly2, ref xf2);
		int num11;
		int num12;
		float num13;
		if (num8 > num6 && num8 > num10)
		{
			num11 = -1;
			num12 = num7;
			num13 = num8;
		}
		else
		{
			if (!(num10 > num6))
			{
				edgeIndex = num3;
				return num6;
			}
			num11 = 1;
			num12 = num9;
			num13 = num10;
		}
		while (true)
		{
			num3 = ((num11 != -1) ? ((num12 + 1 < count) ? (num12 + 1) : 0) : ((num12 - 1 >= 0) ? (num12 - 1) : (count - 1)));
			num6 = EdgeSeparation(poly1, ref xf1, num3, poly2, ref xf2);
			if (!(num6 > num13))
			{
				break;
			}
			num12 = num3;
			num13 = num6;
		}
		edgeIndex = num12;
		return num13;
	}

	private static void FindIncidentEdge(out FixedArray2<ClipVertex> c, PolygonShape poly1, ref Transform xf1, int edge1, PolygonShape poly2, ref Transform xf2)
	{
		c = default(FixedArray2<ClipVertex>);
		int count = poly2.Vertices.Count;
		Vector2 vector = poly1.Normals[edge1];
		float num = xf1.R.Col1.X * vector.X + xf1.R.Col2.X * vector.Y;
		float num2 = xf1.R.Col1.Y * vector.X + xf1.R.Col2.Y * vector.Y;
		Vector2 value = new Vector2(num * xf2.R.Col1.X + num2 * xf2.R.Col1.Y, num * xf2.R.Col2.X + num2 * xf2.R.Col2.Y);
		int num3 = 0;
		float num4 = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			float num5 = Vector2.Dot(value, poly2.Normals[i]);
			if (num5 < num4)
			{
				num4 = num5;
				num3 = i;
			}
		}
		int num6 = num3;
		int num7 = ((num6 + 1 < count) ? (num6 + 1) : 0);
		ClipVertex value2 = c[0];
		Vector2 vector2 = poly2.Vertices[num6];
		value2.V.X = xf2.Position.X + xf2.R.Col1.X * vector2.X + xf2.R.Col2.X * vector2.Y;
		value2.V.Y = xf2.Position.Y + xf2.R.Col1.Y * vector2.X + xf2.R.Col2.Y * vector2.Y;
		value2.ID.Features.IndexA = (byte)edge1;
		value2.ID.Features.IndexB = (byte)num6;
		value2.ID.Features.TypeA = 1;
		value2.ID.Features.TypeB = 0;
		c[0] = value2;
		ClipVertex value3 = c[1];
		Vector2 vector3 = poly2.Vertices[num7];
		value3.V.X = xf2.Position.X + xf2.R.Col1.X * vector3.X + xf2.R.Col2.X * vector3.Y;
		value3.V.Y = xf2.Position.Y + xf2.R.Col1.Y * vector3.X + xf2.R.Col2.Y * vector3.Y;
		value3.ID.Features.IndexA = (byte)edge1;
		value3.ID.Features.IndexB = (byte)num7;
		value3.ID.Features.TypeA = 1;
		value3.ID.Features.TypeB = 0;
		c[1] = value3;
	}
}
