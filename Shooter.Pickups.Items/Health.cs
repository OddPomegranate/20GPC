using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Shooter.Entities;

namespace Shooter.Pickups.Items;

internal class Health : Pickup
{
	public Health(World world, ContentManager contentManager, Vector2 position)
		: base(world, contentManager, position, "Shooter/Objects/Health")
	{
		_body = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(48), ConvertUnits.ToSimUnits(45), 10f);
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
				shooterPlayer.OnHealthPickedUp();
				OnPickedUp();
			}
			return false;
		}
		return false;
	}
}
