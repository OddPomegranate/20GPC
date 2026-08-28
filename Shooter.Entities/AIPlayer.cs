using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using FiftyGames.ShooterGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Guns;
using Shooter.ISHelpers;
using Shooter.Pickups;
using Shooter.Pickups.Items;

namespace Shooter.Entities;

internal class AIPlayer : ShooterPlayer
{
	private const int _maxMapDistance = 2202;

	private AIMode _mode;

	private List<ShooterPlayer> _humanPlayers;

	private List<ShooterPlayer> _aiPlayers;

	public Vector2 _heading;

	public ShooterPlayer _closestPlayer;

	public bool _needsNode;

	public bool _needsPickupNode;

	private AIScore[] _aiScores;

	private AIScore _targetScores;

	private SpriteFont _font;

	private List<PhysObject> _pickups;

	private List<Vector2> _currentPath;

	private Vector2 _lastTimelyPosition;

	private float _timelyUpdateMills;

	private float _roamMills;

	private Vector2 _roamTarget;

	private Vector2 _navMeshDestination;

	private Pickup _destinationPickup;

	private float fov = 10f;

	private float fovDist = 300f;

	private float speed = 50f;

	private int roamTime = 3000;

	private int timelyUpdateTime = 1000;

	private float roamReachedTarget = 50f;

	private float canSeePlayerMinDistance = 200f;

	public AIPlayer(int id, World world, Random random, ContentManager contentManager, NavMesh navMesh, List<ShooterPlayer> allPlayers, List<ShooterPlayer> humanPlayers, List<ShooterPlayer> aiPlayers, List<GunSettings> gunSettings, List<PhysObject> pickups, RenderTarget2D ammoHealthRT)
		: base(id, world, random, contentManager, navMesh, allPlayers, gunSettings, ammoHealthRT)
	{
		_mode = AIMode.ChasingTarget;
		_humanPlayers = humanPlayers;
		_aiPlayers = aiPlayers;
		_color = Color.White;
		_aiScores = new AIScore[100];
		_font = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_pickups = pickups;
		_lastTimelyPosition = Vector2.Zero;
		_timelyUpdateMills = 0f;
		_roamMills = 0f;
		_roamTarget = Vector2.Zero;
		_navMeshDestination = Vector2.Zero;
		fov = _random.Next(10, 20);
		fov = MathHelper.ToRadians(0f - fov);
		fovDist = _random.Next(250, 450);
	}

