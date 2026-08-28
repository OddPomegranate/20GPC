using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.TheSkyIsFalling;

internal class Robot
{
	private Player _player;

	private Vector2 _position;

	private Vector2 _velocity;

	private Color _colour;

	private float _scale;

	private float _rotationLean;

	private float _rotationWheel;

	private float _previousRotationWheel;

	private float _timeLived;

	private bool _alive;

	private Texture2D _legsSprite;

	private Vector2 _legsOrigin;

	private Color[] _legsSpriteData;

	private Texture2D _bodySprite;

	private Vector2 _bodyOrigin;

	private Color[] _bodySpriteData;

	private Texture2D _armsSprite;

	private Vector2 _armsOrigin;

	private Color[] _armsSpriteData;

	private Texture2D _headSprite;

	private Vector2 _headOrigin;

	private Color[] _headSpriteData;

	public bool Alive
	{
		get
		{
			return _alive;
		}
		set
		{
			_alive = value;
		}
	}

	public Vector2 Position
	{
		set
		{
			_position = value;
		}
	}

	public Vector2 Velocity
	{
		set
		{
			_velocity = value;
			_rotationLean = 0f;
		}
	}

	public float TimeLived => _timeLived;

	public Color Color => _colour;

	public Robot(Player player, Vector2 position, float scale, Texture2D legsSprite, Texture2D bodySprite, Texture2D armsSprite, Texture2D headSprite, bool alive)
	{
		_player = player;
		_position = position;
		_velocity = Vector2.Zero;
		_scale = 1f;
		_alive = true;
		_rotationLean = 0f;
		_rotationWheel = 0f;
		_legsSprite = legsSprite;
		_legsOrigin = new Vector2((float)legsSprite.Width / 2f, (float)legsSprite.Height / 2f);
		_legsSpriteData = new Color[_legsSprite.Width * _legsSprite.Height];
		_legsSprite.GetData(_legsSpriteData);
		_bodySprite = bodySprite;
		_bodyOrigin = new Vector2((float)bodySprite.Width / 2f, bodySprite.Height + 10);
		_bodySpriteData = new Color[_bodySprite.Width * _bodySprite.Height];
		_bodySprite.GetData(_bodySpriteData);
		_armsSprite = armsSprite;
		_armsOrigin = new Vector2((float)armsSprite.Width / 2f, (float)armsSprite.Height / 2f);
		_armsSpriteData = new Color[_armsSprite.Width * _armsSprite.Height];
		_armsSprite.GetData(_armsSpriteData);
		_headSprite = headSprite;
		_headOrigin = new Vector2((float)headSprite.Width / 2f, (float)headSprite.Height / 2f);
		_headSpriteData = new Color[_headSprite.Width * _headSprite.Height];
		_headSprite.GetData(_headSpriteData);
		_colour = player.Colour();
	}

	public void Update(Meteor[] meteors, ref SoundManager soundManager, ref MinigameMeta minigame, float timeSurvived)
	{
		if (_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X > 0f)
		{
			_previousRotationWheel = 1f;
		}
		else if (_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X < 0f)
		{
			_previousRotationWheel = -1f;
		}
		_rotationWheel += _velocity.X / 50f;
		_rotationWheel = MathHelper.WrapAngle(_rotationWheel);
		if (_player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X == 0f && !(_rotationWheel < MathHelper.ToRadians(-120f)) && !(_rotationWheel > MathHelper.ToRadians(120f)) && (!(_rotationWheel > MathHelper.ToRadians(-60f)) || !(_rotationWheel < MathHelper.ToRadians(60f))))
		{
			_rotationWheel += _previousRotationWheel * 0.025f;
		}
		_rotationLean += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X / 100f;
		_rotationLean *= 0.99f;
		_velocity.X += _player.GamePadManager.GamePadStateCurrent.ThumbSticks.Left.X;
		_position += _velocity;
		_velocity /= 1.1f;
		_position.X = MathHelper.Clamp(_position.X, 64f, 1216f);
		if (_alive)
		{
			foreach (Meteor meteor in meteors)
			{
				if (meteor.Alive && Collided(meteor))
				{
					_timeLived = timeSurvived;
					soundManager.CreateGameSoundCue("theSkyIsFalling Hit").Play();
					_player.GamePadManager.StartVibration(5, 0.7f);
					_alive = false;
					if (minigame.BestScore < timeSurvived || minigame.BestScore == 0f)
					{
						minigame.SetScore(_player.Name, timeSurvived);
					}
					break;
				}
			}
		}
		if (!_alive)
		{
			_rotationLean += 0.05f;
			_velocity.Y++;
		}
		float num = MathHelper.WrapAngle(_rotationWheel);
		float num2 = MathHelper.WrapAngle(_previousRotationWheel);
		if (!(num > 0f) || !(num2 < 0f) || num < 1f)
		{
		}
		_previousRotationWheel = _rotationWheel;
	}

