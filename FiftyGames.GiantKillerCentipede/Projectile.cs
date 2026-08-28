using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace FiftyGames.GiantKillerCentipede;

internal class Projectile : PhysicsObject
{
	protected Ship _owner;

	protected bool _alive;

	protected int _shotDelay;

	protected float _maxVelocity;

	protected int _damage;

	protected float _splashRadius;

	protected int _splashDamage;

	protected float _force;

	protected Cue _sound;

	public Cue SoundCue
	{
		get
		{
			return _sound;
		}
		set
		{
			_sound = value;
			_sound.Play();
		}
	}

	public int ShotDelay => _shotDelay;

	public int ShotDamage => _damage;

	public float ShotForce => _force;

	public int SplashDamage => _splashDamage;

	public float SplashRadius => _splashRadius;

	public Ship Owner => _owner;

	public bool IsAlive
	{
		get
		{
			return _alive;
		}
		set
		{
			_alive = value;
			if (_sound != null && !_sound.IsStopped && !_sound.IsDisposed)
			{
				_sound.Stop(AudioStopOptions.Immediate);
				_sound = null;
			}
		}
	}

	public Projectile(Ship owner, Vector2 position, Vector2 velocity)
	{
		_owner = owner;
		_alive = true;
		_position = position;
		_velocity = velocity;
		_physVolume.Radius = 0f;
		_shotDelay = 0;
		_maxVelocity = 0f;
		_damage = 0;
		_force = 0f;
		_splashDamage = _damage;
		_splashRadius = _physVolume.Radius;
	}

	public override void Update(GameTime gameTime)
	{
		if (_maxVelocity > 0f && _velocity.LengthSquared() > _maxVelocity * _maxVelocity)
		{
			_velocity.Normalize();
			_velocity *= _maxVelocity;
		}
		base.Update(gameTime);
		if (_position.X < 0f)
		{
			_position.X += 1280f;
		}
		if (_position.X > 1280f)
		{
			_position.X -= 1280f;
		}
	}
}
