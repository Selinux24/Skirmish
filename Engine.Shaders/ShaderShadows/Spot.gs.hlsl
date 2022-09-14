#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerLight : register(b0)
{
    float4x4 gFromLightViewProjection[1];
};

struct GSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(3)]
void main(triangle PSShadowMapPosition input[3] : SV_Position, inout TriangleStream<GSShadowMap> outputStream)
{
    GSShadowMap output;

    output.index = 0;

    for (int v = 0; v < 3; v++)
    {
        output.positionHomogeneous = mul(input[v].positionHomogeneous, gFromLightViewProjection[0]);
            
        outputStream.Append(output);
    }
}
