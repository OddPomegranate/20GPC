using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.LunarLander;

internal class LunarLander(Game game, ref PlayerManager playerManager, ref SoundManager soundManager, ref ContentManager contentManager, ref MinigameMeta minigame, bool demoMode) : Minigame(game, ref playerManager, ref soundManager, ref contentManager, ref minigame, demoMode)
{
	private enum GameState
	{
		Start,
		Progress,
		End
	}

	private const int GlowIntensity = 4;

	private const float DamageCoefficient = 7f;

	private const float RotationDamageCoefficient = 1f;

	private const float LandingPadDamageCoefficient = 5f;

	private const float LandingPadRotationDamageCoefficient = 1f;

	private const float LandingPadRestitutionCoefficient = 0.8f;

	private const float LandingPadFriction = 2f;

	private const float LandingVelocityThreshold = 0.5f;

	private GameState _minigameMetaState;

	private LineRender _lineRender = new LineRender();

	private GameInterface _minigameMetaInterface = new GameInterface();

	private SpriteBatch spriteBatch;

	private RenderTarget2D vectorCanvas;

	private RenderTarget2D interfaceCanvas;

	private RenderTarget2D effectCanvas0;

	private RenderTarget2D effectCanvas1;

	private Effect postEffect;

	private fullScreenQuad quad;

	// TEMPORARY diagnostic (round 2, black-screen investigation): dumps 3
	// intermediate render targets to RenderDiag/LunarLander/ once, ~1.5s into
	// the first Draw() call, to see which stage of the pipeline is actually
	// black. Remove once findings are extracted.
	private int _renderDiagFrameCounter;
	private bool _renderDiagDumped;

	private void DumpRenderDiagIfDue()
	{
		if (_renderDiagDumped)
		{
			return;
		}
		_renderDiagFrameCounter++;
		if (_renderDiagFrameCounter < 360)
		{
			return;
		}
		_renderDiagDumped = true;
		try
		{
			string dir = System.IO.Path.Combine(AppContext.BaseDirectory, "RenderDiag", "LunarLander");
			System.IO.Directory.CreateDirectory(dir);
			void Save(RenderTarget2D rt, string name)
			{
				using System.IO.FileStream stream = System.IO.File.Create(System.IO.Path.Combine(dir, name + ".png"));
				rt.SaveAsPng(stream, rt.Width, rt.Height);
			}
			Save(vectorCanvas, "1_vectorCanvas");
			Save(interfaceCanvas, "2_interfaceCanvas");
			Save(effectCanvas0, "3_effectCanvas0_final");
		}
		catch
		{
		}
	}

	private bool _gameOver;

	private int _currentLevel;

	private bool _levelLoaded;

	private int _eventTimer;

	private int[] _deathTimer;

	private int[] _scores;

	private int _topScore;

	private List<Pod> _pods = new List<Pod>();

	private List<Vector2> _spawns = new List<Vector2>();

	private List<LandingPad> _pads = new List<LandingPad>();

	private List<Wall> _walls = new List<Wall>();

	public override void Initialize()
	{
		base.Initialize();
		spriteBatch = new SpriteBatch(_framework.GraphicsDevice);
		_lineRender.GraphicsDevice = _framework.GraphicsDevice;
		_lineRender.BackBufferSize = new Rectangle(0, 0, 640, 720);
		vectorCanvas = new RenderTarget2D(spriteBatch.GraphicsDevice, _lineRender.BackBufferSize.Width, _lineRender.BackBufferSize.Height);
		interfaceCanvas = new RenderTarget2D(spriteBatch.GraphicsDevice, _lineRender.BackBufferSize.Width, _lineRender.BackBufferSize.Height);
		effectCanvas0 = new RenderTarget2D(spriteBatch.GraphicsDevice, _lineRender.BackBufferSize.Width, _lineRender.BackBufferSize.Height);
		effectCanvas1 = new RenderTarget2D(spriteBatch.GraphicsDevice, _lineRender.BackBufferSize.Width, _lineRender.BackBufferSize.Height);
		int[] deathTimer = new int[4];
		_deathTimer = deathTimer;
		_levelLoaded = false;
		_currentLevel = 0;
		_eventTimer = 0;
		int[] scores = new int[4];
		_scores = scores;
		_topScore = 0;
		string[] cueNames = new string[6] { "lunarLander Bounce", "lunarLander Crash", "lunarLander Flag", "lunarLander Land", "lunarLander Thrust", "lunarLander ThrusterSide" };
		_soundManager.PreloadSounds(cueNames);
		_gameOver = false;
		_minigameMetaState = GameState.Start;
	}

