using System;
using MicroMachinesGame.ISHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.MicroMachines.Entities;

internal class NosSmoke
{
	private const float _multiplier = 0.9f;

	private Texture2D _texture;

	private float _alpha;

	private Vector2 _position;

	private float _speed;

	private float _direction;

	private float _twistSpeed;

	private float _rotation;

	private float _stepScalar;

	private Vector2 _scale;

	private Vector2 _textureCenter;

	private Random _random;

	public bool IsAlive { get; set; }

	public NosSmoke(Texture2D texture, Vector2 position, Vector2 carDirection, Random random)
	{
		_texture = texture;
		_textureCenter = new Vector2(_texture.Width / 2, _texture.Height / 2);
		_random = random;
		_position = position;
		_speed = 2 + random.Next(2);
		_direction = GeometryHelper.V2ToAngle(carDirection) - 180f - 45f + (float)random.Next(90);
		_twistSpeed = choose(-1, -2, -3, -4, 1, 2, 3, 4);
		_stepScalar = (float)(random.NextDouble() * 0.10000000149011612);
		float num = 0.1f + (float)random.NextDouble() * 0.3f;
		_scale = new Vector2(num, num);
		_rotation = random.Next(360);
		_alpha = 0.6f + (float)random.NextDouble() / 2.5f;
		IsAlive = true;
	}

	public void Update(GameTime gameTime)
	{
		_speed *= 0.9f;
		_position += GeometryHelper.AngleToV2(_direction, _speed);
		_alpha -= 0.03f;
		_rotation += _twistSpeed;
		_scale += new Vector2(_stepScalar, _stepScalar);
		if (_alpha < 0f)
		{
			IsAlive = false;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_texture, _position, null, Color.White * _alpha, MathHelper.ToRadians(_rotation), _textureCenter, _scale, SpriteEffects.None, 0f);
		spriteBatch.End();
	}

	private int choose(params int[] ints)
	{
		return ints[_random.Next(0, ints.Length)];
	}
}
