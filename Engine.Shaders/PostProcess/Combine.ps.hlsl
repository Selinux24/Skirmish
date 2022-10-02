
Texture2D gTexture1 : register(t0);
Texture2D gTexture2 : register(t1);

SamplerState SamplerLinear : register(s0)
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct PSVertexEmpty
{
	float4 hpos : SV_Position;
	float2 uv : TEXCOORD0;
};

float4 main(PSVertexEmpty input) : SV_TARGET
{
	float3 output1 = gTexture1.Sample(SamplerLinear, input.uv).rgb;
	float4 output2 = gTexture2.Sample(SamplerLinear, input.uv);

	float3 mix = (output1 * (1 - output2.a)) + (output2.rgb * output2.a);
	return float4(saturate(mix), 1);
}
