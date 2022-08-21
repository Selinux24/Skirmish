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

cbuffer cbVSPerObject : register(b2)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint3 PAD21;
};

Texture2D gMaterialPalette : register(t0);

struct PSVertexPositionNormalTexture2
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    Material material : MATERIAL;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTexture2 main(VSVertexPositionNormalTextureI input)
{
    PSVertexPositionNormalTexture2 output = (PSVertexPositionNormalTexture2) 0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
    float3 instanceNormal = mul(input.normalLocal, (float3x3) input.localTransform);

    uint materialIndex = input.materialIndex >= 0 ? input.materialIndex : gMaterialIndex;
    Material material = GetMaterialData(gMaterialPalette, materialIndex, gMaterialPaletteWidth);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
    output.normalWorld = normalize(mul(instanceNormal, (float3x3) gWorld));
    output.tex = input.tex;
    output.tintColor = input.tintColor * gTintColor;
    output.textureIndex = input.textureIndex;
    output.material = material;

    return output;
}
