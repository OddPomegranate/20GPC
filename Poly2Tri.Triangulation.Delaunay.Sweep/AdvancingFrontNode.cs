namespace Poly2Tri.Triangulation.Delaunay.Sweep;

public class AdvancingFrontNode
{
	public AdvancingFrontNode Next;

	public TriangulationPoint Point;

	public AdvancingFrontNode Prev;

	public DelaunayTriangle Triangle;

	public double Value;

	public bool HasNext => Next != null;

	public bool HasPrev => Prev != null;

	public AdvancingFrontNode(TriangulationPoint point)
	{
		Point = point;
		Value = point.X;
	}
}
