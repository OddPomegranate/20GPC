// ForeverWars/GridWarp/diminishEffect.fx
//
// RECONSTRUCTED (see gridShader.fx for why). Applies an exponential fade to
// a rendered texture each frame: gridSystem.Draw() uses this to fade the
// accumulated "bullet heat" render target so bullet trails/warps fade out
// over time rather than persisting forever. diminishValue is the per-frame
// fade amount; anything that has faded below visualCutoff is fully cleared
// to avoid persistent low-level color creep across frames.

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
float diminishValue;
float visualCutoff;

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

float4 DiminishPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(InputSampler, input.TexCoord) * input.Color;
	color *= saturate(1.0 - diminishValue);
	if (color.a < visualCutoff)
	{
		color = float4(0, 0, 0, 0);
	}
	return color;
}

technique Diminish
{
	pass P0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 DiminishPixelShader();
	}
}
