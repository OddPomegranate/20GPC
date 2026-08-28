using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.SuperHighway;

internal class LineRender
{
	private Effect _effect;

	private GraphicsDevice _graphics;

	private Rectangle _backBufferArea;

	public GraphicsDevice GraphicsDevice
	{
		get
		{
			return _graphics;
		}
		set
		{
			_graphics = value;
		}
	}

	public Rectangle BackBufferSize
	{
		get
		{
			return _backBufferArea;
		}
		set
		{
			_backBufferArea.X = 0;
			_backBufferArea.Y = 0;
			_backBufferArea.Width = value.Width;
			_backBufferArea.Height = value.Height;
		}
	}

	public void Load(ContentManager contentManager)
	{
		_effect = contentManager.Load<Effect>("LunarLander\\Effect\\LineEffect");
	}

	public void DrawIndexedShape(VertexPositionColor[] vertices, short[] indices)
	{
		VertexPositionColor[] array = new VertexPositionColor[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i].Position = ScreenToShader(vertices[i].Position.X, vertices[i].Position.Y);
			array[i].Color = vertices[i].Color;
		}
		foreach (EffectPass pass in _effect.Techniques[0].Passes)
		{
			pass.Apply();
			_graphics.DrawUserIndexedPrimitives(PrimitiveType.LineList, array, 0, array.Length, indices, 0, indices.Length / 2);
		}
	}

	private Vector3 ScreenToShader(float x, float y)
	{
		return new Vector3(x / ((float)_backBufferArea.Width / 2f) - 1f, (y / ((float)_backBufferArea.Height / 2f) - 1f) * -1f, 0f);
	}
}
