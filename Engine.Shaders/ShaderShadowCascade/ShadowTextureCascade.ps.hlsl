
Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

struct GSShadowMapTexture
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

float4 main(GSShadowMapTexture input) : SV_Target
{
    float4 textureColor = gDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

    if (textureColor.a > 0.8f)
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
