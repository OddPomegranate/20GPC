using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.FruitsInARow;

internal class FruitsInARow : Minigame
{
	private const int GlowIntensity = 8;

	private const int HoldLimitTime = 500;

	private const int HoldRepeatTime = 100;

	private const int MaxColumns = 6;

	private const int MaxRows = 5;

	private const int ColumnWidth = 88;

	private const int ColumnSpacing = 14;

	private const int RowHeight = 88;

	private const int RowSpacing = 10;

	private const float outlineDetail = 16f;

	private SpriteBatch _spriteBatch;

	private Rectangle _screenRect;

	private Effect _postEffect;

	private RenderTarget2D _effectCanvas0;

	private RenderTarget2D _effectCanvas1;

	private SpriteFont _minigameMetaFont;

	private SpriteFont _scoreFont;

	private SpriteFont _jumboFont;

	private Texture2D _backgroundTex;

	private Texture2D _fieldTex;

	private Texture2D _indicatorTex;

	private Texture2D _retryTex;

	private Random _ranGen;

	private int _turn;

	private bool _winner;

	private GamePlayer[] _players;

	private int[,] _field;

	private Counter[,] _counters;

	private Counter[,] _connectedCounters;

	private int _eventTimer;

	private int _holdTimer;

	private int _selectedColumn;

	private Rectangle[] _columns;

	public FruitsInARow(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode)
		: base(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		_screenRect = new Rectangle(0, 0, 1280, 720);
		_effectCanvas0 = new RenderTarget2D(base.GraphicsDevice, _screenRect.Width, _screenRect.Height);
		_effectCanvas1 = new RenderTarget2D(base.GraphicsDevice, _screenRect.Width, _screenRect.Height);
		_players = new GamePlayer[2];
		_players[0] = new GamePlayer(_playerManager.PlayersConnected[0]);
		if (_playerManager.NumberOfPlayers > 1)
		{
			_players[1] = new GamePlayer(_playerManager.PlayersConnected[1]);
		}
		else
		{
			_players[1] = new GamePlayer(_playerManager.PlayersConnected[0]);
		}
		if (_players[0].PlayerFruit == _players[1].PlayerFruit)
		{
			if (_players[0].PlayerFruit == GamePlayer.Fruit.Lemon)
			{
				_players[1].PlayerFruit = GamePlayer.Fruit.Apple;
			}
			else
			{
				_players[1].PlayerFruit = _players[0].PlayerFruit + 1;
			}
		}
		_players[0].Load(_contentManager);
		_players[1].Load(_contentManager);
		_ranGen = new Random();
		SetupNewGame();
	}

