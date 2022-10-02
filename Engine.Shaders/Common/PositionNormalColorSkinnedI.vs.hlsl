#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"
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

cbuffer cbPerMaterial : register(b2)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint3 PAD21;
};

Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor2 main(VSVertexPositionNormalColorSkinnedI input)
{
    float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
    ComputePositionNormalWeights(
		gAnimationPalette,
		input.animationOffset,
		input.animationOffsetB,
		input.animationInterpolation,
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);
    float4 instancePosition = mul(positionL, input.localTransform);
    float3 instanceNormal = mul(normalL.xyz, (float3x3) input.localTransform);

    uint materialIndex = input.materialIndex >= 0 ? input.materialIndex : gMaterialIndex;
    Material material = GetMaterialData(gMaterialPalette, materialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionNormalColor2 output = (PSVertexPositionNormalColor2) 0;

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
    output.positionWorld = instancePosition.xyz;
    output.normalWorld = normalize(instanceNormal);
    output.color = input.color * input.tintColor * gTintColor;
    output.material = material;

    return output;
}
