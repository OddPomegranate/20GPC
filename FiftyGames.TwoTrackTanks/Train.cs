using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace FiftyGames.TwoTrackTanks;

internal class Train : PhysicsObject
{
	private Vector2 _direction;

	private float _speed;

	private Train _engine;

	private bool _derailed;

	public bool IsEngine => _engine == null;

	public bool Derailed
	{
		get
		{
			return _derailed;
		}
		set
		{
			_derailed = value;
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
			_direction = Vector2.Normalize(value);
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

	public Train(Train engine)
	{
		_direction = Vector2.UnitX;
		_speed = 1f;
		_engine = engine;
	}

	public Train()
		: this(null)
	{
	}

	public override void Update(GameTime gameTime)
	{
		if (!_derailed)
		{
			if (_engine != null)
			{
				_derailed = _engine.Derailed;
			}
			_physBody.LinearVelocity = ConvertUnits.ToSimUnits(_direction * _speed);
		}
		base.Update(gameTime);
	}
}
