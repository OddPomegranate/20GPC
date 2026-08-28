using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.LunarLander;

internal class Wall
{
	private VertexPositionColor[] _shapeVerts;

	private short[] _shapeIndex;

	private BoundingBox _physVolume;

	private Vector2 _normal;

	public Vector2 Start
	{
		get
		{
			return new Vector2(_shapeVerts[0].Position.X, _shapeVerts[0].Position.Y);
		}
		set
		{
			_shapeVerts[0].Position = new Vector3(value, 0f);
			UpdatePhysics();
		}
	}

	public Vector2 End
	{
		get
		{
			return new Vector2(_shapeVerts[1].Position.X, _shapeVerts[1].Position.Y);
		}
		set
		{
			_shapeVerts[1].Position = new Vector3(value, 0f);
			UpdatePhysics();
		}
	}

	public BoundingBox CollisionVolume => _physVolume;

	public Vector2 CollisionNormal => _normal;

	public Wall(Vector2 start, Vector2 end)
	{
		_shapeVerts = new VertexPositionColor[8];
		ref VertexPositionColor reference = ref _shapeVerts[0];
		reference = new VertexPositionColor(new Vector3(start.X, start.Y, 0f), Color.White);
		ref VertexPositionColor reference2 = ref _shapeVerts[1];
		reference2 = new VertexPositionColor(new Vector3(end.X, end.Y, 0f), Color.White);
		_shapeIndex = new short[2] { 0, 1 };
		UpdatePhysics();
	}

	public void Draw(LineRender graphics)
	{
		graphics.DrawIndexedShape(_shapeVerts, _shapeIndex);
	}

	public void UpdatePhysics()
	{
		Vector2 vector = new Vector2(_shapeVerts[0].Position.X, _shapeVerts[0].Position.Y);
		Vector2 vector2 = new Vector2(_shapeVerts[1].Position.X, _shapeVerts[1].Position.Y);
		_physVolume.Min = new Vector3(Math.Min(vector.X, vector2.X), Math.Min(vector.Y, vector2.Y), 0f);
		_physVolume.Max = new Vector3(Math.Max(vector.X, vector2.X), Math.Max(vector.Y, vector2.Y), 0f);
		_normal.X = (vector2 - vector).Y;
		_normal.Y = (vector2 - vector).X * -1f;
		_normal.Normalize();
	}
}
