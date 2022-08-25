#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorld;
    float4x4 gWorldViewProjection;
};

struct GSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(9)]
void main(triangle PSShadowMapPosition input[3] : SV_Position, inout TriangleStream<GSShadowMap> outputStream)
{
    for (int iFace = 0; iFace < 3; iFace++)
    {
        GSShadowMap output;

        output.index = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.positionHomogeneous = mul(input[v].positionHomogeneous, gWorldViewProjection[iFace]);

            outputStream.Append(output);
        }
        outputStream.RestartStrip();
    }
}
