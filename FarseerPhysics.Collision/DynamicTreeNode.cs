namespace FarseerPhysics.Collision;

internal struct DynamicTreeNode<T>
{
	internal AABB AABB;

	internal int Child1;

	internal int Child2;

	internal int LeafCount;

	internal int ParentOrNext;

	internal T UserData;

	internal bool IsLeaf()
	{
		return Child1 == -1;
	}
}
