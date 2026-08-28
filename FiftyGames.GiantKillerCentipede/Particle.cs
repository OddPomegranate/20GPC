using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.GiantKillerCentipede;

internal class Particle : PhysicsObject
{
	protected Vector2 _startPosition;

	protected float _maxDistance;

	protected float _maxVelocity;

	protected float _minVelocity;

	protected Color _startColour;

	protected Color _endColour;

	protected bool _used;

	protected int _life;

	protected int _lifeSpan;

	public bool IsUsed => _used;

	public Particle(Random rng, Vector2 position, int lifeSpan, float maxDistnace, float minVelocity, float maxVelocity, Color startColour, Color endColour)
	{
		_used = false;
		_position = (_startPosition = position);
		_life = 0;
		_lifeSpan = lifeSpan;
		_maxDistance = maxDistnace;
		_minVelocity = minVelocity;
		_maxVelocity = maxVelocity;
		_startColour = startColour;
		_endColour = endColour;
		_velocity = new Vector2((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f);
		_velocity *= _minVelocity + (float)rng.NextDouble() * _maxVelocity;
	}

	public override void Load(ContentManager contentLoader)
	{
		_sprite = contentLoader.Load<Texture2D>("GiantKillerCentipede\\Image\\ParticleSmall");
		base.Load(contentLoader);
	}

	public override void Update(GameTime gameTime)
	{
		if (!_used)
		{
			float num = (_position - _startPosition).Length();
			float num2 = 0f;
			float num3 = 0f;
			if (_maxDistance != 0f)
			{
				num3 = num / _maxDistance;
			}
			if ((float)_lifeSpan != 0f)
			{
				num2 = (float)_life / (float)_lifeSpan;
			}
			float num4 = ((num2 > num3) ? num2 : num3);
			_colour = Color.Lerp(_startColour, _endColour, num4);
			base.Update(gameTime);
			if (num4 >= 1f)
			{
				_used = true;
			}
			_life += gameTime.ElapsedGameTime.Milliseconds;
		}
	}
}