	protected override void LoadContent()
	{
		_lineRender.Load(_contentManager);
		postEffect = base.Game.Content.Load<Effect>("LunarLander\\Effect\\ScreenEffect");
		quad = new fullScreenQuad(base.GraphicsDevice);
	}

	protected override void UnloadContent()
	{
	}

	public override void Update(GameTime gameTime)
	{
		switch (_minigameMetaState)
		{
		case GameState.Start:
			LevelStart(gameTime);
			break;
		case GameState.Progress:
			LevelProgress(gameTime);
			break;
		case GameState.End:
			LevelEnd(gameTime);
			break;
		}
		base.Update(gameTime);
	}

	private void LoadLevel(int level)
	{
		string text = "Content/LunarLander/Level/" + _currentLevel + ".txt";
		if (!File.Exists(text))
		{
			return;
		}
		StreamReader streamReader = new StreamReader(text);
		_ = string.Empty;
		int num = 0;
		try
		{
			while (!streamReader.EndOfStream)
			{
				string text2 = streamReader.ReadLine();
				if (text2 != "" && text2[0] != '#')
				{
					switch (text2)
					{
					case "[PODS]":
						num = 1;
						break;
					case "[PADS]":
						num = 2;
						break;
					case "[WALLS]":
						num = 3;
						break;
					}
					if (text2[0] >= '0' && text2[0] <= '9')
					{
						switch (num)
						{
						case 1:
						{
							int num2 = text2[0] - 48;
							if (_pods.Count > num2)
							{
								string[] array3 = text2.Substring(2).Split(',');
								_spawns[num2] = new Vector2(float.Parse(array3[0]), float.Parse(array3[1]));
								_pods[num2].Spawn(_spawns[num2]);
							}
							break;
						}
						case 2:
						{
							string[] array2 = text2.Split(',');
							Vector2 position = new Vector2(float.Parse(array2[0]), float.Parse(array2[1]));
							int difficultyClass = int.Parse(array2[2]);
							_pads.Add(new LandingPad(position, (LandingPad.LandingPadClass)difficultyClass));
							break;
						}
						case 3:
						{
							string[] array = text2.Split(',');
							Vector2 start = new Vector2(float.Parse(array[0]), float.Parse(array[1]));
							Vector2 end = new Vector2(float.Parse(array[2]), float.Parse(array[3]));
							_walls.Add(new Wall(start, end));
							break;
						}
						}
					}
				}
				_levelLoaded = true;
			}
		}
		catch (Exception)
		{
			Exception ex2 = new Exception("LunarLander.LoadLevel: Failed to load level " + text);
			throw ex2;
		}
		finally
		{
			streamReader.Close();
		}
	}

	private void UnloadLevel()
	{
		_pads.Clear();
		_walls.Clear();
		_levelLoaded = false;
	}

	private void LevelStart(GameTime gameTime)
	{
		if (_levelLoaded)
		{
			if (_eventTimer <= 0)
			{
				_minigameMetaState = GameState.Progress;
			}
			_eventTimer -= gameTime.ElapsedGameTime.Milliseconds;
			return;
		}
		if (_pods.Count != 1)
		{
			_pods = new List<Pod>();
			foreach (Player item in _playerManager.PlayersConnected)
			{
				Pod pod = new Pod(item);
				pod.FuelThrusterSound = _soundManager.CreateGameSoundCue("lunarLander Thrust");
				pod.AirThrusterSound = _soundManager.CreateGameSoundCue("lunarLander ThrusterSide");
				_pods.Add(pod);
				_minigameMetaInterface.AddPlayer(pod);
				_deathTimer[_pods.IndexOf(pod)] = 0;
				_spawns.Add(Vector2.Zero);
			}
		}
		LoadLevel(_currentLevel);
		_eventTimer = 3000;
	}

