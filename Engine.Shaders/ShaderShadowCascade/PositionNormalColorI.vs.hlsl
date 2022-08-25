#include "..\Lib\IncVertexFormats.hlsl"

PSShadowMapPosition main(VSVertexPositionNormalColorI input)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), input.localTransform);

    return output;
}