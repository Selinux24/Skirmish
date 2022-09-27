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

Texture2D gAnimationPalette : register(t0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent main(VSVertexPositionNormalTextureTangentSkinnedI input)
{
    float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float4 tangentL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    ComputePositionNormalTangentWeights(
		gAnimationPalette,
		input.animationOffset,
		input.animationOffsetB,
		input.animationInterpolation,
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		positionL,
		normalL,
		tangentL);
    float4 instancePosition = mul(positionL, input.localTransform);
    float3 instanceNormal = mul(normalL.xyz, (float3x3) input.localTransform);
    float3 instanceTangent = mul(tangentL.xyz, (float3x3) input.localTransform);
	
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent) 0;

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
    output.positionWorld = instancePosition.xyz;
    output.normalWorld = normalize(instanceNormal);
    output.tangentWorld = normalize(instanceTangent);
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;
    output.materialIndex = input.materialIndex;
    output.tintColor = input.tintColor;
    
    return output;
}
