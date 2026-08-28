using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MicroMachinesGame.ISHelpers;

internal class LineMesh
{
	private int _meshSize;

	private List<MeshNode> _nodes;

	private List<int> _specialNodes;

	private List<Line> _lines;

	private int[,] _nodeToLinesTable;

	private GraphicsDevice _graphicsdevice;

	private ContentManager _contentManager;

	private LineRenderer _lineRenderer;

	private List<VertexPositionColor> _lineVerts;

	private Texture2D _nodeSprite;

	private SpriteFont _font;

	private bool _drawDebugInfo;

	public List<MeshNode> MeshNodes
	{
		get
		{
			return _nodes;
		}
		set
		{
			_nodes = value;
		}
	}

	public List<int> SpecialNodes => _specialNodes;

	public List<Line> Lines
	{
		get
		{
			return _lines;
		}
		set
		{
			_lines = value;
		}
	}

	public bool DrawDebugInfo
	{
		get
		{
			return _drawDebugInfo;
		}
		set
		{
			_drawDebugInfo = value;
		}
	}

	public LineRenderer LineRenderer
	{
		get
		{
			return _lineRenderer;
		}
		set
		{
			_lineRenderer = value;
		}
	}

	public LineMesh(GraphicsDevice graphicsDevice, ContentManager contentManager, int meshSize)
	{
		_nodes = new List<MeshNode>();
		_lines = new List<Line>();
		_specialNodes = new List<int>();
		_graphicsdevice = graphicsDevice;
		_contentManager = contentManager;
		_lineRenderer = new LineRenderer(_graphicsdevice, _contentManager, new Rectangle(0, 0, 1280, 720));
		_lineVerts = new List<VertexPositionColor>();
		_nodeSprite = _contentManager.Load<Texture2D>("MicroMachines/RenderingHelpers/Node");
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_drawDebugInfo = false;
		_nodeToLinesTable = new int[meshSize, meshSize];
		for (int i = 0; i < meshSize; i++)
		{
			for (int j = 0; j < meshSize; j++)
			{
				if (i == j)
				{
					_nodeToLinesTable[i, j] = -2;
				}
				else
				{
					_nodeToLinesTable[i, j] = -1;
				}
			}
		}
		_meshSize = meshSize;
	}

	public void AddNode(Vector2 position)
	{
		_nodes.Add(new MeshNode(_nodes.Count, position));
	}

	public void AddNode(int id, Vector2 position)
	{
		_nodes.Add(new MeshNode(id, position));
	}

	public void AddLink(int startID, int endID, int length)
	{
		if (_nodeToLinesTable[startID, endID] == -1)
		{
			if (length == 0)
			{
				length = (int)Vector2.Distance(_nodes[startID]._position, _nodes[endID]._position);
			}
			_nodes[startID]._neighbours.Add(new NodeLink(startID, endID, length));
			_nodes[endID]._neighbours.Add(new NodeLink(endID, startID, length));
			_nodeToLinesTable[startID, endID] = _lines.Count;
			_nodeToLinesTable[endID, startID] = _lines.Count;
			Line line = new Line();
			line.Start = _nodes[startID]._position;
			line.End = _nodes[endID]._position;
			line.Normal = Vector2.One;
			line.Id = 0;
			_lines.Add(line);
		}
	}

	public void RemoveLink(int startID, int endID)
	{
		for (int i = 0; i < _nodes[startID]._neighbours.Count; i++)
		{
			if (_nodes[startID]._neighbours[i]._neighbourID == endID)
			{
				_nodes[startID]._neighbours.RemoveAt(i);
				break;
			}
		}
		for (int j = 0; j < _nodes[endID]._neighbours.Count; j++)
		{
			if (_nodes[endID]._neighbours[j]._neighbourID == startID)
			{
				_nodes[endID]._neighbours.RemoveAt(j);
				break;
			}
		}
		int num = _nodeToLinesTable[startID, endID];
		_nodeToLinesTable[startID, endID] = -1;
		_nodeToLinesTable[endID, startID] = -1;
		_lines.RemoveAt(num);
		for (int k = 0; k < _meshSize; k++)
		{
			for (int l = 0; l < _meshSize; l++)
			{
				if (_nodeToLinesTable[k, l] >= num)
				{
					_nodeToLinesTable[k, l]--;
				}
			}
		}
	}

	public void RemoveNode(int nodeID)
	{
		List<NodeLink> list = new List<NodeLink>();
		for (int i = 0; i < _nodes[nodeID]._neighbours.Count; i++)
		{
			list.Add(_nodes[nodeID]._neighbours[i]);
		}
		for (int j = 0; j < list.Count; j++)
		{
			RemoveLink(nodeID, list[j]._neighbourID);
		}
		_nodes[nodeID]._neighbours.Clear();
		_nodes.RemoveAt(nodeID);
		for (int k = 0; k < _nodes.Count; k++)
		{
			if (_nodes[k]._id >= nodeID)
			{
				MeshNode value = _nodes[k];
				value._id--;
				_nodes[k] = value;
			}
		}
		for (int l = 0; l < _meshSize; l++)
		{
			for (int m = 0; m < _meshSize; m++)
			{
				if (l == m)
				{
					_nodeToLinesTable[l, m] = -2;
				}
				else
				{
					_nodeToLinesTable[l, m] = -1;
				}
			}
		}
		_lines.Clear();
		for (int n = 0; n < _nodes.Count; n++)
		{
			for (int num = 0; num < _nodes[n]._neighbours.Count; num++)
			{
				NodeLink value2 = _nodes[n]._neighbours[num];
				if (value2._ownerID >= nodeID)
				{
					value2._ownerID--;
				}
				if (value2._neighbourID >= nodeID)
				{
					value2._neighbourID--;
				}
				_nodes[n]._neighbours[num] = value2;
				_nodeToLinesTable[n, _nodes[n]._neighbours[num]._neighbourID] = _lines.Count;
				Line line = new Line();
				line.Start = _nodes[n]._position;
				line.End = _nodes[_nodes[n]._neighbours[num]._neighbourID]._position;
				line.Normal = Vector2.One;
				line.Id = 0;
				_lines.Add(line);
			}
		}
	}

