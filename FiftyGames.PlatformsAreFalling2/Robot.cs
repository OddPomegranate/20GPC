using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.PlatformsAreFalling2;

internal class Robot
{
	private const float Scale = 1f;

	private const float Speed = 0.26f;

	private const float JumpSpeed = 80f;

	private Player _player;

	private PlayerManager _pm;

	private Vector2 _position;

	private Vector2 _drawPosition;

	private Color _colour;

	private float _rotationLean;

	private float _finalRotationLean;

	private float _yOffset;

	private bool _isAlive = true;

	private float _eyeOffset;

	private float _eyeOffsetMax = 15f;

	private float _eyeSpread = 8f;

	private float _eyeSpreadMin = 3f;

	private float _eyeSpreadMax = 8f;

	private int _widthHalf;

	private int _heightHalf;

	private Color _eyeColor = Color.White;

	private bool _isJumping;

	private bool _isOnWall;

	private int _screenWidth;

	private bool _fullyDead;

	private int _score;

	private int _playerNum;

	private Texture2D _bodySprite;

	private Vector2 _bodyOrigin;

	private Texture2D _shirtSprite;

	private Vector2 _shirtOrigin;

	private Texture2D _eyeSprite;

	private Vector2 _eyeOrigin;

	private Texture2D _mouthSprite;

	private Vector2 _mouthOrigin;

	private Texture2D _trollFaceSprite;

	private Vector2 _trollFaceOrigin;

	private Texture2D _foreverAloneSprite;

	private Vector2 _foreverAloneOrigin;

	private bool _isTrolling;

	private int _trollTimer;

	private Body _rectangle;

	private Vector2 _wallDirection;

	private int _jumpForce = 470;

	private float _walkSpeed = 1.3f;

	private List<Contact> _contacts;

	private int _deathFlag;

	private SoundManager _sounds;

	public bool IsAlive => _isAlive;

	public bool FullyDead => _fullyDead;

	public Vector2 Position => _position;

	public Color Colour => _pm.GetPlayerColor(_player);

	public int Score => _score;

	public string Name => _player.Name;

	public int PlayerNum => _playerNum;

	public int ContactCount => _contacts.Count;

