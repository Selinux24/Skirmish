
cbuffer cbPerCube : register(b0)
{
	float gTextureIndex;
};

Texture2DArray gTexture : register(t0);

SamplerState SamplerLinear : register(s0)
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct PSVertex
{
	float4 positionHomogeneous : SV_POSITION;
	float2 tex : TEXCOORD0;
};

float4 main(PSVertex input) : SV_TARGET
{
	return gTexture.Sample(SamplerLinear, float3(input.tex, gTextureIndex));
}
