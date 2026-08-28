using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal abstract class Projectile
{
	protected bool _isAlive;

	protected Body _body;

	public bool IsAlive
	{
		get
		{
			return _isAlive;
		}
		set
		{
			_isAlive = value;
		}
	}

	public Vector2 Position => ConvertUnits.ToDisplayUnits(_body.Position);

	public Projectile()
	{
		_isAlive = true;
	}

	public abstract void Update(GameTime gameTime);

	public abstract void Draw(SpriteBatch spriteBatch, Vector2 offset);

	public void Dispose()
	{
		if (!_body.IsDisposed)
		{
			_body.Dispose();
			_body = null;
		}
	}
}
