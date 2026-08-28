using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.ISHelpers;

internal class LineRenderer
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

	public LineRenderer(GraphicsDevice graphicsDevice, ContentManager contentManager, Rectangle backBufferArea)
	{
		_effect = contentManager.Load<Effect>("Shooter/RenderingHelpers/LineEffect");
		_backBufferArea = backBufferArea;
		_graphics = graphicsDevice;
	}

	public void DrawIndexedShape(VertexPositionColor[] vertices, Vector2 offset, short[] indices)
	{
		VertexPositionColor[] array = new VertexPositionColor[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i].Position = ScreenToShader(vertices[i].Position.X + offset.X, vertices[i].Position.Y + offset.Y);
			array[i].Color = vertices[i].Color;
		}
		if (indices.Length <= 1)
		{
			return;
		}
		foreach (EffectPass pass in _effect.Techniques[0].Passes)
		{
			pass.Apply();
			_graphics.DrawUserIndexedPrimitives(PrimitiveType.LineList, array, 0, array.Length, indices, 0, indices.Length / 2);
		}
	}

	public void DrawShape(VertexPositionColor[] vertices, Vector2 offset)
	{
		if (vertices.Length <= 1)
		{
			return;
		}
		VertexPositionColor[] array = new VertexPositionColor[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i].Position = ScreenToShader(vertices[i].Position.X + offset.X, vertices[i].Position.Y + offset.Y);
			array[i].Color = vertices[i].Color;
		}
		foreach (EffectPass pass in _effect.Techniques[0].Passes)
		{
			pass.Apply();
			_graphics.DrawUserPrimitives(PrimitiveType.LineList, array, 0, array.Length / 2);
		}
	}

	private Vector3 ScreenToShader(float x, float y)
	{
		return new Vector3(x / ((float)_backBufferArea.Width / 2f) - 1f, (y / ((float)_backBufferArea.Height / 2f) - 1f) * -1f, 0f);
	}

	public void DrawShape(VertexPositionColor[] vertices, Vector2 offset, Rectangle backbuffer)
	{
		if (vertices.Length <= 1)
		{
			return;
		}
		VertexPositionColor[] array = new VertexPositionColor[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i].Position = ScreenToShader(vertices[i].Position.X + offset.X, vertices[i].Position.Y + offset.Y, backbuffer);
			array[i].Color = vertices[i].Color;
		}
		foreach (EffectPass pass in _effect.Techniques[0].Passes)
		{
			pass.Apply();
			_graphics.DrawUserPrimitives(PrimitiveType.LineList, array, 0, array.Length / 2);
		}
	}

	private Vector3 ScreenToShader(float x, float y, Rectangle backbuffer)
	{
		return new Vector3(x / ((float)backbuffer.Width / 2f) - 1f, (y / ((float)backbuffer.Height / 2f) - 1f) * -1f, 0f);
	}
}
