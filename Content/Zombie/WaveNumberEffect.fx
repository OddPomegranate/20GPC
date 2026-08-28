// Zombie/WaveNumberEffect.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// the 20GPC project's ForeverWars shader work for why this rewrite-from-
// scratch approach was necessary -- no original .fx source survived).
//
// Usage (from FiftyGames.Zombie.Rendering_Helpers.WaveInfoDrawer.Draw):
// applied via SpriteBatch (SpriteSortMode.Immediate) as a custom effect,
// drawing a small pre-rendered "Wave N" text canvas (_renderTarget, bound
// automatically as the primary SpriteBatch draw texture) with two extra
// parameters:
//   InputTexture - the Zombie/ParticleSprites/Explosion sprite, used here
//                  purely as a fire/turbulence noise source (not drawn
//                  directly).
//   Time         - elapsed time / 5000, i.e. a slow ~0..1-ish ramp.
// Interpreted as a "burning wave-number text" effect: the explosion sprite
// is sampled as a scrolling noise field to both wobble the text's UVs and
// add a warm fire-colored glow, giving the wave counter a flickering,
// on-fire look appropriate to a zombie horde game. Best-effort recreation;
// original visual not recoverable byte-exact.

float4x4 MatrixTransform;

texture Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture InputTexture;
sampler InputSampler = sampler_state
{
	Texture = (InputTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

float Time;

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

float4 WaveNumberPixelShader(VertexShaderOutput input) : COLOR0
{
	float2 noiseUV = input.TexCoord * 2.0 + float2(0, -Time * 2.0);
	float4 noise = tex2D(InputSampler, frac(noiseUV));

	float2 distortedUV = input.TexCoord + (noise.rg - 0.5) * 0.03;
	float4 color = tex2D(TextureSampler, distortedUV);

	float glow = noise.r * color.a;
	color.rgb += float3(1.0, 0.5, 0.1) * glow * 0.5;

	color *= input.Color;
	return color;
}

technique WaveNumber
{
	pass P0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 WaveNumberPixelShader();
	}
}
