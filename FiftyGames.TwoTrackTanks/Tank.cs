using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.TwoTrackTanks;

internal class Tank : PhysicsObject
{
	public const int MaxHealth = 100;

	private const int ReloadTime = 2000;

	private ContentManager _contentLoader;

	private World _physicsWorld;

	private Player _driver;

	private Player _gunner;

	private Color _bodyColour;

	private Color _turretColour;

	private Texture2D _texBodyColour;

	private Texture2D _texBodyDetail;

	private Texture2D _texTurretBase;

	private Texture2D _texTurretColour;

	private Texture2D _texTurretDetail;

	private Texture2D _texTurretBarrel;

	private Texture2D _texTurretGuage;

	private Texture2D _texTurretNeedle;

	private Texture2D _reticleDetail;

	private Texture2D _reticleColour;

	private Texture2D[] _pathLines;

	private Vector2 _reticlePos;

	private Vector2 _turretBaseOrigin;

	private Vector2 _turretBarrelOrigin;

	private Vector2 _turretGuageOrigin;

	private Vector2 _reticleOrigin;

	private Vector2 _pathOrigin;

	private Vector2 _forwardVector;

	private int _health;

	private float _turretRotation;

	private float _turretPower;

	private float _leftTrackSpeed;

	private float _rightTrackSpeed;

	private float MinPower = 100f;

	private float MaxPower = 600f;

	private bool _swapped;

	private int _reloadTimer;

	private Random _ranGen;

	private byte[] _lineValues;

	private Cue _engineSound;

	private Cue _turretTurnSound;

	public int Health => _health;

	public Cue EngineSound
	{
		get
		{
			return _engineSound;
		}
		set
		{
			_engineSound = value;
			_engineSound.Play();
		}
	}

	public Vector2 TargetPosition => _reticlePos;

	public Player Driver
	{
		get
		{
			return _driver;
		}
		set
		{
			_driver = value;
		}
	}

	public Player Gunner
	{
		get
		{
			return _gunner;
		}
		set
		{
			_gunner = value;
		}
	}

	public Tank(Player driver, Player gunner, Random rng)
	{
		_driver = driver;
		_gunner = gunner;
		_bodyColour = driver.Colour(0.8f, 0.8f);
		_turretColour = gunner?.Colour(0.8f, 0.8f) ?? Color.White;
		_colour = Color.White;
		_forwardVector = Vector2.Zero;
		_turretRotation = 0f;
		_reticlePos = Vector2.Zero;
		_health = 100;
		_turretPower = MinPower;
		_swapped = false;
		_ranGen = rng;
		_lineValues = new byte[18];
		for (int i = 0; i < _lineValues.Length; i++)
		{
			_lineValues[i] = (byte)_ranGen.Next(3);
		}
	}

	public Tank(Player driver, Random rng)
		: this(driver, null, rng)
	{
	}

	public void Load(ContentManager contentLoader, World physicsWorld, SoundManager soundManager)
	{
		_sprite = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankBodyBase");
		_texBodyColour = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankBodyColour");
		_texBodyDetail = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankBodyDetail");
		_texTurretBase = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankTurretBase");
		_texTurretColour = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankTurretColour");
		_texTurretDetail = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankTurretDetail");
		_texTurretBarrel = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankTurretBarrel");
		_texTurretGuage = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankTurretGuage");
		_texTurretNeedle = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\TankTurretNeedle");
		_reticleDetail = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\ReticleDetail");
		_reticleColour = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\ReticleColour");
		_pathLines = new Texture2D[3];
		for (int i = 0; i < _pathLines.Length; i++)
		{
			_pathLines[i] = contentLoader.Load<Texture2D>("TwoTrackTanks\\Image\\PathLine" + i);
		}
		_origin = new Vector2((float)_sprite.Width * 0.5f, (float)_sprite.Height * 0.5f);
		_turretBaseOrigin = new Vector2((float)_texTurretBase.Width * 0.5f, (float)_texTurretBase.Height * 0.5f);
		_turretBarrelOrigin = new Vector2((float)_texTurretBarrel.Width * 0.5f, (float)_texTurretBarrel.Height + (float)_texTurretBarrel.Width * 0.8f);
		_turretGuageOrigin = new Vector2((float)_texTurretGuage.Width * 0.5f, (float)_texTurretGuage.Height * 0.5f);
		_reticleOrigin = new Vector2((float)_reticleColour.Width * 0.5f, (float)_reticleColour.Height * 0.5f);
		_pathOrigin = new Vector2((float)_pathLines[0].Width * 0.5f, (float)_pathLines[0].Height * 0.5f);
		_physBody = BodyFactory.CreateRectangle(physicsWorld, ConvertUnits.ToSimUnits((float)_sprite.Width - 6f), ConvertUnits.ToSimUnits((float)_sprite.Height - 6f), 10f);
		_physBody.BodyType = BodyType.Dynamic;
		_physBody.CollisionCategories = Category.Cat1;
		_physBody.CollidesWith = Category.All;
		_physBody.UserData = this;
		_engineSound = soundManager.CreateGameSoundCue("twoTrackTanks TankEngine");
		_engineSound.Play();
		_turretTurnSound = soundManager.CreateGameSoundCue("twoTrackTanks TurretTurn");
		_turretTurnSound.Play();
		_turretTurnSound.Pause();
		_contentLoader = contentLoader;
		_physicsWorld = physicsWorld;
		_soundManager = soundManager;
	}

