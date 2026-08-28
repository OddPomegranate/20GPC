using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Zombie;

internal static class InputState
{
	private static MouseState _currentMouseState;

	private static MouseState _previousMouseState;

	private static KeyboardState _currentKeyboardState;

	private static KeyboardState _previousKeyboardState;

	public static void SetCurrentStates()
	{
		_currentMouseState = Mouse.GetState();
		_currentKeyboardState = Keyboard.GetState();
	}

	public static void SetPreviousStates()
	{
		_previousMouseState = _currentMouseState;
		_previousKeyboardState = _currentKeyboardState;
	}

	public static bool MouseStateChanged()
	{
		if (_currentMouseState != _previousMouseState)
		{
			return true;
		}
		return false;
	}

	public static bool LeftButtonClicked()
	{
		if (_currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
		{
			return true;
		}
		return false;
	}

	public static bool MiddleButtonClicked()
	{
		if (_currentMouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton == ButtonState.Released)
		{
			return true;
		}
		return false;
	}

	public static bool RightButtonClicked()
	{
		if (_currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
		{
			return true;
		}
		return false;
	}

	public static bool LeftButtonReleased()
	{
		if (_currentMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Released)
		{
			return true;
		}
		return false;
	}

	public static bool MiddleButtonReleased()
	{
		if (_currentMouseState.MiddleButton == ButtonState.Released && _previousMouseState.MiddleButton == ButtonState.Released)
		{
			return true;
		}
		return false;
	}

	public static bool RightButtonReleased()
	{
		if (_currentMouseState.RightButton == ButtonState.Released && _previousMouseState.RightButton == ButtonState.Released)
		{
			return true;
		}
		return false;
	}

	public static bool LeftButtonHeld()
	{
		if (_currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Pressed)
		{
			return true;
		}
		return false;
	}

	public static bool MiddleButtonHeld()
	{
		if (_currentMouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton == ButtonState.Pressed)
		{
			return true;
		}
		return false;
	}

	public static bool RightButtonHeld()
	{
		if (_currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Pressed)
		{
			return true;
		}
		return false;
	}

	public static Vector2 GetMouseCoords()
	{
		return new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
	}

	public static bool KeyboardStateChanged()
	{
		if (_currentKeyboardState != _previousKeyboardState)
		{
			return true;
		}
		return false;
	}

	public static MouseState GetCurrentMouseState()
	{
		return _currentMouseState;
	}

	public static MouseState GetPreviousMouseState()
	{
		return _previousMouseState;
	}

	public static KeyboardState GetCurrentKeyboardState()
	{
		return _currentKeyboardState;
	}

	public static KeyboardState GetPreviousKetboardState()
	{
		return _previousKeyboardState;
	}

	public static bool MouseWheelIncremented()
	{
		if (MouseStateChanged())
		{
			if (GetCurrentMouseState().ScrollWheelValue > GetPreviousMouseState().ScrollWheelValue)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool MouseWheelDecremented()
	{
		if (MouseStateChanged())
		{
			if (GetCurrentMouseState().ScrollWheelValue < GetPreviousMouseState().ScrollWheelValue)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
