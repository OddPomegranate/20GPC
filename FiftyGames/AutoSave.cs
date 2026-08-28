using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames;

internal class AutoSave : AnimationSequence
{
	private const int FadeInTime = 500;

	private const int FadeOutTime = 500;

	private MenuComponent _autoSaveIcon;

	private SpriteFont _font;

	private float _fadeOut;

	public override void Initialise()
	{
		base.Initialise();
		_animationTimeLimit = 10000;
		_fadeOut = 1f;
	}

	public override void Load(ContentManager contentManager, SoundManager soundManager)
	{
		_autoSaveIcon = new MenuComponent();
		_autoSaveIcon.Sprite = contentManager.Load<Texture2D>("Menu/Sprites/General/LoadIndicator");
		_autoSaveIcon.FitComponentToImage();
		_autoSaveIcon.SpriteOrigin = new Vector2((float)_autoSaveIcon.Sprite.Width * 0.5f, (float)_autoSaveIcon.Sprite.Height * 0.5f);
		MenuComponent autoSaveIcon = _autoSaveIcon;
		Vector2 position = (_autoSaveIcon.DesiredPosition = new Vector2(640f, 280f));
		autoSaveIcon.Position = position;
		_autoSaveIcon.PositionAnchor = MenuComponent.Anchor.TopLeft;
		_autoSaveIcon.DesiredRotation = (float)Math.PI * 2f;
		_autoSaveIcon.Depth = 1f;
		_font = contentManager.Load<SpriteFont>("Menu/Fonts/MainMenuFont");
		base.Load(contentManager, soundManager);
	}

	public override void Update(GameTime gameTime)
	{
		if (_autoSaveIcon.Rotation == _autoSaveIcon.DesiredRotation)
		{
			_autoSaveIcon.Rotation = 0f;
		}
		_autoSaveIcon.Update(gameTime);
		if (_animationTimeLimit - base.AnimationTimeElapsed <= 500)
		{
			_fadeOut = (float)(_animationTimeLimit - base.AnimationTimeElapsed) * 0.002f;
		}
		base.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		float num = ((base.AnimationTimeElapsed < 500) ? ((float)base.AnimationTimeElapsed / 500f) : 1f);
		num *= _fadeOut;
		spriteBatch.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		MenuComponent autoSaveIcon = _autoSaveIcon;
		Color colour = (_autoSaveIcon.DesiredColour = Color.White * num);
		autoSaveIcon.Colour = colour;
		_autoSaveIcon.Draw(spriteBatch);
		spriteBatch.DrawString(_font, "This game employs an auto-save feature.", new Vector2(640f, 200f), Color.White * num, 0f, _font.MeasureString("This game eploys an auto-save feature.") / 2f, 1f, SpriteEffects.None, 1f);
		spriteBatch.DrawString(_font, "Please do not turn off your Xbox 360 console", new Vector2(640f, 360f), Color.White * num, 0f, _font.MeasureString("Please do not turn off your Xbox 360 console") / 2f, 1f, SpriteEffects.None, 1f);
		spriteBatch.DrawString(_font, "or remove the storage device", new Vector2(640f, 388f), Color.White * num, 0f, _font.MeasureString("or remove the storage device") / 2f, 1f, SpriteEffects.None, 1f);
		spriteBatch.DrawString(_font, "when this icon is displayed.", new Vector2(640f, 416f), Color.White * num, 0f, _font.MeasureString("when the icon is displayed.") / 2f, 1f, SpriteEffects.None, 1f);
		spriteBatch.End();
	}
}
