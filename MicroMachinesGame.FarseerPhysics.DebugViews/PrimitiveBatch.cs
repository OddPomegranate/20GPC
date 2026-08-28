using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroMachinesGame.FarseerPhysics.DebugViews;

public class PrimitiveBatch : IDisposable
{
	private const int DefaultBufferSize = 500;

	private BasicEffect _basicEffect;

	private GraphicsDevice _device;

	private bool _hasBegun;

	private bool _isDisposed;

	private VertexPositionColor[] _lineVertices;

	private int _lineVertsCount;

	private VertexPositionColor[] _triangleVertices;

	private int _triangleVertsCount;

	public PrimitiveBatch(GraphicsDevice graphicsDevice)
		: this(graphicsDevice, 500)
	{
	}

	public PrimitiveBatch(GraphicsDevice graphicsDevice, int bufferSize)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice");
		}
		_device = graphicsDevice;
		_triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];
		_lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];
		_basicEffect = new BasicEffect(graphicsDevice);
		_basicEffect.VertexColorEnabled = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void SetProjection(ref Matrix projection)
	{
		_basicEffect.Projection = projection;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !_isDisposed)
		{
			if (_basicEffect != null)
			{
				_basicEffect.Dispose();
			}
			_isDisposed = true;
		}
	}

	public void Begin(ref Matrix projection, ref Matrix view)
	{
		if (_hasBegun)
		{
			throw new InvalidOperationException("End must be called before Begin can be called again.");
		}
		_basicEffect.Projection = projection;
		_basicEffect.View = view;
		_basicEffect.CurrentTechnique.Passes[0].Apply();
		_hasBegun = true;
	}

	public bool IsReady()
	{
		return _hasBegun;
	}

	public void AddVertex(Vector2 vertex, Color color, PrimitiveType primitiveType)
	{
		if (!_hasBegun)
		{
			throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
		}
		switch (primitiveType)
		{
		case PrimitiveType.TriangleStrip:
		case PrimitiveType.LineStrip:
			throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
		case PrimitiveType.TriangleList:
			if (_triangleVertsCount >= _triangleVertices.Length)
			{
				FlushTriangles();
			}
			_triangleVertices[_triangleVertsCount].Position = new Vector3(vertex, -0.1f);
			_triangleVertices[_triangleVertsCount].Color = color;
			_triangleVertsCount++;
			break;
		}
		if (primitiveType == PrimitiveType.LineList)
		{
			if (_lineVertsCount >= _lineVertices.Length)
			{
				FlushLines();
			}
			_lineVertices[_lineVertsCount].Position = new Vector3(vertex, 0f);
			_lineVertices[_lineVertsCount].Color = color;
			_lineVertsCount++;
		}
	}

	public void End()
	{
		if (!_hasBegun)
		{
			throw new InvalidOperationException("Begin must be called before End can be called.");
		}
		FlushTriangles();
		FlushLines();
		_hasBegun = false;
	}

	private void FlushTriangles()
	{
		if (!_hasBegun)
		{
			throw new InvalidOperationException("Begin must be called before Flush can be called.");
		}
		if (_triangleVertsCount >= 3)
		{
			int num = _triangleVertsCount / 3;
			_device.SamplerStates[0] = SamplerState.AnisotropicClamp;
			_device.DrawUserPrimitives(PrimitiveType.TriangleList, _triangleVertices, 0, num);
			_triangleVertsCount -= num * 3;
		}
	}

	private void FlushLines()
	{
		if (!_hasBegun)
		{
			throw new InvalidOperationException("Begin must be called before Flush can be called.");
		}
		if (_lineVertsCount >= 2)
		{
			int num = _lineVertsCount / 2;
			_device.SamplerStates[0] = SamplerState.AnisotropicClamp;
			_device.DrawUserPrimitives(PrimitiveType.LineList, _lineVertices, 0, num);
			_lineVertsCount -= num * 2;
		}
	}
}
