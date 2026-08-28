using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Shooter.Entities;

namespace Shooter.Pickups.Items;

internal class Ammo : Pickup
{
	public Ammo(World world, ContentManager contentManager, Vector2 position)
		: base(world, contentManager, position, "Shooter/Objects/Ammo")
	{
		_body = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(63), ConvertUnits.ToSimUnits(46), 10f);
		_body.BodyType = BodyType.Static;
		_body.Position = ConvertUnits.ToSimUnits(position);
		_body.OnCollision += body_OnCollision;
		SetBodyUserData();
	}

	private bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (_isActive)
		{
			if (fixtureB.Body.UserData != null && fixtureB.Body.UserData is ShooterPlayer shooterPlayer)
			{
				shooterPlayer.OnAmmoPickedUp();
				OnPickedUp();
			}
			return false;
		}
		return false;
	}
}
