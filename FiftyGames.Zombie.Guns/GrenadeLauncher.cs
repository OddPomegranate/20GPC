using System.Collections.Generic;
using FiftyGames.Zombie.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Guns;

internal class GrenadeLauncher : Gun
{
	private List<Grenade> _grenades;

	public static GunSettings Settings { get; set; }

	public GrenadeLauncher(ZombiePlayer owner)
		: base(owner)
	{
		_magazineSize = Settings.MagazineSize;
		_shootInterval = Settings.ShootInterval;
		_shotLength = Settings.ShotLength;
		_spreadAngle = Settings.SpreadAngle;
		_shotsAtOnce = Settings.ShotsAtOnce;
		_playerKickRotation = Settings.PlayerKickRotation;
		_bulletDamage = Settings.BulletDamage;
		_playerKickbackImpulseMultiplier = Settings.PlayerKickbackImpulseMultiplier;
		_isBigGun = Settings.IsBigGun;
		_gunOffset = Vector2.Zero;
		_muzzleType = (MuzzleType)Settings.MuzzleType;
		_endOfGunPosition = Settings.EndOfGunPosition;
		_hasPenertratingPower = Settings.HasPenertratingPower;
		_gunSprite = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Guns/HoldGrenadeLauncher");
		_grenades = new List<Grenade>();
	}

	public override List<Shot> Shoot(Vector2 origin, float mainRotation)
	{
		List<Shot> result = new List<Shot>();
		if (_rounds > 0)
		{
			_grenades.Add(new Grenade(origin - GeometryHelper.AngleToV2(mainRotation, 20f), GeometryHelper.AngleToV2(mainRotation, 1000f), _bulletDamage, _owner));
			ProjectileManager.AddProjectile(_grenades[_grenades.Count - 1]);
			_rounds--;
			ZombieUtils.PlaySound("Shoot Grenadelauncher");
		}
		_owner.FrameworkPlayer.GamePadManager.StartVibration(Settings.VibrationPerShot);
		return result;
	}

	public override void Draw(Vector2 position, float rotation, SpriteBatch spriteBatch)
	{
		base.Draw(position, rotation, spriteBatch);
	}

	public override void DrawPersistant(SpriteBatch spriteBatch)
	{
	}
}
