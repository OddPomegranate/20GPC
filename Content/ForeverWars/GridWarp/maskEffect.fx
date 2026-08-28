// ForeverWars/GridWarp/maskEffect.fx
//
// RECONSTRUCTED (see gridShader.fx for why). Combines a base texture
// ("Texture", the "bullet" glow sprite) with a mask texture ("Texture2", a
// white background with a rotated directional strip/wedge shape drawn over
// it) to cut the base texture down to that directional wedge shape -- used
// by gridSystem.generateBeamImage/generateDirectionalImage to build
// directional "beam" images. Interpreted as: dark (low-luminance) regions
// of the mask let the base texture through, white background areas cut it
// to transparent. Best-effort recreation; original visual not recoverable
// byte-exact.

texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Texture2;
sampler Texture2Sampler = sampler_state
{
	Texture = (Texture2);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
};

// Applied via a raw full-screen quad (fullScreenQuad.Render), not
// SpriteBatch -- positions arrive already in clip space, no transform
// matrix needed. Vertex format is VertexPositionTexture (no vertex color).
VertexShaderOutput QuadVertexShader(float4 position : POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexShaderOutput output;
	output.Position = float4(position.xy, 0, 1);
	output.TexCoord = texCoord;
	return output;
}

float4 MaskOverlayPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 baseColor = tex2D(TextureSampler, input.TexCoord);
	float4 maskColor = tex2D(Texture2Sampler, input.TexCoord);
	float maskLuminance = dot(maskColor.rgb, float3(0.299, 0.587, 0.114));
	float keep = 1.0 - maskLuminance;
	return float4(baseColor.rgb, baseColor.a * keep);
}

technique MaskOverlay
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 MaskOverlayPixelShader();
	}
}
