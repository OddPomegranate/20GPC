using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FiftyGames.MicroMachinesGame;
using MicroMachinesGame;
using MicroMachinesGame.ISHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.MicroMachines.Entities;

internal class MMPlayer : PhysObject
{
	private const int width = 33;

	private const int height = 33;

	private const int _totalLaps = 5;

	private bool _isAlive;

	private bool _isRacing = true;

	private Player _frameworkPlayer;

	private Texture2D _car;

	private Texture2D _carOverlay;

	private Texture2D _skidTexture;

	private RenderTarget2D _skidRenderTarget;

	private SpriteBatch _spriteBatch;

	private GraphicsDevice _graphicsDevice;

	private Vector2 _halfTexture;

	private Vector2 _halfSkidTexture;

	private float terminalVelocity = 16f;

	private int _lapsCompleted;

	private int lapTime;

	private int raceTime;

	private int sectorTime;

	private List<TimeLog> _logs = new List<TimeLog>();

	private SpriteFont _font;

	private int _nextExpectedSector;

	private bool _firstTimeOverStartLine = true;

	private bool _isSinglePlayer;

	private int averageLapTime;

	private int fastestPersonalLapTime = int.MaxValue;

	private int[] lapTimes = new int[5];

	private int[] sectorTimes = new int[4];

	private List<MMPlayer> _finishedPlayers;

	private SpriteFont _driftPointsFont;

	private int _driftPoints;

	private bool _isBoosting;

	private int _boostLengthTime = 800;

	private int _boostTime;

	private SinglePixelTexture _boostBar;

	private Cue _skidCue;

	private bool _wasSkidingLastFrame;

	private int _place;

	private int _boostFlashingTime;

	private bool _showDarkRedBoostBar;

	private Vector2 _directionPlayerWasWhenBoosted = Vector2.Zero;

	private List<NosSkid> _nosSkids;

	private List<NosSmoke> _nosSmoke;

	private ContentManager _contentManager;

	private Random _random;

	private bool _isSkiding;

	private float _previousRotation;

	private Cue _drivingCue;

	private static Vector2[] startingPositions = new Vector2[4]
	{
		new Vector2(971f, 93f),
		new Vector2(971f, 145f),
		new Vector2(900f, 93f),
		new Vector2(900f, 145f)
	};

	private static int fastestLapTime = int.MaxValue;

	public string Name => _frameworkPlayer.Name;

	public bool IsAlive => _isAlive;

	public bool IsRacing => _isRacing;

	public bool IsBoosting => _isBoosting;

	public bool IsSkiding => _isSkiding;

	public Texture2D Texture => _car;

	public Texture2D TextureOverlay => _carOverlay;

	public Color Color => _frameworkPlayer.Colour();

