using System.IO;

namespace FarseerPhysics.Common;

public class FileBuffer
{
	public string Buffer { get; set; }

	public int Position { get; set; }

	public int Length => Buffer.Length;

	public char Next
	{
		get
		{
			char result = Buffer[Position];
			Position++;
			return result;
		}
	}

	public char Peek => Buffer[Position];

	public bool EndOfBuffer => Position == Length;

	public FileBuffer(Stream stream)
	{
		using (StreamReader streamReader = new StreamReader(stream))
		{
			Buffer = streamReader.ReadToEnd();
		}
		Position = 0;
	}
}
