using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MicroMachinesGame.ISHelpers;

public struct MeshNode(int id, Vector2 position)
{
	public Vector2 _position = position;

	public int _id = id;

	public List<NodeLink> _neighbours = new List<NodeLink>();

	public Color _colour = Color.Red;
}
