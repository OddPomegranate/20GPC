using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Guns;

namespace Shooter;

internal static class ProjectileManager
{
	private static List<Shot> _shots = new List<Shot>();

	public static void AddShot(Shot shot)
	{
		_shots.Add(shot);
	}

	public static void Update(GameTime gameTime)
	{
		foreach (Shot shot in _shots)
		{
			shot.Update(gameTime);
		}
		for (int i = 0; i < _shots.Count; i++)
		{
			if (_shots[i].IsDead)
			{
				_shots.RemoveAt(i);
				i--;
			}
		}
	}

	public static void DeleteAllShots()
	{
		_shots.Clear();
	}

	public static void Draw(SpriteBatch spriteBatch)
	{
		foreach (Shot shot in _shots)
		{
			shot.Draw(spriteBatch);
		}
	}
}
