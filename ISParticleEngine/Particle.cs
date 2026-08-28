using Microsoft.Xna.Framework;

namespace ISParticleEngine;

public class Particle
{
	protected Vector2 _position;

	protected Vector2 _velocity;

	protected Vector2 _scale;

	protected Vector2 _deltaScale1;

	protected Vector2 _deltaScale2;

	protected Vector3 _color;

	protected Vector3 _deltaColor1;

	protected Vector3 _deltaColor2;

	protected float _rotation;

	protected float _deltaRotation;

	protected float _alpha;

	protected float _deltaAlpha1;

	protected float _deltaAlpha2;

	protected int _life;

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

	public Vector2 Velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;
		}
	}

	public Vector2 Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = value;
		}
	}

	public Vector2 DeltaScale1
	{
		get
		{
			return _deltaScale1;
		}
		set
		{
			_deltaScale1 = value;
		}
	}

	public Vector2 DeltaScale2
	{
		get
		{
			return _deltaScale2;
		}
		set
		{
			_deltaScale2 = value;
		}
	}

	public Vector3 Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	public Vector3 DeltaColor1
	{
		get
		{
			return _deltaColor1;
		}
		set
		{
			_deltaColor1 = value;
		}
	}

	public Vector3 DeltaColor2
	{
		get
		{
			return _deltaColor2;
		}
		set
		{
			_deltaColor2 = value;
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

	public float DeltaRotation
	{
		get
		{
			return _deltaRotation;
		}
		set
		{
			_deltaRotation = value;
		}
	}

	public float Alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = value;
		}
	}

	public float DeltaAlpha1
	{
		get
		{
			return _deltaAlpha1;
		}
		set
		{
			_deltaAlpha1 = value;
		}
	}

	public float DeltaAlpha2
	{
		get
		{
			return _deltaAlpha2;
		}
		set
		{
			_deltaAlpha2 = value;
		}
	}

	public int Life
	{
		get
		{
			return _life;
		}
		set
		{
			_life = value;
		}
	}
}
