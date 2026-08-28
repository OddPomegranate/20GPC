using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.TheSkyIsFalling;

internal class Cloud
{
	private Vector2 _position;

	private Vector2 _velocity;

	private Vector2 _scale;

	private Texture2D _sprite;

	private float _rotation;

	private bool _inverted;

	public Cloud(Vector2 position, Vector2 velocity, Vector2 scale, Texture2D sprite, bool inverted, float rotation)
	{
		_position = position;
		_velocity = velocity;
		_scale = scale;
		_sprite = sprite;
		_inverted = inverted;
		_rotation = MathHelper.ToRadians(rotation);
	}

	public void Update()
	{
		_position += _velocity;
		if (_position.X > 1280f)
		{
			_position.X -= 1600f;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_sprite, _position, null, Color.White, _rotation, Vector2.Zero, _scale, (!_inverted) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
	}
}
