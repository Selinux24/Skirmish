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

cbuffer cbPerMaterial : register(b2)
{
    uint gTextureIndex;
    uint3 PAD21;
};

Texture2D gAnimationPalette : register(t0);

struct VSVertex
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
    float3 weights : WEIGHTS;
    uint4 boneIndices : BONEINDICES;
};

struct PSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
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
    output.depth = positionL;
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
