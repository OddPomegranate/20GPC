using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Utils;

internal static class Konsole
{
	private static Queue<string> _stringQueue = new Queue<string>();

	private static SinglePixelTexture _singlePixel;

	private static SpriteFont _font;

	public static void LoadContent(GraphicsDevice graphics, ContentManager content)
	{
		_singlePixel = new SinglePixelTexture(graphics);
		_font = content.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
	}

	public static void Update(GameTime gameTime)
	{
	}

	public static void Draw(SpriteBatch spriteBatch, GameTime gameTime)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_singlePixel, new Rectangle(20, 20, 200, 120), Color.Black);
		for (int i = 0; i < _stringQueue.Count; i++)
		{
			spriteBatch.DrawString(_font, _stringQueue.ElementAt(i), new Vector2(20f, 30 + i * 10), Color.White);
		}
		spriteBatch.End();
	}

	public static void SumbitString(string newString)
	{
		_stringQueue.Enqueue(newString);
		if (_stringQueue.Count > 10)
		{
			_stringQueue.Dequeue();
		}
	}
}
