using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class shipModule
{
	private const int smallTurretFiringCounterMax = 8;

	private const float smallTurretFiringAngle = (float)Math.PI / 8f;

	private const float smallTurretSpeed = 0.07f;

	private const float rocketPackFiringAngle = (float)Math.PI / 5f;

	private const float rocketPackSpeed = 0.04f;

	private const int rocketPackFiringCounterMax = 70;

	private const int rocketRandomReloadDev = 30;

	private const float artPackFiringAngle = (float)Math.PI / 5f;

	private const int artPackFiringCounterMax = 60;

	private const float artFiringRange = 300f;

	private const int artRandomReloadDev = 10;

	private const float laserFiringAngle = (float)Math.PI / 5f;

	private const int laserFiringCounterMax = 50;

	private const int laserRandomReloadDev = 10;

	private const int laserCannonChargingTimeMax = 120;

	private const int laserCannonFiringTimeMax = 190;

	private const float laserCannonFiringAngle = (float)Math.PI / 80f;

	private const int laserCannonFiringCounterMax = 50;

	private const int laserCannonRandomReloadDev = 10;

	private const float laserCannonSpeed = 0.01f;

	private const float laserCannonBeamWidth = 10f;

	private const float laserCannonBeamResolution = 2f;

	private const float cruiserAIExtendedDetectionRadius = 640f;

	private const float cruiserAIDetectionRadius = 500f;

	private const float cruiserAIRotationForce = 0.0001f;

	private const float cruiserAIEngineForce = 0.01f;

	private const float cruiserAIDriftFriction = 0.99f;

	private const float cruiserAIRotationMomentumMAX = 0.001f;

	private const float fighterAIProxiRadius = 300f;

	private const float fighterAIRetreatRadius = 700f;

	private const float fighterAIDetectionRadius = 500f;

	private const float fighterAIRotationForce = 0.001f;

	private const float fighterAIEngineForce = 0.07f;

	private const float fighterAIDriftFriction = 0.99f;

	private const float fighterAIRotationMomentumMAX = 0.07f;

	private const float stationAIExtendedDetectionRadius = 700f;

	private const float stationAIDetectionRadius = 500f;

	private const float stationAIRotationForce = 0.0001f;

	private const float stationAIEngineForce = 0.01f;

	private const float stationAIDriftFriction = 0.99f;

	private const float stationAIRotationMomentumMAX = 0.001f;

	private const int firingDeviationSmallTurret = 10;

	private const float moduleHealthMax = 100f;

	private const float moduleFighterHealthMax = 40f;

	private const float moduleSolarPHealthMax = 40f;

	private const float moduleCoreMax = 500f;

	private const float moduleLGirderMax = 300f;

	private const float moduleSBShieldHealthMax = 200f;

	private const float moduleSBShieldModuleHealthMax = 30f;

	private const float moduleSBShieldRadius = 60f;

	private const float moduleLBShieldHealthMax = 400f;

	private const float moduleLBShieldModuleHealthMax = 50f;

	private const float moduleLBShieldRadius = 80f;

	private const float moduleSTBShieldHealthMax = 600f;

	private const float moduleSTBShieldModuleHealthMax = 50f;

	private const float moduleSTBShieldRadius = 180f;

	private const float shieldAlphaMax = 1f;

	private const float shieldAlphaMin = 0.1f;

	private const float shieldAlphaFadeSpeed = 0.05f;

	private int smallTurretFiringCounter;

	private int rocketPackFiringCounter;

	private int artPackFiringCounter;

	private int laserFiringCounter;

	private int laserCannonChargingTime = 120;

	private int laserCannonFiringTime = 190;

	private int laserCannonStatus;

	private int laserCannonFiringCounter;

	private int firingDeviationCounter;

	private Vector2 fighterRetreatVector;

	private bool isRetreating;

	private Vector2 fighterMomentum = Vector2.Zero;

	private float fighterRotationMomentum;

	private Vector2 cruiserMomentum = Vector2.Zero;

	private float cruiserRotationMomentum;

	private shipModule[] attachedModules;

	private string tempCurrentLineString;

	private bool hasBeenShot;

	private bool isOffscreen = true;

	private bool wasOffscreen = true;

	private bool permittedToFire;

	private bool laserHasAppliedWarp;

	private Vector2 position;

	private float rotation;

	private float drawOverrideRotation;

	private shipData shipDataLocalRef;

	private Texture2D primarySprite;

	private Texture2D secondarySprite;

	private float[] turretRotationArray;

	private Vector2[] turretPositionArray;

	private bool[] turretIsFiringArray;

	private int[] turretFiringCounter;

	private int turretFiringOrderIndex;

	private Vector2 smallOrigin = new Vector2(16f);

	private Vector2 largeOrigin = new Vector2(24f);

	private Vector2 cellOrigin;

	private bool isCore;

	private int attachmentIndex;

	private typeOfBlock typeofShipModule;

	private int ShipType;

	private int ShipAiProfile;

	private explosionManager explosionManagerRef;

	private char tempChar;

	private Random randomRef;

	private ContentManager contentManager;

	private GraphicsDevice graphicsDevice;

	private BoundingSphere collisionVolume;

	private BoundingSphere shieldCollisionVolume;

	private bool m_Alive = true;

	private bool m_aliveLastFrame = true;

	private float moduleHealth = 100f;

	private playerShip lastHitByReference;

	private gridSystem gridManager;

	private Texture2D beamInstanceImage;

	private Texture2D bulletInstanceImage;

	private string shipName;

	private bool isBoss;

	private bool isStation;

	private float shieldRadius;

	private float shieldHealth;

	private float maxShieldHealth;

	private float shieldRotation;

	private VertexPositionColor[] lineArray;

	private List<VertexPositionColor> lineList;

	private LineRender lineRenderer;

	private float shieldAlpha = 1f;

	public shipModule(GraphicsDevice inGraphicsDevice, ContentManager inContentManager, Vector2 initialPosition, float initialRotation, shipData shipDataRef, bool inIsCore, int inShipType, int inShipAiProfile, typeOfBlock typeOfShipBlockToUse, int attachIndex, StreamReader shipConfigStream, explosionManager inExplosionManager, Random inRand, gridSystem inGridManager, string inShipName, bool inIsBoss, bool inIsStation)
	{
		isStation = inIsStation;
		isBoss = inIsBoss;
		shipName = inShipName;
		gridManager = inGridManager;
		contentManager = inContentManager;
		graphicsDevice = inGraphicsDevice;
		randomRef = inRand;
		explosionManagerRef = inExplosionManager;
		isCore = typeOfShipBlockToUse == typeOfBlock.core;
		typeofShipModule = typeOfShipBlockToUse;
		position = initialPosition;
		rotation = initialRotation;
		ShipType = inShipType;
		lineRenderer = new LineRender(graphicsDevice, contentManager, new Rectangle(0, 0, 3000, 3000));
		ShipAiProfile = inShipAiProfile;
		attachmentIndex = attachIndex;
		turretRotationArray = new float[4];
		attachedModules = new shipModule[4];
		turretPositionArray = new Vector2[4];
		turretIsFiringArray = new bool[4];
		turretFiringCounter = new int[4];
		shipDataLocalRef = shipDataRef;
		cellOrigin = ((!getShipModuleSizeisLarge(ShipType)) ? smallOrigin : largeOrigin);
		switch (typeofShipModule)
		{
		case typeOfBlock.core:
			rotation = ForeverHelper.TurnToFace(position, new Vector2(1500f, 1500f), rotation, 6f);
			moduleHealth = 500f;
			if (getShipModuleSizeisLarge(ShipType))
			{
				primarySprite = shipDataRef.lGirder;
				secondarySprite = shipDataRef.lCore;
			}
			else
			{
				primarySprite = shipDataRef.sGirder;
				secondarySprite = shipDataRef.sCore;
				moduleHealth = 40f;
			}
			break;
		case typeOfBlock.Link:
			primarySprite = shipDataRef.sLink;
			break;
		case typeOfBlock.Point:
			if (getShipModuleSizeisLarge(ShipType))
			{
				primarySprite = shipDataRef.lPoint;
				break;
			}
			primarySprite = shipDataRef.sPoint;
			moduleHealth = 40f;
			break;
		case typeOfBlock.Thruster:
			if (getShipModuleSizeisLarge(ShipType))
			{
				primarySprite = shipDataRef.lThruster;
				break;
			}
			primarySprite = shipDataRef.sThruster;
			moduleHealth = 40f;
			break;
		case typeOfBlock.Turret:
			if (getShipModuleSizeisLarge(ShipType))
			{
				primarySprite = shipDataRef.lGirder;
			}
			else
			{
				primarySprite = shipDataRef.sGirder;
				moduleHealth = 40f;
			}
			secondarySprite = shipDataRef.sTurret;
			break;
		case typeOfBlock.Girder:
			if (getShipModuleSizeisLarge(ShipType))
			{
				primarySprite = shipDataRef.lGirder;
				break;
			}
			primarySprite = shipDataRef.sGirder;
			moduleHealth = 40f;
			break;
		case typeOfBlock.Gun:
			if (getShipModuleSizeisLarge(ShipType))
			{
				primarySprite = shipDataRef.lGun;
				break;
			}
			primarySprite = shipDataRef.sGun;
			moduleHealth = 40f;
			break;
		case typeOfBlock.RocketPack:
			primarySprite = shipDataRef.lGirder;
			secondarySprite = shipDataRef.lRocketPack;
			break;
		case typeOfBlock.Panel:
			moduleHealth = 40f;
			primarySprite = shipDataRef.sPanel;
			break;
		case typeOfBlock.LaserCannon:
			primarySprite = shipDataRef.lGirder;
			secondarySprite = shipDataRef.lLaserCannon;
			break;
		case typeOfBlock.BShield:
			if (getShipModuleSizeisLarge(ShipType))
			{
				primarySprite = shipDataRef.lGirder;
				secondarySprite = shipDataRef.lBShield;
				moduleHealth = 50f;
				shieldHealth = 400f;
				maxShieldHealth = 400f;
				shieldRadius = 80f;
			}
			else
			{
				primarySprite = shipDataRef.sGirder;
				secondarySprite = shipDataRef.sBShield;
				moduleHealth = ((inShipAiProfile != 5) ? 30f : 50f);
				shieldHealth = ((inShipAiProfile != 5) ? 200f : 600f);
				maxShieldHealth = ((inShipAiProfile != 5) ? 200f : 600f);
				shieldRadius = ((inShipAiProfile != 5) ? 60f : 180f);
			}
			break;
		}
		for (int i = 0; i < 4; i++)
		{
			bool flag = true;
			while (flag)
			{
				tempCurrentLineString = shipConfigStream.ReadLine();
				int num = 0;
				bool flag2 = false;
				while (!flag2)
				{
					tempChar = tempCurrentLineString.ElementAt(num);
					if (tempChar == '\t' || tempChar == ' ')
					{
						num++;
					}
					else
					{
						flag2 = true;
					}
				}
				if (ShipType == 1)
				{
					switch (tempChar)
					{
					case '0':
						attachedModules[i] = null;
						flag = false;
						break;
					case '6':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Girder, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '3':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Point, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '5':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Turret, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '4':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Thruster, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '2':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Link, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '7':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Gun, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '8':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.BShield, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					}
				}
				else if (ShipType == 2)
				{
					switch (tempChar)
					{
					case '0':
						attachedModules[i] = null;
						flag = false;
						break;
					case '4':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Girder, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '2':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Point, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '3':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Turret, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '5':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Gun, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '6':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.RocketPack, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '7':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Thruster, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '8':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.LaserCannon, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '9':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.BShield, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					}
				}
				else if (ShipType == 3)
				{
					switch (tempChar)
					{
					case '0':
						attachedModules[i] = null;
						flag = false;
						break;
					case '6':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Girder, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '3':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Point, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '5':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Turret, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '4':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Panel, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '2':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Link, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '7':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.Gun, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					case '8':
						attachedModules[i] = new shipModule(graphicsDevice, contentManager, Vector2.Zero, 0f, shipDataRef, inIsCore: false, inShipType, inShipAiProfile, typeOfBlock.BShield, i, shipConfigStream, explosionManagerRef, randomRef, gridManager, shipName, inIsBoss: false, inIsStation: false);
						flag = false;
						break;
					}
				}
			}
		}
	}

	public Vector2 getPosition()
	{
		return position;
	}

	public string getName()
	{
		return shipName;
	}

	public int getShipModuleSizePixels(int inshipType)
	{
		if (inshipType != 2)
		{
			return 32;
		}
		return 48;
	}

	public bool getShipModuleSizeisLarge(int inshipType)
	{
		if (inshipType != 2)
		{
			return false;
		}
		return true;
	}

	public bool getIsBoss()
	{
		return isBoss;
	}

	public bool getIsStation()
	{
		return isStation;
	}

	public bool getHasBeenShot()
	{
		if (hasBeenShot)
		{
			return true;
		}
		for (int i = 0; i < 4; i++)
		{
			if (attachedModules[i] != null && attachedModules[i].getHasBeenShot())
			{
				return true;
			}
		}
		return false;
	}

	public playerShip getLastHitByRef()
	{
		if (typeofShipModule == typeOfBlock.core)
		{
			for (int i = 0; i < 4; i++)
			{
				if (attachedModules[i] != null)
				{
					playerShip lastHitByRef = attachedModules[i].getLastHitByRef();
					if (lastHitByRef != null && lastHitByRef != lastHitByReference)
					{
						lastHitByReference = lastHitByRef;
						return null;
					}
				}
			}
		}
		else
		{
			if (lastHitByReference != null)
			{
				return lastHitByReference;
			}
			for (int j = 0; j < 4; j++)
			{
				if (attachedModules[j] != null)
				{
					playerShip lastHitByRef2 = attachedModules[j].getLastHitByRef();
					if (lastHitByRef2 != null)
					{
						return lastHitByRef2;
					}
				}
			}
		}
		return null;
	}

	public bool Update(playerShip[] playerList, Vector2 positionOfParent, float rotationOfParent, List<eBullet> eBulletList, bool permittedToFire, List<pBullet> pBulletList)
	{
		if (m_Alive)
		{
			if (shieldAlpha > 0.1f)
			{
				shieldAlpha -= 0.05f;
			}
			for (int i = 0; i < pBulletList.Count; i++)
			{
				if (!pBulletList[i].isShockWave())
				{
					if (typeofShipModule != typeOfBlock.BShield || (typeofShipModule == typeOfBlock.BShield && shieldHealth < 1f))
					{
						if (pBulletList[i].getCollisionSphere().Intersects(collisionVolume))
						{
							hasBeenShot = true;
							moduleHealth -= 10f;
							lastHitByReference = pBulletList[i].getOwner();
							explosionManagerRef.addExplosion(pBulletList[i].getPosition(), 1f, explosionType.tiny);
							ForeverHelper.soundManager.CreateGameSoundCue("geometryWars PlayerHit").Play();
							pBulletList.RemoveAt(i);
							i--;
						}
					}
					else if (pBulletList[i].getCollisionSphere().Intersects(shieldCollisionVolume))
					{
						if (pBulletList[i].getIsFirstFrame())
						{
							pBulletList[i].addToShieldModuleRef(this);
						}
						else if (!pBulletList[i].getShieldModuleList().Contains(this))
						{
							hasBeenShot = true;
							shieldHealth -= 10f;
							lastHitByReference = pBulletList[i].getOwner();
							explosionManagerRef.addExplosion(pBulletList[i].getPosition(), 1f, explosionType.tiny);
							ForeverHelper.soundManager.CreateGameSoundCue("geometryWars PlayerHitSub").Play();
							pBulletList.RemoveAt(i);
							i--;
							shieldAlpha = 1f;
						}
						else if (pBulletList[i].getCollisionSphere().Intersects(collisionVolume))
						{
							hasBeenShot = true;
							moduleHealth -= 10f;
							lastHitByReference = pBulletList[i].getOwner();
							explosionManagerRef.addExplosion(pBulletList[i].getPosition(), 1f, explosionType.tiny);
							ForeverHelper.soundManager.CreateGameSoundCue("geometryWars PlayerHit").Play();
							pBulletList.RemoveAt(i);
							i--;
						}
					}
					continue;
				}
				lastHitByReference = null;
				if (!pBulletList[i].getCollisionSphere().Intersects(collisionVolume))
				{
					continue;
				}
				hasBeenShot = true;
				if (ShipAiProfile == 1 || ShipAiProfile == 4)
				{
					moduleHealth = -10f;
				}
				else if (!pBulletList[i].getDamagedModuleList().Contains(this))
				{
					pBulletList[i].getDamagedModuleList().Add(this);
					for (int j = 0; j < 10; j++)
					{
						explosionManagerRef.addExplosion(position + new Vector2((float)randomRef.NextDouble() * 48f - 24f, (float)randomRef.NextDouble() * 48f - 24f), 1f, explosionType.tiny);
					}
					moduleHealth -= 60f;
				}
			}
			if (isCore)
			{
				switch (ShipAiProfile)
				{
				case -1:
					rotation = 0f;
					break;
				case 1:
				{
					fighterRotationMomentum += 0.001f;
					fighterRotationMomentum = MathHelper.Clamp(fighterRotationMomentum, 0f, 0.07f);
					if (position.X < 0f || position.Y < 0f || position.X > 2000f || position.Y > 2000f)
					{
						rotation = ForeverHelper.TurnToFace(position, new Vector2(1000f, 1000f), rotation, fighterRotationMomentum);
					}
					else if (ForeverHelper.getClosestPlayer(position, playerList) != null)
					{
						if (isRetreating)
						{
							rotation = ForeverHelper.TurnToFace(position, position + fighterRetreatVector, rotation, fighterRotationMomentum * 0.3f);
						}
						else
						{
							rotation = ForeverHelper.TurnToFace(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition(), rotation, fighterRotationMomentum);
						}
						if (!isRetreating)
						{
							if (Vector2.Distance(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition()) < 300f)
							{
								isRetreating = true;
								Vector2 vector = position;
								Vector2 vector2 = ForeverHelper.getClosestPlayer(position, playerList).getPosition();
								Vector2 vector3 = vector2 - vector;
								vector3.Normalize();
								float angle = ForeverHelper.V2ToAngle(vector3) + ((randomRef.NextDouble() < 0.5) ? (-1f) : 1f) * ((float)Math.PI / 4f);
								fighterRetreatVector = AngleToV2(angle, 900f);
							}
						}
						else if (Vector2.Distance(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition()) > 700f)
						{
							isRetreating = false;
						}
					}
					Vector2 v3 = ((ForeverHelper.getClosestPlayer(position, playerList) == null) ? Vector2.Zero : (ForeverHelper.getClosestPlayer(position, playerList).getPosition() - position));
					v3.Normalize();
					Vector2 v4 = AngleToV2(rotation, 1f);
					v4.Normalize();
					GeometryHelper.UnsignedAngleBetweenTwoV2(v4, v3);
					fighterMomentum += AngleToV2(rotation, 0.07f);
					float length = fighterMomentum.Length();
					fighterMomentum = AngleToV2(rotation, length);
					position += fighterMomentum;
					fighterRotationMomentum *= 0.99f;
					fighterMomentum *= 0.99f;
					if (ForeverHelper.getClosestPlayer(position, playerList) != null)
					{
						permittedToFire = Vector2.Distance(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition()) < 500f;
					}
					break;
				}
				case 3:
				{
					cruiserRotationMomentum += 0.0001f;
					cruiserRotationMomentum = MathHelper.Clamp(cruiserRotationMomentum, 0f, 0.001f);
					if (position.X < 0f || position.Y < 0f || position.X > 2000f || position.Y > 2000f)
					{
						rotation = ForeverHelper.TurnToFace(position, new Vector2(1000f, 1000f), rotation, cruiserRotationMomentum);
					}
					else if (ForeverHelper.getClosestPlayer(position, playerList) != null)
					{
						rotation = ForeverHelper.TurnToFace(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition(), rotation, cruiserRotationMomentum);
					}
					Vector2 v = ((ForeverHelper.getClosestPlayer(position, playerList) == null) ? Vector2.Zero : (ForeverHelper.getClosestPlayer(position, playerList).getPosition() - position));
					v.Normalize();
					Vector2 v2 = AngleToV2(rotation, 1f);
					v2.Normalize();
					float num = GeometryHelper.UnsignedAngleBetweenTwoV2(v2, v);
					if (num < (float)Math.PI / 8f || float.IsNaN(num) || position.X < 0f || position.Y < 0f || position.X > 2000f || position.Y > 2000f)
					{
						cruiserMomentum += AngleToV2(rotation, 0.01f);
					}
					position += cruiserMomentum;
					cruiserRotationMomentum *= 0.99f;
					cruiserMomentum *= 0.99f;
					if (ForeverHelper.getClosestPlayer(position, playerList) != null)
					{
						permittedToFire = Vector2.Distance(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition()) < 500f;
						if (!permittedToFire && getHasBeenShot())
						{
							permittedToFire = true;
						}
					}
					else
					{
						permittedToFire = false;
					}
					break;
				}
				case 4:
					if (position.X < 0f || position.Y < 0f || position.X > 2000f || position.Y > 2000f)
					{
						rotation = ForeverHelper.TurnToFace(position, new Vector2(1000f, 1000f), rotation, 0.01f);
					}
					fighterMomentum = AngleToV2(rotation, 2f);
					position += fighterMomentum;
					fighterRotationMomentum *= 0.99f;
					fighterMomentum *= 0.99f;
					permittedToFire = ForeverHelper.getClosestPlayer(position, playerList) != null;
					break;
				case 5:
					rotation += 0.0005f;
					if (Vector2.Distance(position, new Vector2(1000f, 1000f)) > 30f)
					{
						fighterMomentum = AngleToV2(ForeverHelper.TurnToFace(position, new Vector2(1000f, 1000f), rotation, 7f), 1f);
					}
					else
					{
						fighterMomentum = Vector2.Zero;
					}
					position += fighterMomentum;
					fighterRotationMomentum *= 0.99f;
					fighterMomentum *= 0.99f;
					if (ForeverHelper.getClosestPlayer(position, playerList) != null)
					{
						permittedToFire = Vector2.Distance(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition()) < 500f;
						if (!permittedToFire && getHasBeenShot())
						{
							permittedToFire = Vector2.Distance(position, ForeverHelper.getClosestPlayer(position, playerList).getPosition()) < 700f;
						}
					}
					else
					{
						permittedToFire = false;
					}
					break;
				}
			}
			else
			{
				rotation = rotationOfParent;
				float angle2 = rotationOfParent + (float)Math.PI / 2f * (float)attachmentIndex;
				position = positionOfParent + AngleToV2(angle2, getShipModuleSizePixels(ShipType));
				switch (typeofShipModule)
				{
				case typeOfBlock.Link:
					drawOverrideRotation = rotation + (-(float)Math.PI / 2f + (float)(attachmentIndex - 1) * ((float)Math.PI / 2f));
					break;
				case typeOfBlock.Point:
					drawOverrideRotation = rotation + (-(float)Math.PI / 2f + (float)(attachmentIndex + 1) * ((float)Math.PI / 2f));
					break;
				case typeOfBlock.Thruster:
					drawOverrideRotation = rotation + (-(float)Math.PI / 2f + (float)(attachmentIndex - 1) * ((float)Math.PI / 2f));
					if (getShipModuleSizeisLarge(ShipType))
					{
						explosionManagerRef.addExplosion(position, 2f, explosionType.tinySmoke);
						explosionManagerRef.addExplosion(position, 1f, explosionType.tiny);
					}
					else
					{
						explosionManagerRef.addExplosion(position, 1f, explosionType.tinySmoke);
						explosionManagerRef.addExplosion(position, 0.4f, explosionType.tiny);
					}
					break;
				case typeOfBlock.Turret:
				{
					if (ShipType != 2)
					{
						ref Vector2 reference2 = ref turretPositionArray[0];
						reference2 = position + AngleToV2((float)Math.PI / 4f + rotation, 16f);
						if (permittedToFire)
						{
							turretRotationArray[0] = ForeverHelper.TurnToFace(turretPositionArray[0], ForeverHelper.getClosestPlayer(turretPositionArray[0], playerList).getPosition(), turretRotationArray[0], 1f);
						}
						if (!permittedToFire)
						{
							break;
						}
						smallTurretFiringCounter--;
						if (smallTurretFiringCounter >= 0)
						{
							break;
						}
						smallTurretFiringCounter = 8 + randomRef.Next(10);
						turretFiringOrderIndex++;
						if (turretFiringOrderIndex > 3)
						{
							turretFiringOrderIndex = 0;
							Vector2 v7 = ForeverHelper.getClosestPlayer(turretPositionArray[turretFiringOrderIndex], playerList).getPosition() - turretPositionArray[turretFiringOrderIndex];
							v7.Normalize();
							Vector2 v8 = AngleToV2(turretRotationArray[turretFiringOrderIndex], 1f);
							v8.Normalize();
							float num4 = GeometryHelper.UnsignedAngleBetweenTwoV2(v8, v7);
							if ((num4 < (float)Math.PI / 8f || float.IsNaN(num4)) && position.X > 0f && position.Y > 0f && position.X < 2000f && position.Y < 2000f)
							{
								eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.smallRound, position, turretRotationArray[turretFiringOrderIndex], randomRef, inNoneDamagingBullet: false, 1f, gridManager, inDistortion: true, null, isSilent: false));
							}
						}
						break;
					}
					for (int l = 0; l < 4; l++)
					{
						turretIsFiringArray[l] = false;
					}
					for (int m = 0; m < 4; m++)
					{
						ref Vector2 reference3 = ref turretPositionArray[m];
						reference3 = position + AngleToV2((float)Math.PI / 4f + (float)m * ((float)Math.PI / 2f) + rotation, 16f);
						if (permittedToFire)
						{
							turretRotationArray[m] = ForeverHelper.TurnToFace(turretPositionArray[m], ForeverHelper.getClosestPlayer(turretPositionArray[m], playerList).getPosition(), turretRotationArray[m], 0.07f);
						}
					}
					if (!permittedToFire)
					{
						break;
					}
					smallTurretFiringCounter--;
					if (smallTurretFiringCounter < 0)
					{
						smallTurretFiringCounter = 8;
						turretFiringOrderIndex++;
						if (turretFiringOrderIndex > 3)
						{
							turretFiringOrderIndex = 0;
						}
						Vector2 v9 = ForeverHelper.getClosestPlayer(turretPositionArray[turretFiringOrderIndex], playerList).getPosition() - turretPositionArray[turretFiringOrderIndex];
						v9.Normalize();
						Vector2 v10 = AngleToV2(turretRotationArray[turretFiringOrderIndex], 1f);
						v10.Normalize();
						float num5 = GeometryHelper.UnsignedAngleBetweenTwoV2(v10, v9);
						if ((num5 < (float)Math.PI / 8f || float.IsNaN(num5)) && position.X > 0f && position.Y > 0f && position.X < 2000f && position.Y < 2000f)
						{
							eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.smallRound, turretPositionArray[turretFiringOrderIndex], turretRotationArray[turretFiringOrderIndex], randomRef, inNoneDamagingBullet: false, 1f, gridManager, inDistortion: true, null, isSilent: false));
						}
					}
					break;
				}
				case typeOfBlock.Gun:
					rotation += (float)Math.PI / 2f + (float)(attachmentIndex - 1) * ((float)Math.PI / 2f);
					if (getShipModuleSizeisLarge(ShipType))
					{
						if (artPackFiringCounter != 60)
						{
							artPackFiringCounter++;
						}
						if (ForeverHelper.getClosestPlayer(position, playerList) != null && artPackFiringCounter == 60 && permittedToFire)
						{
							Vector2 v11 = ForeverHelper.getClosestPlayer(position, playerList).getPosition() - position;
							v11.Normalize();
							Vector2 v12 = AngleToV2(rotation, 1f);
							v12.Normalize();
							float num6 = GeometryHelper.UnsignedAngleBetweenTwoV2(v12, v11);
							if (num6 < (float)Math.PI / 8f || float.IsNaN(num6))
							{
								artPackFiringCounter = randomRef.Next(20) - 10;
								eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.Artillery, position, rotation, randomRef, inNoneDamagingBullet: false, 1f, gridManager, inDistortion: true, null, isSilent: false));
							}
						}
						break;
					}
					if (laserFiringCounter != 50)
					{
						laserFiringCounter++;
					}
					if (ForeverHelper.getClosestPlayer(position, playerList) != null && laserFiringCounter == 50 && permittedToFire)
					{
						Vector2 v13 = ForeverHelper.getClosestPlayer(position, playerList).getPosition() - position;
						v13.Normalize();
						Vector2 v14 = AngleToV2(rotation, 1f);
						v14.Normalize();
						float num7 = GeometryHelper.UnsignedAngleBetweenTwoV2(v14, v13);
						if (num7 < (float)Math.PI / 8f || float.IsNaN(num7))
						{
							laserFiringCounter = randomRef.Next(20) - 10;
							eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.Laser, position, rotation, randomRef, inNoneDamagingBullet: false, 1f, gridManager, inDistortion: true, null, isSilent: false));
						}
					}
					break;
				case typeOfBlock.RocketPack:
				{
					ref Vector2 reference4 = ref turretPositionArray[0];
					reference4 = position;
					if (permittedToFire)
					{
						turretRotationArray[0] = ForeverHelper.TurnToFace(turretPositionArray[0], ForeverHelper.getClosestPlayer(turretPositionArray[0], playerList).getPosition(), turretRotationArray[0], 0.04f);
					}
					if (!permittedToFire)
					{
						break;
					}
					rocketPackFiringCounter--;
					if (rocketPackFiringCounter < 0)
					{
						rocketPackFiringCounter = 70 + randomRef.Next(30);
						Vector2 v15 = ForeverHelper.getClosestPlayer(turretPositionArray[0], playerList).getPosition() - turretPositionArray[0];
						v15.Normalize();
						Vector2 v16 = AngleToV2(turretRotationArray[0], 1f);
						v16.Normalize();
						float num8 = GeometryHelper.UnsignedAngleBetweenTwoV2(v16, v15);
						if ((num8 < (float)Math.PI / 8f || float.IsNaN(num8)) && position.X > 0f && position.Y > 0f && position.X < 2000f && position.Y < 2000f)
						{
							Vector2 vector7 = AngleToV2(turretRotationArray[0] + (float)Math.PI / 2f, (float)randomRef.NextDouble() * 42f - 21f);
							eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.Rocket, turretPositionArray[0] + vector7, turretRotationArray[0], randomRef, inNoneDamagingBullet: false, 1f, gridManager, inDistortion: true, null, isSilent: false));
						}
					}
					break;
				}
				case typeOfBlock.LaserCannon:
				{
					ref Vector2 reference = ref turretPositionArray[0];
					reference = position;
					if (permittedToFire && laserCannonStatus == 0)
					{
						turretRotationArray[0] = ForeverHelper.TurnToFace(turretPositionArray[0], ForeverHelper.getClosestPlayer(turretPositionArray[0], playerList).getPosition(), turretRotationArray[0], 0.01f);
					}
					Vector2 vector4 = AngleToV2(turretRotationArray[0], 8f);
					switch (laserCannonStatus)
					{
					case 0:
						if (permittedToFire)
						{
							Vector2 v5 = ForeverHelper.getClosestPlayer(turretPositionArray[0], playerList).getPosition() - turretPositionArray[0];
							v5.Normalize();
							Vector2 v6 = AngleToV2(turretRotationArray[0], 1f);
							v6.Normalize();
							float num3 = GeometryHelper.UnsignedAngleBetweenTwoV2(v6, v5);
							if ((num3 < (float)Math.PI / 80f || float.IsNaN(num3)) && position.X > 0f && position.Y > 0f && position.X < 2000f && position.Y < 2000f)
							{
								laserCannonStatus = 1;
								laserCannonChargingTime = 120;
								beamInstanceImage = gridManager.generateBeamImage(turretRotationArray[0]);
								laserHasAppliedWarp = false;
							}
						}
						break;
					case 1:
						laserCannonChargingTime--;
						if (laserCannonChargingTime < 0)
						{
							laserCannonStatus = 2;
							laserCannonFiringTime = 190;
							ForeverHelper.soundManager.CreateGameSoundCue("geometryWars LaserCannon").Play();
							ForeverHelper.soundManager.CreateGameSoundCue("geometryWars LaserCannon").Play();
							ForeverHelper.soundManager.CreateGameSoundCue("geometryWars LaserCannon").Play();
							ForeverHelper.soundManager.CreateGameSoundCue("geometryWars LaserCannon").Play();
						}
						eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.LaserBlast, position + vector4, turretRotationArray[0], randomRef, inNoneDamagingBullet: true, 1f, gridManager, inDistortion: false, beamInstanceImage, isSilent: false));
						break;
					case 2:
					{
						laserCannonFiringTime--;
						if (laserCannonFiringTime < 0)
						{
							laserCannonStatus = 0;
						}
						float num2 = 20f;
						for (int k = 0; (float)k < 20f; k++)
						{
							if (!laserHasAppliedWarp)
							{
								Vector2 vector5 = AngleToV2(turretRotationArray[0] + (float)Math.PI / 2f, (float)k / 2f - 5f);
								eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.LaserBlast, position + vector5 + vector4, turretRotationArray[0], randomRef, inNoneDamagingBullet: false, (Math.Abs(((float)k - num2 / 2f) / (num2 / 2f)) - 1f) * -1f, gridManager, k == 10, beamInstanceImage, isSilent: false));
							}
							else
							{
								Vector2 vector6 = AngleToV2(turretRotationArray[0] + (float)Math.PI / 2f, (float)k / 2f - 5f);
								eBulletList.Add(new eBullet(graphicsDevice, contentManager, explosionManagerRef, null, typeOfEnemyBullet.LaserBlast, position + vector6 + vector4, turretRotationArray[0], randomRef, inNoneDamagingBullet: false, (Math.Abs(((float)k - num2 / 2f) / (num2 / 2f)) - 1f) * -1f, gridManager, inDistortion: false, beamInstanceImage, isSilent: false));
							}
						}
						laserHasAppliedWarp = true;
						break;
					}
					}
					break;
				}
				}
			}
			getLastHitByRef();
			collisionVolume = new BoundingSphere(new Vector3(position, 0f), primarySprite.Width / 2);
			if (typeofShipModule == typeOfBlock.BShield)
			{
				shieldCollisionVolume = new BoundingSphere(new Vector3(position, 0f), shieldRadius);
			}
			for (int n = 0; n < 4; n++)
			{
				if (attachedModules[n] != null)
				{
					attachedModules[n].Update(playerList, position, rotation, eBulletList, permittedToFire, pBulletList);
				}
			}
			if (moduleHealth < 0f)
			{
				setToDead(lastHitByReference, isRootExplosion: true);
			}
		}
		else if (m_aliveLastFrame)
		{
			m_aliveLastFrame = false;
		}
		if (typeofShipModule == typeOfBlock.core && !m_Alive && !m_aliveLastFrame)
		{
			ForeverHelper.soundManager.CreateGameSoundCue("geometryWars Explosion Large").Play();
			return true;
		}
		bool flag = true;
		if (typeofShipModule == typeOfBlock.core)
		{
			for (int num9 = 0; num9 < 4; num9++)
			{
				if (attachedModules[num9] != null && attachedModules[num9].getAlive())
				{
					flag = false;
				}
			}
			if (flag)
			{
				setToDead(lastHitByReference, isRootExplosion: true);
			}
		}
		return false;
	}

	public bool getAlive()
	{
		return m_Alive;
	}

	public void setToDead(playerShip shipRef, bool isRootExplosion)
	{
		if (m_Alive)
		{
			if (isRootExplosion)
			{
				Cue cue = ForeverHelper.soundManager.CreateGameSoundCue("geometryWars Explosion Large");
				cue.Play();
			}
			if (lastHitByReference != null && shipRef != null)
			{
				shipRef.blocksDestroyed++;
			}
			if (typeofShipModule == typeOfBlock.core && lastHitByReference != null)
			{
				lastHitByReference.addKill(shipName);
			}
			if (typeofShipModule == typeOfBlock.core)
			{
				explosionManagerRef.addExplosion(position, 1f, explosionType.large);
			}
			else
			{
				explosionManagerRef.addExplosion(position, 1f, explosionType.small);
			}
		}
		m_Alive = false;
		for (int i = 0; i < 4; i++)
		{
			if (attachedModules[i] != null)
			{
				attachedModules[i].setToDead(shipRef, isRootExplosion: false);
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 offset)
	{
		if (!m_Alive)
		{
			return;
		}
		switch (typeofShipModule)
		{
		case typeOfBlock.core:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(secondarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.Link:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, drawOverrideRotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.Point:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, drawOverrideRotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.Thruster:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, drawOverrideRotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.Turret:
		{
			if (ShipType != 2)
			{
				spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
				spriteBatch.Draw(secondarySprite, position + offset, null, Color.White, turretRotationArray[0], cellOrigin, 1f, SpriteEffects.None, 0f);
				break;
			}
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			for (int j = 0; j < 4; j++)
			{
				spriteBatch.Draw(secondarySprite, turretPositionArray[j] + offset, null, Color.White, turretRotationArray[j], smallOrigin, 1f, SpriteEffects.None, 0f);
			}
			break;
		}
		case typeOfBlock.Girder:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.Gun:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.RocketPack:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(secondarySprite, position + offset, null, Color.White, turretRotationArray[0], cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.Panel:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		case typeOfBlock.LaserCannon:
		{
			Color color = Color.Transparent;
			switch (laserCannonStatus)
			{
			case 0:
				color = Color.White;
				break;
			case 1:
				color = new Color(1f, (float)laserCannonChargingTime / 120f, (float)laserCannonChargingTime / 120f);
				break;
			case 2:
			{
				float num = (float)laserCannonFiringTime / 190f * -1f + 1f;
				color = new Color(1f, num, num);
				break;
			}
			}
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(secondarySprite, position + offset, null, color, turretRotationArray[0], cellOrigin, 1f, SpriteEffects.None, 0f);
			break;
		}
		case typeOfBlock.BShield:
			spriteBatch.Draw(primarySprite, position + offset, null, Color.White, rotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(secondarySprite, position + offset, null, Color.White, shieldRotation, cellOrigin, 1f, SpriteEffects.None, 0f);
			if (shieldHealth > 0f && ShipAiProfile != -1)
			{
				Circle circle = GeometryHelper.GenerateCircle((int)shieldRadius, 20, position + offset);
				GeometryHelper.GetCircleLines(circle, out lineList);
				lineArray = lineList.ToArray();
				spriteBatch.End();
				for (int i = 0; i < lineArray.Length; i++)
				{
					lineArray[i].Color = new Color(shieldAlpha, shieldAlpha, shieldAlpha, shieldAlpha);
				}
				graphicsDevice.BlendState = BlendState.AlphaBlend;
				lineRenderer.DrawShape(lineArray);
				spriteBatch.Begin();
			}
			break;
		}
		for (int k = 0; k < 4; k++)
		{
			if (attachedModules[k] != null)
			{
				attachedModules[k].Draw(spriteBatch, offset);
			}
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < 4; i++)
		{
			if (attachedModules[i] != null)
			{
				attachedModules[i].Dispose();
			}
		}
	}

	public Vector2 AngleToV2(float angle, float length)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(angle) * length;
		zero.Y = (float)Math.Sin(angle) * length;
		return zero;
	}
}
