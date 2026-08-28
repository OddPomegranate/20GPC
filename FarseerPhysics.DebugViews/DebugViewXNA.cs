using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.DebugViews;

public class DebugViewXNA : DebugView, IDisposable
{
	private struct ContactPoint
	{
		public Vector2 Normal;

		public Vector2 Position;

		public PointState State;
	}

	private struct StringData(int x, int y, string s, object[] args, Color color)
	{
		public object[] Args = args;

		public Color Color = color;

		public string S = s;

		public int X = x;

		public int Y = y;
	}

	private const int MaxContactPoints = 2048;

	public const int CircleSegments = 16;

	private PrimitiveBatch _primitiveBatch;

	private SpriteBatch _batch;

	private SpriteFont _font;

	private GraphicsDevice _device;

	private Vector2[] _tempVertices = new Vector2[Settings.MaxPolygonVertices];

	private List<StringData> _stringData;

	private Matrix _localProjection;

	private Matrix _localView;

	public Color DefaultShapeColor = new Color(0.9f, 0.7f, 0.7f);

	public Color InactiveShapeColor = new Color(0.5f, 0.5f, 0.3f);

	public Color KinematicShapeColor = new Color(0.5f, 0.5f, 0.9f);

	public Color SleepingShapeColor = new Color(0.6f, 0.6f, 0.6f);

	public Color StaticShapeColor = new Color(0.5f, 0.9f, 0.5f);

	public Color TextColor = Color.White;

	private int _pointCount;

	private ContactPoint[] _points = new ContactPoint[2048];

	public Vector2 DebugPanelPosition = new Vector2(55f, 100f);

	private int _max;

	private int _avg;

	private int _min;

	public bool AdaptiveLimits = true;

	public int ValuesToGraph = 500;

	public int MinimumValue;

	public int MaximumValue = 1000;

	private List<float> _graphValues = new List<float>();

	public Rectangle PerformancePanelBounds = new Rectangle(265, 100, 200, 100);

	private Vector2[] _background = new Vector2[4];

	public bool Enabled = true;

	public DebugViewXNA(World world)
		: base(world)
	{
		ContactManager contactManager = world.ContactManager;
		contactManager.PreSolve = (PreSolveDelegate)Delegate.Combine(contactManager.PreSolve, new PreSolveDelegate(PreSolve));
		AppendFlags(DebugViewFlags.Shape);
		AppendFlags(DebugViewFlags.Controllers);
		AppendFlags(DebugViewFlags.Joint);
	}

	public void BeginCustomDraw(ref Matrix projection, ref Matrix view)
	{
		_primitiveBatch.Begin(ref projection, ref view);
	}

	public void EndCustomDraw()
	{
		_primitiveBatch.End();
	}

	public void Dispose()
	{
		ContactManager contactManager = base.World.ContactManager;
		contactManager.PreSolve = (PreSolveDelegate)Delegate.Remove(contactManager.PreSolve, new PreSolveDelegate(PreSolve));
	}

	private void PreSolve(Contact contact, ref Manifold oldManifold)
	{
		if ((base.Flags & DebugViewFlags.ContactPoints) != DebugViewFlags.ContactPoints)
		{
			return;
		}
		Manifold manifold = contact.Manifold;
		if (manifold.PointCount == 0)
		{
			return;
		}
		Fixture fixtureA = contact.FixtureA;
		FarseerPhysics.Collision.Collision.GetPointStates(out var _, out var state2, ref oldManifold, ref manifold);
		contact.GetWorldManifold(out var normal, out var points);
		for (int i = 0; i < manifold.PointCount; i++)
		{
			if (_pointCount >= 2048)
			{
				break;
			}
			if (fixtureA == null)
			{
				_points[i] = default(ContactPoint);
			}
			ContactPoint contactPoint = _points[_pointCount];
			contactPoint.Position = points[i];
			contactPoint.Normal = normal;
			contactPoint.State = state2[i];
			_points[_pointCount] = contactPoint;
			_pointCount++;
		}
	}

