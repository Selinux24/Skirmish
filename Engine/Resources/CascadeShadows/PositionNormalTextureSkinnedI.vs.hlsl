#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncAnimation.hlsl"

cbuffer cbGlobals : register(b0)
{
    uint gMaterialPaletteWidth;
    uint gAnimationPaletteWidth;
    uint2 PAD01;
};

Texture2D gAnimationPalette : register(t0);

PSShadowMapPositionTexture main(VSVertexPositionNormalTextureSkinnedI input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

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

    output.positionHomogeneous = mul(positionL, input.localTransform);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;

    return output;
}