	protected override void LoadContent()
	{
		_backgroundTex = _contentManager.Load<Texture2D>("FruitsInARow\\Image\\Background");
		_fieldTex = _contentManager.Load<Texture2D>("FruitsInARow\\Image\\Board");
		_indicatorTex = _contentManager.Load<Texture2D>("FruitsInARow\\Image\\Selected");
		_retryTex = _contentManager.Load<Texture2D>("FruitsInARow\\Image\\RetryButton");
		_postEffect = _contentManager.Load<Effect>("FruitsInARow\\Effect\\ScreenEffect");
		_minigameMetaFont = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_scoreFont = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_jumboFont = _contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		if (!_winner)
		{
			if (_players[_turn].PressedLeft)
			{
				SelectLeft();
			}
			else if (_players[_turn].PressedRight)
			{
				SelectRight();
			}
			if (_players[_turn].HoldLeft)
			{
				if (_holdTimer >= 500)
				{
					SelectLeft();
					_holdTimer -= 100;
				}
				_holdTimer += gameTime.ElapsedGameTime.Milliseconds;
			}
			else if (_players[_turn].HoldRight)
			{
				if (_holdTimer >= 500)
				{
					SelectRight();
					_holdTimer -= 100;
				}
				_holdTimer += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				_holdTimer = 0;
			}
			if (_players[_turn].PressedA)
			{
				bool flag = false;
				int num = 0;
				for (int i = 0; i <= 5; i++)
				{
					if (_field[_selectedColumn, i] == -1)
					{
						_field[_selectedColumn, i] = _turn;
						num = i;
						flag = true;
						i = 6;
					}
				}
				_soundManager.CreateGameSoundCue("connect4 Drop").Play();
				if (flag)
				{
					Rectangle column = new Rectangle(_columns[_selectedColumn].X, _columns[_selectedColumn].Y, _columns[_selectedColumn].Width, _columns[_selectedColumn].Height);
					_columns[_selectedColumn].Height -= 98;
					Counter counter = new Counter(_players[_turn], new Vector2((float)column.X + (float)column.Width / 2f, (float)column.Y + (float)column.Width / 2f), column);
					_counters[_selectedColumn, num] = counter;
					int num2 = -1;
					int num3 = 1;
					int num4 = 0;
					int[] array = new int[4];
					_connectedCounters[0, 0] = counter;
					_connectedCounters[1, 0] = counter;
					_connectedCounters[2, 0] = counter;
					_connectedCounters[3, 0] = counter;
					while (!_winner && num3 != 2)
					{
						int num5 = 0;
						if (num2 == num3 * -1)
						{
							num5 = 0;
						}
						else if (num3 == 0)
						{
							num5 = 1;
						}
						else if (num2 == num3)
						{
							num5 = 2;
						}
						else if (num2 == 0)
						{
							num5 = 3;
						}
						if (_selectedColumn + num2 * (num4 + 1) >= 0 && _selectedColumn + num2 * (num4 + 1) <= 6 && num + num3 * (num4 + 1) >= 0 && num + num3 * (num4 + 1) <= 5 && array[num5] + num4 < 3)
						{
							if (_field[_selectedColumn + num2 * (num4 + 1), num + num3 * (num4 + 1)] == _turn)
							{
								_connectedCounters[num5, array[num5] + num4 + 1] = _counters[_selectedColumn + num2 * (num4 + 1), num + num3 * (num4 + 1)];
								num4++;
							}
							else
							{
								array[num5] += num4;
								num4 = 0;
							}
						}
						else
						{
							array[num5] += num4;
							num4 = 0;
						}
						if (num4 == 0)
						{
							if (num2 == 1)
							{
								num3++;
							}
							if (num3 == -1)
							{
								num2++;
							}
							if (num2 == -1)
							{
								num3--;
							}
						}
						if (array[0] == 3 || array[1] == 3 || array[2] == 3 || array[3] == 3)
						{
							for (int j = 0; j != 4; j++)
							{
								_connectedCounters[0, j] = _connectedCounters[num5, j];
							}
							_players[_turn].Wins++;
							_winner = true;
						}
					}
					if (!_winner)
					{
						_turn += 1 - _turn * 2;
						_holdTimer = 0;
					}
					for (int k = 0; k != 4; k++)
					{
						array[k] = 0;
					}
				}
			}
		}
		else
		{
			for (int l = 0; l != 1; l++)
			{
				if (_players[l].PressedA)
				{
					SetupNewGame();
					_eventTimer = 0;
				}
			}
			if (_eventTimer > 100000)
			{
				_eventTimer = 1000 + (_eventTimer - 100000);
			}
			_eventTimer += gameTime.ElapsedGameTime.Milliseconds;
		}
		for (int m = 0; m != 7; m++)
		{
			for (int n = 0; n != 6; n++)
			{
				if (_counters[m, n] != null)
				{
					_counters[m, n].Update(gameTime);
				}
			}
		}
		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime)
	{
		base.GraphicsDevice.SetRenderTarget(_effectCanvas0);
		base.GraphicsDevice.Clear(Color.Transparent);
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _postEffect);
		_postEffect.CurrentTechnique = _postEffect.Techniques["Blur"];
		_postEffect.Parameters["brightness"].SetValue(0.6f * (float)Math.Sin((double)gameTime.TotalGameTime.Milliseconds * 0.01));
		if (_winner)
		{
			for (int i = 0; i != 4; i++)
			{
				_connectedCounters[0, i].Draw(_spriteBatch);
			}
		}
		_spriteBatch.End();
		for (int j = 0; j < 8; j++)
		{
			base.GraphicsDevice.SetRenderTarget(_effectCanvas1);
			base.GraphicsDevice.Clear(Color.Black);
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, _postEffect);
			_spriteBatch.Draw(_effectCanvas0, _screenRect, Color.White);
			_spriteBatch.End();
			base.GraphicsDevice.SetRenderTarget(_effectCanvas0);
			base.GraphicsDevice.Clear(Color.Black);
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, _postEffect);
			_spriteBatch.Draw(_effectCanvas1, _screenRect, Color.White);
			_spriteBatch.End();
		}
		base.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		base.GraphicsDevice.Clear(Color.Black);
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		_spriteBatch.Draw(_backgroundTex, Vector2.Zero, Color.White);
		_spriteBatch.Draw(_players[0].LargeSprite, new Vector2(100f, -10f), Color.White * 0.5f);
		_spriteBatch.Draw(_players[1].LargeSprite, new Vector2(1180f - (float)_players[1].LargeSprite.Width, -10f), Color.White * 0.5f);
		for (int k = 0; k != 7; k++)
		{
			for (int l = 0; l != 6; l++)
			{
				if (_counters[k, l] != null)
				{
					_counters[k, l].Draw(_spriteBatch);
				}
			}
		}
		_spriteBatch.Draw(_fieldTex, new Vector2(288f, 57f), Color.White);
		_spriteBatch.End();
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
		_spriteBatch.Draw(_effectCanvas1, _screenRect, Color.White);
		_spriteBatch.End();
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		_spriteBatch.Draw(_indicatorTex, new Vector2(_columns[_selectedColumn].X, 50f), _players[_turn].Colour);
		Vector2 position = default(Vector2);
		Vector2 origin = default(Vector2);
		position.X = 200f;
		position.Y = 100f;
		origin.X = _minigameMetaFont.MeasureString(_players[0].Name).X / 2f;
		origin.Y = 0f;
		DrawOutlinedString(_spriteBatch, _minigameMetaFont, _players[0].Name, position, 0f, 1f, origin, 4f, _players[0].Colour, Color.Black);
		position.X = 1080f;
		origin.X = _minigameMetaFont.MeasureString(_players[1].Name).X / 2f;
		DrawOutlinedString(_spriteBatch, _minigameMetaFont, _players[1].Name, position, 0f, 1f, origin, 4f, _players[1].Colour, Color.Black);
		position.X = 200f;
		position.Y = 400f;
		origin.X = _minigameMetaFont.MeasureString("WINS").X / 2f;
		DrawOutlinedString(_spriteBatch, _minigameMetaFont, "WINS", position, 0f, 1f, origin, 4f, Color.White, Color.Black);
		position.X = 1080f;
		DrawOutlinedString(_spriteBatch, _minigameMetaFont, "WINS", position, 0f, 1f, origin, 4f, Color.White, Color.Black);
		position.X = 200f;
		position.Y = 450f;
		origin.X = _minigameMetaFont.MeasureString(_players[0].Wins.ToString()).X / 2f;
		DrawOutlinedString(_spriteBatch, _minigameMetaFont, _players[0].Wins.ToString(), position, 0f, 1f, origin, 4f, Color.White, Color.Black);
		position.X = 1080f;
		origin.X = _minigameMetaFont.MeasureString(_players[1].Wins.ToString()).X / 2f;
		DrawOutlinedString(_spriteBatch, _minigameMetaFont, _players[1].Wins.ToString(), position, 0f, 1f, origin, 4f, Color.White, Color.Black);
		if (!_winner)
		{
			position.Y = 250f;
			if (_turn == 0)
			{
				position.X = 200f;
			}
			else
			{
				position.X = 1080f;
			}
			origin.X = _minigameMetaFont.MeasureString("Your turn!").X / 2f;
			DrawOutlinedString(_spriteBatch, _minigameMetaFont, "Your Turn!", position, 0f, 1f, origin, 4f, Color.White, Color.Black);
		}
		else
		{
			float scale;
			float rotation;
			if (_eventTimer < 300)
			{
				scale = (float)Math.Sin((double)_eventTimer / 150.0 * 1.12) * 1.5f;
				rotation = (float)Math.Sin((double)_eventTimer / 150.0) * 1.571f - 1.746f;
			}
			else
			{
				scale = 1f + (float)Math.Sin((double)_eventTimer / 400.0) * 0.1f;
				rotation = 0f;
			}
			_spriteBatch.Draw(_players[_turn].LargeSprite, new Vector2(640f, 360f), null, Color.White, rotation, new Vector2((float)_players[_turn].LargeSprite.Width / 2f, (float)_players[_turn].LargeSprite.Height / 2f), scale, SpriteEffects.None, 0f);
			if (_eventTimer > 100)
			{
				if (_eventTimer < 400)
				{
					scale = (float)Math.Sin((double)(_eventTimer - 100) / 150.0 * 1.12) * 1.5f;
					rotation = (float)Math.Sin((double)(_eventTimer - 100) / 150.0) * 1.571f - 1.746f;
				}
				else
				{
					scale = 1f + (float)Math.Sin((double)_eventTimer / 400.0) * 0.1f;
					rotation = -0.175f;
				}
				position.X = 640f;
				position.Y = 310f;
				origin.X = _jumboFont.MeasureString(_players[_turn].Name).X / 2f;
				origin.Y = _jumboFont.MeasureString(_players[_turn].Name).Y / 2f;
				DrawOutlinedString(_spriteBatch, _jumboFont, _players[_turn].Name, position, rotation, scale, origin, 8f, Color.White, Color.Black);
			}
			if (_eventTimer > 200)
			{
				if (_eventTimer < 500)
				{
					scale = (float)Math.Sin((double)(_eventTimer - 200) / 150.0 * 1.12) * 1.5f;
					rotation = (float)Math.Sin((double)(_eventTimer - 200) / 150.0) * 1.571f - 1.746f;
				}
				else
				{
					scale = 1f + (float)Math.Sin((double)_eventTimer / 400.0) * 0.1f;
					rotation = -0.175f;
				}
				position.Y = 410f;
				origin.X = _jumboFont.MeasureString("WINS!").X / 2f;
				origin.Y = _jumboFont.MeasureString("WINS!").Y / 2f;
				DrawOutlinedString(_spriteBatch, _jumboFont, "WINS!", position, rotation, scale, origin, 8f, Color.White, Color.Black);
				origin.X = _scoreFont.MeasureString("PLAY AGAIN").X / 2f;
				origin.Y = _scoreFont.MeasureString("PLAY AGAIN").Y / 2f;
				_spriteBatch.Draw(_retryTex, new Vector2(440f, 550f), null, Color.White, rotation + 0.175f, new Vector2((float)_retryTex.Width / 2f, (float)_retryTex.Height / 2f), scale, SpriteEffects.None, 0f);
				DrawOutlinedString(_spriteBatch, _scoreFont, "PLAY AGAIN", new Vector2(690f, 550f), rotation + 0.175f, scale + 0.2f, origin, 5f, Color.White, Color.Black);
			}
		}
		_spriteBatch.End();
	}

	private void SetupNewGame()
	{
		_field = new int[7, 6];
		for (int i = 0; i != 7; i++)
		{
			for (int j = 0; j != 6; j++)
			{
				_field[i, j] = -1;
			}
		}
		_columns = new Rectangle[7];
		ref Rectangle reference = ref _columns[0];
		reference = new Rectangle(299, -88, 88, 737);
		ref Rectangle reference2 = ref _columns[1];
		reference2 = new Rectangle(397, -88, 88, 737);
		ref Rectangle reference3 = ref _columns[2];
		reference3 = new Rectangle(495, -88, 88, 737);
		ref Rectangle reference4 = ref _columns[3];
		reference4 = new Rectangle(593, -88, 88, 737);
		ref Rectangle reference5 = ref _columns[4];
		reference5 = new Rectangle(691, -88, 88, 737);
		ref Rectangle reference6 = ref _columns[5];
		reference6 = new Rectangle(789, -88, 88, 737);
		ref Rectangle reference7 = ref _columns[6];
		reference7 = new Rectangle(887, -88, 88, 737);
		_counters = new Counter[7, 6];
		_connectedCounters = new Counter[4, 4];
		_selectedColumn = 3;
		_turn = _ranGen.Next(2);
		_winner = false;
	}

	private void SelectLeft()
	{
		if (_selectedColumn == 0)
		{
			_selectedColumn = 6;
		}
		else
		{
			_selectedColumn--;
		}
		_soundManager.CreateGameSoundCue("connect4 Move").Play();
	}

	private void SelectRight()
	{
		if (_selectedColumn == 6)
		{
			_selectedColumn = 0;
		}
		else
		{
			_selectedColumn++;
		}
		_soundManager.CreateGameSoundCue("connect4 Move").Play();
	}

	private void DrawOutlinedString(SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, float rotation, float scale, Vector2 origin, float thickness, Color colour, Color outlineColour)
	{
		for (int i = 0; (float)i < 16f; i++)
		{
			float num = (float)Math.PI / 8f * (float)i;
			Vector2 vector = new Vector2((float)Math.Sin(num) * thickness, (float)Math.Cos(num) * thickness);
			spriteBatch.DrawString(spriteFont, text, position + vector, outlineColour, rotation, origin, scale, SpriteEffects.None, 0f);
		}
		spriteBatch.DrawString(spriteFont, text, position, colour, rotation, origin, scale, SpriteEffects.None, 0f);
	}
}
