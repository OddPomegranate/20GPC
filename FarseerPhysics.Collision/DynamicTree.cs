using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision;

public class DynamicTree<T>
{
	internal const int NullNode = -1;

	private static Stack<int> _stack = new Stack<int>(256);

	private int _freeList;

	private int _insertionCount;

	private int _nodeCapacity;

	private int _nodeCount;

	private DynamicTreeNode<T>[] _nodes;

	private int _path;

	private int _root;

	public DynamicTree()
	{
		_root = -1;
		_nodeCapacity = 16;
		_nodes = new DynamicTreeNode<T>[_nodeCapacity];
		for (int i = 0; i < _nodeCapacity - 1; i++)
		{
			_nodes[i].ParentOrNext = i + 1;
		}
		_nodes[_nodeCapacity - 1].ParentOrNext = -1;
	}

	public int AddProxy(ref AABB aabb, T userData)
	{
		int num = AllocateNode();
		Vector2 vector = new Vector2(0.1f, 0.1f);
		_nodes[num].AABB.LowerBound = aabb.LowerBound - vector;
		_nodes[num].AABB.UpperBound = aabb.UpperBound + vector;
		_nodes[num].UserData = userData;
		_nodes[num].LeafCount = 1;
		InsertLeaf(num);
		return num;
	}

	public void RemoveProxy(int proxyId)
	{
		RemoveLeaf(proxyId);
		FreeNode(proxyId);
	}

	public bool MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
	{
		if (_nodes[proxyId].AABB.Contains(ref aabb))
		{
			return false;
		}
		RemoveLeaf(proxyId);
		AABB aABB = aabb;
		Vector2 vector = new Vector2(0.1f, 0.1f);
		aABB.LowerBound -= vector;
		aABB.UpperBound += vector;
		Vector2 vector2 = 2f * displacement;
		if (vector2.X < 0f)
		{
			aABB.LowerBound.X += vector2.X;
		}
		else
		{
			aABB.UpperBound.X += vector2.X;
		}
		if (vector2.Y < 0f)
		{
			aABB.LowerBound.Y += vector2.Y;
		}
		else
		{
			aABB.UpperBound.Y += vector2.Y;
		}
		_nodes[proxyId].AABB = aABB;
		InsertLeaf(proxyId);
		return true;
	}

	public void Rebalance(int iterations)
	{
		if (_root == -1)
		{
			return;
		}
		for (int i = 0; i < iterations; i++)
		{
			int num = _root;
			int num2 = 0;
			while (!_nodes[num].IsLeaf())
			{
				num = ((((_path >> num2) & 1) == 0) ? _nodes[num].Child1 : _nodes[num].Child2);
				num2 = (num2 + 1) & 0x1F;
			}
			_path++;
			RemoveLeaf(num);
			InsertLeaf(num);
		}
	}

	public T GetUserData(int proxyId)
	{
		return _nodes[proxyId].UserData;
	}

	public void GetFatAABB(int proxyId, out AABB fatAABB)
	{
		fatAABB = _nodes[proxyId].AABB;
	}

	public int ComputeHeight()
	{
		return ComputeHeight(_root);
	}

	public void Query(Func<int, bool> callback, ref AABB aabb)
	{
		_stack.Clear();
		_stack.Push(_root);
		while (_stack.Count > 0)
		{
			int num = _stack.Pop();
			if (num == -1)
			{
				continue;
			}
			DynamicTreeNode<T> dynamicTreeNode = _nodes[num];
			if (!AABB.TestOverlap(ref dynamicTreeNode.AABB, ref aabb))
			{
				continue;
			}
			if (dynamicTreeNode.IsLeaf())
			{
				if (!callback(num))
				{
					break;
				}
			}
			else
			{
				_stack.Push(dynamicTreeNode.Child1);
				_stack.Push(dynamicTreeNode.Child2);
			}
		}
	}

