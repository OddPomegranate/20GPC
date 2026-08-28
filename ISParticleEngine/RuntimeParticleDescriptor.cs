using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ISParticleEngine;

public class RuntimeParticleDescriptor : ParticleDescriptor
{
	private ParticleParameters _particleParameters;

	public RuntimeParticleDescriptor(Texture2D sprite, ParticleParameters parameters)
		: base(sprite)
	{
		_particleParameters = parameters;
	}

	public override string GetSpritePath()
	{
		return _particleParameters.SpritePath;
	}

	public override float GetMinSpeed()
	{
		return _particleParameters.MinSpeed;
	}

	public override float GetMaxSpeed()
	{
		return _particleParameters.MaxSpeed;
	}

	public override float GetMinDirection()
	{
		return _particleParameters.MinDirection;
	}

	public override float GetMaxDirection()
	{
		return _particleParameters.MaxDirection;
	}

	public override float GetMaxMultiplicative()
	{
		return _particleParameters.Multiplicative;
	}

	public override Vector2 GetGravity()
	{
		return _particleParameters.Gravity;
	}

	public override float GetMinRotation()
	{
		return _particleParameters.MinRotation;
	}

	public override float GetMaxRotation()
	{
		return _particleParameters.MaxRotation;
	}

	public override float GetMinAlpha()
	{
		return _particleParameters.MinAlpha;
	}

	public override float GetMaxAlpha()
	{
		return _particleParameters.MaxAlpha;
	}

	public override float GetMinAlphaChange1()
	{
		return _particleParameters.MinAlphaChange1;
	}

	public override float GetMaxAlphaChange1()
	{
		return _particleParameters.MaxAlphaChange1;
	}

	public override float GetMinAlphaChange2()
	{
		return _particleParameters.MinAlphaChange2;
	}

	public override float GetMaxAlphaChange2()
	{
		return _particleParameters.MaxAlphaChange2;
	}

	public override Vector2 GetOrigin()
	{
		return _particleParameters.Origin;
	}

	public override Vector2 GetMinScale()
	{
		return _particleParameters.MinScale;
	}

	public override Vector2 GetMaxScale()
	{
		return _particleParameters.MaxScale;
	}

	public override Vector2 GetMinScaleChange1()
	{
		return _particleParameters.MinScaleChange1;
	}

	public override Vector2 GetMaxScaleChange1()
	{
		return _particleParameters.MaxScaleChange1;
	}

	public override Vector2 GetMinScaleChange2()
	{
		return _particleParameters.MinScaleChange2;
	}

	public override Vector2 GetMaxScaleChange2()
	{
		return _particleParameters.MaxScaleChange2;
	}

	public override Vector3 GetMinColor()
	{
		return _particleParameters.MinColor;
	}

	public override Vector3 GetMaxColor()
	{
		return _particleParameters.MaxColor;
	}

	public override Vector3 GetMinColorChange1()
	{
		return _particleParameters.MinColorChange1;
	}

	public override Vector3 GetMaxColorChange1()
	{
		return _particleParameters.MaxColorChange1;
	}

	public override Vector3 GetMinColorChange2()
	{
		return _particleParameters.MinColorChange2;
	}

	public override Vector3 GetMaxColorChange2()
	{
		return _particleParameters.MaxColorChange2;
	}

	public override int GetChange()
	{
		return _particleParameters.Change;
	}

	public override bool GetIsDirectional()
	{
		return _particleParameters.Directional;
	}
}
