using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class StarRating : MenuComponent
{
	private Texture2D _starEmpty;

	private Texture2D _starFilled;

	private int _rating;

	public int Rating
	{
		get
		{
			return _rating;
		}
		set
		{
			_rating = value;
		}
	}

	public StarRating()
	{
		_rating = 0;
	}

	public override void Load(ContentManager contentLoader)
	{
		_starEmpty = contentLoader.Load<Texture2D>("Menu/Sprites/Games/StarUnselected");
		_starFilled = contentLoader.Load<Texture2D>("Menu/Sprites/Games/StarSelected");
		base.Load(contentLoader);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 0; i != 5; i++)
		{
			if (i < _rating)
			{
				spriteBatch.Draw(_starFilled, _position - new Vector2((float)_starFilled.Width + 28f * (float)i, (float)_starEmpty.Height * 0.5f), null, _colour, _rotation, _origin, 1f, SpriteEffects.None, _depth);
			}
			else
			{
				spriteBatch.Draw(_starEmpty, _position - new Vector2((float)_starEmpty.Width + 28f * (float)i, (float)_starFilled.Height * 0.5f), null, _colour, _rotation, _origin, 1f, SpriteEffects.None, _depth);
			}
		}
	}
}
