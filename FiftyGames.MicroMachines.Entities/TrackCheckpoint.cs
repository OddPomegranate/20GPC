using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using MicroMachinesGame;
using MicroMachinesGame.ISHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.MicroMachines.Entities;

internal class TrackCheckpoint : PhysObject
{
	private SinglePixelTexture _trackLine;

	private Rectangle _trackLineRect;

	private float _alpha = 0.2f;

	public int ID { get; set; }

	public TrackCheckpoint(Vector2 start, Vector2 end, bool isHorizontal, World world, int id, GraphicsDevice _graphicsDevice)
		: base(world)
	{
		_body = BodyFactory.CreateEdge(world, ConvertUnits.ToSimUnits(start), ConvertUnits.ToSimUnits(end));
		_body.IsSensor = true;
		_body.OnCollision += _body_OnCollision;
		ID = id;
		_trackLine = new SinglePixelTexture(_graphicsDevice);
		if (isHorizontal)
		{
			_trackLineRect = new Rectangle((int)start.X, (int)start.Y, (int)(end.X - start.X), 6);
		}
		else
		{
			_trackLineRect = new Rectangle((int)start.X, (int)start.Y, 6, (int)(end.Y - start.Y));
		}
	}

	private bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (fixtureB.Body.UserData is MMPlayer)
		{
			MMPlayer mMPlayer = fixtureB.Body.UserData as MMPlayer;
			mMPlayer.OnPastCheckpoint(this);
		}
		return true;
	}

	public override void Update(GameTime gameTime)
	{
		if (_alpha > 0.2f)
		{
			_alpha -= 0.05f;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_trackLine, _trackLineRect, Color.White * _alpha);
		spriteBatch.End();
	}

	public void BlinkCheckpoint(MMPlayer player)
	{
		_alpha = 1f;
	}
}
