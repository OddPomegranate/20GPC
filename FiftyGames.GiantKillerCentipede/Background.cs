using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Background
{
	protected Rectangle _view;

	protected Texture2D _tileSprite;

	protected List<Vector2> _tilePositions;

	protected List<Color> _tileColours;

	public Background(Rectangle viewport)
	{
		_view = viewport;
		_tilePositions = new List<Vector2>();
		_tileColours = new List<Color>();
	}

	public void Load(ContentManager contentLoader, Random rng)
	{
		_tileSprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\BackgroundTile");
		Point point = new Point(_tileSprite.Width, _tileSprite.Height);
		Point zero = Point.Zero;
		float[] array = new float[3]
		{
			(float)rng.NextDouble() * 0.2f,
			0.4f + (float)rng.NextDouble() * 0.6f,
			(float)rng.NextDouble() * 0.3f
		};
		while (zero.Y < _view.Height)
		{
			while (zero.X < _view.Width)
			{
				array[0] = 0.1f + (float)rng.NextDouble() * 0.1f;
				array[1] = 0.4f + (float)rng.NextDouble() * 0.3f;
				array[2] = 0.2f + (float)rng.NextDouble() * 0.1f;
				_tileColours.Add(new Color((int)(byte)(array[0] * 0.2f), array[1], (int)(byte)(array[2] * 0.2f)));
				_tilePositions.Add(new Vector2(zero.X, zero.Y));
				zero.X += point.X;
			}
			zero.Y += point.Y;
			zero.X = 0;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < _tilePositions.Count; i++)
		{
			spriteBatch.Draw(_tileSprite, _tilePositions[i], _tileColours[i]);
		}
	}
}
