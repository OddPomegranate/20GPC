using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames;

internal static class GameConsole
{
	public delegate void ConsoleCommandEventHandler(GameConsoleCommand command);

	private static List<string> _log;

	private static List<int> _logLife;

	private static string _commandLine;

	private static List<string> _commandHistory;

	private static bool _usingHistory;

	private static int _currentHistoryItem;

	private static int _logLineShowTime;

	private static int _logLineBufferLength;

	private static int _commandHistoryLength;

	private static bool _visible;

	private static bool _open;

	private static Keys _consoleKey;

	private static Rectangle _drawArea;

	private static KeyboardState _currentKeyboardState;

	private static KeyboardState _previousKeyboardState;

	private static GamePadState _currentGamePadState;

	private static GamePadState _previousGamePadState;

	private static SpriteFont _font;

	private static Texture2D _background;

	private static Color _textColour;

	private static IAsyncResult _enterCommandAsyncResult;

	public static bool IsOpen => _open;

	public static bool IsVisible => _visible;

	public static Color TextColour
	{
		get
		{
			return _textColour;
		}
		set
		{
			_textColour = value;
		}
	}

	public static SpriteFont Font
	{
		get
		{
			return _font;
		}
		set
		{
			_font = value;
		}
	}

	public static Texture2D BackgroundTexture
	{
		get
		{
			return _background;
		}
		set
		{
			_background = value;
		}
	}

	public static int LineShowTime
	{
		get
		{
			return _logLineShowTime;
		}
		set
		{
			_logLineShowTime = value;
		}
	}

	public static int LineBufferLength
	{
		get
		{
			return _logLineBufferLength;
		}
		set
		{
			_logLineBufferLength = value;
		}
	}

	public static Keys ConsoleKey
	{
		get
		{
			return _consoleKey;
		}
		set
		{
			_consoleKey = value;
		}
	}

	public static Rectangle DrawArea
	{
		get
		{
			return _drawArea;
		}
		set
		{
			_drawArea = value;
		}
	}

	public static event ConsoleCommandEventHandler CommandInvoked;

	static GameConsole()
	{
	}

	public static void Initialize()
	{
		_logLineShowTime = 5000;
		_logLineBufferLength = 200;
		_commandHistoryLength = 10;
		_usingHistory = false;
		_currentHistoryItem = 0;
		_commandLine = "";
		_commandHistory = new List<string>(_commandHistoryLength);
		_log = new List<string>(_logLineBufferLength);
		_logLife = new List<int>(_logLineBufferLength);
		_visible = false;
		_open = false;
		_consoleKey = Keys.Escape;
		CommandInvoked += ConsoleCommandInvoked;
	}

