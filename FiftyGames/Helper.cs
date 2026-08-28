using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

public static class Helper
{
	public enum OutlineType
	{
		Diagonal,
		Orthogonal,
		Both
	}

	public static void DrawOutlinedText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor)
	{
		DrawOutlinedText(sb, font, text, position, color, outlineColor, OutlineType.Orthogonal, 0f, Vector2.Zero, 1f, Vector2.One, SpriteEffects.None, 0f);
	}

	public static void DrawOutlinedText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, OutlineType outlineType)
	{
		DrawOutlinedText(sb, font, text, position, color, outlineColor, outlineType, 0f, Vector2.Zero, 1f, Vector2.One, SpriteEffects.None, 0f);
	}

	public static void DrawOutlinedText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, OutlineType outlineType, bool centered, float thickness)
	{
		DrawOutlinedText(sb, font, text, position, color, outlineColor, outlineType, 0f, centered ? (font.MeasureString(text) / 2f) : Vector2.Zero, thickness, Vector2.One, SpriteEffects.None, 0f);
	}

	public static void DrawOutlinedText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, OutlineType outlineType, bool centered, float thickness, Vector2 scale)
	{
		DrawOutlinedText(sb, font, text, position, color, outlineColor, outlineType, 0f, centered ? (font.MeasureString(text) / 2f) : Vector2.Zero, thickness, scale, SpriteEffects.None, 0f);
	}

	public static void DrawOutlinedText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, OutlineType outlineType, float rotation, bool centered, float thickness, Vector2 scale)
	{
		DrawOutlinedText(sb, font, text, position, color, outlineColor, outlineType, rotation, centered ? (font.MeasureString(text) / 2f) : Vector2.Zero, thickness, scale, SpriteEffects.None, 0f);
	}

	public static void DrawOutlinedText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, OutlineType outlineType, float rotation, Vector2 origin, float thickness, Vector2 scale)
	{
		DrawOutlinedText(sb, font, text, position, color, outlineColor, outlineType, rotation, origin, thickness, scale, SpriteEffects.None, 0f);
	}

	public static void DrawOutlinedText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, OutlineType outlineType, float rotation, Vector2 origin, float thickness, Vector2 scale, SpriteEffects spriteEffects, float layerDepth)
	{
		if (outlineType != OutlineType.Diagonal)
		{
			sb.DrawString(font, text, position + new Vector2(1f * thickness, 0f), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
			sb.DrawString(font, text, position + new Vector2(-1f * thickness, 0f), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
			sb.DrawString(font, text, position + new Vector2(0f, 1f * thickness), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
			sb.DrawString(font, text, position + new Vector2(0f, -1f * thickness), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
		}
		if (outlineType != OutlineType.Orthogonal)
		{
			sb.DrawString(font, text, position + new Vector2(1f * thickness, 1f * thickness), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
			sb.DrawString(font, text, position + new Vector2(-1f * thickness, -1f * thickness), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
			sb.DrawString(font, text, position + new Vector2(-1f * thickness, 1f * thickness), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
			sb.DrawString(font, text, position + new Vector2(1f * thickness, -1f * thickness), outlineColor, rotation, origin, scale, spriteEffects, layerDepth);
		}
		sb.DrawString(font, text, position, color, rotation, origin, scale, spriteEffects, layerDepth + 0.001f);
	}

	public static void Shuffle<T>(IList<T> list, Random random)
	{
		int num = list.Count;
		while (num != 1)
		{
			num--;
			int index = random.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static int AnimationFrame(int fps, int timer, int startFrame, int endFrame)
	{
		int num = 1000 / fps;
		int num2 = Math.Abs(endFrame - startFrame);
		timer %= num2 * num;
		return (int)MathHelper.Lerp(startFrame, endFrame, (float)timer / (float)(num2 * num)) % (startFrame + num2);
	}

	public static int AnimationFrame(int fps, int timer, int numFrames)
	{
		return AnimationFrame(fps, timer, 0, numFrames);
	}

	public static bool PerPixelCollision(Texture2D texture1, Vector2 position1, float scale1, float rotation1, Texture2D texture2, Vector2 position2, float scale2, float rotation2)
	{
		if (texture1 == null || texture2 == null)
		{
			return false;
		}
		Matrix matrix = Matrix.CreateTranslation(new Vector3(new Vector2(0f - (float)texture1.Width / 2f, 0f - (float)texture1.Height / 2f), 0f)) * Matrix.CreateScale(scale1) * Matrix.CreateRotationZ(rotation1) * Matrix.CreateTranslation(new Vector3(position1, 0f));
		Rectangle rectangle = CalculateBoundingRectangle(new Rectangle(0, 0, texture1.Width, texture1.Height), matrix);
		Matrix matrix2 = Matrix.CreateTranslation(new Vector3(new Vector2(0f - (float)texture2.Width / 2f, 0f - (float)texture2.Height / 2f), 0f)) * Matrix.CreateScale(scale2) * Matrix.CreateRotationZ(rotation2) * Matrix.CreateTranslation(new Vector3(position2, 0f));
		Rectangle value = CalculateBoundingRectangle(new Rectangle(0, 0, texture2.Width, texture2.Height), matrix2);
		if (rectangle.Intersects(value))
		{
			Color[] array = new Color[texture1.Width * texture1.Height];
			texture1.GetData(array);
			Color[] array2 = new Color[texture2.Width * texture2.Height];
			texture2.GetData(array2);
			if (IntersectPixels(matrix, texture1.Width, texture1.Height, array, matrix2, texture2.Width, texture2.Height, array2))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool PerPixelCollision(Texture2D texture1, Vector2 position1, Vector2 origin1, float scale1, float rotation1, Texture2D texture2, Vector2 position2, Vector2 origin2, float scale2, float rotation2)
	{
		if (texture1 == null || texture2 == null)
		{
			return false;
		}
		Matrix matrix = Matrix.CreateTranslation(new Vector3(-origin1, 0f)) * Matrix.CreateScale(scale1) * Matrix.CreateRotationZ(rotation1) * Matrix.CreateTranslation(new Vector3(position1, 0f));
		Rectangle rectangle = CalculateBoundingRectangle(new Rectangle(0, 0, texture1.Width, texture1.Height), matrix);
		Matrix matrix2 = Matrix.CreateTranslation(new Vector3(-origin2, 0f)) * Matrix.CreateScale(scale2) * Matrix.CreateRotationZ(rotation2) * Matrix.CreateTranslation(new Vector3(position2, 0f));
		Rectangle value = CalculateBoundingRectangle(new Rectangle(0, 0, texture2.Width, texture2.Height), matrix2);
		if (rectangle.Intersects(value))
		{
			Color[] array = new Color[texture1.Width * texture1.Height];
			texture1.GetData(array);
			Color[] array2 = new Color[texture2.Width * texture2.Height];
			texture2.GetData(array2);
			if (IntersectPixels(matrix, texture1.Width, texture1.Height, array, matrix2, texture2.Width, texture2.Height, array2))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool PerPixelCollision(Texture2D texture1, Vector2 position1, Texture2D texture2, Vector2 position2)
	{
		if (texture1 == null || texture2 == null)
		{
			return false;
		}
		Matrix transformA = Matrix.CreateTranslation(new Vector3(position1, 0f));
		Rectangle rectangle = new Rectangle((int)position1.X, (int)position1.Y, texture1.Width, texture1.Height);
		Matrix transformB = Matrix.CreateTranslation(new Vector3(position2, 0f));
		Rectangle value = new Rectangle((int)position2.X, (int)position2.Y, texture2.Width, texture2.Height);
		if (rectangle.Intersects(value))
		{
			Color[] array = new Color[texture1.Width * texture1.Height];
			texture1.GetData(array);
			Color[] array2 = new Color[texture2.Width * texture2.Height];
			texture2.GetData(array2);
			if (IntersectPixels(transformA, texture1.Width, texture1.Height, array, transformB, texture2.Width, texture2.Height, array2))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private static Rectangle CalculateBoundingRectangle(Rectangle rectangle, Matrix transform)
	{
		Vector2 position = new Vector2(rectangle.Left, rectangle.Top);
		Vector2 position2 = new Vector2(rectangle.Right, rectangle.Top);
		Vector2 position3 = new Vector2(rectangle.Left, rectangle.Bottom);
		Vector2 position4 = new Vector2(rectangle.Right, rectangle.Bottom);
		Vector2.Transform(ref position, ref transform, out position);
		Vector2.Transform(ref position2, ref transform, out position2);
		Vector2.Transform(ref position3, ref transform, out position3);
		Vector2.Transform(ref position4, ref transform, out position4);
		Vector2 vector = Vector2.Min(Vector2.Min(position, position2), Vector2.Min(position3, position4));
		Vector2 vector2 = Vector2.Max(Vector2.Max(position, position2), Vector2.Max(position3, position4));
		return new Rectangle((int)vector.X, (int)vector.Y, (int)(vector2.X - vector.X), (int)(vector2.Y - vector.Y));
	}

	private static bool IntersectPixels(Matrix transformA, int widthA, int heightA, Color[] dataA, Matrix transformB, int widthB, int heightB, Color[] dataB)
	{
		Matrix matrix = transformA * Matrix.Invert(transformB);
		Vector2 vector = Vector2.TransformNormal(Vector2.UnitX, matrix);
		Vector2 vector2 = Vector2.TransformNormal(Vector2.UnitY, matrix);
		Vector2 vector3 = Vector2.Transform(Vector2.Zero, matrix);
		for (int i = 0; i < heightA; i++)
		{
			Vector2 vector4 = vector3;
			for (int j = 0; j < widthA; j++)
			{
				int num = (int)Math.Round(vector4.X);
				int num2 = (int)Math.Round(vector4.Y);
				if (0 <= num && num < widthB && 0 <= num2 && num2 < heightB)
				{
					Color color = dataA[j + i * widthA];
					Color color2 = dataB[num + num2 * widthB];
					if (color.A != 0 && color2.A != 0)
					{
						return true;
					}
				}
				vector4 += vector;
			}
			vector3 += vector2;
		}
		return false;
	}

	public static float DistanceToPoint(Vector2 position1, Vector2 position2)
	{
		return (float)Math.Sqrt((position1.X - position2.X) * (position1.X - position2.X) + (position1.Y - position2.Y) * (position1.Y - position2.Y));
	}

	public static float FindBearingFromVector(Vector2 velocity)
	{
		return MathHelper.ToDegrees((float)Math.Atan2(velocity.Y, velocity.X));
	}

	public static float MagnitudeInDirection(Vector2 velocity, float angle)
	{
		Vector2 vector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
		vector *= Vector2.Normalize(velocity);
		vector *= velocity.Length();
		return vector.X / (float)Math.Cos(Math.Atan(vector.Y / vector.X));
	}

	public static Vector2 ReflectedAngle(Vector2 velocity, Vector2 wallNormal)
	{
		return velocity - wallNormal * 2f * Vector2.Dot(wallNormal, velocity);
	}
}
