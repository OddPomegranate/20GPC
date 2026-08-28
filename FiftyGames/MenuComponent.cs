using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class MenuComponent
{
	public enum Anchor
	{
		TopLeft,
		TopCentre,
		TopRight,
		MiddleLeft,
		Centre,
		MiddleRight,
		BottomLeft,
		BottomCentre,
		BottomRight
	}

	protected Anchor _anchor;

	protected Vector2 _position;

	protected Vector2 _size;

	protected float _rotation;

	protected Color _colour;

	protected float _depth;

	protected Vector2 _desiredPosition;

	protected Vector2 _desiredSize;

	protected float _desiredRotation;

	protected Color _desiredColour;

	protected float _moveSpeed;

	protected float _scaleSpeed;

	protected float _rotateSpeed;

	protected float _colourBlendSpeed;

	protected Texture2D _sprite;

	protected Vector2 _origin;

	protected Rectangle _sourceRect;

	public virtual Anchor PositionAnchor
	{
		get
		{
			return _anchor;
		}
		set
		{
			_anchor = value;
		}
	}

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

	public virtual Vector2 Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
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

	public virtual float Depth
	{
		get
		{
			return _depth;
		}
		set
		{
			_depth = value;
		}
	}

	public virtual Vector2 DesiredPosition
	{
		get
		{
			return _desiredPosition;
		}
		set
		{
			_desiredPosition = value;
		}
	}

	public virtual Vector2 DesiredSize
	{
		get
		{
			return _desiredSize;
		}
		set
		{
			_desiredSize = value;
		}
	}

	public virtual float DesiredRotation
	{
		get
		{
			return _desiredRotation;
		}
		set
		{
			_desiredRotation = value;
		}
	}

	public virtual Color DesiredColour
	{
		get
		{
			return _desiredColour;
		}
		set
		{
			_desiredColour = value;
		}
	}

	public virtual float MoveSpeed
	{
		get
		{
			return _moveSpeed;
		}
		set
		{
			_moveSpeed = value;
		}
	}

	public virtual float ScaleSpeed
	{
		get
		{
			return _scaleSpeed;
		}
		set
		{
			_scaleSpeed = value;
		}
	}

	public virtual float RotateSpeed
	{
		get
		{
			return _rotateSpeed;
		}
		set
		{
			_rotateSpeed = value;
		}
	}

	public virtual float ColourBlendSpeed
	{
		get
		{
			return _colourBlendSpeed;
		}
		set
		{
			_colourBlendSpeed = value;
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
		}
	}

	public virtual Rectangle SpriteFrame
	{
		get
		{
			return _sourceRect;
		}
		set
		{
			_sourceRect = value;
		}
	}

	public virtual Vector2 SpriteOrigin
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

	public MenuComponent()
	{
		_anchor = Anchor.Centre;
		_position = Vector2.Zero;
		_size = Vector2.Zero;
		_rotation = 0f;
		_colour = Color.White;
		_depth = 0f;
		_desiredPosition = _position;
		_desiredSize = _size;
		_desiredRotation = _rotation;
		_desiredColour = _colour;
		_moveSpeed = 80f;
		_scaleSpeed = 1f;
		_rotateSpeed = 0.1f;
		_colourBlendSpeed = 0.08f;
		_sprite = null;
		_sourceRect = default(Rectangle);
		_origin = Vector2.Zero;
	}

	public virtual void Load(ContentManager contentLoader)
	{
		FitComponentToImage();
	}

	public virtual void Update(GameTime gameTime)
	{
		if ((_position - _desiredPosition).Length() > _moveSpeed)
		{
			Vector2 vector = _desiredPosition - _position;
			vector.Normalize();
			_position += vector * _moveSpeed;
		}
		else
		{
			_position.X = _desiredPosition.X;
			_position.Y = _desiredPosition.Y;
		}
		if ((_size - _desiredSize).Length() > _scaleSpeed)
		{
			Vector2 vector2 = _desiredSize - _size;
			vector2.Normalize();
			_size += vector2 * _scaleSpeed;
		}
		else
		{
			_size.X = _desiredSize.X;
			_size.Y = _desiredSize.Y;
		}
		if (Math.Abs(_rotation - _desiredRotation) > _rotateSpeed)
		{
			float num = _desiredRotation - _rotation;
			num /= Math.Abs(num);
			_rotation += num * _rotateSpeed;
		}
		else
		{
			_rotation = _desiredRotation;
		}
		if ((_colour.ToVector4() - _desiredColour.ToVector4()).Length() > _colourBlendSpeed)
		{
			Vector4 vector3 = _desiredColour.ToVector4() - _colour.ToVector4();
			vector3.Normalize();
			_colour = new Color(_colour.ToVector4() + vector3 * _colourBlendSpeed);
		}
		else
		{
			_colour = _desiredColour;
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
		if (_sprite != null)
		{
			Rectangle destinationRectangle = new Rectangle(0, 0, (int)_size.X, (int)_size.Y);
			switch (_anchor)
			{
			case Anchor.TopLeft:
				destinationRectangle.X = (int)_position.X;
				destinationRectangle.Y = (int)_position.Y;
				break;
			case Anchor.TopCentre:
				destinationRectangle.X = (int)(_position.X - _size.X * 0.5f);
				destinationRectangle.Y = (int)_position.Y;
				break;
			case Anchor.TopRight:
				destinationRectangle.X = (int)(_position.X - _size.X);
				destinationRectangle.Y = (int)_position.Y;
				break;
			case Anchor.MiddleLeft:
				destinationRectangle.X = (int)_position.X;
				destinationRectangle.Y = (int)(_position.Y - _size.Y * 0.5f);
				break;
			case Anchor.Centre:
				destinationRectangle.X = (int)(_position.X - _size.X * 0.5f);
				destinationRectangle.Y = (int)(_position.Y - _size.Y * 0.5f);
				break;
			case Anchor.MiddleRight:
				destinationRectangle.X = (int)(_position.X - _size.X);
				destinationRectangle.Y = (int)(_position.Y - _size.Y * 0.5f);
				break;
			case Anchor.BottomLeft:
				destinationRectangle.X = (int)_position.X;
				destinationRectangle.Y = (int)(_position.Y - _size.Y);
				break;
			case Anchor.BottomCentre:
				destinationRectangle.X = (int)(_position.X - _size.X * 0.5f);
				destinationRectangle.Y = (int)(_position.Y - _size.Y);
				break;
			case Anchor.BottomRight:
				destinationRectangle.X = (int)(_position.X - _size.X);
				destinationRectangle.Y = (int)(_position.Y - _size.Y);
				break;
			default:
				destinationRectangle = _sourceRect;
				break;
			}
			spriteBatch.Draw(_sprite, destinationRectangle, _sourceRect, _colour, _rotation, _origin, SpriteEffects.None, _depth);
		}
	}

	public void FitComponentToImage()
	{
		if (_sprite != null)
		{
			_size = (_desiredSize = new Vector2(_sprite.Width, _sprite.Height));
			_sourceRect = _sprite.Bounds;
		}
	}
}