	public void RayCast(Func<RayCastInput, int, float> callback, ref RayCastInput input)
	{
		Vector2 value = input.Point1;
		Vector2 point = input.Point2;
		Vector2 vector = point - value;
		vector.Normalize();
		Vector2 value2 = MathUtils.Abs(new Vector2(0f - vector.Y, vector.X));
		float num = input.MaxFraction;
		AABB b = default(AABB);
		Vector2 value3 = value + num * (point - value);
		Vector2.Min(ref value, ref value3, out b.LowerBound);
		Vector2.Max(ref value, ref value3, out b.UpperBound);
		_stack.Clear();
		_stack.Push(_root);
		RayCastInput arg = default(RayCastInput);
		while (_stack.Count > 0)
		{
			int num2 = _stack.Pop();
			if (num2 == -1)
			{
				continue;
			}
			DynamicTreeNode<T> dynamicTreeNode = _nodes[num2];
			if (!AABB.TestOverlap(ref dynamicTreeNode.AABB, ref b))
			{
				continue;
			}
			Vector2 center = dynamicTreeNode.AABB.Center;
			Vector2 extents = dynamicTreeNode.AABB.Extents;
			float num3 = Math.Abs(Vector2.Dot(new Vector2(0f - vector.Y, vector.X), value - center)) - Vector2.Dot(value2, extents);
			if (num3 > 0f)
			{
				continue;
			}
			if (dynamicTreeNode.IsLeaf())
			{
				arg.Point1 = input.Point1;
				arg.Point2 = input.Point2;
				arg.MaxFraction = num;
				float num4 = callback(arg, num2);
				if (num4 == 0f)
				{
					break;
				}
				if (num4 > 0f)
				{
					num = num4;
					Vector2 value4 = value + num * (point - value);
					b.LowerBound = Vector2.Min(value, value4);
					b.UpperBound = Vector2.Max(value, value4);
				}
			}
			else
			{
				_stack.Push(dynamicTreeNode.Child1);
				_stack.Push(dynamicTreeNode.Child2);
			}
		}
	}

	private int CountLeaves(int nodeId)
	{
		if (nodeId == -1)
		{
			return 0;
		}
		DynamicTreeNode<T> dynamicTreeNode = _nodes[nodeId];
		if (dynamicTreeNode.IsLeaf())
		{
			return 1;
		}
		int num = CountLeaves(dynamicTreeNode.Child1);
		int num2 = CountLeaves(dynamicTreeNode.Child2);
		return num + num2;
	}

	private void Validate()
	{
		CountLeaves(_root);
	}

	private int AllocateNode()
	{
		if (_freeList == -1)
		{
			DynamicTreeNode<T>[] nodes = _nodes;
			_nodeCapacity *= 2;
			_nodes = new DynamicTreeNode<T>[_nodeCapacity];
			Array.Copy(nodes, _nodes, _nodeCount);
			for (int i = _nodeCount; i < _nodeCapacity - 1; i++)
			{
				_nodes[i].ParentOrNext = i + 1;
			}
			_nodes[_nodeCapacity - 1].ParentOrNext = -1;
			_freeList = _nodeCount;
		}
		int freeList = _freeList;
		_freeList = _nodes[freeList].ParentOrNext;
		_nodes[freeList].ParentOrNext = -1;
		_nodes[freeList].Child1 = -1;
		_nodes[freeList].Child2 = -1;
		_nodes[freeList].LeafCount = 0;
		_nodeCount++;
		return freeList;
	}

	private void FreeNode(int nodeId)
	{
		_nodes[nodeId].ParentOrNext = _freeList;
		_freeList = nodeId;
		_nodeCount--;
	}

