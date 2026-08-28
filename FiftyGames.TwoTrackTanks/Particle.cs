using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.TwoTrackTanks;

internal class Particle
{
	private const float Scale = 1f;

	private Vector2 _position;

	private Vector2 _velocity;

	private float _rotateSpeed;

	private Texture2D _sprite;

	private Vector2 _origin;

	private Color _startColour;

	private Color _endColour;

	private int _life;

	private int _lifeSpan;

	public Texture2D Sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			_sprite = value;
			_origin = new Vector2((float)_sprite.Width * 0.5f, (float)_sprite.Height * 0.5f);
		}
	}

	public int LifeSpan
	{
		get
		{
			return _lifeSpan;
		}
		set
		{
			_lifeSpan = value;
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

	public float RotationalVelocity
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

	public bool IsExausted => _life > _lifeSpan;

	public Color StartColour
	{
		get
		{
			return _startColour;
		}
		set
		{
			_startColour = value;
		}
	}

	public Color EndColour
	{
		get
		{
			return _endColour;
		}
		set
		{
			_endColour = value;
		}
	}

	public Particle(Vector2 position)
	{
		_position = position;
		_velocity = Vector2.Zero;
		_rotateSpeed = 0f;
		_lifeSpan = 1000;
	}

	public void Update(GameTime gameTime)
	{
		_position += _velocity;
		_life += gameTime.ElapsedGameTime.Milliseconds;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		Color color = Color.Lerp(_startColour, _endColour, (float)_life / (float)_lifeSpan);
		float rotation = (float)_life / (float)_lifeSpan * ((float)Math.PI * 2f) * _rotateSpeed;
		float scale = ((float)_life / (float)_lifeSpan - 1f) * ((float)_life / (float)_lifeSpan - 1f) * 1f - 1f;
		spriteBatch.Draw(_sprite, _position, null, color, rotation, _origin, scale, SpriteEffects.None, 1f);
	}
}
