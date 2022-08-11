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

struct PSVertexPositionColor2
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor2 main(VSVertexPositionColorI input)
{
	PSVertexPositionColor2 output = (PSVertexPositionColor2)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
	Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.color = input.color * input.tintColor * material.Diffuse;

	return output;
}
