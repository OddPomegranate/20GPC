using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.LunarLander;

internal class Pod
{
	public const int MaxHealth = 10;

	public const int MaxLives = 3;

	public const int MaxFuel = 400;

	protected const int THRUST = 1;

	protected const int AIRLEFT = 2;

	protected const int AIRRIGHT = 4;

	protected const float ThrusterAirSize = 4f;

	protected const float MaxVelocity = 10f;

	protected const float MaxRotationalVelocity = 0.2f;

	protected const float RotationalFriction = 0.97f;

	protected const float MaxRotationalVelocityCorrectionCoefficient = 9f;

	protected const float Weight = 0.1f;

	protected const float SteeringPower = 0.01f;

	protected const int FlashRate = 100;

	protected const float DebrisMinSpeed = 1f;

	protected const float DebrisMaxSpeed = 2f;

	protected Player _player;

	protected int _score;

	protected bool _leader;

	protected bool _first;

	protected int _health;

	protected int _lives;

	protected int _fuel;

	protected bool _landed;

	protected VertexPositionColor[] _shapeVerts;

	protected short[] _shapeIndex;

	protected Color _colour;

	protected VertexPositionColor[] _fireVerts;

	protected short[] _fireIndex;

	protected VertexPositionColor[] _airLeftVerts;

	protected short[] _airLeftIndex;

	protected VertexPositionColor[] _airRightVerts;

	protected short[] _airRightIndex;

	protected BoundingSphere _physVolume;

	protected float _restitution;

	protected Vector2 _position;

	protected float _rotation;

	protected int _thrusterState;

	protected Vector2 _velocity;

	protected float _rotationalVelocity;

	protected int _flashTimer;

	protected Random _ranGen;

	protected List<Vector4> _debris = new List<Vector4>();

	protected VertexPositionColor[] _debrisVerts;

	protected short[] _debrisIndex;

	protected Cue _thrustSound;

	protected Cue _turnSound;

	public Player Player
	{
		get
		{
			return _player;
		}
		set
		{
			_player = value;
			_colour = _player.Colour();
		}
	}

	public BoundingSphere CollisionVolume => _physVolume;

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

	public float RotationalVelocity
	{
		get
		{
			return _rotationalVelocity;
		}
		set
		{
			_rotationalVelocity = value;
		}
	}

	public Color Colour => _colour;

	public int Score => _score;

	public bool Leader
	{
		get
		{
			return _leader;
		}
		set
		{
			_leader = value;
		}
	}

	public bool First
	{
		get
		{
			return _first;
		}
		set
		{
			_first = value;
		}
	}

	public int Lives => _lives;

	public int Health => _health;

	public int Fuel => _fuel;

	public bool HasLanded
	{
		get
		{
			return _landed;
		}
		set
		{
			_landed = value;
		}
	}

	public float ShipRestitution
	{
		get
		{
			return _restitution;
		}
		set
		{
			_restitution = value;
		}
	}

	public Cue FuelThrusterSound
	{
		get
		{
			return _thrustSound;
		}
		set
		{
			_thrustSound = value;
			_thrustSound.Play();
			_thrustSound.Pause();
		}
	}

	public Cue AirThrusterSound
	{
		get
		{
			return _turnSound;
		}
		set
		{
			_turnSound = value;
			_turnSound.Play();
			_turnSound.Pause();
		}
	}

