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
    uint3 PAD31;
};

Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor main(VSVertexPositionNormalColorSkinned input)
{
	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionNormalWeights(
		gAnimationPalette,
		gAnimationOffset,
		gAnimationOffset2,
		gAnimationInterpolation,
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor) 0;

    output.positionHomogeneous = mul(positionL, wvp);
    output.positionWorld = mul(positionL, gLocal).xyz;
    output.normalWorld = normalize(mul(normalL.xyz, (float3x3) gLocal));
	output.color = input.color * gTintColor;
	output.material = material;

	return output;
}
