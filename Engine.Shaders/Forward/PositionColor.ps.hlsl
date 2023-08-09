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
        float fog = CalcFogFactor(distToEye, gPerFrame.FogStart, gPerFrame.FogRange);
        if (fog >= 1)
        {
            return gPerFrame.FogColor;
        }
        matColor = ApplyFog(matColor, gPerFrame.FogColor, fog);
    }

	return matColor;
}
