#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

PSVertexPositionTexture main(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gPerFrame.ViewProjection);
    output.positionWorld = input.positionLocal;
    output.tex = input.tex;
    output.textureIndex = 0;

    return output;
}
