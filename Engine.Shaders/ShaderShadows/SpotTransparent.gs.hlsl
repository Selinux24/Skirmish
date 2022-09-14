#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerLight : register(b0)
{
    float4x4 gFromLightViewProjection[1];
};

struct GSShadowMapTexture
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(3)]
void main(triangle PSShadowMapPositionTexture input[3] : SV_Position, inout TriangleStream<GSShadowMapTexture> outputStream)
{
    GSShadowMapTexture output;

    output.index = 0;

    for (int v = 0; v < 3; v++)
    {
        output.positionHomogeneous = mul(input[v].positionHomogeneous, gFromLightViewProjection[0]);
        output.depth = input[v].depth;
        output.tex = input[v].tex;
        output.textureIndex = input[v].textureIndex;
            
        outputStream.Append(output);
    }
}
