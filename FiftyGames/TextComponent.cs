using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class TextComponent : MenuComponent
{
	protected Anchor _textAnchor;

	protected SpriteFont _font;

	protected string _text;

	protected float _textScale;

	protected Color _textColour;

	protected Color _desiredTextColour;

	protected bool _outlined;

	protected Color _outlineColour;

	protected Color _desiredOutlineColour;

	public Anchor TextAnchor
	{
		get
		{
			return _textAnchor;
		}
		set
		{
			_textAnchor = value;
		}
	}

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	public float TextScale
	{
		get
		{
			return _textScale;
		}
		set
		{
			_textScale = value;
		}
	}

	public SpriteFont Font
	{
		get
		{
			return _font;
		}
		set
		{
			_font = value;
		}
	}

	public Color TextColour
	{
		get
		{
			return _textColour;
		}
		set
		{
			_textColour = value;
		}
	}

	public Color DesiredTextColour
	{
		get
		{
			return _desiredTextColour;
		}
		set
		{
			_desiredTextColour = value;
		}
	}

	public bool IsOutlined
	{
		get
		{
			return _outlined;
		}
		set
		{
			_outlined = value;
		}
	}

	public Color OutlineColour
	{
		get
		{
			return _outlineColour;
		}
		set
		{
			_outlineColour = value;
		}
	}

	public Color DesiredOutlineColour
	{
		get
		{
			return _desiredOutlineColour;
		}
		set
		{
			_desiredOutlineColour = value;
		}
	}

	public TextComponent()
	{
		_textAnchor = Anchor.Centre;
		_text = string.Empty;
		_font = null;
		_textScale = 1f;
		_textColour = (_desiredTextColour = Color.White);
		_outlined = false;
		_outlineColour = (_desiredOutlineColour = Color.Black);
	}

	public override void Load(ContentManager contentLoader)
	{
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if ((_textColour.ToVector4() - _desiredTextColour.ToVector4()).Length() > _colourBlendSpeed)
		{
			Vector4 vector = _desiredTextColour.ToVector4() - _textColour.ToVector4();
			vector.Normalize();
			_textColour = new Color(_textColour.ToVector4() + vector * _colourBlendSpeed);
		}
		else
		{
			_textColour = _desiredTextColour;
		}
		if (_outlined && (_outlineColour.ToVector4() - _desiredOutlineColour.ToVector4()).Length() > _colourBlendSpeed)
		{
			Vector4 vector2 = _desiredOutlineColour.ToVector4() - _outlineColour.ToVector4();
			vector2.Normalize();
			_outlineColour = new Color(_outlineColour.ToVector4() + vector2 * _colourBlendSpeed);
		}
		else
		{
			_outlineColour = _desiredOutlineColour;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_font != null)
		{
			Vector2 vector = _size * 0.5f;
			switch (_anchor)
			{
			case Anchor.TopLeft:
				vector.X *= 1f;
				vector.Y *= 1f;
				break;
			case Anchor.TopCentre:
				vector.X *= 0f;
				vector.Y *= 1f;
				break;
			case Anchor.TopRight:
				vector.X *= -1f;
				vector.Y *= 1f;
				break;
			case Anchor.MiddleLeft:
				vector.X *= 1f;
				vector.Y *= 0f;
				break;
			case Anchor.Centre:
				vector.X *= 0f;
				vector.Y *= 0f;
				break;
			case Anchor.MiddleRight:
				vector.X *= -1f;
				vector.Y *= 0f;
				break;
			case Anchor.BottomLeft:
				vector.X *= 1f;
				vector.Y *= -1f;
				break;
			case Anchor.BottomCentre:
				vector.X *= 0f;
				vector.Y *= -1f;
				break;
			case Anchor.BottomRight:
				vector.X *= -1f;
				vector.Y *= -1f;
				break;
			}
			Vector2 size = _size;
			Vector2 vector2 = _font.MeasureString(_text);
			switch (_textAnchor)
			{
			case Anchor.TopLeft:
				size.X *= -0.5f;
				size.Y *= -0.5f;
				vector2.X *= 0f;
				vector2.Y *= 0f;
				break;
			case Anchor.TopCentre:
				size.X *= 0f;
				size.Y *= -0.5f;
				vector2.X *= 0.5f;
				vector2.Y *= 0f;
				break;
			case Anchor.TopRight:
				size.X *= 0.5f;
				size.Y *= -0.5f;
				vector2.X *= 1f;
				vector2.Y *= 0f;
				break;
			case Anchor.MiddleLeft:
				size.X *= -0.5f;
				size.Y *= 0f;
				vector2.X *= 0f;
				vector2.Y *= 0.5f;
				break;
			case Anchor.Centre:
				size.X *= 0f;
				size.Y *= 0f;
				vector2.X *= 0.5f;
				vector2.Y *= 0.5f;
				break;
			case Anchor.MiddleRight:
				size.X *= 0.5f;
				size.Y *= 0f;
				vector2.X *= 1f;
				vector2.Y *= 0.5f;
				break;
			case Anchor.BottomLeft:
				size.X *= 0.5f;
				size.Y *= -0.5f;
				vector2.X *= 0f;
				vector2.Y *= 1f;
				break;
			case Anchor.BottomCentre:
				size.X *= 0f;
				size.Y *= -0.5f;
				vector2.X *= 0.5f;
				vector2.Y *= 1f;
				break;
			case Anchor.BottomRight:
				size.X *= 0.5f;
				size.Y *= 0.5f;
				vector2.X *= 1f;
				vector2.Y *= 1f;
				break;
			}
			if (_outlined)
			{
				Helper.DrawOutlinedText(spriteBatch, _font, _text, _position + (vector + size - vector2) * _textScale, _textColour, _outlineColour, Helper.OutlineType.Both, _rotation, _origin, 1.2f, new Vector2(_textScale), SpriteEffects.None, _depth + 0.001f);
			}
			else
			{
				spriteBatch.DrawString(_font, _text, _position + (vector + size - vector2) * _textScale, _textColour, _rotation, _origin, _textScale, SpriteEffects.None, _depth + 0.001f);
			}
		}
	}

	public void FitComponentToText(float padding)
	{
		_size = _font.MeasureString(_text) * _textScale;
		_size.X += padding * 2f;
		_size.Y += padding * 2f;
		_desiredSize.X = _size.X;
		_desiredSize.Y = _size.Y;
	}

	public void FitTextToWidth(float padding)
	{
		string text = "";
		string text2 = _text;
		_text = "";
		for (int i = 0; i != text2.Length; i++)
		{
			text += text2[i];
			if (_font.MeasureString(_text + text).X < _size.X - padding)
			{
				if (text2[i] == ' ')
				{
					_text += text;
					text = "";
				}
			}
			else
			{
				_text += "\n";
			}
		}
		_text += text;
	}
}