	public Pod(Player controller)
	{
		_player = controller;
		_lives = 3;
		_physVolume.Radius = 10f;
		_restitution = 0.6f;
		_colour = controller.Colour();
		_colour.R = Math.Max(_colour.R, (byte)10);
		_colour.G = Math.Max(_colour.G, (byte)10);
		_colour.B = Math.Max(_colour.B, (byte)10);
		_fireVerts = new VertexPositionColor[6];
		ref VertexPositionColor reference = ref _fireVerts[0];
		reference = new VertexPositionColor(new Vector3(-4f, 5f, 0f), Color.Orange);
		ref VertexPositionColor reference2 = ref _fireVerts[1];
		reference2 = new VertexPositionColor(new Vector3(0f, 14f, 0f), Color.Orange);
		ref VertexPositionColor reference3 = ref _fireVerts[2];
		reference3 = new VertexPositionColor(new Vector3(4f, 5f, 0f), Color.Orange);
		ref VertexPositionColor reference4 = ref _fireVerts[3];
		reference4 = new VertexPositionColor(new Vector3(-2f, 5f, 0f), Color.White);
		ref VertexPositionColor reference5 = ref _fireVerts[4];
		reference5 = new VertexPositionColor(new Vector3(0f, 8f, 0f), Color.White);
		ref VertexPositionColor reference6 = ref _fireVerts[5];
		reference6 = new VertexPositionColor(new Vector3(2f, 5f, 0f), Color.White);
		_fireIndex = new short[8] { 0, 1, 1, 2, 3, 4, 4, 5 };
		_airLeftVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference7 = ref _airLeftVerts[0];
		reference7 = new VertexPositionColor(new Vector3(5f, -5f, 0f), Color.White);
		ref VertexPositionColor reference8 = ref _airLeftVerts[1];
		reference8 = new VertexPositionColor(new Vector3(8f, -5f, 0f), Color.White);
		ref VertexPositionColor reference9 = ref _airLeftVerts[2];
		reference9 = new VertexPositionColor(new Vector3(-5f, 5f, 0f), Color.White);
		ref VertexPositionColor reference10 = ref _airLeftVerts[3];
		reference10 = new VertexPositionColor(new Vector3(-8f, 5f, 0f), Color.White);
		_airLeftIndex = new short[4] { 0, 1, 2, 3 };
		_airRightVerts = new VertexPositionColor[4];
		ref VertexPositionColor reference11 = ref _airRightVerts[0];
		reference11 = new VertexPositionColor(new Vector3(-5f, -5f, 0f), Color.White);
		ref VertexPositionColor reference12 = ref _airRightVerts[1];
		reference12 = new VertexPositionColor(new Vector3(-8f, -5f, 0f), Color.White);
		ref VertexPositionColor reference13 = ref _airRightVerts[2];
		reference13 = new VertexPositionColor(new Vector3(5f, 5f, 0f), Color.White);
		ref VertexPositionColor reference14 = ref _airRightVerts[3];
		reference14 = new VertexPositionColor(new Vector3(8f, 5f, 0f), Color.White);
		_airRightIndex = new short[4] { 0, 1, 2, 3 };
		_debrisVerts = new VertexPositionColor[2];
		_debrisIndex = new short[2] { 0, 1 };
		_ranGen = new Random();
		_position = Vector2.Zero;
		GenerateShip((int)Player.PlayerIndex);
	}

	public void Accelerate(Vector2 acceleration)
	{
		_velocity += acceleration;
	}

	private void Accelerate(float acceleration, float bearing)
	{
		_velocity += new Vector2(acceleration * (float)Math.Sin(bearing), acceleration * (float)Math.Cos(bearing) * -1f);
	}

