// ForeverWars/Effects/gridShader.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect. The
// original was GPU shader bytecode compiled for the Xbox 360's hardware,
// which MonoGame's DesktopGL/OpenGL backend cannot load at all ("This does
// not appear to be a MonoGame MGFX file!"). No original .fx (HLSL) source
// survived, so this was rewritten from scratch based on how the game code
// uses it.
//
// NOTE: as of this rewrite, `gridShader` is loaded by
// FiftyGames.ForeverWars.gridSystem but is never actually applied anywhere
// in the code (no Parameters.SetValue / CurrentTechnique.Passes[].Apply
// calls reference it) -- it appears to be leftover/unused from the original
// game. This is a minimal passthrough SpriteBatch effect so the
// Content.Load<Effect> call succeeds; its exact behavior doesn't matter
// since it's never drawn with.

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
