#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncHelpers.hlsl"

Texture2DArray gDiffuseMapArray : register(t0);
Texture2DArray gNormalMapArray : register(t1);

SamplerState SamplerDiffuse : register(s0);
SamplerState SamplerNormal : register(s1);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionNormalTextureTangent input)
{
    float4 diffuse = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));
    float3 normalMapSample = gNormalMapArray.Sample(SamplerNormal, float3(input.tex, input.textureIndex)).rgb;
    float3 bumpedNormalW = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

    GBufferPSOutput output = (GBufferPSOutput) 0;
    
    output.color = diffuse * input.tintColor;
    output.normal = float4(bumpedNormalW, 0);
    output.depth = float4(input.positionWorld, input.materialIndex);

    return output;
}