	public void Update(GameTime gameTime, float gravity)
	{
		if (_health != 0 && !_landed)
		{
			if (_rotation > (float)Math.PI)
			{
				float num = _rotation - (float)Math.PI * 2f;
				_rotation = num % ((float)Math.PI * 2f);
			}
			if (_rotation < -(float)Math.PI)
			{
				float num2 = _rotation + (float)Math.PI * 2f;
				_rotation = num2 % ((float)Math.PI * 2f);
			}
			_velocity.Y += gravity * 0.1f;
			_rotationalVelocity -= MathHelper.Clamp(_rotation * 0.1f * 0.01f, -1f / 45f, 1f / 45f);
			if (_velocity.Length() > 10f)
			{
				float num3 = (float)Math.Atan2(_velocity.Y, _velocity.X);
				_velocity = new Vector2(10f * (float)Math.Cos(num3), 10f * (float)Math.Sin(num3));
			}
			_rotationalVelocity = MathHelper.Clamp(_rotationalVelocity, -0.2f, 0.2f);
			if (_position.Y > 360f + _physVolume.Radius)
			{
				_health = 0;
				_player.GamePadManager.StartVibration(800, 1f, 1f, 0f, 0f);
			}
			if (_position.Y < 0f - _physVolume.Radius)
			{
				_velocity.Y = 0f;
				_position.Y = 0f - _physVolume.Radius;
			}
			if (_position.X < 0f)
			{
				_position.X += 640f;
			}
			if (_position.X > 640f)
			{
				_position.X -= 640f;
			}
			if (_player.GamePadManager.ButtonIsHeld(Buttons.A) && _fuel != 0)
			{
				_thrusterState |= 1;
				Accelerate(0.1f, _rotation);
				_fuel--;
				_player.GamePadManager.StartVibration(10, 0.12f);
				_thrustSound.Resume();
			}
			else
			{
				_thrusterState &= -2;
				_thrustSound.Pause();
			}
			float x = _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X;
			if (x != 0f)
			{
				_rotationalVelocity += x * 0.01f;
				if (x > 0f)
				{
					_thrusterState |= 4;
					_thrusterState &= -3;
					_airRightVerts[1].Position.X = -5f - x * 4f;
					_airRightVerts[3].Position.X = 5f + x * 4f;
				}
				else
				{
					_thrusterState |= 2;
					_thrusterState &= -5;
					_airLeftVerts[1].Position.X = 5f - x * 4f;
					_airLeftVerts[3].Position.X = -5f + x * 4f;
				}
				_turnSound.Resume();
			}
			else
			{
				_thrusterState &= -3;
				_thrusterState &= -5;
				_turnSound.Pause();
			}
			_position += _velocity;
			_rotation += _rotationalVelocity * 0.5f;
			_rotationalVelocity *= 0.97f;
			_physVolume.Center = new Vector3(_position, 0f);
			return;
		}
		if (_health == 0)
		{
			_physVolume.Radius = 0f;
			_physVolume.Center = new Vector3(-10f, -10f, -10f);
			for (int i = 0; i < _debris.Count; i++)
			{
				Vector4 value = _debris[i];
				value.X += value.Z;
				value.Y += value.W;
				_debris[i] = value;
			}
		}
		_thrusterState = 0;
		_velocity = Vector2.Zero;
		_rotation = 0f;
	}

