#include "..\Lib\IncVertexFormats.hlsl"

PSShadowMapPositionTexture main(VSVertexPositionTextureI input)
{
    PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), input.localTransform);
    output.depth = float4(input.positionLocal, 1);
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;
    
    return output;
}
