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
PSShadowMapPositionTexture main(VSVertexPositionTextureI input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;

    return output;
}
