using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FiftyGames.Zombie;

internal class Entity
{
	protected Vector2 _position;

	protected Vector2 _direction;

	protected float _rotation;

	protected float _speed;

	protected float _health;

	protected bool _isAlive;

	protected Body _body;

	public Vector2 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public Vector2 Direction
	{
		get
		{
			return _direction;
		}
		set
		{
			_direction = value;
		}
	}

	public float Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
		}
	}

	public float Rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
		}
	}

	public float Health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = value;
		}
	}

	public bool IsAlive
	{
		get
		{
			return _isAlive;
		}
		set
		{
			_isAlive = value;
		}
	}

	public Body PhysBody => _body;

	public Entity()
	{
		_position = new Vector2(0f, 0f);
		_direction = new Vector2(0f, 1f);
		_speed = 0f;
		_health = 1f;
		_isAlive = true;
	}

	public virtual void TakeDamage(float damage, Vector2 fromDirection)
	{
		_health -= damage;
	}
}
