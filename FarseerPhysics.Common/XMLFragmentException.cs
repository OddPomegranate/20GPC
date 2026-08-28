using System;

namespace FarseerPhysics.Common;

public class XMLFragmentException : Exception
{
	public XMLFragmentException()
	{
	}

	public XMLFragmentException(string message)
		: base(message)
	{
	}

	public XMLFragmentException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
