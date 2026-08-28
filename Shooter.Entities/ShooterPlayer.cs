using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FiftyGames.Shooter;
using FiftyGames.ShooterGame;
using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Guns;
using Shooter.ISHelpers;

namespace Shooter.Entities;

internal class ShooterPlayer : PhysObject
{
	private const int _respawnTime = 3000;

	private const int _fullHealth = 100;

	protected NavMesh _navMesh;

	protected List<ShooterPlayer> _allPlayers;

	protected Random _random;

	protected float _rotation;

	protected bool _isShooting;

	protected Gun _currentGun;

	protected int _health;

	protected bool _isAlive;

	protected int _id;

	protected bool _hasChangedToNewGun;

	protected int[] _damageByPlayers;

	public Cue _lastShotCuePlayed;

	protected bool _hasJustShot;

	protected string _lastShotPath;

	private ContentManager _contentManager;

	private Texture2D _playerHead;

	private Texture2D _playerBody;

	private Texture2D _bigGunArmsAnim;

	private Texture2D _smallGunArmsAnim;

	private Texture2D _healthBar;

	private RenderTarget2D _tempRT1;

	private RenderTarget2D _healthRT;

	private RenderTarget2D _ammoRT;

	private float xOffset = 23f;

	private float yOffset = 28f;

	private int _armPositionIndex;

	private int _elapsedMilliseconds;

	private SinglePixelTexture _debugTexture;

	private int _gunIndex;

	private List<GunSettings> _guns;

	private int _respawnTimerMills;

	public float _lastLookAngle;

	public Color _color;

	private ParticleParameters _particleParams;

	private ParticleParameters _particleParamsBlood;

	private CustomParticleDescriptor _particleDescriptor;

	private List<Texture2D> _deathTextures;

	private Vector2 _lastDirectionOfDamage;

	private bool _hasWon;

	private bool _isDrawingGunChange;

	private Vector2 _drawingGunChangePoisition;

	private float _alphaGunChange = 1.5f;

	private Effect _maskEffect;

	private Vector2 _lastPosition;

	private Vector2 lastStepSoundPosition;

	private bool _wasAtTheSamePlaceLastFrame;

	public bool IsAlive => _isAlive;

	public bool HasWon => _hasWon;

