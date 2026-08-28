using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Shooter.World_Ridgid_Bodies;

internal class Crate : WorldRidgidBody
{
	public Crate(World world, ContentManager contentManager, Vector2 position)
		: base(world, contentManager, "Shooter/Objects/Crate", position)
	{
		_body = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(82), ConvertUnits.ToSimUnits(82), 10f);
		_body.BodyType = BodyType.Dynamic;
		_body.LinearDamping = 10f;
		_body.AngularDamping = 10f;
		_body.Mass = 1f;
		_body.Position = ConvertUnits.ToSimUnits(position);
		SetBodyUserData();
	}

	public override void Update(GameTime gameTime)
	{
	}
}
