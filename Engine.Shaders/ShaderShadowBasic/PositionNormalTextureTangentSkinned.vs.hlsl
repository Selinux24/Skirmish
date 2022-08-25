#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncAnimation.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
{
    uint gAnimationPaletteWidth;
    uint3 PAD01;
};

cbuffer cbPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

cbuffer cbPerInstance : register(b2)
{
	uint gTextureIndex;
	uint gAnimationOffset;
	uint gAnimationOffset2;
	float gAnimationInterpolation;
};

Texture2D gAnimationPalette : register(t0);

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSShadowMapPositionTexture main(VSVertexPositionNormalTextureTangentSkinned input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;

	return output;
}
