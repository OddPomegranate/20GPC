// LightBikes/Shaders/GridShader.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect. The
// original was GPU shader bytecode compiled for the Xbox 360's hardware,
// which MonoGame's DesktopGL/OpenGL backend cannot load at all ("This does
// not appear to be a MonoGame MGFX file!"). No original .fx (HLSL) source
// survived, so this was rewritten from scratch based on how the game code
// uses it (see the 20GPC project's ForeverWars shader work for the full
// background on why this approach was necessary).
//
// Usage (from FiftyGames.LightBikes.Grid.DrawBackground): every frame, after
// the grid's per-cell "backing" dots have been rendered once into a static
// render target (backgroundRT), this effect is applied via a raw full-screen
// quad (not SpriteBatch) with three time-derived parameters:
//   delta          - base animation clock, increments ~0.003/frame, wraps at 6
//   colorDelta     - delta + 0.1 (slightly phase-shifted copy of delta)
//   positionDelta  - same as delta
// and InputTexture = the static backing-dots render target.
//
// Interpreted as an ambient "grid glow" background animation: a slow
// positional wobble (positionDelta) plus a slow color-cycle/pulse
// (colorDelta, delta) over the backing dots, fitting the Tron-esque
// light-cycle grid aesthetic. Best-effort recreation; original visual not
// recoverable byte-exact.

texture InputTexture;
sampler InputSampler = sampler_state
{
	Texture = (InputTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

float delta;
float colorDelta;
float positionDelta;

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

float3 HueShift(float3 color, float hueTurns)
{
	float angle = hueTurns * 6.2831853;
	float3 k = float3(0.57735, 0.57735, 0.57735);
	float cosAngle = cos(angle);
	return color * cosAngle + cross(k, color) * sin(angle) + k * dot(k, color) * (1.0 - cosAngle);
}

float4 GridBackgroundPixelShader(VertexShaderOutput input) : COLOR0
{
	float2 wobble = float2(sin(positionDelta * 3.0), cos(positionDelta * 2.0)) * 0.01;
	float4 color = tex2D(InputSampler, input.TexCoord + wobble);

	float pulse = 0.5 + 0.5 * sin(delta * 4.0);
	color.rgb = HueShift(color.rgb, frac(colorDelta * 0.1));
	color.rgb *= (0.7 + 0.3 * pulse);

	return color;
}

technique GridBackground
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 GridBackgroundPixelShader();
	}
}
