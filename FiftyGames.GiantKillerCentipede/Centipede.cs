using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.GiantKillerCentipede;

internal class Centipede
{
	public const float MaxPlayerInfluence = 2f;

	public const float FrontInfuence = 1f;

	public const float RearInfuence = 1f;

	protected const int HoldEatTime = 200;

	public const int NomDamage = 8;

	protected const int MushroomSpeedBoost = 5;

	protected const int BoostLimit = 3000;

	protected const float BoostPlayerInfluence = 1.5f;

	protected const int PreferenceBubbleTime = 2000;

	private ContentManager _contentLoader;

	protected Player _player;

	protected Color _colour;

	protected List<BodySegment> _body;

	protected Vector2 _playerInfluence;

	protected bool _eating;

	protected int _eatingTimer;

	protected int _mushroomCounter;

	protected int _boostTimer;

	protected Texture2D[] _preferenceBubbles;

	protected Vector2 _preferenceBubbleOrigin;

	protected bool _centipedeCandidate;

	protected int _preferenceChangeTimer;

	public Player Player => _player;

	public bool IsEating => _eating;

	public List<BodySegment> Body => _body;

	public bool ElegableCentipede
	{
		get
		{
			return _centipedeCandidate;
		}
		set
		{
			_centipedeCandidate = value;
		}
	}

	public Centipede(Player player, Vector2 headPosition, int length)
	{
		_player = player;
		if (_player != null)
		{
			_colour = _player.Colour();
		}
		else
		{
			_colour = Color.Lime;
		}
		_body = new List<BodySegment>();
		BodySegment item = new BodySegment(this, BodySegment.BodySegmentType.Head, _colour, _body.Count)
		{
			Position = headPosition
		};
		_body.Add(item);
		for (int i = 0; i < length - 1; i++)
		{
			item = new BodySegment(this, BodySegment.BodySegmentType.Body, _colour, _body.Count);
			headPosition.Y -= 48f;
			item.Position = headPosition;
			_body.Add(item);
		}
		_eating = false;
		_mushroomCounter = 0;
		_boostTimer = 0;
		_eatingTimer = 0;
		_centipedeCandidate = true;
	}

	public Centipede(Player player, List<BodySegment> existingBody)
	{
		_player = player;
		if (_player != null)
		{
			_colour = _player.Colour();
		}
		else
		{
			_colour = Color.Lime;
		}
		_body = existingBody;
		_body[0].BodyType = BodySegment.BodySegmentType.Head;
		_eating = false;
		_mushroomCounter = 0;
		_boostTimer = 0;
		_eatingTimer = 0;
	}

	public void Load(ContentManager contentLoader)
	{
		_preferenceBubbles = new Texture2D[2];
		_preferenceBubbles[0] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\OptIn");
		_preferenceBubbles[1] = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\OptOut");
		_preferenceBubbleOrigin = new Vector2((float)_preferenceBubbles[0].Width * 0.5f, (float)_preferenceBubbles[0].Height * 0.5f);
		foreach (BodySegment item in _body)
		{
			item.Load(contentLoader);
		}
		_contentLoader = contentLoader;
	}

