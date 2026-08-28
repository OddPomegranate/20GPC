using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.HammerFight;

internal class HammerFight : Minigame
{
	private class Goal
	{
		public List<int> DiamondIDs;

		private List<int> BannedIDs;

		public Goal()
		{
			DiamondIDs = new List<int>();
			BannedIDs = new List<int>();
		}

		public void Add(int id)
		{
			if (!DiamondIDs.Contains(id) && !BannedIDs.Contains(id))
			{
				DiamondIDs.Add(id);
			}
		}

		public void Remove(int id)
		{
			if (DiamondIDs.Contains(id))
			{
				DiamondIDs.Remove(id);
			}
		}

		public void RemoveForever(int id)
		{
			if (!BannedIDs.Contains(id))
			{
				BannedIDs.Add(id);
			}
			if (DiamondIDs.Contains(id))
			{
				DiamondIDs.Remove(id);
			}
		}
	}

	private SpriteBatch _spriteBatch;

	private SpriteFont _font;

	private Texture2D _background;

	private Texture2D _wallTexture;

	private Texture2D _pixelTexture;

	private List<int> _winners;

	private Random _random;

	private World _world;

	private List<Fixture> _breakables;

	private int _timer;

	private bool _isFinished;

	private bool _isReset;

	private float _screenWipeAlpha;

	private List<Fighter> _fighters;

	private List<Diamond> _diamonds;

	private Body _wall;

	private Vector2[] _wallSizes;

	private Vector2[] _wallPositions;

	private Body[] _goalBodies;

	private int[] _scores;

	private bool _soundPlayed;

	public HammerFight(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
	}

	public override void Initialize()
	{
		_random = new Random();
		ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
		_world = new World(new Vector2(0f, 50f));
		if (!_demoMode)
		{
			ContactManager contactManager = _world.ContactManager;
			contactManager.BeginContact = (BeginContactDelegate)Delegate.Combine(contactManager.BeginContact, new BeginContactDelegate(BeginContact));
			ContactManager contactManager2 = _world.ContactManager;
			contactManager2.EndContact = (EndContactDelegate)Delegate.Combine(contactManager2.EndContact, new EndContactDelegate(EndContact));
		}
		base.Initialize();
	}

