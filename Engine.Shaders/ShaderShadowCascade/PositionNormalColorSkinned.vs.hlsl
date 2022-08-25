#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncAnimation.hlsl"

cbuffer cbGlobals : register(b0)
{
    uint gMaterialPaletteWidth;
    uint gAnimationPaletteWidth;
    uint2 PAD01;
};

cbuffer cbPerInstance : register(b1)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint gTextureIndex;
    uint2 PAD11;
    uint gAnimationOffset;
    uint gAnimationOffset2;
    float gAnimationInterpolation;
    float PAD12;
};

Texture2D gAnimationPalette : register(t0);

PSShadowMapPosition main(VSVertexPositionNormalColorSkinned input)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

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
	
    output.positionHomogeneous = positionL;

    return output;
}
