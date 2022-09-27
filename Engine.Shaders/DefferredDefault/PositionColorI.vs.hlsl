#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor main(VSVertexPositionColorI input)
{
    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    PSVertexPositionColor output = (PSVertexPositionColor) 0;

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
    output.positionWorld = instancePosition.xyz;
    output.color = input.color * input.tintColor;
    output.materialIndex = input.materialIndex;
    
    return output;
}
