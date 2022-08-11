#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"
#include "..\Lib\IncAnimation.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
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

struct PSVertexPositionTexture2
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float2 tex : TEXCOORD0;
	float4 tintColor : TINTCOLOR;
	uint textureIndex : TEXTUREINDEX;
};

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture2 main(VSVertexPositionTextureSkinnedI input)
{
	PSVertexPositionTexture2 output = (PSVertexPositionTexture2)0;

	Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionWeights(
		gAnimationPalette,
		input.animationOffset,
		input.animationOffsetB,
		input.animationInterpolation,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	float4 instancePosition = mul(positionL, input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.tex = input.tex;
	output.tintColor = input.tintColor * material.Diffuse;
	output.textureIndex = input.textureIndex;

	return output;
}
