// ForeverWars/Effects/borderShader.fx
//
// RECONSTRUCTED (see gridShader.fx for why). Draws the composed play-field
// texture (InputTexture) with a border/vignette highlight of thickness
// borderThickness near the edges of a maxWidth x maxHeight field. Applied
// via a raw full-screen quad, not SpriteBatch. Best-effort recreation;
// original visual not recoverable byte-exact.

texture InputTexture;
sampler InputSampler = sampler_state
{
	Texture = (InputTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float borderThickness;
float maxWidth;
float maxHeight;

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput QuadVertexShader(float4 position : POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexShaderOutput output;
	output.Position = float4(position.xy, 0, 1);
	output.TexCoord = texCoord;
	return output;
}

float4 BorderPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(InputSampler, input.TexCoord);

	float2 pixelPos = input.TexCoord * float2(maxWidth, maxHeight);
	float distFromEdge = min(min(pixelPos.x, maxWidth - pixelPos.x),
	                          min(pixelPos.y, maxHeight - pixelPos.y));

	float borderAmount = 1.0 - saturate(distFromEdge / max(borderThickness, 1.0));
	float3 borderColor = float3(0.4, 0.7, 1.0);

	color.rgb = lerp(color.rgb, borderColor, borderAmount * 0.6);
	color.a = max(color.a, borderAmount * 0.6);
	return color;
}

technique Border
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 BorderPixelShader();
	}
}