	private void InsertLeaf(int leaf)
	{
		_insertionCount++;
		if (_root == -1)
		{
			_root = leaf;
			_nodes[_root].ParentOrNext = -1;
			return;
		}
		AABB aabb = _nodes[leaf].AABB;
		int num = _root;
		while (!_nodes[num].IsLeaf())
		{
			int child = _nodes[num].Child1;
			int child2 = _nodes[num].Child2;
			_nodes[num].AABB.Combine(ref aabb);
			_nodes[num].LeafCount++;
			float perimeter = _nodes[num].AABB.Perimeter;
			AABB aABB = default(AABB);
			aABB.Combine(ref _nodes[num].AABB, ref aabb);
			float perimeter2 = aABB.Perimeter;
			float num2 = 2f * perimeter2;
			float num3 = 2f * (perimeter2 - perimeter);
			float num4;
			if (_nodes[child].IsLeaf())
			{
				AABB aABB2 = default(AABB);
				aABB2.Combine(ref aabb, ref _nodes[child].AABB);
				num4 = aABB2.Perimeter + num3;
			}
			else
			{
				AABB aABB3 = default(AABB);
				aABB3.Combine(ref aabb, ref _nodes[child].AABB);
				float perimeter3 = _nodes[child].AABB.Perimeter;
				float perimeter4 = aABB3.Perimeter;
				num4 = perimeter4 - perimeter3 + num3;
			}
			float num5;
			if (_nodes[child2].IsLeaf())
			{
				AABB aABB4 = default(AABB);
				aABB4.Combine(ref aabb, ref _nodes[child2].AABB);
				num5 = aABB4.Perimeter + num3;
			}
			else
			{
				AABB aABB5 = default(AABB);
				aABB5.Combine(ref aabb, ref _nodes[child2].AABB);
				float perimeter5 = _nodes[child2].AABB.Perimeter;
				float perimeter6 = aABB5.Perimeter;
				num5 = perimeter6 - perimeter5 + num3;
			}
			if (num2 < num4 && num2 < num5)
			{
				break;
			}
			_nodes[num].AABB.Combine(ref aabb);
			num = ((!(num4 < num5)) ? child2 : child);
		}
		int parentOrNext = _nodes[num].ParentOrNext;
		int num6 = AllocateNode();
		_nodes[num6].ParentOrNext = parentOrNext;
		_nodes[num6].UserData = default(T);
		_nodes[num6].AABB.Combine(ref aabb, ref _nodes[num].AABB);
		_nodes[num6].LeafCount = _nodes[num].LeafCount + 1;
		if (parentOrNext != -1)
		{
			if (_nodes[parentOrNext].Child1 == num)
			{
				_nodes[parentOrNext].Child1 = num6;
			}
			else
			{
				_nodes[parentOrNext].Child2 = num6;
			}
			_nodes[num6].Child1 = num;
			_nodes[num6].Child2 = leaf;
			_nodes[num].ParentOrNext = num6;
			_nodes[leaf].ParentOrNext = num6;
		}
		else
		{
			_nodes[num6].Child1 = num;
			_nodes[num6].Child2 = leaf;
			_nodes[num].ParentOrNext = num6;
			_nodes[leaf].ParentOrNext = num6;
			_root = num6;
		}
	}

	private void RemoveLeaf(int leaf)
	{
		if (leaf == _root)
		{
			_root = -1;
			return;
		}
		int parentOrNext = _nodes[leaf].ParentOrNext;
		int parentOrNext2 = _nodes[parentOrNext].ParentOrNext;
		int num = ((_nodes[parentOrNext].Child1 != leaf) ? _nodes[parentOrNext].Child1 : _nodes[parentOrNext].Child2);
		if (parentOrNext2 != -1)
		{
			if (_nodes[parentOrNext2].Child1 == parentOrNext)
			{
				_nodes[parentOrNext2].Child1 = num;
			}
			else
			{
				_nodes[parentOrNext2].Child2 = num;
			}
			_nodes[num].ParentOrNext = parentOrNext2;
			FreeNode(parentOrNext);
			for (parentOrNext = parentOrNext2; parentOrNext != -1; parentOrNext = _nodes[parentOrNext].ParentOrNext)
			{
				_nodes[parentOrNext].AABB.Combine(ref _nodes[_nodes[parentOrNext].Child1].AABB, ref _nodes[_nodes[parentOrNext].Child2].AABB);
				_nodes[parentOrNext].LeafCount--;
			}
		}
		else
		{
			_root = num;
			_nodes[num].ParentOrNext = -1;
			FreeNode(parentOrNext);
		}
	}

	private int ComputeHeight(int nodeId)
	{
		if (nodeId == -1)
		{
			return 0;
		}
		DynamicTreeNode<T> dynamicTreeNode = _nodes[nodeId];
		int val = ComputeHeight(dynamicTreeNode.Child1);
		int val2 = ComputeHeight(dynamicTreeNode.Child2);
		return 1 + Math.Max(val, val2);
	}
}
