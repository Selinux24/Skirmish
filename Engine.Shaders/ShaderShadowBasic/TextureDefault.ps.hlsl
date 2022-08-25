#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

/**********************************************************************************************************
TRANSPARENT TEXTURES
**********************************************************************************************************/
float4 main(PSShadowMapPositionTexture input) : SV_TARGET
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
