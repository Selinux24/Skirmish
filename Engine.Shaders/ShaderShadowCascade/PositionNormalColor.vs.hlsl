#include "..\Lib\IncVertexFormats.hlsl"

PSShadowMapPosition main(VSVertexPositionNormalColor input)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);

    return output;
}
