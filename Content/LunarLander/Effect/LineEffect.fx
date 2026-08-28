// ForeverWars/Effects/LineEffect.fx
//
// RECONSTRUCTED (see gridShader.fx for why). A minimal vertex-color
// line-drawing effect: LineRender pre-transforms vertex positions into clip
// space itself (ScreenToShader) and never sets any effect parameters, so
// this just needs to pass position and color straight through.
//
// This exact C# driving pattern (same ScreenToShader logic, same
// Load<Effect>(".../LineEffect")) is reused nearly verbatim by most other
// minigames' own LineEffect.xnb (MicroMachines, Shooter, SwingGems,
// LunarLander, SuperHighway, Zombie, ...), so this same shader content
// should work for those too once each one is ported over.

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
};

VertexShaderOutput LineVertexShader(float4 position : POSITION0, float4 color : COLOR0)
{
	VertexShaderOutput output;
	output.Position = float4(position.xyz, 1);
	output.Color = color;
	return output;
}

float4 LinePixelShader(VertexShaderOutput input) : COLOR0
{
	return input.Color;
}

technique Line
{
	pass P0
	{
		VertexShader = compile vs_3_0 LineVertexShader();
		PixelShader = compile ps_3_0 LinePixelShader();
	}
}
