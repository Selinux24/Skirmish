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
Texture2D gMaterialPalette : register(t0);

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

struct PSVertexPositionColor2
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor2 main(VSVertexPositionColor input)
{
	PSVertexPositionColor2 output = (PSVertexPositionColor2)0;

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.color = input.color * gTintColor * material.Diffuse;

	return output;
}
