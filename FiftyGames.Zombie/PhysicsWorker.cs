using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie;

internal static class PhysicsWorker
{
	public struct BodySubmission
	{
		public Shape _bodyShape;

		public Category _bodyCategory;

		public Vector2 _bodyPosition;

		public float _mass;

		public object _userData;
	}

	public struct ChangePositionSubmission
	{
		public int _id;

		public Vector2 _newPosition;
	}

	private static List<Body> _bodyList;

	private static Queue<BodySubmission> _bodySubmissions;

	private static Queue<ChangePositionSubmission> _changePositionsSubmissions;

	public static void InitPhysicsWorker()
	{
		_bodyList = new List<Body>();
		_bodySubmissions = new Queue<BodySubmission>();
		_changePositionsSubmissions = new Queue<ChangePositionSubmission>();
	}

	public static int AddBody(Shape shape, Category category, Vector2 position, float mass, object userData)
	{
		BodySubmission item = default(BodySubmission);
		item._bodyShape = shape;
		item._bodyCategory = category;
		item._bodyPosition = position;
		item._mass = mass;
		item._userData = userData;
		lock (_bodySubmissions)
		{
			_bodySubmissions.Enqueue(item);
		}
		return _bodyList.Count;
	}

	public static void SetPosition(int bodyIndex, Vector2 newPosition)
	{
		ChangePositionSubmission item = default(ChangePositionSubmission);
		item._id = bodyIndex;
		item._newPosition = newPosition;
		lock (_changePositionsSubmissions)
		{
			_changePositionsSubmissions.Enqueue(item);
		}
	}

	public static BodySubmission GetNextBodySubmission()
	{
		lock (_bodySubmissions)
		{
			return _bodySubmissions.Dequeue();
		}
	}

	public static ChangePositionSubmission GetNextPositionChange()
	{
		lock (_changePositionsSubmissions)
		{
			return _changePositionsSubmissions.Dequeue();
		}
	}
}
