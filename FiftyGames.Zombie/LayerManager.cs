using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie;

internal static class LayerManager
{
	private const int _numberOfLayers = 10;

	private static float[] _layerSpeeds = new float[10];

	private static Texture2D[] _layerTextures = new Texture2D[10];

	public static void AddTexture(Texture2D texture, int layerIndex)
	{
		_layerTextures[layerIndex] = texture;
	}

	public static void AddLayerDescriptor(int layerIndex, float speed)
	{
		_layerSpeeds[layerIndex] = speed;
	}

	public static void DrawLayers(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin();
		spriteBatch.End();
	}
}