	public ShooterPlayer(int id, World world, Random random, ContentManager contentManager, NavMesh navMesh, List<ShooterPlayer> allPlayers, List<GunSettings> gunSettings, RenderTarget2D ammoHealthRT)
		: base(world)
	{
		_random = random;
		_id = id;
		_navMesh = navMesh;
		_allPlayers = allPlayers;
		CreateBody(GetFreeSpawnPosition());
		_contentManager = contentManager;
		_guns = gunSettings;
		_maskEffect = contentManager.Load<Effect>("Shooter/Player/MaskEffect");
		_playerHead = contentManager.Load<Texture2D>("Shooter/Player/PlayerHead");
		_playerBody = contentManager.Load<Texture2D>("Shooter/Player/PlayerBody");
		_bigGunArmsAnim = contentManager.Load<Texture2D>("Shooter/Player/PlayerAnimation");
		_smallGunArmsAnim = contentManager.Load<Texture2D>("Shooter/Player/PlayerAnimationSmall");
		_healthBar = contentManager.Load<Texture2D>("Shooter/Player/HealthBar1");
		_isShooting = true;
		_debugTexture = new SinglePixelTexture(_playerHead.GraphicsDevice);
		_tempRT1 = new RenderTarget2D(_playerHead.GraphicsDevice, 64, 64);
		_healthRT = new RenderTarget2D(_playerHead.GraphicsDevice, 64, 64);
		_ammoRT = new RenderTarget2D(_playerHead.GraphicsDevice, 64, 64);
		_gunIndex = 0;
		_health = 100;
		_respawnTimerMills = 3000;
		_damageByPlayers = new int[100];
		_isAlive = true;
		_hasChangedToNewGun = false;
		_deathTextures = new List<Texture2D>();
		_deathTextures.Add(contentManager.Load<Texture2D>("Shooter/Particles/PlayerPart1"));
		_deathTextures.Add(contentManager.Load<Texture2D>("Shooter/Particles/PlayerPart2"));
		_deathTextures.Add(contentManager.Load<Texture2D>("Shooter/Particles/PlayerPart3"));
		_deathTextures.Add(contentManager.Load<Texture2D>("Shooter/Particles/PlayerBlood"));
		_particleParams = default(ParticleParameters);
		_particleDescriptor = new CustomParticleDescriptor(contentManager);
		_particleParams = _particleDescriptor.ToParticleParameters();
		_particleParams.MinColor = Vector3.One;
		_particleParams.MaxColor = Vector3.One;
		_particleParams.MinColorChange1 = Vector3.Zero;
		_particleParams.MinColorChange2 = Vector3.Zero;
		_particleParams.MaxColorChange1 = Vector3.Zero;
		_particleParams.MaxColorChange2 = Vector3.Zero;
		_particleParams.MinScaleChange2 = Vector2.Zero;
		_particleParams.MaxScaleChange2 = Vector2.Zero;
		_particleParams.MinSpeed = 0.25f;
		_particleParams.MaxSpeed = 8f;
		_particleParams.MinAlpha = 0.6f;
		_particleParams.MaxAlpha = 1f;
		_particleParams.MinAlphaChange1 = 0f;
		_particleParams.MaxAlphaChange1 = 0f;
		_particleParams.MaxAlphaChange2 = -0.0125f;
		_particleParams.MinAlphaChange2 = -0.005f;
		_particleParams.MaxScale = new Vector2(1f, 1f);
		_particleParams.MinScale = new Vector2(0.7f, 0.7f);
		_particleParams.Directional = true;
		_particleParams.MinRotation = 0f;
		_particleParams.MaxRotation = 100f;
		_particleParams.CanRotate = true;
		_particleParams.Origin = new Vector2(14f, 7f);
		_particleParams.Change = 100;
		_particleParams.Multiplicative = 0.85f;
		_particleParamsBlood = _particleDescriptor.ToParticleParameters();
		_particleParamsBlood.MaxAlphaChange1 = 0f;
		_particleParamsBlood.MaxAlphaChange2 = -0.01f;
		_particleParamsBlood.MinAlphaChange1 = 0f;
		_particleParamsBlood.MinAlphaChange2 = -0.005f;
		_particleParamsBlood.MaxColorChange1 = Vector3.One;
		_particleParamsBlood.MinColorChange1 = Vector3.One;
		_particleParamsBlood.MaxColorChange2 = Vector3.One;
		_particleParamsBlood.MinColorChange2 = Vector3.One;
		_particleParamsBlood.MinAlpha = 0.3f;
		_particleParamsBlood.MaxAlpha = 0.5f;
		_particleParamsBlood.MinRotation = 0f;
		_particleParamsBlood.MaxRotation = 100f;
		_particleParamsBlood.MinSpeed = 0f;
		_particleParamsBlood.MaxSpeed = 64f;
		_particleParamsBlood.Multiplicative = 0f;
		_particleParamsBlood.Change = 400;
	}

	public override void Update(GameTime gameTime)
	{
		if (_isAlive)
		{
			_elapsedMilliseconds += gameTime.TotalGameTime.Milliseconds;
			if (_elapsedMilliseconds % 2 == 0 && _armPositionIndex > 0)
			{
				_armPositionIndex--;
			}
			_currentGun.Update(gameTime);
			_rotation = base.Body.Rotation;
			if (_lastPosition != base.DisplayPosition)
			{
				if (_wasAtTheSamePlaceLastFrame)
				{
					ShooterGame.PlayCue("Walk");
					_wasAtTheSamePlaceLastFrame = false;
					lastStepSoundPosition = base.DisplayPosition;
				}
				else if (Vector2.Distance(lastStepSoundPosition, base.DisplayPosition) > 100f)
				{
					ShooterGame.PlayCue("Walk");
					lastStepSoundPosition = base.DisplayPosition;
				}
			}
			else
			{
				_wasAtTheSamePlaceLastFrame = true;
			}
		}
		else
		{
			_respawnTimerMills -= gameTime.ElapsedGameTime.Milliseconds;
			if (_respawnTimerMills < 0)
			{
				_respawnTimerMills = 3000;
				OnRespawn();
			}
		}
		if (_isDrawingGunChange)
		{
			if (_alphaGunChange <= 0f)
			{
				_isDrawingGunChange = false;
				_alphaGunChange = 1.5f;
				_drawingGunChangePoisition = Vector2.Zero;
			}
			else
			{
				_alphaGunChange -= 0.01f;
			}
		}
		_lastPosition = base.DisplayPosition;
	}

