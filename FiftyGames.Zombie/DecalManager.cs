using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class DecalManager
{
	private VertexPositionTexture[] verts;

	private short[] _indices = new short[6] { 0, 1, 2, 2, 3, 0 };

	private GraphicsDevice _graphicsDevice;

	private VertexBuffer _vb;

	private IndexBuffer _ib;

	private Effect _effect;

	private int _backBufferWidth;

	private int _backBufferHeight;

	private Vector2 _scale;

	private Queue<Matrix> _decalQueue;

	private DynamicVertexBuffer dvb;

	private Texture2D _texture;

	private bool _needsUpdating;

	private static VertexDeclaration _instanceVertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0), new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1), new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2), new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3));

	public int NumberOfDecals => _decalQueue.Count - 1;

	public DecalManager(GraphicsDevice graphicsDevice, ContentManager content, Rectangle backBufferRect, Rectangle sourceRect, Texture2D texture)
	{
		_graphicsDevice = graphicsDevice;
		_backBufferWidth = backBufferRect.Width;
		_backBufferHeight = backBufferRect.Height;
		_texture = texture;
		_effect = content.Load<Effect>("Zombie/DecalManagerEffect");
		verts = new VertexPositionTexture[4]
		{
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(1f, 1f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 1f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 0f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(1f, 0f))
		};
		_scale = new Vector2((float)sourceRect.Width / (float)backBufferRect.Width, (float)sourceRect.Height / (float)backBufferRect.Height);
		float num = 1f;
		float num2 = 1f;
		verts[0].Position.X = num;
		verts[0].Position.Y = num2 * -1f;
		verts[1].Position.X = num * -1f;
		verts[1].Position.Y = num2 * -1f;
		verts[2].Position.X = num * -1f;
		verts[2].Position.Y = num2;
		verts[3].Position.X = num;
		verts[3].Position.Y = num2;
		_vb = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.None);
		_vb.SetData(verts);
		_ib = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
		_ib.SetData(_indices);
		_decalQueue = new Queue<Matrix>();
		dvb = new DynamicVertexBuffer(_graphicsDevice, _instanceVertexDeclaration, 2000, BufferUsage.WriteOnly);
		_decalQueue.Enqueue(Matrix.CreateScale(new Vector3(_scale, 1f)) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(ScreenToShader(new Vector2(2000f, 2000f), _backBufferWidth, _backBufferHeight), 0f)));
		dvb.SetData(_decalQueue.ToArray(), 0, 1, SetDataOptions.Discard);
	}

	public void AddNewDecal(Vector2 position, Vector2 scale, float rotation)
	{
		_decalQueue.Enqueue(Matrix.CreateRotationZ(rotation) * Matrix.CreateScale(new Vector3(_scale * scale, 1f)) * Matrix.CreateTranslation(new Vector3(ScreenToShader(position, _backBufferWidth, _backBufferHeight), 0f)));
		if (_decalQueue.Count > 2000)
		{
			RemoveOldestDecal();
		}
		_needsUpdating = true;
	}

	public void RemoveAllDecals()
	{
		_decalQueue.Clear();
		_decalQueue.Enqueue(Matrix.CreateScale(new Vector3(_scale, 1f)) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(ScreenToShader(new Vector2(2000f, 2000f), _backBufferWidth, _backBufferHeight), 0f)));
	}

	public void RemoveDecalsInArea(Rectangle rect)
	{
		List<Matrix> list = _decalQueue.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Vector2 vector = new Vector2(list[i].Translation.X, list[i].Translation.Y);
			vector = ShaderToScreen(new Vector2(vector.X, vector.Y), _backBufferWidth, _backBufferHeight);
			if (rect.Contains((int)vector.X, (int)vector.Y))
			{
				list.RemoveAt(i);
				i--;
			}
		}
		_decalQueue = new Queue<Matrix>(list);
		_needsUpdating = true;
		ApplyChanges();
	}

	public void RemoveOldestDecal()
	{
		_decalQueue.Dequeue();
		_needsUpdating = true;
	}

	public void ApplyChanges()
	{
		if (_needsUpdating)
		{
			dvb.SetData(_decalQueue.ToArray(), 0, _decalQueue.Count, SetDataOptions.Discard);
			_needsUpdating = false;
		}
	}

	public void Render(Vector2 offset)
	{
		ScreenToShader(new Vector2(630f, 350f), 1280, 720);
		_graphicsDevice.SetVertexBuffers(new VertexBufferBinding(_vb, 0, 0), new VertexBufferBinding(dvb, 0, 1));
		_graphicsDevice.Indices = _ib;
		_effect.Parameters["Texture"].SetValue(_texture);
		_effect.Parameters["Alpha"].SetValue(0.4f);
		_effect.Parameters["Offset"].SetValue(ScreenToShader(offset, _backBufferWidth, _backBufferHeight));
		_effect.CurrentTechnique.Passes[0].Apply();
		_graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, _decalQueue.Count);
	}

	private static Vector2 ScreenToShader(Vector2 position, int backBufferWidth, int backBufferHeight)
	{
		return new Vector2(position.X / (float)backBufferWidth * 2f - 1f, -1f * (position.Y / (float)backBufferHeight * 2f - 1f));
	}

	private static Vector2 ShaderToScreen(Vector2 position, int backBufferWidth, int backBufferHeight)
	{
		float x = (position.X + 1f) * (float)(backBufferWidth / 2);
		float y = -1f * ((position.Y - 1f) / 2f) * (float)backBufferHeight;
		return new Vector2(x, y);
	}
}
