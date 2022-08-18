#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbVSGlobals : register(b0)
{
    uint gMaterialPaletteWidth;
    uint3 PAD01;
};

cbuffer cbVSPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

Texture2D gMaterialPalette : register(t0);

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
PSVertexPositionNormalTextureTangent2 main(VSVertexPositionNormalTextureTangentI input)
{
    PSVertexPositionNormalTextureTangent2 output = (PSVertexPositionNormalTextureTangent2) 0;

	Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);
	
	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
	float3 instanceNormal = mul(input.normalLocal, (float3x3) input.localTransform);
	float3 instanceTangent = mul(input.tangentLocal, (float3x3) input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3) gWorld));
	output.tangentWorld = normalize(mul(instanceTangent, (float3x3) gWorld));
	output.tex = input.tex;
	output.tintColor = input.tintColor;
	output.textureIndex = input.textureIndex;
    output.material = material;

	return output;
}
