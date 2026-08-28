using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.HammerFight;

internal class Fighter
{
	public class HandState
	{
		public int ContactCounter;

		public bool IsAttacking;
	}

	private const float Speed = 2f;

	private Player _player;

	private PlayerManager _pm;

	private Vector2 _position;

	private Color _colour;

	private bool _isAlive;

	private Texture2D _chainTexture;

	private Texture2D _rockTexture;

	private Texture2D _handTexture;

	private Texture2D _grabberTexture;

	private Texture2D _haloTexture;

	private Texture2D _hornTexture;

	private Texture2D _skullTexture;

	private Color _eyeColor = Color.White;

	private int _score;

	private int _playerNum;

	private int _numOfPlayers;

	private int _idRange;

	private int _timer;

	private float _width;

	private float _height;

	private Vector2[] _indexBuffer;

	private List<Vertices> _brokenShape;

	private World _world;

	private BreakableBody _breakableBody;

	private TimedBreakablePiece[] _breakablePieces;

	private Body _rock;

	private Body _lock;

	private Body _horn1;

	private Body _horn2;

	private List<Body> _chainBodies;

	private bool _isGrabbing;

	private bool _isAttached;

	private bool _isAttacking;

	private RevoluteJoint _attachedJoint;

	private Body _attachedBody;

	private HandState _handState;

	private bool _isFullyDead;

	private SoundManager _sounds;

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

	public Vector2 Position => _position;

	public Color Colour => _colour;

	public int Score => _score;

	public int PlayerNum => _playerNum;

	public string Name => _player.Name;

	public Fighter(Player player, int numOfPlayers, int playerNum, ref PlayerManager pm, ref SoundManager sounds)
	{
		_player = player;
		_playerNum = playerNum;
		_numOfPlayers = numOfPlayers;
		_pm = pm;
		_colour = pm.GetPlayerColor(player, 0.8f, 1f);
		_sounds = sounds;
	}

	public void LoadContent(ContentManager content, GraphicsDevice gd, World world)
	{
		_chainTexture = content.Load<Texture2D>("Hammerfight/Sprites/bone");
		_rockTexture = content.Load<Texture2D>("Hammerfight/Sprites/ball");
		_handTexture = content.Load<Texture2D>("Hammerfight/Sprites/hand1");
		_grabberTexture = content.Load<Texture2D>("Hammerfight/Sprites/hand2");
		_haloTexture = content.Load<Texture2D>("Hammerfight/Sprites/halo");
		_hornTexture = content.Load<Texture2D>("Hammerfight/Sprites/horn");
		_skullTexture = content.Load<Texture2D>("Hammerfight/Sprites/skull");
		PolygonTools.CreateRectangle(1f, 1f);
		_world = world;
		_indexBuffer = new Vector2[20]
		{
			new Vector2(72f, 37f),
			new Vector2(136f, 0f),
			new Vector2(341f, 37f),
			new Vector2(377f, 99f),
			new Vector2(399f, 186f),
			new Vector2(19f, 99f),
			new Vector2(4f, 299f),
			new Vector2(5f, 379f),
			new Vector2(76f, 394f),
			new Vector2(85f, 456f),
			new Vector2(315f, 456f),
			new Vector2(323f, 394f),
			new Vector2(3f, 186f),
			new Vector2(399f, 299f),
			new Vector2(403f, 379f),
			new Vector2(147f, 492f),
			new Vector2(249f, 492f),
			new Vector2(272f, 354f),
			new Vector2(165f, 153f),
			new Vector2(275f, 0f)
		};
		for (int i = 0; i != _indexBuffer.Length; i++)
		{
			_indexBuffer[i].X /= 160f;
			_indexBuffer[i].Y /= 160f;
			_width = ((_indexBuffer[i].X > _width) ? _indexBuffer[i].X : _width);
			_height = ((_indexBuffer[i].Y > _height) ? _indexBuffer[i].Y : _height);
		}
		_brokenShape = new List<Vertices>
		{
			new Vertices
			{
				_indexBuffer[0],
				_indexBuffer[1],
				_indexBuffer[19],
				_indexBuffer[2],
				_indexBuffer[18],
				_indexBuffer[12],
				_indexBuffer[5]
			},
			new Vertices
			{
				_indexBuffer[18],
				_indexBuffer[2],
				_indexBuffer[3],
				_indexBuffer[4],
				_indexBuffer[13],
				_indexBuffer[17]
			},
			new Vertices
			{
				_indexBuffer[13],
				_indexBuffer[14],
				_indexBuffer[11],
				_indexBuffer[17]
			},
			new Vertices
			{
				_indexBuffer[17],
				_indexBuffer[11],
				_indexBuffer[10],
				_indexBuffer[16],
				_indexBuffer[15],
				_indexBuffer[9],
				_indexBuffer[8]
			},
			new Vertices
			{
				_indexBuffer[18],
				_indexBuffer[17],
				_indexBuffer[8],
				_indexBuffer[7],
				_indexBuffer[6],
				_indexBuffer[12]
			}
		};
		ReloadContent(gd);
	}

