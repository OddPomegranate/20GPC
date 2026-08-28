using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.LunarLander;

internal class LandingPad
{
	public enum LandingPadClass
	{
		Easy,
		Medium,
		Hard,
		Legendary
	}

	protected const float EasyWidth = 75f;

	protected const float MediumWidth = 50f;

	protected const float HardWidth = 30f;

	protected const float LegendaryWidth = 15f;

	protected const int FirstBonus = 2;

	protected LandingPadClass _class;

	protected int[] _rewards = new int[4] { 15, 30, 50, 100 };

	protected Pod _user;

	protected Vector2 _position;

	protected Vector2 _size;

	protected VertexPositionColor[] _shapeVerts;

	protected short[] _shapeIndex;

	protected BoundingBox _physVolume;

	public BoundingBox CollisionVolume => _physVolume;

	public Vector2 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
			UpdatePad();
		}
	}

	public Vector2 Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
			UpdatePad();
		}
	}

	public bool IsUsed => _user != null;

	public Pod User => _user;

	public LandingPad(Vector2 position, LandingPadClass difficultyClass)
	{
		_class = difficultyClass;
		_position = position;
		_size = new Vector2(1f, 10f);
		switch (_class)
		{
		case LandingPadClass.Easy:
			_size.X = 75f;
			break;
		case LandingPadClass.Medium:
			_size.X = 50f;
			break;
		case LandingPadClass.Hard:
			_size.X = 30f;
			break;
		case LandingPadClass.Legendary:
			_size.X = 15f;
			break;
		}
		Color color = new Color(90, 90, 90, 255);
		_shapeVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference = ref _shapeVerts[0];
		reference = new VertexPositionColor(new Vector3(position.X - _size.X / 2f, position.Y, 0f), color);
		ref VertexPositionColor reference2 = ref _shapeVerts[1];
		reference2 = new VertexPositionColor(new Vector3(position.X + _size.X / 2f, position.Y, 0f), color);
		ref VertexPositionColor reference3 = ref _shapeVerts[2];
		reference3 = new VertexPositionColor(new Vector3(position.X + _size.X / 2f, position.Y + _size.Y, 0f), color);
		ref VertexPositionColor reference4 = ref _shapeVerts[3];
		reference4 = new VertexPositionColor(new Vector3(position.X - _size.X / 2f, position.Y + _size.Y, 0f), color);
		_shapeIndex = new short[8] { 0, 1, 1, 2, 2, 3, 3, 0 };
		_user = null;
		UpdatePad();
	}

	public void Draw(LineRender graphics)
	{
		graphics.DrawIndexedShape(_shapeVerts, _shapeIndex);
	}

	protected void UpdatePad()
	{
		_physVolume.Min = new Vector3(_position.X - _size.X / 2f, _position.Y, 0f);
		_physVolume.Max = new Vector3(_position.X + _size.X / 2f, _position.Y + _size.Y, 0f);
	}

	public void Use(Pod pod, bool first)
	{
		_user = pod;
		int num = _rewards[(int)_class];
		if (first)
		{
			num *= 2;
		}
		_user.AwardScore(num);
		for (int i = 0; i < _shapeVerts.Length; i++)
		{
			_shapeVerts[i].Color = _user.Colour;
		}
	}
}
