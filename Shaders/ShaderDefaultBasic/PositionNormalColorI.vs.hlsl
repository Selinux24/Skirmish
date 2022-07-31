#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbVSPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor main(VSVertexPositionNormalColorI input)
{
	PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
	float3 instanceNormal = mul(input.normalLocal, (float3x3) input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3) gWorld));
	output.color = input.color * input.tintColor;
	output.materialIndex = input.materialIndex;

	return output;
}