	private bool Collided(Meteor meteor)
	{
		if (Helper.DistanceToPoint(meteor.Position, _position) < 198f)
		{
			if (PerPixelCollision(meteor.Sprite, meteor.SpriteData, meteor.Position, meteor.Origin, meteor.Scale, meteor.Rotation, _legsSprite, _legsSpriteData, _position, _legsOrigin, _scale, _rotationWheel))
			{
				return true;
			}
			if (PerPixelCollision(meteor.Sprite, meteor.SpriteData, meteor.Position, meteor.Origin, meteor.Scale, meteor.Rotation, _bodySprite, _bodySpriteData, _position, _bodyOrigin, _scale, _rotationLean))
			{
				return true;
			}
			Vector2 vector = new Vector2(86f * (float)Math.Sin(_rotationLean), -86f * (float)Math.Cos(_rotationLean));
			if (PerPixelCollision(meteor.Sprite, meteor.SpriteData, meteor.Position, meteor.Origin, meteor.Scale, meteor.Rotation, _headSprite, _headSpriteData, _position + vector, _headOrigin, _scale, 0f - _rotationWheel))
			{
				return true;
			}
		}
		return false;
	}

	private bool PerPixelCollision(Texture2D texture1, Color[] textureData1, Vector2 position1, Vector2 origin1, float scale1, float rotation1, Texture2D texture2, Color[] textureData2, Vector2 position2, Vector2 origin2, float scale2, float rotation2)
	{
		if (texture1 == null || texture2 == null)
		{
			return false;
		}
		Matrix matrix = Matrix.CreateTranslation(new Vector3(-origin1, 0f)) * Matrix.CreateScale(scale1) * Matrix.CreateRotationZ(rotation1) * Matrix.CreateTranslation(new Vector3(position1, 0f));
		Rectangle rectangle = CalculateBoundingRectangle(new Rectangle(0, 0, texture1.Width, texture1.Height), matrix);
		Matrix matrix2 = Matrix.CreateTranslation(new Vector3(-origin2, 0f)) * Matrix.CreateScale(scale2) * Matrix.CreateRotationZ(rotation2) * Matrix.CreateTranslation(new Vector3(position2, 0f));
		Rectangle value = CalculateBoundingRectangle(new Rectangle(0, 0, texture2.Width, texture2.Height), matrix2);
		if (rectangle.Intersects(value))
		{
			if (IntersectPixels(matrix, texture1.Width, texture1.Height, textureData1, matrix2, texture2.Width, texture2.Height, textureData2))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private static bool IntersectPixels(Matrix transformA, int widthA, int heightA, Color[] dataA, Matrix transformB, int widthB, int heightB, Color[] dataB)
	{
		Matrix matrix = transformA * Matrix.Invert(transformB);
		Vector2 vector = Vector2.TransformNormal(Vector2.UnitX, matrix);
		Vector2 vector2 = Vector2.TransformNormal(Vector2.UnitY, matrix);
		Vector2 vector3 = Vector2.Transform(Vector2.Zero, matrix);
		for (int i = 0; i < heightA; i++)
		{
			Vector2 vector4 = vector3;
			for (int j = 0; j < widthA; j++)
			{
				int num = (int)Math.Round(vector4.X);
				int num2 = (int)Math.Round(vector4.Y);
				if (0 <= num && num < widthB && 0 <= num2 && num2 < heightB)
				{
					Color color = dataA[j + i * widthA];
					Color color2 = dataB[num + num2 * widthB];
					if (color.A != 0 && color2.A != 0)
					{
						return true;
					}
				}
				vector4 += vector;
			}
			vector3 += vector2;
		}
		return false;
	}

	private static Rectangle CalculateBoundingRectangle(Rectangle rectangle, Matrix transform)
	{
		Vector2 position = new Vector2(rectangle.Left, rectangle.Top);
		Vector2 position2 = new Vector2(rectangle.Right, rectangle.Top);
		Vector2 position3 = new Vector2(rectangle.Left, rectangle.Bottom);
		Vector2 position4 = new Vector2(rectangle.Right, rectangle.Bottom);
		Vector2.Transform(ref position, ref transform, out position);
		Vector2.Transform(ref position2, ref transform, out position2);
		Vector2.Transform(ref position3, ref transform, out position3);
		Vector2.Transform(ref position4, ref transform, out position4);
		Vector2 vector = Vector2.Min(Vector2.Min(position, position2), Vector2.Min(position3, position4));
		Vector2 vector2 = Vector2.Max(Vector2.Max(position, position2), Vector2.Max(position3, position4));
		return new Rectangle((int)vector.X, (int)vector.Y, (int)(vector2.X - vector.X), (int)(vector2.Y - vector.Y));
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_legsSprite, _position, null, _colour, _rotationWheel, _legsOrigin, _scale, SpriteEffects.None, 0f);
		spriteBatch.Draw(_bodySprite, _position, null, _colour, _rotationLean, _bodyOrigin, _scale, SpriteEffects.None, 0f);
		Vector2 vector = new Vector2(86f * (float)Math.Sin(_rotationLean), -86f * (float)Math.Cos(_rotationLean));
		spriteBatch.Draw(_headSprite, _position + vector, null, _colour, 0f - _rotationLean, _headOrigin, _scale, SpriteEffects.None, 0f);
		vector = new Vector2(40f * (float)Math.Sin(_rotationLean), -40f * (float)Math.Cos(_rotationLean));
		spriteBatch.Draw(_armsSprite, _position + vector, null, _colour, _rotationWheel, _armsOrigin, _scale, SpriteEffects.None, 0f);
	}
}
