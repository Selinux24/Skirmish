#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerMaterial : register(b0)
{
    uint gTextureIndex;
    uint3 PAD01;
};

PSShadowMapPositionTexture main(VSVertexPositionNormalTexture input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
