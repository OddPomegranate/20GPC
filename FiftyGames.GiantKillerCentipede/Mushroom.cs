using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Mushroom : PhysicsObject
{
	public const int MaxHealth = 50;

	protected const int AnimTime = 1000;

	protected int _health;

	protected object _lastToDamage;

	protected int _animTimer;

	protected float _originOffset;

	protected float _originBase;

	private Texture2D[] _sprites;

	public int Health => _health;

	public object Destroyer => _lastToDamage;

	public Mushroom(Vector2 position, Random rng)
	{
		_position = position;
		_physVolume.Center = new Vector3(_position, 0f);
		_physVolume.Radius = 24f;
		_health = 50;
		_sprites = new Texture2D[4];
		_animTimer = rng.Next(1000);
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprites[0] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\Mushroom0");
		_sprites[1] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\Mushroom1");
		_sprites[2] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\Mushroom2");
		_sprites[3] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\Mushroom3");
		_sprite = _sprites[0];
		base.Load(contentLoader);
		_originBase = _origin.Y;
	}

	public override void Update(GameTime gameTime)
	{
		if (_animTimer > 1000)
		{
			_originOffset = _originOffset * -1f + 6f;
			_animTimer = 0;
		}
		_animTimer += gameTime.ElapsedGameTime.Milliseconds;
		_origin.Y = _originBase + _originOffset;
		base.Update(gameTime);
		if ((float)_health > 37.5f)
		{
			_sprite = _sprites[0];
		}
		else if ((float)_health > 25f)
		{
			_sprite = _sprites[1];
		}
		else if ((float)_health > 12.5f)
		{
			_sprite = _sprites[2];
		}
		else
		{
			_sprite = _sprites[3];
		}
	}

	public void Damage(int damage, object instigator)
	{
		if (_health != 0)
		{
			_health -= damage;
			_lastToDamage = instigator;
			if (_health < 0)
			{
				_health = 0;
			}
			_animTimer = 1000;
		}
	}
}