	public void ResetPosition()
	{
		_lock.Position = _position;
		_horn1.Position = Vector2.Zero;
		_horn1.IgnoreGravity = true;
		_horn2.Position = Vector2.Zero;
		_horn2.IgnoreGravity = true;
		_breakableBody.MainBody.Position = _position;
		for (int i = 0; i < _chainBodies.Count; i++)
		{
			_chainBodies[i].Position = _position + new Vector2(0f, 0.7f * (float)i);
			_chainBodies[i].LinearVelocity = Vector2.Zero;
			_chainBodies[i].AngularVelocity = 0f;
		}
		_isAttached = false;
		_isAttacking = false;
		_isGrabbing = false;
		_handState = new HandState();
		_rock.UserData = _handState;
		_isFullyDead = false;
		((HandState)_rock.UserData).ContactCounter = 0;
		_player.GamePadManager.StartVibration(400);
	}

	public void ReloadContent(GraphicsDevice gd)
	{
		_isFullyDead = false;
		_isAlive = true;
		_position = new Vector2(ConvertUnits.ToSimUnits(140) + ConvertUnits.ToSimUnits(900) * (float)(_playerNum % 2), ConvertUnits.ToSimUnits(150) + ConvertUnits.ToSimUnits(300) * (float)(_playerNum / 2));
		_isAttached = false;
		_isAttacking = false;
		_isGrabbing = false;
		_idRange = BreakablePiece.CurrentId;
		_breakablePieces = new TimedBreakablePiece[_brokenShape.Count];
		Matrix projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(gd.Viewport.Width), ConvertUnits.ToSimUnits(gd.Viewport.Height), 0f, 0f, 1f);
		for (int i = 0; i != _brokenShape.Count; i++)
		{
			_breakablePieces[i] = new TimedBreakablePiece(_brokenShape[i], _width, _height, gd, projection, 3000);
			_breakablePieces[i].LoadContent(_skullTexture);
		}
		_lock = BodyFactory.CreateRectangle(_world, _width, _height, 1f);
		_lock.BodyType = BodyType.Dynamic;
		_lock.Position = _position;
		_lock.CollidesWith = CategoryHelper(5) | Category.Cat1;
		_lock.CollisionCategories = (Category)Math.Pow(2.0, _playerNum + 5);
		_lock.FixedRotation = true;
		_lock.IgnoreGravity = true;
		_lock.SleepingAllowed = false;
		_lock.LinearDamping = 5f;
		_horn1 = BodyFactory.CreateRectangle(_world, _width, _height, 1f);
		_horn1.BodyType = BodyType.Dynamic;
		_horn1.CollidesWith = Category.None;
		_horn1.IgnoreGravity = true;
		_horn2 = BodyFactory.CreateRectangle(_world, _width, _height, 1f);
		_horn2.BodyType = BodyType.Dynamic;
		_horn2.CollidesWith = Category.None;
		_horn2.IgnoreGravity = true;
		_breakableBody = new BreakableBody(_brokenShape, _world, 2f);
		_breakableBody.MainBody.Position = _position;
		_breakableBody.MainBody.CollidesWith = (Category)((int)CategoryHelper(15) & ~(int)Math.Pow(2.0, _playerNum + 15));
		_breakableBody.MainBody.CollisionCategories = (Category)Math.Pow(2.0, _playerNum % 2 + 10);
		_breakableBody.MainBody.FixedRotation = true;
		_breakableBody.MainBody.IgnoreGravity = true;
		_breakableBody.MainBody.SleepingAllowed = false;
		_breakableBody.MainBody.LinearDamping = 5f;
		_breakableBody.Strength = 25f;
		_breakableBody.MainBody.UserData = "hit";
		for (int j = 0; j != _breakableBody.Parts.Count; j++)
		{
			_breakableBody.Parts[j].UserData = _breakablePieces[j].ID;
			_breakablePieces[j].Fixture = _breakableBody.MainBody.FixtureList[0];
		}
		BreakablePiece.CurrentId += 100;
		BreakablePiece.CurrentId = BreakablePiece.CurrentId / 100 * 100;
		_world.AddBreakableBody(_breakableBody);
		_handState = new HandState();
		_rock = BodyFactory.CreateCircle(_world, 0.625f, 0.5f);
		_rock.UserData = _handState;
		_rock.Position = new Vector2(_position.X, _position.Y + 4f);
		_rock.Mass = 0.15f;
		_rock.BodyType = BodyType.Dynamic;
		_rock.CollisionCategories = (Category)Math.Pow(2.0, _playerNum + 15);
		_rock.CollidesWith = (Category)((int)(CategoryHelper(10) | Category.Cat1 | Category.Cat3) & ~(int)Math.Pow(2.0, _playerNum % 2 + 10));
		_rock.SleepingAllowed = false;
		_rock.OnCollision += rock_OnCollision;
		Path path = new Path();
		path.Add(_breakableBody.MainBody.Position);
		path.Add(_rock.Position);
		path.Closed = false;
		Vertices vertices = PolygonTools.CreateRectangle(ConvertUnits.ToSimUnits(5), ConvertUnits.ToSimUnits(10));
		PolygonShape shape = new PolygonShape(vertices, 10f);
		_chainBodies = PathManager.EvenlyDistributeShapesAlongPath(_world, path, shape, BodyType.Dynamic, 7);
		foreach (Body chainBody in _chainBodies)
		{
			chainBody.Mass = 0.1f;
			chainBody.CollisionCategories = Category.Cat2;
			chainBody.CollidesWith = Category.Cat1 | Category.Cat4 | Category.Cat5 | Category.Cat6 | Category.Cat7 | Category.Cat8 | Category.Cat9 | Category.Cat10 | Category.Cat11 | Category.Cat12 | Category.Cat13 | Category.Cat14 | Category.Cat15 | Category.Cat16 | Category.Cat17 | Category.Cat18 | Category.Cat19 | Category.Cat20 | Category.Cat21 | Category.Cat22 | Category.Cat23 | Category.Cat24 | Category.Cat25 | Category.Cat26 | Category.Cat27 | Category.Cat28 | Category.Cat29 | Category.Cat30 | Category.Cat31;
		}
		_chainBodies.Insert(0, _lock);
		_chainBodies.Add(_rock);
		PathManager.AttachBodiesWithRevoluteJoint(_world, _chainBodies, new Vector2(0f, -0.35f), new Vector2(0f, 0.35f), connectFirstAndLast: false, collideConnected: false);
		_player.GamePadManager.StartVibration(400);
	}

	public static Category CategoryHelper(int start)
	{
		return (Category)((int)Math.Pow(2.0, start) | (int)Math.Pow(2.0, start + 1) | (int)Math.Pow(2.0, start + 2) | (int)Math.Pow(2.0, start + 3));
	}

	public void Update(GraphicsDevice gd, GameTime gameTime, List<Fixture> breakables)
	{
		_timer += gameTime.ElapsedGameTime.Milliseconds;
		if (((HandState)_rock.UserData).ContactCounter < 0)
		{
			((HandState)_rock.UserData).ContactCounter = 0;
		}
		for (int i = 0; i != _breakablePieces.Length; i++)
		{
			_breakablePieces[i].Update(gameTime, _breakableBody.Broken);
		}
		if (!_breakableBody.Broken)
		{
			if (_isAlive)
			{
				_breakableBody.MainBody.Position = _lock.Position - _breakableBody.MainBody.LocalCenter;
				_lock.LinearVelocity += new Vector2(_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X * 2f, (0f - _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y) * 2f);
				_rock.IsSensor = !_isAttacking;
				if (!_isGrabbing)
				{
					if (_player.GamePadManager.ButtonIsHeld(Buttons.B) && !_player.GamePadManager.ButtonWasReleased(Buttons.A))
					{
						if (((HandState)_rock.UserData).ContactCounter == 0 && _timer > 100)
						{
							_isAttacking = true;
						}
					}
					else if (_player.GamePadManager.ButtonWasReleased(Buttons.B))
					{
						_isAttacking = false;
					}
					((HandState)_rock.UserData).IsAttacking = _isAttacking;
				}
				if (_isAttacking)
				{
					return;
				}
				if (_player.GamePadManager.ButtonIsHeld(Buttons.A))
				{
					_isGrabbing = true;
					if (_isAttached || _attachedBody == null || _attachedBody.FixtureList == null)
					{
						return;
					}
					Vector2 point = _rock.Position;
					foreach (Fixture fixture in _attachedBody.FixtureList)
					{
						if (fixture.TestPoint(ref point))
						{
							_isAttached = true;
							break;
						}
					}
					if (_isAttached)
					{
						_attachedBody.IgnoreGravity = false;
						_attachedBody.Mass = 0.05f;
						_attachedJoint = new RevoluteJoint(_rock, _attachedBody, Vector2.Zero, _rock.Position - _attachedBody.Position);
						_world.AddJoint(_attachedJoint);
						((Diamond.DiamondGrab)_attachedBody.UserData).GrabCount++;
					}
				}
				else
				{
					if (!_player.GamePadManager.ButtonWasReleased(Buttons.A))
					{
						return;
					}
					_timer = 0;
					_isGrabbing = false;
					if (!_isAttached)
					{
						return;
					}
					_isAttached = false;
					if (_attachedBody != null && _attachedBody.UserData != null)
					{
						((Diamond.DiamondGrab)_attachedBody.UserData).GrabCount--;
						if (((Diamond.DiamondGrab)_attachedBody.UserData).GrabCount == 0)
						{
							_attachedBody.Mass = 1.4f;
						}
					}
					_world.RemoveJoint(_attachedJoint);
					_attachedBody = null;
				}
			}
			else
			{
				_breakableBody.Break();
				_sounds.CreateGameSoundCue("hammer Smash Player").Play();
			}
			return;
		}
		if (_isAlive)
		{
			_sounds.CreateGameSoundCue("hammer Smash Player").Play();
			_isAlive = false;
			_horn1.IgnoreGravity = false;
			_horn1.Position = _lock.Position;
			_horn1.LinearVelocity = _lock.LinearVelocity;
			_horn1.AngularVelocity = -1.4f;
			_horn2.IgnoreGravity = false;
			_horn2.Position = _lock.Position;
			_horn2.LinearVelocity = _lock.LinearVelocity * new Vector2(-1f, 0f);
			_horn2.AngularVelocity = 1.4f;
		}
		if (!_isFullyDead)
		{
			_isFullyDead = true;
			_player.GamePadManager.StartVibration(400);
		}
		if (_isAttached)
		{
			_world.RemoveJoint(_attachedJoint);
			_attachedBody = null;
			_isAttached = false;
		}
		if (_lock.IgnoreGravity)
		{
			_lock.IgnoreGravity = false;
			_lock.CollidesWith = Category.None;
			_lock.CollisionCategories = Category.None;
			_lock.Mass = 0.1f;
			_timer = 0;
		}
		foreach (Body chainBody in _chainBodies)
		{
			if (chainBody.FixtureList != null)
			{
				chainBody.CollidesWith = Category.None;
				chainBody.CollisionCategories = Category.None;
			}
		}
		if (_rock.FixtureList != null)
		{
			_rock.CollidesWith = Category.None;
			_rock.CollisionCategories = Category.None;
		}
		for (int j = 0; j != breakables.Count; j++)
		{
			if (breakables[j].UserData != null && (int)breakables[j].UserData >= _idRange && (int)breakables[j].UserData < _idRange + 100)
			{
				_breakablePieces[(int)breakables[j].UserData % 100].Fixture = breakables[j];
				_breakablePieces[(int)breakables[j].UserData % 100].Fixture.Body.CollidesWith = Category.Cat1;
				_breakablePieces[(int)breakables[j].UserData % 100].Fixture.Body.CollisionCategories = Category.Cat2;
				breakables[j].UserData = null;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		foreach (Body chainBody in _chainBodies)
		{
			spriteBatch.Draw(_chainTexture, new Rectangle((int)ConvertUnits.ToDisplayUnits(chainBody.Position.X), (int)ConvertUnits.ToDisplayUnits(chainBody.Position.Y), 10, 20), null, _colour, chainBody.Rotation, new Vector2(5f, 10f), SpriteEffects.None, 0f);
		}
		if (_playerNum % 2 == 0)
		{
			if (_isAlive)
			{
				spriteBatch.Draw(_haloTexture, new Vector2(ConvertUnits.ToDisplayUnits(_lock.Position.X), (int)ConvertUnits.ToDisplayUnits(_lock.Position.Y)), null, _colour, 0f, new Vector2(167f, 250f), 0.25f, SpriteEffects.None, 0f);
			}
			else
			{
				spriteBatch.Draw(_haloTexture, new Vector2(ConvertUnits.ToDisplayUnits(_horn1.Position.X), (int)ConvertUnits.ToDisplayUnits(_horn1.Position.Y)), null, _colour, _horn1.Rotation, new Vector2(167f, 250f), 0.25f, SpriteEffects.None, 0f);
			}
		}
		else if (_isAlive)
		{
			spriteBatch.Draw(_hornTexture, new Vector2(ConvertUnits.ToDisplayUnits(_lock.Position.X), (int)ConvertUnits.ToDisplayUnits(_lock.Position.Y)), null, _colour, 0f, new Vector2(167f, 250f), 0.25f, SpriteEffects.None, 0f);
			spriteBatch.Draw(_hornTexture, new Vector2(ConvertUnits.ToDisplayUnits(_lock.Position.X), (int)ConvertUnits.ToDisplayUnits(_lock.Position.Y)), null, _colour, 0f, new Vector2(167f, 250f), 0.25f, SpriteEffects.FlipHorizontally, 0f);
		}
		else
		{
			spriteBatch.Draw(_hornTexture, new Vector2(ConvertUnits.ToDisplayUnits(_horn1.Position.X), (int)ConvertUnits.ToDisplayUnits(_horn1.Position.Y)), null, _colour, _horn1.Rotation, new Vector2(167f, 250f), 0.25f, SpriteEffects.None, 0f);
			spriteBatch.Draw(_hornTexture, new Vector2(ConvertUnits.ToDisplayUnits(_horn2.Position.X), (int)ConvertUnits.ToDisplayUnits(_horn2.Position.Y)), null, _colour, _horn2.Rotation, new Vector2(167f, 250f), 0.25f, SpriteEffects.FlipHorizontally, 0f);
		}
		if (_isAttacking)
		{
			spriteBatch.Draw(_rockTexture, new Rectangle((int)ConvertUnits.ToDisplayUnits(_rock.Position.X), (int)ConvertUnits.ToDisplayUnits(_rock.Position.Y), 34, 34), null, _colour, _rock.Rotation, new Vector2(16f, 16f), SpriteEffects.None, 0f);
		}
		else if (_isGrabbing)
		{
			spriteBatch.Draw(_grabberTexture, new Rectangle((int)ConvertUnits.ToDisplayUnits(_rock.Position.X), (int)ConvertUnits.ToDisplayUnits(_rock.Position.Y), 34, 34), null, _colour, _rock.Rotation, new Vector2(16f, 16f), SpriteEffects.None, 0f);
		}
		else
		{
			spriteBatch.Draw(_handTexture, new Rectangle((int)ConvertUnits.ToDisplayUnits(_rock.Position.X), (int)ConvertUnits.ToDisplayUnits(_rock.Position.Y), 34, 34), null, _colour, _rock.Rotation, new Vector2(16f, 16f), SpriteEffects.None, 0f);
		}
		spriteBatch.End();
		TimedBreakablePiece[] breakablePieces = _breakablePieces;
		foreach (BreakablePiece breakablePiece in breakablePieces)
		{
			breakablePiece.Draw(spriteBatch.GraphicsDevice, _colour);
		}
		spriteBatch.Begin();
	}

	private bool rock_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (_isGrabbing && !_isAttached)
		{
			if (fixtureB.Body.UserData != null && fixtureB.Body.UserData is Diamond.DiamondGrab)
			{
				_attachedBody = fixtureB.Body;
			}
		}
		else if (!_isGrabbing && !_isAttached && _isAttacking && fixtureB.Body != null && fixtureB.Body.UserData as string == "hit")
		{
			_sounds.CreateGameSoundCue("hammer Hit").Play();
		}
		return true;
	}
}