	private void DrawDebugData()
	{
		if ((base.Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
		{
			foreach (Body body in base.World.BodyList)
			{
				body.GetTransform(out var transform);
				foreach (Fixture fixture in body.FixtureList)
				{
					if (!body.Enabled)
					{
						DrawShape(fixture, transform, InactiveShapeColor);
					}
					else if (body.BodyType == BodyType.Static)
					{
						DrawShape(fixture, transform, StaticShapeColor);
					}
					else if (body.BodyType == BodyType.Kinematic)
					{
						DrawShape(fixture, transform, KinematicShapeColor);
					}
					else if (!body.Awake)
					{
						DrawShape(fixture, transform, SleepingShapeColor);
					}
					else
					{
						DrawShape(fixture, transform, DefaultShapeColor);
					}
				}
			}
		}
		if ((base.Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
		{
			for (int i = 0; i < _pointCount; i++)
			{
				ContactPoint contactPoint = _points[i];
				if (contactPoint.State == PointState.Add)
				{
					DrawPoint(contactPoint.Position, 0.1f, new Color(0.3f, 0.95f, 0.3f));
				}
				else if (contactPoint.State == PointState.Persist)
				{
					DrawPoint(contactPoint.Position, 0.1f, new Color(0.3f, 0.3f, 0.95f));
				}
				if ((base.Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
				{
					Vector2 position = contactPoint.Position;
					Vector2 end = position + 0.3f * contactPoint.Normal;
					DrawSegment(position, end, new Color(0.4f, 0.9f, 0.4f));
				}
			}
			_pointCount = 0;
		}
		if ((base.Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
		{
			foreach (Body body2 in base.World.BodyList)
			{
				foreach (Fixture fixture2 in body2.FixtureList)
				{
					if (fixture2.Shape is PolygonShape polygonShape)
					{
						body2.GetTransform(out var transform2);
						for (int j = 0; j < polygonShape.Vertices.Count; j++)
						{
							Vector2 p = MathUtils.Multiply(ref transform2, polygonShape.Vertices[j]);
							DrawPoint(p, 0.1f, Color.Red);
						}
					}
				}
			}
		}
		if ((base.Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
		{
			foreach (Joint joint in base.World.JointList)
			{
				DrawJoint(joint);
			}
		}
		if ((base.Flags & DebugViewFlags.Pair) == DebugViewFlags.Pair)
		{
			Color color = new Color(0.3f, 0.9f, 0.9f);
			for (int k = 0; k < base.World.ContactManager.ContactList.Count; k++)
			{
				Contact contact = base.World.ContactManager.ContactList[k];
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				fixtureA.GetAABB(out var aabb, 0);
				fixtureB.GetAABB(out var aabb2, 0);
				Vector2 center = aabb.Center;
				Vector2 center2 = aabb2.Center;
				DrawSegment(center, center2, color);
			}
		}
		if ((base.Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
		{
			Color color2 = new Color(0.9f, 0.3f, 0.9f);
			IBroadPhase broadPhase = base.World.ContactManager.BroadPhase;
			foreach (Body body3 in base.World.BodyList)
			{
				if (!body3.Enabled)
				{
					continue;
				}
				foreach (Fixture fixture3 in body3.FixtureList)
				{
					for (int l = 0; l < fixture3.ProxyCount; l++)
					{
						FixtureProxy fixtureProxy = fixture3.Proxies[l];
						broadPhase.GetFatAABB(fixtureProxy.ProxyId, out var aabb3);
						DrawAABB(ref aabb3, color2);
					}
				}
			}
		}
		if ((base.Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
		{
			foreach (Body body4 in base.World.BodyList)
			{
				body4.GetTransform(out var transform3);
				transform3.Position = body4.WorldCenter;
				DrawTransform(ref transform3);
			}
		}
		if ((base.Flags & DebugViewFlags.Controllers) == DebugViewFlags.Controllers)
		{
			for (int m = 0; m < base.World.ControllerList.Count; m++)
			{
				Controller controller = base.World.ControllerList[m];
				if (controller is BuoyancyController { Container: var aabb4 })
				{
					DrawAABB(ref aabb4, Color.LightBlue);
				}
			}
		}
		if ((base.Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
		{
			DrawDebugPanel();
		}
	}

	private void DrawPerformanceGraph()
	{
		_graphValues.Add(base.World.UpdateTime);
		if (_graphValues.Count > ValuesToGraph + 1)
		{
			_graphValues.RemoveAt(0);
		}
		float num = PerformancePanelBounds.X;
		float num2 = (float)PerformancePanelBounds.Width / (float)ValuesToGraph;
		float num3 = (float)PerformancePanelBounds.Bottom - (float)PerformancePanelBounds.Top;
		if (_graphValues.Count > 2)
		{
			_max = (int)_graphValues.Max();
			_avg = (int)_graphValues.Average();
			_min = (int)_graphValues.Min();
			if (AdaptiveLimits)
			{
				MaximumValue = _max;
				MinimumValue = 0;
			}
			for (int num4 = _graphValues.Count - 1; num4 > 0; num4--)
			{
				float value = (float)PerformancePanelBounds.Bottom - _graphValues[num4] / (float)(MaximumValue - MinimumValue) * num3;
				float value2 = (float)PerformancePanelBounds.Bottom - _graphValues[num4 - 1] / (float)(MaximumValue - MinimumValue) * num3;
				Vector2 start = new Vector2(MathHelper.Clamp(num, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(value, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
				Vector2 end = new Vector2(MathHelper.Clamp(num + num2, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(value2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
				DrawSegment(start, end, Color.LightGreen);
				num += num2;
			}
		}
		DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top, "Max: " + _max);
		DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Center.Y - 7, "Avg: " + _avg);
		DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15, "Min: " + _min);
		ref Vector2 reference = ref _background[0];
		reference = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
		ref Vector2 reference2 = ref _background[1];
		reference2 = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
		ref Vector2 reference3 = ref _background[2];
		reference3 = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
		ref Vector2 reference4 = ref _background[3];
		reference4 = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y);
		DrawSolidPolygon(_background, 4, Color.DarkGray, outline: true);
	}

	private void DrawDebugPanel()
	{
		int num = 0;
		for (int i = 0; i < base.World.BodyList.Count; i++)
		{
			num += base.World.BodyList[i].FixtureList.Count;
		}
		int num2 = (int)DebugPanelPosition.X;
		int y = (int)DebugPanelPosition.Y;
		DrawString(num2, y, "Objects:\n- Bodies: " + base.World.BodyList.Count + "\n- Fixtures: " + num + "\n- Contacts: " + base.World.ContactList.Count + "\n- Joints: " + base.World.JointList.Count + "\n- Controllers: " + base.World.ControllerList.Count + "\n- Proxies: " + base.World.ProxyCount);
		DrawString(num2 + 110, y, "Update time:\n- Body: " + base.World.SolveUpdateTime + "\n- Contact: " + base.World.ContactsUpdateTime + "\n- CCD: " + base.World.ContinuousPhysicsTime + "\n- Joint: " + base.World.Island.JointUpdateTime + "\n- Controller: " + base.World.ControllersUpdateTime + "\n- Total: " + base.World.UpdateTime);
	}

	public void DrawAABB(ref AABB aabb, Color color)
	{
		DrawPolygon(new Vector2[4]
		{
			new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y),
			new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y),
			new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y),
			new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y)
		}, 4, color);
	}

	private void DrawJoint(Joint joint)
	{
		if (joint.Enabled)
		{
			Body bodyA = joint.BodyA;
			Body bodyB = joint.BodyB;
			bodyA.GetTransform(out var transform);
			Vector2 vector = Vector2.Zero;
			if (!joint.IsFixedType())
			{
				bodyB.GetTransform(out var transform2);
				vector = transform2.Position;
			}
			Vector2 worldAnchorA = joint.WorldAnchorA;
			Vector2 worldAnchorB = joint.WorldAnchorB;
			Vector2 position = transform.Position;
			Color color = new Color(0.5f, 0.8f, 0.8f);
			switch (joint.JointType)
			{
			case JointType.Distance:
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.Pulley:
			{
				PulleyJoint pulleyJoint = (PulleyJoint)joint;
				Vector2 groundAnchorA = pulleyJoint.GroundAnchorA;
				Vector2 groundAnchorB = pulleyJoint.GroundAnchorB;
				DrawSegment(groundAnchorA, worldAnchorA, color);
				DrawSegment(groundAnchorB, worldAnchorB, color);
				DrawSegment(groundAnchorA, groundAnchorB, color);
				break;
			}
			case JointType.FixedMouse:
				DrawPoint(worldAnchorA, 0.5f, new Color(0f, 1f, 0f));
				DrawSegment(worldAnchorA, worldAnchorB, new Color(0.8f, 0.8f, 0.8f));
				break;
			case JointType.Revolute:
				DrawSegment(worldAnchorB, worldAnchorA, color);
				DrawSolidCircle(worldAnchorB, 0.1f, Vector2.Zero, Color.Red);
				DrawSolidCircle(worldAnchorA, 0.1f, Vector2.Zero, Color.Blue);
				break;
			case JointType.FixedRevolute:
				DrawSegment(position, worldAnchorA, color);
				DrawSolidCircle(worldAnchorA, 0.1f, Vector2.Zero, Color.Pink);
				break;
			case JointType.FixedLine:
				DrawSegment(position, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.FixedDistance:
				DrawSegment(position, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.FixedPrismatic:
				DrawSegment(position, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				break;
			case JointType.Gear:
				DrawSegment(position, vector, color);
				break;
			default:
				DrawSegment(position, worldAnchorA, color);
				DrawSegment(worldAnchorA, worldAnchorB, color);
				DrawSegment(vector, worldAnchorB, color);
				break;
			case JointType.FixedAngle:
				break;
			}
		}
	}

	public void DrawShape(Fixture fixture, Transform xf, Color color)
	{
		switch (fixture.ShapeType)
		{
		case ShapeType.Circle:
		{
			CircleShape circleShape = (CircleShape)fixture.Shape;
			Vector2 center = MathUtils.Multiply(ref xf, circleShape.Position);
			float radius = circleShape.Radius;
			Vector2 col = xf.R.Col1;
			DrawSolidCircle(center, radius, col, color);
			break;
		}
		case ShapeType.Polygon:
		{
			PolygonShape polygonShape = (PolygonShape)fixture.Shape;
			int count2 = polygonShape.Vertices.Count;
			for (int j = 0; j < count2; j++)
			{
				ref Vector2 reference = ref _tempVertices[j];
				reference = MathUtils.Multiply(ref xf, polygonShape.Vertices[j]);
			}
			DrawSolidPolygon(_tempVertices, count2, color);
			break;
		}
		case ShapeType.Edge:
		{
			EdgeShape edgeShape = (EdgeShape)fixture.Shape;
			Vector2 start = MathUtils.Multiply(ref xf, edgeShape.Vertex1);
			Vector2 end = MathUtils.Multiply(ref xf, edgeShape.Vertex2);
			DrawSegment(start, end, color);
			break;
		}
		case ShapeType.Loop:
		{
			LoopShape loopShape = (LoopShape)fixture.Shape;
			int count = loopShape.Vertices.Count;
			Vector2 vector = MathUtils.Multiply(ref xf, loopShape.Vertices[count - 1]);
			DrawCircle(vector, 0.05f, color);
			for (int i = 0; i < count; i++)
			{
				Vector2 vector2 = MathUtils.Multiply(ref xf, loopShape.Vertices[i]);
				DrawSegment(vector, vector2, color);
				vector = vector2;
			}
			break;
		}
		}
	}

	public override void DrawPolygon(Vector2[] vertices, int count, float red, float green, float blue)
	{
		DrawPolygon(vertices, count, new Color(red, green, blue));
	}

	public void DrawPolygon(Vector2[] vertices, int count, Color color)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		for (int i = 0; i < count - 1; i++)
		{
			_primitiveBatch.AddVertex(vertices[i], color, PrimitiveType.LineList);
			_primitiveBatch.AddVertex(vertices[i + 1], color, PrimitiveType.LineList);
		}
		_primitiveBatch.AddVertex(vertices[count - 1], color, PrimitiveType.LineList);
		_primitiveBatch.AddVertex(vertices[0], color, PrimitiveType.LineList);
	}

	public override void DrawSolidPolygon(Vector2[] vertices, int count, float red, float green, float blue)
	{
		DrawSolidPolygon(vertices, count, new Color(red, green, blue), outline: true);
	}

	public void DrawSolidPolygon(Vector2[] vertices, int count, Color color)
	{
		DrawSolidPolygon(vertices, count, color, outline: true);
	}

	public void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		if (count == 2)
		{
			DrawPolygon(vertices, count, color);
			return;
		}
		Color color2 = color * (outline ? 0.5f : 1f);
		for (int i = 1; i < count - 1; i++)
		{
			_primitiveBatch.AddVertex(vertices[0], color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertices[i], color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertices[i + 1], color2, PrimitiveType.TriangleList);
		}
		if (outline)
		{
			DrawPolygon(vertices, count, color);
		}
	}

	public override void DrawCircle(Vector2 center, float radius, float red, float green, float blue)
	{
		DrawCircle(center, radius, new Color(red, green, blue));
	}

	public void DrawCircle(Vector2 center, float radius, Color color)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		double num = 0.0;
		for (int i = 0; i < 16; i++)
		{
			Vector2 vertex = center + radius * new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
			Vector2 vertex2 = center + radius * new Vector2((float)Math.Cos(num + Math.PI / 8.0), (float)Math.Sin(num + Math.PI / 8.0));
			_primitiveBatch.AddVertex(vertex, color, PrimitiveType.LineList);
			_primitiveBatch.AddVertex(vertex2, color, PrimitiveType.LineList);
			num += Math.PI / 8.0;
		}
	}

	public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float green, float blue)
	{
		DrawSolidCircle(center, radius, axis, new Color(red, green, blue));
	}

	public void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		double num = 0.0;
		Color color2 = color * 0.5f;
		Vector2 vertex = center + radius * new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
		num += Math.PI / 8.0;
		for (int i = 1; i < 15; i++)
		{
			Vector2 vertex2 = center + radius * new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
			Vector2 vertex3 = center + radius * new Vector2((float)Math.Cos(num + Math.PI / 8.0), (float)Math.Sin(num + Math.PI / 8.0));
			_primitiveBatch.AddVertex(vertex, color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertex2, color2, PrimitiveType.TriangleList);
			_primitiveBatch.AddVertex(vertex3, color2, PrimitiveType.TriangleList);
			num += Math.PI / 8.0;
		}
		DrawCircle(center, radius, color);
		DrawSegment(center, center + axis * radius, color);
	}

	public override void DrawSegment(Vector2 start, Vector2 end, float red, float green, float blue)
	{
		DrawSegment(start, end, new Color(red, green, blue));
	}

	public void DrawSegment(Vector2 start, Vector2 end, Color color)
	{
		if (!_primitiveBatch.IsReady())
		{
			throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
		}
		_primitiveBatch.AddVertex(start, color, PrimitiveType.LineList);
		_primitiveBatch.AddVertex(end, color, PrimitiveType.LineList);
	}

	public override void DrawTransform(ref Transform transform)
	{
		Vector2 position = transform.Position;
		Vector2 end = position + 0.4f * transform.R.Col1;
		DrawSegment(position, end, Color.Red);
		end = position + 0.4f * transform.R.Col2;
		DrawSegment(position, end, Color.Green);
	}

	public void DrawPoint(Vector2 p, float size, Color color)
	{
		Vector2[] array = new Vector2[4];
		float num = size / 2f;
		ref Vector2 reference = ref array[0];
		reference = p + new Vector2(0f - num, 0f - num);
		ref Vector2 reference2 = ref array[1];
		reference2 = p + new Vector2(num, 0f - num);
		ref Vector2 reference3 = ref array[2];
		reference3 = p + new Vector2(num, num);
		ref Vector2 reference4 = ref array[3];
		reference4 = p + new Vector2(0f - num, num);
		DrawSolidPolygon(array, 4, color, outline: true);
	}

	public void DrawString(int x, int y, string s, params object[] args)
	{
		_stringData.Add(new StringData(x, y, s, args, TextColor));
	}

	public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color)
	{
		DrawSegment(start, end, color);
		float num = width / 2f;
		Vector2 vector = start - end;
		vector.Normalize();
		float radians = (float)Math.Atan2(vector.X, 0f - vector.Y);
		Matrix matrix = Matrix.CreateRotationZ(radians);
		Matrix matrix2 = Matrix.CreateTranslation(end.X, end.Y, 0f);
		Vector2[] array = new Vector2[3]
		{
			new Vector2(0f, 0f),
			new Vector2(0f - num, 0f - length),
			new Vector2(num, 0f - length)
		};
		Vector2.Transform(array, ref matrix, array);
		Vector2.Transform(array, ref matrix2, array);
		DrawSolidPolygon(array, 3, color, outline: false);
		if (drawStartIndicator)
		{
			Matrix matrix3 = Matrix.CreateTranslation(start.X, start.Y, 0f);
			Vector2[] array2 = new Vector2[4]
			{
				new Vector2(0f - num, length / 4f),
				new Vector2(num, length / 4f),
				new Vector2(num, 0f),
				new Vector2(0f - num, 0f)
			};
			Vector2.Transform(array2, ref matrix, array2);
			Vector2.Transform(array2, ref matrix3, array2);
			DrawSolidPolygon(array2, 4, color, outline: false);
		}
	}

	public void RenderDebugData(ref Matrix projection, ref Matrix view)
	{
		if (Enabled && base.Flags != 0)
		{
			_device.RasterizerState = RasterizerState.CullNone;
			_device.DepthStencilState = DepthStencilState.Default;
			_primitiveBatch.Begin(ref projection, ref view);
			DrawDebugData();
			_primitiveBatch.End();
			if ((base.Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
			{
				_primitiveBatch.Begin(ref _localProjection, ref _localView);
				DrawPerformanceGraph();
				_primitiveBatch.End();
			}
			_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			for (int i = 0; i < _stringData.Count; i++)
			{
				_batch.DrawString(_font, string.Format(_stringData[i].S, _stringData[i].Args), new Vector2((float)_stringData[i].X + 1f, (float)_stringData[i].Y + 1f), Color.Black);
				_batch.DrawString(_font, string.Format(_stringData[i].S, _stringData[i].Args), new Vector2(_stringData[i].X, _stringData[i].Y), _stringData[i].Color);
			}
			_batch.End();
			_stringData.Clear();
		}
	}

	public void RenderDebugData(ref Matrix projection)
	{
		if (Enabled)
		{
			Matrix view = Matrix.Identity;
			RenderDebugData(ref projection, ref view);
		}
	}

	public void LoadContent(GraphicsDevice device, ContentManager content)
	{
		_device = device;
		_batch = new SpriteBatch(_device);
		_primitiveBatch = new PrimitiveBatch(_device, 1000);
		_font = content.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_stringData = new List<StringData>();
		_localProjection = Matrix.CreateOrthographicOffCenter(0f, _device.Viewport.Width, _device.Viewport.Height, 0f, 0f, 1f);
		_localView = Matrix.Identity;
	}
}
