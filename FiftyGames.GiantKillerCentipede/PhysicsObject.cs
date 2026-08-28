using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class PhysicsObject
{
	protected Vector2 _position;

	protected Vector2 _velocity;

	protected float _rotation;

	protected float _scale;

	protected Texture2D _sprite;

	protected Vector2 _origin;

	protected Color _colour;

	protected BoundingSphere _physVolume;

	public virtual Vector2 Position
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

	public virtual Vector2 Velocity
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

	public virtual float Rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
		}
	}

	public virtual float Scale
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

	public virtual Color Colour
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

	public virtual Texture2D Sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			_sprite = value;
			_origin = new Vector2((float)_sprite.Width / 2f, (float)_sprite.Height / 2f);
		}
	}

	public virtual BoundingSphere CollisionVolume
	{
		get
		{
			return _physVolume;
		}
		set
		{
			_physVolume = value;
		}
	}

	public PhysicsObject()
	{
		_position = Vector2.Zero;
		_velocity = Vector2.Zero;
		_rotation = 0f;
		_scale = 1f;
		_sprite = null;
		_colour = Color.White;
		_origin = Vector2.Zero;
		_physVolume = new BoundingSphere(new Vector3(_position, 0f), 20f);
	}

	public virtual void Load(ContentManager contentLoader)
	{
		_origin = new Vector2((float)_sprite.Width / 2f, (float)_sprite.Height / 2f);
	}

	public virtual void Update(GameTime gameTime)
	{
		_position += _velocity;
		_physVolume.Center = new Vector3(_position, 0f);
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
		if (_sprite != null)
		{
			spriteBatch.Draw(_sprite, _position, null, _colour, _rotation, _origin, _scale, SpriteEffects.None, 0f);
		}
	}
}
