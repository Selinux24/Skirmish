#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerInstance : register(b1)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint gTextureIndex;
    uint2 PAD11;
    uint gAnimationOffset;
    uint gAnimationOffset2;
    float gAnimationInterpolation;
    float PAD12;
};

PSShadowMapPositionTexture main(VSVertexPositionTexture input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
