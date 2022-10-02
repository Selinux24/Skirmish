#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionColor2 input)
{
    GBufferPSOutput output = (GBufferPSOutput) 0;

    output.color = input.color;
    output.normal = float4(0, 0, 0, 0);
    output.depth = float4(input.positionWorld, 0);
    output.mat1 = float4(0, 0, 0, 0);
    output.mat2 = float4(0, 0, 0, 0);
    output.mat3 = float4(0, 0, 0, 0);

    return output;
}
