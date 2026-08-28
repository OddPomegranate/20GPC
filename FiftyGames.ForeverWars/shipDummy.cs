using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class shipDummy
{
	public RenderTarget2D shipImage;

	public shipModule shipModule;

	private GraphicsDevice graphicsDevice;

	public shipDummy(shipModule inModule, GraphicsDevice inGraphicsDevice)
	{
		shipModule = inModule;
		shipImage = new RenderTarget2D(inGraphicsDevice, 440, 440);
		graphicsDevice = inGraphicsDevice;
	}

	public void initaliseOnce()
	{
		playerShip[] playerList = new playerShip[1];
		shipModule.Update(playerList, new Vector2(220f, 220f), 0f, new List<eBullet>(), permittedToFire: false, new List<pBullet>());
		SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
		graphicsDevice.SetRenderTarget(shipImage);
		graphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		shipModule.Draw(spriteBatch, new Vector2(0f, 0f));
		spriteBatch.End();
	}

	public Texture2D getImage()
	{
		return shipImage;
	}

	public void Dispose()
	{
		shipImage.Dispose();
	}
}
