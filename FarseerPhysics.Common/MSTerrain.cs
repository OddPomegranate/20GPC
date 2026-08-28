using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Common;

public class MSTerrain
{
	public World World;

	public Vector2 Center;

	public float Width;

	public float Height;

	public int PointsPerUnit;

	public int CellSize;

	public int SubCellSize;

	public int Iterations = 2;

	public Decomposer Decomposer;

	private sbyte[,] _terrainMap;

	private List<Body>[,] _bodyMap;

	private float _localWidth;

	private float _localHeight;

	private int _xnum;

	private int _ynum;

	private AABB _dirtyArea;

	private Vector2 _topLeft;

	public MSTerrain(World world, AABB area)
	{
		World = world;
		Width = area.Extents.X * 2f;
		Height = area.Extents.Y * 2f;
		Center = area.Center;
	}

	public void Initialize()
	{
		_topLeft = new Vector2(Center.X - Width * 0.5f, Center.Y - (0f - Height) * 0.5f);
		_localWidth = Width * (float)PointsPerUnit;
		_localHeight = Height * (float)PointsPerUnit;
		_terrainMap = new sbyte[(int)_localWidth + 1, (int)_localHeight + 1];
		for (int i = 0; (float)i < _localWidth; i++)
		{
			for (int j = 0; (float)j < _localHeight; j++)
			{
				_terrainMap[i, j] = 1;
			}
		}
		_xnum = (int)(_localWidth / (float)CellSize);
		_ynum = (int)(_localHeight / (float)CellSize);
		_bodyMap = new List<Body>[_xnum, _ynum];
		_dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
	}

	public void ApplyTexture(Texture2D texture, Vector2 position, TerrainTester tester)
	{
		Color[] array = new Color[texture.Width * texture.Height];
		texture.GetData(array);
		for (int i = (int)position.Y; i < texture.Height + (int)position.Y; i++)
		{
			for (int j = (int)position.X; j < texture.Width + (int)position.X; j++)
			{
				if (j >= 0 && (float)j < _localWidth && i >= 0 && (float)i < _localHeight)
				{
					if (!tester(array[(i - (int)position.Y) * texture.Width + (j - (int)position.X)]))
					{
						_terrainMap[j, i] = 1;
					}
					else
					{
						_terrainMap[j, i] = -1;
					}
				}
			}
		}
		for (int k = 0; k < _ynum; k++)
		{
			for (int l = 0; l < _xnum; l++)
			{
				if (_bodyMap[l, k] != null)
				{
					for (int m = 0; m < _bodyMap[l, k].Count; m++)
					{
						World.RemoveBody(_bodyMap[l, k][m]);
					}
				}
				_bodyMap[l, k] = null;
				GenerateTerrain(l, k);
			}
		}
	}

	public void ApplyData(sbyte[,] data, Vector2 position)
	{
		for (int i = (int)position.Y; i < data.GetUpperBound(1) + (int)position.Y; i++)
		{
			for (int j = (int)position.X; j < data.GetUpperBound(0) + (int)position.X; j++)
			{
				if (j >= 0 && (float)j < _localWidth && i >= 0 && (float)i < _localHeight)
				{
					_terrainMap[j, i] = data[j, i];
				}
			}
		}
		for (int k = 0; k < _ynum; k++)
		{
			for (int l = 0; l < _xnum; l++)
			{
				if (_bodyMap[l, k] != null)
				{
					for (int m = 0; m < _bodyMap[l, k].Count; m++)
					{
						World.RemoveBody(_bodyMap[l, k][m]);
					}
				}
				_bodyMap[l, k] = null;
				GenerateTerrain(l, k);
			}
		}
	}

	public static sbyte[,] ConvertTextureToData(Texture2D texture, TerrainTester tester)
	{
		sbyte[,] array = new sbyte[texture.Width, texture.Height];
		Color[] array2 = new Color[texture.Width * texture.Height];
		texture.GetData(array2);
		for (int i = 0; i < texture.Height; i++)
		{
			for (int j = 0; j < texture.Width; j++)
			{
				if (!tester(array2[i * texture.Width + j]))
				{
					array[j, i] = 1;
				}
				else
				{
					array[j, i] = -1;
				}
			}
		}
		return array;
	}

