using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.PlatformsAreFalling2;

internal class Acid
{
	private Vector2 _position;

	private int _screenWidth;

	private Texture2D _texture;

	private Color _color;

	public Vector2 Position => _position;

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

	public Acid(int screenWidth, GraphicsDevice graphics)
	{
		_screenWidth = screenWidth;
		_position.X = (1280 - screenWidth) / 2;
		_texture = new Texture2D(graphics, 1, 1);
		Color[] data = new Color[1] { Color.White };
		_texture.SetData(data);
	}

	public void Update(float _screenOffset)
	{
		_position.Y -= 0.65f * Math.Max(1f, Math.Abs(_screenOffset) / 10000f);
		if (_screenOffset - _position.Y < -1250f)
		{
			_position.Y = _screenOffset + 1250f;
		}
	}

	public void Reset()
	{
		_position.Y = 1100f;
	}

	public void Draw(SpriteBatch spriteBatch, float _screenOffset)
	{
		spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)(_position.Y - _screenOffset), _screenWidth, 4), Color.Black);
		spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)(_position.Y + 4f - _screenOffset), _screenWidth, (int)(716f - _position.Y)), _color);
	}
}
