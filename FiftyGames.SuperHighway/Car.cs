using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SuperHighway;

internal class Car
{
	public const float CarHalfWidth = 0.05f;

	public const float CarHalfLength = 0.05f;

	public const float CarHeight = 30f;

	protected Color _colour;

	protected Vector2 _position;

	protected Vector2 _velocity;

	protected BoundingBox _physVolume;

	protected bool _alive;

	protected Cue _engineSound;

	public Vector2 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public Vector2 Velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;
		}
	}

	public Color Colour => _colour;

	public bool IsAlive
	{
		get
		{
			return _alive;
		}
		set
		{
			_alive = value;
		}
	}

	public BoundingBox CollisionVolume => _physVolume;

	public Cue EngineSound
	{
		get
		{
			return _engineSound;
		}
		set
		{
			_engineSound = value;
			_engineSound.Play();
		}
	}

	public Car(Vector2 position)
	{
		_position = position;
		_velocity = Vector2.Zero;
		_physVolume = default(BoundingBox);
		_alive = true;
	}

	public virtual void Update(GameTime gameTime)
	{
		_position.X += _velocity.X;
		_position.Y += _velocity.Y * _position.Y;
		if (_position.X < 0.05f)
		{
			_position.X = 0.05f;
			_velocity.X *= -1f;
		}
		if (_position.X > 0.95f)
		{
			_position.X = 0.95f;
			_velocity.X *= -1f;
		}
		_physVolume.Min.X = _position.X - 0.05f;
		_physVolume.Min.Y = _position.Y - 0.05f * _position.Y;
		_physVolume.Min.Z = 0f;
		_physVolume.Max.X = _position.X + 0.05f;
		_physVolume.Max.Y = _position.Y + 0.05f * _position.Y;
		_physVolume.Max.Z = 0f;
		if (_engineSound != null)
		{
			_engineSound.SetVariable("Speed", 100f - (_position.Y - 0.1f) * 62.5f);
		}
	}

	public virtual void Draw(LineRender graphics)
	{
		VertexPositionColor[] vertices = new VertexPositionColor[4]
		{
			new VertexPositionColor(new Vector3(_physVolume.Min.X * 640f, _physVolume.Min.Y * 360f, 0f), _colour),
			new VertexPositionColor(new Vector3(_physVolume.Max.X * 640f, _physVolume.Min.Y * 360f, 0f), _colour),
			new VertexPositionColor(new Vector3(_physVolume.Min.X * 640f, _physVolume.Max.Y * 360f, 0f), _colour),
			new VertexPositionColor(new Vector3(_physVolume.Max.X * 640f, _physVolume.Max.Y * 360f, 0f), _colour)
		};
		short[] indices = new short[8] { 0, 1, 1, 3, 3, 2, 2, 0 };
		graphics.DrawIndexedShape(vertices, indices);
	}
}
