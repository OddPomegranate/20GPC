using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.DynamicLights;

internal class SimpleDynamicLight : DynamicLightMask
{
	private Texture2D _texture;

	private Color _color;

	private float _scale;

	private float _alpha;

	public SimpleDynamicLight(ContentManager contentManager, Vector2 position, Color color, float scale, float alpha)
		: base(contentManager, position)
	{
		_texture = contentManager.Load<Texture2D>("Zombie/explosionLightMask");
		_scale = scale;
		_alpha = alpha;
		_color = color;
	}

	public override void Update(GameTime gameTime)
	{
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_texture, _position + offset, null, _color * _alpha, 0f, new Vector2(256f, 256f), _scale, SpriteEffects.None, 0f);
		spriteBatch.End();
	}

	public void SetPosition(Vector2 position)
	{
		_position = position;
	}
}
