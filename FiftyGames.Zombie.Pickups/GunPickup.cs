using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FiftyGames.Zombie.Utils;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Pickups;

internal abstract class GunPickup : Pickup
{
	public GunPickup(PickupManager pickupManager, Vector2 position, int id, int amountOfAmmo, bool dummy)
		: base(pickupManager, position, id, dummy)
	{
		base.NumberSupplied = amountOfAmmo;
		if (!dummy)
		{
			_body = BodyFactory.CreateRectangle(ZombieUtils.World(), ConvertUnits.ToSimUnits(55), ConvertUnits.ToSimUnits(37), 10f, Vector2.Zero);
			_body.BodyType = BodyType.Dynamic;
			_body.Friction = 0f;
			_body.Position = ConvertUnits.ToSimUnits(_position + new Vector2(29f, 18f));
			_body.Mass = 0f;
			_body.SleepingAllowed = true;
			_body.UserData = this;
		}
	}
}
