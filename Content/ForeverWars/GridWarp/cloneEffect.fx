// ForeverWars/GridWarp/cloneEffect.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// gridShader.fx for the full explanation of why these had to be rewritten
// from scratch rather than converted).
//
// NOTE: like gridShader, `cloneEffect` is loaded by
// FiftyGames.ForeverWars.gridSystem but never actually applied anywhere in
// the code -- appears to be leftover/unused. Minimal passthrough SpriteBatch
// effect so Content.Load<Effect> succeeds.

texture InputTexture;
sampler InputSampler = sampler_state
{
	Texture = (InputTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float4x4 MatrixTransform;

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

float4 SpritePixelShader(VertexShaderOutput input) : COLOR0
{
	return tex2D(InputSampler, input.TexCoord) * input.Color;
}

technique SpriteBatch
{
	pass P0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}
