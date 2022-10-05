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
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor2 main(VSVertexPositionColorSkinnedI input)
{
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
	float4 instancePosition = mul(positionL, input.localTransform);

    Material material = GetMaterialData(gMaterialPalette, input.materialIndex + gMaterialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionColor2 output = (PSVertexPositionColor2) 0;

	output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
	output.positionWorld = instancePosition.xyz;
    output.color = input.color * input.tintColor * gTintColor * material.Diffuse;

	return output;
}
