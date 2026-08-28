using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames;

public class GamePadManager
{
	protected class Vibration
	{
		private int _uniqueID;

		private int _runningTime;

		private bool _active;

		private int _duration;

		private float _leftMotorStartStrength;

		private float _rightMotorStartStrength;

		private float _leftMotorEndStrength;

		private float _rightMotorEndStrength;

		private int _pulseTime;

		private int _restTime;

		public int UniqueID => _uniqueID;

		public int RunningTime => _runningTime;

		public bool IsActive => _active;

		public int Duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
			}
		}

		public float LeftMotorStartStrength
		{
			get
			{
				return _leftMotorStartStrength;
			}
			set
			{
				_leftMotorStartStrength = value;
			}
		}

		public float RightMotorStartStrength
		{
			get
			{
				return _rightMotorStartStrength;
			}
			set
			{
				_rightMotorStartStrength = value;
			}
		}

		public float LeftMotorEndStrength
		{
			get
			{
				return _leftMotorEndStrength;
			}
			set
			{
				_leftMotorEndStrength = value;
			}
		}

		public float RightMotorEndStrength
		{
			get
			{
				return _rightMotorEndStrength;
			}
			set
			{
				_rightMotorEndStrength = value;
			}
		}

		public int PulseTime
		{
			get
			{
				return _pulseTime;
			}
			set
			{
				_pulseTime = value;
			}
		}

		public int RestTime
		{
			get
			{
				return _restTime;
			}
			set
			{
				_restTime = value;
			}
		}

		public Vibration(int uniqueID, int duration, float leftMotorStartStrength, float rightMotorStartStrength, float leftMotorEndStrength, float rightMotorEndStrength, int pulseTime, int restTime)
		{
			_uniqueID = uniqueID;
			_runningTime = 0;
			_active = true;
			_duration = duration;
			_leftMotorStartStrength = leftMotorStartStrength;
			_rightMotorStartStrength = rightMotorStartStrength;
			_leftMotorEndStrength = leftMotorEndStrength;
			_rightMotorEndStrength = rightMotorEndStrength;
			_pulseTime = pulseTime;
			_restTime = restTime;
		}

		public float[] Update(GameTime gameTime)
		{
			float[] array = new float[2];
			float[] array2 = array;
			if (_active)
			{
				int num = _pulseTime + _restTime;
				if (_runningTime % num <= _pulseTime)
				{
					if (_duration != 0)
					{
						float num2 = (float)_runningTime / (float)_duration;
						array2[0] = _leftMotorStartStrength + (_leftMotorEndStrength - _leftMotorStartStrength) * num2;
						array2[1] = _rightMotorStartStrength + (_rightMotorEndStrength - _rightMotorStartStrength) * num2;
					}
					else
					{
						array2[0] = _leftMotorStartStrength;
						array2[1] = _rightMotorStartStrength;
					}
				}
				_runningTime += gameTime.ElapsedGameTime.Milliseconds;
				if (_duration != 0 && _runningTime > _duration)
				{
					_active = false;
				}
			}
			return array2;
		}
	}

	private List<Vibration> _vibrations;

	private int _vibrationMotorCount;

	private PlayerIndex _playerIndex;

	private GamePadState _currentGamePadState;

	private GamePadState _previousGamePadState;

	private Player _associatedPlayer;

	private bool _hide;

	public PlayerIndex PlayerIndex
	{
		get
		{
			return _playerIndex;
		}
		set
		{
			if (_associatedPlayer == null || _associatedPlayer.PlayerIndex == value)
			{
				_playerIndex = value;
			}
		}
	}

	public Player Player
	{
		get
		{
			return _associatedPlayer;
		}
		set
		{
			_associatedPlayer = value;
			if (_associatedPlayer != null)
			{
				_playerIndex = _associatedPlayer.PlayerIndex;
			}
		}
	}

	public GamePadState GamePadStateCurrent => _currentGamePadState;

	public GamePadState GamePadStatePrevious => _previousGamePadState;

	public bool HideInput
	{
		get
		{
			return _hide;
		}
		set
		{
			_hide = value;
		}
	}

	public GamePadManager(PlayerIndex playerIndex)
	{
		_associatedPlayer = null;
		_playerIndex = playerIndex;
		_hide = false;
		Initialise();
	}

	public virtual void Initialise()
	{
		_currentGamePadState = GamePad.GetState(_playerIndex);
		_previousGamePadState = GamePad.GetState(_playerIndex);
		_vibrations = new List<Vibration>();
		if (GamePad.GetCapabilities(_playerIndex).HasLeftVibrationMotor)
		{
			_vibrationMotorCount++;
		}
		if (GamePad.GetCapabilities(_playerIndex).HasRightVibrationMotor)
		{
			_vibrationMotorCount++;
		}
	}

	public virtual void Update(GameTime gameTime)
	{
		if (!_previousGamePadState.IsConnected && _currentGamePadState.IsConnected)
		{
			Initialise();
		}
		_previousGamePadState = _currentGamePadState;
		_currentGamePadState = GamePad.GetState(_playerIndex);
		if (_vibrationMotorCount == 0)
		{
			return;
		}
		float[] array = new float[2];
		float[] array2 = array;
		for (int i = 0; i < _vibrations.Count; i++)
		{
			float[] array3 = _vibrations[i].Update(gameTime);
			array2[0] = ((array3[0] > array2[0]) ? array3[0] : array2[0]);
			array2[1] = ((array3[1] > array2[1]) ? array3[1] : array2[1]);
		}
		for (int num = _vibrations.Count - 1; num >= 0; num--)
		{
			if (!_vibrations[num].IsActive)
			{
				_vibrations.RemoveAt(num);
			}
		}
		if (_vibrationMotorCount == 1)
		{
			array2[0] = (array2[1] = (array2[0] + array2[1]) / 2f);
		}
		GamePad.SetVibration(_playerIndex, array2[0], array2[1]);
	}

	public bool ButtonWasPressed(Buttons button)
	{
		if (_currentGamePadState.IsButtonDown(button) && _previousGamePadState.IsButtonUp(button))
		{
			return !_hide;
		}
		return false;
	}

	public bool ButtonWasReleased(Buttons button)
	{
		if (_currentGamePadState.IsButtonUp(button) && _previousGamePadState.IsButtonDown(button))
		{
			return !_hide;
		}
		return false;
	}

	public bool ButtonIsHeld(Buttons button)
	{
		if (_currentGamePadState.IsButtonDown(button))
		{
			return !_hide;
		}
		return false;
	}

	public int StartVibration()
	{
		return StartVibration(0, 1f, 1f, 1f, 1f, 1, 0);
	}

	public int StartVibration(int duration)
	{
		return StartVibration(duration, 1f, 1f, 1f, 1f, 1, 0);
	}

	public int StartVibration(int duration, float motorStrength)
	{
		return StartVibration(duration, motorStrength, motorStrength, motorStrength, motorStrength, 1, 0);
	}

	public int StartVibration(int duration, float leftMotorStrength, float rightMotorStrength)
	{
		return StartVibration(duration, leftMotorStrength, leftMotorStrength, rightMotorStrength, rightMotorStrength, 1, 0);
	}

	public int StartVibration(int duration, float leftMotorStrength, float rightMotorStrength, int pulseTime, int timeBetweenPulses)
	{
		return StartVibration(duration, leftMotorStrength, leftMotorStrength, rightMotorStrength, rightMotorStrength, pulseTime, timeBetweenPulses);
	}

	public int StartVibration(int duration, float leftMotorStartStrength, float rightMotorStartStrength, float leftMotorEndStrength, float rightMotorEndStrength)
	{
		return StartVibration(duration, leftMotorStartStrength, rightMotorStartStrength, leftMotorEndStrength, rightMotorEndStrength, 1, 0);
	}

	public int StartVibration(int duration, float leftMotorStartStrength, float rightMotorStartStrength, float leftMotorEndStrength, float rightMotorEndStrength, int pulseTime, int timeBetweenPulses)
	{
		int num = 0;
		if (_associatedPlayer.AllowsVibration && _vibrationMotorCount != 0)
		{
			for (int i = 0; i != _vibrations.Count; i++)
			{
				if (_vibrations[i].UniqueID == num)
				{
					num++;
				}
			}
			_vibrations.Add(new Vibration(num, duration, leftMotorStartStrength, rightMotorStartStrength, leftMotorEndStrength, rightMotorEndStrength, pulseTime, timeBetweenPulses));
		}
		else
		{
			num = -1;
		}
		return num;
	}

	public bool EndVibration(int vibrationID)
	{
		bool result = false;
		for (int i = 0; i < _vibrations.Count; i++)
		{
			if (_vibrations[i].UniqueID == vibrationID)
			{
				_vibrations.RemoveAt(i);
				result = true;
			}
		}
		return result;
	}
}
