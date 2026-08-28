using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Guns;

internal class Pistol : Gun
{
	public static GunSettings Settings { get; set; }

	public Pistol(ZombiePlayer owner)
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
		_gunSprite = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Guns/HoldPistol");
	}

	public override List<Shot> Shoot(Vector2 origin, float mainRotation)
	{
		List<Shot> list = new List<Shot>();
		Shot item = default(Shot);
		for (int i = 0; i < _shotsAtOnce; i++)
		{
			int shotLength = _shotLength;
			Vector2 vector = GeometryHelper.AngleToV2(mainRotation + MathHelper.ToRadians(_rand.Next(_spreadAngle / 2 * -1, _spreadAngle / 2)), shotLength);
			item.startPosition = origin;
			item.bulletVector = vector;
			vector.Normalize();
			item.direction = vector;
			item.magnitude = shotLength;
			item.startColor = Color.White;
			item.endColor = Color.White;
			list.Add(item);
			ZombieUtils.PlaySound("Shoot Pistol");
		}
		_owner.FrameworkPlayer.GamePadManager.StartVibration(Settings.VibrationPerShot);
		_rounds--;
		return list;
	}
}
