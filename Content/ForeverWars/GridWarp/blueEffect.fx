// ForeverWars/GridWarp/blueEffect.fx
//
// RECONSTRUCTED (see gridShader.fx for why). Used by gridSystem to render a
// weaker/alternate-intensity copy of the "bullet" stamp sprite, tinting it
// toward blue as blueOverride drops from 1 (full original color) toward 0
// (fully blue tint), for lower-intensity warp events. Best-effort
// recreation of intent from the parameter name; original visual is not
// recoverable byte-exact.

texture InputTexture;
sampler InputSampler = sampler_state
{
	Texture = (InputTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float blueOverride;

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
};

// Applied via a raw full-screen quad (fullScreenQuad.Render), not
// SpriteBatch -- positions arrive already in clip space (-1..1), no
// transform matrix needed. Vertex format is VertexPositionTexture (no
// per-vertex color).
VertexShaderOutput QuadVertexShader(float4 position : POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexShaderOutput output;
	output.Position = float4(position.xy, 0, 1);
	output.TexCoord = texCoord;
	return output;
}

float4 BlueOverridePixelShader(VertexShaderOutput input) : COLOR0
{
	float4 baseColor = tex2D(InputSampler, input.TexCoord);
	float3 blueTint = float3(0.25, 0.45, 1.0) * baseColor.a;
	float3 finalColor = lerp(blueTint, baseColor.rgb, blueOverride);
	return float4(finalColor, baseColor.a);
}

technique BlueOverride
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 BlueOverridePixelShader();
	}
}
