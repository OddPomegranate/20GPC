using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.World_Ridgid_Bodies;

internal class Barrel : WorldRidgidBody
{
	public Barrel(World world, ContentManager contentManager, Vector2 position)
		: base(world, contentManager, "Shooter/Objects/Barrel", position)
	{
		_body = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(21), 10f);
		_body.BodyType = BodyType.Dynamic;
		_body.Position = ConvertUnits.ToSimUnits(position);
		_body.Friction = 10f;
		_body.Mass = 1f;
		_body.LinearDamping = 6f;
		_body.AngularDamping = 5f;
		SetBodyUserData();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
	}
}