	public void Draw(LineRender graphics, GameTime gameTime)
	{
		if (_health != 0)
		{
			if (_health > 4 || _flashTimer > 0)
			{
				Matrix matrix = Matrix.Multiply(Matrix.CreateRotationZ(_rotation), Matrix.CreateTranslation(new Vector3(_position, 0f)));
				VertexPositionColor[] array = new VertexPositionColor[_shapeVerts.Length];
				VertexPositionColor[] array2 = new VertexPositionColor[_fireVerts.Length];
				VertexPositionColor[] array3 = new VertexPositionColor[_airLeftVerts.Length];
				VertexPositionColor[] array4 = new VertexPositionColor[_airRightVerts.Length];
				for (int i = 0; i < array.Length; i++)
				{
					Vector3.Transform(ref _shapeVerts[i].Position, ref matrix, out array[i].Position);
					array[i].Color = _colour;
				}
				for (int j = 0; j < array2.Length; j++)
				{
					Vector3.Transform(ref _fireVerts[j].Position, ref matrix, out array2[j].Position);
					array2[j].Color = _fireVerts[j].Color;
				}
				for (int k = 0; k < array3.Length; k++)
				{
					Vector3.Transform(ref _airLeftVerts[k].Position, ref matrix, out array3[k].Position);
					array3[k].Color = _airLeftVerts[k].Color;
				}
				for (int l = 0; l < array4.Length; l++)
				{
					Vector3.Transform(ref _airRightVerts[l].Position, ref matrix, out array4[l].Position);
					array4[l].Color = _airRightVerts[l].Color;
				}
				if ((_thrusterState & 1) == 1)
				{
					graphics.DrawIndexedShape(array2, _fireIndex);
				}
				if ((_thrusterState & 2) == 2)
				{
					graphics.DrawIndexedShape(array3, _airLeftIndex);
				}
				if ((_thrusterState & 4) == 4)
				{
					graphics.DrawIndexedShape(array4, _airRightIndex);
				}
				graphics.DrawIndexedShape(array, _shapeIndex);
			}
			if (_health < 5)
			{
				if (_flashTimer >= 100)
				{
					_flashTimer = -100;
				}
				_flashTimer += gameTime.ElapsedGameTime.Milliseconds;
			}
			return;
		}
		foreach (Vector4 item in _debris)
		{
			Vector2 vector = new Vector2(item.X, item.Y);
			if (vector.Length() < 200f)
			{
				float radians = vector.Length() / 30f;
				Matrix matrix2 = Matrix.Multiply(Matrix.CreateRotationZ(radians), Matrix.CreateTranslation(new Vector3(_position + vector, 0f)));
				VertexPositionColor[] array5 = new VertexPositionColor[_debrisVerts.Length];
				Vector3.Transform(ref _debrisVerts[0].Position, ref matrix2, out array5[0].Position);
				Vector3.Transform(ref _debrisVerts[1].Position, ref matrix2, out array5[1].Position);
				array5[0].Color = _colour;
				array5[1].Color = _colour;
				graphics.DrawIndexedShape(array5, _debrisIndex);
			}
		}
	}

	public void Spawn(Vector2 position)
	{
		_lives--;
		_health = 10;
		_fuel = 400;
		_first = false;
		_landed = false;
		_leader = false;
		_position = position;
		_rotation = 0f;
		_velocity = new Vector2(0f);
		_rotationalVelocity = 0f;
		_physVolume.Center = new Vector3(_position, 0f);
		_physVolume.Radius = 10f;
		_flashTimer = 100;
		ref VertexPositionColor reference = ref _debrisVerts[0];
		reference = new VertexPositionColor(new Vector3(-5f, 0f, 0f), _colour);
		ref VertexPositionColor reference2 = ref _debrisVerts[1];
		reference2 = new VertexPositionColor(new Vector3(5f, 0f, 0f), _colour);
		_debris.Clear();
		for (int i = 0; i < 10; i++)
		{
			float num = Math.Max((float)_ranGen.NextDouble() * 2f, 1f);
			float num2 = Math.Max((float)_ranGen.NextDouble() * 2f, 1f);
			switch (_ranGen.Next(4))
			{
			case 1:
				num *= -1f;
				break;
			case 2:
				num2 *= -1f;
				break;
			case 3:
				num *= -1f;
				num2 *= -1f;
				break;
			}
			_debris.Add(new Vector4(0f, 0f, num, num2));
		}
	}

