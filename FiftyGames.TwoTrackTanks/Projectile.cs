using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.TwoTrackTanks;

internal class Projectile : PhysicsObject
{
	public const float DamageRadius = 140f;

	private const float DestroyRange = 10f;

	private Tank _owner;

	private bool _destroyed;

	private Vector2 _shellOrigin;

	private Vector2 _destination;

	public Tank Owner => _owner;

	public bool Destroyed => _destroyed;

	public Projectile(Tank owner)
	{
		_destination = owner.TargetPosition;
		_shellOrigin = owner.Position;
		_owner = owner;
		_destroyed = false;
	}

	public void Load(ContentManager contentLoader, World physicsWorld)
	{
		base.Sprite = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\ProjectileShell");
		_physBody = BodyFactory.CreateCircle(physicsWorld, ConvertUnits.ToSimUnits(6f), 4f);
		_physBody.BodyType = BodyType.Dynamic;
		_physBody.CollisionCategories = Category.Cat3;
		_physBody.CollidesWith = Category.Cat2;
		_physBody.OnCollision += _physBody_OnCollision;
		_position = base.Position;
	}

	public new void Update(GameTime gameTime)
	{
		if ((base.Position - _destination).Length() <= 10f)
		{
			Destroy();
		}
		base.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		float num = (base.Position - _shellOrigin).Length() / (_destination - _shellOrigin).Length();
		float num2 = (_destination - _shellOrigin).Length() * 0.002f;
		_scale = (num * 2f - 1f) * (num * 2f - 1f) * (0f - num2) + (1f + num2);
		base.Draw(spriteBatch);
	}

	private bool _physBody_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (_physBody != null)
		{
			Destroy();
		}
		return false;
	}

	private void Destroy()
	{
		_physBody.Dispose();
		_physBody = null;
		_destroyed = true;
	}
}
