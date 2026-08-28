using System.Collections.Generic;
using System.IO;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shooter.ISHelpers;

internal class WallMeshEditor
{
	private NavMesh _currentWallMesh;

	private List<VertexPositionColor> _wallVerts;

	private int _currentSelectedNodeID;

	private bool _wallMeshChanged;

	private Stack<MeshEditorHistory> _history;

	private int _previousLinkNodeID;

	private bool _isCreatingSelectionBox;

	private Vector2 _selectionBoxStart;

	private Vector2 _selectionBoxEnd;

	private VertexPositionColor[] _selectionBoxVerts;

	private List<int> _selectedNodes;

	private Vector2 _startMouseDrag;

	private List<Body> _wallBodies;

	private World _world;

	private GraphicsDevice _graphicsDevice;

	private ContentManager _contentManager;

	private List<Body> _physWalls;

	public WallMeshEditor(GraphicsDevice graphicsDevice, ContentManager contentManager, World world)
	{
		_graphicsDevice = graphicsDevice;
		_contentManager = contentManager;
		_world = world;
		SetNavMesh(new NavMesh(200, _graphicsDevice, _contentManager));
		_physWalls = new List<Body>();
	}

	public void SetNavMesh(NavMesh newNavMesh)
	{
		_currentWallMesh = newNavMesh;
		_currentSelectedNodeID = -1;
		_wallVerts = new List<VertexPositionColor>();
		_history = new Stack<MeshEditorHistory>();
		_previousLinkNodeID = -1;
		_isCreatingSelectionBox = false;
		_selectionBoxStart = Vector2.Zero;
		_selectionBoxEnd = Vector2.Zero;
		_selectionBoxVerts = new VertexPositionColor[8];
		_selectedNodes = new List<int>();
		_startMouseDrag = Vector2.Zero;
		_wallBodies = new List<Body>();
		_currentWallMesh.LoadNavMesh(new StreamReader("Content/Shooter/Data/walls.wls").BaseStream);
		for (int i = 0; i < _currentWallMesh.LineMesh.Lines.Count; i++)
		{
			Body body = BodyFactory.CreateEdge(_world, ConvertUnits.ToSimUnits(_currentWallMesh.LineMesh.Lines[i].Start), ConvertUnits.ToSimUnits(_currentWallMesh.LineMesh.Lines[i].End));
			body.BodyType = BodyType.Static;
			body.Mass = 1000f;
			body.UserData = _currentWallMesh.LineMesh.Lines[i];
			body.Friction = 0f;
			_wallBodies.Add(body);
		}
		List<Line> list = new List<Line>();
		Line line = new Line();
		line.Start = new Vector2(0f, 0f);
		line.End = new Vector2(1920f, 0f);
		list.Add(line);
		Line line2 = new Line();
		line2.Start = new Vector2(0f, 0f);
		line2.End = new Vector2(0f, 1200f);
		list.Add(line2);
		Line line3 = new Line();
		line3.Start = new Vector2(1920f, 0f);
		line3.End = new Vector2(1920f, 1200f);
		list.Add(line3);
		Line line4 = new Line();
		line4.Start = new Vector2(0f, 1200f);
		line4.End = new Vector2(1920f, 1200f);
		list.Add(line4);
		for (int j = 0; j < list.Count; j++)
		{
			Body body2 = BodyFactory.CreateEdge(_world, ConvertUnits.ToSimUnits(list[j].Start), ConvertUnits.ToSimUnits(list[j].End));
			body2.BodyType = BodyType.Static;
			body2.Mass = 1000f;
			body2.UserData = 999;
			body2.Friction = 0f;
			_wallBodies.Add(body2);
		}
	}

	private void RefreshWallMesh()
	{
		for (int i = 0; i < _wallBodies.Count; i++)
		{
			_wallBodies[i].Dispose();
		}
		_wallBodies.Clear();
		for (int j = 0; j < _currentWallMesh.LineMesh.Lines.Count; j++)
		{
			Body body = BodyFactory.CreateEdge(_world, _currentWallMesh.LineMesh.Lines[j].Start, _currentWallMesh.LineMesh.Lines[j].End);
			body.BodyType = BodyType.Static;
			body.Mass = 1000f;
			_wallBodies.Add(body);
		}
	}

