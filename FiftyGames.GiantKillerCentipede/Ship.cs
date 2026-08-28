using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.GiantKillerCentipede;

internal class Ship : PhysicsObject
{
	protected const int PreferenceBubbleTime = 2000;

	protected const int FlashFrames = 2;

	protected const int ProtectTime = 2000;

	protected const int GrowSpeed = 400;

	protected const float GrowSize = 1f;

	protected const float PulseSpeed = 0.06f;

	protected const float Acceleration = 4f;

	protected const float MaxSpeed = 5f;

	protected const float MinSpeed = 0.001f;

	protected const float Friction = 0.2f;

	private ContentManager _contentLoader;

	private SoundManager _soundManager;

	protected Player _player;

	protected Texture2D _colourSprite;

	protected Texture2D[] _shieldSprites;

	protected Random _ranGen;

	protected Texture2D[] _preferenceBubbles;

	protected Vector2 _preferenceBubbleOrigin;

	protected bool _centipedeCandidate;

	protected int _preferenceChangeTimer;

	protected bool _alive;

	protected int _shield;

	protected int _flashTimer;

	protected int _protectTimer;

	protected int _growTimer;

	protected float _pulseTimer;

	protected Powerup.PowerupType _powerWeapon;

	protected int _powerAmmo;

	protected int _bulletShotTimer;

	protected int _specialShotTimer;

	public Player Player => _player;

	public bool ElegableCentipede
	{
		get
		{
			return _centipedeCandidate;
		}
		set
		{
			_centipedeCandidate = value;
		}
	}

	public bool IsAlive
	{
		get
		{
			return _alive;
		}
		set
		{
			_alive = value;
		}
	}

	public int Shields
	{
		get
		{
			int result = _shield;
			if (_protectTimer != 0)
			{
				result = 1;
			}
			if (_shield == -1)
			{
				result = 0;
			}
			return result;
		}
	}

	public bool Invunerable => _protectTimer != 0;

	public Random RandomGenerator => _ranGen;

	public SoundManager SoundManager
	{
		get
		{
			return _soundManager;
		}
		set
		{
			_soundManager = value;
		}
	}

	public Ship(Player player, Vector2 position, Random rng)
	{
		_player = player;
		if (player != null)
		{
			_colour = player.Colour();
		}
		else
		{
			byte[] array = new byte[3];
			rng.NextBytes(array);
			_colour = new Color(array[0], array[1], array[2]);
		}
		_position = position;
		_ranGen = rng;
		_shield = 1;
		_powerWeapon = Powerup.PowerupType.None;
		_powerAmmo = 0;
		_bulletShotTimer = 0;
		_specialShotTimer = 0;
		_alive = true;
		_centipedeCandidate = true;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ShipColour");
		_colourSprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ShipOverlay");
		_shieldSprites = new Texture2D[3];
		_shieldSprites[0] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\Shield0");
		_shieldSprites[1] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\Shield1");
		_shieldSprites[2] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\Shield2");
		_preferenceBubbles = new Texture2D[2];
		_preferenceBubbles[0] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\OptIn");
		_preferenceBubbles[1] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\OptOut");
		_preferenceBubbleOrigin = new Vector2((float)_preferenceBubbles[0].Width * 0.5f, (float)_preferenceBubbles[0].Height * 0.5f);
		base.Load(contentLoader);
		_contentLoader = contentLoader;
	}

