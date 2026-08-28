using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal static class ProjectileManager
{
	private static Queue<Projectile> _physProjectiles;

	public static int Count => _physProjectiles.Count;

	private static void CheckForNullProjectileList()
	{
		if (_physProjectiles == null)
		{
			_physProjectiles = new Queue<Projectile>();
		}
	}

	public static void AddProjectile(Projectile newProjectile)
	{
		CheckForNullProjectileList();
		_physProjectiles.Enqueue(newProjectile);
	}

	public static void Update(GameTime gameTime)
	{
		CheckForNullProjectileList();
		for (int i = 0; i < _physProjectiles.Count; i++)
		{
			if (_physProjectiles.ElementAt(i).IsAlive)
			{
				_physProjectiles.ElementAt(i).Update(gameTime);
			}
		}
		if (_physProjectiles.Count > 0 && !_physProjectiles.ElementAt(0).IsAlive)
		{
			_physProjectiles.ElementAt(0).Dispose();
			_physProjectiles.Dequeue();
		}
	}

	public static void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		CheckForNullProjectileList();
		for (int i = 0; i < _physProjectiles.Count; i++)
		{
			if (_physProjectiles.ElementAt(i).IsAlive)
			{
				_physProjectiles.ElementAt(i).Draw(spriteBatch, offset);
			}
		}
	}
}
