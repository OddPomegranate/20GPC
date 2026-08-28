using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FiftyGames.Zombie.Pickups;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal abstract class BadGuy : Entity
{
	protected Texture2D _sprite;

	protected Circle _collisionCircle;

	protected List<Line> _collisionCircleLines = new List<Line>();

	protected int _currentNodeIndex;

	protected int _collisionCircleRadius;

	protected int _collisionCircleQuality;

	protected string _pathToSprite;

	protected int _spriteSheetIndex;

	protected int _widthOfSpriteInSpriteSheet;

	protected Vector2 _spriteCenter;

	protected double _lastUpdateTime;

	protected int _timeUntilNextSprite;

	protected int _numberOfDifferentSprites;

	protected bool _hasDisposedBody;

	protected List<Texture2D> _deathTextures = new List<Texture2D>();

	protected Random rand;

	protected Vector2 _lastDirectionOfDamage;

	protected int _damagePerHit;

	protected int _killPoints = 1;

	protected bool _usePath;

	public int DamagePerHit => _damagePerHit;

	public int KillPoints
	{
		get
		{
			return _killPoints;
		}
		set
		{
			_killPoints = value;
		}
	}

	public bool UsePath
	{
		get
		{
			return _usePath;
		}
		set
		{
			_usePath = value;
		}
	}

	public BadGuy()
	{
	}

	protected void Init(int health, int damage, int radius, int widthOfSprite, Vector2 spriteCenter, int spriteChanges, int timeUntilNextSprite, string pathToSprite)
	{
		_health = health;
		_collisionCircleRadius = radius;
		_collisionCircleQuality = radius / (radius / 8);
		_widthOfSpriteInSpriteSheet = widthOfSprite;
		_spriteCenter = spriteCenter;
		_pathToSprite = pathToSprite;
		_timeUntilNextSprite = timeUntilNextSprite;
		_numberOfDifferentSprites = spriteChanges;
		_hasDisposedBody = false;
		_damagePerHit = damage;
		rand = new Random();
		_currentNodeIndex = ZombieUtils.NavMesh.LineMesh.SpecialNodes[0];
		_position = ZombieUtils.NavMesh.LineMesh.MeshNodes[_currentNodeIndex]._position;
		_sprite = ZombieUtils.ContentManager().Load<Texture2D>(_pathToSprite);
		_sprite.Tag = _pathToSprite;
		_collisionCircle = GeometryHelper.GenerateCircle(_collisionCircleRadius, _collisionCircleQuality, _position);
		GeometryHelper.GetCircleLines(_collisionCircle, out _collisionCircleLines);
		_body = BodyFactory.CreateCircle(ZombieUtils.World(), ConvertUnits.ToSimUnits(_collisionCircleRadius), 1f);
		_body.BodyType = BodyType.Dynamic;
		_body.CollisionCategories = Category.None;
		_body.Friction = 10f;
		_body.Position = ConvertUnits.ToSimUnits(_position);
		_body.Mass = 1f;
		_body.LinearDamping = 3f;
		_body.AngularDamping = 5f;
		_body.UserData = this;
		_body.SleepingAllowed = true;
		_body.OnCollision += _body_OnCollision;
		_body.Enabled = false;
		ZombieUtils.TotalBadGuysCreated++;
		ZombieUtils.GlobalBadGuyList.Add(this);
	}

	private bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		return OnHitOtherFixtureObject(fixtureB, contact);
	}

	public void MoveTowardsPoint(Vector2 position, float speed)
	{
		if (_body != null)
		{
			Vector2 vector = _position - position;
			vector.Normalize();
			_body.ApplyForce(vector * (0f - speed));
		}
	}

	public void MoveTowardsNode(int nodeID, float speed)
	{
		Vector2 vector = _position - ZombieUtils.NavMesh.LineMesh.MeshNodes[nodeID]._position;
		vector.Normalize();
		_body.LinearVelocity = vector * (0f - speed);
	}

	public void EnableBody()
	{
		if (_body != null)
		{
			_body.CollisionCategories = Category.All;
			_body.Enabled = true;
		}
	}

	public void DisableBody()
	{
		if (_body != null)
		{
			_body.CollisionCategories = Category.None;
			_body.Enabled = false;
		}
	}

	public void SetPositionFromSpawnNode(int nodeId)
	{
		_position = ZombieUtils.NavMesh.LineMesh.MeshNodes[ZombieUtils.NavMesh.LineMesh.SpecialNodes[nodeId]]._position;
		_body.Position = ConvertUnits.ToSimUnits(_position);
	}

	public Vector2 GetWaypointToClosestPlayer()
	{
		return GetWaypointToDestination(ZombieUtils.Players[GetClosestPlayer()].Position);
	}

	public Vector2 GetWaypointToDestination(Vector2 destination)
	{
		int closestWaypointToPosition = GetClosestWaypointToPosition(base.Position);
		int closestWaypointToPosition2 = GetClosestWaypointToPosition(destination);
		List<Vector2> path = ZombieUtils.NavMesh.GetPath(closestWaypointToPosition, closestWaypointToPosition2);
		if (path.Count > 1)
		{
			return path[1];
		}
		return Vector2.One;
	}

	public int GetClosestWaypointToPosition(Vector2 position)
	{
		return ZombieUtils.NavMesh.LineMesh.GetNearestNodeID(position, 10000f);
	}

	public Vector2[] GetLineIntersectionsWithCollisionCircle(Line line)
	{
		return GeometryHelper.IntersectionPoint(line, _collisionCircle);
	}

	public int GetClosestPlayer()
	{
		int result = -1;
		float num = 1000000f;
		for (int i = 0; i < ZombieUtils.Players.Count; i++)
		{
			if (ZombieUtils.Players[i] is ZombiePlayer)
			{
				float num2 = Vector2.Distance(_position, ZombieUtils.Players[i].Position);
				if (num > num2)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	public float GetClosestPlayerDistance()
	{
		float num = 1000000f;
		for (int i = 0; i < ZombieUtils.Players.Count; i++)
		{
			if (ZombieUtils.Players[i] is ZombiePlayer)
			{
				float num2 = Vector2.Distance(_position, ZombieUtils.Players[i].Position);
				if (num > num2)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public override void TakeDamage(float damage, Vector2 fromDirection)
	{
		_lastDirectionOfDamage = fromDirection;
		base.TakeDamage(damage, fromDirection);
	}

	public virtual void CheckDeath()
	{
		if (_health <= 0f)
		{
			base.IsAlive = false;
			if (!_hasDisposedBody)
			{
				OnDeath();
				_body.Dispose();
				_body = null;
				_hasDisposedBody = true;
				ZombieUtils.TotalBadGuysCreated--;
			}
		}
	}

	protected abstract void OnDeath();

	public abstract int GetKillPoints();

	protected virtual bool OnHitOtherFixtureObject(Fixture other, Contact contact)
	{
		if (other.Body.UserData is Pickup)
		{
			return false;
		}
		return true;
	}

	public virtual void Update()
	{
		if (ZombieUtils.GameTime.TotalGameTime.TotalMilliseconds - _lastUpdateTime > 50.0)
		{
			if (_spriteSheetIndex < _numberOfDifferentSprites - 1)
			{
				_spriteSheetIndex++;
			}
			else
			{
				_spriteSheetIndex = 0;
			}
			_lastUpdateTime = ZombieUtils.GameTime.TotalGameTime.TotalMilliseconds;
		}
		CheckDeath();
	}

	protected virtual void Draw()
	{
		if (base.IsAlive)
		{
			Vector2 vector = _position + ZombieUtils.Offset;
			ZombieUtils.SpriteBatch.Begin();
			ZombieUtils.SpriteBatch.Draw(_sprite, new Rectangle((int)vector.X, (int)vector.Y, _widthOfSpriteInSpriteSheet, _sprite.Height), new Rectangle(_spriteSheetIndex * _widthOfSpriteInSpriteSheet, 0, _widthOfSpriteInSpriteSheet, _sprite.Height), Color.White, _rotation, _spriteCenter, SpriteEffects.None, 0f);
			ZombieUtils.SpriteBatch.End();
		}
	}

	public void Draw(bool drawDebug)
	{
		Draw();
		if (drawDebug)
		{
			GeometryHelper.GetCircleLines(_collisionCircle, out List<VertexPositionColor> circleVerts);
			GeometryHelper.LineRenderer.DrawShape(circleVerts.ToArray(), ZombieUtils.Offset);
		}
	}
}
