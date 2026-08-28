using System;
using Microsoft.Xna.Framework;

namespace FiftyGames.Rotoball;

internal static class RotoballHelper
{
	public static SoundManager soundManager;

	public static float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed)
	{
		float num = faceThis.X - position.X;
		float num2 = faceThis.Y - position.Y;
		float num3 = (float)Math.Atan2(num2, num);
		float value = WrapAngle(num3 - currentAngle);
		value = MathHelper.Clamp(value, 0f - turnSpeed, turnSpeed);
		return WrapAngle(currentAngle + value);
	}

	private static float WrapAngle(float radians)
	{
		while (radians < -(float)Math.PI)
		{
			radians += (float)Math.PI * 2f;
		}
		while (radians > (float)Math.PI)
		{
			radians -= (float)Math.PI * 2f;
		}
		return radians;
	}
}