	public override void Update(GameTime gameTime)
	{
		bool flag = false;
		List<ShooterPlayer> playersInFOV = GetPlayersInFOV();
		for (int i = 0; i < _allPlayers.Count; i++)
		{
			if (_allPlayers[i] != this && _allPlayers[i].IsAlive)
			{
				ref AIScore reference = ref _aiScores[i];
				reference = EvaluatePlayer(_allPlayers[i], playersInFOV);
			}
		}
		int num = 0;
		int num2 = -1;
		for (int j = 0; j < _allPlayers.Count; j++)
		{
			if (_aiScores[j].score > num2)
			{
				num2 = _aiScores[j].score;
				num = j;
			}
		}
		_targetScores = _aiScores[num];
		ShooterPlayer shooterPlayer = _allPlayers[num];
		if (!shooterPlayer.IsAlive && _mode != AIMode.InitRoam && _mode != AIMode.Roam)
		{
			_mode = AIMode.InitRoam;
		}
		_closestPlayer = shooterPlayer;
		for (int k = 0; k < _allPlayers.Count; k++)
		{
			if (_allPlayers[k] != this)
			{
				float num3 = Vector2.Distance(_allPlayers[k].DisplayPosition, base.DisplayPosition);
				if (num3 - 50f < 50f)
				{
					MoveTowardsPoint(_allPlayers[k].DisplayPosition, speed, -1f);
				}
			}
		}
		if (_isAlive)
		{
			switch (_mode)
			{
			case AIMode.SearchAmmo:
				_navMeshDestination = GetClosestPickupPosition(typeof(Ammo), out _destinationPickup);
				if (_navMeshDestination == Vector2.Zero)
				{
					_mode = AIMode.InitRoam;
				}
				else
				{
					_mode = AIMode.MovingToAmmo;
				}
				break;
			case AIMode.SearchHealth:
				_navMeshDestination = GetClosestPickupPosition(typeof(Health), out _destinationPickup);
				if (_navMeshDestination == Vector2.Zero)
				{
					_mode = AIMode.InitRoam;
				}
				else
				{
					_mode = AIMode.MovingToHealth;
				}
				break;
			case AIMode.MovingToAmmo:
			{
				float num7 = Vector2.Distance(_destinationPickup.DisplayPosition, base.DisplayPosition);
				if (num7 > 50f)
				{
					MoveTowardsPoint(_heading, speed);
					base.Body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, _heading, base.Body.Rotation, 0.1f);
				}
				if (!_destinationPickup.IsActive())
				{
					_mode = AIMode.SearchAmmo;
				}
				break;
			}
			case AIMode.MovingToHealth:
			{
				float num8 = Vector2.Distance(_destinationPickup.DisplayPosition, base.DisplayPosition);
				if (num8 > 50f)
				{
					MoveTowardsPoint(_heading, speed);
					base.Body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, _heading, base.Body.Rotation, 0.1f);
				}
				if (!_destinationPickup.IsActive())
				{
					_mode = AIMode.SearchHealth;
				}
				break;
			}
			case AIMode.ChasingTarget:
				if (_aiScores[num].canSeeMultiplier == 2)
				{
					base.Body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, shooterPlayer.DisplayPosition, base.Body.Rotation, 0.1f);
					float num5 = Vector2.Distance(shooterPlayer.DisplayPosition, base.DisplayPosition);
					if (num5 - 50f > canSeePlayerMinDistance)
					{
						MoveTowardsPoint(shooterPlayer.DisplayPosition, speed);
					}
				}
				else
				{
					_navMeshDestination = shooterPlayer.DisplayPosition;
					base.Body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, _heading, base.Body.Rotation, 0.1f);
					float num6 = Vector2.Distance(shooterPlayer.DisplayPosition, base.DisplayPosition);
					if (num6 - 50f > 100f)
					{
						MoveTowardsPoint(_heading, speed);
					}
				}
				if (_health < 50)
				{
					_mode = AIMode.SearchHealth;
				}
				else if (_currentGun.GetAmmoRemaining() <= 0)
				{
					_mode = AIMode.SearchAmmo;
				}
				break;
			case AIMode.InitRoam:
				_roamTarget = _navMesh.LineMesh.MeshNodes[_random.Next(0, _navMesh.LineMesh.MeshNodes.Count)]._position;
				_roamMills = 0f;
				_mode = AIMode.Roam;
				break;
			case AIMode.Roam:
			{
				_roamMills += gameTime.ElapsedGameTime.Milliseconds;
				if (_roamMills >= (float)roamTime || (playersInFOV.Count > 0 && _roamMills > 1000f))
				{
					_mode = AIMode.ChasingTarget;
					break;
				}
				float num4 = Vector2.Distance(base.DisplayPosition, _roamTarget);
				if (num4 < roamReachedTarget)
				{
					_mode = AIMode.InitRoam;
					break;
				}
				_navMeshDestination = _roamTarget;
				MoveTowardsPoint(_heading, speed);
				base.Body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, _heading, base.Body.Rotation, 0.1f);
				break;
			}
			}
			if (playersInFOV.Contains(shooterPlayer))
			{
				base.Body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, shooterPlayer.DisplayPosition, base.Body.Rotation, 0.1f);
				_lastShotPath = _currentGun.Settings.SoundEffectPath;
				if (_currentGun.Shoot(GeometryHelper.AngleToV2(base.Body.Rotation, 1f), _random, this))
				{
					if (_lastShotPath == "Laser")
					{
						if (!_hasJustShot)
						{
							_lastShotCuePlayed = ShooterGame.PlayCue("Shoot " + _lastShotPath);
						}
					}
					else
					{
						_lastShotCuePlayed = ShooterGame.PlayCue("Shoot " + _lastShotPath);
					}
					_hasJustShot = true;
					flag = true;
				}
			}
			_timelyUpdateMills += gameTime.ElapsedGameTime.Milliseconds;
			if (_timelyUpdateMills >= (float)timelyUpdateTime)
			{
				float num9 = Vector2.Distance(base.DisplayPosition, _lastTimelyPosition);
				if (num9 < 5f)
				{
					_mode = AIMode.InitRoam;
				}
				_lastTimelyPosition = base.DisplayPosition;
				_timelyUpdateMills = 0f;
			}
			_lastLookAngle = base.Body.Rotation;
		}
		if (!flag && _lastShotPath == "Laser" && _hasJustShot)
		{
			if (!_lastShotCuePlayed.IsDisposed)
			{
				_lastShotCuePlayed.Stop(AudioStopOptions.AsAuthored);
			}
			ShooterGame.PlayCue("End Laser");
			_hasJustShot = false;
		}
		base.Update(gameTime);
	}

	public void ThreadWork()
	{
		List<Vector2> path = new List<Vector2>();
		_heading = GetWaypointToDestination(_navMeshDestination, out path);
		SetCurrentPath(path);
	}

	public override void OnRespawn()
	{
		_mode = AIMode.ChasingTarget;
		base.OnRespawn();
	}

	public override void OnAmmoPickedUp()
	{
		_mode = AIMode.ChasingTarget;
		base.OnAmmoPickedUp();
	}

	public override void OnHealthPickedUp()
	{
		_mode = AIMode.ChasingTarget;
		base.OnHealthPickedUp();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
	}

	public void DrawDebug(SpriteBatch spriteBatch)
	{
		Vector2 vector = new Vector2(base.DisplayPosition.X * 1280f / 1920f, base.DisplayPosition.Y * 720f / 1080f);
		spriteBatch.Begin();
		spriteBatch.DrawString(_font, "NumInFOV: " + GetPlayersInFOV().Count, vector - new Vector2(0f, -10f), Color.White);
		spriteBatch.DrawString(_font, "MODE: " + _mode, vector - new Vector2(0f, -20f), Color.White);
		spriteBatch.End();
		List<VertexPositionColor> list = new List<VertexPositionColor>();
		if (_currentPath != null)
		{
			lock (_currentPath)
			{
				for (int i = 0; i < _currentPath.Count - 1; i++)
				{
					list.Add(new VertexPositionColor(new Vector3(_currentPath[i], 0f), _color));
					list.Add(new VertexPositionColor(new Vector3(_currentPath[i + 1], 0f), _color));
				}
				GeometryHelper.LineRenderer.DrawShape(list.ToArray(), Vector2.Zero);
			}
		}
		List<VertexPositionColor> list2 = new List<VertexPositionColor>();
		Vector2 vector2 = GeometryHelper.AngleToV2(base.Rotation + fov / 2f, 1f);
		vector2.Normalize();
		Vector2 vector3 = GeometryHelper.AngleToV2(base.Rotation - fov / 2f, 1f);
		vector3.Normalize();
		Vector2.Transform(vector2, Matrix.CreateRotationZ(base.Rotation));
		list2.Add(new VertexPositionColor(new Vector3(base.DisplayPosition, 0f), Color.Red));
		list2.Add(new VertexPositionColor(new Vector3(base.DisplayPosition + vector2 * 300f, 0f), Color.Red));
		list2.Add(new VertexPositionColor(new Vector3(base.DisplayPosition, 0f), Color.Red));
		list2.Add(new VertexPositionColor(new Vector3(base.DisplayPosition + vector3 * 300f, 0f), Color.Red));
		GeometryHelper.LineRenderer.DrawShape(list2.ToArray(), Vector2.Zero);
	}

	public override void CreateBody(Vector2 position)
	{
		base.CreateBody(position);
	}

	private AIScore EvaluatePlayer(ShooterPlayer player, List<ShooterPlayer> fovPlayers)
	{
		AIScore result = new AIScore
		{
			hundredMinusHealth = 100 - player.GetHealth(),
			pickedOnMe = _damageByPlayers[player.GetID()],
			maxDistMinusDist = 2202 - (int)Vector2.Distance(base.DisplayPosition, player.DisplayPosition)
		};
		if (!IsRayCollisionToOtherPlayer(player))
		{
			result.canSeeMultiplier = 2;
		}
		else
		{
			result.canSeeMultiplier = 1;
		}
		result.score = (result.hundredMinusHealth + result.pickedOnMe + result.maxDistMinusDist) * result.canSeeMultiplier;
		return result;
	}

	private List<ShooterPlayer> GetPlayersInFOV()
	{
		List<ShooterPlayer> list = new List<ShooterPlayer>();
		List<Line> fOV = GetFOV();
		foreach (ShooterPlayer allPlayer in _allPlayers)
		{
			if (allPlayer != this && GeometryHelper.PointInPolygon(allPlayer.DisplayPosition, fOV))
			{
				list.Add(allPlayer);
			}
		}
		return list;
	}

	private List<Line> GetFOV()
	{
		List<Line> list = new List<Line>();
		Vector2 vector = GeometryHelper.AngleToV2(base.Rotation + fov / 2f, 1f);
		vector.Normalize();
		Vector2 vector2 = GeometryHelper.AngleToV2(base.Rotation - fov / 2f, 1f);
		vector2.Normalize();
		Vector2 displayPosition = base.DisplayPosition;
		Vector2 vector3 = base.DisplayPosition + vector * fovDist;
		Vector2 vector4 = base.DisplayPosition + vector2 * fovDist;
		Line line = new Line();
		line.Start = displayPosition;
		line.End = vector3;
		list.Add(line);
		Line line2 = new Line();
		line2.Start = vector3;
		line2.End = vector4;
		list.Add(line2);
		Line line3 = new Line();
		line3.Start = vector4;
		line3.End = displayPosition;
		list.Add(line3);
		return list;
	}

	private Vector2 GetClosestPickupPosition(Type pickupType, out Pickup result)
	{
		result = null;
		foreach (Pickup pickup6 in _pickups)
		{
			if ((object)pickup6.GetType() == pickupType)
			{
				pickup6.TempDistanceFromPlayer = Vector2.Distance(pickup6.DisplayPosition, base.DisplayPosition);
			}
			else
			{
				pickup6.TempDistanceFromPlayer = 100000f;
			}
		}
		_pickups.Sort();
		foreach (Pickup pickup7 in _pickups)
		{
			foreach (ShooterPlayer allPlayer in _allPlayers)
			{
				if (allPlayer != this && Vector2.Distance(allPlayer.DisplayPosition, pickup7.DisplayPosition) < 100f)
				{
					pickup7.HasPlayersAroundMe = true;
					break;
				}
			}
		}
		foreach (Pickup pickup8 in _pickups)
		{
			if ((object)pickup8.GetType() == pickupType && !pickup8.HasPlayersAroundMe && pickup8.IsActive())
			{
				result = pickup8;
				break;
			}
		}
		if (result != null)
		{
			return result.DisplayPosition;
		}
		foreach (Pickup pickup9 in _pickups)
		{
			if ((object)pickup9.GetType() == pickupType && pickup9.IsActive())
			{
				result = pickup9;
				return pickup9.DisplayPosition;
			}
		}
		foreach (Pickup pickup10 in _pickups)
		{
			if ((object)pickup10.GetType() == pickupType)
			{
				result = pickup10;
				return pickup10.DisplayPosition;
			}
		}
		return Vector2.Zero;
	}

	private Vector2 GetClosestHealthPickupPosition()
	{
		Health health = null;
		float num = 1000000f;
		foreach (Pickup pickup in _pickups)
		{
			if (pickup is Health health2)
			{
				float num2 = Vector2.Distance(health2.DisplayPosition, base.DisplayPosition);
				if (num2 < num)
				{
					health = health2;
					num = num2;
				}
			}
		}
		return health?.DisplayPosition ?? Vector2.Zero;
	}

	public void SetCurrentPath(List<Vector2> path)
	{
		_currentPath = path;
	}
}
