using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FiftyGames.Zombie.Pickups;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Utils;

internal class Wave
{
	private List<BadGuy> _badguysInWave = new List<BadGuy>();

	private int _badGuysSpawned;

	private int _numberOfStartingZombies;

	private int _durationOfWave;

	private int _numberPerSpawn;

	private double _timeBetweenSpawn;

	private bool _waveFinished;

	private bool _ignoreDuration;

	private double _elapsedTime;

	private int _pickupIntervalTime;

	private List<Pickup> _possiblePickups;

	private int _maxPickups;

	private Dictionary<BadGuy, int> _dictionary;

	public int BadGuysLeft => _badguysInWave.Count - _badGuysSpawned;

	public int DurationOfWave => _durationOfWave;

	public bool IgnoreDurationOfWave => _ignoreDuration;

	public int NumOnSpawn => _numberOfStartingZombies;

	public int PickupIntervalTime => _pickupIntervalTime;

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

	public Wave(Dictionary<BadGuy, int> dictionary, int timeBetweenSpawns, int numberOfStartingZombies, int durationOfWave, bool ignoreDuration, int numberPerSpawn, List<Pickup> possiblePickups, int pickupIntervalTime, int maxPickups)
	{
		_ignoreDuration = ignoreDuration;
		_pickupIntervalTime = pickupIntervalTime;
		_possiblePickups = possiblePickups;
		_dictionary = dictionary;
		_timeBetweenSpawn = timeBetweenSpawns;
		_numberOfStartingZombies = numberOfStartingZombies;
		_durationOfWave = durationOfWave;
		_numberPerSpawn = numberPerSpawn;
		_maxPickups = maxPickups;
	}

	public void Start(GameTime gameTime, List<BadGuy> badguys)
	{
		WaveManager.PickupManager.PossiblePickups = _possiblePickups;
		WaveManager.PickupManager.SpawnInterval = _pickupIntervalTime;
		for (int i = 0; i < _dictionary.Count; i++)
		{
			KeyValuePair<BadGuy, int> keyValuePair = _dictionary.ElementAt(i);
			Type type = keyValuePair.Key.GetType();
			ConstructorInfo constructorInfo = type.GetConstructors()[0];
			for (int j = 0; j < keyValuePair.Value; j++)
			{
				_badguysInWave.Add((BadGuy)constructorInfo.Invoke(new object[0]));
			}
		}
		Helper.Shuffle(_badguysInWave, new Random());
		Spawn(badguys, _numberOfStartingZombies);
	}

	public void Update(GameTime gameTime, List<BadGuy> badguys)
	{
		_elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
		if (!_waveFinished && _elapsedTime % _timeBetweenSpawn == 0.0)
		{
			Spawn(badguys, _numberPerSpawn);
			_elapsedTime = 0.0;
		}
	}

	private void Spawn(List<BadGuy> badguys, int numberToSpawn)
	{
		for (int i = 0; i < numberToSpawn; i++)
		{
			if (_badGuysSpawned >= _badguysInWave.Count)
			{
				_waveFinished = true;
				break;
			}
			WaveManager.BadguyQueue.Enqueue(_badguysInWave[_badGuysSpawned]);
			_badGuysSpawned++;
		}
	}

	public void EmptyBadGuyQueue()
	{
		for (int i = 0; i < _badguysInWave.Count; i++)
		{
			_badguysInWave[i].Health = 0f;
			_badguysInWave[i].Update();
		}
	}
}
