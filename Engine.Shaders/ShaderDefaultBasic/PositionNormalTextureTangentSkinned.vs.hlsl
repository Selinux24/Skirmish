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

cbuffer cbVSPerInstance : register(b2)
{
	float4 gTintColor;
	uint gMaterialIndex;
	uint gTextureIndex;
	uint2 PAD21;
	uint gAnimationOffset;
	uint gAnimationOffset2;
	float gAnimationInterpolation;
	float PAD22;
};

Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent2 main(VSVertexPositionNormalTextureTangentSkinned input)
{
    PSVertexPositionNormalTextureTangent2 output = (PSVertexPositionNormalTextureTangent2) 0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 tangentL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionNormalTangentWeights(
		gAnimationPalette,
		gAnimationOffset,
		gAnimationOffset2,
		gAnimationInterpolation,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		positionL,
		normalL,
		tangentL);

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3) gWorld));
	output.tangentWorld = normalize(mul(tangentL.xyz, (float3x3) gWorld));
	output.tex = input.tex;
	output.tintColor = gTintColor;
	output.textureIndex = gTextureIndex;
    output.material = material;

	return output;
}
