#include "..\Lib\IncGBuffer.hlsl"
#include "..\Lib\IncHelpers.hlsl"

Texture2DArray gDiffuseMapArray : register(t0);
Texture2DArray gNormalMapArray : register(t1);

SamplerState SamplerDiffuse : register(s0);
SamplerState SamplerNormal : register(s1);

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
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
    float3 normalMapSample = gNormalMapArray.Sample(SamplerNormal, float3(input.tex, input.textureIndex)).rgb;
    float3 bumpedNormalW = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);
    
    return Pack(input.positionWorld, bumpedNormalW, diffuse * input.tintColor, true, input.material);
}