	public void Update(GameTime gameTime, List<Projectile> projectiles)
	{
		_leftTrackSpeed = 0f;
		if (_driver != null)
		{
			_leftTrackSpeed = _driver.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Y;
		}
		_rightTrackSpeed = 0f;
		if (_driver != null)
		{
			_rightTrackSpeed = _driver.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Y;
		}
		_forwardVector = Vector2.Transform(new Vector2(0f, -1f), Matrix.CreateRotationZ(base.Rotation));
		Vector2 vector = Vector2.Transform(_forwardVector, Matrix.CreateRotationZ(-(float)Math.PI / 2f)) * ConvertUnits.ToSimUnits(100f);
		Vector2 vector2 = Vector2.Transform(_forwardVector, Matrix.CreateRotationZ((float)Math.PI / 2f)) * ConvertUnits.ToSimUnits(100f);
		_physBody.LinearVelocity = Vector2.Zero;
		_physBody.AngularVelocity = 0f;
		_physBody.ApplyLinearImpulse(_forwardVector * ConvertUnits.ToSimUnits(_leftTrackSpeed) * _physBody.Mass * 0.1f, ConvertUnits.ToSimUnits(base.Position + vector));
		_physBody.ApplyLinearImpulse(_forwardVector * ConvertUnits.ToSimUnits(_rightTrackSpeed) * _physBody.Mass * 0.1f, ConvertUnits.ToSimUnits(base.Position + vector2));
		_engineSound.SetVariable("Speed", Math.Abs(_leftTrackSpeed) * 10f + Math.Abs(_rightTrackSpeed) * 10f);
		bool flag = false;
		bool flag2 = false;
		_ = Vector2.Zero;
		_ = Vector2.Zero;
		float num = 0f;
		if (_gunner != null && _gunner.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.Length() > 0.8f)
		{
			Vector2 left = _gunner.GamePadManager.GamePadStateCurrent.ThumbSticks.Left;
			Vector2 left2 = _gunner.GamePadManager.GamePadStatePrevious.ThumbSticks.Left;
			num = (float)Math.Atan2(left.X, left.Y) - (float)Math.Atan2(left2.X, left2.Y);
		}
		if (Math.Abs(num) > (float)Math.PI)
		{
			num = ((num < 0f) ? (num + (float)Math.PI * 2f) : ((float)Math.PI * 2f - num));
		}
		if (num != 0f)
		{
			_turretRotation += num * 0.2f;
			flag = true;
		}
		num = 0f;
		if (_gunner != null && _gunner.GamePadManager.GamePadStateCurrent.ThumbSticks.Right.Length() > 0.8f)
		{
			Vector2 left = _gunner.GamePadManager.GamePadStateCurrent.ThumbSticks.Right;
			Vector2 left2 = _gunner.GamePadManager.GamePadStatePrevious.ThumbSticks.Right;
			num = (float)Math.Atan2(left.X, left.Y) - (float)Math.Atan2(left2.X, left2.Y);
		}
		if (Math.Abs(num) > (float)Math.PI)
		{
			num = ((num < 0f) ? (num + (float)Math.PI * 2f) : ((float)Math.PI * 2f - num));
		}
		if (num != 0f)
		{
			_turretPower += num * 10f;
			if (_turretPower < MinPower)
			{
				_turretPower = MinPower;
			}
			else if (_turretPower > MaxPower)
			{
				_turretPower = MaxPower;
			}
			flag2 = true;
		}
		if (flag || flag2)
		{
			_turretTurnSound.Resume();
		}
		else
		{
			_turretTurnSound.Pause();
		}
		_reticlePos = base.Position + Vector2.Transform(new Vector2(0f, 0f - _turretPower), Matrix.CreateRotationZ(base.Rotation + _turretRotation));
		if (_reloadTimer == 0 && ((_gunner == null && _driver.GamePadManager.ButtonWasPressed(Buttons.A)) || (_gunner != null && _gunner.GamePadManager.ButtonWasPressed(Buttons.A))))
		{
			Projectile projectile = new Projectile(this);
			projectile.Load(_contentLoader, _physicsWorld);
			projectile.PhysicsBody.Position = _physBody.Position;
			projectile.Rotation = base.Rotation + _turretRotation;
			projectile.PhysicsBody.LinearVelocity = new Vector2((float)Math.Cos(projectile.Rotation - (float)Math.PI / 2f), (float)Math.Sin(projectile.Rotation - (float)Math.PI / 2f)) * ConvertUnits.ToSimUnits(0.8f);
			projectile.PhysicsBody.IgnoreCollisionWith(_physBody);
			projectiles.Add(projectile);
			_soundManager.CreateGameSoundCue("twoTrackTanks FireCannon").Play();
			_reloadTimer = 2000;
		}
		if (_reloadTimer > 0)
		{
			_reloadTimer -= gameTime.ElapsedGameTime.Milliseconds;
		}
		else if (_reloadTimer < 0)
		{
			_reloadTimer = 0;
		}
		if (!_swapped && (_driver == null || _driver.GamePadManager.ButtonIsHeld(Buttons.B)) && (_gunner == null || _gunner.GamePadManager.ButtonIsHeld(Buttons.B)))
		{
			Player driver = _driver;
			_driver = _gunner;
			_gunner = driver;
			Color bodyColour = _bodyColour;
			_bodyColour = _turretColour;
			_turretColour = bodyColour;
			_swapped = true;
		}
		if ((_driver == null || !_driver.GamePadManager.ButtonIsHeld(Buttons.B)) && (_gunner == null || !_gunner.GamePadManager.ButtonIsHeld(Buttons.B)))
		{
			_swapped = false;
		}
		base.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_forwardVector = Vector2.Transform(new Vector2(0f, -1f), Matrix.CreateRotationZ(base.Rotation));
		base.Draw(spriteBatch);
		spriteBatch.Draw(_texBodyColour, base.Position, null, _bodyColour, base.Rotation, _origin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_texBodyDetail, base.Position, null, Color.White, base.Rotation, _origin, 1f, SpriteEffects.None, 0f);
	}

