using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Shooter.World_Ridgid_Bodies;

internal class MetalCrate : WorldRidgidBody
{
	public MetalCrate(World world, ContentManager contentManager, Vector2 position)
		: base(world, contentManager, "Shooter/Objects/MetalCrate", position)
	{
		_body = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(82), ConvertUnits.ToSimUnits(52), 10f);
		_body.BodyType = BodyType.Dynamic;
		_body.LinearDamping = 8f;
		_body.AngularDamping = 4f;
		_body.Mass = 1f;
		_body.Position = ConvertUnits.ToSimUnits(position);
		SetBodyUserData();
	}

	public override void Update(GameTime gameTime)
	{
	}
}