	public virtual void Update(GameTime gameTime, List<Projectile> projectiles, bool gameOver)
	{
		if (!gameOver && _player != null)
		{
			_velocity += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left * new Vector2(4f, 0f);
			if (_bulletShotTimer == 0 && (_player.GamePadManager.ButtonIsHeld(Buttons.A) || _player.GamePadManager.ButtonIsHeld(Buttons.RightTrigger)))
			{
				FireBullet(projectiles);
			}
			if (_specialShotTimer == 0 && _powerWeapon != Powerup.PowerupType.None && (_player.GamePadManager.ButtonIsHeld(Buttons.B) || _player.GamePadManager.ButtonIsHeld(Buttons.LeftTrigger)))
			{
				FireSpecial(projectiles);
				if (_powerAmmo == 0)
				{
					_powerWeapon = Powerup.PowerupType.None;
				}
			}
		}
		if (_velocity.Length() < 0.001f)
		{
			_velocity = Vector2.Zero;
		}
		else
		{
			_velocity += _velocity * -0.2f;
		}
		if (_velocity.Length() > 5f)
		{
			Vector2 velocity = _velocity;
			velocity.Normalize();
			_velocity = velocity * 5f;
		}
		base.Update(gameTime);
		if (_position.X < 0f)
		{
			_position.X += 1280f;
		}
		if (_position.X > 1280f)
		{
			_position.X -= 1280f;
		}
		if (_bulletShotTimer != 0)
		{
			_bulletShotTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (_bulletShotTimer < 0)
			{
				_bulletShotTimer = 0;
			}
		}
		if (_specialShotTimer != 0)
		{
			_specialShotTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (_specialShotTimer < 0)
			{
				_specialShotTimer = 0;
			}
		}
		if (_protectTimer != 0)
		{
			_flashTimer++;
			if (_flashTimer == 4)
			{
				_flashTimer = 0;
			}
			_protectTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (_protectTimer < 0)
			{
				_protectTimer = 0;
				_flashTimer = 0;
			}
		}
		else
		{
			_protectTimer = 0;
			_flashTimer = 0;
		}
		if (_powerWeapon != Powerup.PowerupType.None && _growTimer < 400)
		{
			_growTimer += gameTime.ElapsedGameTime.Milliseconds;
		}
		else
		{
			_growTimer = 0;
		}
		_pulseTimer += 0.06f;
		if (_pulseTimer > (float)Math.PI * 2f)
		{
			_pulseTimer -= (float)Math.PI * 2f;
		}
		if (_preferenceChangeTimer != 0)
		{
			_preferenceChangeTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (_preferenceChangeTimer < 0)
			{
				_preferenceChangeTimer = 0;
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_flashTimer < 2)
		{
			base.Draw(spriteBatch);
			spriteBatch.Draw(_colourSprite, _position, null, Color.White, _rotation, _origin, _scale, SpriteEffects.None, 0f);
			if (_powerWeapon != Powerup.PowerupType.None)
			{
				Color color = _powerWeapon switch
				{
					Powerup.PowerupType.ShockwaveWeapon => Color.Violet, 
					Powerup.PowerupType.RocketWeapon => Color.Maroon, 
					Powerup.PowerupType.HeatseekerWeapon => Color.Orange, 
					Powerup.PowerupType.GrenadeWeapon => Color.DarkGreen, 
					Powerup.PowerupType.LaserWeapon => Color.Turquoise, 
					_ => Color.White, 
				};
				float num = (float)_growTimer / 400f;
				float num2 = num * 1f;
				spriteBatch.Draw(_sprite, _position, null, color * (1f - num), _rotation, _origin, _scale + num2, SpriteEffects.None, 0f);
			}
			for (int i = 0; i < _shield; i++)
			{
				spriteBatch.Draw(_shieldSprites[i], _position, null, Color.White * (0.75f + (float)Math.Sin(_pulseTimer) * 0.25f), _rotation, _origin + new Vector2(6f * (float)i), _scale, SpriteEffects.None, 0f);
			}
		}
	}

	public void DrawPrefenceBubbles(SpriteBatch spriteBatch)
	{
		if (_preferenceChangeTimer != 0)
		{
			int num = ((!_centipedeCandidate) ? 1 : 0);
			spriteBatch.Draw(_preferenceBubbles[num], _position + new Vector2(0f, -50f), null, Color.White * ((float)_preferenceChangeTimer / 2000f), 0f, _preferenceBubbleOrigin, 1f, SpriteEffects.None, 0f);
		}
	}

	public void FireBullet(List<Projectile> projectileList)
	{
		Projectile projectile = new Bullet(this);
		projectile.Load(_contentLoader);
		_bulletShotTimer = projectile.ShotDelay;
		projectileList.Add(projectile);
		projectile.SoundCue = _soundManager.CreateGameSoundCue("centipede FireBullet");
	}

	public void FireSpecial(List<Projectile> projectileList)
	{
		Projectile projectile;
		switch (_powerWeapon)
		{
		case Powerup.PowerupType.ShockwaveWeapon:
			projectile = new Shockwave(this);
			projectile.SoundCue = _soundManager.CreateGameSoundCue("centipede FireShockwave");
			break;
		case Powerup.PowerupType.RocketWeapon:
			projectile = new Rocket(this);
			projectile.SoundCue = _soundManager.CreateGameSoundCue("centipede FireRocket");
			break;
		case Powerup.PowerupType.HeatseekerWeapon:
			projectile = new Heatseeker(this);
			projectile.SoundCue = _soundManager.CreateGameSoundCue("centipede FireRocket");
			break;
		case Powerup.PowerupType.GrenadeWeapon:
			projectile = new Grenade(this);
			projectile.SoundCue = _soundManager.CreateGameSoundCue("centipede FireNuke");
			break;
		case Powerup.PowerupType.LaserWeapon:
			projectile = new LaserPhoton(this);
			projectile.SoundCue = _soundManager.CreateGameSoundCue("centipede FireLaser");
			break;
		default:
			projectile = new Bullet(this);
			break;
		}
		projectile.Load(_contentLoader);
		_specialShotTimer = projectile.ShotDelay;
		projectileList.Add(projectile);
		_powerAmmo--;
	}

	public void Damage()
	{
		if (_protectTimer == 0)
		{
			_shield--;
			_protectTimer = 2000;
			if (_player != null)
			{
				_player.GamePadManager.StartVibration(800, 1f, 1f, 0f, 0f);
			}
		}
	}

	public void GivePowerup(Powerup.PowerupType type)
	{
		switch (type)
		{
		case Powerup.PowerupType.ShockwaveWeapon:
			_powerWeapon = type;
			_powerAmmo = 3;
			_soundManager.CreateGameSoundCue("centipede PickupShockwave").Play();
			break;
		case Powerup.PowerupType.RocketWeapon:
			_powerWeapon = type;
			_powerAmmo = 8;
			_soundManager.CreateGameSoundCue("centipede PickupRocket").Play();
			break;
		case Powerup.PowerupType.HeatseekerWeapon:
			_powerWeapon = type;
			_powerAmmo = 15;
			_soundManager.CreateGameSoundCue("centipede PickupHeatseeker").Play();
			break;
		case Powerup.PowerupType.GrenadeWeapon:
			_powerWeapon = type;
			_powerAmmo = 2;
			_soundManager.CreateGameSoundCue("centipede PickupGrenade").Play();
			break;
		case Powerup.PowerupType.LaserWeapon:
			_powerWeapon = type;
			_powerAmmo = 300;
			_soundManager.CreateGameSoundCue("centipede PickupLaser").Play();
			break;
		case Powerup.PowerupType.Shield:
			if (_shield < 3)
			{
				_shield++;
			}
			_soundManager.CreateGameSoundCue("centipede PickupShield").Play();
			break;
		}
		if (_player != null)
		{
			_player.GamePadManager.StartVibration(600, 0f, 0f, 0.4f, 0.4f);
		}
	}

	public void ShowPreferenceBubble()
	{
		_preferenceChangeTimer = 2000;
	}
}
