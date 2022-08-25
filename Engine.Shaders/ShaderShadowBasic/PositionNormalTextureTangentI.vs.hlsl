#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSShadowMapPositionTexture main(VSVertexPositionNormalTextureTangentI input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;

    return output;
}
