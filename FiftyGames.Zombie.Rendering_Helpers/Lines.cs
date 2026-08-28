using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Rendering_Helpers;

internal class Lines
{
	private List<Line> _lines;

	public List<Line> LineList => _lines;

	public Lines()
	{
		_lines = new List<Line>();
	}

	public Lines(string fileName)
	{
		_lines = new List<Line>();
		LoadLinesFromFile(fileName);
	}

	public void LoadLinesFromFile(string fileName)
	{
		StreamReader streamReader = new StreamReader(fileName);
		BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Vector2 start = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
			Vector2 end = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
			Line line = new Line();
			line.Start = start;
			line.End = end;
			_lines.Add(line);
		}
		binaryReader.Close();
	}
}
