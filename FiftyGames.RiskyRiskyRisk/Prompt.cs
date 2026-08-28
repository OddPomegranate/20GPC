using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RiskyRiskyRisk;

internal class Prompt
{
	public enum Button
	{
		A,
		B,
		X,
		Y,
		None
	}

	private const float Scale = 0.6f;

	private Vector2 _position;

	private Texture2D[] _textures;

	private Button _button;

	private string _text;

	private Point _size;

	private bool _isDrawn = true;

	private Color _color;

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

	public bool IsDrawn
	{
		get
		{
			return _isDrawn;
		}
		set
		{
			_isDrawn = value;
		}
	}

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	public Prompt(Vector2 position, Button button, string text)
		: this(position, button, text, Color.White)
	{
	}

	public Prompt(Vector2 position, Button button, string text, Color color)
	{
		_position = position;
		_button = button;
		_text = text;
		_textures = new Texture2D[4];
		_color = color;
	}

	public void LoadContent(ContentManager content)
	{
		_textures[0] = content.Load<Texture2D>("Menu/Sprites/Buttons/A");
		_textures[1] = content.Load<Texture2D>("Menu/Sprites/Buttons/B");
		_textures[2] = content.Load<Texture2D>("Menu/Sprites/Buttons/X");
		_textures[3] = content.Load<Texture2D>("Menu/Sprites/Buttons/Y");
		_size = new Point((int)((float)_textures[0].Width * 0.6f), (int)((float)_textures[0].Height * 0.6f));
	}

	public void Draw(SpriteBatch spriteBatch, SpriteFont font)
	{
		if (_isDrawn)
		{
			if (_button != Button.None)
			{
				spriteBatch.Draw(_textures[(int)_button], _position, null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
			}
			Helper.DrawOutlinedText(spriteBatch, font, _text, new Vector2(_position.X + (float)_size.X, _position.Y + ((float)_size.Y - font.MeasureString(_text).Y) / 2f), _color, Color.Black, Helper.OutlineType.Orthogonal, centered: false, 1f);
		}
	}
}