	private void LevelProgress(GameTime gameTime)
	{
		Vector2[] array = new Vector2[_pods.Count];
		float[] array2 = new float[_pods.Count];
		foreach (Pod pod in _pods)
		{
			if (pod.Health <= 0)
			{
				continue;
			}
			ref Vector2 reference = ref array[_pods.IndexOf(pod)];
			reference = pod.Velocity;
			array2[_pods.IndexOf(pod)] = pod.RotationalVelocity;
			foreach (Pod pod2 in _pods)
			{
				if (pod != pod2 && pod.CollisionVolume.Intersects(pod2.CollisionVolume))
				{
					Vector2 value = new Vector2(pod2.Position.X - pod.Position.X, pod2.Position.Y - pod.Position.Y);
					float num = (float)Math.Atan2(value.Y, value.X);
					float num2 = (pod2.Velocity - pod.Velocity).Length();
					array[_pods.IndexOf(pod)].X -= (pod.Velocity.X + (float)Math.Cos(num) * num2) * pod.ShipRestitution;
					array[_pods.IndexOf(pod)].Y -= (pod.Velocity.Y + (float)Math.Sin(num) * num2) * pod.ShipRestitution;
					array2[_pods.IndexOf(pod)] = Vector2.Dot(pod.Velocity, value);
					pod.Player.GamePadManager.StartVibration(200, pod.Velocity.Length());
				}
			}
			foreach (LandingPad pad in _pads)
			{
				if (!pod.CollisionVolume.Intersects(pad.CollisionVolume))
				{
					continue;
				}
				if (!pad.IsUsed && pod.Position.Y < pad.CollisionVolume.Min.Y && pod.Position.X > pad.CollisionVolume.Min.X && pod.Position.X < pad.CollisionVolume.Max.X && pod.Velocity.Length() < 0.5f && Math.Abs(pod.Rotation) < 0.175f)
				{
					pod.HasLanded = true;
					if (pod.Health == 0)
					{
						pod.Damage(-1);
					}
					bool first = false;
					int num3 = 0;
					foreach (Pod pod3 in _pods)
					{
						if (pod3.HasLanded)
						{
							num3++;
						}
					}
					if (num3 == 1)
					{
						first = true;
						_soundManager.CreateGameSoundCue("lunarLander Flag").Play();
					}
					else
					{
						_soundManager.CreateGameSoundCue("lunarLander Land").Play();
					}
					pad.Use(pod, first);
					pod.First = first;
				}
				else if (!pod.HasLanded)
				{
					float num4 = MathHelper.Max(Math.Abs(pod.Rotation) * 1f, 1f);
					int num5 = (int)(pod.Velocity.Length() * 5f * num4);
					pod.Damage(num5);
					if (num5 != 0)
					{
						_soundManager.CreateGameSoundCue("lunarLander Bounce").Play();
					}
					if (pod.Health != 0)
					{
						array[_pods.IndexOf(pod)].Y = (0f - Math.Abs(pod.Velocity.Y)) * pod.ShipRestitution * 0.8f;
						array[_pods.IndexOf(pod)].X /= 2f;
						pod.Position = new Vector2(pod.Position.X, pad.CollisionVolume.Min.Y - pod.CollisionVolume.Radius);
					}
					else
					{
						_soundManager.CreateGameSoundCue("lunarLander Crash").Play();
					}
				}
			}
			foreach (Wall wall in _walls)
			{
				if (!pod.CollisionVolume.Intersects(wall.CollisionVolume))
				{
					continue;
				}
				float num6 = (wall.End.Y - wall.Start.Y) / (wall.End.X - wall.Start.X);
				float num7 = (pod.Position.Y - wall.Start.Y) / num6;
				float num8 = pod.Position.X - wall.Start.X;
				if ((wall.Start.Y < wall.End.Y && num8 < num7 + pod.CollisionVolume.Radius) || (wall.Start.Y > wall.End.Y && num8 > num7 - pod.CollisionVolume.Radius) || (wall.Start.Y == wall.End.Y && wall.CollisionNormal.Y < 0f && pod.Position.Y < wall.Start.Y) || (wall.Start.Y == wall.End.Y && wall.CollisionNormal.Y > 0f && pod.Position.Y > wall.Start.Y) || (wall.Start.X == wall.End.X && wall.CollisionNormal.X < 0f && pod.Position.X < wall.Start.X) || (wall.Start.X == wall.End.X && wall.CollisionNormal.X > 0f && pod.Position.X > wall.Start.X))
				{
					float value2 = pod.Rotation - (float)Math.Atan2(wall.CollisionNormal.Y, wall.CollisionNormal.X);
					float num9 = MathHelper.Max(Math.Abs(value2) * 1f, 1f);
					pod.Damage((int)(pod.Velocity.Length() * 7f * num9));
					if (pod.Health != 0)
					{
						ref Vector2 reference2 = ref array[_pods.IndexOf(pod)];
						reference2 = Vector2.Reflect(pod.Velocity, wall.CollisionNormal) * pod.ShipRestitution;
						pod.Position += wall.CollisionNormal;
						_soundManager.CreateGameSoundCue("lunarLander Bounce").Play();
					}
					else
					{
						_soundManager.CreateGameSoundCue("lunarLander Crash").Play();
					}
				}
			}
		}
		int num10 = 0;
		foreach (Pod pod4 in _pods)
		{
			if (pod4.Lives != 0 || !pod4.HasLanded)
			{
				if (pod4.Health != 0)
				{
					pod4.Velocity = array[_pods.IndexOf(pod4)];
					pod4.RotationalVelocity = array2[_pods.IndexOf(pod4)];
				}
				else
				{
					if (_deathTimer[_pods.IndexOf(pod4)] >= 2000 && pod4.Lives > 0)
					{
						pod4.Spawn(_spawns[_pods.IndexOf(pod4)]);
						_deathTimer[_pods.IndexOf(pod4)] = 0;
					}
					_deathTimer[_pods.IndexOf(pod4)] += gameTime.ElapsedGameTime.Milliseconds;
				}
				pod4.Update(gameTime, 0.1f);
			}
			if ((pod4.Lives == 0 && pod4.Health == 0) || pod4.HasLanded)
			{
				num10++;
			}
		}
		if (num10 != _playerManager.NumberOfPlayers)
		{
			return;
		}
		foreach (Pod pod5 in _pods)
		{
			_scores[_pods.IndexOf(pod5)] += pod5.Score;
			if (pod5.Score > _topScore)
			{
				_topScore = pod5.Score;
				foreach (Pod pod6 in _pods)
				{
					pod6.Leader = false;
				}
				pod5.Leader = true;
			}
			else if (pod5.Score == _topScore)
			{
				pod5.Leader = true;
			}
			if (pod5.FuelThrusterSound.IsStopping && pod5.FuelThrusterSound.IsStopped && pod5.FuelThrusterSound.IsDisposed)
			{
				pod5.FuelThrusterSound.Pause();
			}
			if (pod5.AirThrusterSound.IsStopping && pod5.AirThrusterSound.IsStopped && pod5.AirThrusterSound.IsDisposed)
			{
				pod5.AirThrusterSound.Pause();
			}
		}
		_minigameMetaState = GameState.End;
		_eventTimer = 0;
	}

