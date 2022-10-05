#include "..\Lib\IncGBuffer.hlsl"

Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerDiffuse : register(s0);

struct PSVertex
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
GBuffer main(PSVertex input)
{
    float4 diffuse = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));
    
    return Pack(input.positionWorld, float3(0, 0, 0), diffuse * input.tintColor, true, (Material) 0);
}
