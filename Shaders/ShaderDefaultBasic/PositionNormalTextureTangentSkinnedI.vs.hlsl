#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"
#include "..\Lib\IncAnimation.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbVSGlobals : register(b0)
{
    uint gMaterialPaletteWidth;
    uint gAnimationPaletteWidth;
    uint2 PAD01;
};

cbuffer cbVSPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

struct PSVertexPositionNormalTextureTangent2
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    Material material : MATERIAL;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent2 main(VSVertexPositionNormalTextureTangentSkinnedI input)
{
    PSVertexPositionNormalTextureTangent2 output = (PSVertexPositionNormalTextureTangent2) 0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 tangentL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionNormalTangentWeights(
		gAnimationPalette,
		input.animationOffset,
		input.animationOffsetB,
		input.animationInterpolation,
		gAnimationPaletteWidth,
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

    Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal.xyz, (float3x3) gWorld));
	output.tangentWorld = normalize(mul(instanceTangent.xyz, (float3x3) gWorld));
	output.tex = input.tex;
	output.tintColor = input.tintColor;
	output.textureIndex = input.textureIndex;
    output.material = material;

	return output;
}
