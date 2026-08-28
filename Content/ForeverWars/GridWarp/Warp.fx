// ForeverWars/GridWarp/Warp.fx
//
// RECONSTRUCTED. Original was Xbox 360 GPU shader bytecode; no .fx source
// survived (see gridShader.fx for the full explanation). This is the core
// "grid warp" visual for ForeverWars: gridSystem.Draw() draws the grid
// texture (rawGridRT) through this effect, while a second texture
// (Texture2 = bulletTempRT, an accumulating, fading "heat" field stamped
// wherever bullets/warp events have occurred) is used to visually displace
// the grid near those points, based on the local gradient of the heat
// field (a standard heightfield -> screen-space-distortion technique).
// This is a best-effort recreation of the intended "grid ripples around
// bullets" effect, not a byte-exact recovery of the original.
//
// FIX (round 4 diagnostic): this effect was previously applied via
// SpriteBatch (spriteBatch.Begin(..., this effect); spriteBatch.Draw(...)),
// matching several other reconstructed effects in this project
// (FinalPassEffect.fx, ScreenEffect.fx, MaskEffect.fx). Render-target
// diagnostics proved that pattern produces 100% solid black output project
// -wide, even for a test shader that unconditionally returned solid red
// with no texture reads at all -- meaning SpriteBatch's custom-effect path
// itself was never actually invoking the shader/reaching the output. Every
// OTHER custom effect in this project that actually works on screen
// (GridShader.fx, InnerGlow.fx, DecalManagerEffect.fx, maskEffect.fx,
// ShadowHelper2DEffect.fx's ApplyShadowMap technique) is instead applied
// via effect.CurrentTechnique.Passes[0].Apply() followed by a raw
// full-screen quad draw (fullScreenQuad.Render), which takes vertex
// positions already in clip space and needs no MatrixTransform/vertex
// color. Rewritten to match that proven-working pattern: the vertex
// shader below now matches GridShader.fx's QuadVertexShader exactly
// (POSITION0 + TEXCOORD0, no COLOR0, no MatrixTransform), and
// gridSystem.cs's draw calls were switched to quad.Render() to match.

texture Texture2;
sampler Texture2Sampler = sampler_state
{
	Texture = (Texture2);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

static const float2 TexelSize = float2(1.0 / 1280.0, 1.0 / 720.0);
static const float WarpStrength = 0.06;

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

float4 WarpPixelShader(VertexShaderOutput input) : COLOR0
{
	float hL = tex2D(Texture2Sampler, input.TexCoord - float2(TexelSize.x, 0)).r;
	float hR = tex2D(Texture2Sampler, input.TexCoord + float2(TexelSize.x, 0)).r;
	float hU = tex2D(Texture2Sampler, input.TexCoord - float2(0, TexelSize.y)).r;
	float hD = tex2D(Texture2Sampler, input.TexCoord + float2(0, TexelSize.y)).r;

	float2 displacement = float2(hR - hL, hD - hU) * WarpStrength;
	return tex2D(TextureSampler, input.TexCoord + displacement);
}

technique Warp
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 WarpPixelShader();
	}
}
