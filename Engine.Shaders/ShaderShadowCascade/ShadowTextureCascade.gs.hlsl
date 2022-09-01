#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

struct GSShadowMapTexture
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(9)]
void main(triangle PSShadowMapPositionTexture input[3] : SV_Position, inout TriangleStream<GSShadowMapTexture> outputStream)
{
    for (int iFace = 0; iFace < 3; iFace++)
    {
        GSShadowMapTexture output;

        output.index = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.positionHomogeneous = mul(input[v].positionHomogeneous, gPerFrame.ViewProjection[iFace]);
            output.depth = input[v].depth;
            output.tex = input[v].tex;
            output.textureIndex = input[v].textureIndex;

            outputStream.Append(output);
        }
        outputStream.RestartStrip();
    }
}
