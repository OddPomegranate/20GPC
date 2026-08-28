using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RiskyRiskyRisk;

internal class Dice
{
	public enum State
	{
		Flying,
		Attacking,
		Finishing,
		Waiting,
		Defending,
		Dying
	}

	private Texture2D _texture;

	private Vector2 _position;

	private Vector2 _target;

	private Vector2 _velocity;

	private Vector2 _origin;

	private Point _size;

	private Rectangle _sourceRect;

	private int _value;

	private float _scale;

	private float _scaleVelocity;

	private float _scaleTarget;

	private bool _isAlive;

	private int _timer;

	private int _rollTimer;

	private State _state;

	private Random _random;

	private bool _isRolling;

	private bool _isDefending;

	private Vector2[] _targets;

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

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public bool IsAlive
	{
		get
		{
			return _isAlive;
		}
		set
		{
			_isAlive = value;
		}
	}

	public State DState
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
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

	public Point Size => _size;

	public void LoadContent(ContentManager content, ref Random random, Rectangle titleSafeArea)
	{
		_texture = content.Load<Texture2D>("RiskyRiskyRisk/Sprites/dice2");
		_size = new Point(_texture.Width / 6, _texture.Height);
		_sourceRect = new Rectangle(0, 0, _size.X, _size.Y);
		_targets = new Vector2[2]
		{
			new Vector2(100f, titleSafeArea.Top + 50),
			new Vector2(1180f, titleSafeArea.Top + 50)
		};
		_random = random;
	}

	public void Update(GameTime gameTime, int diceIndex, int currentDiceIndex, float currentDiceScale, bool isSynched, float speedMod)
	{
		if (!_isAlive)
		{
			return;
		}
		_timer += gameTime.ElapsedGameTime.Milliseconds;
		if (_isRolling)
		{
			_rollTimer += gameTime.ElapsedGameTime.Milliseconds;
			if (_rollTimer > 120)
			{
				_rollTimer -= 120;
				Roll();
			}
		}
		switch (_state)
		{
		case State.Flying:
			if (_scale < _scaleTarget - _scaleVelocity * 2f)
			{
				_position += _velocity;
				_scale += _scaleVelocity;
				break;
			}
			_position = _target;
			_scale = _scaleTarget;
			_isRolling = true;
			_state = State.Waiting;
			_velocity = new Vector2(30f, 0f);
			_scaleTarget = 1f;
			_scaleVelocity = 0.1f;
			_target = new Vector2(640f, _position.Y);
			if (!_isDefending)
			{
				_velocity *= -1f;
				_target.X += _size.X;
			}
			else
			{
				_target.X -= _size.X;
			}
			_timer = 0;
			break;
		case State.Attacking:
			if (Math.Abs(_target.X - _position.X) > Math.Abs(_velocity.X))
			{
				_position += _velocity;
				if (_scaleVelocity > 0f)
				{
					if (_scale < _scaleTarget - _scaleVelocity)
					{
						_scale += _scaleVelocity;
					}
					else
					{
						_scale = _scaleTarget;
					}
				}
				else if (_scale > _scaleTarget - _scaleVelocity)
				{
					_scale += _scaleVelocity;
				}
				else
				{
					_scale = _scaleTarget;
				}
			}
			else
			{
				_position = _target;
				_isRolling = false;
				_scaleTarget = 0.5f;
				_scaleVelocity = -0.1f;
				_state = State.Finishing;
				_timer = 0;
			}
			break;
		case State.Finishing:
			if (_scaleVelocity > 0f)
			{
				if (_scale < _scaleTarget - _scaleVelocity)
				{
					_scale += _scaleVelocity;
				}
				else
				{
					_scale = _scaleTarget;
				}
			}
			else if (_scale > _scaleTarget - _scaleVelocity)
			{
				_scale += _scaleVelocity;
			}
			else
			{
				_scale = _scaleTarget;
			}
			break;
		case State.Waiting:
			break;
		}
	}

	public void Draw(SpriteBatch spriteBatch, Color color)
	{
		if (_isAlive)
		{
			spriteBatch.Draw(_texture, _position, _sourceRect, color, 0f, _origin, _scale, SpriteEffects.None, 1f);
		}
	}

	public void Spawn(Vector2 position, bool isDefending, int diceIndex)
	{
		_position = position;
		_isDefending = isDefending;
		_isAlive = false;
		_state = State.Flying;
		_isRolling = true;
		_scaleTarget = 0.5f;
		if (_isDefending)
		{
			_target = _targets[0] + new Vector2(0f, (float)diceIndex * ((float)_size.Y * _scaleTarget));
			_origin = Vector2.Zero;
		}
		else
		{
			_target = _targets[1] + new Vector2(0f, (float)diceIndex * ((float)_size.Y * _scaleTarget));
			_origin = new Vector2(_size.X, 0f);
		}
		_velocity = (_target - _position) / 10f;
		_scaleVelocity = _scaleTarget / 10f;
		_scale = _scaleVelocity;
	}

	public void Roll()
	{
		_value = _random.Next(1, 7);
		_sourceRect.X = (_value - 1) * _size.X;
	}

	public void ResetTimer()
	{
		_timer = 0;
	}
}
