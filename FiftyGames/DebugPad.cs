using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames;

internal class DebugPad
{
	public KeyboardState _keyboardStateCurrnet;

	public KeyboardState _keyboardStatePrevious;

	public MouseState _mouseStateCurrnet;

	public MouseState _mouseStatePrevious;

	public DebugPad()
	{
		_keyboardStatePrevious = (_keyboardStateCurrnet = Keyboard.GetState());
		_mouseStatePrevious = (_mouseStateCurrnet = Mouse.GetState());
	}

	public void Update(GameTime gameTime)
	{
		_keyboardStatePrevious = _keyboardStateCurrnet;
		_mouseStatePrevious = _mouseStateCurrnet;
		_keyboardStateCurrnet = Keyboard.GetState();
		_mouseStateCurrnet = Mouse.GetState();
	}
}
