using Microsoft.Xna.Framework;

namespace ISParticleEngine;

public struct ParticleParameters
{
	public string SpritePath;

	public Vector2 Origin;

	public float MinSpeed;

	public float MaxSpeed;

	public float MinDirection;

	public float MaxDirection;

	public float Multiplicative;

	public Vector2 Gravity;

	public float MinRotation;

	public float MaxRotation;

	public float MinAlpha;

	public float MaxAlpha;

	public float MinAlphaChange1;

	public float MaxAlphaChange1;

	public float MinAlphaChange2;

	public float MaxAlphaChange2;

	public Vector2 MinScale;

	public Vector2 MaxScale;

	public Vector2 MinScaleChange1;

	public Vector2 MaxScaleChange1;

	public Vector2 MinScaleChange2;

	public Vector2 MaxScaleChange2;

	public Vector3 MinColor;

	public Vector3 MaxColor;

	public Vector3 MinColorChange1;

	public Vector3 MaxColorChange1;

	public Vector3 MinColorChange2;

	public Vector3 MaxColorChange2;

	public int Change;

	public bool Directional;

	public bool CanRotate;
}
