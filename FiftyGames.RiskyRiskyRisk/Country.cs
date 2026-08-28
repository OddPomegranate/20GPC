using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Zombie;

namespace FiftyGames.RiskyRiskyRisk;

internal class Country
{
	private List<Hex> _hexagons;

	private List<Country> _connectedCountries;

	private int _order;

	private static int CountryNumber;

	private float _yOffset;

	private bool _isSelected;

	private RenderTarget2D _texture;

	private Texture2D _dieTexture;

	private Point _dieSize;

	private Rectangle _drawRect;

	private Commander _owner;

	private int _dice;

	private Vector2[] _dicePositions;

	private int _maxDice = 5;

	private bool _isFlashing;

	private int _flashTimer;

	private Vector4 _flash;

	private bool _isFloodChecked;

	private Color _diceColor;

	private Color _countryColor;

	public int Order => _order;

	public List<Hex> Hexagons => _hexagons;

	public List<Country> ConnectedCountries => _connectedCountries;

	public int HexCount => _hexagons.Count;

	public Vector2 Position => new Vector2(_drawRect.X, _drawRect.Y);

	public Texture2D Texture => _texture;

	public bool IsSelected => _isSelected;

	public Commander Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
			_countryColor = ((_owner == null) ? Color.GhostWhite : _owner.CountryColor);
			_diceColor = ((_owner == null) ? Color.GhostWhite : _owner.DiceColor);
		}
	}

	public int Dice
	{
		get
		{
			return _dice;
		}
		set
		{
			_dice = value;
		}
	}

	public int MaxDice => _maxDice;

	public bool IsFloodChecked
	{
		get
		{
			return _isFloodChecked;
		}
		set
		{
			_isFloodChecked = value;
		}
	}

	public Color DiceColor => _diceColor;

	public Country()
	{
		_hexagons = new List<Hex>();
		_connectedCountries = new List<Country>();
		_order = CountryNumber;
		CountryNumber++;
	}

	public void CreateTexture(FullscreenQuad fsq, SpriteBatch spriteBatch, Effect effect, Random random)
	{
		Point point = new Point(1280, 720);
		Point point2 = default(Point);
		_maxDice = _hexagons.Count;
		for (int i = 0; i < _maxDice; i++)
		{
			point.X = Math.Min(point.X, (int)_hexagons[i].Position.X);
			point.Y = Math.Min(point.Y, (int)_hexagons[i].Position.Y);
			point2.X = Math.Max(point2.X, (int)_hexagons[i].Position.X);
			point2.Y = Math.Max(point2.Y, (int)_hexagons[i].Position.Y);
		}
		_drawRect = new Rectangle(point.X + 2, point.Y + 2, 4 + point2.X - point.X + 2 * _hexagons[0].Size.X, 4 + point2.Y - point.Y + 2 * _hexagons[0].Size.Y);
		_dicePositions = new Vector2[_maxDice];
		new Vector2(_drawRect.Center.X, _drawRect.Center.Y);
		Helper.Shuffle(_hexagons, random);
		for (int j = 0; j < _dicePositions.Length; j++)
		{
			ref Vector2 reference = ref _dicePositions[j];
			reference = _hexagons[j].Position + new Vector2((int)((float)_dieSize.X * 1.375f), 0f);
		}
		if ((float)_drawRect.Height / (float)_drawRect.Width > 1.3f)
		{
			_drawRect.Width += 50;
		}
		_texture = new RenderTarget2D(spriteBatch.GraphicsDevice, _drawRect.Width, _drawRect.Height);
		RenderTarget2D renderTarget2D = new RenderTarget2D(spriteBatch.GraphicsDevice, _drawRect.Width, _drawRect.Height);
		spriteBatch.GraphicsDevice.SetRenderTarget(_texture);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin();
		for (int k = 0; k < _hexagons.Count; k++)
		{
			_hexagons[k].Draw(spriteBatch, _drawRect);
		}
		spriteBatch.End();
		spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget2D);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		effect.CurrentTechnique = effect.Techniques["InnerGlow"];
		effect.Parameters["Color"].SetValue(new Vector4(0f, 0f, 0f, 0f));
		effect.Parameters["Texture"].SetValue(_texture);
		effect.Parameters["TextureSize"].SetValue(new Vector2(_drawRect.Width, _drawRect.Height));
		effect.CurrentTechnique.Passes[0].Apply();
		fsq.Render(Vector2.One * -1f, Vector2.One);
		spriteBatch.GraphicsDevice.SetRenderTarget(_texture);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		effect.CurrentTechnique = effect.Techniques["Normal"];
		effect.Parameters["Color"].SetValue(new Vector4(0f, 0f, 0f, 0f));
		effect.Parameters["Texture"].SetValue(renderTarget2D);
		effect.Parameters["TextureSize"].SetValue(new Vector2(_drawRect.Width, _drawRect.Height));
		effect.CurrentTechnique.Passes[0].Apply();
		fsq.Render(Vector2.One * -1f, Vector2.One);
		spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget2D);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		effect.CurrentTechnique = effect.Techniques["Outline"];
		effect.Parameters["Color"].SetValue(new Vector4(0f, 0f, 0f, 1f));
		effect.Parameters["Texture"].SetValue(_texture);
		effect.Parameters["TextureSize"].SetValue(new Vector2(_drawRect.Width, _drawRect.Height));
		effect.CurrentTechnique.Passes[0].Apply();
		fsq.Render(Vector2.One * -1f, Vector2.One);
		spriteBatch.GraphicsDevice.SetRenderTarget(_texture);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		effect.CurrentTechnique = effect.Techniques["Outline"];
		effect.Parameters["Color"].SetValue(new Vector4(0f, 0f, 0f, 1f));
		effect.Parameters["Texture"].SetValue(renderTarget2D);
		effect.Parameters["TextureSize"].SetValue(new Vector2(_drawRect.Width, _drawRect.Height));
		effect.CurrentTechnique.Passes[0].Apply();
		fsq.Render(Vector2.One * -1f, Vector2.One);
	}

	public void LoadContent(ContentManager content)
	{
		_dieTexture = content.Load<Texture2D>("RiskyRiskyRisk/Sprites/dice");
		_dieSize = new Point(_dieTexture.Width - 1, _dieTexture.Height - 13);
	}

	public void Update(GameTime gameTime)
	{
		if (_isFlashing)
		{
			_flashTimer += gameTime.ElapsedGameTime.Milliseconds;
			if (_flashTimer < 300)
			{
				_flash = new Vector4(0.01953125f, 0.05859375f, 0.09765625f, 0f);
			}
			else
			{
				_flash = Vector4.Zero;
			}
			if (_flashTimer > 600)
			{
				_flashTimer -= 600;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_texture, new Vector2(_drawRect.X, (float)_drawRect.Y + _yOffset), (_owner == null) ? new Color(Color.Linen.ToVector4() + _flash) : new Color(_countryColor.ToVector4() + _flash));
	}

	public void DrawDice(SpriteBatch spriteBatch)
	{
		for (int i = 0; i != _dice; i++)
		{
			spriteBatch.Draw(_dieTexture, new Vector2(_dicePositions[i].X - 6f, _dicePositions[i].Y + 18f + _yOffset), (_owner == null) ? Color.Linen : _diceColor);
		}
	}

	public static int SortSelected(Country c1, Country c2)
	{
		if (c1.Order != c2.Order)
		{
			if (c1.Order >= c2.Order)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public int Flood(Commander owner)
	{
		if (_isFloodChecked)
		{
			return 0;
		}
		int num = 0;
		if (owner == _owner)
		{
			num = 1;
			_isFloodChecked = true;
			foreach (Country connectedCountry in _connectedCountries)
			{
				num += connectedCountry.Flood(owner);
			}
		}
		return num;
	}

	public void AddHex(Hex hex)
	{
		hex.Country = this;
		_hexagons.Add(hex);
	}

	public void AddConnectedCountry(Country country)
	{
		if (!_connectedCountries.Contains(country))
		{
			_connectedCountries.Add(country);
		}
	}

	public void RemoveConnectedCountry(Country country)
	{
		if (_connectedCountries.Contains(country))
		{
			_connectedCountries.Remove(country);
		}
	}

	public void Combine(Country country)
	{
		for (int i = 0; i != country.HexCount; i++)
		{
			country.Hexagons[i].Country = this;
		}
		_hexagons.AddRange(country.Hexagons);
		foreach (Country connectedCountry in country.ConnectedCountries)
		{
			AddConnectedCountry(connectedCountry);
			connectedCountry.AddConnectedCountry(this);
		}
		country.Hexagons.Clear();
	}

	public Point HexPPos(int index)
	{
		return _hexagons[index].PPosition;
	}

	public bool Select()
	{
		if (!_isSelected)
		{
			_yOffset = -10f;
			_order += 40;
			_isSelected = true;
		}
		return _isSelected;
	}

	public bool DeSelect()
	{
		if (_isSelected)
		{
			_yOffset = 0f;
			_order -= 40;
			_isSelected = false;
		}
		return _isSelected;
	}

	public void Flash(bool active)
	{
		if (!active)
		{
			_isFlashing = false;
			_flashTimer = 0;
			_flash = Vector4.Zero;
		}
		else
		{
			_isFlashing = true;
			_flashTimer = 0;
		}
	}

	public Vector2 DicePosition(int index)
	{
		return _dicePositions[index];
	}
}
