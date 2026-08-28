using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories;

public static class LinkFactory
{
	public static Path CreateChain(World world, Vector2 start, Vector2 end, float linkWidth, float linkHeight, bool fixStart, bool fixEnd, int numberOfLinks, float linkDensity)
	{
		Path path = new Path();
		path.Add(start);
		path.Add(end);
		PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(linkWidth, linkHeight), linkDensity);
		List<Body> list = PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, BodyType.Dynamic, numberOfLinks);
		if (fixStart)
		{
			JointFactory.CreateFixedRevoluteJoint(world, list[0], new Vector2(0f, 0f - linkHeight / 2f), list[0].Position);
		}
		if (fixEnd)
		{
			JointFactory.CreateFixedRevoluteJoint(world, list[list.Count - 1], new Vector2(0f, linkHeight / 2f), list[list.Count - 1].Position);
		}
		PathManager.AttachBodiesWithRevoluteJoint(world, list, new Vector2(0f, 0f - linkHeight), new Vector2(0f, linkHeight), connectFirstAndLast: false, collideConnected: false);
		return path;
	}
}
