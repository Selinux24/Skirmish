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
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor main(VSVertexPositionColorSkinned input)
{
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

    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionColor output = (PSVertexPositionColor) 0;

	output.positionHomogeneous = mul(positionL, wvp);
	output.positionWorld = mul(positionL, gLocal).xyz;
	output.color = input.color * gTintColor * material.Diffuse;

	return output;
}
