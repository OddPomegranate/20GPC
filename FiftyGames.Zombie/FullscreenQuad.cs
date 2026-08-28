using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class FullscreenQuad
{
	private VertexPositionTexture[] verts;

	private short[] ib;

	private GraphicsDevice _graphicsDevice;

	public FullscreenQuad(GraphicsDevice graphicsDevice)
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

	// ADDED (round 6, offset-lighting bug fix): renders a full -1..1 clip-space quad
	// rotated around its own center -- used by ShadowHelper2D's "Copy" pass, which
	// replicates a SpriteBatch.Draw that centered a light-mask sprite in its target,
	// scaled to exactly fill it, and rotated it around its own center. SpriteBatch's
	// rotation parameter is clockwise as seen in pixel space (Y axis down); clip
	// space here is Y-up, so the standard rotation-matrix formula is applied with the
	// sin terms' signs swapped (equivalent to negating the angle) to reproduce the
	// same clockwise-for-positive-rotation visual result instead of a mirrored one.
	public void RenderRotated(float rotation)
	{
		float num = (float)System.Math.Cos(rotation);
		float num2 = (float)System.Math.Sin(rotation);
		Vector2[] array = new Vector2[4]
		{
			new Vector2(1f, -1f),
			new Vector2(-1f, -1f),
			new Vector2(-1f, 1f),
			new Vector2(1f, 1f)
		};
		for (int i = 0; i < 4; i++)
		{
			float x = array[i].X;
			float y = array[i].Y;
			verts[i].Position.X = x * num + y * num2;
			verts[i].Position.Y = 0f - x * num2 + y * num;
		}
		_graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
	}
}
