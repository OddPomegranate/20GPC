using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes;

public class LoopShape : Shape
{
	private static EdgeShape _edgeShape = new EdgeShape();

	public Vertices Vertices;

	public override int ChildCount => Vertices.Count;

	private LoopShape()
		: base(0f)
	{
		base.ShapeType = ShapeType.Loop;
		_radius = 0.01f;
	}

	public LoopShape(Vertices vertices)
		: base(0f)
	{
		base.ShapeType = ShapeType.Loop;
		_radius = 0.01f;
		Vertices = new Vertices(vertices);
	}

	public override Shape Clone()
	{
		LoopShape loopShape = new LoopShape();
		loopShape._density = _density;
		loopShape._radius = _radius;
		loopShape.Vertices = Vertices;
		loopShape.MassData = MassData;
		return loopShape;
	}

	public void GetChildEdge(ref EdgeShape edge, int index)
	{
		edge.ShapeType = ShapeType.Edge;
		edge._radius = _radius;
		edge.HasVertex0 = true;
		edge.HasVertex3 = true;
		int index2 = ((index - 1 >= 0) ? (index - 1) : (Vertices.Count - 1));
		int index3 = ((index + 1 < Vertices.Count) ? (index + 1) : 0);
		int num;
		for (num = index + 2; num >= Vertices.Count; num -= Vertices.Count)
		{
		}
		edge.Vertex0 = Vertices[index2];
		edge.Vertex1 = Vertices[index];
		edge.Vertex2 = Vertices[index3];
		edge.Vertex3 = Vertices[num];
	}

	public override bool TestPoint(ref Transform transform, ref Vector2 point)
	{
		return false;
	}

	public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
	{
		int num = childIndex + 1;
		if (num == Vertices.Count)
		{
			num = 0;
		}
		_edgeShape.Vertex1 = Vertices[childIndex];
		_edgeShape.Vertex2 = Vertices[num];
		return _edgeShape.RayCast(out output, ref input, ref transform, 0);
	}

	public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
	{
		int num = childIndex + 1;
		if (num == Vertices.Count)
		{
			num = 0;
		}
		Vector2 value = MathUtils.Multiply(ref transform, Vertices[childIndex]);
		Vector2 value2 = MathUtils.Multiply(ref transform, Vertices[num]);
		aabb.LowerBound = Vector2.Min(value, value2);
		aabb.UpperBound = Vector2.Max(value, value2);
	}

	public override void ComputeProperties()
	{
	}

	public override float ComputeSubmergedArea(Vector2 normal, float offset, Transform xf, out Vector2 sc)
	{
		sc = Vector2.Zero;
		return 0f;
	}
}
