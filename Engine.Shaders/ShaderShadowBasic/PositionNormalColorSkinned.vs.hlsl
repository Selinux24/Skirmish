#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncAnimation.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
{
	Globals gGlobals;
};

cbuffer cbPerFrame : register(b1)
{
	PerFrame gPerFrame;
};

cbuffer cbPerMesh : register(b2)
{
	float4x4 gLocal;
	uint gAnimationOffset;
	uint gAnimationOffset2;
	float gAnimationInterpolation;
	float PAD21;
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
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

	output.positionHomogeneous = mul(positionL, wvp);

	return output;
}
