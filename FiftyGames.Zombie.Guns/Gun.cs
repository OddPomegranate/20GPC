using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Guns;

internal abstract class Gun
{
	protected int _shootInterval;

	protected int _spreadAngle;

	protected int _shotsAtOnce;

	protected int _playerKickRotation;

	protected int _bulletDamage;

	protected int _shotLength;

	protected Vector2 _playerKickbackImpulseMultiplier;

	protected Texture2D _gunSprite;

	protected Vector2 _gunOffset;

	protected bool _isBigGun;

	protected Random _rand;

	protected int _animationFrames;

	protected bool _hasPenertratingPower;

	protected int _rounds;

	protected int _magazineSize;

	protected bool _hasEverBeenReloaded;

	protected MuzzleType _muzzleType;

	protected Vector2 _endOfGunPosition;

	private List<List<Texture2D>> _muzzleTextures;

	private int _muzzleIndex;

	protected ZombiePlayer _owner;

	public int AnimationFrames
	{
		get
		{
			return _animationFrames;
		}
		set
		{
			_animationFrames = value;
		}
	}

	public int ShotLength
	{
		get
		{
			return _shotLength;
		}
		set
		{
			_shotLength = value;
		}
	}

	public int BulletDamage
	{
		get
		{
			return _bulletDamage;
		}
		set
		{
			_bulletDamage = value;
		}
	}

	public Vector2 PlayerKickbackImpulseMultiplier
	{
		get
		{
			return _playerKickbackImpulseMultiplier;
		}
		set
		{
			_playerKickbackImpulseMultiplier = value;
		}
	}

	public int PlayerKickRotation
	{
		get
		{
			return _playerKickRotation;
		}
		set
		{
			_playerKickRotation = value;
		}
	}

	public Vector2 GunOffset
	{
		get
		{
			return _gunOffset;
		}
		set
		{
			_gunOffset = value;
		}
	}

	public bool IsBigGun
	{
		get
		{
			return _isBigGun;
		}
		set
		{
			_isBigGun = value;
		}
	}

	public Texture2D GunSprite
	{
		get
		{
			return _gunSprite;
		}
		set
		{
			_gunSprite = value;
		}
	}

	public int ShotsAtOnce
	{
		get
		{
			return _shotsAtOnce;
		}
		set
		{
			_shotsAtOnce = value;
		}
	}

	public int SpreadAngle
	{
		get
		{
			return _spreadAngle;
		}
		set
		{
			_spreadAngle = value;
		}
	}

	public int ShootInterval
	{
		get
		{
			return _shootInterval;
		}
		set
		{
			_shootInterval = value;
		}
	}

	public bool HasPenertratingPower => _hasPenertratingPower;

	public int RoundsRemaining
	{
		get
		{
			return _rounds;
		}
		set
		{
			_rounds = value;
		}
	}

	public int MagazineSize => _magazineSize;

	public bool HasEverBeenReloaded
	{
		get
		{
			return _hasEverBeenReloaded;
		}
		set
		{
			_hasEverBeenReloaded = value;
		}
	}

	public Gun(ZombiePlayer owner)
	{
		_owner = owner;
		_rand = new Random();
		_hasPenertratingPower = false;
		_hasEverBeenReloaded = false;
		List<Texture2D> item = new List<Texture2D>
		{
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleBright1"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleBright2"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleBright3")
		};
		List<Texture2D> item2 = new List<Texture2D>
		{
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleFat1"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleFat2"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleFat3")
		};
		List<Texture2D> item3 = new List<Texture2D>
		{
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleLong1"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleLong2"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleLong3")
		};
		List<Texture2D> item4 = new List<Texture2D>
		{
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleRed1"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleRed2")
		};
		List<Texture2D> item5 = new List<Texture2D>
		{
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleSmoke1"),
			ZombieUtils.ContentManager().Load<Texture2D>("Zombie/MuzzleSprites/MuzzleSmoke2")
		};
		_muzzleTextures = new List<List<Texture2D>>();
		_muzzleTextures.Add(item);
		_muzzleTextures.Add(item2);
		_muzzleTextures.Add(item3);
		_muzzleTextures.Add(item4);
		_muzzleTextures.Add(item5);
	}

	public abstract List<Shot> Shoot(Vector2 origin, float mainRotation);

	public virtual void Draw(Vector2 position, float rotation, SpriteBatch spriteBatch)
	{
		Vector2 vector = position + _gunOffset;
		Rectangle bounds = _gunSprite.Bounds;
		bounds.X += (int)vector.X;
		bounds.Y += (int)vector.Y;
		spriteBatch.Begin();
		spriteBatch.Draw(_gunSprite, bounds, null, Color.White, rotation, new Vector2(0f, -15f), SpriteEffects.None, 0f);
		spriteBatch.End();
	}

	public void DrawMuzzle(SpriteBatch spriteBatch, Vector2 position, float rotation)
	{
		List<Texture2D> list = _muzzleTextures[(int)_muzzleType];
		Texture2D texture2D = list[_muzzleIndex];
		Rectangle bounds = texture2D.Bounds;
		bounds.X += (int)position.X;
		bounds.Y += (int)position.Y;
		spriteBatch.Begin();
		spriteBatch.Draw(texture2D, bounds, null, Color.White, rotation, _endOfGunPosition, SpriteEffects.None, 0f);
		spriteBatch.End();
		if (_muzzleIndex + 1 < list.Count)
		{
			_muzzleIndex++;
		}
		else
		{
			_muzzleIndex = 0;
		}
	}

	public virtual void DrawPersistant(SpriteBatch spriteBatch)
	{
	}

	public void AddRounds(int numberOfRounds)
	{
		if (!_hasEverBeenReloaded)
		{
			ZombieUtils.PlaySound("Pick Up Gun");
			_hasEverBeenReloaded = true;
		}
		else
		{
			ZombieUtils.PlaySound("Collect Ammo");
		}
		_rounds += numberOfRounds;
	}
}
