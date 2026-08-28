using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal abstract class TexturedPolygon : IDisposable
{
	protected VertexPositionTexture[] _vertices;

	protected Texture2D _texture;

	private int _primitiveCount;

	private BasicEffect _basicEffect;

	private RasterizerState _rasterizerState;

	public TexturedPolygon(IList<Vector2> polyVerts, float width, float height, GraphicsDevice gd, Matrix projection)
	{
		_primitiveCount = polyVerts.Count - 2;
		_vertices = new VertexPositionTexture[_primitiveCount * 3];
		for (int i = 0; i != _primitiveCount; i++)
		{
			ref VertexPositionTexture reference = ref _vertices[i * 3];
			reference = new VertexPositionTexture(new Vector3(polyVerts[0].X, polyVerts[0].Y, 0f), new Vector2(polyVerts[0].X / width, polyVerts[0].Y / height));
			ref VertexPositionTexture reference2 = ref _vertices[i * 3 + 1];
			reference2 = new VertexPositionTexture(new Vector3(polyVerts[i + 1].X, polyVerts[i + 1].Y, 0f), new Vector2(polyVerts[i + 1].X / width, polyVerts[i + 1].Y / height));
			ref VertexPositionTexture reference3 = ref _vertices[i * 3 + 2];
			reference3 = new VertexPositionTexture(new Vector3(polyVerts[i + 2].X, polyVerts[i + 2].Y, 0f), new Vector2(polyVerts[i + 2].X / width, polyVerts[i + 2].Y / height));
		}
		_basicEffect = new BasicEffect(gd);
		_basicEffect.Projection = projection;
		_basicEffect.View = Matrix.Identity;
		_basicEffect.TextureEnabled = true;
		_rasterizerState = new RasterizerState();
		_rasterizerState.CullMode = CullMode.None;
		gd.RasterizerState = _rasterizerState;
	}

	protected void SetProjection(Matrix projection)
	{
		_basicEffect.Projection = projection;
	}

	protected void SetView(Matrix view)
	{
		_basicEffect.View = view;
	}

	public virtual void Draw(GraphicsDevice gd, Color color)
	{
		if (_primitiveCount != 0)
		{
			gd.RasterizerState = _rasterizerState;
			_basicEffect.CurrentTechnique.Passes[0].Apply();
			_basicEffect.DiffuseColor = color.ToVector3();
			gd.SamplerStates[0] = SamplerState.AnisotropicClamp;
			gd.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _primitiveCount);
		}
	}

	protected virtual void LoadContent()
	{
		_basicEffect.Texture = _texture;
	}

	public virtual void Dispose()
	{
		if (_basicEffect != null)
		{
			_basicEffect.Dispose();
		}
	}
}
