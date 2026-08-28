using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common.PolygonManipulation;

public static class CuttingTools
{
	public static void SplitShape(Fixture fixture, Vector2 entryPoint, Vector2 exitPoint, float splitSize, out Vertices first, out Vertices second)
	{
		Vector2 localPoint = fixture.Body.GetLocalPoint(ref entryPoint);
		Vector2 localPoint2 = fixture.Body.GetLocalPoint(ref exitPoint);
		if (!(fixture.Shape is PolygonShape polygonShape))
		{
			first = new Vertices();
			second = new Vertices();
			return;
		}
		Vertices vertices = new Vertices(polygonShape.Vertices);
		Vertices[] array = new Vertices[2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vertices(vertices.Count);
		}
		int[] array2 = new int[2] { -1, -1 };
		int num = -1;
		for (int j = 0; j < vertices.Count; j++)
		{
			int num2 = ((!(Vector2.Dot(MathUtils.Cross(localPoint2 - localPoint, 1f), vertices[j] - localPoint) > 1.1920929E-07f)) ? 1 : 0);
			if (num != num2)
			{
				if (num == 0)
				{
					array2[0] = array[num].Count;
					array[num].Add(localPoint2);
					array[num].Add(localPoint);
				}
				if (num == 1)
				{
					array2[num] = array[num].Count;
					array[num].Add(localPoint);
					array[num].Add(localPoint2);
				}
			}
			array[num2].Add(vertices[j]);
			num = num2;
		}
		if (array2[0] == -1)
		{
			array2[0] = array[0].Count;
			array[0].Add(localPoint2);
			array[0].Add(localPoint);
		}
		if (array2[1] == -1)
		{
			array2[1] = array[1].Count;
			array[1].Add(localPoint);
			array[1].Add(localPoint2);
		}
		for (int k = 0; k < 2; k++)
		{
			Vector2 vector = ((array2[k] <= 0) ? (array[k][array[k].Count - 1] - array[k][0]) : (array[k][array2[k] - 1] - array[k][array2[k]]));
			vector.Normalize();
			array[k][array2[k]] += splitSize * vector;
			vector = ((array2[k] >= array[k].Count - 2) ? (array[k][0] - array[k][array[k].Count - 1]) : (array[k][array2[k] + 2] - array[k][array2[k] + 1]));
			vector.Normalize();
			array[k][array2[k] + 1] += splitSize * vector;
		}
		first = array[0];
		second = array[1];
	}

	public static void Cut(World world, Vector2 start, Vector2 end, float thickness)
	{
		List<Fixture> fixtures = new List<Fixture>();
		List<Vector2> entryPoints = new List<Vector2>();
		List<Vector2> exitPoints = new List<Vector2>();
		if (world.TestPoint(start) != null || world.TestPoint(end) != null)
		{
			return;
		}
		world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			fixtures.Add(f);
			entryPoints.Add(p);
			return 1f;
		}, start, end);
		world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			exitPoints.Add(p);
			return 1f;
		}, end, start);
		if (entryPoints.Count + exitPoints.Count < 2)
		{
			return;
		}
		for (int num = 0; num < fixtures.Count; num++)
		{
			if (fixtures[num].Shape.ShapeType == ShapeType.Polygon && fixtures[num].Body.BodyType != BodyType.Static)
			{
				SplitShape(fixtures[num], entryPoints[num], exitPoints[num], thickness, out var first, out var second);
				if (SanityCheck(first))
				{
					Body body = BodyFactory.CreatePolygon(world, first, fixtures[num].Shape.Density, fixtures[num].Body.Position);
					body.Rotation = fixtures[num].Body.Rotation;
					body.LinearVelocity = fixtures[num].Body.LinearVelocity;
					body.AngularVelocity = fixtures[num].Body.AngularVelocity;
					body.BodyType = BodyType.Dynamic;
				}
				if (SanityCheck(second))
				{
					Body body2 = BodyFactory.CreatePolygon(world, second, fixtures[num].Shape.Density, fixtures[num].Body.Position);
					body2.Rotation = fixtures[num].Body.Rotation;
					body2.LinearVelocity = fixtures[num].Body.LinearVelocity;
					body2.AngularVelocity = fixtures[num].Body.AngularVelocity;
					body2.BodyType = BodyType.Dynamic;
				}
				world.RemoveBody(fixtures[num].Body);
			}
		}
	}

	private static bool SanityCheck(Vertices vertices)
	{
		if (vertices.Count < 3)
		{
			return false;
		}
		if (vertices.GetArea() < 1E-05f)
		{
			return false;
		}
		for (int i = 0; i < vertices.Count; i++)
		{
			int index = i;
			int index2 = ((i + 1 < vertices.Count) ? (i + 1) : 0);
			if ((vertices[index2] - vertices[index]).LengthSquared() < 1.4210855E-14f)
			{
				return false;
			}
		}
		for (int j = 0; j < vertices.Count; j++)
		{
			int num = j;
			int num2 = ((j + 1 < vertices.Count) ? (j + 1) : 0);
			Vector2 vector = vertices[num2] - vertices[num];
			for (int k = 0; k < vertices.Count; k++)
			{
				if (k != num && k != num2)
				{
					Vector2 vector2 = vertices[k] - vertices[num];
					float num3 = vector.X * vector2.Y - vector.Y * vector2.X;
					if (num3 < 0f)
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
