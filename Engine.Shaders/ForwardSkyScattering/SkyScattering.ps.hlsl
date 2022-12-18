#include "..\Lib\IncScattering.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerObject : register(b0)
{
    float4 gSphereRadii;
    float4 gScatteringCoeffs;
    float4 gInvWaveLength;
    float4 gMisc;
    float4 gBackColor;
    float3 gLightDirection;
    float gHDRExposure;
    uint gSamples;
    uint3 PAD;
};

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 direction : DIRECTION;
    float4 colorR : COLOR0;
    float4 colorM : COLOR1;
};

float4 main(PSVertex input) : SV_TARGET
{
    return HDR(pixelPhase(gLightDirection, input.direction, input.colorR, input.colorM), gHDRExposure);
}