	public void DrawTurret(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_texTurretBase, base.Position, null, Color.White, base.Rotation + _turretRotation, _turretBaseOrigin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_texTurretColour, base.Position, null, _turretColour, base.Rotation + _turretRotation, _turretBaseOrigin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_texTurretDetail, base.Position, null, Color.White, base.Rotation + _turretRotation, _turretBaseOrigin, 1f, SpriteEffects.None, 0f);
		Vector2 vector = Vector2.Transform(_forwardVector * -30f, Matrix.CreateRotationZ(_turretRotation));
		spriteBatch.Draw(_texTurretGuage, base.Position + vector, null, Color.White, base.Rotation + _turretRotation, _turretGuageOrigin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_texTurretNeedle, base.Position + vector, null, Color.White, base.Rotation + _turretRotation + (_turretPower - MinPower) / (MaxPower - MinPower) * (float)Math.PI * 1.4f, _turretGuageOrigin, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(_texTurretBarrel, new Rectangle((int)base.Position.X, (int)base.Position.Y, _texTurretBarrel.Width, (int)((float)_texTurretBarrel.Height * (1f - (_turretPower - MinPower) / ((MaxPower - MinPower) * 3f)))), null, Color.White, base.Rotation + _turretRotation, _turretBarrelOrigin, SpriteEffects.None, 1f);
	}

	public void DrawReticle(SpriteBatch spriteBatch)
	{
		Rectangle value = new Rectangle((int)((float)_reloadTimer / 2000f * 8f) * _reticleColour.Width, 0, _reticleColour.Width, _reticleColour.Height);
		for (int i = 2; (float)i < ((base.Position - _reticlePos).Length() - 40f) / 60f; i++)
		{
			Vector2 vector = base.Position + Vector2.Transform(new Vector2(0f, -60f * (float)i), Matrix.CreateRotationZ(base.Rotation + _turretRotation));
			float num = (vector - _reticlePos).Length();
			spriteBatch.Draw(_pathLines[_lineValues[i]], vector, null, _turretColour * ((num > 120f) ? 1f : (num / 120f)), base.Rotation + _turretRotation, _pathOrigin, 1f, (SpriteEffects)_lineValues[i + _lineValues.Length / 2 - 2], 1f);
		}
		spriteBatch.Draw(_reticleColour, _reticlePos, null, Color.Lerp(_turretColour, Color.Gray, 0.3f), 0f, _reticleOrigin, 1f, SpriteEffects.None, 1f);
		spriteBatch.Draw(_reticleDetail, _reticlePos, value, Color.White, 0f, _reticleOrigin, 1f, SpriteEffects.None, 1f);
	}

	public void Damage(int damage)
	{
		_health -= damage;
		if (_health < 0)
		{
			_health = 0;
		}
	}
}
