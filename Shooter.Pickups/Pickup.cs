using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Pickups;

internal class Pickup : PhysObject, IComparable
{
	private ContentManager _contentManager;

	private Texture2D _texture;

	private int _millsSincePickedUp;

	protected bool _isActive;

	protected int _timeUntilRespawn;

	public float TempDistanceFromPlayer { get; set; }

	public bool HasPlayersAroundMe { get; set; }

	public Pickup(World world, ContentManager contentManager, Vector2 position, string texturePath)
		: base(world)
	{
		_contentManager = contentManager;
		_texture = contentManager.Load<Texture2D>(texturePath);
		_isActive = true;
		_millsSincePickedUp = 0;
		_timeUntilRespawn = 10000;
	}

	public override void Update(GameTime gameTime)
	{
		if (!_isActive)
		{
			_millsSincePickedUp += gameTime.ElapsedGameTime.Milliseconds;
			if (_millsSincePickedUp >= _timeUntilRespawn)
			{
				_millsSincePickedUp = 0;
				_isActive = true;
				_body.CollisionCategories = Category.All;
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isActive)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(_texture, base.DisplayPosition, null, Color.White, _body.Rotation, new Vector2(_texture.Width / 2, _texture.Height / 2), 1f, SpriteEffects.None, 0f);
			spriteBatch.End();
		}
	}

	public void OnPickedUp()
	{
		_isActive = false;
		_body.CollisionCategories = Category.None;
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public int CompareTo(object obj)
	{
		int result = 1;
		if (obj != null && obj is Pickup)
		{
			Pickup pickup = obj as Pickup;
			result = TempDistanceFromPlayer.CompareTo(pickup.TempDistanceFromPlayer);
		}
		return result;
	}
}
