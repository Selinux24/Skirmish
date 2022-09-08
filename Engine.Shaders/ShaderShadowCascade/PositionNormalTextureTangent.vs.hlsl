#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerMesh : register(b0)
{
    float4x4 gLocal;
};

cbuffer cbPerMaterial : register(b1)
{
    uint gTextureIndex;
    uint3 PAD11;
};

PSShadowMapPositionTexture main(VSVertexPositionNormalTextureTangent input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gLocal);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
