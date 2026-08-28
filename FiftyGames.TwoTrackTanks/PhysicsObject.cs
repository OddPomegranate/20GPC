using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.TwoTrackTanks;

internal class PhysicsObject
{
	protected Texture2D _sprite;

	protected Vector2 _origin;

	protected Color _colour;

	protected Body _physBody;

	protected float _scale;

	protected int _animationFrameCurrent;

	protected int _animationFrameTimer;

	protected int _animationFrames;

	protected Point _animationFrameSize;

	protected int _animationSpeed;

	protected Vector2 _position;

	protected SoundManager _soundManager;

	public Texture2D Sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			_sprite = value;
			_animationFrameSize = new Point(_sprite.Width, _sprite.Height);
			_origin = new Vector2((float)_animationFrameSize.X * 0.5f, (float)_animationFrameSize.Y * 0.5f);
		}
	}

	public Vector2 Position
	{
		get
		{
			if (_physBody != null)
			{
				return ConvertUnits.ToDisplayUnits(_physBody.Position);
			}
			return _position;
		}
		set
		{
			if (_physBody != null)
			{
				_physBody.Position = ConvertUnits.ToSimUnits(value);
			}
			else
			{
				_position = value;
			}
		}
	}

	public Point AnimationFrameSize
	{
		get
		{
			return _animationFrameSize;
		}
		set
		{
			_animationFrameSize = value;
			_origin = new Vector2((float)_animationFrameSize.X * 0.5f, (float)_animationFrameSize.Y * 0.5f);
		}
	}

	public Body PhysicsBody
	{
		get
		{
			return _physBody;
		}
		set
		{
			_physBody = value;
		}
	}

	public Vector2 Origin
	{
		get
		{
			return _origin;
		}
		set
		{
			_origin = value;
		}
	}

	public Color Colour
	{
		get
		{
			return _colour;
		}
		set
		{
			_colour = value;
		}
	}

	public float Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = value;
		}
	}

	public float Rotation
	{
		get
		{
			return _physBody.Rotation;
		}
		set
		{
			_physBody.Rotation = value;
		}
	}

	public int AnimationFrames
	{
		get
		{
			return _animationFrames;
		}
		set
		{
			_animationFrames = value;
		}
	}

	public int AnimationSpeed
	{
		get
		{
			return _animationSpeed;
		}
		set
		{
			_animationSpeed = value;
		}
	}

	public SoundManager SoundManager
	{
		get
		{
			return _soundManager;
		}
		set
		{
			_soundManager = value;
		}
	}

	public PhysicsObject()
	{
		_sprite = null;
		_origin = Vector2.Zero;
		_colour = Color.White;
		_physBody = null;
		_scale = 1f;
		_animationFrames = 1;
		_animationFrameSize = new Point(1, 1);
		_animationSpeed = 12;
		_animationFrameCurrent = 0;
	}

	public virtual void Update(GameTime gameTime)
	{
		if (_physBody != null)
		{
			_position = Position;
		}
		if (_physBody != null)
		{
			if (_physBody.LinearVelocity.Length() > 10f)
			{
				_physBody.LinearVelocity = Vector2.Normalize(_physBody.LinearVelocity) * 10f;
			}
			if (_physBody.AngularVelocity > 0.01f)
			{
				_physBody.AngularVelocity = 0.01f;
			}
			else if (_physBody.AngularVelocity < -0.01f)
			{
				_physBody.AngularVelocity = -0.01f;
			}
		}
		if (_animationFrames != 1 && _animationSpeed != 0)
		{
			if (_animationFrameTimer > 1000 / _animationSpeed)
			{
				_animationFrameCurrent++;
				_animationFrameTimer = 0;
			}
			_animationFrameTimer += gameTime.ElapsedGameTime.Milliseconds;
			if (_animationFrameCurrent == _animationFrames)
			{
				_animationFrameCurrent = 0;
			}
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
		if (_sprite != null && _physBody != null)
		{
			spriteBatch.Draw(sourceRectangle: new Rectangle(_animationFrameSize.X * _animationFrameCurrent, 0, _animationFrameSize.X, _animationFrameSize.Y), texture: _sprite, position: Position, color: _colour, rotation: Rotation, origin: _origin, scale: _scale, effects: SpriteEffects.None, layerDepth: 1f);
		}
	}
}
