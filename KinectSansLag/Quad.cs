using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectSansLag;

internal class Quad
{
	private VertexPositionTexture[] _vertices;

	private uint[] _indices;

	private uint _width;

	private uint _height;

	public VertexPositionTexture[] Vertices => _vertices;

	public uint[] Indices => _indices;

	public uint Width => _width;

	public uint Height => _height;

	public Quad(uint width, uint height)
	{
		_width = width;
		_height = height;
		_vertices = new VertexPositionTexture[4];
		_indices = new uint[6];
		ref VertexPositionTexture reference = ref _vertices[0];
		reference = new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 0f));
		ref VertexPositionTexture reference2 = ref _vertices[1];
		reference2 = new VertexPositionTexture(new Vector3(width, 0f, 0f), new Vector2(1f, 0f));
		ref VertexPositionTexture reference3 = ref _vertices[2];
		reference3 = new VertexPositionTexture(new Vector3(0f, height, 0f), new Vector2(0f, 1f));
		ref VertexPositionTexture reference4 = ref _vertices[3];
		reference4 = new VertexPositionTexture(new Vector3(width, height, 0f), new Vector2(1f, 1f));
		_indices[0] = 0u;
		_indices[1] = 1u;
		_indices[2] = 3u;
		_indices[3] = 0u;
		_indices[4] = 3u;
		_indices[5] = 2u;
	}
}
