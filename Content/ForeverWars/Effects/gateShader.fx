// ForeverWars/Effects/gateShader.fx
//
// RECONSTRUCTED (see gridShader.fx for why). jumpGate feeds this an
// uninitialized/blank dummy texture, so the visual is effectively fully
// procedural: a rotating "gate/portal" ring pattern driven by delta (an
// animated angle that cycles 0..pi/4) over a screenDimensions-sized quad.
// Applied via a raw full-screen quad. Best-effort recreation of a
// jump-gate/portal visual; original not recoverable byte-exact.

texture InputTexture;
sampler InputSampler = sampler_state
{
	Texture = (InputTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float delta;
float2 screenDimensions;

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

float4 GatePixelShader(VertexShaderOutput input) : COLOR0
{
	float2 center = float2(0.5, 0.5);
	float2 offset = (input.TexCoord - center) * screenDimensions;
	float radius = length(offset) / (min(screenDimensions.x, screenDimensions.y) * 0.5);
	float angle = atan2(offset.y, offset.x);

	float spin = angle + delta * 8.0;
	float bands = abs(sin(spin * 3.0));
	float ring = smoothstep(1.0, 0.85, radius) * smoothstep(0.0, 0.15, radius);

	float3 gateColor = lerp(float3(0.1, 0.3, 0.9), float3(0.6, 0.85, 1.0), bands);
	float alpha = ring * (0.5 + 0.5 * bands);

	// Keep InputTexture "live" (it's set from C# every frame) even though
	// the visual is procedural -- a blank dummy texture contributes ~nothing.
	float4 baseColor = tex2D(InputSampler, input.TexCoord);
	gateColor += baseColor.rgb * 0.001;

	return float4(gateColor, alpha);
}

technique Gate
{
	pass P0
	{
		VertexShader = compile vs_3_0 QuadVertexShader();
		PixelShader = compile ps_3_0 GatePixelShader();
	}
}
