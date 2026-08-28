using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FiftyGames.GiantKillerCentipede;

internal class CentipedeBot : Centipede
{
	private List<Mushroom> _mushrooms;

	private List<Ship> _ships;

	private Mushroom _closestShroom;

	private float _shroomDist;

	private Ship _closestShip;

	private float _shipDist;

	private int _eatSpeed;

	private int _attemptTimer;

	private int _eatingTime;

	private int _killingTime;

	private Random _ranGen;

	public int EatSpeed
	{
		get
		{
			return _eatSpeed;
		}
		set
		{
			_eatSpeed = value;
		}
	}

	public CentipedeBot(Vector2 headPosition, int length, ref List<Mushroom> mushrooms, ref List<Ship> ships, Random rng)
		: base(null, headPosition, length)
	{
		_mushrooms = mushrooms;
		_ships = ships;
		_closestShroom = null;
		_closestShip = null;
		_ranGen = rng;
		_eatingTime = 4000 + _ranGen.Next(11000);
		_killingTime = (_eatingTime = 4000 + _ranGen.Next(21000));
		if (_ranGen.Next(2) == 0)
		{
			_attemptTimer = _eatingTime;
		}
		else
		{
			_attemptTimer = _killingTime;
		}
		_eatSpeed = 80 + _ranGen.Next(120);
	}

	public CentipedeBot(List<BodySegment> existingBody, ref List<Mushroom> mushrooms, ref List<Ship> ships, Random rng)
		: base(null, existingBody)
	{
		_mushrooms = mushrooms;
		_ships = ships;
		_closestShroom = null;
		_closestShip = null;
		_ranGen = rng;
		_eatingTime = 4000 + _ranGen.Next(11000);
		_killingTime = (_eatingTime = 4000 + _ranGen.Next(21000));
		if (_ranGen.Next(2) == 0)
		{
			_attemptTimer = _eatingTime;
		}
		else
		{
			_attemptTimer = _killingTime;
		}
		_eatSpeed = 80 + _ranGen.Next(320);
	}

	public override void Update(GameTime gameTime, bool gameOver)
	{
		if (!gameOver)
		{
			if ((_attemptTimer >= _killingTime && _closestShroom == null) || (_closestShroom != null && _closestShroom.Health == 0))
			{
				if (_closestShroom == null || _closestShroom.Health == 0)
				{
					if (_closestShroom == null)
					{
						_attemptTimer = 0;
					}
					_closestShip = null;
					_shroomDist = 2.1474836E+09f;
					foreach (Mushroom mushroom in _mushrooms)
					{
						float num = (mushroom.Position - _body[0].Position).LengthSquared();
						if (num < _shroomDist)
						{
							_closestShroom = mushroom;
							_shroomDist = num;
						}
					}
				}
			}
			else if ((_attemptTimer >= _eatingTime && _closestShip == null) || (_closestShip != null && !_closestShip.IsAlive))
			{
				_attemptTimer = 0;
				_closestShroom = null;
				_shipDist = 2.1474836E+09f;
				foreach (Ship ship in _ships)
				{
					float num2 = (ship.Position - _body[0].Position).LengthSquared();
					if (num2 < _shipDist)
					{
						_closestShip = ship;
						_shipDist = num2;
					}
				}
			}
			_eating = false;
			if (_closestShip != null)
			{
				if (!_closestShip.Invunerable)
				{
					_playerInfluence = Vector2.Normalize(_closestShip.Position - _body[0].Position);
				}
				else
				{
					_playerInfluence = new Vector2(0f, 1f);
				}
				_attemptTimer += gameTime.ElapsedGameTime.Milliseconds;
			}
			else if (_closestShroom != null)
			{
				float num3 = (_closestShroom.Position - _body[0].Position).Length();
				if (num3 <= _closestShroom.CollisionVolume.Radius + _body[0].CollisionVolume.Radius + 0.01f)
				{
					_playerInfluence = Vector2.Zero;
					if (_eatingTimer > _eatSpeed)
					{
						_eating = true;
						_eatingTimer = 0;
					}
					else
					{
						_eatingTimer += gameTime.ElapsedGameTime.Milliseconds;
					}
				}
				else
				{
					_playerInfluence = Vector2.Normalize(_closestShroom.Position - _body[0].Position);
				}
				_attemptTimer += gameTime.ElapsedGameTime.Milliseconds;
			}
			_playerInfluence *= 2f;
		}
		base.Update(gameTime, gameOver);
	}
}
