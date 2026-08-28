using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class ControllerIndexDisplay : MenuComponent
{
	private Texture2D _segmentLight;

	private PlayerIndex _playerIndex;

	private bool _connected;

	public PlayerIndex PlayerIndex
	{
		get
		{
			return _playerIndex;
		}
		set
		{
			_playerIndex = value;
		}
	}

	public bool Connected
	{
		get
		{
			return _connected;
		}
		set
		{
			_connected = value;
		}
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("Menu/Sprites/Connect/XboxLogo");
		_segmentLight = contentLoader.Load<Texture2D>("Menu/Sprites/Connect/XboxLightSegment");
		FitComponentToImage();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_connected)
		{
			Vector2 zero = Vector2.Zero;
			SpriteEffects effects = SpriteEffects.None;
			switch (_playerIndex)
			{
			case PlayerIndex.One:
				zero.X = 30f;
				zero.Y = 30f;
				break;
			case PlayerIndex.Two:
				zero.Y = 30f;
				effects = SpriteEffects.FlipHorizontally;
				break;
			case PlayerIndex.Three:
				zero.X = 30f;
				effects = SpriteEffects.FlipVertically;
				break;
			case PlayerIndex.Four:
				effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
				break;
			}
			spriteBatch.Draw(_segmentLight, _position - zero, null, _colour, 0f, Vector2.Zero, 1f, effects, _depth + 0.001f);
		}
	}
}
