using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.PlatformsAreFalling2;

internal class Platform
{
	private Random _random;

	private Vector2 _position;

	private Vector2 _velocity;

	private Vector2 _origin;

	private float _scale;

	private Texture2D _sprite;

	private bool _active;

	private int _widthHalf;

	private int _heightHalf;

	private Body _rectangle;

	private int _screenEdge;

	private int _zone;

	private World _world;


	public Vector2 Position => _position;

	public Texture2D Sprite => _sprite;

	public Vector2 Origin => _origin;

	public float Scale => _scale;

	public bool Active => _active;

	public int WidthHalf => _widthHalf;

	public int HeightHalf => _widthHalf;

	public int Zone => _zone;

	public Platform(ref Random random, Texture2D sprite, int screenWidth, int screenOffsetMax, int zone, ref float[] prevY, World world)
	{
		_sprite = sprite;
		_zone = zone;
		_widthHalf = _sprite.Width / 2;
		_heightHalf = _sprite.Height / 2;
		_origin = new Vector2(_widthHalf, _heightHalf);
		_random = random;
		_world = world;
		_velocity = Vector2.UnitY * 4f;
		_screenEdge = (1280 - screenWidth) / 2;
		_scale = 0.5f + (float)_random.NextDouble() * 0.5f;
		_widthHalf = (int)((float)_widthHalf * _scale);
		_heightHalf = (int)((float)_heightHalf * _scale);
		prevY[zone] = Math.Min(screenOffsetMax, prevY[_zone]);
		prevY[zone] = Math.Min(prevY[_zone], prevY[(_zone + 1) % 3]);
		prevY[zone] = Math.Min(prevY[_zone], prevY[(_zone + 2) % 3]);
		_position = new Vector2(_random.Next(_screenEdge + _widthHalf, 1280 - _screenEdge - _widthHalf), (float)(-_heightHalf) + prevY[zone] - (float)_random.Next(128, 600));
		_rectangle = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(_widthHalf * 2), ConvertUnits.ToSimUnits(_heightHalf * 2), 1f);
		_rectangle.BodyType = BodyType.Dynamic;
		_rectangle.Position = ConvertUnits.ToSimUnits(_position);
		_rectangle.Mass = 100f;
		_rectangle.CollisionCategories = Category.Cat10;
		_rectangle.FixtureList[0].Body.UserData = 1;
		_rectangle.LinearVelocity = new Vector2(0f, 20f);
		FixedPrismaticJoint joint = new FixedPrismaticJoint(_rectangle, _rectangle.Position, new Vector2(0f, 1f));
		world.AddJoint(joint);
		_rectangle.OnCollision += SimpleDemo10_OnCollision;
		prevY[_zone] = _position.Y;
		_active = true;
	}

	public void Update(List<Platform> platforms, float acidPosition, ref float[] prevY)
	{
		_rectangle.LinearVelocity = new Vector2(0f, 10f);
		prevY[_zone] = ConvertUnits.ToDisplayUnits(_rectangle.Position.Y);
		if (_position.Y > acidPosition + 1000f)
		{
			platforms.Remove(this);
			_rectangle.Dispose();
			_rectangle = null;
		}
	}

	public void Draw(SpriteBatch spriteBatch, float screenOffset)
	{
		spriteBatch.Draw(destinationRectangle: new Rectangle((int)ConvertUnits.ToDisplayUnits(_rectangle.Position.X), (int)(ConvertUnits.ToDisplayUnits(_rectangle.Position.Y) - screenOffset), (int)((float)_sprite.Width * _scale), (int)((float)_sprite.Height * _scale)), texture: _sprite, sourceRectangle: null, color: Color.White, rotation: 0f, origin: _origin, effects: SpriteEffects.None, layerDepth: 0f);
	}

	private bool SimpleDemo10_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		contact.GetManifold(out var _);
		if ((int)fixtureB.Body.UserData != 1 && (int)fixtureB.Body.UserData < 30)
		{
			Vector2 position = _rectangle.Position;
			_rectangle.OnCollision -= SimpleDemo10_OnCollision;
			_rectangle.Dispose();
			_rectangle = null;
			_rectangle = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(_widthHalf * 2), ConvertUnits.ToSimUnits(_heightHalf * 2), 1f);
			_rectangle.Position = position;
			_rectangle.CollisionCategories = Category.Cat10;
			_rectangle.BodyType = BodyType.Static;
			_rectangle.UserData = 4;
			_rectangle.Friction = 0.1f;
			_active = false;
		}
		return true;
	}
}
