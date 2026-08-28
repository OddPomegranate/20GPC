using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.FruitsInARow;

internal class Counter
{
	private const float Radius = 40f;

	private const float Restitution = 0.2f;

	private const float Friction = 0.5f;

	private const float Gravity = 0.5f;

	private const float RestThreshold = 2f;

	private GamePlayer _owner;

	private Vector2 _position;

	private Vector2 _velocity;

	private float _rotation;

	private float _rotationalVelocity;

	private bool _gravity;

	private Rectangle _column;

	private Random _ranGen;

	public Counter(GamePlayer owner, Vector2 position, Rectangle column)
	{
		_owner = owner;
		_ranGen = new Random();
		_position = position;
		_velocity = new Vector2(((float)_ranGen.NextDouble() - 0.5f) * 2f, 0f);
		_gravity = true;
		_column = column;
	}

	public void Update(GameTime gameTime)
	{
		if (_gravity)
		{
			_velocity.Y += 0.5f;
		}
		if (_position.Y - 40f <= (float)_column.Top)
		{
			_position.Y = (float)_column.Top + 40f;
			_velocity.Y *= -0.2f;
		}
		else if (_position.Y + 40f >= (float)_column.Bottom)
		{
			_position.Y = (float)_column.Bottom - 40f;
			if (_velocity.Y < 2f)
			{
				_gravity = false;
				_velocity.Y = 0f;
			}
			else
			{
				_velocity.Y *= -0.2f;
			}
			_velocity.X -= 0.5f * (_velocity.X - _rotationalVelocity);
			_rotationalVelocity = _velocity.X * ((float)Math.PI * 80f) * 0.0001f;
		}
		if (_position.X - 40f <= (float)_column.Left)
		{
			_position.X = (float)_column.Left + 40f;
			_velocity.X += 0.5f * _rotationalVelocity;
			_velocity.X *= -0.2f;
			_velocity.Y -= 0.5f * (_velocity.Y + _rotationalVelocity);
			_rotationalVelocity = _velocity.Y * ((float)Math.PI * 80f) * 0.0001f;
		}
		else if (_position.X + 40f >= (float)_column.Right)
		{
			_position.X = (float)_column.Right - 40f;
			_velocity.X += 0.5f * _rotationalVelocity;
			_velocity.X *= -0.2f;
			_velocity.Y -= 0.5f * (_velocity.Y - _rotationalVelocity);
			_rotationalVelocity = _velocity.Y * ((float)Math.PI * 80f) * -0.0001f;
		}
		_position += _velocity;
		_rotation += _rotationalVelocity;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_owner.Sprite, _position, null, Color.White, _rotation, new Vector2((float)_owner.Sprite.Width / 2f, (float)_owner.Sprite.Height / 2f), 1f, SpriteEffects.None, 0f);
	}
}
