using System.Collections.Generic;
using System.IO;

namespace FarseerPhysics.Common;

public class XMLFragmentParser
{
	private static List<char> _punctuation = new List<char> { '/', '<', '>', '=' };

	private FileBuffer _buffer;

	private XMLFragmentElement _rootNode;

	public XMLFragmentElement RootNode => _rootNode;

	public XMLFragmentParser(Stream stream)
	{
		Load(stream);
	}

	public XMLFragmentParser(string fileName)
	{
		using FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		Load(stream);
	}

	public void Load(Stream stream)
	{
		_buffer = new FileBuffer(stream);
	}

	public static XMLFragmentElement LoadFromFile(string fileName)
	{
		XMLFragmentParser xMLFragmentParser = new XMLFragmentParser(fileName);
		xMLFragmentParser.Parse();
		return xMLFragmentParser.RootNode;
	}

	public static XMLFragmentElement LoadFromStream(Stream stream)
	{
		XMLFragmentParser xMLFragmentParser = new XMLFragmentParser(stream);
		xMLFragmentParser.Parse();
		return xMLFragmentParser.RootNode;
	}

	private string NextToken()
	{
		string text = "";
		bool flag = false;
		do
		{
			IL_0008:
			char next = _buffer.Next;
			if (_punctuation.Contains(next))
			{
				if (text != "")
				{
					_buffer.Position--;
					break;
				}
				flag = true;
			}
			else if (char.IsWhiteSpace(next))
			{
				if (text != "")
				{
					break;
				}
				goto IL_0008;
			}
			text += next;
		}
		while (!flag);
		text = TrimControl(text);
		if (text[0] == '"')
		{
			text = text.Remove(0, 1);
		}
		if (text[text.Length - 1] == '"')
		{
			text = text.Remove(text.Length - 1, 1);
		}
		return text;
	}

	private string PeekToken()
	{
		int position = _buffer.Position;
		string result = NextToken();
		_buffer.Position = position;
		return result;
	}

	private string ReadUntil(char c)
	{
		string text = "";
		while (true)
		{
			char next = _buffer.Next;
			if (next == c)
			{
				break;
			}
			text += next;
		}
		_buffer.Position--;
		if (text[0] == '"')
		{
			text = text.Remove(0, 1);
		}
		if (text[text.Length - 1] == '"')
		{
			text = text.Remove(text.Length - 1, 1);
		}
		return text;
	}

	private string TrimControl(string str)
	{
		string text = str;
		int num = 0;
		while (num != text.Length)
		{
			if (char.IsControl(text[num]))
			{
				text = text.Remove(num, 1);
			}
			else
			{
				num++;
			}
		}
		return text;
	}

	private string TrimTags(string outer)
	{
		int num = outer.IndexOf('>') + 1;
		int num2 = outer.LastIndexOf('<');
		return TrimControl(outer.Substring(num, num2 - num));
	}

	public XMLFragmentElement TryParseNode()
	{
		if (_buffer.EndOfBuffer)
		{
			return null;
		}
		int position = _buffer.Position;
		string text = NextToken();
		if (text != "<")
		{
			throw new XMLFragmentException("Expected \"<\", got " + text);
		}
		XMLFragmentElement xMLFragmentElement = new XMLFragmentElement();
		xMLFragmentElement.Name = NextToken();
		while (true)
		{
			text = NextToken();
			if (text == ">")
			{
				break;
			}
			if (text == "/")
			{
				NextToken();
				xMLFragmentElement.OuterXml = TrimControl(_buffer.Buffer.Substring(position, _buffer.Position - position)).Trim();
				xMLFragmentElement.InnerXml = "";
				return xMLFragmentElement;
			}
			XMLFragmentAttribute xMLFragmentAttribute = new XMLFragmentAttribute();
			xMLFragmentAttribute.Name = text;
			if ((text = NextToken()) != "=")
			{
				throw new XMLFragmentException("Expected \"=\", got " + text);
			}
			xMLFragmentAttribute.Value = NextToken();
			xMLFragmentElement.Attributes.Add(xMLFragmentAttribute);
		}
		while (true)
		{
			int position2 = _buffer.Position;
			text = NextToken();
			if (text == "<")
			{
				text = PeekToken();
				if (text == "/")
				{
					break;
				}
				_buffer.Position = position2;
				xMLFragmentElement.Elements.Add(TryParseNode());
			}
			else
			{
				_buffer.Position = position2;
				xMLFragmentElement.Value = ReadUntil('<');
			}
		}
		NextToken();
		text = NextToken();
		NextToken();
		xMLFragmentElement.OuterXml = TrimControl(_buffer.Buffer.Substring(position, _buffer.Position - position)).Trim();
		xMLFragmentElement.InnerXml = TrimTags(xMLFragmentElement.OuterXml);
		if (text != xMLFragmentElement.Name)
		{
			throw new XMLFragmentException("Mismatched element pairs: \"" + xMLFragmentElement.Name + "\" vs \"" + text + "\"");
		}
		return xMLFragmentElement;
	}

	public void Parse()
	{
		_rootNode = TryParseNode();
		if (_rootNode == null)
		{
			throw new XMLFragmentException("Unable to load root node");
		}
	}
}
