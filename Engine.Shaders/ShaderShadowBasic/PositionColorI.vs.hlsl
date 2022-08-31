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
PSShadowMapPosition main(VSVertexPositionColorI input)
{
    PSShadowMapPosition output = (PSShadowMapPosition)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);

    return output;
}
