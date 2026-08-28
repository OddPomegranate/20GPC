using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ISParticleEngine;

public static class ParticleEngine
{
	private static List<IEmitter> _emitters;

	public static void InitEngine()
	{
		_emitters = new List<IEmitter>();
	}

	public static void AddEmitter(IEmitter emitter)
	{
		_emitters.Add(emitter);
	}

	public static void CleanEmitterList()
	{
		for (int i = 0; i < _emitters.Count; i++)
		{
			if (_emitters[i].HasFinishedEmitting())
			{
				_emitters.RemoveAt(i);
				i--;
			}
		}
	}

	public static void DestroyAllEmitters()
	{
		for (int i = 0; i < _emitters.Count; i++)
		{
			_emitters[i].Dispose();
		}
		_emitters.Clear();
	}

	public static int GetEmitterCount()
	{
		return _emitters.Count;
	}

	public static void Update()
	{
		for (int i = 0; i < _emitters.Count; i++)
		{
			_emitters[i].Update();
		}
	}

	public static void Draw(SpriteBatch spriteBatch, Vector2 offset, Vector2 forceScale)
	{
		for (int i = 0; i < _emitters.Count; i++)
		{
			_emitters[i].Draw(spriteBatch, offset, forceScale);
		}
	}
}
