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
PSSpriteColor main(VSVertexPositionColor input)
{
    PSSpriteColor output = (PSSpriteColor) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gPerFrame.ViewProjection);
    output.positionWorld = output.positionHomogeneous.xyz;
    output.color = input.color;
    
    return output;
}
