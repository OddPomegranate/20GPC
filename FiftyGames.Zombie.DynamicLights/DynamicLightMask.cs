using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.DynamicLights;

internal abstract class DynamicLightMask
{
	protected Vector2 _position;

	protected bool _readyForRemoval;

	public bool ReadyForRemoval => _readyForRemoval;

	public DynamicLightMask(ContentManager contentManager, Vector2 position)
	{
		_position = position;
		_readyForRemoval = false;
	}

	public abstract void Update(GameTime gameTime);

	public abstract void Draw(SpriteBatch spriteBatch, Vector2 offset);
}
