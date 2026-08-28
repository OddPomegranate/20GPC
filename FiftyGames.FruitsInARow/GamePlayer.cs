using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.FruitsInARow;

internal class GamePlayer
{
	public enum Fruit
	{
		Grape,
		Apple,
		Blueberry,
		Rasin,
		Strawberry,
		Lemon
	}

	private Fruit _fruit;

	private Color _colour;

	private Texture2D _fruitTex;

	private Texture2D _fruitLargeTex;

	private int _wins;

	private Player _player;

	public Texture2D Sprite => _fruitTex;

	public Texture2D LargeSprite => _fruitLargeTex;

	public Color Colour => _colour;

	public bool PressedLeft
	{
		get
		{
			bool flag = _player.GamePadManager.ButtonWasPressed(Buttons.DPadLeft);
			bool result = _player.GamePadManager.ButtonWasPressed(Buttons.LeftThumbstickLeft);
			if (!flag)
			{
				return result;
			}
			return true;
		}
	}

	public bool HoldLeft
	{
		get
		{
			bool flag = _player.GamePadManager.ButtonIsHeld(Buttons.DPadLeft);
			bool result = _player.GamePadManager.ButtonIsHeld(Buttons.LeftThumbstickLeft);
			if (!flag)
			{
				return result;
			}
			return true;
		}
	}

	public bool PressedRight
	{
		get
		{
			bool flag = _player.GamePadManager.ButtonWasPressed(Buttons.DPadRight);
			bool result = _player.GamePadManager.ButtonWasPressed(Buttons.LeftThumbstickRight);
			if (!flag)
			{
				return result;
			}
			return true;
		}
	}

	public bool HoldRight
	{
		get
		{
			bool flag = _player.GamePadManager.ButtonIsHeld(Buttons.DPadRight);
			bool result = _player.GamePadManager.ButtonIsHeld(Buttons.LeftThumbstickRight);
			if (!flag)
			{
				return result;
			}
			return true;
		}
	}

	public bool PressedA => _player.GamePadManager.ButtonWasPressed(Buttons.A);

	public Fruit PlayerFruit
	{
		get
		{
			return _fruit;
		}
		set
		{
			_fruit = value;
		}
	}

	public string Name => _player.Name;

	public int Wins
	{
		get
		{
			return _wins;
		}
		set
		{
			_wins = value;
		}
	}

	public GamePlayer(Player player)
	{
		_player = player;
		_colour = player.Colour();
	}

	public void Load(ContentManager content)
	{
		switch (_fruit)
		{
		case Fruit.Apple:
			_fruitTex = content.Load<Texture2D>("FruitsInARow\\Image\\GreenPlayer");
			_fruitLargeTex = content.Load<Texture2D>("FruitsInARow\\Image\\GreenPlayer_Large");
			break;
		case Fruit.Blueberry:
			_fruitTex = content.Load<Texture2D>("FruitsInARow\\Image\\LightBluePlayer");
			_fruitLargeTex = content.Load<Texture2D>("FruitsInARow\\Image\\LightBluePlayer_Large");
			break;
		case Fruit.Grape:
			_fruitTex = content.Load<Texture2D>("FruitsInARow\\Image\\DarkBluePlayer");
			_fruitLargeTex = content.Load<Texture2D>("FruitsInARow\\Image\\DarkBluePlayer_Large");
			break;
		case Fruit.Lemon:
			_fruitTex = content.Load<Texture2D>("FruitsInARow\\Image\\YellowPlayer");
			_fruitLargeTex = content.Load<Texture2D>("FruitsInARow\\Image\\YellowPlayer_Large");
			break;
		case Fruit.Rasin:
			_fruitTex = content.Load<Texture2D>("FruitsInARow\\Image\\PurplePlayer");
			_fruitLargeTex = content.Load<Texture2D>("FruitsInARow\\Image\\PurplePlayer_Large");
			break;
		case Fruit.Strawberry:
			_fruitTex = content.Load<Texture2D>("FruitsInARow\\Image\\RedPlayer");
			_fruitLargeTex = content.Load<Texture2D>("FruitsInARow\\Image\\RedPlayer_Large");
			break;
		}
	}

	public void Update(GameTime gameTime)
	{
	}
}