	public Robot(Player player, int screenWidth, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
	{
		_player = player;
		_playerNum = playerNum;
		_position = new Vector2((1280 - screenWidth) / 2 + screenWidth / (numOfPlayers + 1) * (_playerNum % numOfPlayers + 1), 550f);
		_drawPosition = _position;
		_pm = pm;
		_colour = pm.GetPlayerColor(player);
		_screenWidth = screenWidth;
		_contacts = new List<Contact>();
		_sounds = sounds;
	}

	public void LoadContent(ContentManager content, World world)
	{
		_bodySprite = content.Load<Texture2D>("PlatformsAreFalling/Sprites/Body");
		_shirtSprite = content.Load<Texture2D>("PlatformsAreFalling/Sprites/Shirt");
		_eyeSprite = content.Load<Texture2D>("PlatformsAreFalling/Sprites/Eye");
		_mouthSprite = content.Load<Texture2D>("PlatformsAreFalling/Sprites/Mouth");
		_widthHalf = _bodySprite.Width / 2;
		_heightHalf = _bodySprite.Height / 2;
		_bodyOrigin = new Vector2(_widthHalf, _heightHalf);
		_shirtOrigin = new Vector2(_bodyOrigin.X - 2f, _bodyOrigin.Y - 35f);
		_eyeOrigin = new Vector2(_bodyOrigin.X - 22f, _bodyOrigin.Y - 15f);
		_mouthOrigin = new Vector2(_bodyOrigin.X - 22f, _bodyOrigin.Y - 22f);
		_trollFaceOrigin = new Vector2(_bodyOrigin.X + 14f, _bodyOrigin.Y + 26f);
		_foreverAloneOrigin = new Vector2(_bodyOrigin.X + 14f, _bodyOrigin.Y + 38f);
		Vertices item = PolygonTools.CreateRectangle(ConvertUnits.ToSimUnits(_widthHalf), ConvertUnits.ToSimUnits(_heightHalf));
		Vertices vertices = PolygonTools.CreateRectangle(ConvertUnits.ToSimUnits(_widthHalf), ConvertUnits.ToSimUnits(_heightHalf));
		Vector2 vector = ConvertUnits.ToSimUnits(new Vector2(_screenWidth, 0f));
		vertices.Translate(ref vector);
		List<Vertices> list = new List<Vertices>(2);
		list.Add(item);
		list.Add(vertices);
		_rectangle = BodyFactory.CreateCompoundPolygon(world, list, 1f);
		_rectangle.BodyType = BodyType.Dynamic;
		_rectangle.UserData = 30 + _playerNum;
		_rectangle.FixedRotation = true;
		_rectangle.CollisionCategories = (Category)(1.0 * Math.Pow(2.0, _playerNum));
		_rectangle.CollidesWith = Category.Cat10;
		_rectangle.LinearDamping = 2f;
		_rectangle.SleepingAllowed = false;
		_rectangle.Mass = 7.5f;
		_rectangle.Position = ConvertUnits.ToSimUnits(_position);
	}

	public void Update(float acidPosition, GameTime gameTime)
	{
		_position = ConvertUnits.ToDisplayUnits(_rectangle.Position);
		if (_isAlive)
		{
			if (_contacts.Count > 1)
			{
				_deathFlag = 0;
				foreach (Contact contact in _contacts)
				{
					contact.GetManifold(out var manifold);
					if (contact.FixtureB != null)
					{
						if ((int)contact.FixtureB.Body.UserData < 30)
						{
							if ((int)contact.FixtureA.Body.UserData < 30)
							{
								manifold.LocalNormal = Vector2.Zero;
							}
							else
							{
								manifold.LocalNormal *= -Vector2.One;
							}
						}
						else if ((int)contact.FixtureA.Body.UserData >= 30)
						{
							manifold.LocalNormal = Vector2.Zero;
						}
					}
					if (manifold.LocalNormal.Y > 0f)
					{
						_deathFlag |= 1;
					}
					else if (manifold.LocalNormal.Y < 0f)
					{
						_deathFlag |= 2;
					}
					if (_deathFlag == 3)
					{
						break;
					}
				}
			}
			if (_deathFlag == 3 || _position.Y + (float)(_heightHalf / 2) > acidPosition)
			{
				_isAlive = false;
				_sounds.CreateGameSoundCue("platformsAreFalling Acid Die").Play();
				_rectangle.CollidesWith = Category.Cat11;
				_rectangle.ApplyLinearImpulse(new Vector2(0f, 45f));
				return;
			}
			_score = Math.Max(_score, (int)(Math.Abs(_position.Y - 552f) / 10f));
			if (_isOnWall)
			{
				_rectangle.LinearVelocity = new Vector2(_rectangle.LinearVelocity.X, Math.Min(7f, _rectangle.LinearVelocity.Y));
			}
			if (_player.GamePadManager.ButtonWasPressed(Buttons.A))
			{
				if (_wallDirection != Vector2.Zero)
				{
					_rectangle.LinearVelocity = new Vector2(_rectangle.LinearVelocity.X, 0f);
					_rectangle.ApplyLinearImpulse(new Vector2((0f - _wallDirection.X) * 0.4f * (float)_jumpForce, -_jumpForce));
					_wallDirection = Vector2.Zero;
					_sounds.CreateGameSoundCue("platformsAreFalling Jump").Play();
				}
				else if (!_isJumping)
				{
					_rectangle.ApplyLinearImpulse(new Vector2(0f, -_jumpForce));
					_sounds.CreateGameSoundCue("platformsAreFalling Jump").Play();
				}
			}
			_rectangle.LinearVelocity += new Vector2(_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X * _walkSpeed, 0f);
			if (_position.X > (float)_screenWidth)
			{
				_rectangle.Position = new Vector2(_rectangle.Position.X - ConvertUnits.ToSimUnits(_screenWidth), _rectangle.Position.Y);
				_position = ConvertUnits.ToDisplayUnits(_rectangle.Position);
			}
			else if (_position.X < 0f)
			{
				_rectangle.Position = new Vector2(_rectangle.Position.X + ConvertUnits.ToSimUnits(_screenWidth), _rectangle.Position.Y);
				_position = ConvertUnits.ToDisplayUnits(_rectangle.Position);
			}
			if (!_isOnWall && Math.Abs(_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X) > 0.1f)
			{
				_rotationLean += 0.26f * (float)(-Math.Sign(_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X));
				_eyeOffset = MathHelper.Clamp(_eyeOffset - _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X, 0f - _eyeOffsetMax, _eyeOffsetMax);
				_eyeSpread = MathHelper.Clamp(_eyeSpreadMax - Math.Abs(_eyeOffset) / 2f, _eyeSpreadMin, _eyeSpreadMax);
				_finalRotationLean = (float)Math.Sin(_rotationLean) * 0.4f;
			}
			else if (!_isOnWall)
			{
				_rotationLean = 0f;
				LinearTendToward(ref _finalRotationLean, 0f, 0.01f);
			}
			_yOffset = Math.Abs(_finalRotationLean * 40f);
		}
		else
		{
			_finalRotationLean += 0.2f;
			if (_position.Y > 1000f)
			{
				_fullyDead = true;
				_rectangle.Dispose();
				_rectangle = null;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, float screenOffset, int alivePlayers)
	{
		for (short num = 0; num != 2; num++)
		{
			spriteBatch.Draw(_bodySprite, new Vector2(_position.X + (float)(_screenWidth * num), _position.Y - _yOffset - screenOffset), null, Color.White, _finalRotationLean, _bodyOrigin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(_shirtSprite, new Vector2(_position.X + (float)(_screenWidth * num), _position.Y - _yOffset - screenOffset), null, _colour, _finalRotationLean, _shirtOrigin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(_eyeSprite, new Vector2(_position.X + (float)(_screenWidth * num), _position.Y - _yOffset - screenOffset), null, _eyeColor, _finalRotationLean, new Vector2(_eyeOrigin.X - _eyeSpread + _eyeOffset, _eyeOrigin.Y), 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(_eyeSprite, new Vector2(_position.X + (float)(_screenWidth * num), _position.Y - _yOffset - screenOffset), null, _eyeColor, _finalRotationLean, new Vector2(_eyeOrigin.X + _eyeSpread + _eyeOffset, _eyeOrigin.Y), 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(_mouthSprite, new Vector2(_position.X + (float)(_screenWidth * num), _position.Y - _yOffset - screenOffset), null, Color.White, _finalRotationLean, new Vector2(_mouthOrigin.X + _eyeOffset, _mouthOrigin.Y), 1f, SpriteEffects.None, 0f);
		}
	}

	private void LinearTendToward(ref float number, float target, float step)
	{
		if (Math.Abs(number) - target < step)
		{
			number = target;
		}
		else if (number - target > step)
		{
			float num = step + number / step * (step / 4f);
			number -= num;
		}
		else if (number - target < 0f - step)
		{
			float num2 = step + (0f - number) / step * (step / 4f);
			number += num2;
		}
	}

	private bool SlameXDirection(Vector2 v1, Vector2 v2)
	{
		if ((v1.X > 0f && v2.X > 0f) || (v1.X < 0f && v2.X < 0f))
		{
			return true;
		}
		return false;
	}

	public void AddContact(ref Contact contact)
	{
		if (!_contacts.Contains(contact) && !ContactDuplicate(contact))
		{
			contact.GetManifold(out var manifold);
			if ((int)contact.FixtureB.Body.UserData - 30 == _playerNum)
			{
				manifold.LocalNormal *= -Vector2.One;
			}
			if (manifold.LocalNormal.Y > 0f)
			{
				_isJumping = false;
				_isOnWall = false;
				_sounds.CreateGameSoundCue("platformsAreFalling Platform Land").Play();
			}
			if (manifold.LocalNormal.X > 0f || manifold.LocalNormal.X < 0f)
			{
				_wallDirection = manifold.LocalNormal;
				_isOnWall = true;
			}
			_contacts.Add(contact);
		}
	}

	public bool ContactDuplicate(Contact contactA)
	{
		foreach (Contact contact in _contacts)
		{
			if (contactA.FixtureA == contact.FixtureB && contact.FixtureA == contactA.FixtureB)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveContact(ref Contact contact)
	{
		if (_contacts.Contains(contact))
		{
			contact.GetManifold(out var manifold);
			if ((int)contact.FixtureB.Body.UserData - 30 == _playerNum)
			{
				manifold.LocalNormal *= -Vector2.One;
			}
			if (manifold.LocalNormal.X > 0f || manifold.LocalNormal.X < 0f)
			{
				_wallDirection = Vector2.Zero;
				_isOnWall = false;
			}
			if (manifold.LocalNormal.Y > 0f)
			{
				_isJumping = true;
			}
			_contacts.Remove(contact);
		}
	}
}
