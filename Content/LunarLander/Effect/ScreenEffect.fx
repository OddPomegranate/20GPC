// LunarLander/Effect/ScreenEffect.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// the 20GPC project's ForeverWars shader work for why this rewrite-from-
// scratch approach was necessary -- no original .fx source survived).
//
// Usage (from FiftyGames.LunarLander.LunarLander.Draw): needs THREE
// techniques by name, all sharing the same "brightness" float parameter:
//   "Blur"           - same repeated additive-blur-glow recipe as
//                       FruitsInARow's ScreenEffect (see that file), used
//                       here for the vector/interface render-target glow.
//   "ScanLines"       - applied to the blurred composite when drawn to the
//                       backbuffer, no brightness set immediately before
//                       this call (inherits whatever brightness was last
//                       set, from the Blur loop) -- a CRT-style horizontal
//                       scanline darkening pattern.
//   "ScanLinesBright" - same scanline pattern, applied to the vector/
//                       interface canvases with an explicit brightness
//                       value, slightly stronger/brighter than ScanLines
//                       (hence the name) so foreground UI reads clearly
//                       over the scanlined backdrop.
// Best-effort recreation of a lunar-lander-vector-display CRT look;
// original not recoverable byte-exact.
//
// FIX (round 4): this effect was previously applied via SpriteBatch
// (spriteBatch.Begin(..., this effect); spriteBatch.Draw(...)), matching
// several other reconstructed effects in this project (Warp.fx,
// FinalPassEffect.fx, MaskEffect.fx). Render-target diagnostics proved
// that pattern produces 100% solid black output project-wide, even for a
// test shader that unconditionally returned solid red with no texture
// reads at all -- meaning SpriteBatch's custom-effect path itself was
// never actually invoking the shader/reaching the output. Every OTHER
// custom effect in this project that actually works on screen
// (GridShader.fx, InnerGlow.fx, DecalManagerEffect.fx, maskEffect.fx,
// ShadowHelper2DEffect.fx's ApplyShadowMap technique) is instead applied
// via effect.CurrentTechnique.Passes[0].Apply() followed by a raw
// full-screen quad draw (fullScreenQuad.Render), which takes vertex
// positions already in clip space and needs no MatrixTransform/vertex
// color. Rewritten to match that proven-working pattern: the vertex
// shader below now matches GridShader.fx's QuadVertexShader exactly
// (POSITION0 + TEXCOORD0, no COLOR0, no MatrixTransform), and each pixel
// shader's "* input.Color" multiply was dropped (SpriteBatch always
// passed Color.White here anyway, so this is a no-op mathematically).
// LunarLander.cs's draw calls were switched to quad.Render() to match,
// with GraphicsDevice.BlendState set explicitly in place of the blend
// state SpriteBatch.Begin used to apply.

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float brightness;

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

// Small 5-tap cross box blur (screen assumed 1280x720) plus a flat
// brightness boost, so repeated additive passes build up a soft glow.
float4 BlurPixelShader(VertexShaderOutput input) : COLOR0
{
	float2 texel = float2(1.0 / 1280.0, 1.0 / 720.0);

	float4 color = tex2D(TextureSampler, input.TexCoord) * 0.4;
	color += tex2D(TextureSampler, input.TexCoord + float2(texel.x, 0)) * 0.15;
	color += tex2D(TextureSampler, input.TexCoord - float2(texel.x, 0)) * 0.15;
	color += tex2D(TextureSampler, input.TexCoord + float2(0, texel.y)) * 0.15;
	color += tex2D(TextureSampler, input.TexCoord - float2(0, texel.y)) * 0.15;

	color.rgb += brightness;
	return color;
}

float4 ScanLinesPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TexCoord);
	float scanline = (frac(input.TexCoord.y * 360.0) < 0.5) ? 1.0 : 0.75;
	color.rgb *= scanline * brightness;
	return color;
}

float4 ScanLinesBrightPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TexCoord);
	float scanline = (frac(input.TexCoord.y * 360.0) < 0.5) ? 1.0 : 0.85;
	color.rgb *= scanline * brightness * 1.3;
	return color;
}

technique Blur
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 BlurPixelShader();
	}
}

technique ScanLines
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 ScanLinesPixelShader();
	}
}

technique ScanLinesBright
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 ScanLinesBrightPixelShader();
	}
}
