// FruitsInARow/Effect/ScreenEffect.fx
//
// RECONSTRUCTED replacement for the original Xbox 360-compiled effect (see
// the 20GPC project's ForeverWars shader work for the general background on
// why this rewrite-from-scratch approach was necessary -- no original .fx
// source survived).
//
// Usage (from FiftyGames.FruitsInARow.FruitsInARow.Draw): applied via
// SpriteBatch (SpriteSortMode.Immediate) as a custom effect, technique
// explicitly selected as "Blur":
//   _postEffect.CurrentTechnique = _postEffect.Techniques["Blur"];
//   _postEffect.Parameters["brightness"].SetValue(...);
// then a render target is drawn through it 8 times in an additive-blend
// ping-pong loop between two canvases (_effectCanvas0/_effectCanvas1) --
// the classic "repeated blur + brightness boost accumulated with additive
// blending" recipe for a soft bloom/glow, used here to make the winning
// 4-in-a-row counters glow. Best-effort recreation of that visual; original
// not recoverable byte-exact.

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

float brightness;

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
	color *= input.Color;
	return color;
}

technique Blur
{
	pass P0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 BlurPixelShader();
	}
}
