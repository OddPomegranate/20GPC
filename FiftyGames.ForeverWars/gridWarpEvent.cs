using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

public struct gridWarpEvent
{
	public Vector2 position;

	public float scale;

	public float rotation;

	public bool isRadial;

	public bool isBeam;

	public Vector2 endPosition;

	public Texture2D imageToUse;

	public float intensity;

	public gridWarpEvent(Vector2 inPosition, float inScale, float inRotation, float inIntensity)
	{
		position = inPosition;
		scale = inScale;
		rotation = inRotation;
		isRadial = true;
		isBeam = false;
		endPosition = Vector2.Zero;
		imageToUse = null;
		intensity = inIntensity;
	}

	public gridWarpEvent(Texture2D inImage, Vector2 inPosition, float inScale)
	{
		position = inPosition;
		scale = inScale;
		rotation = 0f;
		isRadial = true;
		isBeam = false;
		endPosition = Vector2.Zero;
		imageToUse = inImage;
		intensity = 1f;
	}

	public gridWarpEvent(Texture2D inImage, Vector2 inPosition, float inScale, float inRotation)
	{
		position = inPosition;
		scale = inScale;
		rotation = inRotation;
		isRadial = false;
		isBeam = false;
		endPosition = Vector2.Zero;
		imageToUse = inImage;
		intensity = 1f;
	}

	public gridWarpEvent(Texture2D inImage, Vector2 inPosition, float inScale, float inRotation, Vector2 inEndPosition)
	{
		position = inPosition;
		scale = inScale;
		rotation = inRotation;
		isRadial = false;
		isBeam = true;
		endPosition = inEndPosition;
		imageToUse = inImage;
		intensity = 1f;
	}
}
