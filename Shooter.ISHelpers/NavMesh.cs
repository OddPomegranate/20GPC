using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.ISHelpers;

internal class NavMesh
{
	private LineMesh _lineMesh;

	private int _numberOfNodes;

	private float[,] dist;

	private int[] visited;

	private float[] d;

	private int n;

	private int e;

	private GraphicsDevice _graphicsDevice;

	private ContentManager _contentManager;

	public LineMesh LineMesh
	{
		get
		{
			return _lineMesh;
		}
		set
		{
			_lineMesh = value;
		}
	}

	public NavMesh(int numberOfNodes, GraphicsDevice graphicsDevice, ContentManager contentManager)
	{
		_graphicsDevice = graphicsDevice;
		_contentManager = contentManager;
		_lineMesh = new LineMesh(graphicsDevice, contentManager, 150);
		_numberOfNodes = numberOfNodes;
		dist = new float[numberOfNodes, numberOfNodes];
		d = new float[numberOfNodes];
		visited = new int[numberOfNodes];
		n = numberOfNodes;
		e = numberOfNodes;
	}

	public NavMesh(int numberOfNodes, Stream fileStream, GraphicsDevice graphicsDevice, ContentManager contentManager)
	{
		_graphicsDevice = graphicsDevice;
		_contentManager = contentManager;
		_lineMesh = new LineMesh(_graphicsDevice, _contentManager, numberOfNodes);
		_numberOfNodes = numberOfNodes;
		dist = new float[numberOfNodes, numberOfNodes];
		d = new float[numberOfNodes];
		visited = new int[numberOfNodes];
		n = numberOfNodes;
		e = numberOfNodes;
		LoadNavMesh(fileStream);
	}

	public void ProcessMesh()
	{
		for (int i = 0; i < _numberOfNodes; i++)
		{
			for (int j = 0; j < _numberOfNodes; j++)
			{
				dist[i, j] = 10000000f;
			}
		}
		for (int k = 0; k < _lineMesh.MeshNodes.Count; k++)
		{
			for (int l = 0; l < _lineMesh.MeshNodes[k]._neighbours.Count; l++)
			{
				dist[k, _lineMesh.MeshNodes[k]._neighbours[l]._neighbourID] = _lineMesh.MeshNodes[k]._neighbours[l]._length;
			}
		}
	}

	public List<Vector2> GetPath(int startID, int endID)
	{
		MeshNode? nodeFromID = _lineMesh.GetNodeFromID(startID);
		MeshNode? nodeFromID2 = _lineMesh.GetNodeFromID(endID);
		if (nodeFromID.HasValue && nodeFromID2.HasValue)
		{
			MeshNode value = nodeFromID2.Value;
			MeshNode value2 = nodeFromID.Value;
			for (int i = 0; i < n; i++)
			{
				d[i] = 1000000f;
				visited[i] = 0;
			}
			d[value._id] = 0f;
			for (int j = 0; j < n; j++)
			{
				int num = -1;
				for (int i = 0; i < n; i++)
				{
					if (visited[i] == 0 && (num == -1 || d[i] < d[num]))
					{
						num = i;
					}
				}
				visited[num] = 1;
				for (int i = 0; i < n; i++)
				{
					if (dist[num, i] != 0f && d[num] + dist[num, i] < d[i])
					{
						d[i] = d[num] + dist[num, i];
					}
				}
			}
			List<Vector2> list = new List<Vector2>();
			_ = d[value2._id];
			int num2 = value2._id;
			list.Add(_lineMesh.MeshNodes[num2]._position);
			do
			{
				for (int k = 0; k < _lineMesh.MeshNodes[num2]._neighbours.Count; k++)
				{
					if (d[num2] - (float)_lineMesh.MeshNodes[num2]._neighbours[k]._length == d[_lineMesh.MeshNodes[num2]._neighbours[k]._neighbourID])
					{
						list.Add(_lineMesh.MeshNodes[_lineMesh.MeshNodes[num2]._neighbours[k]._neighbourID]._position);
						num2 = _lineMesh.MeshNodes[num2]._neighbours[k]._neighbourID;
						break;
					}
				}
			}
			while (num2 != value._id);
			return list;
		}
		return new List<Vector2>();
	}

	public void SaveNavMesh(Stream fileStream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		binaryWriter.Write(_lineMesh.MeshNodes.Count);
		for (int i = 0; i < _lineMesh.MeshNodes.Count; i++)
		{
			binaryWriter.Write(_lineMesh.MeshNodes[i]._id);
			binaryWriter.Write(_lineMesh.MeshNodes[i]._position.X);
			binaryWriter.Write(_lineMesh.MeshNodes[i]._position.Y);
			binaryWriter.Write(_lineMesh.MeshNodes[i]._neighbours.Count);
			for (int j = 0; j < _lineMesh.MeshNodes[i]._neighbours.Count; j++)
			{
				binaryWriter.Write(_lineMesh.MeshNodes[i]._neighbours[j]._length);
				binaryWriter.Write(_lineMesh.MeshNodes[i]._neighbours[j]._neighbourID);
			}
		}
		binaryWriter.Write(_lineMesh.SpecialNodes.Count);
		for (int k = 0; k < _lineMesh.SpecialNodes.Count; k++)
		{
			binaryWriter.Write(_lineMesh.SpecialNodes[k]);
		}
		binaryWriter.Close();
	}

	public void LoadNavMesh(Stream fileStream)
	{
		_lineMesh = new LineMesh(_graphicsDevice, _contentManager, _numberOfNodes);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		BinaryReader binaryReader = new BinaryReader(fileStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = binaryReader.ReadInt32();
			float x = binaryReader.ReadSingle();
			float y = binaryReader.ReadSingle();
			_lineMesh.AddNode(num2, new Vector2(x, y));
			int num3 = binaryReader.ReadInt32();
			for (int j = 0; j < num3; j++)
			{
				list3.Add(binaryReader.ReadInt32());
				list.Add(num2);
				list2.Add(binaryReader.ReadInt32());
			}
		}
		int num4 = binaryReader.ReadInt32();
		for (int k = 0; k < num4; k++)
		{
			_lineMesh.SpecialNodes.Add(binaryReader.ReadInt32());
			MeshNode value = _lineMesh.MeshNodes[_lineMesh.SpecialNodes[k]];
			value._colour = Color.Blue;
			_lineMesh.MeshNodes[_lineMesh.SpecialNodes[k]] = value;
		}
		binaryReader.Close();
		for (int l = 0; l < list.Count; l++)
		{
			_lineMesh.AddLink(list[l], list2[l], list3[l]);
		}
		ProcessMesh();
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset, Rectangle rect)
	{
		_lineMesh.DrawMesh(spriteBatch, offset, rect);
	}
}
