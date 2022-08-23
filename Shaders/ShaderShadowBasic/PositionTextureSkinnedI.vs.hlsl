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

Texture2D gAnimationPalette : register(t0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSShadowMapPositionTexture main(VSVertexPositionTextureSkinnedI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}
