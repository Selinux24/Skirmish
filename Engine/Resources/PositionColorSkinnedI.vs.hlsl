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

struct PSVertexPositionColor2
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float4 color : COLOR0;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor2 main(VSVertexPositionColorSkinnedI input)
{
	PSVertexPositionColor2 output = (PSVertexPositionColor2)0;

	Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionWeights(
		gAnimationPalette,
		input.animationOffset,
		input.animationOffsetB,
		input.animationInterpolation,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	float4 instancePosition = mul(positionL, input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.color = input.color * input.tintColor * material.Diffuse;

	return output;
}
