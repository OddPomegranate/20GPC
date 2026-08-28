// ForeverWars/Effects/DecalManagerEffect.fx
//
// RECONSTRUCTED (see gridShader.fx for why). Hardware-instanced decal
// renderer: DecalManager draws a shared unit quad (stream 0,
// VertexPositionTexture, spanning -1..1) once per queued decal, using a
// per-instance 4x4 transform matrix supplied via a second, instance-
// frequency vertex stream packed as four Vector4 BLENDWEIGHT0-3 elements
// (DecalManager's `_instanceVertexDeclaration`; each XNA/MonoGame `Matrix`
// is 16 floats = exactly 4 Vector4 rows in memory, so the instance buffer
// is literally an array of raw Matrix structs). The base quad position is
// already in clip space, so multiplying by the instance matrix and adding
// the (also clip-space) Offset parameter produces the final position
// directly -- no additional world/view/projection matrix needed.
//
// Hardware instancing requires shader model 3.0, so Content.mgcb's global
// profile was changed from Reach to HiDef to support this (and the vs_3_0/
// ps_3_0 shaders used by this whole batch of reconstructed effects).

texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float Alpha;
float2 Offset;

struct VSInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 InstanceRow0 : BLENDWEIGHT0;
	float4 InstanceRow1 : BLENDWEIGHT1;
	float4 InstanceRow2 : BLENDWEIGHT2;
	float4 InstanceRow3 : BLENDWEIGHT3;
};

struct VSOutput
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
};

VSOutput DecalVertexShader(VSInput input)
{
	float4x4 instanceTransform = float4x4(input.InstanceRow0, input.InstanceRow1, input.InstanceRow2, input.InstanceRow3);

	float4 transformedPosition = mul(input.Position, instanceTransform);
	transformedPosition.xy += Offset;

	VSOutput output;
	output.Position = transformedPosition;
	output.TexCoord = input.TexCoord;
	return output;
}

float4 DecalPixelShader(VSOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TexCoord);
	color.a *= Alpha;
	return color;
}

technique Decal
{
	pass P0
	{
		VertexShader = compile vs_3_0 DecalVertexShader();
		PixelShader = compile ps_3_0 DecalPixelShader();
	}
}
