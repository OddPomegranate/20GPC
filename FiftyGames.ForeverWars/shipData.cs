using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FiftyGames.ForeverWars;

internal class shipData
{
	public Texture2D sCore;

	public Texture2D sLink;

	public Texture2D sPoint;

	public Texture2D sThruster;

	public Texture2D sTurret;

	public Texture2D sGirder;

	public Texture2D sGun;

	public Texture2D sPanel;

	public Texture2D sBShield;

	public Texture2D lCore;

	public Texture2D lRocketPack;

	public Texture2D lLaserCannon;

	public Texture2D lPoint;

	public Texture2D lThruster;

	public Texture2D lGirder;

	public Texture2D lGun;

	public Texture2D lBShield;

	public shipData(ContentManager contentManager)
	{
		sCore = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Life");
		sLink = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Link");
		sPoint = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Point");
		sThruster = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Thruster");
		sTurret = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Turret");
		sGirder = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Block");
		sGun = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Gun");
		sPanel = contentManager.Load<Texture2D>("ForeverWars/Sprites/32Panel");
		sBShield = contentManager.Load<Texture2D>("ForeverWars/Sprites/32BShield");
		lCore = contentManager.Load<Texture2D>("ForeverWars/Sprites/48Life");
		lRocketPack = contentManager.Load<Texture2D>("ForeverWars/Sprites/48RocketLauncher");
		lLaserCannon = contentManager.Load<Texture2D>("ForeverWars/Sprites/48LaserCannon");
		lPoint = contentManager.Load<Texture2D>("ForeverWars/Sprites/48Point");
		lThruster = contentManager.Load<Texture2D>("ForeverWars/Sprites/48Thruster");
		lGirder = contentManager.Load<Texture2D>("ForeverWars/Sprites/48Block");
		lGun = contentManager.Load<Texture2D>("ForeverWars/Sprites/48Gun");
		lBShield = contentManager.Load<Texture2D>("ForeverWars/Sprites/48BShield");
	}
}
