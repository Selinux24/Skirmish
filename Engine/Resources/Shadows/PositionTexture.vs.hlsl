#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

cbuffer cbPerInstance : register(b1)
{
    uint gTextureIndex;
    uint3 PAD51;
};

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSShadowMapPositionTexture main(VSVertexPositionTexture input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
