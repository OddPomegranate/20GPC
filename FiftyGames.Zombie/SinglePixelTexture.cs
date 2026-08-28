using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal class SinglePixelTexture : Texture2D
{
	public SinglePixelTexture(GraphicsDevice graphicsDevice)
		: base(graphicsDevice, 1, 1)
	{
		SetData(new Color[1]
		{
			new Color(255, 255, 255, 255)
		});
	}
}
