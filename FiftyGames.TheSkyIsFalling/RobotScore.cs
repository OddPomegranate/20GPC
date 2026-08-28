using Microsoft.Xna.Framework;

namespace FiftyGames.TheSkyIsFalling;

public struct RobotScore(Color colour, float score, bool alive)
{
	public Color _color = colour;

	public float _score = score;

	public bool _alive = alive;
}
