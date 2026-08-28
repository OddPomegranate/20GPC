using FiftyGames.Zombie.Guns;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Pickups;

internal class ShotgunPickup : GunPickup
{
	public ShotgunPickup(PickupManager pickupManager, Vector2 position, int id, int ammo, bool dummy)
		: base(pickupManager, position, id, ammo, dummy)
	{
		base.Sprite = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Pickups/PickupShotgun");
	}

	public override void OnPlayerTouch(ZombiePlayer player)
	{
		if (player.CurrentGun is Shotgun shotgun)
		{
			shotgun.AddRounds(player.CurrentGun.MagazineSize);
		}
		else
		{
			player.CurrentGun = new Shotgun(player);
			player.CurrentGun.AddRounds(player.CurrentGun.MagazineSize);
		}
		base.OnPlayerTouch(player);
	}

	public override void Draw()
	{
		ZombieUtils.SpriteBatch.Begin();
		ZombieUtils.SpriteBatch.Draw(base.Sprite, base.Position + ZombieUtils.Offset, Color.White);
		ZombieUtils.SpriteBatch.End();
	}
}
