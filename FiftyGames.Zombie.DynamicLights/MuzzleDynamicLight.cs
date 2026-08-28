using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.DynamicLights;

internal class MuzzleDynamicLight : DynamicLightMask
{
	private Texture2D _texture;

	private float _scale;

	private float _alpha;

	public MuzzleDynamicLight(ContentManager contentManager, Vector2 position)
		: base(contentManager, position)
	{
		_texture = contentManager.Load<Texture2D>("Zombie/explosion");
		_scale = 0.75f;
		_alpha = 0.5f;
	}

	public override void Update(GameTime gameTime)
	{
		_scale -= 0.1f;
		if (_scale < 0f)
		{
			_readyForRemoval = true;
		}
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_texture, _position + offset, null, Color.White * _alpha, 0f, new Vector2(256f, 256f), _scale, SpriteEffects.None, 0f);
		spriteBatch.End();
	}
}
