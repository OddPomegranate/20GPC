using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.LightBikes;

internal class GridBlock
{
	private Texture2D m_Sprite;

	private byte alpha = byte.MaxValue;

	private bool set;

	private bool firstPass;

	public Color LastBackingColor = Color.White;

	// FIX (round 5, grey-screen bug): was Color.White. setBackingColor(byte,byte,byte)
	// below is never called anywhere in the project, so this field initializer is the
	// ONLY value backingColor ever takes -- every one of the 239x129 grid cells' backing
	// dot renders pure white at full alpha. Two problems: (1) white sits exactly on the
	// grey axis, and GridShader.fx's HueShift() is a mathematical no-op on any color
	// where r=g=b (rotating a vector around its own axis leaves it unchanged), so the
	// shader's intended slow color-cycle animation was invisible; (2) densely-packed
	// opaque white dots across nearly the whole 1280x720 canvas read as a flat grey
	// wash rather than the "ambient grid glow" the shader's own header comment
	// describes. Switched to a dim, chroma-bearing accent (Tron-esque dim blue) so the
	// hue-cycle is actually visible and the background reads as dark/ambient instead of
	// grey.
	public Color backingColor = new Color(15, 45, 70);

	public Color blockColor = Color.White;

	public GridBlock(Texture2D inSprite)
	{
		m_Sprite = inSprite;
	}

	public void Update()
	{
		if (set && firstPass)
		{
			firstPass = false;
			alpha = 128;
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 gridPosition, int inPixelGap, int x, int y, bool clearMode)
	{
		if (set)
		{
			spriteBatch.Draw(m_Sprite, new Vector2(gridPosition.X + (float)(x * inPixelGap), gridPosition.Y + (float)(y * inPixelGap)), null, blockColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
		}
	}

	public void DrawBackingOnly(SpriteBatch spriteBatch, Vector2 gridPosition, int inPixelGap, int x, int y, bool clearMode)
	{
		Color color = backingColor;
		color.A = alpha;
		spriteBatch.Draw(m_Sprite, new Vector2(gridPosition.X + (float)(x * inPixelGap), gridPosition.Y + (float)(y * inPixelGap)), null, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
	}

	public void setColor(Color inColor)
	{
		blockColor = inColor;
	}

	public bool getSet()
	{
		return set;
	}

	public Color getColor()
	{
		return blockColor;
	}

	public void setBackingColor(byte R, byte G, byte B)
	{
		backingColor = new Color(R, G, B);
	}

	public void setElement(Color inColor)
	{
		blockColor = inColor;
		alpha = byte.MaxValue;
		set = true;
		firstPass = true;
	}
}
