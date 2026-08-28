using Microsoft.Xna.Framework;

namespace FiftyGames.SuperHighway;

internal class ObstacleCar : Car
{
	private float _acceleration;

	public ObstacleCar(Vector2 position, Vector2 velocity, float acceleration)
		: base(position)
	{
		_velocity = velocity;
		_acceleration = acceleration;
		_colour = Color.White;
	}

	public override void Update(GameTime gameTime)
	{
		_velocity.Y += _acceleration;
		base.Update(gameTime);
	}
}
