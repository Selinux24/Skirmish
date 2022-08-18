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

Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

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
PSVertexPositionNormalColor2 main(VSVertexPositionNormalColorSkinned input)
{
	PSVertexPositionNormalColor2 output = (PSVertexPositionNormalColor2)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionNormalWeights(
		gAnimationPalette,
		gAnimationOffset,
		gAnimationOffset2,
		gAnimationInterpolation,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);

	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3) gWorld));
	output.color = input.color * gTintColor;
	output.material = material;

	return output;
}
