#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionNormalColor2 input)
{
    GBufferPSOutput output = (GBufferPSOutput) 0;

    output.color = input.color;
    output.normal = float4(normalize(input.normalWorld), 1);
    output.depth = float4(input.positionWorld, input.material.Algorithm);
    output.mat1 = float4(input.material.Specular, input.material.Shininess);
    output.mat2 = float4(input.material.Emissive, input.material.Metallic);
    output.mat3 = float4(input.material.Ambient, input.material.Roughness);

    return output;
}
