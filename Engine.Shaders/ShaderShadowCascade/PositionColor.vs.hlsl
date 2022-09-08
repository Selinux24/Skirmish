#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerMesh : register(b0)
{
    float4x4 gLocal;
};

PSShadowMapPosition main(VSVertexPositionColor input)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gLocal);

    return output;
}
