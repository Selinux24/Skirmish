#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"
#include "..\Lib\IncAnimation.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbVSGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	uint gAnimationPaletteWidth;
	uint2 PAD01;
};
Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

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
	uint gAnimationOffset;
	uint gAnimationOffset2;
	float gAnimationInterpolation;
	float PAD22;
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
PSVertexPositionColor2 main(VSVertexPositionColorSkinned input)
{
	PSVertexPositionColor2 output = (PSVertexPositionColor2)0;

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionWeights(
		gAnimationPalette,
		gAnimationOffset,
		gAnimationOffset2,
		gAnimationInterpolation,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.color = input.color * gTintColor * material.Diffuse;

	return output;
}