	private bool BeginContact(Contact contact)
	{
		if (contact.FixtureA.Body.UserData != null && contact.FixtureB.Body.UserData != null)
		{
			if (contact.FixtureA.Body.UserData is Diamond.DiamondGrab)
			{
				if (contact.FixtureB.Body.UserData is Diamond.DiamondGrab)
				{
					contact.FixtureA.Body.LinearVelocity = Vector2.Zero;
					contact.FixtureB.Body.LinearVelocity = Vector2.Zero;
				}
				else if (contact.FixtureB.Body.UserData is Fighter.HandState)
				{
					if (((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter < 0)
					{
						((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter = 0;
					}
					((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter++;
					if (((Fighter.HandState)contact.FixtureB.Body.UserData).IsAttacking)
					{
						contact.FixtureA.Body.LinearVelocity = Vector2.Zero;
					}
				}
				else if (contact.FixtureB.Body.UserData is Goal)
				{
					((Goal)contact.FixtureB.Body.UserData).Add(((Diamond.DiamondGrab)contact.FixtureA.Body.UserData).ID);
				}
			}
			else if (contact.FixtureA.Body.UserData is Goal)
			{
				if (contact.FixtureB.Body.UserData is Diamond.DiamondGrab)
				{
					((Goal)contact.FixtureA.Body.UserData).Add(((Diamond.DiamondGrab)contact.FixtureB.Body.UserData).ID);
				}
			}
			else if ((string)contact.FixtureA.Body.UserData == "wall" && contact.FixtureB.Body.UserData is Fighter.HandState)
			{
				if (((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter < 0)
				{
					((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter = 0;
				}
				((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter++;
			}
		}
		return true;
	}

	private void EndContact(Contact contact)
	{
		if (contact.FixtureA.Body.UserData == null || contact.FixtureB.Body.UserData == null)
		{
			return;
		}
		if (contact.FixtureA.Body.UserData is Diamond.DiamondGrab)
		{
			if (contact.FixtureB.Body.UserData == null)
			{
				return;
			}
			if (contact.FixtureB.Body.UserData is Fighter.HandState)
			{
				((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter--;
				if (((Fighter.HandState)contact.FixtureB.Body.UserData).IsAttacking)
				{
					contact.FixtureA.Body.LinearVelocity = Vector2.Zero;
				}
			}
			else if (contact.FixtureB.Body.UserData is Goal)
			{
				((Goal)contact.FixtureB.Body.UserData).Remove(((Diamond.DiamondGrab)contact.FixtureA.Body.UserData).ID);
			}
		}
		else if (contact.FixtureA.Body.UserData is Goal)
		{
			if (contact.FixtureB.Body.UserData is Diamond.DiamondGrab)
			{
				((Goal)contact.FixtureA.Body.UserData).Remove(((Diamond.DiamondGrab)contact.FixtureB.Body.UserData).ID);
			}
		}
		else if ((string)contact.FixtureA.Body.UserData == "wall" && contact.FixtureB.Body.UserData is Fighter.HandState)
		{
			((Fighter.HandState)contact.FixtureB.Body.UserData).ContactCounter--;
		}
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_winners = new List<int>();
		_background = _contentManager.Load<Texture2D>("Hammerfight/Sprites/bg");
		_wallTexture = _contentManager.Load<Texture2D>("Hammerfight/Sprites/wall2");
		_fighters = new List<Fighter>();
		_diamonds = new List<Diamond>();
		_breakables = new List<Fixture>();
		_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_wallSizes = new Vector2[4]
		{
			new Vector2(ConvertUnits.ToSimUnits(1400), ConvertUnits.ToSimUnits(20)),
			new Vector2(ConvertUnits.ToSimUnits(20), ConvertUnits.ToSimUnits(800)),
			new Vector2(ConvertUnits.ToSimUnits(30), ConvertUnits.ToSimUnits(340)),
			new Vector2(ConvertUnits.ToSimUnits(30), ConvertUnits.ToSimUnits(340))
		};
		_wallPositions = new Vector2[6]
		{
			new Vector2(ConvertUnits.ToSimUnits(640), 0f),
			new Vector2(ConvertUnits.ToSimUnits(640), ConvertUnits.ToSimUnits(720)),
			new Vector2(0f, ConvertUnits.ToSimUnits(400)),
			new Vector2(ConvertUnits.ToSimUnits(1280), ConvertUnits.ToSimUnits(400)),
			new Vector2(ConvertUnits.ToSimUnits(340), ConvertUnits.ToSimUnits(545)),
			new Vector2(ConvertUnits.ToSimUnits(940), ConvertUnits.ToSimUnits(545))
		};
		_pixelTexture = new Texture2D(base.GraphicsDevice, 1, 1);
		Color[] data = new Color[1] { Color.White };
		_pixelTexture.SetData(data);
		ReloadContent();
	}

	private void ReloadContent()
	{
		_winners.Clear();
		_world.Clear();
		_fighters.Clear();
		_diamonds.Clear();
		_breakables.Clear();
		BreakablePiece.CurrentId = 0;
		_timer = 0;
		int[] scores = new int[2];
		_scores = scores;
		for (int i = (_demoMode ? 1 : 0); i != 6; i++)
		{
			_wall = BodyFactory.CreateRectangle(_world, _wallSizes[i / 2].X, _wallSizes[i / 2].Y, 1f);
			_wall.BodyType = BodyType.Static;
			_wall.Position = _wallPositions[i];
			_wall.CollisionCategories = Category.Cat1;
		}
		_goalBodies = new Body[2]
		{
			BodyFactory.CreateRectangle(_world, _wallPositions[4].X, _wallSizes[3].Y, 1f),
			BodyFactory.CreateRectangle(_world, _wallPositions[4].X, _wallSizes[3].Y, 1f)
		};
		_goalBodies[0].BodyType = BodyType.Static;
		_goalBodies[0].IsSensor = true;
		_goalBodies[0].Position = new Vector2(_wallPositions[4].X / 2f, _wallPositions[4].Y);
		_goalBodies[0].UserData = new Goal();
		_goalBodies[1].BodyType = BodyType.Static;
		_goalBodies[1].IsSensor = true;
		_goalBodies[1].Position = new Vector2(_wallPositions[5].X + _wallPositions[4].X / 2f, _wallPositions[4].Y);
		_goalBodies[1].UserData = new Goal();
		List<int> list2;
		if (_playerManager.PlayersConnected.Count == 2)
		{
			List<int> list = new List<int>();
			list.Add(0);
			list.Add(1);
			list2 = list;
		}
		else
		{
			List<int> list3 = new List<int>();
			list3.Add(0);
			list3.Add(1);
			list3.Add(2);
			list3.Add(3);
			list2 = list3;
		}
		Helper.Shuffle(list2, _random);
		if (!_demoMode)
		{
			for (int j = 0; j != 4; j++)
			{
				if (j < _playerManager.PlayersConnected.Count)
				{
					_fighters.Add(new Fighter(_playerManager.PlayersConnected[j], _playerManager.PlayersConnected.Count, list2[j], ref _playerManager, ref _soundManager));
					_fighters[j].LoadContent(_contentManager, base.Game.GraphicsDevice, _world);
					_winners.Add(j);
				}
				_diamonds.Add(new Diamond(_playerManager.PlayersConnected.Count, j, _demoMode, this, _soundManager));
				_diamonds[j].LoadContent(_contentManager, base.Game.GraphicsDevice, _world);
			}
			_font = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		}
		else
		{
			for (int k = 0; k != 4; k++)
			{
				_diamonds.Add(new Diamond(k, _demoMode));
				_diamonds[k].LoadContent(_contentManager, base.Game.GraphicsDevice, _world);
			}
		}
	}

	public override void Update(GameTime gameTime)
	{
		_world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 1f / 30f));
		_timer += gameTime.ElapsedGameTime.Milliseconds;
		for (int i = 0; i != _breakables.Count; i++)
		{
			if (_breakables[i].Body == null || _breakables[i].Body.IsDisposed)
			{
				_breakables.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j != _world.BodyList.Count; j++)
		{
			if (!_world.BodyList[j].IsDisposed && _world.BodyList[j].Position.Y > ConvertUnits.ToSimUnits(850))
			{
				_world.BodyList[j].Dispose();
			}
		}
		for (int k = 0; k != _world.BodyList.Count; k++)
		{
			if (_world.BodyList[k].FixtureList.Count == 1 && _world.BodyList[k].FixtureList[0].UserData != null && !_breakables.Contains(_world.BodyList[k].FixtureList[0]))
			{
				while (_breakables.Count > 24)
				{
					_breakables[0].Dispose();
					_breakables.RemoveAt(0);
				}
				_breakables.Add(_world.BodyList[k].FixtureList[0]);
			}
		}
		if (!_demoMode)
		{
			if (_isFinished)
			{
				_screenWipeAlpha += (float)(gameTime.ElapsedGameTime.Milliseconds * (1 - _timer / 500)) / 500f;
			}
			if (!_isFinished && _timer > 20000)
			{
				_timer = 0;
				_screenWipeAlpha = 0f;
				_isFinished = true;
				_isReset = false;
			}
			else if (_isFinished && _timer > 2000)
			{
				_isFinished = false;
			}
			else if (_isFinished && !_isReset && _timer > 500)
			{
				for (int l = 0; l != _fighters.Count; l++)
				{
					if (!_fighters[l].IsAlive)
					{
						_fighters[l].ReloadContent(base.GraphicsDevice);
					}
					else
					{
						_fighters[l].ResetPosition();
					}
				}
				if (_diamonds.Count > 0)
				{
					foreach (Diamond diamond in _diamonds)
					{
						if (diamond.IsBroken)
						{
							((Goal)_goalBodies[0].UserData).Remove(diamond.ID);
							((Goal)_goalBodies[1].UserData).Remove(diamond.ID);
						}
						diamond.Dispose();
					}
					_diamonds.Clear();
				}
				for (int m = 0; m != 4; m++)
				{
					_diamonds.Add(new Diamond(_playerManager.PlayersConnected.Count, m, _demoMode, this, _soundManager));
					_diamonds[m].LoadContent(_contentManager, base.Game.GraphicsDevice, _world);
				}
				_scores[0] += ((Goal)_goalBodies[0].UserData).DiamondIDs.Count;
				_scores[1] += ((Goal)_goalBodies[1].UserData).DiamondIDs.Count;
				_isReset = true;
				_soundPlayed = false;
			}
			else if (_isFinished && !_isReset && !_soundPlayed)
			{
				_soundManager.CreateGameSoundCue("hammer Round Complete").Play();
				_soundPlayed = true;
			}
			for (int n = 0; n != _fighters.Count; n++)
			{
				_fighters[n].Update(base.GraphicsDevice, gameTime, _breakables);
			}
		}
		else if (_timer > 2000)
		{
			_timer -= 2000;
			while (_diamonds.Count > 30)
			{
				_diamonds[0].Dispose();
				_diamonds.RemoveAt(0);
			}
			_diamonds.Add(new Diamond(_random.Next(0, 4), _demoMode));
			_diamonds[_diamonds.Count - 1].LoadContent(_contentManager, base.Game.GraphicsDevice, _world);
		}
		for (int num = 0; num != _diamonds.Count; num++)
		{
			_diamonds[num].Update(gameTime, _breakables);
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.White);
		_spriteBatch.Begin();
		_spriteBatch.Draw(_background, new Rectangle(0, 0, 1280, 720), null, Color.White);
		foreach (Diamond diamond in _diamonds)
		{
			diamond.Draw(_spriteBatch);
		}
		if (!_demoMode)
		{
			foreach (Fighter fighter in _fighters)
			{
				fighter.Draw(_spriteBatch);
			}
			Helper.DrawOutlinedText(_spriteBatch, _font, ((20000 - _timer) / 1000).ToString(), new Vector2(_titleSafeArea.Left + _titleSafeArea.Width / 2, _titleSafeArea.Top + 60), Color.White, Color.Black, Helper.OutlineType.Orthogonal, centered: true, 2f);
			Helper.DrawOutlinedText(_spriteBatch, _font, (_scores[0] + ((Goal)_goalBodies[0].UserData).DiamondIDs.Count).ToString(), new Vector2(_titleSafeArea.Left + 300, _titleSafeArea.Top + 60), Color.White, Color.Black, Helper.OutlineType.Orthogonal, centered: true, 2f);
			Helper.DrawOutlinedText(_spriteBatch, _font, (_scores[1] + ((Goal)_goalBodies[1].UserData).DiamondIDs.Count).ToString(), new Vector2(_titleSafeArea.Right - 300, _titleSafeArea.Top + 60), Color.White, Color.Black, Helper.OutlineType.Orthogonal, centered: true, 2f);
			_spriteBatch.Draw(_pixelTexture, new Rectangle(0, 720 - (int)ConvertUnits.ToDisplayUnits(_wallSizes[3].Y), (int)ConvertUnits.ToDisplayUnits(_wallPositions[4].X), (int)ConvertUnits.ToDisplayUnits(_wallSizes[3].Y)), Color.White * 0.3f);
			_spriteBatch.Draw(_pixelTexture, new Rectangle(1280 - (int)ConvertUnits.ToDisplayUnits(_wallPositions[4].X), 720 - (int)ConvertUnits.ToDisplayUnits(_wallSizes[3].Y), (int)ConvertUnits.ToDisplayUnits(_wallPositions[4].X), (int)ConvertUnits.ToDisplayUnits(_wallSizes[3].Y)), Color.Black * 0.3f);
		}
		_spriteBatch.Draw(_wallTexture, ConvertUnits.ToDisplayUnits(_wallPositions[4]), null, Color.White, 0f, ConvertUnits.ToDisplayUnits(_wallSizes[3]) / 2f + Vector2.UnitX * 3f, 1f, SpriteEffects.None, 0f);
		_spriteBatch.Draw(_wallTexture, ConvertUnits.ToDisplayUnits(_wallPositions[5]), null, Color.White, 0f, ConvertUnits.ToDisplayUnits(_wallSizes[3]) / 2f + Vector2.UnitX * 3f, 1f, SpriteEffects.None, 0f);
		if (!_demoMode && _isFinished)
		{
			_spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, 1280, 720), Color.White * _screenWipeAlpha);
		}
		_spriteBatch.End();
		base.Draw(gameTime);
	}

	public void RemoveDiamond(int id)
	{
		((Goal)_goalBodies[0].UserData).RemoveForever(id);
		((Goal)_goalBodies[1].UserData).RemoveForever(id);
	}
}
