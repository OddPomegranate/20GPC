using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FiftyGames.ForeverWars;

internal static class ForeverHelper
{
	public static SoundManager soundManager;

	public static Rectangle _titleSafeArea;

	public static MinigameMeta _minigameMeta;

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

	public static float V2ToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.Y, vector.X);
	}

	public static Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}

	public static playerShip getClosestPlayer(Vector2 selfPosition, playerShip[] playerList)
	{
		List<float> list = new List<float>();
		foreach (playerShip playerShip2 in playerList)
		{
			if (playerShip2 == null)
			{
				return null;
			}
			float item = Vector2.Distance(selfPosition, playerShip2.getPosition());
			list.Add(item);
		}
		int num = -1;
		float num2 = -1f;
		for (int j = 0; j < list.Count; j++)
		{
			if (playerList[j].getAlive() && (list[j] < num2 || num2 < 0f))
			{
				num = j;
				num2 = list[j];
			}
		}
		if (num < 0)
		{
			return null;
		}
		return playerList[num];
	}

	public static playerShip getFurthestPlayer(Vector2 selfPosition, playerShip[] playerList)
	{
		List<float> list = new List<float>();
		foreach (playerShip playerShip2 in playerList)
		{
			float item = Vector2.Distance(selfPosition, playerShip2.getPosition());
			list.Add(item);
		}
		int num = -1;
		float num2 = -1f;
		for (int j = 0; j < list.Count; j++)
		{
			if (playerList[j].getAlive() && (list[j] > num2 || num2 < 0f))
			{
				num = j;
				num2 = list[j];
			}
		}
		if (num < 0)
		{
			return null;
		}
		return playerList[num];
	}
}
