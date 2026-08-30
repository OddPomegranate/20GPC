using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.HammerFight;

internal class Diamond : IDisposable
{
	public delegate void BrokenCallback(int id);

	public class DiamondGrab
	{
		public int GrabCount;

		public int ID;

		public DiamondGrab(int id)
		{
			ID = id;
		}
	}

	private int _numPlayers;

	private int _playerNum;

	private int _idRange;

	private bool _isDemo;

	private float _width;

	private float _height;

	private BreakableBody _breakableBody;

	private TimedBreakablePiece[] _breakablePieces;

	private BrokenCallback _brokenCallback;

	private bool _smashPlayed;

	private SoundManager _sounds;

	public bool IsBroken => _breakableBody.Broken;

	public int ID => _idRange;

	public Diamond(int playerNum, bool isDemo)
		: this(0, playerNum, isDemo, null, null)
	{
	}

	public Diamond(int numPlayers, int playerNum, bool isDemo, HammerFight hammerFight, SoundManager sounds)
	{
		_numPlayers = numPlayers;
		_playerNum = playerNum;
		_isDemo = isDemo;
		_idRange = BreakablePiece.CurrentId;
		if (hammerFight != null)
		{
			_brokenCallback = hammerFight.RemoveDiamond;
		}
		_sounds = sounds;
	}

	public void LoadContent(ContentManager content, GraphicsDevice gd, World world)
	{
		Vector2[] array = new Vector2[9]
		{
			new Vector2(81f, 4f),
			new Vector2(419f, 3f),
			new Vector2(490f, 80f),
			new Vector2(490f, 114f),
			new Vector2(246f, 330f),
			new Vector2(5f, 114f),
			new Vector2(5f, 80f),
			new Vector2(234f, 203f),
			new Vector2(268f, 88f)
		};
		for (int i = 0; i != array.Length; i++)
		{
			array[i].X /= 120f;
			array[i].Y /= 120f;
			_width = ((array[i].X > _width) ? array[i].X : _width);
			_height = ((array[i].Y > _height) ? array[i].Y : _height);
		}
		List<Vertices> list = new List<Vertices>();
		list.Add(new Vertices
		{
			array[5],
			array[6],
			array[0],
			array[7],
			array[4]
		});
		list.Add(new Vertices
		{
			array[0],
			array[8],
			array[7]
		});
		list.Add(new Vertices
		{
			array[0],
			array[1],
			array[2],
			array[8]
		});
		list.Add(new Vertices
		{
			array[8],
			array[2],
			array[7]
		});
		list.Add(new Vertices
		{
			array[7],
			array[2],
			array[3],
			array[4]
		});
		List<Vertices> list2 = list;
		_breakablePieces = new TimedBreakablePiece[list2.Count];
		// PORTING FIX (fullscreen sizing): this camera projection used to be built from
		// gd.Viewport, the real (fullscreen-variable) backbuffer size, instead of this
		// game's fixed 1280x720 virtual canvas (see FiftyGames.cs's MasterRenderTarget/
		// letterboxing) - wrong in fullscreen, where the viewport is the desktop's
		// native resolution.
		Matrix projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(1280), ConvertUnits.ToSimUnits(720), 0f, 0f, 1f);
		for (int j = 0; j != list2.Count; j++)
		{
			_breakablePieces[j] = new TimedBreakablePiece(list2[j], _width, _height, gd, projection, 3000);
			_breakablePieces[j].LoadContent(content.Load<Texture2D>("Hammerfight/Sprites/diamond"));
		}
		_breakableBody = new BreakableBody(list2, world, 2f);
		_breakableBody.MainBody.Position = new Vector2(ConvertUnits.ToSimUnits(380) + ConvertUnits.ToSimUnits(140) * (float)_playerNum, _isDemo ? ConvertUnits.ToSimUnits(-350) : ConvertUnits.ToSimUnits(640));
		_breakableBody.MainBody.CollisionCategories = Category.Cat3;
		_breakableBody.MainBody.FixedRotation = true;
		_breakableBody.MainBody.SleepingAllowed = false;
		_breakableBody.MainBody.Mass = 1.4f;
		_breakableBody.Strength = 45f;
		_breakableBody.MainBody.UserData = new DiamondGrab(_idRange);
		for (int k = 0; k != _breakableBody.Parts.Count; k++)
		{
			_breakableBody.Parts[k].UserData = _breakablePieces[k].ID;
			_breakablePieces[k].Fixture = _breakableBody.MainBody.FixtureList[0];
		}
		BreakablePiece.CurrentId += 100;
		BreakablePiece.CurrentId = BreakablePiece.CurrentId / 100 * 100;
		world.AddBreakableBody(_breakableBody);
	}

	public void Update(GameTime gameTime, List<Fixture> breakables)
	{
		for (int i = 0; i != _breakablePieces.Length; i++)
		{
			_breakablePieces[i].Update(gameTime, _breakableBody.Broken);
		}
		if (!_breakableBody.Broken)
		{
			return;
		}
		if (!_isDemo && !_smashPlayed)
		{
			_sounds.CreateGameSoundCue("hammer Smash Diamond").Play();
			_smashPlayed = true;
		}
		if (_brokenCallback != null)
		{
			_brokenCallback(_idRange);
			_brokenCallback = null;
			_breakableBody.MainBody.UserData = null;
		}
		for (int j = 0; j != breakables.Count; j++)
		{
			if (breakables[j].UserData != null && (int)breakables[j].UserData >= _idRange && (int)breakables[j].UserData < _idRange + 100)
			{
				_breakablePieces[(int)breakables[j].UserData % 100].Fixture = breakables[j];
				_breakablePieces[(int)breakables[j].UserData % 100].Fixture.Body.CollidesWith = Category.Cat1 | Category.Cat2;
				_breakablePieces[(int)breakables[j].UserData % 100].Fixture.Body.CollisionCategories = Category.Cat2;
				_breakablePieces[(int)breakables[j].UserData % 100].Fixture.Body.ApplyAngularImpulse(4f);
				breakables[j].UserData = null;
				breakables[j].Body.UserData = null;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		TimedBreakablePiece[] breakablePieces = _breakablePieces;
		foreach (BreakablePiece breakablePiece in breakablePieces)
		{
			breakablePiece.Draw(spriteBatch.GraphicsDevice, Color.GhostWhite);
		}
		spriteBatch.Begin();
	}

	public void Dispose()
	{
		for (int i = 0; i != _breakablePieces.Length; i++)
		{
			if (!_breakablePieces[i].IsDisposed)
			{
				_breakablePieces[i].Dispose();
			}
		}
	}

	public bool TestPoint(Vector2 position)
	{
		Vector2 vector = _breakableBody.MainBody.Position - _breakableBody.MainBody.LocalCenter;
		if (position.X > vector.X && position.X <= vector.X + _width && position.Y > vector.Y && position.Y <= vector.Y + _height)
		{
			return true;
		}
		return false;
	}
}
