using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.RiskyRiskyRisk;

internal class Hex
{
	private Point _pPosition;

	private bool _isActive;

	private int _decay;

	private Texture2D _texture;

	private Vector2 _position;

	private Point _size;

	private float _scale;

	private Country _country;

	public Color Color { get; set; }

	public bool Filled { get; set; }

	public int Pass { get; set; }

	public static Vector2 WorldPosition => new Vector2(16f, 10f);

	public Point PPosition => _pPosition;

	public Vector2 Position => _position;

	public Point Size => _size;

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			_isActive = value;
		}
	}

	public int Decay
	{
		get
		{
			return _decay;
		}
		set
		{
			_decay = value;
		}
	}

	public Country Country
	{
		get
		{
			return _country;
		}
		set
		{
			_country = value;
		}
	}

	public Hex(Point pPosition, bool isActive, ref Random random, float scale)
	{
		_pPosition = pPosition;
		_isActive = random.Next(10) - ((!isActive) ? 9 : 0) >= 0;
		_decay = ((!_isActive) ? random.Next(4, 10) : 0);
		_scale = scale;
		Filled = false;
		Color = Color.White;
	}

	public void LoadContent(ContentManager content)
	{
		_texture = content.Load<Texture2D>("RiskyRiskyRisk/Sprites/hex");
		_size = new Point((int)((float)_texture.Width * _scale), (int)((float)_texture.Height * _scale));
		_position = new Vector2(_pPosition.X * _size.X + _size.X / 2 * (_pPosition.Y % 2), _pPosition.Y * _size.Y - _size.Y / 4 * _pPosition.Y);
	}

	public void Draw(SpriteBatch spriteBatch, Rectangle drawRect)
	{
		if (_isActive)
		{
			spriteBatch.Draw(_texture, WorldPosition + _position - new Vector2(drawRect.X, drawRect.Y), null, Color, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
		}
	}
}
