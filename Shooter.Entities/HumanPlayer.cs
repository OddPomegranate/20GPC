using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FiftyGames;
using FiftyGames.ShooterGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shooter.Guns;
using Shooter.ISHelpers;

namespace Shooter.Entities;

internal class HumanPlayer : ShooterPlayer
{
	private GamePadState previousGamePadState;

	private List<VertexPositionColor> _lineVerts;

	private Player _frameworkPlayer;

	public HumanPlayer(int id, Player frameworkPlayer, World world, Random random, ContentManager contentManager, NavMesh navMesh, List<ShooterPlayer> allPlayers, List<GunSettings> gunSettings, RenderTarget2D ammoHealthRT)
		: base(id, world, random, contentManager, navMesh, allPlayers, gunSettings, ammoHealthRT)
	{
		_lineVerts = new List<VertexPositionColor>();
		_frameworkPlayer = frameworkPlayer;
		_color = _frameworkPlayer.Colour(0.5f, 0.7f);
	}

	public override void Update(GameTime gameTime)
	{
		if (_isAlive)
		{
			GamePadState gamePadStateCurrent = _frameworkPlayer.GamePadManager.GamePadStateCurrent;
			Vector2 v = new Vector2(gamePadStateCurrent.ThumbSticks.Right.Y, gamePadStateCurrent.ThumbSticks.Right.X);
			Vector2 vector = new Vector2(gamePadStateCurrent.ThumbSticks.Left.X, gamePadStateCurrent.ThumbSticks.Left.Y * -1f);
			float num = 50f;
			Vector2 vector2 = vector * num;
			vector2 = (vector2 - _body.LinearVelocity) * _body.Mass;
			_body.ApplyLinearImpulse(ConvertUnits.ToSimUnits(vector2));
			if (v.Length() > 0.5f)
			{
				float num2 = GeometryHelper.V2ToAngle(v) - (float)Math.PI / 2f;
				float num3 = base.Body.Rotation - MathHelper.ToRadians(_currentGun.Settings.SpreadDegrees / 2f);
				float num4 = base.Body.Rotation + MathHelper.ToRadians(_currentGun.Settings.SpreadDegrees / 2f);
				if (num2 < num3 || num2 > num4)
				{
					float rotation = GeometryHelper.TurnToFace(base.Body.Position, base.Body.Position + GeometryHelper.AngleToV2(num2, 1f), base.Body.Rotation, 0.5f);
					base.Body.Rotation = rotation;
					_lastLookAngle = base.Body.Rotation;
				}
			}
			if (gamePadStateCurrent.IsButtonUp(Buttons.RightTrigger))
			{
				_hasChangedToNewGun = false;
			}
			if (((gamePadStateCurrent.IsButtonDown(Buttons.RightTrigger) && _currentGun.Settings.IsAutomatic) || (!_currentGun.Settings.IsAutomatic && previousGamePadState.IsButtonUp(Buttons.RightTrigger) && gamePadStateCurrent.IsButtonDown(Buttons.RightTrigger))) && !_hasChangedToNewGun)
			{
				_lastShotPath = _currentGun.Settings.SoundEffectPath;
				if (_currentGun.Shoot(GeometryHelper.AngleToV2(base.Body.Rotation, 1f), _random, this))
				{
					if (_lastShotPath == "Laser")
					{
						if (!_hasJustShot)
						{
							_lastShotCuePlayed = ShooterGame.PlayCue("Shoot " + _lastShotPath);
						}
					}
					else
					{
						_lastShotCuePlayed = ShooterGame.PlayCue("Shoot " + _lastShotPath);
					}
					_frameworkPlayer.GamePadManager.StartVibration(1, 0.4f);
					_hasJustShot = true;
				}
			}
			else if (_lastShotPath == "Laser" && _hasJustShot)
			{
				_lastShotCuePlayed.Stop(AudioStopOptions.AsAuthored);
				ShooterGame.PlayCue("End Laser");
				_hasJustShot = false;
			}
			previousGamePadState = gamePadStateCurrent;
		}
		base.Update(gameTime);
	}

	public override void OnTakeDamage(ShooterPlayer player, int damage)
	{
		_frameworkPlayer.GamePadManager.StartVibration(1, 1f);
		base.OnTakeDamage(player, damage);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
	}

	private Player GetFrameworkPlayer()
	{
		return _frameworkPlayer;
	}
}
