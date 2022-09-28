#include "..\Lib\IncVertexFormats.hlsl"

Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerDiffuse : register(s0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionTexture2 input)
{
    float4 diffuse = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));
    
    GBufferPSOutput output = (GBufferPSOutput) 0;

    output.color = diffuse * input.tintColor;
    output.normal = float4(0, 0, 0, 0);
    output.depth = float4(input.positionWorld, 0);
    output.mat1 = float4(0, 0, 0, 0);
    output.mat2 = float4(0, 0, 0, 0);
    output.mat3 = float4(0, 0, 0, 0);

    return output;
}
