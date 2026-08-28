using FiftyGames.Zombie.Guns;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Pickups;

internal class SubmachineGunPickup : GunPickup
{
	public SubmachineGunPickup(PickupManager pickupManager, Vector2 position, int id, int ammo, bool dummy)
		: base(pickupManager, position, id, ammo, dummy)
	{
		base.Sprite = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Pickups/PickupSubMachineGun");
	}

	public override void OnPlayerTouch(ZombiePlayer player)
	{
		if (player.CurrentGun is SubmachineGun submachineGun)
		{
			submachineGun.AddRounds(player.CurrentGun.MagazineSize);
		}
		else
		{
			player.CurrentGun = new SubmachineGun(player);
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
