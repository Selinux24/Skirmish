#include "..\Lib\IncScattering.hlsl"
#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

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

float4 main(PSVertexSkyScattering input) : SV_TARGET
{
    return HDR(pixelPhase(gLightDirection, input.direction, input.colorR, input.colorM), gHDRExposure);
}
