using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class Light
{
	private Vector2 _position;

	private float _radius;

	private Texture2D _mask;

	private Texture2D _lightMask;

	private float _maskRotation;

	private Vector2 _maskScale;

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

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public Texture2D Mask => _mask;

	public Texture2D LightMask => _lightMask;

	public float MaskRotation
	{
		get
		{
			return _maskRotation;
		}
		set
		{
			_maskRotation = value;
		}
	}

	public Vector2 MaskScale
	{
		get
		{
			return _maskScale;
		}
		set
		{
			_maskScale = value;
		}
	}

	public bool HasMask
	{
		get
		{
			if (_mask != null)
			{
				return true;
			}
			return false;
		}
	}

	public Light(Vector2 position, float radius)
	{
		_position = position;
		_radius = radius;
		_mask = null;
		_maskRotation = 0f;
		_maskScale = Vector2.Zero;
	}

	public Light(Vector2 position, float radius, Texture2D mask, Texture2D lightMask, float rotation, Vector2 scale)
	{
		_position = position;
		_radius = radius;
		_mask = mask;
		_lightMask = lightMask;
		_maskRotation = rotation;
		_maskScale = scale;
	}
}
