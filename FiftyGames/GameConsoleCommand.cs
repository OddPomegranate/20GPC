using System.Collections.Generic;

namespace FiftyGames;

public class GameConsoleCommand
{
	private string _command;

	private List<string> _arguments;

	public bool IsSet => _command != string.Empty;

	public string Command
	{
		get
		{
			return _command;
		}
		set
		{
			_command = value;
		}
	}

	public List<string> Arguments
	{
		get
		{
			List<string> list = new List<string>(_arguments.Count);
			for (int i = 0; i < _arguments.Count; i++)
			{
				list.Add(_arguments[i]);
			}
			return list;
		}
	}

	public GameConsoleCommand(string command, string[] arguments)
	{
		_command = command.ToLower();
		if (arguments != null)
		{
			_arguments = new List<string>(arguments.Length);
			for (int i = 0; i < arguments.Length; i++)
			{
				_arguments.Add(arguments[i]);
			}
		}
		else
		{
			_arguments = new List<string>();
		}
	}

	public GameConsoleCommand(string command, List<string> arguments)
		: this(command, arguments.ToArray())
	{
	}

	public GameConsoleCommand(string command)
		: this(command, (string[])null)
	{
	}

	public int setArgument(string argument)
	{
		_arguments.Add(argument);
		return _arguments.Count - 1;
	}

	public bool setArgument(string argument, int argumentIndex)
	{
		bool flag = argumentIndex < _arguments.Count;
		if (flag)
		{
			_arguments[argumentIndex] = argument;
		}
		return flag;
	}

	public void setAllArguments(string[] arguments)
	{
		_arguments = new List<string>(arguments.Length);
		for (int i = 0; i < arguments.Length; i++)
		{
			_arguments.Add(arguments[i]);
		}
	}

	public void setAllArguments(List<string> arguments)
	{
		setAllArguments(arguments.ToArray());
	}

	public bool clearArgument(int argumentIndex)
	{
		bool flag = argumentIndex < _arguments.Count;
		if (flag)
		{
			_arguments.RemoveAt(argumentIndex);
		}
		return flag;
	}

	public void clearAllArguments()
	{
		_arguments.Clear();
	}
}
