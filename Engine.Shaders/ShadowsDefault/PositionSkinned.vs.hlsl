#include "..\Lib\IncBuiltIn.hlsl"
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

struct VSVertex
{
    float3 positionLocal : POSITION;
    float3 weights : WEIGHTS;
    uint4 boneIndices : BONEINDICES;
};

struct PSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
};

PSShadowMap main(VSVertex input)
{
    PSShadowMap output = (PSShadowMap) 0;

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
