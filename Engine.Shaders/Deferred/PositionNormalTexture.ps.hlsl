#include "..\Lib\IncGBuffer.hlsl"

Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerDiffuse : register(s0);

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    Material material;
};

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
GBuffer main(PSVertex input)
{
    float4 diffuse = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

    return Pack(input.positionWorld, normalize(input.normalWorld), diffuse * input.tintColor, true, input.material);
}
