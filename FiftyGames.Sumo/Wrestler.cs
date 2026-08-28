using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Sumo;

internal class Wrestler
{
	private Player _player1;

	private Player _player2;

	private Texture2D _originalSprite;

	private Texture2D _armOverlay;

	private Texture2D _armUnderlay;

	private Texture2D _sumoOverlay;

	private Texture2D _sumoUnderlay;

	private SpriteFont _font;

	private Vector2 _center;

	private Vector2 _origin;

	private Vector2 _originalOrigin;

	private Vector2 _position;

	private Vector2 _velocity;

	private Vector2 _startPosition;

	private float _rotation;

	private float _leftPower;

	private float _rightPower;

	private float _pushPower;

	private float _strugglePower;

	private bool _active;

	private int _winner;

	private Random _random;

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			if (!value)
			{
				Vector2 vector = _position - _center;
				float angle = (float)Math.Atan2(vector.Y, vector.X);
				angle = MathHelper.WrapAngle(angle);
				float angle2 = _rotation - angle;
				angle2 = MathHelper.WrapAngle(angle2);
				if (angle2 < (float)Math.PI / 2f && angle2 > -(float)Math.PI / 2f)
				{
					_winner = 0;
				}
				else
				{
					_winner = 1;
				}
			}
			_active = value;
		}
	}

	public int Winner => _winner;

	public Vector2 Position => _position;

	public Vector2 Center => _center;

	public Wrestler(Player player1, Player player2, Texture2D sprite, Texture2D armOverlay, Texture2D armUnderlay, Texture2D sumoOverlay, Texture2D sumoUnderlay, SpriteFont sumoFont, Vector2 position, float rotation)
	{
		_startPosition = position;
		_player1 = player1;
		_player2 = player2;
		_originalSprite = sprite;
		_origin = new Vector2(armOverlay.Width, (float)armOverlay.Height / 2f);
		_originalOrigin = new Vector2((float)sprite.Width / 2f, (float)sprite.Height / 2f);
		_armOverlay = armOverlay;
		_armUnderlay = armUnderlay;
		_sumoOverlay = sumoOverlay;
		_sumoUnderlay = sumoUnderlay;
		_position = position;
		_velocity = Vector2.Zero;
		_font = sumoFont;
		_center = new Vector2(640f, 360f);
		_active = true;
		_random = new Random();
	}

	public void Update()
	{
		float num = 0f;
		float num2 = 0f;
		if (_active)
		{
			num = _player1.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y;
			num2 = _player1.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y;
			num -= _player2.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y;
			num2 -= _player2.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y;
			_strugglePower = MathHelper.Clamp(_player1.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y, 0f, 1f);
			_strugglePower = MathHelper.Clamp(_player1.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y, 0f, 1f);
			_strugglePower = MathHelper.Clamp(_player2.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y, 0f, 1f);
			_strugglePower = MathHelper.Clamp(_player2.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y, 0f, 1f);
			_pushPower = (num + num2) * 4f;
		}
		_leftPower = num;
		_rightPower = num2;
		_rotation += num / 50f - num2 / 50f;
		_velocity += new Vector2((num2 + num) * (float)Math.Cos(_rotation), (num2 + num) * (float)Math.Sin(_rotation)) / 50f;
		_position += _velocity;
		_velocity *= 0.995f;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		float num = ((float)_random.NextDouble() - 0.5f) * _strugglePower * 4f;
		Vector2 vector = new Vector2((float)Math.Cos(_rotation) * (_pushPower + num), (float)Math.Sin(_rotation) * (_pushPower + num));
		spriteBatch.Draw(_armUnderlay, _position + vector, null, Color.White, _rotation, _origin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_armOverlay, _position + vector, null, _player1.Colour(), _rotation, _origin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_sumoUnderlay, _position, null, Color.White, _rotation, _origin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_sumoOverlay, _position, null, _player1.Colour(), _rotation, _origin, 1f, SpriteEffects.None, 0f);
		vector = new Vector2((float)Math.Cos(_rotation) * (_pushPower + num), (float)Math.Sin(_rotation) * (_pushPower + num));
		spriteBatch.Draw(_armUnderlay, _position + vector, null, Color.White, _rotation + (float)Math.PI, _origin, new Vector2(1f, 1f), SpriteEffects.FlipVertically, 0f);
		spriteBatch.Draw(_armOverlay, _position + vector, null, _player2.Colour(), _rotation + (float)Math.PI, _origin, new Vector2(1f, 1f), SpriteEffects.FlipVertically, 0f);
		spriteBatch.Draw(_sumoUnderlay, _position, null, Color.White, _rotation + (float)Math.PI, _origin, new Vector2(1f, 1f), SpriteEffects.FlipVertically, 0f);
		spriteBatch.Draw(_sumoOverlay, _position, null, _player2.Colour(), _rotation + (float)Math.PI, _origin, new Vector2(1f, 1f), SpriteEffects.FlipVertically, 0f);
		Vector2 vector2 = _position - _center;
		float angle = (float)Math.Atan2(vector2.Y, vector2.X);
		angle = MathHelper.WrapAngle(angle);
		float angle2 = _rotation - angle;
		angle2 = MathHelper.WrapAngle(angle2);
		if (angle2 < (float)Math.PI / 2f)
		{
		}
		(_center - _position).Length();
	}

	public int Reset()
	{
		Vector2 vector = _position - _center;
		float angle = (float)Math.Atan2(vector.Y, vector.X);
		angle = MathHelper.WrapAngle(angle);
		float angle2 = _rotation - angle;
		angle2 = MathHelper.WrapAngle(angle2);
		_position = _startPosition;
		_rotation = 0f;
		_velocity = Vector2.Zero;
		_active = true;
		if (angle2 < (float)Math.PI / 2f && angle2 > -(float)Math.PI / 2f)
		{
			return 0;
		}
		return 1;
	}
}
