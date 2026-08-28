using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.TwoTrackTanks;

internal class Turntable : PhysicsObject
{
	private const int PauseTime = 6000;

	private float[] _sinRot;

	private int _pauseTimer;

	private bool _occupied;

	public bool HasStopped => _pauseTimer > 1000;

	public bool IsVertical
	{
		get
		{
			if (!((double)_sinRot[0] > 0.5))
			{
				return false;
			}
			return true;
		}
	}

	public bool BeingUsed
	{
		get
		{
			return _occupied;
		}
		set
		{
			_occupied = value;
		}
	}

	public Turntable()
	{
		_sinRot = new float[4];
	}

	public void Load(ContentManager contentLaoder, World physicsWorld)
	{
		base.Sprite = contentLaoder.Load<Texture2D>("TwoTrackTanks/Image/Turntable");
		_physBody = BodyFactory.CreateCircle(physicsWorld, ConvertUnits.ToSimUnits(130f), 1f);
		_physBody.BodyType = BodyType.Kinematic;
		_physBody.CollisionCategories = Category.Cat4;
		_physBody.AngularVelocity = ConvertUnits.ToSimUnits(0.01f);
		_physBody.AngularDamping = 0f;
		_physBody.OnCollision += CollisionEvent;
	}

	public override void Update(GameTime gameTime)
	{
		if (_pauseTimer == 0 && !_occupied)
		{
			_physBody.AngularVelocity = ConvertUnits.ToSimUnits(0.01f);
			_sinRot[3] = _sinRot[2];
			_sinRot[2] = _sinRot[1];
			_sinRot[1] = _sinRot[0];
			_sinRot[0] = (float)Math.Abs(Math.Sin(base.Rotation));
			if ((_sinRot[3] < _sinRot[2] && _sinRot[1] > _sinRot[0]) || (_sinRot[3] > _sinRot[2] && _sinRot[1] < _sinRot[0]))
			{
				_pauseTimer = 6000;
				_physBody.AngularVelocity = 0f;
				_sinRot[3] = (_sinRot[2] = (_sinRot[1] = _sinRot[0]));
			}
		}
		else
		{
			_pauseTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (_pauseTimer < 0)
			{
				_pauseTimer = 0;
			}
		}
		base.Update(gameTime);
	}

	public void ForceStart()
	{
		_pauseTimer = 0;
	}

	private bool CollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (_pauseTimer == 0 && !_occupied)
		{
			if (fixtureB.Body.UserData == null || (object)fixtureB.Body.UserData.GetType() != typeof(Tank))
			{
				fixtureB.Body.LinearVelocity = Vector2.Zero;
				fixtureB.Body.AngularVelocity = 0f;
			}
			Vector2 position = Vector2.UnitY;
			float num = 0f;
			if (fixtureB.Body.Position != _physBody.Position)
			{
				position = fixtureB.Body.Position - _physBody.Position;
				num = position.Length();
			}
			float num2 = num / fixtureA.Shape.Radius;
			position.Normalize();
			Vector2 position2 = Vector2.Transform(position, Matrix.CreateRotationZ((float)Math.PI / 2f));
			float num3 = (num + 0.08f - fixtureA.Shape.Radius) / 0.32f;
			num3 = ((num3 < -1f) ? (-1f) : num3);
			num3 = ((num3 > 0.5f) ? 0.5f : num3);
			fixtureB.Body.ApplyAngularImpulse(_physBody.AngularVelocity * (0f - num3) * ConvertUnits.ToSimUnits(170f) * fixtureB.Body.Mass);
			fixtureB.Body.ApplyLinearImpulse(Vector2.Transform(position2, Matrix.CreateRotationZ(-(float)Math.PI / 50f / fixtureB.Body.Mass * (float)Math.Pow(num2, 10.0))) * _physBody.AngularVelocity * ((num2 > 1f) ? 1f : num2) * ConvertUnits.ToSimUnits(120f) * fixtureB.Body.Mass);
		}
		return false;
	}
}
