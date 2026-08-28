using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.SuperHighway;

internal class PlayerCar : Car
{
	private const float Traction = 0.08f;

	private const float MoveSpeed = 0.001f;

	private const float IdleFallback = 0.0004f;

	private Player _player;

	private int _distnace;

	private int _accelerationVibrationID;

	public Player Player => _player;

	public int Score
	{
		get
		{
			return _distnace;
		}
		set
		{
			_distnace = value;
		}
	}

	public PlayerCar(Player player, Vector2 position)
		: base(position)
	{
		_player = player;
		_colour = _player.Colour();
		_alive = false;
		_accelerationVibrationID = 0;
	}

	public override void Update(GameTime gameTime)
	{
		if (_alive)
		{
			_velocity.X -= _velocity.X * 0.08f;
			_velocity.Y -= _velocity.Y * 0.08f;
			if (Math.Abs(_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X) > 0.3f)
			{
				_velocity.X += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X * 0.001f;
			}
			if (_player.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed)
			{
				_velocity.Y -= 0.001f;
			}
			else if (_player.GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Pressed)
			{
				_velocity.Y += 0.001f;
			}
			else
			{
				_velocity.Y += 0.0004f;
			}
			if (_player.GamePadManager.ButtonWasPressed(Buttons.A))
			{
				_accelerationVibrationID = _player.GamePadManager.StartVibration(1000, 0.2f, 0.2f, 0f, 0f);
			}
			if (_player.GamePadManager.ButtonWasReleased(Buttons.A))
			{
				_player.GamePadManager.EndVibration(_accelerationVibrationID);
			}
			base.Update(gameTime);
			if ((double)_position.Y < 0.15000000074505807)
			{
				_position.Y = 0.15f;
				_velocity.Y = 0f;
			}
			if ((double)_position.Y > 0.44999999925494194)
			{
				_position.Y = 0.45f;
				_velocity.Y = 0f;
			}
		}
	}

	public void Spawn()
	{
		_alive = true;
	}
}
