#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncAnimation.hlsl"

cbuffer cbGlobals : register(b0)
{
	Globals gGlobals;
};

cbuffer cbPerMesh : register(b1)
{
	float4x4 gLocal;
	uint gAnimationOffset;
	uint gAnimationOffset2;
	float gAnimationInterpolation;
	float PAD11;
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
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
    output.positionHomogeneous = mul(positionL, gLocal);

    return output;
}
