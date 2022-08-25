#include "..\Lib\IncVertexFormats.hlsl"

PSShadowMapPositionTexture main(VSVertexPositionNormalTextureTangentI input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), input.localTransform);
    output.depth = output.positionHomogeneous;
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;

    return output;
}
