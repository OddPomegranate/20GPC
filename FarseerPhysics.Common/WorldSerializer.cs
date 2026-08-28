using System.IO;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Common;

public static class WorldSerializer
{
	public static void Serialize(World world, string filename)
	{
		using FileStream stream = new FileStream(filename, FileMode.Create);
		new WorldXmlSerializer().Serialize(world, stream);
	}

	public static void Deserialize(World world, string filename)
	{
		using FileStream stream = new FileStream(filename, FileMode.Open);
		new WorldXmlDeserializer().Deserialize(world, stream);
	}

	public static World Deserialize(string filename)
	{
		using FileStream stream = new FileStream(filename, FileMode.Open);
		return new WorldXmlDeserializer().Deserialize(stream);
	}
}
