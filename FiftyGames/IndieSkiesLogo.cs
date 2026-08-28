using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class IndieSkiesLogo : AnimationSequence
{
	private const float StartScale = 0.8f;

	private const float EndScale = 1f;

	private const int FadeInTime = 3000;

	private const int FadeOutTime = 1000;

	private Texture2D _texLogo;

	private Texture2D _texGradient;

	private float _fadeOut;

	public override void Initialise()
	{
		base.Initialise();
		_animationTimeLimit = 6000;
		_fadeOut = 1f;
	}

	public override void Load(ContentManager contentManager, SoundManager soundManager)
	{
		_texLogo = contentManager.Load<Texture2D>("Logo/IndieSkies/Logo");
		_texGradient = contentManager.Load<Texture2D>("Logo/IndieSkies/Gradient");
		base.Load(contentManager, soundManager);
	}

	public override void Update(GameTime gameTime)
	{
		if (base.AnimationTimeElapsed == 0)
		{
			_soundManager.CreateGameSoundCue("menu IntroSound").Play();
		}
		if (_animationTimeLimit - base.AnimationTimeElapsed <= 1000)
		{
			_fadeOut = (float)(_animationTimeLimit - base.AnimationTimeElapsed) * 0.001f;
		}
		base.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		float num = (float)_texGradient.Height * ((float)base.AnimationTimeElapsed / (float)_animationTimeLimit);
		float scale = 0.8f + 0.19999999f * ((float)base.AnimationTimeElapsed / (float)_animationTimeLimit);
		float num2 = ((base.AnimationTimeElapsed < 3000) ? ((float)base.AnimationTimeElapsed / 3000f) : 1f);
		num2 *= _fadeOut;
		spriteBatch.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		spriteBatch.Draw(_texLogo, new Vector2(640f, 360f), null, Color.White * num2, 0f, new Vector2(_texLogo.Width / 2, _texLogo.Height / 2), scale, SpriteEffects.None, 1f);
		spriteBatch.Draw(_texGradient, new Vector2(640f, 360f + num), null, Color.White, 0f, new Vector2(_texGradient.Width / 2, _texGradient.Height / 2), 1f, SpriteEffects.None, 1f);
		spriteBatch.End();
	}
}