	public void Update(Vector2 mouseCoord, World physWorld)
	{
		if (_currentWallMesh == null)
		{
			return;
		}
		if (InputState.LeftButtonClicked())
		{
			int nearestNodeID = _currentWallMesh.LineMesh.GetNearestNodeID(mouseCoord, 15f);
			if (_selectedNodes.Count > 0)
			{
				if (nearestNodeID != -1)
				{
					_currentSelectedNodeID = nearestNodeID;
					_startMouseDrag = mouseCoord;
				}
				else
				{
					ResetSelection();
					_selectionBoxStart = mouseCoord;
				}
			}
			else if (nearestNodeID == -1 && !InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl))
			{
				_currentWallMesh.LineMesh.AddNode(mouseCoord);
				NodeAddedHistoryData nodeAddedHistoryData = default(NodeAddedHistoryData);
				nodeAddedHistoryData.NodeID = _currentWallMesh.LineMesh.MeshNodes.Count - 1;
				MeshEditorHistory item = default(MeshEditorHistory);
				item.ActionType = HistoryActionType.NodeAdded;
				item.ActionData = nodeAddedHistoryData;
				_history.Push(item);
				_currentSelectedNodeID = -1;
				_wallMeshChanged = true;
			}
			else
			{
				_selectionBoxStart = mouseCoord;
				_currentSelectedNodeID = nearestNodeID;
			}
		}
		if (InputState.LeftButtonHeld())
		{
			if (_currentSelectedNodeID != -1)
			{
				if (_selectedNodes.Count > 0)
				{
					for (int i = 0; i < _selectedNodes.Count; i++)
					{
						_currentWallMesh.LineMesh.MoveNode(_selectedNodes[i], _currentWallMesh.LineMesh.MeshNodes[_selectedNodes[i]]._position - (_startMouseDrag - mouseCoord));
					}
					_startMouseDrag = mouseCoord;
					_wallMeshChanged = true;
				}
				else
				{
					Vector2 position = _currentWallMesh.LineMesh.MeshNodes[_currentSelectedNodeID]._position;
					_currentWallMesh.LineMesh.MoveNode(_currentSelectedNodeID, mouseCoord);
					NodeMovedHistoryData nodeMovedHistoryData = default(NodeMovedHistoryData);
					nodeMovedHistoryData.NodeID = _currentSelectedNodeID;
					nodeMovedHistoryData.OldPos = position;
					MeshEditorHistory item2 = default(MeshEditorHistory);
					item2.ActionType = HistoryActionType.NodeMoved;
					item2.ActionData = nodeMovedHistoryData;
					_history.Push(item2);
					_wallMeshChanged = true;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl))
			{
				_isCreatingSelectionBox = true;
				_selectionBoxEnd = mouseCoord;
				ref VertexPositionColor reference = ref _selectionBoxVerts[0];
				reference = new VertexPositionColor(new Vector3(_selectionBoxStart, 0f), Color.White);
				ref VertexPositionColor reference2 = ref _selectionBoxVerts[1];
				reference2 = new VertexPositionColor(new Vector3(_selectionBoxEnd.X, _selectionBoxStart.Y, 0f), Color.White);
				ref VertexPositionColor reference3 = ref _selectionBoxVerts[2];
				reference3 = new VertexPositionColor(new Vector3(_selectionBoxStart, 0f), Color.White);
				ref VertexPositionColor reference4 = ref _selectionBoxVerts[3];
				reference4 = new VertexPositionColor(new Vector3(_selectionBoxStart.X, _selectionBoxEnd.Y, 0f), Color.White);
				ref VertexPositionColor reference5 = ref _selectionBoxVerts[4];
				reference5 = new VertexPositionColor(new Vector3(_selectionBoxEnd.X, _selectionBoxStart.Y, 0f), Color.White);
				ref VertexPositionColor reference6 = ref _selectionBoxVerts[5];
				reference6 = new VertexPositionColor(new Vector3(_selectionBoxEnd, 0f), Color.White);
				ref VertexPositionColor reference7 = ref _selectionBoxVerts[6];
				reference7 = new VertexPositionColor(new Vector3(_selectionBoxStart.X, _selectionBoxEnd.Y, 0f), Color.White);
				ref VertexPositionColor reference8 = ref _selectionBoxVerts[7];
				reference8 = new VertexPositionColor(new Vector3(_selectionBoxEnd, 0f), Color.White);
			}
		}
		if (InputState.LeftButtonReleased() && _isCreatingSelectionBox)
		{
			_selectedNodes.Clear();
			_isCreatingSelectionBox = false;
			BoundingBox boundingBox = new BoundingBox(new Vector3(_selectionBoxStart, 0f), new Vector3(_selectionBoxEnd, 0f));
			for (int j = 0; j < _currentWallMesh.LineMesh.MeshNodes.Count; j++)
			{
				_currentWallMesh.LineMesh.ChangeNodeColour(j, Color.Red);
				ContainmentType containmentType = boundingBox.Contains(new Vector3(_currentWallMesh.LineMesh.MeshNodes[j]._position, 0f));
				if (containmentType == ContainmentType.Contains)
				{
					_selectedNodes.Add(j);
					_currentWallMesh.LineMesh.ChangeNodeColour(j, Color.LimeGreen);
				}
			}
		}
		if (InputState.MouseStateChanged())
		{
			InputState.LeftButtonReleased();
		}
		if (InputState.KeyboardStateChanged())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl) && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.S))
			{
				_currentWallMesh.SaveNavMesh(new StreamWriter("Content/Shooter/Data/walls.wls").BaseStream);
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl) && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				_currentWallMesh.LoadNavMesh(new StreamReader("Content/Shooter/Data/walls.wls").BaseStream);
				for (int k = 0; k < _currentWallMesh.LineMesh.Lines.Count; k++)
				{
					Body body = BodyFactory.CreateEdge(physWorld, _currentWallMesh.LineMesh.Lines[k].Start, _currentWallMesh.LineMesh.Lines[k].End);
					body.BodyType = BodyType.Static;
					body.Mass = 1000f;
					_physWalls.Add(body);
				}
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Delete) && _selectedNodes.Count > 0)
			{
				for (int num = _selectedNodes.Count - 1; num > -1; num--)
				{
					_currentWallMesh.LineMesh.RemoveNode(_selectedNodes[num]);
					_currentWallMesh.ProcessMesh();
				}
				_selectedNodes.Clear();
				_wallMeshChanged = true;
			}
		}
		InputState.MiddleButtonClicked();
		if (InputState.RightButtonClicked())
		{
			int nearestNodeID2 = _currentWallMesh.LineMesh.GetNearestNodeID(mouseCoord, 15f);
			if (nearestNodeID2 != -1)
			{
				if (_previousLinkNodeID == -1)
				{
					_previousLinkNodeID = nearestNodeID2;
				}
				else
				{
					_currentWallMesh.LineMesh.ChangeNodeColour(_previousLinkNodeID, Color.Red);
					_currentWallMesh.LineMesh.AddLink(_previousLinkNodeID, nearestNodeID2, 0);
					_previousLinkNodeID = nearestNodeID2;
					_wallMeshChanged = true;
				}
				_currentWallMesh.LineMesh.ChangeNodeColour(_previousLinkNodeID, Color.LimeGreen);
			}
		}
		if (InputState.KeyboardStateChanged())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Escape))
			{
				if (_previousLinkNodeID != -1)
				{
					_currentWallMesh.LineMesh.ChangeNodeColour(_previousLinkNodeID, Color.Red);
				}
				ResetSelection();
				_previousLinkNodeID = -1;
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl) && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.Z) && _history.Count > 0)
			{
				MeshEditorHistory meshEditorHistory = _history.Pop();
				switch (meshEditorHistory.ActionType)
				{
				case HistoryActionType.NodeAdded:
					_currentWallMesh.LineMesh.RemoveNode(((NodeAddedHistoryData)meshEditorHistory.ActionData).NodeID);
					break;
				case HistoryActionType.NodeMoved:
					_currentWallMesh.LineMesh.MoveNode(((NodeMovedHistoryData)meshEditorHistory.ActionData).NodeID, ((NodeMovedHistoryData)meshEditorHistory.ActionData).OldPos);
					break;
				}
				_wallMeshChanged = true;
			}
		}
		if (_wallMeshChanged)
		{
			_currentWallMesh.ProcessMesh();
			_wallMeshChanged = false;
		}
	}

	private void ResetSelection()
	{
		_selectedNodes.Clear();
		_isCreatingSelectionBox = false;
		for (int i = 0; i < _currentWallMesh.LineMesh.MeshNodes.Count; i++)
		{
			_currentWallMesh.LineMesh.ChangeNodeColour(i, Color.Red);
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset, Rectangle rect)
	{
		if (_currentWallMesh != null)
		{
			_currentWallMesh.Draw(spriteBatch, offset, rect);
			_currentWallMesh.LineMesh.LineRenderer.DrawShape(_wallVerts.ToArray(), offset, rect);
			if (_isCreatingSelectionBox)
			{
				_currentWallMesh.LineMesh.LineRenderer.DrawShape(_selectionBoxVerts, offset);
			}
		}
	}
}
