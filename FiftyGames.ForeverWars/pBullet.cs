using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class pBullet
{
	private const float smallRoundBulletSpeed = 15f;

	private const int numberOfShockWavePoints = 20;

	private const float shockWaveRadiusMax = 400f;

	private const float shockWaveSpeed = 10f;

	private List<shipModule> listOfDamagedModules = new List<shipModule>();

	private GraphicsDevice graphicsDevice;

	private ContentManager contentManager;

	private Texture2D sprite;

	private Vector2 position;

	private typeOfEnemyBullet typeOfBullet;

	private float rotation;

	private Vector2 origin;

	private Color colorOfPlayer;

	private BoundingSphere collisionSphere;

	private bool isBomb;

	private LineRender bombWaveRenderer;

	private float shockWaveRadius;

	private VertexPositionColor[] lineArray = new VertexPositionColor[2];

	private Vector2 playerFiringMomentum;

	private playerShip owner;

	private gridSystem gridManager;

	private List<shipModule> shieldModulePassThroughList;

	private bool isFirstFrame = true;

	private bool firstFrameTick = true;

	public pBullet(GraphicsDevice inGraphicsDevice, ContentManager inContentManager, Vector2 inPosition, float inRotation, Color playerColor, bool inIsBomb, playerShip inOwner, gridSystem inGridManager, Vector2 inPlayerFiringMomentum)
	{
		isFirstFrame = true;
		playerFiringMomentum = inPlayerFiringMomentum;
		graphicsDevice = inGraphicsDevice;
		contentManager = inContentManager;
		owner = inOwner;
		position = inPosition;
		rotation = inRotation;
		colorOfPlayer = playerColor;
		gridManager = inGridManager;
		isBomb = inIsBomb;
		shieldModulePassThroughList = new List<shipModule>();
		sprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/BulletPlayer");
		origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
		collisionSphere = new BoundingSphere(new Vector3(position, 0f), 4f);
		bombWaveRenderer = new LineRender(graphicsDevice, contentManager, new Rectangle(0, 0, 2000, 2000));
		_ = isBomb;
	}

	public void setIsFirstFrame(bool value)
	{
		isFirstFrame = value;
	}

	public bool getIsFirstFrame()
	{
		return isFirstFrame;
	}

	public bool Update()
	{
		if (firstFrameTick)
		{
			firstFrameTick = false;
		}
		else if (isFirstFrame)
		{
			isFirstFrame = false;
		}
		if (!isBomb)
		{
			position += AngleToV2(rotation, 15f);
			collisionSphere = new BoundingSphere(new Vector3(position, 0f), 4f);
			if (position.X < 0f || position.Y < 0f || position.X > 2000f || position.Y > 2000f)
			{
				return true;
			}
			return false;
		}
		shockWaveRadius += 10f;
		collisionSphere = new BoundingSphere(new Vector3(position, 0f), shockWaveRadius);
		rotation += 0.1f;
		float inIntensity = (400f - shockWaveRadius) / 400f;
		gridManager.AddWarpEvent(position, shockWaveRadius * 0.02f, rotation, inIntensity);
		if (shockWaveRadius > 400f)
		{
			return true;
		}
		return false;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (!isBomb)
		{
			spriteBatch.Draw(sprite, position, null, colorOfPlayer, rotation, origin, 1f, SpriteEffects.None, 0f);
			return;
		}
		for (int i = 0; i < 20; i++)
		{
			float angle = (float)i / 20f * ((float)Math.PI * 2f);
			lineArray[0].Position = new Vector3(ForeverHelper.AngleToV2(angle, shockWaveRadius) + position, 0f);
			lineArray[0].Color = colorOfPlayer;
			angle = ((float)i + 1f) / 20f * ((float)Math.PI * 2f);
			lineArray[1].Position = new Vector3(ForeverHelper.AngleToV2(angle, shockWaveRadius) + position, 0f);
			lineArray[1].Color = colorOfPlayer;
			bombWaveRenderer.DrawShape(lineArray);
		}
	}

	public List<shipModule> getShieldModuleList()
	{
		return shieldModulePassThroughList;
	}

	public void addToShieldModuleRef(shipModule inShipModule)
	{
		shieldModulePassThroughList.Add(inShipModule);
	}

	public List<shipModule> getDamagedModuleList()
	{
		return listOfDamagedModules;
	}

	public Vector2 getPosition()
	{
		return position;
	}

	public playerShip getOwner()
	{
		return owner;
	}

	public BoundingSphere getCollisionSphere()
	{
		return collisionSphere;
	}

	public bool isShockWave()
	{
		return isBomb;
	}

	public float V2ToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.X, vector.Y);
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
