using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class explosionElement
{
	private const int smallAnimationFrameMax = 12;

	private const int smallAnimationCounterMax = 2;

	private const int tinyAnimationFrameMax = 12;

	private const int tinyAnimationCounterMax = 0;

	private Vector2 position;

	private int animationSpeed;

	private int animationFrameMax;

	private int animationFrame;

	private int animationCounter;

	private int animationCounterMax;

	private int animationFrameIncrement = 1;

	private bool flipped;

	private float rotation;

	private float scale;

	private Vector2 explosionOrigin;

	private Color explosionOverlayYellow = Color.Yellow;

	private Color explosionOverlayBright = new Color(255, 255, 255);

	private Color explosionOverlayGrey = Color.Gray;

	private Texture2D explosionSpriteSheet;

	private explosionColor explosionColor;

	private gridSystem gridManager;

	public explosionElement(Texture2D explosionSheet, Vector2 inPosition, float inRotation, float inScale, explosionColor explosionColorToUse, explosionType explosionTypeToUse, bool inFlipped, gridSystem inGridManager)
	{
		gridManager = inGridManager;
		explosionSpriteSheet = explosionSheet;
		position = inPosition;
		rotation = inRotation;
		scale = inScale;
		explosionOrigin = new Vector2(80f);
		flipped = inFlipped;
		explosionColor = explosionColorToUse;
		switch (explosionTypeToUse)
		{
		case explosionType.tiny:
			animationFrameMax = 12;
			animationFrame = 0;
			animationCounter = 0;
			animationCounterMax = 0;
			animationFrameIncrement = 1;
			break;
		case explosionType.small:
			animationFrameMax = 12;
			animationFrame = 0;
			animationCounter = 2;
			animationCounterMax = 2;
			animationFrameIncrement = 1;
			break;
		case explosionType.large:
			animationFrameMax = 12;
			animationFrame = 0;
			animationCounter = 2;
			animationCounterMax = 2;
			animationFrameIncrement = 1;
			break;
		case explosionType.smallSmoke:
			animationFrameMax = 12;
			animationFrame = 0;
			animationCounter = 2;
			animationCounterMax = 2;
			animationFrameIncrement = 1;
			break;
		case explosionType.tinySmoke:
			break;
		}
	}

	public bool Update()
	{
		gridManager.AddWarpEvent(null, position, scale);
		animationCounter--;
		if (animationCounter < 0)
		{
			animationFrame += animationFrameIncrement;
			animationCounter = animationCounterMax;
		}
		if (animationFrame > animationFrameMax)
		{
			return true;
		}
		return false;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		switch (explosionColor)
		{
		case explosionColor.Yellow:
			spriteBatch.Draw(explosionSpriteSheet, position, new Rectangle(animationFrame * 160, 0, 160, 160), explosionOverlayYellow, rotation, explosionOrigin, scale, (!flipped) ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
			break;
		case explosionColor.Bright:
			spriteBatch.Draw(explosionSpriteSheet, position, new Rectangle(animationFrame * 160, 0, 160, 160), explosionOverlayBright, rotation, explosionOrigin, scale, (!flipped) ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
			break;
		case explosionColor.Grey:
			spriteBatch.Draw(explosionSpriteSheet, position, new Rectangle(animationFrame * 160, 0, 160, 160), explosionOverlayGrey, rotation, explosionOrigin, scale, (!flipped) ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
			break;
		}
	}
}
