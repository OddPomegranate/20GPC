using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter.ISHelpers;

namespace Shooter.Guns;

internal class LaserShot : Shot
{
	private Texture2D _texture;

	private bool _update;

	public LaserShot(Texture2D laserTexture, Vector2 start, Vector2 end)
	{
		_start = start;
		_end = end;
		_texture = laserTexture;
	}

	public override void Update(GameTime gameTime)
	{
		if (_update)
		{
			base.IsDead = true;
		}
		_update = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Vector2 v = _end - _start;
		v.Normalize();
		Vector2 vector = _end - _start;
		vector = _start + vector / 2f;
		float rotation = GeometryHelper.V2ToAngle(v);
		float x = Vector2.Distance(_start, _end);
		if (!base.IsDead)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(_texture, vector, null, Color.Red * 0.5f, rotation, new Vector2(0.5f, 0.5f), new Vector2(x, 5f), SpriteEffects.None, 0f);
			spriteBatch.End();
		}
	}
}
