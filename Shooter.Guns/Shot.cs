using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Guns;

internal abstract class Shot
{
	protected Vector2 _start;

	protected Vector2 _end;

	protected Vector2 _direction;

	public bool IsDead { get; set; }

	public Shot()
	{
	}

	public abstract void Update(GameTime gameTime);

	public abstract void Draw(SpriteBatch spriteBatch);
}
