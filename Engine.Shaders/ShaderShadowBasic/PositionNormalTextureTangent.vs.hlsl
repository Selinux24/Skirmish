#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerMesh : register(b1)
{
    float4x4 gLocal;
};

cbuffer cbPerMaterial : register(b2)
{
    uint gTextureIndex;
    uint3 PAD21;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSShadowMapPositionTexture main(VSVertexPositionNormalTextureTangent input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), wvp);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
