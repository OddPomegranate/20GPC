using ISParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FiftyGames.Shooter;

internal class CustomParticleDescriptor : ParticleDescriptor
{
	public CustomParticleDescriptor(ContentManager contentManager)
		: base(contentManager)
	{
	}

	public override string GetSpritePath()
	{
		return "Shooter/Particles/ParticleSmoke";
	}

	public override float GetMinSpeed()
	{
		return 4f;
	}

	public override float GetMaxSpeed()
	{
		return 8f;
	}

	public override float GetMinDirection()
	{
		return -45f;
	}

	public override float GetMaxDirection()
	{
		return 45f;
	}

	public override float GetMaxMultiplicative()
	{
		return 0.95f;
	}

	public override Vector2 GetGravity()
	{
		return Vector2.Zero;
	}

	public override float GetMinRotation()
	{
		return 0f;
	}

	public override float GetMaxRotation()
	{
		return 0f;
	}

	public override float GetMinAlpha()
	{
		return 0.2f;
	}

	public override float GetMaxAlpha()
	{
		return 0.8f;
	}

	public override float GetMinAlphaChange1()
	{
		return 0f;
	}

	public override float GetMaxAlphaChange1()
	{
		return 0f;
	}

	public override float GetMinAlphaChange2()
	{
		return -0.05f;
	}

	public override float GetMaxAlphaChange2()
	{
		return -0.05f;
	}

	public override Vector2 GetOrigin()
	{
		return new Vector2((float)base.Sprite.Width / 2f, (float)base.Sprite.Height / 2f);
	}

	public override Vector2 GetMinScale()
	{
		return Vector2.Zero;
	}

	public override Vector2 GetMaxScale()
	{
		return Vector2.One;
	}

	public override Vector2 GetMinScaleChange1()
	{
		return Vector2.Zero;
	}

	public override Vector2 GetMaxScaleChange1()
	{
		return Vector2.Zero;
	}

	public override Vector2 GetMinScaleChange2()
	{
		return Vector2.Zero;
	}

	public override Vector2 GetMaxScaleChange2()
	{
		return Vector2.Zero;
	}

	public override Vector3 GetMinColor()
	{
		return Color.White.ToVector3();
	}

	public override Vector3 GetMaxColor()
	{
		return Color.White.ToVector3();
	}

	public override Vector3 GetMinColorChange1()
	{
		return Vector3.Zero;
	}

	public override Vector3 GetMaxColorChange1()
	{
		return -Vector3.One * 0.05f;
	}

	public override Vector3 GetMinColorChange2()
	{
		return Vector3.Zero;
	}

	public override Vector3 GetMaxColorChange2()
	{
		return Vector3.Zero;
	}

	public override int GetChange()
	{
		return 8;
	}

	public override bool GetIsDirectional()
	{
		return true;
	}
}
