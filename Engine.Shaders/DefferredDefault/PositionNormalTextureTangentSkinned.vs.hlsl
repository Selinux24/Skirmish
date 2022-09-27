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

cbuffer cbPerMaterial : register(b3)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint gTextureIndex;
    uint2 PAD31;
};

Texture2D gAnimationPalette : register(t0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent main(VSVertexPositionNormalTextureTangentSkinned input)
{
    float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float4 tangentL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    ComputePositionNormalTangentWeights(
		gAnimationPalette,
		gAnimationOffset,
		gAnimationOffset2,
		gAnimationInterpolation,
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		positionL,
		normalL,
		tangentL);
    
    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);
	
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent) 0;

    output.positionHomogeneous = mul(positionL, wvp);
    output.positionWorld = mul(positionL, gLocal).xyz;
    output.normalWorld = normalize(mul(normalL.xyz, (float3x3) gLocal));
    output.tangentWorld = normalize(mul(tangentL.xyz, (float3x3) gLocal));
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;
    output.materialIndex = gMaterialIndex;
    output.tintColor = gTintColor;
    
    return output;
}