	public void AddOffsetToAllNodes(Vector2 offset)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			MeshNode value = _nodes[i];
			value._position += offset;
			_nodes[i] = value;
		}
	}

	public int GetNearestNodeID(Vector2 position, float radius)
	{
		int result = 0;
		float num = 1000000f;
		for (int i = 0; i < _nodes.Count; i++)
		{
			float num2 = Vector2.Distance(_nodes[i]._position, position);
			if (num2 < num)
			{
				result = i;
				num = num2;
			}
		}
		if (num <= radius)
		{
			return result;
		}
		return -1;
	}

	public MeshNode? GetNearestNode(Vector2 position, float radius)
	{
		return GetNodeFromID(GetNearestNodeID(position, radius));
	}

	public float GetDistanceFromLine(Vector2 position, int startNodeID, int endNodeID)
	{
		Vector3 vector = new Vector3(_nodes[startNodeID]._position, 0f);
		Vector3 vector2 = new Vector3(_nodes[endNodeID]._position, 0f);
		Vector3 vector3 = new Vector3(position, 0f);
		Vector3 vector4 = vector2 - vector;
		float num = vector4.Length();
		if (vector4 == Vector3.Zero)
		{
			return (vector3 - vector).Length();
		}
		vector4.Normalize();
		float num2 = Vector3.Dot(vector3 - vector, vector4);
		if (num2 < 0f)
		{
			return (vector3 - vector).Length();
		}
		if (num2 > num)
		{
			return (vector3 - vector2).Length();
		}
		return (vector3 - (vector + vector4 * num2)).Length();
	}

	public float GetDistanceFromLine(Vector2 position, NodeLink linkLine)
	{
		return GetDistanceFromLine(position, linkLine._ownerID, linkLine._neighbourID);
	}

	public MeshNode? GetNodeFromID(int id)
	{
		if (id >= 0 && id < _nodes.Count)
		{
			return _nodes[id];
		}
		return null;
	}

	public void MoveNode(int id, Vector2 newPosition)
	{
		MeshNode value = _nodes[id];
		value._position = newPosition;
		_nodes[id] = value;
		for (int i = 0; i < _nodes[id]._neighbours.Count; i++)
		{
			NodeLink value2 = _nodes[id]._neighbours[i];
			int length = (value2._length = (int)Vector2.Distance(_nodes[id]._position, _nodes[value2._neighbourID]._position));
			_nodes[id]._neighbours[i] = value2;
			MeshNode value3 = _nodes[_nodes[id]._neighbours[i]._neighbourID];
			for (int j = 0; j < value3._neighbours.Count; j++)
			{
				if (value3._neighbours[j]._neighbourID == id)
				{
					NodeLink value4 = value3._neighbours[j];
					value4._length = length;
					value3._neighbours[j] = value4;
					break;
				}
			}
			_nodes[_nodes[id]._neighbours[i]._neighbourID] = value3;
		}
		_lines.Clear();
		for (int k = 0; k < _nodes.Count; k++)
		{
			for (int l = 0; l < _nodes[k]._neighbours.Count; l++)
			{
				_nodeToLinesTable[k, _nodes[k]._neighbours[l]._neighbourID] = _lines.Count;
				Line line = new Line();
				line.Start = _nodes[k]._position;
				line.End = _nodes[_nodes[k]._neighbours[l]._neighbourID]._position;
				line.Normal = Vector2.One;
				line.Id = 0;
				_lines.Add(line);
			}
		}
	}

	public void ChangeNodeColour(int id, Color colour)
	{
		MeshNode value = _nodes[id];
		value._colour = colour;
		_nodes[id] = value;
	}

	public void DrawMesh(SpriteBatch spriteBatch, Vector2 offset, Rectangle rect)
	{
		_lineVerts.Clear();
		for (int i = 0; i < _lines.Count; i++)
		{
			_lineVerts.Add(new VertexPositionColor(new Vector3(_lines[i].Start, 0f), Color.White));
			_lineVerts.Add(new VertexPositionColor(new Vector3(_lines[i].End, 0f), Color.White));
		}
		_lineRenderer.DrawShape(_lineVerts.ToArray(), offset, rect);
		spriteBatch.Begin();
		for (int j = 0; j < _nodes.Count; j++)
		{
			Vector2 position = _nodes[j]._position - new Vector2(4f, 4f) + offset;
			position.X = 1280f / (float)rect.Width * position.X;
			position.Y = 720f / (float)rect.Height * position.Y;
			spriteBatch.Draw(_nodeSprite, position, _nodes[j]._colour);
			if (_drawDebugInfo)
			{
				spriteBatch.DrawString(_font, j.ToString(), position, Color.CornflowerBlue);
			}
		}
		spriteBatch.End();
	}
}
