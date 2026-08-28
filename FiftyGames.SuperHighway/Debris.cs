using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SuperHighway;

internal class Debris
{
	private const float XSpeedCoeficient = 0.032f;

	private const float YSpeedCoeficient = 10f;

	private const float ZSpeedCoeficient = 0.008f;

	private const float RotationalSpeedCoeficient = 0.6f;

	private const float Gravity = 0.02f;

	private const float GroundSpeed = 0.06f;

	public Vector3 _position;

	public Vector3 _velocity;

	public Vector3 _rotation;

	public Vector3 _rotationalVelocity;

	public Color _colour;

	public Debris(Car destroyedCar, Random randomGenerator)
	{
		_position = new Vector3(destroyedCar.Position.X, -1f, destroyedCar.Position.Y);
		_colour = destroyedCar.Colour;
		_velocity = new Vector3(((float)randomGenerator.NextDouble() - 0.5f) * 0.032f, ((float)randomGenerator.NextDouble() - 1f) * 10f * (_position.Z * 4f), ((float)randomGenerator.NextDouble() - 0.5f) * 0.008f + 0.06f * _position.Z);
		_rotation = new Vector3((float)randomGenerator.NextDouble() - 0.5f, (float)randomGenerator.NextDouble() - 0.5f, (float)randomGenerator.NextDouble() - 0.5f);
		_rotationalVelocity = new Vector3(((float)randomGenerator.NextDouble() - 0.5f) * 0.6f, ((float)randomGenerator.NextDouble() - 0.5f) * 0.6f, ((float)randomGenerator.NextDouble() - 0.5f) * 0.6f);
	}

	public void Update(GameTime gameTime)
	{
		if (_position.Z < 0f)
		{
			_position.Z = 0f;
		}
		if (_position.Y < 0f)
		{
			_velocity.Y += 0.02f;
			_rotation += _rotationalVelocity;
		}
		else
		{
			_velocity.X = 0f;
			_velocity.Y = 0f;
			_velocity.Z = 0.06f * Math.Abs(_position.Z);
			_rotation.X = 0f;
			_rotation.Z = 0f;
		}
		_position += _velocity;
	}

	public void Draw(LineRender graphics)
	{
		VertexPositionColor[] vertices = new VertexPositionColor[4]
		{
			new VertexPositionColor(new Vector3((_position.X - 0.01f) * 640f, (_position.Z + _position.Y / 360f) * 360f, 0f), _colour),
			new VertexPositionColor(new Vector3((_position.X + 0.01f) * 640f, (_position.Z + _position.Y / 360f) * 360f, 0f), _colour),
			new VertexPositionColor(new Vector3((_position.X - 0.01f) * 640f, _position.Z * 360f, 0f), _colour),
			new VertexPositionColor(new Vector3((_position.X + 0.01f) * 640f, _position.Z * 360f, 0f), _colour)
		};
		short[] indices = new short[8] { 0, 1, 1, 3, 3, 2, 2, 0 };
		graphics.DrawIndexedShape(vertices, indices);
	}
}
