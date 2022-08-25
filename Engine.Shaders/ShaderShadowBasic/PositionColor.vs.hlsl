#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSShadowMapPosition main(VSVertexPositionColor input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
