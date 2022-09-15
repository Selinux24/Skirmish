#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMatrix.hlsl"

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

    float4x4 translation = translateToMatrix(gPerFrame.EyePosition);
    float4x4 wvp = mul(translation, gPerFrame.ViewProjection);
    
    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = input.positionLocal;
    output.tex = input.tex;
    output.textureIndex = 0;

    return output;
}
