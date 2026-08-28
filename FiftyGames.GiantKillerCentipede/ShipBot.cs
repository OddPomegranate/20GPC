using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FiftyGames.GiantKillerCentipede;

internal class ShipBot : Ship
{
	private const float ShootDistance = 20f;

	private List<Mushroom> _mushrooms;

	private List<Centipede> _centipedes;

	private List<Powerup> _powerups;

	private Mushroom _closestShroom;

	private float _shroomDist;

	private BodySegment _closestBody;

	private float _bodyDist;

	private Powerup _closestPowerup;

	private float _powerupDist;

	private float _variation;

	private float _variationMagnitude;

	private int _variationTimer;

	private int _variationFrequency;

	public ShipBot(Vector2 position, Random rng, ref List<Mushroom> mushrooms, ref List<Centipede> centipedes, ref List<Powerup> powerups)
		: base(null, position, rng)
	{
		_mushrooms = mushrooms;
		_centipedes = centipedes;
		_powerups = powerups;
		if (_centipedes.Count > 3)
		{
			_shield = 2;
		}
		if (_centipedes.Count > 7)
		{
			_shield = 3;
		}
		_closestShroom = null;
		_closestBody = null;
		_closestPowerup = null;
		_variationMagnitude = (float)_ranGen.NextDouble() * 100f;
		_variationFrequency = 100 + _ranGen.Next(900);
		_centipedeCandidate = false;
	}

	public override void Update(GameTime gameTime, List<Projectile> projectiles, bool gameOver)
	{
		if (!gameOver)
		{
			if (_powerWeapon == Powerup.PowerupType.None)
			{
				_closestBody = null;
				_powerupDist = 2.1474836E+09f;
				foreach (Powerup powerup in _powerups)
				{
					float num = (powerup.Position - Position).LengthSquared();
					if (num < _powerupDist)
					{
						_closestPowerup = powerup;
						_powerupDist = num;
					}
				}
				if (_closestShroom == null || _closestShroom.Health == 0)
				{
					_closestShroom = null;
					_shroomDist = 2.1474836E+09f;
					foreach (Mushroom mushroom in _mushrooms)
					{
						float num2 = (mushroom.Position - Position).LengthSquared();
						if (num2 < _shroomDist)
						{
							_closestShroom = mushroom;
							_shroomDist = num2;
						}
					}
				}
			}
			else
			{
				_closestShroom = null;
				_closestPowerup = null;
				if (_ranGen.Next(1000) == 0)
				{
					int index = _ranGen.Next(_centipedes.Count);
					if (_centipedes[index].Body.Count != 0)
					{
						_closestBody = _centipedes[index].Body[_ranGen.Next(_centipedes[index].Body.Count)];
					}
				}
				else if (_ranGen.Next(1000) == 0 || _closestBody == null)
				{
					_bodyDist = 2.1474836E+09f;
					foreach (Centipede centipede in _centipedes)
					{
						foreach (BodySegment item in centipede.Body)
						{
							float num3 = (item.Position - Position).LengthSquared();
							if (num3 < _bodyDist)
							{
								_closestBody = item;
								_bodyDist = num3;
							}
						}
					}
				}
			}
			if (_centipedes.Count == 0)
			{
				_closestBody = null;
			}
			if (_powerups.Count == 0)
			{
				_closestPowerup = null;
			}
			if (_mushrooms.Count == 0)
			{
				_closestShroom = null;
			}
			Vector2 position = _position;
			if (_closestBody != null)
			{
				position = _closestBody.Position;
			}
			else if (_closestPowerup != null)
			{
				position = _closestPowerup.Position;
			}
			else if (_closestShroom != null)
			{
				position = _closestShroom.Position;
			}
			if (_variationTimer > _variationFrequency)
			{
				_variationTimer = 0;
				_variation = ((float)_ranGen.NextDouble() - 0.5f) * _variationMagnitude;
			}
			_variationTimer += gameTime.ElapsedGameTime.Milliseconds;
			position.X += _variation;
			float num4 = 0f;
			if (position.X < _position.X - 20f)
			{
				num4 = Math.Min((position.X - _position.X) * 0.5f, 1f);
			}
			else if (position.X > _position.X + 20f)
			{
				num4 = Math.Max((position.X - _position.X) * 0.5f, -1f);
			}
			_velocity += new Vector2(num4 * 4f, 0f);
			if (Math.Abs(position.X - _position.X) < 20f)
			{
				if (_bulletShotTimer == 0)
				{
					FireBullet(projectiles);
				}
				if (_specialShotTimer == 0 && _powerWeapon != Powerup.PowerupType.None)
				{
					FireSpecial(projectiles);
					if (_powerAmmo == 0)
					{
						_powerWeapon = Powerup.PowerupType.None;
					}
				}
			}
		}
		base.Update(gameTime, projectiles, gameOver);
	}
}