	public MMPlayer(World world, Player player, ContentManager contentManager, SpriteBatch spriteBatch, RenderTarget2D skidRT, int id, bool isSinglePlayer, List<MMPlayer> finishedPlayers, List<NosSkid> nosSkids, List<NosSmoke> nosSmoke)
		: base(world)
	{
		_contentManager = contentManager;
		_frameworkPlayer = player;
		_isAlive = true;
		_finishedPlayers = finishedPlayers;
		_nosSmoke = nosSmoke;
		_nosSkids = nosSkids;
		ConvertUnits.ToSimUnits(33);
		ConvertUnits.ToSimUnits(33);
		_body = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(20), 1f, ConvertUnits.ToSimUnits(startingPositions[id]));
		_body.BodyType = BodyType.Dynamic;
		_body.Friction = 1f;
		_body.Mass = 1f;
		_body.LinearDamping = 1.4f;
		_body.AngularDamping = 8f;
		_body.UserData = this;
		_body.SleepingAllowed = false;
		_body.OnCollision += _body_OnCollision;
		_previousRotation = _body.Rotation;
		_car = contentManager.Load<Texture2D>("MicroMachines/Car");
		_carOverlay = contentManager.Load<Texture2D>("MicroMachines/CarOverlay");
		_skidTexture = contentManager.Load<Texture2D>("MicroMachines/SkidMark");
		_font = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_driftPointsFont = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_boostBar = new SinglePixelTexture(_carOverlay.GraphicsDevice);
		_halfTexture = new Vector2(_carOverlay.Width / 2, _carOverlay.Height / 2);
		_halfSkidTexture = new Vector2(_skidTexture.Width / 2, _skidTexture.Height / 2);
		_skidRenderTarget = skidRT;
		_spriteBatch = spriteBatch;
		_graphicsDevice = spriteBatch.GraphicsDevice;
		_isSinglePlayer = isSinglePlayer;
		_random = new Random();
		for (int i = 0; i < sectorTimes.Length; i++)
		{
			sectorTimes[i] = int.MaxValue;
		}
		_drivingCue = global::FiftyGames.MicroMachinesGame.MicroMachinesGame.PlaySound("CarEngine");
		_drivingCue.SetVariable("Speed", 0f);
	}

	private bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (fixtureB.Body.UserData is Line && _body.LinearVelocity.Length() > 5f)
		{
			global::FiftyGames.MicroMachinesGame.MicroMachinesGame.PlaySound("Crash");
			float angle = GeometryHelper.V2ToAngle(contact.Manifold.LocalNormal) - (float)Math.PI;
			float rotation = _body.Rotation;
			float num = GeometryHelper.UnsignedAngleBetweenTwoV2(GeometryHelper.AngleToV2(angle, 1f), GeometryHelper.AngleToV2(rotation, 1f));
			num /= 2f;
			num = MathHelper.Clamp(num, 0f, 100f);
			int num2 = (int)(100f * (1f - num));
			if (num2 < 1)
			{
				num2 = 1;
			}
			if (num2 > 2000)
			{
				num2 = 2000;
			}
			_frameworkPlayer.GamePadManager.StartVibration(num2);
		}
		return true;
	}

	public override void Update(GameTime gameTime)
	{
		if (_isAlive && _isRacing)
		{
			GamePadState gamePadStateCurrent = _frameworkPlayer.GamePadManager.GamePadStateCurrent;
			new Vector2(gamePadStateCurrent.ThumbSticks.Right.Y, gamePadStateCurrent.ThumbSticks.Right.X);
			Vector2 vector = new Vector2(gamePadStateCurrent.ThumbSticks.Left.X, gamePadStateCurrent.ThumbSticks.Left.Y * -1f);
			GeometryHelper.AngleToV2(_body.Rotation, 1f);
			_drivingCue.SetVariable("Speed", 100f * (_body.LinearVelocity.Length() / terminalVelocity));
			if (gamePadStateCurrent.IsButtonDown(Buttons.A) && _driftPoints > 100 && !_isBoosting)
			{
				_isBoosting = true;
				_directionPlayerWasWhenBoosted = GeometryHelper.AngleToV2(_body.Rotation, 1f);
			}
			float num = 30f;
			if (_isBoosting)
			{
				num = 35f;
				_boostTime += gameTime.ElapsedGameTime.Milliseconds * 2;
				_driftPoints = 0;
				if (_boostTime > _boostLengthTime)
				{
					_isBoosting = false;
					_boostTime = 0;
				}
				if (vector.Length() > 0.2f)
				{
					float num2 = GeometryHelper.V2ToAngle(vector);
					float rotation = _body.Rotation;
					float num3 = Math.Abs(num2 - rotation);
					if (num3 < MathHelper.ToRadians(90f))
					{
						_directionPlayerWasWhenBoosted = vector;
					}
				}
				Vector2 vector2 = GeometryHelper.AngleToV2(_body.Rotation, _directionPlayerWasWhenBoosted.Length() * num);
				vector2 = (vector2 - _body.LinearVelocity) * _body.Mass;
				float turnSpeed = (terminalVelocity - _body.LinearVelocity.Length()) / 15f;
				_body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, base.DisplayPosition + _directionPlayerWasWhenBoosted, _body.Rotation, turnSpeed);
				_body.ApplyLinearImpulse(ConvertUnits.ToSimUnits(vector2));
			}
			else if (vector.Length() > 0.2f)
			{
				Vector2 vector2 = GeometryHelper.AngleToV2(_body.Rotation, vector.Length() * num);
				vector2 = (vector2 - _body.LinearVelocity) * _body.Mass;
				float turnSpeed2 = (terminalVelocity - _body.LinearVelocity.Length()) / 15f;
				_body.Rotation = GeometryHelper.TurnToFace(base.DisplayPosition, base.DisplayPosition + vector, _body.Rotation, turnSpeed2);
				_body.ApplyLinearImpulse(ConvertUnits.ToSimUnits(vector2));
			}
			float num4 = GeometryHelper.UnsignedAngleBetweenTwoV2(GeometryHelper.AngleToV2(_body.Rotation, 1f), GeometryHelper.AngleToV2(_previousRotation, 1f));
			if (_body.LinearVelocity.Length() > terminalVelocity / 18f && num4 > MathHelper.ToRadians(10f))
			{
				_driftPoints += 6;
				if (!_wasSkidingLastFrame)
				{
					OnStartSkid();
					_wasSkidingLastFrame = true;
				}
			}
			else if (_wasSkidingLastFrame)
			{
				OnEndSkid();
				_wasSkidingLastFrame = false;
			}
			if (_wasSkidingLastFrame)
			{
				_skidCue.SetVariable("SkidVolume", 100f - MathHelper.Lerp(0f, 100f, num4 / (float)Math.PI));
			}
			if (gamePadStateCurrent.Buttons.B == ButtonState.Pressed)
			{
				GameConsole.PrintString(_body.LinearVelocity.Length().ToString());
			}
			lapTime += gameTime.ElapsedGameTime.Milliseconds * 2;
			sectorTime += gameTime.ElapsedGameTime.Milliseconds * 2;
			raceTime += gameTime.ElapsedGameTime.Milliseconds * 2;
		}
		for (int i = 0; i < _logs.Count; i++)
		{
			TimeLog value = _logs[i];
			value.position = new Vector2(value.position.X, value.position.Y - 0.6f);
			_logs[i] = value;
		}
		for (int j = 0; j < _logs.Count; j++)
		{
			if (_logs[j].position.Y < 0f)
			{
				_logs.RemoveAt(j);
				j--;
			}
		}
		_previousRotation = _body.Rotation;
	}

	public void OnDeath()
	{
		if (_drivingCue != null && !_drivingCue.IsPlaying)
		{
			_drivingCue.Stop(AudioStopOptions.AsAuthored);
		}
	}

	public void OnPastCheckpoint(TrackCheckpoint checkpoint)
	{
		if (checkpoint.ID == _nextExpectedSector)
		{
			if (checkpoint.ID == 0)
			{
				LapCompleted();
			}
			else
			{
				global::FiftyGames.MicroMachinesGame.MicroMachinesGame.PlaySound("Checkpoint");
				PassedCheckpoint(checkpoint);
			}
			checkpoint.BlinkCheckpoint(this);
			if (checkpoint.ID < 3)
			{
				_nextExpectedSector++;
			}
			else
			{
				_nextExpectedSector = 0;
			}
		}
	}

	private void LapCompleted()
	{
		if (_firstTimeOverStartLine)
		{
			lapTime = 0;
			raceTime = 0;
			_firstTimeOverStartLine = false;
		}
		else
		{
			if (fastestLapTime > lapTime)
			{
				LogTime(lapTime, Color.LimeGreen);
				fastestLapTime = lapTime;
			}
			else
			{
				LogTime(lapTime, Color.Red);
			}
			lapTimes[_lapsCompleted] = lapTime;
			_lapsCompleted++;
			bool flag = false;
			if (_lapsCompleted == 5)
			{
				_isRacing = false;
				if (!_finishedPlayers.Contains(this))
				{
					if (_finishedPlayers.Count == 0)
					{
						global::FiftyGames.MicroMachinesGame.MicroMachinesGame.PlaySound("Win");
						flag = true;
					}
					_finishedPlayers.Add(this);
					_place = _finishedPlayers.Count;
				}
			}
			if (!flag)
			{
				global::FiftyGames.MicroMachinesGame.MicroMachinesGame.PlaySound("Checkpoint");
			}
			UpdateFastestPersonalLapTime();
			int totalLapsTime = GetTotalLapsTime();
			averageLapTime = totalLapsTime / _lapsCompleted;
			lapTime = 0;
		}
		sectorTime = 0;
	}

	public void ForceFinish()
	{
		_isRacing = false;
		if (!_finishedPlayers.Contains(this))
		{
			_finishedPlayers.Add(this);
		}
	}

	public void UpdateFastestPersonalLapTime()
	{
		int[] array = lapTimes;
		foreach (int num in array)
		{
			if (num != 0 && num < fastestPersonalLapTime)
			{
				fastestPersonalLapTime = num;
			}
		}
	}

	public int GetTotalLapsTime()
	{
		int num = 0;
		int[] array = lapTimes;
		foreach (int num2 in array)
		{
			num += num2;
		}
		return num;
	}

	private void PassedCheckpoint(TrackCheckpoint checkpoint)
	{
		if (_isSinglePlayer)
		{
			if (sectorTime < sectorTimes[checkpoint.ID])
			{
				sectorTimes[checkpoint.ID] = sectorTime;
				LogTime(sectorTime, Color.LimeGreen);
			}
			else
			{
				LogTime(sectorTime, Color.Red);
			}
		}
		sectorTime = 0;
	}

	private void LogTime(int mills, Color color)
	{
		float num = (float)mills / 1000f;
		TimeLog item = new TimeLog
		{
			position = new Vector2(0f, 20f),
			text = num.ToString("F3"),
			color = color
		};
		_logs.Add(item);
	}

	private void OnStartSkid()
	{
		if (_skidCue == null)
		{
			_skidCue = global::FiftyGames.MicroMachinesGame.MicroMachinesGame.PlaySound("Skid");
		}
		else
		{
			_skidCue.Resume();
		}
		_isSkiding = true;
	}

	private void OnEndSkid()
	{
		_skidCue.Pause();
		_isSkiding = false;
	}

	public void DrawSkid(bool fullAlpha)
	{
		float rotation = ((!(_body.LinearVelocity.Length() < 0.1f)) ? GeometryHelper.V2ToAngle(_body.LinearVelocity) : (GeometryHelper.V2ToAngle(_body.LinearVelocity) - (float)Math.PI / 2f));
		if (fullAlpha)
		{
			_spriteBatch.Draw(_skidTexture, base.DisplayPosition, null, Color.Red, rotation, _halfTexture, 1f, SpriteEffects.None, 0f);
		}
		else
		{
			_spriteBatch.Draw(_skidTexture, base.DisplayPosition, null, Color.White * (_body.LinearVelocity.Length() / terminalVelocity), rotation, _halfTexture, 1f, SpriteEffects.None, 0f);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin();
		_spriteBatch.Draw(_car, base.DisplayPosition, null, Color.White, _body.Rotation, _halfTexture, 1f, SpriteEffects.None, 0f);
		_spriteBatch.Draw(_carOverlay, base.DisplayPosition, null, _frameworkPlayer.Colour(), _body.Rotation, _halfTexture, 1f, SpriteEffects.None, 0f);
		_spriteBatch.Draw(_car, base.DisplayPosition, null, _frameworkPlayer.Colour() * 0.3f, _body.Rotation, _halfTexture, 1f, SpriteEffects.None, 0f);
		foreach (TimeLog log in _logs)
		{
			Helper.DrawOutlinedText(_spriteBatch, _font, log.text, base.DisplayPosition + log.position - new Vector2(0f, 50f), log.color, Color.Black);
		}
		int x = (int)base.DisplayPosition.X - 10;
		int y = (int)base.DisplayPosition.Y - 30;
		_spriteBatch.Draw(_boostBar, new Rectangle(x, y, 25, 5), Color.Black);
		if (MathHelper.Clamp(_driftPoints, 0f, 100f) < 99f)
		{
			_spriteBatch.Draw(_boostBar, new Rectangle(x, y, (int)MathHelper.Clamp(_driftPoints, 0f, 100f) / 4, 5), Color.Green);
		}
		else
		{
			_boostFlashingTime--;
			if (_boostFlashingTime < 0)
			{
				if (_showDarkRedBoostBar)
				{
					_showDarkRedBoostBar = false;
				}
				else
				{
					_showDarkRedBoostBar = true;
				}
				_boostFlashingTime = 20;
			}
			if (_showDarkRedBoostBar)
			{
				spriteBatch.Draw(_boostBar, new Rectangle(x, y, (int)MathHelper.Clamp(_driftPoints, 0f, 100f) / 4, 5), Color.Green);
			}
			else
			{
				spriteBatch.Draw(_boostBar, new Rectangle(x, y, (int)MathHelper.Clamp(_driftPoints, 0f, 100f) / 4, 5), Color.White);
			}
		}
		spriteBatch.End();
		if (_isBoosting)
		{
			Vector2 linearVelocity = _body.LinearVelocity;
			linearVelocity.Normalize();
			_nosSmoke.Add(new NosSmoke(_contentManager.Load<Texture2D>("MicroMachines/smoke"), base.DisplayPosition, linearVelocity, _random));
			_nosSmoke.Add(new NosSmoke(_contentManager.Load<Texture2D>("MicroMachines/smoke"), base.DisplayPosition, linearVelocity, _random));
			_nosSmoke.Add(new NosSmoke(_contentManager.Load<Texture2D>("MicroMachines/smoke"), base.DisplayPosition, linearVelocity, _random));
		}
	}

	public int GetLapTime(int lap)
	{
		return lapTimes[lap];
	}

	public RaceStats GetRaceStats()
	{
		return new RaceStats
		{
			lapTimes = lapTimes,
			averageLapTime = averageLapTime,
			bestLap = fastestPersonalLapTime,
			totalTime = GetTotalLapsTime(),
			player = this,
			place = _place
		};
	}
}
