using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ISParticleEngine;

public interface IEmitter : IDisposable
{
	void Update();

	void Draw(SpriteBatch spriteBatch, Vector2 offset, Vector2 forceScale);

	bool HasFinishedEmitting();
}
