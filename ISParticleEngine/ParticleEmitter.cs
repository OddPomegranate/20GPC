using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ISParticleEngine;

public class ParticleEmitter : IEmitter, IDisposable
{
	private List<Particle> _particles;

	private ParticleDescriptor _particleDescriptor;

	private BlendState _blendState;

	private Random _random;

	private Vector2 _position;

	private int _tickInterval;

	private int _tickCount;

	private int _emitCount;

	private int _amountOnTick;

	private int _tickDuration;

	private bool _hasFinished;

	private bool _firstRun;

	private bool _allowMoreParticles;

	public ParticleEmitter(ParticleDescriptor particleDescriptor, int seed, BlendState blendState, Vector2 position, int tickInterval, int amountOntick)
	{
		_particleDescriptor = particleDescriptor;
		_tickCount = 0;
		_emitCount = 0;
		_tickInterval = tickInterval;
		_amountOnTick = amountOntick;
		_position = position;
		_tickDuration = -1;
		_particles = new List<Particle>();
		_random = new Random(seed);
		_hasFinished = false;
		_firstRun = true;
		_blendState = blendState;
		_allowMoreParticles = true;
	}

	public ParticleEmitter(ParticleDescriptor particleDescriptor, int seed, BlendState blendState, Vector2 position, int tickInterval, int amountOntick, int tickDuration)
	{
		_particleDescriptor = particleDescriptor;
		_tickCount = 0;
		_emitCount = 0;
		_tickInterval = tickInterval;
		_amountOnTick = amountOntick;
		_position = position;
		_tickDuration = tickDuration;
		_particles = new List<Particle>();
		_random = new Random(seed);
		_hasFinished = false;
		_firstRun = true;
		_allowMoreParticles = true;
		_blendState = blendState;
	}

