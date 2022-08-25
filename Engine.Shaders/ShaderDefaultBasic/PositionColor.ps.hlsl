#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    float3 gEyePositionWorld;
    float PAD11;
    float4 gFogColor;
    float gFogStart;
    float gFogRange;
    float2 PAD12;
    float3 gLOD;
    float gShadowIntensity;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
float4 main(PSVertexPositionColor2 input) : SV_TARGET
{
	float4 matColor = input.color;

	if (gFogRange > 0)
	{
		float distToEye = length(gEyePositionWorld - input.positionWorld);

		matColor = ComputeFog(matColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return matColor;
}
