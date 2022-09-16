#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerMaterial : register(b0)
{
    float4 gTintColor;
    
    uint gMaterialIndex;
    uint gTextureCount;
    uint gNormalMapCount;
    uint PAD51;
};

Texture2DArray gTextureArray : register(t3);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

struct PSFoliage
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint primitiveID : SV_PRIMITIVEID;
};

float4 main(PSFoliage input) : SV_Target
{
    float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);

    float4 diffuseColor = gTextureArray.Sample(SamplerLinear, uvw);

    if (diffuseColor.a > 0.8f)
    {
        float depthValue = input.depth.z / input.depth.w;

        return float4(depthValue, depthValue, depthValue, 1.0f);
    }
    else
    {
        discard;

        return 0.0f;
    }
}
