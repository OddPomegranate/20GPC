using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ISParticleEngine;

public class EmitterContainer : IEmitter, IDisposable
{
	private List<ParticleEmitter> _emitters;

	private List<KevEmitterInfo> _emitterInfo;

	private int _tickCount;

	public EmitterContainer()
	{
		_emitters = new List<ParticleEmitter>();
		_emitterInfo = new List<KevEmitterInfo>();
		_tickCount = 0;
	}

	public void AddEmitter(ParticleEmitter emitter, int startTick, int tickDuration)
	{
		KevEmitterInfo item = default(KevEmitterInfo);
		item._startTick = startTick;
		item._tickDuration = tickDuration;
		item._currentTick = 0;
		item._active = false;
		item._finished = false;
		_emitterInfo.Add(item);
		_emitters.Add(emitter);
	}

	public void Update()
	{
		if (!HasFinishedEmitting())
		{
			for (int i = 0; i < _emitterInfo.Count; i++)
			{
				if (_emitterInfo[i]._startTick <= _tickCount && !_emitterInfo[i]._active && !_emitterInfo[i]._finished)
				{
					KevEmitterInfo value = _emitterInfo[i];
					value._active = true;
					_emitterInfo[i] = value;
				}
				else if (_emitterInfo[i]._active)
				{
					if (_emitterInfo[i]._currentTick > _emitterInfo[i]._tickDuration)
					{
						KevEmitterInfo value2 = _emitterInfo[i];
						value2._active = false;
						value2._finished = true;
						_emitterInfo[i] = value2;
					}
					else
					{
						_emitters[i].Update();
						KevEmitterInfo value3 = _emitterInfo[i];
						value3._currentTick++;
						_emitterInfo[i] = value3;
					}
				}
			}
			_tickCount++;
		}
		else
		{
			Dispose();
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset, Vector2 forceScale)
	{
		for (int i = 0; i < _emitterInfo.Count; i++)
		{
			if (_emitterInfo[i]._active)
			{
				_emitters[i].Draw(spriteBatch, offset, forceScale);
			}
		}
	}

	public bool HasFinishedEmitting()
	{
		for (int i = 0; i < _emitterInfo.Count; i++)
		{
			if (!_emitterInfo[i]._finished)
			{
				return false;
			}
		}
		return true;
	}

	public void Dispose()
	{
		_emitters.Clear();
		_emitterInfo.Clear();
		ParticleEngine.CleanEmitterList();
	}
}
