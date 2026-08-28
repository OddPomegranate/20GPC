using FiftyGames.Zombie.Rendering_Helpers;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Utils;

internal class LineWalker
{
	private Lines _lines;

	private float _speed;

	private Vector2 _position;

	private int _lineNumber;

	public OnWalkerStep OnStep { get; set; }

	public OnEnded OnEnded { get; set; }

	public LineWalker(Lines lines, float speed)
	{
		_lines = lines;
		_speed = speed;
		_position = lines.LineList[0].Start;
		_lineNumber = 0;
	}

	public void Update()
	{
		if (Vector2.Distance(_position, _lines.LineList[_lineNumber].End) < 20f)
		{
			_lineNumber++;
			if (_lineNumber < _lines.LineList.Count)
			{
				_position = _lines.LineList[_lineNumber].Start;
			}
		}
		if (_lineNumber < _lines.LineList.Count)
		{
			Vector2 vector = _lines.LineList[_lineNumber].End - _position;
			vector.Normalize();
			_position += vector * _speed;
			if (OnStep != null)
			{
				OnStep(_position);
			}
		}
		else if (OnEnded != null)
		{
			OnEnded();
		}
	}
}
