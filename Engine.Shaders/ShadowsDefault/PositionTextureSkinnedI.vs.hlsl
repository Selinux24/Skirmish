#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncAnimation.hlsl"

cbuffer cbGlobals : register(b0)
{
	Globals gGlobals;
};

Texture2D gAnimationPalette : register(t0);

struct VSVertex
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
    float3 weights : WEIGHTS;
    uint4 boneIndices : BONEINDICES;
    row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    int materialIndex : MATERIALINDEX;
    uint animationOffset : ANIMATIONOFFSET;
    uint animationOffsetB : ANIMATIONOFFSETB;
    float animationInterpolation : ANIMATIONINTERPOLATION;
    uint instanceId : SV_INSTANCEID;
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
