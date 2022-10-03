#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncAnimation.hlsl"

cbuffer cbGlobals : register(b0)
{
	Globals gGlobals;
};

Texture2D gAnimationPalette : register(t0);

PSShadowMapPositionTexture main(VSVertexPositionTextureSkinnedI input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
    ComputePositionWeights(
		gAnimationPalette,
		input.animationOffset,
		input.animationOffsetB,
		input.animationInterpolation,
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    output.positionHomogeneous = mul(positionL, input.localTransform);
    output.depth = positionL;
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;
    
    return output;
}
