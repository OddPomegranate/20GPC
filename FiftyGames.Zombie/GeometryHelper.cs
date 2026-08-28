using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal static class GeometryHelper
{
	private static LineRenderer _lineDrawer;

	public static LineRenderer LineRenderer => _lineDrawer;

	public static void InitLineRenderer(GraphicsDevice graphicsDevice, ContentManager contentManager, Rectangle screen)
	{
		_lineDrawer = new LineRenderer(graphicsDevice, contentManager, new Rectangle(0, 0, 1280, 720));
	}

	public static void DrawLine(Vector2 v1, Vector2 v2, Vector2 offset)
	{
		List<VertexPositionColor> list = new List<VertexPositionColor>();
		list.Add(new VertexPositionColor(new Vector3(v1, 0f), Color.Red));
		list.Add(new VertexPositionColor(new Vector3(v2, 0f), Color.Red));
		_lineDrawer.DrawShape(list.ToArray(), offset);
	}

	public static float UnsignedAngleBetweenTwoV2(Vector2 v1, Vector2 v2)
	{
		v1.Normalize();
		v2.Normalize();
		double num = (float)Math.Acos(Vector2.Dot(v1, v2));
		return (float)num;
	}

	public static double GetSignedAngleBetween2DVectors(Vector2 FromVector, Vector2 DestVector, Vector2 DestVectorsRight)
	{
		FromVector.Normalize();
		DestVector.Normalize();
		DestVectorsRight.Normalize();
		float value = Vector2.Dot(FromVector, DestVector);
		float num = Vector2.Dot(FromVector, DestVectorsRight);
		value = MathHelper.Clamp(value, -1f, 1f);
		double num2 = Math.Acos(value);
		if (num < 0f)
		{
			num2 *= -1.0;
		}
		return num2;
	}

	public static void GetCircleLines(Circle circle, out List<VertexPositionColor> circleVerts, out List<Line> lines)
	{
		circleVerts = new List<VertexPositionColor>();
		lines = new List<Line>();
		Vector2 vector = circle.Points[0];
		circleVerts.Clear();
		for (int i = 1; i < circle.Points.Count; i++)
		{
			circleVerts.Add(new VertexPositionColor(new Vector3(vector + circle.Position, 0f), Color.White));
			circleVerts.Add(new VertexPositionColor(new Vector3(circle.Points[i] + circle.Position, 0f), Color.White));
			Line line = new Line();
			line.Start = vector + circle.Position;
			line.End = circle.Points[i] + circle.Position;
			lines.Add(line);
			vector = circle.Points[i];
		}
		circleVerts.Add(new VertexPositionColor(new Vector3(vector + circle.Position, 0f), Color.White));
		circleVerts.Add(new VertexPositionColor(new Vector3(circle.Points[0] + circle.Position, 0f), Color.White));
	}

	public static void GetCircleLines(Circle circle, out List<Line> lines)
	{
		lines = new List<Line>();
		Vector2 vector = circle.Points[0];
		for (int i = 1; i < circle.Points.Count; i++)
		{
			Line line = new Line();
			line.Start = vector + circle.Position;
			line.End = circle.Points[i] + circle.Position;
			lines.Add(line);
			vector = circle.Points[i];
		}
	}

	public static void GetCircleLines(Circle circle, out List<VertexPositionColor> circleVerts)
	{
		circleVerts = new List<VertexPositionColor>();
		Vector2 vector = circle.Points[0];
		circleVerts.Clear();
		for (int i = 1; i < circle.Points.Count; i++)
		{
			circleVerts.Add(new VertexPositionColor(new Vector3(vector + circle.Position, 0f), Color.White));
			circleVerts.Add(new VertexPositionColor(new Vector3(circle.Points[i] + circle.Position, 0f), Color.White));
			vector = circle.Points[i];
		}
		circleVerts.Add(new VertexPositionColor(new Vector3(vector + circle.Position, 0f), Color.White));
		circleVerts.Add(new VertexPositionColor(new Vector3(circle.Points[0] + circle.Position, 0f), Color.White));
	}

	public static Circle GenerateCircle(int radius, int quality, Vector2 initialPosition)
	{
		List<Vector2> list = new List<Vector2>();
		float num = (float)Math.PI * 2f / (float)quality;
		float num2 = 0f;
		for (int i = 0; i < quality; i++)
		{
			list.Add(AngleToV2(num2, radius));
			num2 += num;
		}
		Circle result = default(Circle);
		result.Radius = radius;
		result.Points = list;
		result.Position = initialPosition;
		return result;
	}

	public static Vector2 GetDefaultNormal(Line line)
	{
		_ = Vector2.Zero;
		Vector2 position = ((!(line.Start.X > line.End.X)) ? (line.Start - line.End) : (line.End - line.Start));
		position = Vector2.Transform(position, Matrix.CreateRotationZ((float)Math.PI / 2f));
		position.Normalize();
		return position;
	}

	public static Vector2 GetCenterOfLine(Line line)
	{
		_ = Vector2.Zero;
		Vector2 vector = line.Start - line.End;
		return line.Start - vector / 2f;
	}

	public static Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}

	public static float V2ToAngle(Vector2 v)
	{
		return (float)Math.Atan2(v.Y, v.X);
	}

	public static Vector2 ProcessIntersection(Line l1, Line l2)
	{
		Vector2 result = Vector2.Zero;
		Vector2 start = l1.Start;
		Vector2 end = l1.End;
		Vector2 start2 = l2.Start;
		Vector2 end2 = l2.End;
		float num = (end2.X - start2.X) * (start.Y - start2.Y) - (end2.Y - start2.Y) * (start.X - start2.X);
		float num2 = (end.X - start.X) * (start.Y - start2.Y) - (end.Y - start.Y) * (start.X - start2.X);
		float num3 = (end2.Y - start2.Y) * (end.X - start.X) - (end2.X - start2.X) * (end.Y - start.Y);
		if (Math.Abs(num3) <= 1E-05f)
		{
			if (Math.Abs(num) <= 1E-05f && Math.Abs(num2) <= 1E-05f)
			{
				result = (start + end) / 2f;
			}
		}
		else
		{
			num /= num3;
			num2 /= num3;
			if (num >= 0f && num <= 1f && num2 >= 0f && num2 <= 1f)
			{
				result.X = start.X + num * (end.X - start.X);
				result.Y = start.Y + num * (end.Y - start.Y);
			}
		}
		return result;
	}

	public static float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed)
	{
		float num = faceThis.X - position.X;
		float num2 = faceThis.Y - position.Y;
		float num3 = (float)Math.Atan2(num2, num);
		float value = WrapAngle(num3 - currentAngle);
		value = MathHelper.Clamp(value, 0f - turnSpeed, turnSpeed);
		return WrapAngle(currentAngle + value);
	}

	private static float WrapAngle(float radians)
	{
		while (radians < -(float)Math.PI)
		{
			radians += (float)Math.PI * 2f;
		}
		while (radians > (float)Math.PI)
		{
			radians -= (float)Math.PI * 2f;
		}
		return radians;
	}

	public static Vector2[] IntersectionPoint(Line line, Circle circle)
	{
		double num = (line.End.X - line.Start.X) * (line.End.X - line.Start.X) + (line.End.Y - line.Start.Y) * (line.End.Y - line.Start.Y);
		double num2 = 2f * ((line.End.X - line.Start.X) * (line.Start.X - circle.Position.X) + (line.End.Y - line.Start.Y) * (line.Start.Y - circle.Position.Y));
		double num3 = circle.Position.X * circle.Position.X + circle.Position.Y * circle.Position.Y + line.Start.X * line.Start.X + line.Start.Y * line.Start.Y - 2f * (circle.Position.X * line.Start.X + circle.Position.Y * line.Start.Y) - circle.Radius * circle.Radius;
		double num4 = num2 * num2 - 4.0 * num * num3;
		if (num4 == 0.0)
		{
			Vector2[] array = new Vector2[1];
			double num5 = (0.0 - num2) / (2.0 * num);
			ref Vector2 reference = ref array[0];
			reference = new Vector2((float)((double)line.Start.X + num5 * (double)(line.End.X - line.Start.X)), (float)((double)line.Start.Y + num5 * (double)(line.End.Y - line.Start.Y)));
			return array;
		}
		if (num4 > 0.0)
		{
			Vector2[] array = new Vector2[2];
			double num5 = (0.0 - num2 + Math.Sqrt(num2 * num2 - 4.0 * num * num3)) / (2.0 * num);
			ref Vector2 reference2 = ref array[0];
			reference2 = new Vector2((float)((double)line.Start.X + num5 * (double)(line.End.X - line.Start.X)), (float)((double)line.Start.Y + num5 * (double)(line.End.Y - line.Start.Y)));
			num5 = (0.0 - num2 - Math.Sqrt(num2 * num2 - 4.0 * num * num3)) / (2.0 * num);
			ref Vector2 reference3 = ref array[1];
			reference3 = new Vector2((float)((double)line.Start.X + num5 * (double)(line.End.X - line.Start.X)), (float)((double)line.Start.Y + num5 * (double)(line.End.Y - line.Start.Y)));
			return array;
		}
		return new Vector2[0];
	}

	public static bool PointWithinSegment(Line line, Vector2 point)
	{
		if (line.Start.X > line.End.X)
		{
			if (point.X > line.Start.X || point.X < line.End.X)
			{
				return false;
			}
		}
		else if (point.X > line.End.X || point.X < line.Start.X)
		{
			return false;
		}
		if (line.Start.Y > line.End.Y)
		{
			if (point.Y > line.Start.Y || point.Y < line.End.Y)
			{
				return false;
			}
		}
		else if (point.Y > line.End.Y || point.Y < line.Start.Y)
		{
			return false;
		}
		return true;
	}
}