	public void ModifyTerrain(Vector2 location, sbyte value)
	{
		Vector2 vector = location - _topLeft;
		vector.X = vector.X * _localWidth / Width;
		vector.Y = vector.Y * (0f - _localHeight) / Height;
		if (vector.X >= 0f && vector.X < _localWidth && vector.Y >= 0f && vector.Y < _localHeight)
		{
			_terrainMap[(int)vector.X, (int)vector.Y] = value;
			if (vector.X < _dirtyArea.LowerBound.X)
			{
				_dirtyArea.LowerBound.X = vector.X;
			}
			if (vector.X > _dirtyArea.UpperBound.X)
			{
				_dirtyArea.UpperBound.X = vector.X;
			}
			if (vector.Y < _dirtyArea.LowerBound.Y)
			{
				_dirtyArea.LowerBound.Y = vector.Y;
			}
			if (vector.Y > _dirtyArea.UpperBound.Y)
			{
				_dirtyArea.UpperBound.Y = vector.Y;
			}
		}
	}

	public void RegenerateTerrain()
	{
		int num = (int)(_dirtyArea.LowerBound.X / (float)CellSize);
		int num2 = (int)(_dirtyArea.UpperBound.X / (float)CellSize) + 1;
		if (num < 0)
		{
			num = 0;
		}
		if (num2 > _xnum)
		{
			num2 = _xnum;
		}
		int num3 = (int)(_dirtyArea.LowerBound.Y / (float)CellSize);
		int num4 = (int)(_dirtyArea.UpperBound.Y / (float)CellSize) + 1;
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num4 > _ynum)
		{
			num4 = _ynum;
		}
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				if (_bodyMap[i, j] != null)
				{
					for (int k = 0; k < _bodyMap[i, j].Count; k++)
					{
						World.RemoveBody(_bodyMap[i, j][k]);
					}
				}
				_bodyMap[i, j] = null;
				GenerateTerrain(i, j);
			}
		}
		_dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
	}

	private void GenerateTerrain(int gx, int gy)
	{
		float num = gx * CellSize;
		float num2 = gy * CellSize;
		List<Vertices> list = MarchingSquares.DetectSquares(new AABB(new Vector2(num, num2), new Vector2(num + (float)CellSize, num2 + (float)CellSize)), SubCellSize, SubCellSize, _terrainMap, Iterations, combine: true);
		if (list.Count == 0)
		{
			return;
		}
		_bodyMap[gx, gy] = new List<Body>();
		Vector2 value = new Vector2(1f / (float)PointsPerUnit, 1f / (float)(-PointsPerUnit));
		foreach (Vertices item in list)
		{
			item.Scale(ref value);
			item.Translate(ref _topLeft);
			item.ForceCounterClockWise();
			Vertices vertices = SimplifyTools.CollinearSimplify(item);
			List<Vertices> list2 = new List<Vertices>();
			switch (Decomposer)
			{
			case Decomposer.Bayazit:
				list2 = BayazitDecomposer.ConvexPartition(vertices);
				break;
			case Decomposer.CDT:
				list2 = CDTDecomposer.ConvexPartition(vertices);
				break;
			case Decomposer.Earclip:
				list2 = EarclipDecomposer.ConvexPartition(vertices);
				break;
			case Decomposer.Flipcode:
				list2 = FlipcodeDecomposer.ConvexPartition(vertices);
				break;
			case Decomposer.Seidel:
				list2 = SeidelDecomposer.ConvexPartition(vertices, 0.001f);
				break;
			}
			foreach (Vertices item2 in list2)
			{
				if (item2.Count > 2)
				{
					_bodyMap[gx, gy].Add(BodyFactory.CreatePolygon(World, item2, 1f));
				}
			}
		}
	}
}
