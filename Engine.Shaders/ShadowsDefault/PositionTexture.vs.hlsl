#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerMesh : register(b0)
{
    float4x4 gLocal;
};

cbuffer cbPerMaterial : register(b1)
{
    uint gTextureIndex;
    uint3 PAD21;
};

PSShadowMapPositionTexture main(VSVertexPositionTexture input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gLocal);
    output.depth = float4(input.positionLocal, 1);
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
