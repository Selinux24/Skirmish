#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerLight : register(b0)
{
    float4x4 gFromLightViewProjection[6];
};

struct GSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(18)]
void main(triangle PSShadowMapPosition input[3] : SV_Position, inout TriangleStream<GSShadowMap> outputStream)
{
    for (int iFace = 0; iFace < 6; iFace++)
    {
        GSShadowMap output;

        output.index = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.positionHomogeneous = mul(input[v].positionHomogeneous, gFromLightViewProjection[iFace]);
            
            outputStream.Append(output);
        }
        outputStream.RestartStrip();
    }
}
