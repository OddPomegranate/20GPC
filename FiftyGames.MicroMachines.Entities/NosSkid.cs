using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.MicroMachines.Entities;

internal class NosSkid
{
	private const float speedOfOscilationConstant = 8f;

	private Texture2D _texture;

	private float _alpha;

	private float timePassedSinceCreation;

	private float _rotation;

	private Vector2 _position;

	private float timeItTakesToFadeInSecondsConstant = 100f;

	public bool IsAlive { get; set; }

	public NosSkid(Texture2D specialSkidTexture, Vector2 position, float rotation, Random random)
	{
		IsAlive = true;
		timePassedSinceCreation = (float)random.NextDouble() * 50f;
		_texture = specialSkidTexture;
		_rotation = rotation;
		_position = position;
		timeItTakesToFadeInSecondsConstant += (float)random.NextDouble() * 500f;
	}

	public void Update(GameTime gameTime)
	{
		timePassedSinceCreation += gameTime.ElapsedGameTime.Milliseconds * 2;
		_alpha = (float)((Math.Sin(timePassedSinceCreation * 8f) + 1.0) / 2.0) - timePassedSinceCreation / timeItTakesToFadeInSecondsConstant;
		if (timePassedSinceCreation > timeItTakesToFadeInSecondsConstant)
		{
			IsAlive = false;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin();
		spriteBatch.Draw(_texture, _position, null, Color.White * _alpha, _rotation, new Vector2(_texture.Width / 2, _texture.Height / 2), 1f, SpriteEffects.None, 0f);
		spriteBatch.End();
	}
}
