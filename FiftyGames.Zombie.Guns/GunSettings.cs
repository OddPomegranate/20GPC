using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Guns;

internal class GunSettings
{
	public int MagazineSize { get; set; }

	public bool HasPenertratingPower { get; set; }

	public int ShootInterval { get; set; }

	public int ShotLength { get; set; }

	public int SpreadAngle { get; set; }

	public int ShotsAtOnce { get; set; }

	public int PlayerKickRotation { get; set; }

	public int BulletDamage { get; set; }

	public Vector2 PlayerKickbackImpulseMultiplier { get; set; }

	public bool IsBigGun { get; set; }

	public int MuzzleType { get; set; }

	public Vector2 EndOfGunPosition { get; set; }

	public int VibrationPerShot { get; set; }
}