	public void Update()
	{
		if (_tickDuration != -1 && _emitCount > _tickDuration)
		{
			Dispose();
		}
		if (_firstRun)
		{
			for (int i = 0; i < _amountOnTick; i++)
			{
				Particle particle = new Particle();
				double num = MathHelper.Lerp(_particleDescriptor.GetMinDirection(), _particleDescriptor.GetMaxDirection(), (float)_random.NextDouble());
				particle.Position = _position;
				particle.Velocity = new Vector2((float)Math.Sin(num), (float)Math.Cos(num)) * MathHelper.Lerp(_particleDescriptor.GetMinSpeed(), _particleDescriptor.GetMaxSpeed(), (float)_random.NextDouble());
				particle.Scale = Vector2.Lerp(_particleDescriptor.GetMinScale(), _particleDescriptor.GetMaxScale(), (float)_random.NextDouble());
				particle.DeltaScale1 = Vector2.Lerp(_particleDescriptor.GetMinScaleChange1(), _particleDescriptor.GetMaxScaleChange1(), (float)_random.NextDouble());
				particle.DeltaScale2 = Vector2.Lerp(_particleDescriptor.GetMinScaleChange2(), _particleDescriptor.GetMaxScaleChange2(), (float)_random.NextDouble());
				particle.Color = Vector3.Lerp(_particleDescriptor.GetMinColor(), _particleDescriptor.GetMaxColor(), (float)_random.NextDouble());
				particle.DeltaColor1 = Vector3.Lerp(_particleDescriptor.GetMinColorChange1(), _particleDescriptor.GetMaxColorChange1(), (float)_random.NextDouble());
				particle.DeltaColor2 = Vector3.Lerp(_particleDescriptor.GetMinColorChange2(), _particleDescriptor.GetMaxColorChange2(), (float)_random.NextDouble());
				particle.Rotation = 0f;
				particle.DeltaRotation = MathHelper.Lerp(_particleDescriptor.GetMinRotation(), _particleDescriptor.GetMaxRotation(), (float)_random.NextDouble());
				particle.Alpha = MathHelper.Lerp(_particleDescriptor.GetMinAlpha(), _particleDescriptor.GetMaxAlpha(), (float)_random.NextDouble());
				particle.DeltaAlpha1 = MathHelper.Lerp(_particleDescriptor.GetMinAlphaChange1(), _particleDescriptor.GetMaxAlphaChange1(), (float)_random.NextDouble());
				particle.DeltaAlpha2 = MathHelper.Lerp(_particleDescriptor.GetMinAlphaChange2(), _particleDescriptor.GetMaxAlphaChange2(), (float)_random.NextDouble());
				_particles.Add(particle);
			}
			_firstRun = false;
		}
		if (_tickCount > _emitCount * _tickInterval + _tickInterval && _allowMoreParticles)
		{
			for (int j = 0; j < _amountOnTick; j++)
			{
				Particle particle2 = new Particle();
				double num2 = MathHelper.Lerp(_particleDescriptor.GetMinDirection(), _particleDescriptor.GetMaxDirection(), (float)_random.NextDouble());
				particle2.Position = _position;
				particle2.Velocity = new Vector2((float)Math.Sin(num2), (float)Math.Cos(num2)) * MathHelper.Lerp(_particleDescriptor.GetMinSpeed(), _particleDescriptor.GetMaxSpeed(), (float)_random.NextDouble());
				particle2.Scale = Vector2.Lerp(_particleDescriptor.GetMinScale(), _particleDescriptor.GetMaxScale(), (float)_random.NextDouble());
				particle2.DeltaScale1 = Vector2.Lerp(_particleDescriptor.GetMinScaleChange1(), _particleDescriptor.GetMaxScaleChange1(), (float)_random.NextDouble());
				particle2.DeltaScale2 = Vector2.Lerp(_particleDescriptor.GetMinScaleChange2(), _particleDescriptor.GetMaxScaleChange2(), (float)_random.NextDouble());
				particle2.Color = Vector3.Lerp(_particleDescriptor.GetMinColor(), _particleDescriptor.GetMaxColor(), (float)_random.NextDouble());
				particle2.DeltaColor1 = Vector3.Lerp(_particleDescriptor.GetMinColorChange1(), _particleDescriptor.GetMaxColorChange1(), (float)_random.NextDouble());
				particle2.DeltaColor2 = Vector3.Lerp(_particleDescriptor.GetMinColorChange2(), _particleDescriptor.GetMaxColorChange2(), (float)_random.NextDouble());
				particle2.Rotation = 0f;
				particle2.DeltaRotation = MathHelper.Lerp(_particleDescriptor.GetMinRotation(), _particleDescriptor.GetMaxRotation(), (float)_random.NextDouble());
				particle2.Alpha = MathHelper.Lerp(_particleDescriptor.GetMinAlpha(), _particleDescriptor.GetMaxAlpha(), (float)_random.NextDouble());
				particle2.DeltaAlpha1 = MathHelper.Lerp(_particleDescriptor.GetMinAlphaChange1(), _particleDescriptor.GetMaxAlphaChange1(), (float)_random.NextDouble());
				particle2.DeltaAlpha2 = MathHelper.Lerp(_particleDescriptor.GetMinAlphaChange2(), _particleDescriptor.GetMaxAlphaChange2(), (float)_random.NextDouble());
				_particles.Add(particle2);
			}
			_emitCount++;
		}
		for (int k = 0; k < _particles.Count; k++)
		{
			if (_particles[k].Life != _particleDescriptor.GetChange())
			{
				_particles[k].Alpha += _particles[k].DeltaAlpha1;
				_particles[k].Color += _particles[k].DeltaColor1;
				_particles[k].Life++;
				_particles[k].Position += _particles[k].Velocity;
				if (_particleDescriptor.GetIsDirectional())
				{
					_particles[k].Rotation = (float)Math.Atan2(_particles[k].Velocity.Y, _particles[k].Velocity.X);
				}
				else
				{
					_particles[k].Rotation += _particles[k].DeltaRotation;
				}
				_particles[k].Scale += _particles[k].DeltaScale1;
				_particles[k].Velocity += _particleDescriptor.GetGravity();
				_particles[k].Velocity *= _particleDescriptor.GetMaxMultiplicative();
			}
			else
			{
				_particles[k].Alpha += _particles[k].DeltaAlpha2;
				_particles[k].Color += _particles[k].DeltaColor2;
				_particles[k].Position += _particles[k].Velocity;
				if (_particleDescriptor.GetIsDirectional())
				{
					_particles[k].Rotation = (float)Math.Atan2(_particles[k].Velocity.Y, _particles[k].Velocity.X);
				}
				else
				{
					_particles[k].Rotation += _particles[k].DeltaRotation;
				}
				_particles[k].Scale += _particles[k].DeltaScale2;
				_particles[k].Velocity += _particleDescriptor.GetGravity();
				_particles[k].Velocity *= _particleDescriptor.GetMaxMultiplicative();
			}
			if (_particles[k].Alpha < 0f)
			{
				_particles.RemoveAt(k);
				if (_particles.Count == 0)
				{
					Dispose();
				}
			}
		}
		_tickCount++;
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset, Vector2 forceScale)
	{
		spriteBatch.Begin(SpriteSortMode.Immediate, _blendState);
		foreach (Particle particle in _particles)
		{
			Color color = new Color(particle.Color) * particle.Alpha;
			spriteBatch.Draw(_particleDescriptor.Sprite, particle.Position + offset, null, color, particle.Rotation, _particleDescriptor.GetOrigin(), particle.Scale * forceScale, SpriteEffects.None, 0f);
		}
		spriteBatch.End();
	}

	public void ForceStopFutureParticles()
	{
		_allowMoreParticles = false;
	}

	public bool HasFinishedEmitting()
	{
		return _hasFinished;
	}

	public void SetPosition(Vector2 position)
	{
		_position = position;
	}

	public void Dispose()
	{
		_particles.Clear();
		_hasFinished = true;
		ParticleEngine.CleanEmitterList();
	}
}
