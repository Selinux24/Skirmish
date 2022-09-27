#include "..\Lib\IncVertexFormats.hlsl"

Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerDiffuse : register(s0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionNormalTexture input)
{
    float4 diffuse = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

    GBufferPSOutput output = (GBufferPSOutput) 0;
    
    output.color = diffuse * input.tintColor;
    output.normal = float4(normalize(input.normalWorld), 0);
    output.depth = float4(input.positionWorld, input.materialIndex);

    return output;
}
