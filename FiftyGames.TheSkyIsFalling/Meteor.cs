using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.TheSkyIsFalling;

internal class Meteor
{
	private Random _random;

	private Vector2 _position;

	private Vector2 _previousPosition;

	private Vector2 _velocity;

	private Vector2 _origin;

	private float _scale;

	private Texture2D _sprite;

	private Color[] _spriteData;

	private float _rotation;

	private bool _active;

	private bool _dangerous;

	public bool Dangerious
	{
		set
		{
			_dangerous = value;
		}
	}

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

	public Texture2D Sprite => _sprite;

	public Color[] SpriteData => _spriteData;

	public Vector2 Origin => _origin;

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

	public float Rotation => _rotation;

	public bool Alive => _dangerous;

	public Meteor(Vector2 position, Vector2 velocity, float scale, Texture2D sprite, bool active, ref Random random)
	{
		_random = random;
		_position = position;
		_previousPosition = position;
		_velocity = velocity;
		_scale = scale;
		_sprite = sprite;
		_spriteData = new Color[sprite.Width * sprite.Height];
		sprite.GetData(_spriteData);
		_origin = new Vector2((float)sprite.Width / 2f, (float)sprite.Height / 2f);
		_rotation = 0f;
		_active = active;
		_dangerous = true;
	}

	public void Update(int timePassed, ref SoundManager soundManager)
	{
		if (_active)
		{
			_rotation += _velocity.Length() / 30f;
			_position += _velocity;
		}
		if (_dangerous && _position.Y + _scale * (float)_sprite.Width / 2f > 630f)
		{
			_dangerous = false;
			soundManager.CreateGameSoundCue("theSkyIsFalling Stone Land").Play();
		}
		if (_position.Y > 860f)
		{
			_position.Y = -200f;
			_dangerous = true;
			_position = new Vector2(_random.Next(0, 1280), -_random.Next(100, 200));
			_velocity = new Vector2(((float)_random.NextDouble() - 0.5f) * 3f, 2f + (float)_random.NextDouble() * 5f + (float)timePassed * 0.003f);
			if (_velocity.Length() > 10f)
			{
				_velocity.Normalize();
				_velocity *= 10f;
			}
			_scale = 0.25f + (float)_random.NextDouble() * 0.75f;
		}
		_previousPosition = _position;
	}

	public void Reset()
	{
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		Color color = Color.White;
		if (!_dangerous)
		{
			color = new Color((int)color.R, (int)color.G, (int)color.B, 0.1f);
		}
		spriteBatch.Draw(_sprite, _position, null, color, _rotation, _origin, _scale, SpriteEffects.None, 0f);
	}
}
