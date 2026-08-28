using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Impossible;

internal class fullScreenQuad
{
	private VertexPositionTexture[] verts;

	private short[] ib;

	private GraphicsDevice _graphicsDevice;

	public fullScreenQuad(GraphicsDevice graphicsDevice)
	{
		verts = new VertexPositionTexture[4]
		{
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(1f, 1f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 1f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 0f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(1f, 0f))
		};
		ib = new short[6] { 0, 1, 2, 2, 3, 0 };
		_graphicsDevice = graphicsDevice;
	}

	public void Render(Vector2 v1, Vector2 v2)
	{
		verts[0].Position.X = v2.X;
		verts[0].Position.Y = v1.Y;
		verts[1].Position.X = v1.X;
		verts[1].Position.Y = v1.Y;
		verts[2].Position.X = v1.X;
		verts[2].Position.Y = v2.Y;
		verts[3].Position.X = v2.X;
		verts[3].Position.Y = v2.Y;
		_graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
	}
}
