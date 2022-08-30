#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerEmitter : register(b0)
{
    float gTotalTime;
    float3 gEyePositionWorld;

    bool gRotation;
    float2 gRotateSpeed;
    uint gTextureCount;

    float3 gGravity;
    float gEndVelocity;

    float2 gStartSize;
    float2 gEndSize;
    float4 gMinColor;
    float4 gMaxColor;

    float gMaxDuration;
    float gMaxDurationRandomness;
    float2 PAD11;
}

Texture2DArray gTextureArray : register(t0);

SamplerState SamplerPointParticle : register(s0)
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

float4 main(PSCPUParticle input) : SV_Target
{
	float2 tex = input.tex;

	if (gRotation == true) {
		float4 rot = (input.rotationWorld * 2.0f) - 1.0f;

		tex -= 0.5f;
		tex = mul(tex, float2x2(rot));
		tex *= sqrt(2.0f);
		tex += 0.5f;
	}

	float3 uvw = float3(tex, input.primitiveID % gTextureCount);
	return gTextureArray.Sample(SamplerPointParticle, uvw) * input.color;
}
