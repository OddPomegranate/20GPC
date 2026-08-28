using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.Zombie.Rendering_Helpers;

internal class WaveInfoDrawer
{
	private SpriteFont _font;

	private RenderTarget2D _renderTarget;

	private GraphicsDevice _graphicsDevice;

	private Texture2D _overlay;

	private Effect _effect;

	public WaveInfoDrawer(GraphicsDevice graphicsDevice, ContentManager contentManager)
	{
		_font = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		_renderTarget = new RenderTarget2D(graphicsDevice, 400, 200);
		_graphicsDevice = graphicsDevice;
		_overlay = contentManager.Load<Texture2D>("Zombie/ParticleSprites/Explosion");
		_effect = contentManager.Load<Effect>("Zombie/WaveNumberEffect");
	}

	public void BeginRTDraw(SpriteBatch spriteBatch, string text)
	{
		_graphicsDevice.SetRenderTarget(_renderTarget);
		_graphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin();
		spriteBatch.DrawString(_font, text, new Vector2(200f, 100f), Color.Brown * 1f, 0f, _font.MeasureString(text) * 0.5f, 1f, SpriteEffects.None, 1f);
		spriteBatch.End();
		_graphicsDevice.SetRenderTarget(global::FiftyGames.FiftyGames.MasterRenderTarget);
	}

	public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		_effect.Parameters["InputTexture"].SetValue(_overlay);
		_effect.Parameters["Time"].SetValue((float)ZombieUtils.ElapsedTime / 5000f);
		spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _effect);
		spriteBatch.Draw(_renderTarget, new Vector2(900f, 440f), Color.White);
		spriteBatch.End();
	}
}
