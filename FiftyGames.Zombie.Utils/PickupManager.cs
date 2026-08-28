using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FiftyGames.Zombie.Pickups;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Utils;

internal class PickupManager
{
	private List<Pickup> _pickups = new List<Pickup>();

	private List<PickupNode> _pickupPoints = new List<PickupNode>();

	private List<Pickup> _possiblePickups = new List<Pickup>();

	private List<bool> _nodesAvailible = new List<bool>();

	private double _lastSecondTime;

	private Random _rand = new Random();

	private long _elapsedTime;

	private int _spawnInterval;

	private int _maxPickups = 10;

	public List<Pickup> PossiblePickups
	{
		get
		{
			return _possiblePickups;
		}
		set
		{
			_possiblePickups = value;
		}
	}

	public List<bool> AvailibleNodes => _nodesAvailible;

	public int SpawnInterval
	{
		get
		{
			return _spawnInterval;
		}
		set
		{
			_spawnInterval = value;
		}
	}

	public int MaxPickups
	{
		get
		{
			return _maxPickups;
		}
		set
		{
			_maxPickups = value;
		}
	}

	public void AddPickup(Pickup pickup)
	{
		_pickups.Add(pickup);
	}

	public void LoadPickupNodes()
	{
		StreamReader streamReader = new StreamReader("Content/Zombie/Data/latest.pick");
		BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
		int num = binaryReader.ReadInt32();
		_pickupPoints.Clear();
		_nodesAvailible.Clear();
		for (int i = 0; i < num; i++)
		{
			PickupNode item = new PickupNode
			{
				_count = binaryReader.ReadInt32(),
				_pickupIndex = binaryReader.ReadInt32(),
				_position = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle()),
				_probability = binaryReader.ReadInt32(),
				_spawnProbability = binaryReader.ReadInt32()
			};
			_pickupPoints.Add(item);
		}
		for (int j = 0; j < _pickupPoints.Count; j++)
		{
			_nodesAvailible.Add(item: true);
		}
	}

	public void RemoveAll()
	{
		for (int i = 0; i < _pickups.Count; i++)
		{
			_pickups[i].Destory();
		}
		_pickups.Clear();
	}

	public bool AreAllPlayersAwayFromNode(Vector2 position)
	{
		Console.WriteLine("");
		for (int i = 0; i < ZombieUtils.Players.Count; i++)
		{
			float num = Vector2.Distance(ZombieUtils.Players[i].Position, position);
			Console.WriteLine((object)num);
			if (num < (float)ZombieUtils.SpawnDistance)
			{
				return false;
			}
		}
		return true;
	}

	public void Update(GameTime gameTime)
	{
		_elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
		if (_elapsedTime % 2 == 0 && _lastSecondTime != (double)gameTime.TotalGameTime.Seconds && _pickups.Count < _pickupPoints.Count)
		{
			_lastSecondTime = gameTime.TotalGameTime.Seconds;
			List<int> list = new List<int>();
			for (int i = 0; i < _pickupPoints.Count; i++)
			{
				list.Add(i);
			}
			Helper.Shuffle(list, _rand);
			for (int j = 0; j < list.Count; j++)
			{
				if (_nodesAvailible[list[j]] && AreAllPlayersAwayFromNode(_pickupPoints[list[j]]._position) && _pickups.Count < _maxPickups)
				{
					int num = list[j];
					int num2 = _rand.Next(1, 101);
					int num3 = 0;
					int num4 = -1;
					do
					{
						num4++;
						num3 += _possiblePickups[num4].ProbabilityOfSpawn;
					}
					while (num2 > num3);
					num2 = num4;
					_possiblePickups[num2].GetType();
					ConstructorInfo constructorInfo = _possiblePickups[num2].GetType().GetConstructors()[0];
					_pickups.Add((Pickup)constructorInfo.Invoke(new object[5]
					{
						this,
						_pickupPoints[num]._position,
						num,
						_possiblePickups[num2].NumberSupplied,
						false
					}));
					_nodesAvailible[num] = false;
					break;
				}
			}
		}
		for (int k = 0; k < _pickups.Count; k++)
		{
			if (_pickups[k].PickedUp)
			{
				_pickups.RemoveAt(k);
				k--;
			}
		}
	}

	public void Draw()
	{
		foreach (Pickup pickup in _pickups)
		{
			pickup.Draw();
		}
	}
}
