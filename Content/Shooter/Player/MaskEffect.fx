// Shooter/Player/MaskEffect.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// the 20GPC project's ForeverWars shader work for why this rewrite-from-
// scratch approach was necessary -- no original .fx source survived).
//
// Usage (from Shooter.Entities.ShooterPlayer.GenerateMaskedBar): applied
// via SpriteBatch as a custom effect drawing the full health/ammo bar
// texture (bound automatically as the primary draw texture), with a
// second parameter:
//   TextureTwo - a render target holding the SAME bar texture, pre-drawn
//                rotated around its own center by an angle proportional to
//                the remaining health/ammo fraction (PI at 0%, 0 at 100%).
// Combining the primary texture's color with TextureTwo's alpha as a
// stencil produces a radial "pie wipe" reveal -- the classic rotating-wedge
// depletion bar look (like a clock hand sweeping away the used portion).
// Best-effort recreation; original visual not recoverable byte-exact.

float4x4 MatrixTransform;

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture TextureTwo;
sampler TextureTwoSampler = sampler_state
{
	Texture = (TextureTwo);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput SpriteVertexShader(float4 position : POSITION0, float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{
	VertexShaderOutput output;
	output.Position = mul(position, MatrixTransform);
	output.Color = color;
	output.TexCoord = texCoord;
	return output;
}

float4 MaskPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 primary = tex2D(TextureSampler, input.TexCoord) * input.Color;
	float4 mask = tex2D(TextureTwoSampler, input.TexCoord);
	primary.a *= mask.a;
	return primary;
}

technique Mask
{
	pass P0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 MaskPixelShader();
	}
}