	public virtual void CreateBody(Vector2 position)
	{
		_body = BodyFactory.CreateCircle(_world, ConvertUnits.ToSimUnits(25), 10f, ConvertUnits.ToSimUnits(position));
		_body.BodyType = BodyType.Dynamic;
		_body.Friction = 1f;
		_body.Mass = 1f;
		_body.LinearDamping = 5f;
		_body.AngularDamping = 8f;
		_body.UserData = this;
		_body.SleepingAllowed = true;
	}

	public void DisposeRTs()
	{
		ShooterGame.DisposeRenderTarget(_tempRT1);
		ShooterGame.DisposeRenderTarget(_healthRT);
		ShooterGame.DisposeRenderTarget(_ammoRT);
		_tempRT1 = null;
		_healthRT = null;
		_ammoRT = null;
	}

	public Vector2 GetHandPosition()
	{
		return base.DisplayPosition + Vector2.Transform(new Vector2(18 + (4 - _armPositionIndex) * 3, 14f), Matrix.CreateRotationZ(base.Body.Rotation));
	}

	public Vector2 GetRelativePosition(int x, int y)
	{
		return base.DisplayPosition + Vector2.Transform(new Vector2(18 + (4 - _armPositionIndex) * 3 + x, 14 * y), Matrix.CreateRotationZ(base.Body.Rotation));
	}