	public virtual void Update(GameTime gameTime, bool gameOver)
	{
		if (_player != null)
		{
			_playerInfluence = Vector2.Zero;
			if (!gameOver)
			{
				_playerInfluence = _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left * new Vector2(2f, -2f);
				if (_player.GamePadManager.ButtonWasPressed(Buttons.A))
				{
					_eating = true;
				}
				else if (_player.GamePadManager.ButtonIsHeld(Buttons.A))
				{
					if (_eatingTimer > 200)
					{
						_eating = true;
						_eatingTimer = 0;
					}
					else
					{
						_eating = false;
					}
					_eatingTimer += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					_eatingTimer = 0;
					_eating = false;
				}
			}
			else
			{
				_eating = false;
			}
		}
		if (_playerInfluence.Length() > 0.1f && _body[0].Velocity.Length() < 2f)
		{
			_body[0].Velocity += _playerInfluence;
			_body[0].Rotation = (float)Math.Atan2(0f - _playerInfluence.Y, 0f - _playerInfluence.X);
		}
		bool flag = true;
		bool flag2 = true;
		for (int i = 0; i < _body.Count; i++)
		{
			if (_body[i].WrapOffset.X != _body[0].WrapOffset.X)
			{
				flag = false;
			}
			if (_body[i].WrapOffset.Y != _body[0].WrapOffset.Y)
			{
				flag2 = false;
			}
			if (_body[i].Position.X < 0f)
			{
				_body[i].Position += new Vector2(1280f, 0f);
				_body[i].WrapOffset += new Vector2(-1280f, 0f);
				flag = false;
			}
			else if (_body[i].Position.X > 1280f)
			{
				_body[i].Position += new Vector2(-1280f, 0f);
				_body[i].WrapOffset += new Vector2(1280f, 0f);
				flag = false;
			}
			if (_body[i].Position.Y > 720f)
			{
				_body[i].Position += new Vector2(0f, -720f);
				_body[i].WrapOffset += new Vector2(0f, 720f);
				flag2 = false;
			}
			if (i < _body.Count - 1)
			{
				float num = (_body[i].Position + _body[i].WrapOffset + _body[i].Velocity - (_body[i + 1].Position + _body[i + 1].WrapOffset + _body[i + 1].Velocity)).Length();
				float num2 = _body[i].CollisionVolume.Radius + _body[i + 1].CollisionVolume.Radius;
				if (num > num2)
				{
					Vector2 vector = _body[i].Position + _body[i].WrapOffset + _body[i].Velocity - (_body[i + 1].Position + _body[i + 1].WrapOffset + _body[i + 1].Velocity);
					vector.Normalize();
					if (_playerInfluence.Length() == 0f)
					{
						_body[i + 1].Velocity += vector * ((num - num2) / 2f) * 1f;
					}
					else
					{
						_body[i + 1].Velocity += vector * (num - num2) * 0.9f * 1f;
					}
				}
			}
			if (i > 0)
			{
				float num3 = (_body[i].Position + _body[i].WrapOffset + _body[i].Velocity - (_body[i - 1].Position + _body[i - 1].WrapOffset + _body[i - 1].Velocity)).Length();
				float num4 = _body[i].CollisionVolume.Radius + _body[i - 1].CollisionVolume.Radius;
				if (num3 > num4)
				{
					Vector2 vector2 = _body[i].Position + _body[i].WrapOffset + _body[i].Velocity - (_body[i - 1].Position - _body[i - 1].WrapOffset + _body[i - 1].Velocity);
					vector2.Normalize();
					if (_playerInfluence.Length() == 0f)
					{
						_body[i - 1].Velocity += vector2 * ((num3 - num4) / 2f) * 1f;
					}
				}
			}
			_body[i].Update(gameTime);
		}
		if (flag)
		{
			foreach (BodySegment item in _body)
			{
				item.WrapOffset = new Vector2(0f, item.WrapOffset.Y);
			}
		}
		if (flag2)
		{
			foreach (BodySegment item2 in _body)
			{
				item2.WrapOffset = new Vector2(item2.WrapOffset.X, 0f);
			}
		}
		if (_boostTimer != 0)
		{
			_boostTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (_boostTimer < 0)
			{
				_boostTimer = 0;
			}
		}
		if (_preferenceChangeTimer != 0)
		{
			_preferenceChangeTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (_preferenceChangeTimer < 0)
			{
				_preferenceChangeTimer = 0;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		for (int num = _body.Count - 1; num >= 0; num--)
		{
			if (num == 0)
			{
				if (_eating)
				{
					_body[num].Scale = 1.2f;
				}
				else
				{
					_body[num].Scale = 1f;
				}
			}
			_body[num].Draw(spriteBatch);
		}
	}

	public void DrawPrefenceBubbles(SpriteBatch spriteBatch)
	{
		if (_preferenceChangeTimer != 0 && _body.Count != 0)
		{
			int num = ((!_centipedeCandidate) ? 1 : 0);
			spriteBatch.Draw(_preferenceBubbles[num], _body[0].Position + new Vector2(0f, -50f), null, Color.White * ((float)_preferenceChangeTimer / 2000f), 0f, _preferenceBubbleOrigin, 1f, SpriteEffects.None, 0f);
		}
	}

	public void Grow()
	{
		BodySegment bodySegment = new BodySegment(this, BodySegment.BodySegmentType.Body, _colour, _body.Count);
		Vector2 vector;
		if (_body.Count == 1)
		{
			vector = new Vector2((float)Math.Sin(_body[0].Rotation), (float)Math.Cos(_body[0].Rotation));
		}
		else
		{
			vector = _body[_body.Count - 1].Position - _body[_body.Count - 2].Position;
			vector.Normalize();
		}
		bodySegment.Position = _body[_body.Count - 1].Position + vector;
		bodySegment.WrapOffset = _body[_body.Count - 1].WrapOffset;
		bodySegment.Load(_contentLoader);
		_body.Add(bodySegment);
		_mushroomCounter++;
		if (_mushroomCounter == 5)
		{
			_mushroomCounter = 0;
			_boostTimer = 3000;
		}
	}

	public void ShowPreferenceBubble()
	{
		_preferenceChangeTimer = 2000;
	}
}
