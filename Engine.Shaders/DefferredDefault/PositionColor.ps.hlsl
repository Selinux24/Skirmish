#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionColor input)
{
    GBufferPSOutput output = (GBufferPSOutput) 0;

    output.color = input.color;
    output.normal = float4(0, 0, 0, 0);
    output.depth = float4(input.positionWorld, input.materialIndex);

    return output;
}
