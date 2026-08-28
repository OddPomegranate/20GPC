using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.DynamicLights;

internal class DynamicLightMaskManager
{
	private List<DynamicLightMask> _masks;

	public DynamicLightMaskManager()
	{
		_masks = new List<DynamicLightMask>();
	}

	public void Add(DynamicLightMask dlm)
	{
		_masks.Add(dlm);
	}

	public void Update(GameTime gameTime)
	{
		for (int i = 0; i < _masks.Count; i++)
		{
			_masks[i].Update(gameTime);
		}
		for (int j = 0; j < _masks.Count; j++)
		{
			if (_masks[j].ReadyForRemoval)
			{
				_masks.RemoveAt(j);
				j--;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		for (int i = 0; i < _masks.Count; i++)
		{
			_masks[i].Draw(spriteBatch, offset);
		}
	}
}
