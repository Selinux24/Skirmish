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

cbuffer cbVSPerObject : register(b2)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint3 PAD21;
};

Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

struct PSVertexPositionNormalColor2
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float4 color : COLOR0;
    Material material : MATERIAL;
};

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor2 main(VSVertexPositionNormalColorSkinnedI input)
{
    PSVertexPositionNormalColor2 output = (PSVertexPositionNormalColor2) 0;

    float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    ComputePositionNormalWeights(
		gAnimationPalette,
		input.animationOffset,
		input.animationOffsetB,
		input.animationInterpolation,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);
    float4 instancePosition = mul(positionL, input.localTransform);
    float3 instanceNormal = mul(normalL.xyz, (float3x3) input.localTransform);

    uint materialIndex = input.materialIndex >= 0 ? input.materialIndex : gMaterialIndex;
    Material material = GetMaterialData(gMaterialPalette, materialIndex, gMaterialPaletteWidth);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
    output.normalWorld = normalize(mul(instanceNormal, (float3x3) gWorld));
    output.color = input.color * input.tintColor * gTintColor;
    output.material = material;

    return output;
}
