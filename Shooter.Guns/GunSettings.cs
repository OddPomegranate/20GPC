using System.IO;

namespace Shooter.Guns;

internal class GunSettings
{
	public string Name { get; set; }

	public string SpritePath { get; set; }

	public int ShotsPerMin { get; set; }

	public int MagazineSize { get; set; }

	public int OffsetX { get; set; }

	public int OffsetY { get; set; }

	public float Recoil { get; set; }

	public float SpreadDegrees { get; set; }

	public bool IsAutomatic { get; set; }

	public bool IsLaser { get; set; }

	public int ProjectileCount { get; set; }

	public int ProjectileType { get; set; }

	public bool RandomSpray { get; set; }

	public bool IsSmallGun { get; set; }

	public int DamageOnHit { get; set; }

	public int ShotLength { get; set; }

	public int ProjectileSpread { get; set; }

	public int ForceOnWorldObjects { get; set; }

	public string MuzzlePath { get; set; }

	public int MuzzleOffsetX { get; set; }

	public int MuzzleOffsetY { get; set; }

	public int MuzzleSpriteOffsetX { get; set; }

	public int MuzzleSpriteOffsetY { get; set; }

	public string SoundEffectPath { get; set; }

	public static GunSettings LoadFromStream(Stream stream)
	{
		GunSettings gunSettings = new GunSettings();
		BinaryReader binaryReader = new BinaryReader(stream);
		gunSettings.Name = binaryReader.ReadString();
		gunSettings.SpritePath = binaryReader.ReadString();
		gunSettings.ShotsPerMin = binaryReader.ReadInt32();
		gunSettings.MagazineSize = binaryReader.ReadInt32();
		gunSettings.OffsetX = binaryReader.ReadInt32();
		gunSettings.OffsetY = binaryReader.ReadInt32();
		gunSettings.Recoil = binaryReader.ReadSingle();
		gunSettings.SpreadDegrees = binaryReader.ReadSingle();
		gunSettings.IsAutomatic = binaryReader.ReadBoolean();
		gunSettings.IsLaser = binaryReader.ReadBoolean();
		gunSettings.ProjectileCount = binaryReader.ReadInt32();
		gunSettings.ProjectileType = binaryReader.ReadInt32();
		gunSettings.IsSmallGun = binaryReader.ReadBoolean();
		gunSettings.DamageOnHit = binaryReader.ReadInt32();
		gunSettings.ShotLength = binaryReader.ReadInt32();
		gunSettings.ForceOnWorldObjects = binaryReader.ReadInt32();
		gunSettings.MuzzlePath = binaryReader.ReadString();
		gunSettings.MuzzleOffsetX = binaryReader.ReadInt32();
		gunSettings.MuzzleOffsetY = binaryReader.ReadInt32();
		gunSettings.RandomSpray = binaryReader.ReadBoolean();
		gunSettings.ProjectileSpread = binaryReader.ReadInt32();
		gunSettings.MuzzleSpriteOffsetX = binaryReader.ReadInt32();
		gunSettings.MuzzleSpriteOffsetY = binaryReader.ReadInt32();
		gunSettings.SoundEffectPath = binaryReader.ReadString();
		return gunSettings;
	}

	public static void SaveToStream(GunSettings settings, Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(settings.Name);
		binaryWriter.Write(settings.SpritePath);
		binaryWriter.Write(settings.ShotsPerMin);
		binaryWriter.Write(settings.MagazineSize);
		binaryWriter.Write(settings.OffsetX);
		binaryWriter.Write(settings.OffsetY);
		binaryWriter.Write(settings.Recoil);
		binaryWriter.Write(settings.SpreadDegrees);
		binaryWriter.Write(settings.IsAutomatic);
		binaryWriter.Write(settings.IsLaser);
		binaryWriter.Write(settings.ProjectileCount);
		binaryWriter.Write(settings.ProjectileType);
		binaryWriter.Write(settings.IsSmallGun);
		binaryWriter.Write(settings.DamageOnHit);
		binaryWriter.Write(settings.ShotLength);
		binaryWriter.Write(settings.ForceOnWorldObjects);
		binaryWriter.Write(settings.MuzzlePath);
		binaryWriter.Write(settings.MuzzleOffsetX);
		binaryWriter.Write(settings.MuzzleOffsetY);
		binaryWriter.Write(settings.RandomSpray);
		binaryWriter.Write(settings.ProjectileSpread);
		binaryWriter.Write(settings.MuzzleSpriteOffsetX);
		binaryWriter.Write(settings.MuzzleSpriteOffsetY);
		binaryWriter.Write(settings.SoundEffectPath);
	}

	public static GunSettings Copy(GunSettings sourceSettings)
	{
		GunSettings gunSettings = new GunSettings();
		gunSettings.Name = sourceSettings.Name;
		gunSettings.SpritePath = sourceSettings.SpritePath;
		gunSettings.ShotsPerMin = sourceSettings.ShotsPerMin;
		gunSettings.OffsetX = sourceSettings.OffsetX;
		gunSettings.OffsetY = sourceSettings.OffsetY;
		gunSettings.Recoil = sourceSettings.Recoil;
		gunSettings.SpreadDegrees = sourceSettings.SpreadDegrees;
		gunSettings.MagazineSize = sourceSettings.MagazineSize;
		gunSettings.IsAutomatic = sourceSettings.IsAutomatic;
		gunSettings.IsLaser = sourceSettings.IsLaser;
		gunSettings.ProjectileCount = sourceSettings.ProjectileCount;
		gunSettings.ProjectileType = sourceSettings.ProjectileType;
		gunSettings.IsSmallGun = sourceSettings.IsSmallGun;
		gunSettings.DamageOnHit = sourceSettings.DamageOnHit;
		gunSettings.ShotLength = sourceSettings.ShotLength;
		gunSettings.ForceOnWorldObjects = sourceSettings.ForceOnWorldObjects;
		gunSettings.MuzzlePath = sourceSettings.MuzzlePath;
		gunSettings.MuzzleOffsetX = sourceSettings.MuzzleOffsetX;
		gunSettings.MuzzleOffsetY = sourceSettings.MuzzleOffsetY;
		gunSettings.RandomSpray = sourceSettings.RandomSpray;
		gunSettings.ProjectileSpread = sourceSettings.ProjectileSpread;
		gunSettings.MuzzleSpriteOffsetX = sourceSettings.MuzzleSpriteOffsetX;
		gunSettings.MuzzleSpriteOffsetY = sourceSettings.MuzzleSpriteOffsetY;
		gunSettings.SoundEffectPath = sourceSettings.SoundEffectPath;
		return gunSettings;
	}
}