	public void GenerateShip(int index)
	{
		switch (index)
		{
		case 0:
		{
			_shapeVerts = new VertexPositionColor[8];
			ref VertexPositionColor reference32 = ref _shapeVerts[0];
			reference32 = new VertexPositionColor(new Vector3(0f, -10f, 0f), _colour);
			ref VertexPositionColor reference33 = ref _shapeVerts[1];
			reference33 = new VertexPositionColor(new Vector3(-5f, -5f, 0f), _colour);
			ref VertexPositionColor reference34 = ref _shapeVerts[2];
			reference34 = new VertexPositionColor(new Vector3(5f, -5f, 0f), _colour);
			ref VertexPositionColor reference35 = ref _shapeVerts[3];
			reference35 = new VertexPositionColor(new Vector3(-5f, 5f, 0f), _colour);
			ref VertexPositionColor reference36 = ref _shapeVerts[4];
			reference36 = new VertexPositionColor(new Vector3(5f, 5f, 0f), _colour);
			ref VertexPositionColor reference37 = ref _shapeVerts[5];
			reference37 = new VertexPositionColor(new Vector3(0f, 5f, 0f), _colour);
			ref VertexPositionColor reference38 = ref _shapeVerts[6];
			reference38 = new VertexPositionColor(new Vector3(-6f, 10f, 0f), _colour);
			ref VertexPositionColor reference39 = ref _shapeVerts[7];
			reference39 = new VertexPositionColor(new Vector3(6f, 10f, 0f), _colour);
			_shapeIndex = new short[20]
			{
				0, 1, 0, 2, 1, 2, 2, 4, 4, 3,
				3, 1, 3, 6, 6, 5, 4, 7, 7, 5
			};
			break;
		}
		case 1:
		{
			_shapeVerts = new VertexPositionColor[12];
			ref VertexPositionColor reference20 = ref _shapeVerts[0];
			reference20 = new VertexPositionColor(new Vector3(-4f, -10f, 0f), _colour);
			ref VertexPositionColor reference21 = ref _shapeVerts[1];
			reference21 = new VertexPositionColor(new Vector3(4f, -10f, 0f), _colour);
			ref VertexPositionColor reference22 = ref _shapeVerts[2];
			reference22 = new VertexPositionColor(new Vector3(-8f, -6f, 0f), _colour);
			ref VertexPositionColor reference23 = ref _shapeVerts[3];
			reference23 = new VertexPositionColor(new Vector3(8f, -6f, 0f), _colour);
			ref VertexPositionColor reference24 = ref _shapeVerts[4];
			reference24 = new VertexPositionColor(new Vector3(-8f, 2f, 0f), _colour);
			ref VertexPositionColor reference25 = ref _shapeVerts[5];
			reference25 = new VertexPositionColor(new Vector3(8f, 2f, 0f), _colour);
			ref VertexPositionColor reference26 = ref _shapeVerts[6];
			reference26 = new VertexPositionColor(new Vector3(-4f, 6f, 0f), _colour);
			ref VertexPositionColor reference27 = ref _shapeVerts[7];
			reference27 = new VertexPositionColor(new Vector3(4f, 6f, 0f), _colour);
			ref VertexPositionColor reference28 = ref _shapeVerts[8];
			reference28 = new VertexPositionColor(new Vector3(0f, 6f, 0f), _colour);
			ref VertexPositionColor reference29 = ref _shapeVerts[9];
			reference29 = new VertexPositionColor(new Vector3(-5f, 10f, 0f), _colour);
			ref VertexPositionColor reference30 = ref _shapeVerts[10];
			reference30 = new VertexPositionColor(new Vector3(0f, 10f, 0f), _colour);
			ref VertexPositionColor reference31 = ref _shapeVerts[11];
			reference31 = new VertexPositionColor(new Vector3(5f, 10f, 0f), _colour);
			_shapeIndex = new short[22]
			{
				0, 1, 0, 2, 1, 3, 2, 4, 3, 5,
				4, 6, 5, 7, 6, 7, 6, 9, 7, 11,
				8, 10
			};
			break;
		}
		case 2:
		{
			_shapeVerts = new VertexPositionColor[11];
			ref VertexPositionColor reference9 = ref _shapeVerts[0];
			reference9 = new VertexPositionColor(new Vector3(0f, -10f, 0f), _colour);
			ref VertexPositionColor reference10 = ref _shapeVerts[1];
			reference10 = new VertexPositionColor(new Vector3(-4f, -6f, 0f), _colour);
			ref VertexPositionColor reference11 = ref _shapeVerts[2];
			reference11 = new VertexPositionColor(new Vector3(4f, -6f, 0f), _colour);
			ref VertexPositionColor reference12 = ref _shapeVerts[3];
			reference12 = new VertexPositionColor(new Vector3(-2f, -4f, 0f), _colour);
			ref VertexPositionColor reference13 = ref _shapeVerts[4];
			reference13 = new VertexPositionColor(new Vector3(2f, -4f, 0f), _colour);
			ref VertexPositionColor reference14 = ref _shapeVerts[5];
			reference14 = new VertexPositionColor(new Vector3(-8f, 2f, 0f), _colour);
			ref VertexPositionColor reference15 = ref _shapeVerts[6];
			reference15 = new VertexPositionColor(new Vector3(8f, 2f, 0f), _colour);
			ref VertexPositionColor reference16 = ref _shapeVerts[7];
			reference16 = new VertexPositionColor(new Vector3(-8f, 5f, 0f), _colour);
			ref VertexPositionColor reference17 = ref _shapeVerts[8];
			reference17 = new VertexPositionColor(new Vector3(8f, 5f, 0f), _colour);
			ref VertexPositionColor reference18 = ref _shapeVerts[9];
			reference18 = new VertexPositionColor(new Vector3(-6f, 10f, 0f), _colour);
			ref VertexPositionColor reference19 = ref _shapeVerts[10];
			reference19 = new VertexPositionColor(new Vector3(6f, 10f, 0f), _colour);
			_shapeIndex = new short[20]
			{
				0, 1, 0, 2, 1, 3, 2, 4, 3, 5,
				4, 6, 5, 7, 6, 8, 7, 8, 5, 6
			};
			break;
		}
		case 3:
		{
			_shapeVerts = new VertexPositionColor[8];
			ref VertexPositionColor reference = ref _shapeVerts[0];
			reference = new VertexPositionColor(new Vector3(0f, -10f, 0f), _colour);
			ref VertexPositionColor reference2 = ref _shapeVerts[1];
			reference2 = new VertexPositionColor(new Vector3(-8f, 4f, 0f), _colour);
			ref VertexPositionColor reference3 = ref _shapeVerts[2];
			reference3 = new VertexPositionColor(new Vector3(8f, 4f, 0f), _colour);
			ref VertexPositionColor reference4 = ref _shapeVerts[3];
			reference4 = new VertexPositionColor(new Vector3(-3f, 4f, 0f), _colour);
			ref VertexPositionColor reference5 = ref _shapeVerts[4];
			reference5 = new VertexPositionColor(new Vector3(3f, 4f, 0f), _colour);
			ref VertexPositionColor reference6 = ref _shapeVerts[5];
			reference6 = new VertexPositionColor(new Vector3(-5f, 10f, 0f), _colour);
			ref VertexPositionColor reference7 = ref _shapeVerts[6];
			reference7 = new VertexPositionColor(new Vector3(0f, 10f, 0f), _colour);
			ref VertexPositionColor reference8 = ref _shapeVerts[7];
			reference8 = new VertexPositionColor(new Vector3(5f, 10f, 0f), _colour);
			_shapeIndex = new short[18]
			{
				0, 1, 0, 2, 1, 2, 1, 5, 5, 3,
				3, 6, 6, 4, 4, 7, 7, 2
			};
			break;
		}
		}
	}

	public void Damage(int damage)
	{
		_health -= damage;
		if (_health <= 0 || _fuel == 0)
		{
			_health = 0;
			_player.GamePadManager.StartVibration(800, 1f, 1f, 0f, 0f);
			_thrustSound.Pause();
			_turnSound.Pause();
		}
		_player.GamePadManager.StartVibration(200, (float)damage * 0.2f);
	}

	public void ResetScore()
	{
		_score = 0;
	}

	public void AwardScore(int number)
	{
		_score += number;
	}

	public void AwardLives(int number)
	{
		_lives += number;
	}
}