	public static void Update(GameTime gameTime)
	{
		_previousKeyboardState = _currentKeyboardState;
		_currentKeyboardState = Keyboard.GetState();
		_previousGamePadState = _currentGamePadState;
		_currentGamePadState = GamePad.GetState(PlayerIndex.One);
		if ((_currentKeyboardState != _previousKeyboardState && _currentKeyboardState.GetPressedKeys().Length > 0) || (_enterCommandAsyncResult != null && _enterCommandAsyncResult.IsCompleted))
		{
			Keys[] pressedKeys = _currentKeyboardState.GetPressedKeys();
			Keys[] pressedKeys2 = _previousKeyboardState.GetPressedKeys();
			Keys keys = Keys.None;
			for (int i = 0; i < pressedKeys.Length; i++)
			{
				if (!pressedKeys2.Contains(pressedKeys[i]))
				{
					keys = pressedKeys[i];
					break;
				}
			}
			if (_open)
			{
				if (_enterCommandAsyncResult != null && _enterCommandAsyncResult.IsCompleted)
				{
					_commandLine = Guide.EndShowKeyboardInput(_enterCommandAsyncResult);
					if (_commandLine == null)
					{
						_commandLine = "";
					}
					_enterCommandAsyncResult = null;
					keys = Keys.Enter;
				}
				if (keys == Keys.Enter && _commandLine != "")
				{
					_commandHistory.Add(_commandLine);
					if (_commandHistory.Count > _commandHistoryLength)
					{
						_commandHistory.RemoveRange(0, _commandHistory.Count - _commandHistoryLength);
					}
					_usingHistory = false;
					_currentHistoryItem = 0;
					List<string> list = _commandLine.Split(' ').ToList();
					List<int> list2 = new List<int>();
					int num = -1;
					int num2 = -1;
					for (int j = 0; j != list.Count; j++)
					{
						if (num == -1 && list[j][0] == '"')
						{
							num = j;
						}
						if (num != -1 && list[j][list[j].Length - 1] == '"')
						{
							num2 = j;
						}
						if (num == -1 || num2 == -1)
						{
							continue;
						}
						for (int k = num; k < num2; k++)
						{
							if (k == num)
							{
								list[num] = list[num].Substring(1);
							}
							if (k == num2)
							{
								list[num2] = list[num2].Substring(list[num2].Length - 2);
							}
							if (k != num)
							{
								List<string> list3;
								int index;
								(list3 = list)[index = num] = list3[index] + " " + list[k];
								list2.Add(k);
							}
						}
						num = -1;
						num2 = -1;
					}
					for (int l = list2.Count - 1; l >= 0; l++)
					{
						list.RemoveAt(list2[l]);
					}
					GameConsoleCommand gameConsoleCommand = new GameConsoleCommand(list[0]);
					list.RemoveAt(0);
					gameConsoleCommand.setAllArguments(list.ToArray());
					CommandInvoked(gameConsoleCommand);
					_commandLine = "";
				}
				else
				{
					switch (keys)
					{
					case Keys.Back:
						if (_commandLine.Length == 0)
						{
							_commandLine = "";
						}
						else
						{
							_commandLine = _commandLine.Remove(_commandLine.Length - 1);
						}
						break;
					case Keys.Space:
						_commandLine += ' ';
						break;
					case Keys.Up:
						if (_commandHistory.Count != 0)
						{
							_usingHistory = true;
							_currentHistoryItem--;
							if (_currentHistoryItem < 0)
							{
								_currentHistoryItem = _commandHistory.Count - 1;
							}
							_commandLine = _commandHistory[_currentHistoryItem];
						}
						break;
					case Keys.Down:
						if (_commandHistory.Count != 0)
						{
							if (!_usingHistory)
							{
								_usingHistory = true;
							}
							else
							{
								_currentHistoryItem++;
							}
							if (_currentHistoryItem > _commandHistory.Count - 1)
							{
								_currentHistoryItem = 0;
							}
							_commandLine = _commandHistory[_currentHistoryItem];
						}
						break;
					case Keys.OemPeriod:
						_commandLine += '.';
						break;
					case Keys.D0:
					case Keys.D1:
					case Keys.D2:
					case Keys.D3:
					case Keys.D4:
					case Keys.D5:
					case Keys.D6:
					case Keys.D7:
					case Keys.D8:
					case Keys.D9:
						if (_currentKeyboardState.IsKeyDown(Keys.RightShift))
						{
							switch (keys)
							{
							case Keys.D0:
								_commandLine += ')';
								break;
							case Keys.D1:
								_commandLine += '!';
								break;
							case Keys.D2:
								_commandLine += '"';
								break;
							case Keys.D4:
								_commandLine += '$';
								break;
							case Keys.D5:
								_commandLine += '%';
								break;
							case Keys.D6:
								_commandLine += '^';
								break;
							case Keys.D7:
								_commandLine += '&';
								break;
							case Keys.D8:
								_commandLine += '*';
								break;
							case Keys.D9:
								_commandLine += '(';
								break;
							}
						}
						else
						{
							_commandLine += (int)(keys - 48);
						}
						break;
					default:
						if (keys >= (Keys)58 && keys <= Keys.Z)
						{
							if (_currentKeyboardState.IsKeyDown(Keys.RightShift))
							{
								_commandLine += keys;
							}
							else
							{
								_commandLine += keys.ToString().ToLower();
							}
						}
						break;
					}
				}
			}
			if (keys == _consoleKey)
			{
				_open = !_open;
			}
		}
		if (_currentGamePadState.IsButtonDown(Buttons.LeftShoulder) && _currentGamePadState.IsButtonDown(Buttons.RightShoulder) && _currentGamePadState.IsButtonDown(Buttons.RightStick) && _currentGamePadState.IsButtonDown(Buttons.Back) && _currentGamePadState.Buttons != _previousGamePadState.Buttons)
		{
			_visible = !_visible;
			_open = _visible;
		}
		if (_visible && _currentGamePadState.IsButtonDown(Buttons.Back) && _previousGamePadState.IsButtonUp(Buttons.Back))
		{
			_open = !_open;
		}
		if (_open && !Guide.IsVisible && _currentGamePadState.IsButtonDown(Buttons.A) && _previousGamePadState.IsButtonUp(Buttons.A))
		{
			_enterCommandAsyncResult = Guide.BeginShowKeyboardInput(PlayerIndex.One, "Console Command", "Enter the console command you wish to execute", _commandLine, null, null);
		}
		if (_open && _currentGamePadState.IsButtonDown(Buttons.DPadUp) && _previousGamePadState.IsButtonUp(Buttons.DPadUp) && _commandHistory.Count != 0)
		{
			_usingHistory = true;
			_currentHistoryItem--;
			if (_currentHistoryItem < 0)
			{
				_currentHistoryItem = _commandHistory.Count - 1;
			}
			_commandLine = _commandHistory[_currentHistoryItem];
		}
		if (_open && _currentGamePadState.IsButtonDown(Buttons.DPadDown) && _previousGamePadState.IsButtonUp(Buttons.DPadDown) && _commandHistory.Count != 0)
		{
			if (!_usingHistory)
			{
				_usingHistory = true;
			}
			else
			{
				_currentHistoryItem++;
			}
			if (_currentHistoryItem > _commandHistory.Count - 1)
			{
				_currentHistoryItem = 0;
			}
			_commandLine = _commandHistory[_currentHistoryItem];
		}
		for (int m = 0; m < _logLife.Count; m++)
		{
			if (_logLife[m] != 0)
			{
				_logLife[m] -= gameTime.ElapsedGameTime.Milliseconds;
				if (_logLife[m] < 0)
				{
					_logLife[m] = 0;
				}
			}
		}
	}