	private void LevelEnd(GameTime gameTime)
	{
		if (_eventTimer > 5000)
		{
			if (!_gameOver)
			{
				int num = 0;
				foreach (Pod pod in _pods)
				{
					if (pod.Lives == 0 && !pod.HasLanded)
					{
						num++;
					}
				}
				if (num == _playerManager.NumberOfPlayers || _currentLevel == 6)
				{
					_topScore = 0;
					foreach (Pod pod2 in _pods)
					{
						pod2.Leader = false;
					}
					foreach (Pod pod3 in _pods)
					{
						if (_scores[_pods.IndexOf(pod3)] > _topScore)
						{
							_topScore = _scores[_pods.IndexOf(pod3)];
							foreach (Pod pod4 in _pods)
							{
								pod4.Leader = false;
							}
							pod3.Leader = true;
						}
						else if (_scores[_pods.IndexOf(pod3)] == _topScore)
						{
							pod3.Leader = true;
						}
					}
					string text = "";
					if ((float)_topScore > _minigameMeta.BestScore)
					{
						foreach (Pod pod5 in _pods)
						{
							if (pod5.Leader)
							{
								text = ((!(text == "")) ? (text + ", " + pod5.Player.Name) : pod5.Player.Name);
							}
						}
						_minigameMeta.SetScore(text, _topScore);
					}
					_gameOver = true;
					_eventTimer = 0;
				}
			}
			if (!_gameOver)
			{
				_currentLevel++;
				_minigameMetaState = GameState.Start;
				_topScore = 0;
				_eventTimer = 0;
				if (_pods.Count > 1)
				{
					_pods.Clear();
					_minigameMetaInterface.RemoveAllPlayers();
				}
				else
				{
					_pods[0].ResetScore();
					_pods[0].AwardLives(1);
				}
				UnloadLevel();
			}
			else if (_eventTimer > 100000)
			{
				_eventTimer = 1000 + (_eventTimer - 100000);
			}
		}
		if (_gameOver)
		{
			_eventTimer = 100000;
			foreach (Player item in _playerManager.PlayersConnected)
			{
				if (item.GamePadManager.ButtonWasPressed(Buttons.A))
				{
					_pods.Clear();
					_minigameMetaInterface.RemoveAllPlayers();
					UnloadLevel();
					Initialize();
					break;
				}
			}
		}
		_eventTimer += gameTime.ElapsedGameTime.Milliseconds;
	}

