using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroMachinesGame;

internal abstract class PhysObject
{
	protected World _world;

	protected Body _body;

	public Body Body => _body;

	public Vector2 DisplayPosition
	{
		get
		{
			if (_body != null)
			{
				if (!_body.IsDisposed)
				{
					return ConvertUnits.ToDisplayUnits(_body.Position);
				}
				return Vector2.Zero;
			}
			return Vector2.Zero;
		}
	}

	public float Rotation
	{
		get
		{
			if (_body != null)
			{
				if (!_body.IsDisposed)
				{
					return _body.Rotation;
				}
				return 0f;
			}
			return 0f;
		}
	}

	public PhysObject(World world)
	{
		_world = world;
	}

	public abstract void Update(GameTime gameTime);

	public abstract void Draw(SpriteBatch spriteBatch);

	public void DestroyBody()
	{
		if (_body != null && !_body.IsDisposed)
		{
			_body.Dispose();
		}
	}

	public void SetBodyUserData()
	{
		if (_body != null)
		{
			_body.UserData = this;
		}
	}
}
