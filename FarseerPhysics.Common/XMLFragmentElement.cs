using System.Collections.Generic;

namespace FarseerPhysics.Common;

public class XMLFragmentElement
{
	private List<XMLFragmentAttribute> _attributes = new List<XMLFragmentAttribute>();

	private List<XMLFragmentElement> _elements = new List<XMLFragmentElement>();

	public IList<XMLFragmentElement> Elements => _elements;

	public IList<XMLFragmentAttribute> Attributes => _attributes;

	public string Name { get; set; }

	public string Value { get; set; }

	public string OuterXml { get; set; }

	public string InnerXml { get; set; }
}
