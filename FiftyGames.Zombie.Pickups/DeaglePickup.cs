using FiftyGames.Zombie.Guns;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Pickups;

internal class DeaglePickup : GunPickup
{
	public DeaglePickup(PickupManager pickupManager, Vector2 position, int id, int ammo, bool dummy)
		: base(pickupManager, position, id, ammo, dummy)
	{
		base.Sprite = ZombieUtils.ContentManager().Load<Texture2D>("Zombie/Pickups/PickupDeagle");
	}

	public override void OnPlayerTouch(ZombiePlayer player)
	{
		if (player.CurrentGun is Deagle deagle)
		{
			deagle.AddRounds(player.CurrentGun.MagazineSize);
		}
		else
		{
			player.CurrentGun = new Deagle(player);
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
