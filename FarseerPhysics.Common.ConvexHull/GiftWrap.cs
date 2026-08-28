using System;

namespace FarseerPhysics.Common.ConvexHull;

public static class GiftWrap
{
	public static Vertices GetConvexHull(Vertices vertices)
	{
		if (vertices.Count < 3)
		{
			return vertices;
		}
		int[] array = new int[vertices.Count];
		int num = 0;
		float num2 = float.MaxValue;
		int num3 = vertices.Count;
		for (int i = 0; i < vertices.Count; i++)
		{
			if (vertices[i].Y < num2)
			{
				num2 = vertices[i].Y;
				num3 = i;
			}
		}
		int num4 = num3;
		int num5 = -1;
		float num6 = -1f;
		float num7 = 0f;
		while (num5 != num3)
		{
			float num8 = -2f;
			float num11;
			for (int j = 0; j < vertices.Count; j++)
			{
				if (j != num4)
				{
					float num9 = vertices[j].X - vertices[num4].X;
					float num10 = vertices[j].Y - vertices[num4].Y;
					num11 = (float)Math.Sqrt(num9 * num9 + num10 * num10);
					num11 = ((num11 == 0f) ? 1f : num11);
					num9 /= num11;
					num10 /= num11;
					float num12 = num9 * num6 + num10 * num7;
					if (num12 > num8)
					{
						num8 = num12;
						num5 = j;
					}
				}
			}
			array[num++] = num5;
			num6 = vertices[num5].X - vertices[num4].X;
			num7 = vertices[num5].Y - vertices[num4].Y;
			num11 = (float)Math.Sqrt(num6 * num6 + num7 * num7);
			num11 = ((num11 == 0f) ? 1f : num11);
			num6 /= num11;
			num7 /= num11;
			num4 = num5;
		}
		Vertices vertices2 = new Vertices(num);
		for (int k = 0; k < num; k++)
		{
			vertices2.Add(vertices[array[k]]);
		}
		return vertices2;
	}
}
