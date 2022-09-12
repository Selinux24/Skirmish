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
POSITION TEXTURE
**********************************************************************************************************/
PSSpriteTexture main(VSVertexPositionTexture input)
{
    PSSpriteTexture output = (PSSpriteTexture) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gPerFrame.ViewProjection);
    output.positionWorld = output.positionHomogeneous.xyz;
    output.tex = input.tex;
    
    return output;
}
