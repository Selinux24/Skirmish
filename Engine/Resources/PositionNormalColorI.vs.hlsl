#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbVSGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	uint3 PAD01;
};

cbuffer cbVSPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

Texture2D gMaterialPalette : register(t0);

struct PSVertexPositionNormalColor2
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float4 color : COLOR0;
	Material material : MATERIAL;
};

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor2 main(VSVertexPositionNormalColorI input)
{
	PSVertexPositionNormalColor2 output = (PSVertexPositionNormalColor2)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
	float3 instanceNormal = mul(input.normalLocal, (float3x3) input.localTransform);
	Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3) gWorld));
	output.color = input.color * input.tintColor;
	output.material = material;

	return output;
}
