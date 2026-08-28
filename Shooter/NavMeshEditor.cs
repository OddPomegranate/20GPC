using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shooter.ISHelpers;

namespace Shooter;

internal class NavMeshEditor
{
	private NavMesh _currentNavMesh;

	private bool _navMeshChanged;

	private int _currentSelectedNodeID;

	private int _startOfLinkNodeID;

	private int _startOfPathID;

	private int _endOfPathID;

	private List<VertexPositionColor> _pathVerts;

	public List<int> SpecialNodes => _currentNavMesh.LineMesh.SpecialNodes;

	public NavMesh NavMesh => _currentNavMesh;

	public NavMeshEditor(GraphicsDevice graphicsDevice, ContentManager contentManager)
	{
		SetNavMesh(new NavMesh(500, new StreamReader("Content/Shooter/Data/waypoints.wpts").BaseStream, graphicsDevice, contentManager));
	}

	public void SetNavMesh(NavMesh newNavMesh)
	{
		_currentNavMesh = newNavMesh;
		_currentNavMesh.LineMesh.DrawDebugInfo = true;
		_navMeshChanged = true;
		_currentSelectedNodeID = -1;
		_startOfLinkNodeID = -1;
		_pathVerts = new List<VertexPositionColor>();
	}

	public void Update(Vector2 mousePosition)
	{
		if (_currentNavMesh == null)
		{
			return;
		}
		if (InputState.LeftButtonClicked())
		{
			int nearestNodeID = _currentNavMesh.LineMesh.GetNearestNodeID(mousePosition, 15f);
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.S))
			{
				if (nearestNodeID != -1)
				{
					_startOfPathID = nearestNodeID;
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.E))
			{
				if (nearestNodeID != -1)
				{
					_endOfPathID = nearestNodeID;
					List<Vector2> path = _currentNavMesh.GetPath(_startOfPathID, _endOfPathID);
					_pathVerts.Clear();
					int index = 0;
					for (int i = 0; i < path.Count; i++)
					{
						_pathVerts.Add(new VertexPositionColor(new Vector3(path[index], 0f), Color.Red));
						_pathVerts.Add(new VertexPositionColor(new Vector3(path[i], 0f), Color.Red));
						index = i;
					}
				}
			}
			else if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.P))
			{
				if (nearestNodeID != -1)
				{
					if (!_currentNavMesh.LineMesh.SpecialNodes.Contains(nearestNodeID))
					{
						_currentNavMesh.LineMesh.SpecialNodes.Add(nearestNodeID);
						_currentNavMesh.LineMesh.ChangeNodeColour(nearestNodeID, Color.Blue);
					}
					else
					{
						_currentNavMesh.LineMesh.SpecialNodes.Remove(nearestNodeID);
						_currentNavMesh.LineMesh.ChangeNodeColour(nearestNodeID, Color.Red);
					}
				}
			}
			else
			{
				if (_currentSelectedNodeID != -1)
				{
					if (!_currentNavMesh.LineMesh.SpecialNodes.Contains(_currentSelectedNodeID))
					{
						_currentNavMesh.LineMesh.ChangeNodeColour(_currentSelectedNodeID, Color.Red);
					}
					else
					{
						_currentNavMesh.LineMesh.ChangeNodeColour(_currentSelectedNodeID, Color.Blue);
					}
				}
				if (nearestNodeID == -1)
				{
					_currentNavMesh.LineMesh.AddNode(mousePosition);
					_navMeshChanged = true;
				}
				else
				{
					_currentNavMesh.LineMesh.ChangeNodeColour(nearestNodeID, Color.Yellow);
					_currentSelectedNodeID = nearestNodeID;
				}
			}
		}
		if (InputState.LeftButtonHeld() && _currentSelectedNodeID != -1)
		{
			List<Vector2> path2 = _currentNavMesh.GetPath(_startOfPathID, _endOfPathID);
			_pathVerts.Clear();
			int index2 = 0;
			for (int j = 0; j < path2.Count; j++)
			{
				_pathVerts.Add(new VertexPositionColor(new Vector3(path2[index2], 0f), Color.Red));
				_pathVerts.Add(new VertexPositionColor(new Vector3(path2[j], 0f), Color.Red));
				index2 = j;
			}
			_currentNavMesh.LineMesh.MoveNode(_currentSelectedNodeID, mousePosition);
			_navMeshChanged = true;
		}
		if (InputState.LeftButtonReleased())
		{
			if (_currentSelectedNodeID != -1)
			{
				if (!_currentNavMesh.LineMesh.SpecialNodes.Contains(_currentSelectedNodeID))
				{
					_currentNavMesh.LineMesh.ChangeNodeColour(_currentSelectedNodeID, Color.Red);
				}
				else
				{
					_currentNavMesh.LineMesh.ChangeNodeColour(_currentSelectedNodeID, Color.Blue);
				}
			}
			_currentSelectedNodeID = -1;
		}
		if (InputState.MiddleButtonClicked())
		{
			int nearestNodeID2 = _currentNavMesh.LineMesh.GetNearestNodeID(mousePosition, 15f);
			if (nearestNodeID2 != -1)
			{
				_currentNavMesh.LineMesh.RemoveNode(nearestNodeID2);
				_navMeshChanged = true;
			}
		}
		if (InputState.RightButtonClicked())
		{
			int nearestNodeID3 = _currentNavMesh.LineMesh.GetNearestNodeID(mousePosition, 15f);
			if (nearestNodeID3 != -1)
			{
				if (_startOfLinkNodeID == -1)
				{
					_startOfLinkNodeID = nearestNodeID3;
					_currentNavMesh.LineMesh.ChangeNodeColour(_startOfLinkNodeID, Color.LimeGreen);
				}
				else
				{
					_currentNavMesh.LineMesh.AddLink(_startOfLinkNodeID, nearestNodeID3, 0);
					if (!_currentNavMesh.LineMesh.SpecialNodes.Contains(_startOfLinkNodeID))
					{
						_currentNavMesh.LineMesh.ChangeNodeColour(_startOfLinkNodeID, Color.Red);
					}
					else
					{
						_currentNavMesh.LineMesh.ChangeNodeColour(_startOfLinkNodeID, Color.Blue);
					}
					_startOfLinkNodeID = -1;
					_navMeshChanged = true;
				}
			}
		}
		if (InputState.KeyboardStateChanged())
		{
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl) && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.S))
			{
				_currentNavMesh.SaveNavMesh(new StreamWriter("waypoints.wpts").BaseStream);
			}
			if (InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftControl) && InputState.GetCurrentKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				_currentNavMesh.LoadNavMesh(new StreamReader("waypoints.wpts").BaseStream);
			}
		}
		if (_navMeshChanged)
		{
			_currentNavMesh.ProcessMesh();
			_navMeshChanged = false;
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset, Rectangle rect)
	{
		if (_currentNavMesh != null)
		{
			_currentNavMesh.Draw(spriteBatch, offset, rect);
			_currentNavMesh.LineMesh.LineRenderer.DrawShape(_pathVerts.ToArray(), offset, rect);
		}
	}
}
