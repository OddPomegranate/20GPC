// RiskyRiskyRisk/Effects/InnerGlow.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// the 20GPC project's ForeverWars shader work for why this rewrite-from-
// scratch approach was necessary -- no original .fx source survived).
//
// Usage (from FiftyGames.RiskyRiskyRisk.Country.CreateTexture): applied via
// a raw full-screen quad (fsq.Render), not SpriteBatch -- positions arrive
// already in clip space, no transform matrix needed. Builds a country's
// on-screen shape (a cluster of hexagon dice tiles rendered to a small
// RenderTarget2D silhouette) in three passes, each selecting a different
// technique by name and sharing three parameters:
//   Color       (float4) - a tint/outline color, alpha channel used as an
//                           on/off switch by the pass (0 in the InnerGlow
//                           and Normal passes here, opaque black (0,0,0,1)
//                           in the Outline pass).
//   Texture     (texture)  - the source to read this pass.
//   TextureSize (float2)   - source dimensions, for computing a 1-texel
//                           offset for neighbor sampling.
// Techniques:
//   "InnerGlow" - reads the flat hexagon silhouette, adds a soft glow just
//                 inside the shape's edges (interior only, fades toward
//                 the center) for a subtle rim-lit look.
//   "Normal"    - straightforward passthrough of the glow-pass result,
//                 with Color/Color.a available to tint the whole image if
//                 a non-zero alpha is ever passed (unused in the observed
//                 call site, where Color.a is always 0 here).
//   "Outline"   - alpha edge-detection: draws Color solidly just outside
//                 the shape's silhouette (where a neighboring texel has
//                 alpha but the current texel doesn't), for a hard outline
//                 ring around the country shape.
// Best-effort recreation of this three-pass "glowing outlined country
// silhouette" pipeline; original not recoverable byte-exact.

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float4 Color;
float2 TextureSize;

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
};

// Applied via a raw full-screen quad (fullScreenQuad-style helper), not
// SpriteBatch -- positions arrive already in clip space (-1..1).
VertexShaderOutput QuadVertexShader(float4 position : POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexShaderOutput output;
	output.Position = float4(position.xy, 0, 1);
	output.TexCoord = texCoord;
	return output;
}

float4 InnerGlowPixelShader(VertexShaderOutput input) : COLOR0
{
	float2 texel = 1.0 / TextureSize;
	float4 center = tex2D(TextureSampler, input.TexCoord);

	float edge = 0;
	edge += 1.0 - tex2D(TextureSampler, input.TexCoord + float2(texel.x * 2, 0)).a;
	edge += 1.0 - tex2D(TextureSampler, input.TexCoord - float2(texel.x * 2, 0)).a;
	edge += 1.0 - tex2D(TextureSampler, input.TexCoord + float2(0, texel.y * 2)).a;
	edge += 1.0 - tex2D(TextureSampler, input.TexCoord - float2(0, texel.y * 2)).a;
	edge *= 0.25;

	// Only glow inside the shape (center.a > 0), stronger near the edges.
	float glow = edge * center.a * 0.6;
	float4 result = center + float4(1, 1, 1, 1) * glow;
	result.a = center.a;
	return result;
}

float4 NormalPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TexCoord);
	color.rgb = lerp(color.rgb, Color.rgb, Color.a);
	return color;
}

float4 OutlinePixelShader(VertexShaderOutput input) : COLOR0
{
	float2 texel = 1.0 / TextureSize;
	float4 center = tex2D(TextureSampler, input.TexCoord);

	float neighborAlpha = 0;
	neighborAlpha = max(neighborAlpha, tex2D(TextureSampler, input.TexCoord + float2(texel.x, 0)).a);
	neighborAlpha = max(neighborAlpha, tex2D(TextureSampler, input.TexCoord - float2(texel.x, 0)).a);
	neighborAlpha = max(neighborAlpha, tex2D(TextureSampler, input.TexCoord + float2(0, texel.y)).a);
	neighborAlpha = max(neighborAlpha, tex2D(TextureSampler, input.TexCoord - float2(0, texel.y)).a);

	float outline = saturate(neighborAlpha - center.a);
	float4 outlineColor = float4(Color.rgb, 1) * outline;
	return lerp(outlineColor, center, center.a);
}

technique InnerGlow
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 InnerGlowPixelShader();
	}
}

technique Normal
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 NormalPixelShader();
	}
}

technique Outline
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 OutlinePixelShader();
	}
}