	public static void Draw(SpriteBatch spriteBatch)
	{
		if (!_visible || _font == null)
		{
			return;
		}
		if (_open)
		{
			if (_background != null)
			{
				spriteBatch.Draw(_background, _drawArea, Color.White * 0.5f);
			}
			float num = _font.MeasureString("lj").Y;
			float num2 = num * (float)(_log.Count + 2);
			spriteBatch.DrawString(_font, _commandLine, new Vector2(_drawArea.Left, (float)_drawArea.Bottom - num), _textColour);
			for (int i = 0; i < _log.Count; i++)
			{
				if ((float)_drawArea.Bottom - num2 + num > (float)_drawArea.Top)
				{
					spriteBatch.DrawString(_font, _log[i], new Vector2(_drawArea.Left, (float)_drawArea.Bottom - num2 + num), _textColour);
				}
				num += _font.MeasureString(_log[i]).Y;
			}
		}
		else
		{
			if (!_visible)
			{
				return;
			}
			float num3 = 0f;
			float num4 = 0f;
			for (int j = 0; j < _log.Count; j++)
			{
				if (_logLife[j] != 0)
				{
					num4 += _font.MeasureString("lj").Y;
				}
			}
			for (int k = 0; k < _log.Count; k++)
			{
				if (_logLife[k] != 0)
				{
					float num5 = _drawArea.Top;
					if (num4 > (float)_drawArea.Height)
					{
						num5 -= num4 - (float)_drawArea.Height;
					}
					if (num5 + num3 >= (float)_drawArea.Top)
					{
						spriteBatch.DrawString(_font, _log[k], new Vector2(_drawArea.Left, num5 + num3), _textColour);
					}
					num3 += _font.MeasureString(_log[k]).Y;
				}
			}
		}
	}

	public static void ConsoleCommandInvoked(GameConsoleCommand command)
	{
		if (command.IsSet)
		{
			switch (command.Command)
			{
			case "print":
			{
				if (command.Arguments.Count == 0)
				{
					command.setArgument("");
				}
				string text = command.Arguments[0];
				for (int i = 1; i < command.Arguments.Count; i++)
				{
					text = text + " " + command.Arguments[i];
				}
				_log.Add(text);
				_logLife.Add(_logLineShowTime);
				break;
			}
			case "show":
				_visible = true;
				break;
			case "hide":
				_visible = false;
				break;
			case "open":
				_open = true;
				break;
			case "close":
				_open = false;
				break;
			case "clear":
				_log.Clear();
				_logLife.Clear();
				break;
			}
		}
		else
		{
			_log.Add("Error: Null command invoked.");
			_logLife.Add(_logLineShowTime);
		}
	}

	public static void PrintString(string printString)
	{
		GameConsoleCommand gameConsoleCommand = new GameConsoleCommand("print");
		gameConsoleCommand.setArgument(printString);
		ConsoleCommandInvoked(gameConsoleCommand);
	}

	public static void Show()
	{
		GameConsoleCommand command = new GameConsoleCommand("show");
		ConsoleCommandInvoked(command);
	}

	public static void Hide()
	{
		GameConsoleCommand command = new GameConsoleCommand("hide");
		ConsoleCommandInvoked(command);
	}

	public static void Open()
	{
		GameConsoleCommand command = new GameConsoleCommand("open");
		ConsoleCommandInvoked(command);
	}

	public static void Close()
	{
		GameConsoleCommand command = new GameConsoleCommand("close");
		ConsoleCommandInvoked(command);
	}
}
