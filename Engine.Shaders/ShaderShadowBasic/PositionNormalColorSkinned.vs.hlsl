#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncAnimation.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
{
	uint PAD01;
	uint gAnimationPaletteWidth;
	uint2 PAD02;
};

cbuffer cbPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

cbuffer cbPerInstance : register(b2)
{
	uint gAnimationOffset;
	uint gAnimationOffset2;
	float gAnimationInterpolation;
	float PAD22;
};

Texture2D gAnimationPalette : register(t0);

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSShadowMapPosition main(VSVertexPositionNormalColorSkinned input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

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

	return output;
}