	public override void Draw(GameTime gameTime)
	{
		spriteBatch.GraphicsDevice.SetRenderTarget(vectorCanvas);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		foreach (Wall wall in _walls)
		{
			wall.Draw(_lineRender);
		}
		foreach (LandingPad pad in _pads)
		{
			if (pad.User != null && pad.User.First)
			{
				Vector2 position = pad.Position;
				position.X -= 7f;
				position.Y -= 14f;
				_minigameMetaInterface.DrawFlag(_lineRender, position, 0.7f);
			}
			pad.Draw(_lineRender);
		}
		foreach (Pod pod in _pods)
		{
			pod.Draw(_lineRender, gameTime);
		}
		_minigameMetaInterface.DrawHUD(_lineRender, new Vector2((float)(_titleSafeArea.Center.X / 2) - 25f * (float)_pods.Count, (float)(_titleSafeArea.Top / 2) + 5f));
		spriteBatch.GraphicsDevice.SetRenderTarget(interfaceCanvas);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		VertexPositionColor[] vertices = new VertexPositionColor[18]
		{
			new VertexPositionColor(new Vector3(245f, 55f, 0f), Color.White),
			new VertexPositionColor(new Vector3(345f, 55f, 0f), Color.White),
			new VertexPositionColor(new Vector3(395f, 55f, 0f), Color.White),
			new VertexPositionColor(new Vector3(245f, 105f, 0f), Color.White),
			new VertexPositionColor(new Vector3(295f, 105f, 0f), Color.White),
			new VertexPositionColor(new Vector3(345f, 105f, 0f), Color.White),
			new VertexPositionColor(new Vector3(245f, 155f, 0f), Color.White),
			new VertexPositionColor(new Vector3(345f, 155f, 0f), Color.White),
			new VertexPositionColor(new Vector3(245f, 205f, 0f), Color.White),
			new VertexPositionColor(new Vector3(295f, 205f, 0f), Color.White),
			new VertexPositionColor(new Vector3(345f, 205f, 0f), Color.White),
			new VertexPositionColor(new Vector3(395f, 205f, 0f), Color.White),
			new VertexPositionColor(new Vector3(245f, 255f, 0f), Color.White),
			new VertexPositionColor(new Vector3(295f, 255f, 0f), Color.White),
			new VertexPositionColor(new Vector3(345f, 255f, 0f), Color.White),
			new VertexPositionColor(new Vector3(395f, 255f, 0f), Color.White),
			new VertexPositionColor(new Vector3(245f, 305f, 0f), Color.White),
			new VertexPositionColor(new Vector3(395f, 305f, 0f), Color.White)
		};
		short[] array = new short[24];
		switch (_minigameMetaState)
		{
		case GameState.Start:
			if (_eventTimer < 1000)
			{
				array[0] = 0;
				array[1] = 1;
				array[2] = 1;
				array[3] = 14;
				array[4] = 14;
				array[5] = 15;
				array[6] = 15;
				array[7] = 17;
				array[8] = 17;
				array[9] = 16;
				array[10] = 16;
				array[11] = 12;
				array[12] = 12;
				array[13] = 13;
				array[14] = 13;
				array[15] = 4;
				array[16] = 4;
				array[17] = 3;
				array[18] = 3;
				array[19] = 0;
				array[20] = 0;
				array[21] = 0;
				array[22] = 0;
				array[23] = 0;
			}
			else if (_eventTimer < 2000)
			{
				array[0] = 0;
				array[1] = 2;
				array[2] = 2;
				array[3] = 11;
				array[4] = 11;
				array[5] = 9;
				array[6] = 9;
				array[7] = 13;
				array[8] = 13;
				array[9] = 15;
				array[10] = 15;
				array[11] = 17;
				array[12] = 17;
				array[13] = 16;
				array[14] = 16;
				array[15] = 6;
				array[16] = 6;
				array[17] = 7;
				array[18] = 7;
				array[19] = 5;
				array[20] = 5;
				array[21] = 3;
				array[22] = 3;
				array[23] = 0;
			}
			else if (_eventTimer < 3000)
			{
				array[0] = 0;
				array[1] = 2;
				array[2] = 2;
				array[3] = 17;
				array[4] = 17;
				array[5] = 16;
				array[6] = 16;
				array[7] = 12;
				array[8] = 12;
				array[9] = 14;
				array[10] = 14;
				array[11] = 10;
				array[12] = 10;
				array[13] = 8;
				array[14] = 8;
				array[15] = 6;
				array[16] = 6;
				array[17] = 7;
				array[18] = 7;
				array[19] = 5;
				array[20] = 5;
				array[21] = 3;
				array[22] = 3;
				array[23] = 0;
			}
			_lineRender.DrawIndexedShape(vertices, array);
			break;
		case GameState.End:
		{
			bool flag = _eventTimer / 500 % 2 == 0 || _eventTimer < 1000;
			if (_gameOver)
			{
				if (_eventTimer > 250 && _pods.Count > 0 && (!_pods[0].Leader || flag))
				{
					_minigameMetaInterface.DrawString(_lineRender, _scores[0].ToString(), new Vector2(303f, 95f), 1f, _pods[0].Colour);
				}
				if (_eventTimer > 500 && _pods.Count > 1 && (!_pods[1].Leader || flag))
				{
					_minigameMetaInterface.DrawString(_lineRender, _scores[1].ToString(), new Vector2(303f, 125f), 1f, _pods[1].Colour);
				}
				if (_eventTimer > 750 && _pods.Count > 2 && (!_pods[2].Leader || flag))
				{
					_minigameMetaInterface.DrawString(_lineRender, _scores[2].ToString(), new Vector2(303f, 155f), 1f, _pods[2].Colour);
				}
				if (_eventTimer > 1000 && _pods.Count > 3 && (!_pods[3].Leader || flag))
				{
					_minigameMetaInterface.DrawString(_lineRender, _scores[3].ToString(), new Vector2(303f, 185f), 1f, _pods[3].Colour);
				}
				_minigameMetaInterface.DrawRetry(_lineRender, new Vector3(320f, 260f, 0f));
				break;
			}
			if (_eventTimer > 250 && _pods.Count > 0 && (!_pods[0].Leader || flag))
			{
				_minigameMetaInterface.DrawString(_lineRender, _pods[0].Score.ToString(), new Vector2(303f, 95f), 1f, _pods[0].Colour);
				if (_pods[0].First)
				{
					_minigameMetaInterface.DrawFlag(_lineRender, new Vector2(283f, 95f), 1f);
				}
			}
			if (_eventTimer > 500 && _pods.Count > 1 && (!_pods[1].Leader || flag))
			{
				_minigameMetaInterface.DrawString(_lineRender, _pods[1].Score.ToString(), new Vector2(303f, 125f), 1f, _pods[1].Colour);
				if (_pods[1].First)
				{
					_minigameMetaInterface.DrawFlag(_lineRender, new Vector2(283f, 125f), 1f);
				}
			}
			if (_eventTimer > 750 && _pods.Count > 2 && (!_pods[2].Leader || flag))
			{
				_minigameMetaInterface.DrawString(_lineRender, _pods[2].Score.ToString(), new Vector2(303f, 155f), 1f, _pods[2].Colour);
				if (_pods[2].First)
				{
					_minigameMetaInterface.DrawFlag(_lineRender, new Vector2(283f, 155f), 1f);
				}
			}
			if (_eventTimer > 1000 && _pods.Count > 3 && (!_pods[3].Leader || flag))
			{
				_minigameMetaInterface.DrawString(_lineRender, _pods[3].Score.ToString(), new Vector2(303f, 185f), 1f, _pods[3].Colour);
				if (_pods[3].First)
				{
					_minigameMetaInterface.DrawFlag(_lineRender, new Vector2(283f, 185f), 1f);
				}
			}
			break;
		}
		}
		float value = 1f;
		postEffect.CurrentTechnique = postEffect.Techniques["Blur"];
		for (int i = 0; i < 4; i++)
		{
			spriteBatch.GraphicsDevice.SetRenderTarget(effectCanvas1);
			// FIX (round 5, grey-screen bug): was Clear(Color.Black), which clears alpha to
			// 255 (opaque). Since this target is immediately re-sampled and additively
			// blended into the next stage, an opaque background alpha defeats the additive
			// blend's alpha-gating (BlendState.Additive scales source RGB by source alpha),
			// letting BlurPixelShader's unconditional "color.rgb += brightness" leak a flat
			// grey wash across the whole screen instead of staying confined near real
			// glow content. Clearing to Transparent (alpha 0) keeps empty background pixels
			// at alpha 0 through the accumulation loop, so the leaked brightness term gets
			// correctly zeroed at the blend stage.
			spriteBatch.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.GraphicsDevice.BlendState = BlendState.Additive;
			if (i == 0)
			{
				if (_minigameMetaState != GameState.Progress)
				{
					value = 0.1f;
				}
				postEffect.Parameters["brightness"].SetValue(value);
				postEffect.Parameters["Texture"].SetValue(vectorCanvas);
				postEffect.CurrentTechnique.Passes[0].Apply();
				quad.Render(Vector2.One * -1f, Vector2.One);
				postEffect.Parameters["brightness"].SetValue(0.6f);
				postEffect.Parameters["Texture"].SetValue(interfaceCanvas);
				postEffect.CurrentTechnique.Passes[0].Apply();
				quad.Render(Vector2.One * -1f, Vector2.One);
			}
			else
			{
				postEffect.Parameters["Texture"].SetValue(effectCanvas0);
				postEffect.CurrentTechnique.Passes[0].Apply();
				quad.Render(Vector2.One * -1f, Vector2.One);
			}
			spriteBatch.GraphicsDevice.SetRenderTarget(effectCanvas0);
			// FIX (round 5, grey-screen bug): see the matching comment at the effectCanvas1
			// clear above -- same alpha-leak issue, same fix.
			spriteBatch.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.GraphicsDevice.BlendState = BlendState.Additive;
			postEffect.Parameters["Texture"].SetValue(effectCanvas1);
			postEffect.CurrentTechnique.Passes[0].Apply();
			quad.Render(Vector2.One * -1f, Vector2.One);
		}
		spriteBatch.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		spriteBatch.GraphicsDevice.Clear(Color.Black);
		spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		postEffect.CurrentTechnique = postEffect.Techniques["ScanLines"];
		postEffect.Parameters["Texture"].SetValue(effectCanvas0);
		postEffect.CurrentTechnique.Passes[0].Apply();
		quad.Render(Vector2.One * -1f, Vector2.One);
		postEffect.CurrentTechnique = postEffect.Techniques["ScanLinesBright"];
		postEffect.Parameters["brightness"].SetValue(value);
		postEffect.Parameters["Texture"].SetValue(vectorCanvas);
		postEffect.CurrentTechnique.Passes[0].Apply();
		quad.Render(Vector2.One * -1f, Vector2.One);
		postEffect.Parameters["brightness"].SetValue(1f);
		postEffect.Parameters["Texture"].SetValue(interfaceCanvas);
		postEffect.CurrentTechnique.Passes[0].Apply();
		quad.Render(Vector2.One * -1f, Vector2.One);
		DumpRenderDiagIfDue();
		base.Draw(gameTime);
	}
}