	public void PullBackArms()
	{
		_armPositionIndex = 4;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isAlive)
		{
			Vector2 displayPosition = base.DisplayPosition;
			Vector2 position = displayPosition + Vector2.Transform(new Vector2(18 + (4 - _armPositionIndex) * 3, 14f), Matrix.CreateRotationZ(base.Body.Rotation));
			spriteBatch.Begin();
			spriteBatch.Draw(_playerBody, displayPosition, null, _color, base.Body.Rotation, new Vector2(xOffset, yOffset), 1f, SpriteEffects.None, 0f);
			if (_currentGun.Settings.IsSmallGun)
			{
				spriteBatch.Draw(_smallGunArmsAnim, displayPosition, new Rectangle(_armPositionIndex * 64, 0, 64, 56), _color, base.Body.Rotation, new Vector2(xOffset, yOffset), 1f, SpriteEffects.None, 0.2f);
			}
			else
			{
				spriteBatch.Draw(_bigGunArmsAnim, displayPosition, new Rectangle(_armPositionIndex * 64, 0, 64, 56), _color, base.Body.Rotation, new Vector2(xOffset, yOffset), 1f, SpriteEffects.None, 0.2f);
			}
			spriteBatch.Draw(_debugTexture, position, null, Color.White, 0f, new Vector2(0.5f, 0.5f), 2f, SpriteEffects.None, 0f);
			spriteBatch.End();
			_currentGun.Draw(spriteBatch, position, base.Body.Rotation, this);
			spriteBatch.Begin();
			spriteBatch.Draw(_playerHead, displayPosition, null, _color, base.Body.Rotation, new Vector2(xOffset, yOffset), 1f, SpriteEffects.None, 0.2f);
			if (_isDrawingGunChange)
			{
				Point center = _currentGun.GetGroundTexture().Bounds.Center;
				spriteBatch.Draw(_currentGun.GetGroundTexture(), base.DisplayPosition - new Vector2(0f, 50f), null, Color.White * _alphaGunChange, 0f, new Vector2(center.X, center.Y), 1f, SpriteEffects.None, 0f);
			}
			spriteBatch.Draw(_healthRT, displayPosition - new Vector2(32f, 32f), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
			spriteBatch.Draw(_ammoRT, displayPosition - new Vector2(32f, 32f), null, Color.CornflowerBlue, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
			spriteBatch.End();
		}
	}

	public void GenerateMaskedBar(SpriteBatch spriteBatch)
	{
		spriteBatch.GraphicsDevice.SetRenderTarget(_tempRT1);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin();
		float num = (float)Math.PI / 100f;
		float rotation = (float)Math.PI - num * (float)_health;
		spriteBatch.Draw(_healthBar, new Vector2(_healthBar.Width / 2, _healthBar.Height / 2), null, Color.White, rotation, new Vector2(_healthBar.Width / 2, _healthBar.Height / 2), 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
		spriteBatch.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		spriteBatch.GraphicsDevice.SetRenderTarget(_healthRT);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		_maskEffect.Parameters["TextureTwo"].SetValue(_tempRT1);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _maskEffect);
		spriteBatch.Draw(_healthBar, Vector2.Zero, Color.White);
		spriteBatch.End();
		spriteBatch.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		spriteBatch.GraphicsDevice.SetRenderTarget(_tempRT1);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin();
		num = (float)Math.PI / (float)_currentGun.Settings.MagazineSize;
		rotation = (float)Math.PI - num * (float)_currentGun.GetAmmoRemaining();
		spriteBatch.Draw(_healthBar, new Vector2(_healthBar.Width / 2, _healthBar.Height / 2), null, Color.White, rotation, new Vector2(_healthBar.Width / 2, _healthBar.Height / 2), 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
		spriteBatch.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
		spriteBatch.GraphicsDevice.SetRenderTarget(_ammoRT);
		spriteBatch.GraphicsDevice.Clear(Color.Transparent);
		_maskEffect.Parameters["TextureTwo"].SetValue(_tempRT1);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _maskEffect);
		spriteBatch.Draw(_healthBar, Vector2.Zero, Color.White);
		spriteBatch.End();
		spriteBatch.GraphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
	}

	public void MoveTowardsPoint(Vector2 position, float speed)
	{
		if (_body != null)
		{
			Vector2 vector = base.DisplayPosition - position;
			vector.Normalize();
			if (!float.IsNaN(vector.X) && !float.IsNaN(vector.Y))
			{
				_body.ApplyForce(vector * (0f - speed));
			}
		}
	}

	public void MoveTowardsPoint(Vector2 position, float speed, float modifier)
	{
		if (_body != null)
		{
			Vector2 vector = base.DisplayPosition - position;
			vector.Normalize();
			if (!float.IsNaN(vector.X) && !float.IsNaN(vector.Y))
			{
				_body.ApplyForce(modifier * vector * (0f - speed));
			}
		}
	}

	public ShooterPlayer GetClosestPlayer()
	{
		float num = 1000000f;
		ShooterPlayer result = _allPlayers[0];
		foreach (ShooterPlayer allPlayer in _allPlayers)
		{
			if (allPlayer != this)
			{
				float num2 = Vector2.Distance(base.DisplayPosition, allPlayer.DisplayPosition);
				if (num > num2)
				{
					num = num2;
					result = allPlayer;
				}
			}
		}
		return result;
	}

	public ShooterPlayer GetClosestPlayer(Type exclusionType)
	{
		float num = 1000000f;
		ShooterPlayer result = _allPlayers[0];
		foreach (ShooterPlayer allPlayer in _allPlayers)
		{
			if (allPlayer != this && (object)allPlayer.GetType() != exclusionType)
			{
				float num2 = Vector2.Distance(base.DisplayPosition, allPlayer.DisplayPosition);
				if (num > num2)
				{
					num = num2;
					result = allPlayer;
				}
			}
		}
		return result;
	}

	public Vector2 GetWaypointToDestination(Vector2 destination, out List<Vector2> path)
	{
		int closestWaypointToPosition = GetClosestWaypointToPosition(base.DisplayPosition);
		int closestWaypointToPosition2 = GetClosestWaypointToPosition(destination);
		path = _navMesh.GetPath(closestWaypointToPosition, closestWaypointToPosition2);
		if (path.Count > 1)
		{
			return path[1];
		}
		if (path.Count == 1)
		{
			return path[0];
		}
		return Vector2.One;
	}

	public virtual int GetClosestWaypointToPosition(Vector2 position)
	{
		return _navMesh.LineMesh.GetNearestNodeID(position, 10000f);
	}

	public void SetCurrentGun(Gun gun)
	{
		_currentGun = gun;
	}

	public bool IsRayCollisionFromPlayerTo(Vector2 destinationPoint)
	{
		bool foundCollision = false;
		_world.RayCast(delegate
		{
			foundCollision = true;
			return 0f;
		}, ConvertUnits.ToSimUnits(base.DisplayPosition), ConvertUnits.ToSimUnits(destinationPoint));
		return foundCollision;
	}

	public bool IsRayCollisionFromPlayerTo(Vector2 destinationPoint, out Body hitBody)
	{
		bool foundCollision = false;
		hitBody = null;
		Body returnBody = null;
		_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			foundCollision = true;
			returnBody = f.Body;
			return 0f;
		}, ConvertUnits.ToSimUnits(base.DisplayPosition), ConvertUnits.ToSimUnits(destinationPoint));
		hitBody = returnBody;
		return foundCollision;
	}

	public bool IsRayCollisionFromPlayerTo(Vector2 destinationPoint, object exclusionObject)
	{
		bool foundCollision = false;
		_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if (f.Body.UserData != exclusionObject)
			{
				foundCollision = true;
				return 0f;
			}
			return -1f;
		}, ConvertUnits.ToSimUnits(base.DisplayPosition), ConvertUnits.ToSimUnits(destinationPoint));
		return foundCollision;
	}

	public bool IsRayCollisionFromPlayerTo(Vector2 destinationPoint, object exclusionObject, out Body hitObject)
	{
		bool foundCollision = false;
		hitObject = null;
		Body hitBodyObj = null;
		_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			hitBodyObj = f.Body;
			if (f.Body.UserData != exclusionObject)
			{
				foundCollision = true;
				return 0f;
			}
			return -1f;
		}, ConvertUnits.ToSimUnits(base.DisplayPosition), ConvertUnits.ToSimUnits(destinationPoint));
		hitObject = hitBodyObj;
		return foundCollision;
	}

	public bool IsRayCollisionFromPlayerTo(Vector2 destinationPoint, List<object> exclusionObjects, out Body hitObject)
	{
		bool foundCollision = false;
		hitObject = null;
		Body hitBodyObj = null;
		_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			hitBodyObj = f.Body;
			if (!exclusionObjects.Contains(f.Body.UserData) && f.Body.UserData != this)
			{
				foundCollision = true;
				return 0f;
			}
			return -1f;
		}, ConvertUnits.ToSimUnits(base.DisplayPosition), ConvertUnits.ToSimUnits(destinationPoint));
		hitObject = hitBodyObj;
		return foundCollision;
	}

	public bool IsRayCollisionFromGunToOtherPlayer(ShooterPlayer otherPlayer)
	{
		bool foundCollision = false;
		_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if (f.Body.UserData != null)
			{
				if (f.Body.UserData != otherPlayer && f.Body.UserData != this)
				{
					foundCollision = true;
					return 0f;
				}
				return -1f;
			}
			return 0f;
		}, ConvertUnits.ToSimUnits(GetRelativePosition(_currentGun.Settings.MuzzleOffsetX, _currentGun.Settings.MuzzleOffsetY)), otherPlayer.Body.Position);
		return foundCollision;
	}

	public bool IsRayCollisionToOtherPlayer(ShooterPlayer otherPlayer)
	{
		bool foundCollision = false;
		_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			if (f.Body.UserData != null)
			{
				if (f.Body.UserData != otherPlayer)
				{
					foundCollision = true;
					return 0f;
				}
				return -1f;
			}
			return 0f;
		}, _body.Position, otherPlayer.Body.Position);
		return foundCollision;
	}

	public bool IsRayCollisionFromPlayerGunMuzzleTo(Vector2 destinationPoint, object exclusionObject)
	{
		bool foundCollision = false;
		_world.RayCast(delegate(Fixture f, Vector2 p, Vector2 n, float fr)
		{
			_ = f.Body.UserData;
			foundCollision = true;
			return 0f;
		}, ConvertUnits.ToSimUnits(GetRelativePosition(_currentGun.Settings.MuzzleOffsetX, _currentGun.Settings.MuzzleOffsetY)), ConvertUnits.ToSimUnits(destinationPoint));
		return foundCollision;
	}

	public Gun GetCurrentGun()
	{
		return _currentGun;
	}

	public void OnKilledOpponent()
	{
		if (_gunIndex < _guns.Count - 1)
		{
			_gunIndex++;
			_currentGun = new Gun(this, _world, _contentManager, _guns[_gunIndex]);
			_hasChangedToNewGun = true;
			_isDrawingGunChange = true;
		}
		else
		{
			_hasWon = true;
		}
	}

	public virtual void OnTakeDamage(ShooterPlayer player, int damage)
	{
		if (_health > 0)
		{
			_health -= damage;
			_damageByPlayers[player.GetID()] += damage;
			if (_health <= 0)
			{
				_isAlive = false;
				OnDeath();
			}
			_lastDirectionOfDamage = base.DisplayPosition - player.DisplayPosition;
			_lastDirectionOfDamage.Normalize();
		}
	}

	public virtual void OnHealthPickedUp()
	{
		_health = 100;
		ShooterGame.PlayCue("Pickup");
	}

	public virtual void OnAmmoPickedUp()
	{
		_currentGun.Reload();
		ShooterGame.PlayCue("Pickup");
	}

	public Vector2 GetFreeSpawnPosition()
	{
		if (_navMesh.LineMesh.SpecialNodes.Count > 0)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < _navMesh.LineMesh.SpecialNodes.Count; i++)
			{
				list.Add(i);
			}
			int index = 0;
			float num = -2.1474836E+09f;
			foreach (int item in list)
			{
				float num2 = 2.1474836E+09f;
				foreach (ShooterPlayer allPlayer in _allPlayers)
				{
					if (allPlayer != this && allPlayer.IsAlive)
					{
						float num3 = Vector2.Distance(_navMesh.LineMesh.MeshNodes[_navMesh.LineMesh.SpecialNodes[item]]._position, allPlayer.DisplayPosition);
						if (num3 < num2)
						{
							num2 = num3;
						}
					}
				}
				if (num2 > num)
				{
					index = item;
					num = num2;
				}
			}
			return _navMesh.LineMesh.MeshNodes[_navMesh.LineMesh.SpecialNodes[index]]._position;
		}
		return Vector2.Zero;
	}

	public virtual void OnRespawn()
	{
		CreateBody(GetFreeSpawnPosition());
		_currentGun = new Gun(this, _world, _contentManager, _guns[_gunIndex]);
		_health = 100;
		_isAlive = true;
	}

	public void Reset()
	{
		_gunIndex = 0;
		_hasWon = false;
		DestroyBody();
		OnRespawn();
		_body.Position = new Vector2(640f, 480f);
		_isAlive = false;
		for (int i = 0; i < _allPlayers.Count; i++)
		{
			_allPlayers[i].ResetDamageByPlayer(GetID());
		}
	}

	public void OnDeath()
	{
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[3], _particleParamsBlood), _random.Next(1, 100), BlendState.AlphaBlend, base.DisplayPosition, 1000, 6, 100));
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[0], _particleParams), _random.Next(1, 100), BlendState.AlphaBlend, base.DisplayPosition, 1000, 4, 100));
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[1], _particleParams), _random.Next(1, 100), BlendState.AlphaBlend, base.DisplayPosition, 1000, 8, 100));
		ParticleEngine.AddEmitter(new ParticleEmitter(new RuntimeParticleDescriptor(_deathTextures[2], _particleParams), _random.Next(1, 100), BlendState.AlphaBlend, base.DisplayPosition, 1000, 4, 100));
		if (_body != null && !_body.IsDisposed)
		{
			_body.Dispose();
		}
		for (int i = 0; i < _allPlayers.Count; i++)
		{
			_allPlayers[i].ResetDamageByPlayer(GetID());
		}
	}

	public void OnFireGun()
	{
	}

	public int GetHealth()
	{
		return _health;
	}

	public int GetID()
	{
		return _id;
	}

	public Vector2 GetDisplayPosition()
	{
		return base.DisplayPosition;
	}

	public void ResetDamageByPlayer(int playerID)
	{
		_damageByPlayers[playerID] = 0;
	}

	public List<ShooterPlayer> GetAllPlayers()
	{
		return _allPlayers;
	}
}
