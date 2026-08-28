using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.TheSkyIsFalling;

internal class TheSkyIsFalling : Minigame
{
	private SpriteBatch _spriteBatch;

	private MinigameMeta _minigame;

	private Texture2D _background;

	private SpriteFont _bigFont;

	private Texture2D _singlePixel;

	private Cloud[] _clouds;

	private Meteor[] _meteors;

	private List<Robot> _robots;

	private Random _random;

	private int _timePassed;

	private float _timeLived;

	private float _lfo;

	private float _alphaOverlay;

	public TheSkyIsFalling(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
		_minigame = minigame;
		_lfo = 1f;
		_alphaOverlay = 0f;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_random = new Random();
		_robots = new List<Robot>();
		for (int i = 0; i < _playerManager.NumberOfPlayers; i++)
		{
			_robots.Add(new Robot(_playerManager.PlayersConnected[i], new Vector2(640 - (_playerManager.NumberOfPlayers - 2) * 64 + i * 64, 600f), 1f, _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/legs"), _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/body"), _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/arms"), _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/head"), alive: true));
		}
		_clouds = new Cloud[20];
		for (int j = 0; j < _clouds.Length; j++)
		{
			_clouds[j] = new Cloud(new Vector2((float)_random.NextDouble() * 1280f, (float)_random.NextDouble() * 200f - 100f), new Vector2(0.1f + (float)_random.NextDouble() * 4f, 0f), new Vector2((float)_random.NextDouble() / 4f + 0.75f, (float)_random.NextDouble() / 4f + 0.75f), _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/cloud"), _random.Next(0, 2) == 1, ((float)_random.NextDouble() - 0.5f) * 10f);
		}
		_meteors = new Meteor[10];
		for (int k = 0; k < _meteors.Length; k++)
		{
			_meteors[k] = new Meteor(new Vector2(_random.Next(0, 1280), 0f - (float)_random.Next(100, 400)), new Vector2(((float)_random.NextDouble() - 0.5f) * 3f, 2f + (float)_random.NextDouble() * 5f), 0.25f + (float)_random.NextDouble() * 0.75f, _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/rock"), active: true, ref _random);
		}
		_bigFont = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_singlePixel = _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/pixel");
		_background = _contentManager.Load<Texture2D>("TheSkyIsFalling/Sprites/background");
	}

	public void Restart()
	{
		_alphaOverlay = 1f;
		_timePassed = 0;
		_timeLived = 0f;
		Meteor[] meteors = _meteors;
		foreach (Meteor meteor in meteors)
		{
			meteor.Dangerious = true;
			meteor.Position = new Vector2(_random.Next(0, 1280), 0f - (float)_random.Next(100, 400));
			meteor.Velocity = new Vector2(((float)_random.NextDouble() - 0.5f) * 3f, 2f + (float)_random.NextDouble() * 5f);
			meteor.Scale = 0.25f + (float)_random.NextDouble() * 0.75f;
		}
		for (int j = 0; j < _playerManager.NumberOfPlayers; j++)
		{
			_robots[j].Alive = true;
			_robots[j].Position = new Vector2(640 - (_playerManager.NumberOfPlayers - 2) * 64 + j * 64, 600f);
			_robots[j].Velocity = Vector2.Zero;
		}
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		_timePassed++;
		_timeLived += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (_lfo < 0f)
		{
			_lfo = 1f;
		}
		else
		{
			_lfo -= 0.05f;
		}
		if (_alphaOverlay > 0f)
		{
			_alphaOverlay -= 0.05f;
		}
		bool flag = true;
		foreach (Robot robot in _robots)
		{
			robot.Update(_meteors, ref _soundManager, ref _minigame, _timeLived);
			if (robot.Alive)
			{
				flag = false;
			}
		}
		if (flag)
		{
			foreach (Player item in _playerManager.PlayersConnected)
			{
				if (item.GamePadManager.GamePadStateCurrent.Buttons.A == ButtonState.Pressed || item.GamePadManager.GamePadStateCurrent.Buttons.X == ButtonState.Pressed || item.GamePadManager.GamePadStateCurrent.Buttons.Y == ButtonState.Pressed || item.GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Pressed || item.GamePadManager.GamePadStateCurrent.Buttons.Back == ButtonState.Pressed || item.GamePadManager.GamePadStateCurrent.Buttons.RightShoulder == ButtonState.Pressed || item.GamePadManager.GamePadStateCurrent.Buttons.LeftShoulder == ButtonState.Pressed)
				{
					Restart();
				}
			}
		}
		Cloud[] clouds = _clouds;
		foreach (Cloud cloud in clouds)
		{
			cloud.Update();
		}
		Meteor[] meteors = _meteors;
		foreach (Meteor meteor in meteors)
		{
			meteor.Update(_timePassed, ref _soundManager);
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.Clear(Color.CornflowerBlue);
		_spriteBatch.Begin();
		_spriteBatch.Draw(_background, Vector2.Zero, Color.White);
		foreach (Robot robot in _robots)
		{
			robot.Draw(_spriteBatch);
		}
		Cloud[] clouds = _clouds;
		foreach (Cloud cloud in clouds)
		{
			cloud.Draw(_spriteBatch);
		}
		Meteor[] meteors = _meteors;
		foreach (Meteor meteor in meteors)
		{
			meteor.Draw(_spriteBatch);
		}
		float num = 0f;
		bool flag = false;
		for (int k = 0; k < _robots.Count; k++)
		{
			if (_robots[k].Alive)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			string text = _timeLived.ToString("0.00", CultureInfo.InvariantCulture);
			Helper.DrawOutlinedText(_spriteBatch, _bigFont, text, new Vector2(640f, (float)((FiftyGames)base.Game).TitleSafeArea.Top + _bigFont.MeasureString(text).Y * 0.5f + num), Color.White, Color.Black, Helper.OutlineType.Both, centered: true, 1f);
			num += 64f;
		}
		List<RobotScore> list = new List<RobotScore>(_robots.Count);
		for (int l = 0; l < _robots.Count; l++)
		{
			list.Add(new RobotScore(_robots[l].Color, _robots[l].TimeLived, _robots[l].Alive));
		}
		list.Sort(CompareRobotsByTime);
		for (int m = 0; m < list.Count; m++)
		{
			float num2 = 1f;
			if (!flag && m == 0 && _lfo < 0.5f)
			{
				num2 = 0.4f;
			}
			if (!list[m]._alive)
			{
				string text2 = list[m]._score.ToString("0.00", CultureInfo.InvariantCulture);
				Color color = list[m]._color;
				color *= num2;
				Helper.DrawOutlinedText(_spriteBatch, _bigFont, text2, new Vector2(640f, (float)((FiftyGames)base.Game).TitleSafeArea.Top + _bigFont.MeasureString(text2).Y * 0.5f + num), color, Color.Black, Helper.OutlineType.Both, centered: true, 1f);
				num += 64f;
			}
		}
		_spriteBatch.Draw(_singlePixel, new Rectangle(0, 0, 1280, 720), Color.White * _alphaOverlay);
		_spriteBatch.End();
		base.Draw(gameTime);
	}

	private static int CompareRobotsByTime(RobotScore x, RobotScore y)
	{
		if (x._score > y._score)
		{
			return -1;
		}
		if (x._score < y._score)
		{
			return 1;
		}
		return 0;
	}
}
