using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.DynamicLights;

internal class RotatingLight : DynamicLightMask
{
	private Texture2D _texture;

	private float _scale;

	private float _alpha;

	private float _rotation;

	public RotatingLight(ContentManager contentManager, Vector2 position)
		: base(contentManager, position)
	{
		_texture = contentManager.Load<Texture2D>("Zombie/circlelight");
		_scale = 1f;
		_alpha = 1f;
	}

	public override void Update(GameTime gameTime)
	{
		_rotation += 0.05f;
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_texture, _position + offset, null, Color.Green * _alpha, _rotation, new Vector2(256f, 256f), _scale, SpriteEffects.None, 0f);
		spriteBatch.End();
	}
}
