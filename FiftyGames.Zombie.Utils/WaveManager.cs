using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie.Utils;

internal static class WaveManager
{
	private static Queue<Wave> _waves;

	private static bool _hasStarted = false;

	private static double _startTime;

	private static Wave _currentWave;

	private static bool _hasReachedBadguyLimit = false;

	private static PickupManager _pickupManager;

	private static Queue<BadGuy> _badGuyQueue;

	private static List<BadGuy> _lastBadGuyFromNode;

	private static int _numberOfStartingWaves;

	private static double _elapsedTime;

	private static int _totalAddedWaves = 0;

	private static bool _hasAdvanced = false;

	private static WaveState _waveState = WaveState.Running;

	private static int _countdown = 1000;

	private static int _waveCount = 0;

	public static bool Completed
	{
		get
		{
			if (_waves.Count <= 0 && _currentWave.BadGuysLeft <= 0)
			{
				return true;
			}
			return false;
		}
	}

	public static bool HasReachedBadguyLimit => _hasReachedBadguyLimit;

	public static PickupManager PickupManager => _pickupManager;

	public static int TimeUntilNextWave => 0;

	public static Queue<BadGuy> BadguyQueue => _badGuyQueue;

	public static int CurrentWave => _waveCount;

	public static OnNewWaveDelegate OnNewWave { get; set; }

	public static OnWaveStart OnStart { get; set; }

	public static int Countdown => _countdown;

	public static void Init()
	{
		if (_pickupManager != null)
		{
			_pickupManager.RemoveAll();
		}
		if (_waves != null)
		{
			for (int i = 0; i < _waves.Count; i++)
			{
				_waves.ElementAt(i).EmptyBadGuyQueue();
			}
		}
		ZombieUtils.RemoveAllBadGuys();
		_waves = new Queue<Wave>();
		_pickupManager = new PickupManager();
		_badGuyQueue = new Queue<BadGuy>();
		_lastBadGuyFromNode = new List<BadGuy>();
		_pickupManager.LoadPickupNodes();
		for (int j = 0; j < _pickupManager.AvailibleNodes.Count; j++)
		{
			_lastBadGuyFromNode.Add(null);
		}
		_hasStarted = false;
		_hasAdvanced = false;
		_totalAddedWaves = 0;
		_waveState = WaveState.Running;
		if (OnStart != null)
		{
			OnStart();
		}
		_countdown = 1000;
		_waveCount = 0;
		if (ZombieUtils.BadGuys != null)
		{
			for (int k = 0; k < ZombieUtils.BadGuys.Count; k++)
			{
				ZombieUtils.BadGuys[k].IsAlive = false;
				ZombieUtils.BadGuys[k].Health = 0f;
				ZombieUtils.BadGuys[k].Update();
			}
			ZombieUtils.BadGuys.Clear();
		}
	}

	public static void AddWave(Wave wave)
	{
		_waves.Enqueue(wave);
		_totalAddedWaves++;
	}

	public static void AdvanceWave()
	{
		if (_waves.Count > 0)
		{
			_currentWave = _waves.Dequeue();
			_hasAdvanced = true;
		}
		if (ZombieUtils.BadGuys != null)
		{
			for (int i = 0; i < ZombieUtils.BadGuys.Count; i++)
			{
				ZombieUtils.BadGuys[i].IsAlive = false;
				ZombieUtils.BadGuys[i].Health = 0f;
				ZombieUtils.BadGuys[i].Update();
			}
			ZombieUtils.BadGuys.Clear();
		}
	}

	public static void Update(GameTime gameTime, List<BadGuy> badguys)
	{
		_elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
		bool flag = false;
		if (!_hasStarted)
		{
			_numberOfStartingWaves = _waves.Count;
			if (!_hasAdvanced)
			{
				_currentWave = _waves.Dequeue();
			}
			_pickupManager.MaxPickups = _currentWave.MaxPickups;
			_currentWave.Start(gameTime, badguys);
			_startTime = gameTime.TotalGameTime.TotalSeconds;
			_hasStarted = true;
			_waveCount++;
			flag = true;
		}
		if (_waveState == WaveState.Running)
		{
			if ((_elapsedTime > (double)_currentWave.DurationOfWave && !_currentWave.IgnoreDurationOfWave) || (badguys.Count == 0 && _currentWave.IgnoreDurationOfWave && !flag))
			{
				if (_waves.Count > 0)
				{
					if (OnNewWave != null)
					{
						OnNewWave();
					}
					_countdown = 1000;
					_waveCount++;
					_waveState = WaveState.Paused;
				}
			}
			else
			{
				_currentWave.Update(gameTime, badguys);
			}
		}
		if (badguys.Count > 0)
		{
			for (int i = 0; i < badguys.Count; i++)
			{
				if (!badguys[i].IsAlive)
				{
					badguys.RemoveAt(i);
					i--;
				}
			}
		}
		else if (_waves.Count <= 0)
		{
			_ = _currentWave.BadGuysLeft;
		}
		if (_waveState == WaveState.Paused)
		{
			_countdown -= gameTime.ElapsedGameTime.Milliseconds;
			if (_countdown < 0)
			{
				if (OnStart != null)
				{
					OnStart();
				}
				_currentWave = _waves.Dequeue();
				_pickupManager.MaxPickups = _currentWave.MaxPickups;
				_currentWave.Start(gameTime, badguys);
				_startTime = gameTime.TotalGameTime.TotalSeconds;
				_elapsedTime = 0.0;
				_hasReachedBadguyLimit = false;
				_waveState = WaveState.Running;
			}
		}
		if (_badGuyQueue.Count > 0)
		{
			List<int> list = new List<int>();
			for (int j = 0; j < ZombieUtils.NavMesh.LineMesh.SpecialNodes.Count; j++)
			{
				list.Add(j);
			}
			Helper.Shuffle(list, ZombieUtils.Random);
			List<int> list2 = new List<int>();
			for (int k = 0; k < ZombieUtils.NavMesh.LineMesh.SpecialNodes.Count; k++)
			{
				bool flag2 = false;
				if (_lastBadGuyFromNode[list[k]] != null)
				{
					BadGuy badGuy = _lastBadGuyFromNode[list[k]];
					float num = Vector2.Distance(badGuy.Position, ZombieUtils.NavMesh.LineMesh.MeshNodes[ZombieUtils.NavMesh.LineMesh.SpecialNodes[list[k]]]._position);
					if (num < 50f)
					{
						continue;
					}
				}
				for (int l = 0; l < ZombieUtils.Players.Count; l++)
				{
					float num2 = Vector2.Distance(ZombieUtils.Players[l].Position, ZombieUtils.NavMesh.LineMesh.MeshNodes[ZombieUtils.NavMesh.LineMesh.SpecialNodes[list[k]]]._position);
					if (num2 < (float)ZombieUtils.SpawnDistance)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					list2.Add(list[k]);
				}
			}
			for (int m = 0; m < list2.Count; m++)
			{
				if (_badGuyQueue.Count <= 0)
				{
					break;
				}
				BadGuy badGuy2 = _badGuyQueue.Dequeue();
				badGuy2.SetPositionFromSpawnNode(list2[m]);
				badGuy2.EnableBody();
				_lastBadGuyFromNode[list2[m]] = badGuy2;
				badguys.Add(badGuy2);
			}
		}
		_pickupManager.Update(gameTime);
	}

	public static void Draw()
	{
		_pickupManager.Draw();
	}
}
