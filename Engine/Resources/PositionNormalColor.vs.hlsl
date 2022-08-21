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

cbuffer cbVSPerInstance : register(b2)
{
	float4 gTintColor;
	uint gMaterialIndex;
	uint3 PAD21;
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
PSVertexPositionNormalColor2 main(VSVertexPositionNormalColor input)
{
	PSVertexPositionNormalColor2 output = (PSVertexPositionNormalColor2)0;

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3) gWorld));
	output.color = input.color * gTintColor;
	output.material = material;

	return output;
}