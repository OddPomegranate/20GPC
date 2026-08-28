using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Impossible;

internal static class ImpossibleHelper
{
	public static SoundManager soundManager;

	public static FiftyGames _framework;

	public static MinigameMeta _minigameMeta;

	public static void drawStringBacking(SpriteBatch spriteBatch, SpriteFont spriteFont, Texture2D singlePixelTexture, string text, Vector2 position, float rotation, float scale)
	{
		Rectangle destinationRectangle = new Rectangle((int)(position.X - spriteFont.MeasureString(text).X / 2f), (int)(position.Y - spriteFont.MeasureString(text).Y / 2f), (int)spriteFont.MeasureString(text).X, (int)spriteFont.MeasureString(text).Y);
		spriteBatch.Draw(singlePixelTexture, destinationRectangle, Color.Gray * 0.4f);
		spriteBatch.DrawString(spriteFont, text, position, Color.White, rotation, spriteFont.MeasureString(text) / 2f, scale, SpriteEffects.None, 0f);
	}
}
