#include "..\Lib\IncVertexFormats.hlsl"

Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerDiffuse : register(s0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionNormalTexture2 input)
{
    float4 diffuse = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

    GBufferPSOutput output = (GBufferPSOutput) 0;
    
    output.color = diffuse * input.tintColor * input.material.Diffuse;
    output.normal = float4(normalize(input.normalWorld), 1);
    output.depth = float4(input.positionWorld, input.material.Algorithm);
    output.mat1 = float4(input.material.Specular, input.material.Shininess);
    output.mat2 = float4(input.material.Emissive, input.material.Metallic);
    output.mat3 = float4(input.material.Ambient, input.material.Roughness);

    return output;
}
