
cbuffer cbPerEmitter : register(b0)
{
	float gMaxDuration;
	float gMaxDurationRandomness;
	float gTotalTime;
    float gElapsedTime;

	bool gRotation;
	float2 gRotateSpeed;
	uint gTextureCount;

	float3 gGravity;
	float gEndVelocity;

	float2 gStartSize;
	float2 gEndSize;
	float4 gMinColor;
	float4 gMaxColor;
}

Texture2DArray gTextureArray : register(t0);

SamplerState SamplerPointParticle : register(s0)
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

struct PSParticle
{
    uint primitiveID : SV_PRIMITIVEID;
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float4 rotationWorld : ROTATION;
    float2 tex : TEXCOORD0;
    float4 color : COLOR0;
};

float4 main(PSParticle input) : SV_Target
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
