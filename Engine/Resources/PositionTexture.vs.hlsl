#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	uint3 PAD01;
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
};

Texture2D gMaterialPalette : register(t0);

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
PSVertexPositionTexture2 main(VSVertexPositionTexture input)
{
	PSVertexPositionTexture2 output = (PSVertexPositionTexture2)0;

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
	output.tintColor = gTintColor * material.Diffuse;
	output.textureIndex = gTextureIndex;

	return output;
}