using System;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.HammerFight;

internal class BreakablePiece : TexturedPolygon, IDisposable
{
	public static int CurrentId;

	protected int _id;

	protected Fixture _fixture;

	public bool IsDisposed { get; set; }

	public int ID
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public Fixture Fixture
	{
		get
		{
			return _fixture;
		}
		set
		{
			_fixture = value;
		}
	}

	public BreakablePiece(Vertices polyVerts, float width, float height, GraphicsDevice gd, Matrix projection)
		: base(polyVerts, width, height, gd, projection)
	{
	}

	public void LoadContent(Texture2D texture)
	{
		_texture = texture;
		_id = CurrentId;
		CurrentId++;
		IsDisposed = false;
		base.LoadContent();
	}

	public override void Draw(GraphicsDevice gd, Color color)
	{
		if (_fixture.Body != null)
		{
			SetView(Matrix.CreateRotationZ(_fixture.Body.Rotation) * Matrix.CreateTranslation(new Vector3(_fixture.Body.Position, 0f)));
			base.Draw(gd, color);
		}
	}

	public override void Dispose()
	{
		if (_fixture.Body != null)
		{
			_fixture.Body.Dispose();
		}
	}
}
