using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Guns;

public struct Shot
{
	public Vector2 startPosition;

	public Vector2 bulletVector;

	public Vector2 direction;

	public int magnitude;

	public Color startColor;

	public Color endColor;
}
