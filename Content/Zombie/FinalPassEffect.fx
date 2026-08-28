// Zombie/FinalPassEffect.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// the 20GPC project's ForeverWars shader work for why this rewrite-from-
// scratch approach was necessary -- no original .fx source survived).
//
// Usage (from FiftyGames.Zombie.Zombie.Draw, the very last draw call each
// frame): draws the fully-composited backbuffer render target as a final
// screen-space polish pass. No effect parameters are ever set from C#
// besides Texture, so a subtle vignette darkening toward the screen edges
// was chosen as a tasteful, parameter-free "final pass" effect. Best-effort
// recreation; original visual not recoverable byte-exact.
//
// FIX (round 4): this effect was previously applied via SpriteBatch
// (spriteBatch.Begin(..., this effect); spriteBatch.Draw(...)), matching
// several other reconstructed effects in this project (Warp.fx,
// ScreenEffect.fx, MaskEffect.fx). Render-target diagnostics proved that
// pattern produces 100% solid black output project-wide, even for a test
// shader that unconditionally returned solid red with no texture reads at
// all -- meaning SpriteBatch's custom-effect path itself was never actually
// invoking the shader/reaching the output. Every OTHER custom effect in
// this project that actually works on screen (GridShader.fx, InnerGlow.fx,
// DecalManagerEffect.fx, maskEffect.fx, ShadowHelper2DEffect.fx's
// ApplyShadowMap technique) is instead applied via
// effect.CurrentTechnique.Passes[0].Apply() followed by a raw full-screen
// quad draw (FullscreenQuad.Render), which takes vertex positions already
// in clip space and needs no MatrixTransform/vertex color. Rewritten to
// match that proven-working pattern: the vertex shader below now matches
// GridShader.fx's QuadVertexShader exactly (POSITION0 + TEXCOORD0, no
// COLOR0, no MatrixTransform), and Zombie.cs's final draw call was
// switched to _finalPassQuad.Render() to match (losing the CPU-side
// "shudder" rotation/scale wobble that a raw quad can't reproduce -- see
// the comment at that call site).

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
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

// Applied via a raw full-screen quad (FullscreenQuad.Render), not
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

float4 FinalPassPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TexCoord);
	float2 centered = input.TexCoord - 0.5;
	float vignette = saturate(1.0 - dot(centered, centered) * 1.2);
	color.rgb *= vignette;
	return color;
}

technique FinalPass
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 FinalPassPixelShader();
	}
}
