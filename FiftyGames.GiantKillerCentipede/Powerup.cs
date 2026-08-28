using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Powerup : PhysicsObject
{
	public enum PowerupType
	{
		None,
		ShockwaveWeapon,
		RocketWeapon,
		HeatseekerWeapon,
		GrenadeWeapon,
		LaserWeapon,
		Shield
	}

	protected PowerupType _type;

	public PowerupType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public Powerup(PowerupType type, Vector2 position)
	{
		_type = type;
		_position = position;
		_velocity = new Vector2(0f, 2f);
	}

	public override void Load(ContentManager contentLoader)
	{
		switch (_type)
		{
		case PowerupType.ShockwaveWeapon:
			_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\PowerupShockwave");
			break;
		case PowerupType.RocketWeapon:
			_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\PowerupRocket");
			break;
		case PowerupType.HeatseekerWeapon:
			_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\PowerupHeatseeker");
			break;
		case PowerupType.GrenadeWeapon:
			_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\PowerupGrenade");
			break;
		case PowerupType.LaserWeapon:
			_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\PowerupLaser");
			break;
		case PowerupType.Shield:
			_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\PowerupShield");
			break;
		}
		base.Load(contentLoader);
		_physVolume.Radius = (float)_sprite.Width / 2f;
	}
}
