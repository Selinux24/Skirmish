#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncScattering.hlsl"
#include "..\Lib\IncMatrix.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerObject : register(b1)
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

struct VSVertex
{
    float3 positionLocal : POSITION;
};

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 direction : DIRECTION;
    float4 colorR : COLOR0;
    float4 colorM : COLOR1;
};

PSVertex main(VSVertex input)
{
    float4 colorM;
    float4 colorR;
    float3 rayPos;
    vertexPhase(
        gSamples,
		input.positionLocal, gLightDirection, gBackColor,
		gSphereRadii, gScatteringCoeffs, gInvWaveLength, gMisc,
		colorM, colorR, rayPos);

    PSVertex output = (PSVertex) 0;

    float4x4 translation = translateToMatrix(gPerFrame.EyePosition);
    float4x4 wvp = mul(translation, gPerFrame.ViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = input.positionLocal;
    output.colorM = colorM;
    output.colorR = colorR;
    output.direction = rayPos;

    return output;
}
