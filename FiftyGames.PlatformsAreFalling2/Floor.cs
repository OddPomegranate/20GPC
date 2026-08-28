using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.PlatformsAreFalling2;

internal class Floor
{
	private Vector2 _position;

	private bool _active = true;

	private Texture2D _sprite;

	private Vector2 _origin;

	private int _widthHalf;

	private int _heightHalf;

	private int _screenWidth;

	private Body _rectangle;


	public Vector2 Position => _position;

	public Texture2D Sprite => _sprite;

	public bool Active => _active;

	public int WidthHalf => _widthHalf;

	public int HeightHalf => _heightHalf;

	public Floor(int screenWidth)
	{
		_screenWidth = screenWidth;
	}

	public void LoadContent(ContentManager content, World world, Vector2 position)
	{
		_position = position;
		_sprite = content.Load<Texture2D>("PlatformsAreFalling/Sprites/Floor");
		_widthHalf = _sprite.Width / 2;
		_heightHalf = _sprite.Height / 2;
		_origin = new Vector2(_widthHalf, _heightHalf + 2);
		_rectangle = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(1280 + _screenWidth), ConvertUnits.ToSimUnits(_sprite.Height), 1f);
		_rectangle.BodyType = BodyType.Static;
		_rectangle.CollisionCategories = Category.Cat10;
		_rectangle.Position = ConvertUnits.ToSimUnits(_position);
		_rectangle.FixtureList[0].Body.UserData = 2;
		_rectangle.Friction = 0.1f;
	}

	public void Draw(SpriteBatch spriteBatch, float screenOffset)
	{
		spriteBatch.Draw(destinationRectangle: new Rectangle((int)ConvertUnits.ToDisplayUnits(_rectangle.Position.X), (int)(ConvertUnits.ToDisplayUnits(_rectangle.Position.Y) - screenOffset), _screenWidth, _sprite.Height), texture: _sprite, sourceRectangle: null, color: Color.White, rotation: 0f, origin: _origin, effects: SpriteEffects.None, layerDepth: 0f);
	}
}
