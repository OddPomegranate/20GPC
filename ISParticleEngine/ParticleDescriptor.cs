using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ISParticleEngine;

public abstract class ParticleDescriptor : IDisposable
{
	private Texture2D _sprite;

	public Texture2D Sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			_sprite = value;
		}
	}

	public ParticleDescriptor(ContentManager contentManager)
	{
		_sprite = contentManager.Load<Texture2D>(GetSpritePath());
	}

	public ParticleDescriptor(ContentManager contentManager, string spritePath)
	{
		_sprite = contentManager.Load<Texture2D>(spritePath);
	}

	public ParticleDescriptor(Texture2D sprite)
	{
		_sprite = sprite;
	}

	public abstract string GetSpritePath();

	public abstract float GetMinSpeed();

	public abstract float GetMaxSpeed();

	public abstract float GetMinDirection();

	public abstract float GetMaxDirection();

	public abstract float GetMaxMultiplicative();

	public abstract Vector2 GetGravity();

	public abstract float GetMinRotation();

	public abstract float GetMaxRotation();

	public abstract float GetMinAlpha();

	public abstract float GetMaxAlpha();

	public abstract float GetMinAlphaChange1();

	public abstract float GetMaxAlphaChange1();

	public abstract float GetMinAlphaChange2();

	public abstract float GetMaxAlphaChange2();

	public abstract Vector2 GetOrigin();

	public abstract Vector2 GetMinScale();

	public abstract Vector2 GetMaxScale();

	public abstract Vector2 GetMinScaleChange1();

	public abstract Vector2 GetMaxScaleChange1();

	public abstract Vector2 GetMinScaleChange2();

	public abstract Vector2 GetMaxScaleChange2();

	public abstract Vector3 GetMinColor();

	public abstract Vector3 GetMaxColor();

	public abstract Vector3 GetMinColorChange1();

	public abstract Vector3 GetMaxColorChange1();

	public abstract Vector3 GetMinColorChange2();

	public abstract Vector3 GetMaxColorChange2();

	public abstract int GetChange();

	public abstract bool GetIsDirectional();

	public void Dispose()
	{
		_sprite.Dispose();
	}

	public ParticleParameters ToParticleParameters()
	{
		return new ParticleParameters
		{
			Change = GetChange(),
			Directional = GetIsDirectional(),
			Gravity = GetGravity(),
			Multiplicative = GetMaxMultiplicative(),
			Origin = GetOrigin(),
			SpritePath = GetSpritePath(),
			MaxAlpha = GetMaxAlpha(),
			MaxAlphaChange1 = GetMaxAlphaChange1(),
			MaxAlphaChange2 = GetMaxAlphaChange2(),
			MaxColor = GetMaxColor(),
			MaxColorChange1 = GetMaxColorChange1(),
			MaxColorChange2 = GetMaxColorChange2(),
			MaxDirection = GetMaxDirection(),
			MaxRotation = GetMaxRotation(),
			MaxScale = GetMaxScale(),
			MaxScaleChange1 = GetMaxScaleChange1(),
			MaxScaleChange2 = GetMaxScaleChange2(),
			MaxSpeed = GetMaxSpeed(),
			MinAlpha = GetMinAlpha(),
			MinAlphaChange1 = GetMinAlphaChange1(),
			MinAlphaChange2 = GetMinAlphaChange2(),
			MinColor = GetMinColor(),
			MinColorChange1 = GetMinColorChange1(),
			MinColorChange2 = GetMinColorChange2(),
			MinDirection = GetMinDirection(),
			MinRotation = GetMinRotation(),
			MinScale = GetMinScale(),
			MinScaleChange1 = GetMinScaleChange1(),
			MinScaleChange2 = GetMinScaleChange2(),
			MinSpeed = GetMinSpeed()
		};
	}
}
