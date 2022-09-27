#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
GBufferPSOutput main(PSVertexPositionNormalColor input)
{
    GBufferPSOutput output = (GBufferPSOutput) 0;

    output.color = input.color;
    output.normal = float4(normalize(input.normalWorld), 0);
    output.depth = float4(input.positionWorld, input.materialIndex);

    return output;
}
