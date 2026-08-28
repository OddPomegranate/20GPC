using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes;

public struct MassData : IEquatable<MassData>
{
	public float Area;

	public Vector2 Centroid;

	public float Inertia;

	public float Mass;

	public bool Equals(MassData other)
	{
		return this == other;
	}

	public static bool operator ==(MassData left, MassData right)
	{
		if (left.Area == right.Area && left.Mass == right.Mass && left.Centroid == right.Centroid)
		{
			return left.Inertia == right.Inertia;
		}
		return false;
	}

	public static bool operator !=(MassData left, MassData right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if ((object)obj.GetType() != typeof(MassData))
		{
			return false;
		}
		return Equals((MassData)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = Area.GetHashCode();
		hashCode = (hashCode * 397) ^ Centroid.GetHashCode();
		hashCode = (hashCode * 397) ^ Inertia.GetHashCode();
		return (hashCode * 397) ^ Mass.GetHashCode();
	}
}
