using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace FiftyGames.Zombie.Utils;

internal class ZombieContentManager(IServiceProvider serviceProvider) : ContentManager(serviceProvider)
{
	private List<string> _loadCalls = new List<string>();

	public override T Load<T>(string assetName)
	{
		_loadCalls.Add(assetName);
		return base.Load<T>(assetName);
	}
}
