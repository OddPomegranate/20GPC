using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class eBullet
{
	private const int flakTimerMinToDetonate = 30;

	private const int flakTimerMax = 90;

	private const int flackTrailTimerMax = 30;

	private const float flakBulletSpeed = 6f;

	private const float laserBulletSpeed = 6f;

	private const float laserCannonSpeed = 15f;

	private const float smallRoundBulletSpeed = 2f;

	private const float rocketBulletSpeed = 4f;

	private const int numberOfFlakshrapnel = 5;

	private const float flakProxyRange = 100f;

	private const float constExplosionScale = 0.6f;

	private const int laserExistanceCounterMax = 2;

	private const float laserDistortResolution = 0.03f;

	private GraphicsDevice graphicsDevice;

	private ContentManager contentManager;

	private Texture2D sprite;

	private Vector2 position;

	private typeOfEnemyBullet typeOfBullet;

	private float rotation;

	private playerShip playerTracking;

	private Vector2 origin;

	private int flakTimer = 90;

	private int flakTrailTimer = 30;

	private float bulletSpeed;

	private explosionManager explosionManagerRef;

	private BoundingSphere collisionSphere;

	private Random rand;

	private Vector2 laserBlastEndPosition;

	private Vector2 lastPosition;

	private Line laserLine = default(Line);

	private LineRender lineRenderer;

	private int laserExistanceCounter = 2;

	private bool noneDamagingBullet;

	private float bulletAlpha;

	private gridSystem gridManager;

	private bool hasDistortion;

	private Texture2D processedBulletWarpSprite;

	private Cue soundEffectCue;

	public eBullet(GraphicsDevice inGraphicsDevice, ContentManager inContentManager, explosionManager InExplosionManagerRef, playerShip playerToTrack, typeOfEnemyBullet inBulletType, Vector2 inPosition, float inRotation, Random inRand, bool inNoneDamagingBullet, float inBulletAlpha, gridSystem inGridManager, bool inDistortion, Texture2D warpImageToUse, bool isSilent)
	{
		hasDistortion = inDistortion;
		gridManager = inGridManager;
		bulletAlpha = inBulletAlpha;
		noneDamagingBullet = inNoneDamagingBullet;
		rand = inRand;
		graphicsDevice = inGraphicsDevice;
		contentManager = inContentManager;
		playerTracking = playerToTrack;
		typeOfBullet = inBulletType;
		if (typeOfBullet == typeOfEnemyBullet.LaserBlast)
		{
			processedBulletWarpSprite = warpImageToUse;
		}
		position = inPosition;
		rotation = inRotation;
		explosionManagerRef = InExplosionManagerRef;
		switch (typeOfBullet)
		{
		case typeOfEnemyBullet.smallRound:
			sprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/Bullet");
			bulletSpeed = 2f;
			if (!isSilent)
			{
				soundEffectCue = ForeverHelper.soundManager.CreateGameSoundCue("geometryWars EnemyFire");
				soundEffectCue.Play();
			}
			break;
		case typeOfEnemyBullet.Rocket:
			sprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/Rocket");
			bulletSpeed = 4f;
			if (!isSilent)
			{
				soundEffectCue = ForeverHelper.soundManager.CreateGameSoundCue("geometryWars RocketFired");
				soundEffectCue.Play();
			}
			break;
		case typeOfEnemyBullet.Laser:
			sprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/Bullet");
			bulletSpeed = 6f;
			break;
		case typeOfEnemyBullet.Artillery:
			sprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/Flak");
			bulletSpeed = 6f;
			break;
		case typeOfEnemyBullet.LaserBlast:
			sprite = contentManager.Load<Texture2D>("ForeverWars/Sprites/LaserCannonBlast");
			bulletSpeed = 0f;
			break;
		}
		origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
		lineRenderer = new LineRender(graphicsDevice, contentManager, new Rectangle(0, 0, 2000, 2000));
		if (typeOfBullet != typeOfEnemyBullet.LaserBlast && typeOfBullet != typeOfEnemyBullet.smallRound)
		{
			processedBulletWarpSprite = gridManager.generateDirectionalImage(rotation);
		}
	}

	public void destroyBullet()
	{
		if (soundEffectCue != null && !soundEffectCue.IsDisposed)
		{
			soundEffectCue.Stop(AudioStopOptions.AsAuthored);
		}
	}

	public BoundingSphere getCollisionSphere()
	{
		return collisionSphere;
	}

	public bool checkForCollision(BoundingSphere sphereToCheckWith)
	{
		if (noneDamagingBullet)
		{
			return false;
		}
		if (typeOfBullet != typeOfEnemyBullet.LaserBlast)
		{
			return collisionSphere.Intersects(sphereToCheckWith);
		}
		Circle circle = GeometryHelper.GenerateCircle((int)sphereToCheckWith.Radius, 8, new Vector2(sphereToCheckWith.Center.X, sphereToCheckWith.Center.Y));
		Vector2[] array = GeometryHelper.IntersectionPoint(laserLine, circle);
		Vector2 v = laserLine.End - laserLine.Start;
		v.Normalize();
		Vector2 v2 = circle.Position - laserLine.Start;
		v2.Normalize();
		float num = GeometryHelper.UnsignedAngleBetweenTwoV2(v, v2);
		if (num < 1f && array.Length > 0)
		{
			return true;
		}
		return false;
	}

	public typeOfEnemyBullet getTypeOfBullet()
	{
		return typeOfBullet;
	}

	public bool Update(List<eBullet> listOfEBullets, playerShip[] playerList, List<pBullet> listOfpBullets)
	{
		position += AngleToV2(rotation, bulletSpeed);
		if (typeOfBullet == typeOfEnemyBullet.LaserBlast)
		{
			laserLine.Start = position;
			laserLine.End = position + AngleToV2(rotation, 3000f);
		}
		foreach (pBullet listOfpBullet in listOfpBullets)
		{
			if (listOfpBullet.isShockWave() && listOfpBullet.getCollisionSphere().Intersects(collisionSphere))
			{
				return true;
			}
		}
		if (typeOfBullet == typeOfEnemyBullet.Rocket)
		{
			explosionManagerRef.addExplosion(position, 1f, explosionType.tinySmoke);
		}
		collisionSphere = new BoundingSphere(new Vector3(position, 0f), sprite.Width);
		if (hasDistortion)
		{
			lastPosition = position;
			switch (typeOfBullet)
			{
			case typeOfEnemyBullet.Rocket:
				gridManager.AddWarpEvent(processedBulletWarpSprite, position - AngleToV2(rotation, 30.000002f), 0.6f, rotation);
				break;
			case typeOfEnemyBullet.LaserBlast:
				gridManager.AddWarpEvent(laserLine.Start, 1f, 0f, 1f);
				gridManager.AddWarpEvent(processedBulletWarpSprite, laserLine.Start, 1f, rotation, laserLine.End);
				break;
			}
		}
		if (typeOfBullet == typeOfEnemyBullet.Artillery)
		{
			flakTimer--;
			if (flakTimer < 0)
			{
				for (int i = 0; i < 5; i++)
				{
					listOfEBullets.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.smallRound, position, (float)rand.NextDouble() * ((float)Math.PI * 2f), rand, inNoneDamagingBullet: false, 1f, gridManager, inDistortion: true, null, isSilent: true));
				}
				explosionManagerRef.addExplosion(position, 0.4f, explosionType.small);
				ForeverHelper.soundManager.CreateGameSoundCue("geometryWars Explosion Small").Play();
				return true;
			}
			if (flakTimer < 60)
			{
				for (int j = 0; j < playerList.Length; j++)
				{
					if (Vector2.Distance(playerList[j].getPosition(), position) < 100f)
					{
						for (int k = 0; k < 5; k++)
						{
							listOfEBullets.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.smallRound, position, (float)rand.NextDouble() * ((float)Math.PI * 2f), rand, inNoneDamagingBullet: false, 1f, gridManager, inDistortion: true, gridManager.generateDirectionalImage(rotation), isSilent: true));
						}
						explosionManagerRef.addExplosion(position, 0.4f, explosionType.small);
						ForeverHelper.soundManager.CreateGameSoundCue("geometryWars Explosion Small").Play();
						return true;
					}
				}
			}
		}
		if (position.X < 0f || position.Y < 0f || position.X > 2000f || position.Y > 2000f)
		{
			return true;
		}
		if (typeOfBullet == typeOfEnemyBullet.LaserBlast)
		{
			laserExistanceCounter--;
			if (laserExistanceCounter < 0)
			{
				return true;
			}
		}
		return false;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (typeOfBullet != typeOfEnemyBullet.LaserBlast)
		{
			spriteBatch.Draw(sprite, position, null, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);
			return;
		}
		VertexPositionColor[] array = new VertexPositionColor[2];
		array[0].Position = new Vector3(laserLine.Start, 0f);
		array[0].Color = Color.Red * bulletAlpha;
		array[1].Position = new Vector3(laserLine.End, 0f);
		array[1].Color = Color.Red * bulletAlpha;
		lineRenderer.DrawShape(array);
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
