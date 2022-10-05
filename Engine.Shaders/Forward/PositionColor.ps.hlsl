#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
float4 main(PSVertexPositionColor input) : SV_TARGET
{
	float4 matColor = input.color;

    if (gPerFrame.FogRange > 0)
	{
        float distToEye = length(gPerFrame.EyePosition - input.positionWorld);

        matColor = ComputeFog(matColor, distToEye, gPerFrame.FogStart, gPerFrame.FogRange, gPerFrame.FogColor);
    }

	return matColor;
}
