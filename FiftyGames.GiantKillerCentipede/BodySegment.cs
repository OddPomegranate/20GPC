using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class BodySegment : PhysicsObject
{
	public enum BodySegmentType
	{
		Head,
		Body
	}

	protected const int MaxFlashDelay = 100;

	protected const float HealthAlert = 0.5f;

	protected const float Friction = 0.5f;

	protected BodySegmentType _bodyType;

	protected Centipede _owner;

	protected int _health;

	protected int _maxHealth;

	protected Texture2D _headSprite;

	protected Vector2 _prevPosition;

	protected Vector2 _wrapOffset;

	protected Color _playerColour;

	protected Color[] _flashColours;

	protected int _flashIndex;

	protected int _flashTimer;

	protected int _flashDelay;

	protected bool _damageThisFrame;

	public int Health => _health;

	public BodySegmentType BodyType
	{
		get
		{
			return _bodyType;
		}
		set
		{
			if (_bodyType != BodySegmentType.Head && value == BodySegmentType.Head)
			{
				_maxHealth *= 2;
				_health *= 2;
			}
			_bodyType = value;
		}
	}

	public override float Rotation
	{
		set
		{
			if (_bodyType == BodySegmentType.Head)
			{
				_rotation = value;
			}
		}
	}

	public Vector2 WrapOffset
	{
		get
		{
			return _wrapOffset;
		}
		set
		{
			_wrapOffset = value;
		}
	}

	public BodySegment(Centipede owner, BodySegmentType bodyType, Color colour, int handicap)
	{
		_owner = owner;
		_bodyType = bodyType;
		if (handicap == 0)
		{
			handicap = 1;
		}
		_maxHealth = 80 - 6 * handicap;
		_health = _maxHealth;
		_prevPosition = _position;
		_wrapOffset = Vector2.Zero;
		_colour = colour;
		_headSprite = null;
		_physVolume.Radius = 24f;
		_damageThisFrame = false;
		_playerColour = _colour;
		_flashColours = new Color[3];
		ref Color reference = ref _flashColours[0];
		reference = _colour;
		ref Color reference2 = ref _flashColours[1];
		reference2 = Color.Red;
		ref Color reference3 = ref _flashColours[2];
		reference3 = Color.Yellow;
		_flashIndex = 0;
		_flashDelay = 100;
		_flashTimer = 0;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\CentipedeBody");
		_headSprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\CentipedeHead");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		float num = _position.Y - _physVolume.Radius;
		if (num < 0f)
		{
			_position.Y += 0.8f;
		}
		if (num + _velocity.Y < 0f && _velocity.Y < 0f)
		{
			_velocity.Y = 0f;
		}
		if (_velocity.Length() > 20f)
		{
			_velocity.Normalize();
			_velocity *= 20f;
		}
		base.Update(gameTime);
		_velocity *= 0.5f;
		if (_bodyType != BodySegmentType.Head && _velocity.Length() > 0.6f)
		{
			Vector2 vector = _position - (_position + _velocity);
			_rotation = (float)Math.Atan2(vector.Y, vector.X);
		}
		if (_health < (int)((float)_maxHealth * 0.5f))
		{
			_flashTimer += gameTime.ElapsedGameTime.Milliseconds;
			if (_flashTimer > _flashDelay)
			{
				_flashIndex++;
				if (_flashIndex == _flashColours.Length)
				{
					_flashIndex = 0;
				}
				_flashTimer = 0;
			}
		}
		else
		{
			_flashIndex = 0;
			_flashTimer = 0;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_damageThisFrame)
		{
			_colour = Color.White;
			_damageThisFrame = false;
		}
		else
		{
			_colour = _flashColours[_flashIndex];
		}
		base.Draw(spriteBatch);
		if (_bodyType == BodySegmentType.Head)
		{
			spriteBatch.Draw(_headSprite, _position, null, Color.White, _rotation, _origin, _scale, SpriteEffects.None, 0f);
		}
	}

	public void Damage(int damage)
	{
		_health -= damage;
		if (_health < 0)
		{
			_health = 0;
		}
		_flashDelay = (int)((float)_health / (float)_maxHealth * 100f);
		_damageThisFrame = true;
		if (_owner.Player != null)
		{
			_owner.Player.GamePadManager.StartVibration(100 + damage * 10, (float)damage * 0.05f);
		}
	}
}
