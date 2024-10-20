
cbuffer cbPerLight : register(b0)
{
    float4x4 gFromLightViewProjection[6];
    uint gFaceCount;
    uint3 PAD01;
};

struct PSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
};

struct GSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(18)]
void main(triangle PSShadowMap input[3] : SV_Position, inout TriangleStream<GSShadowMap> outputStream)
{
    for (uint iFace = 0; iFace < gFaceCount; iFace++)
    {
        GSShadowMap output;

        output.index = iFace;

        for (uint v = 0; v < 3; v++)
        {
            output.positionHomogeneous = mul(input[v].positionHomogeneous, gFromLightViewProjection[iFace]);
            output.depth = input[v].depth;
            output.tex = input[v].tex;
            output.textureIndex = input[v].textureIndex;
            
            outputStream.Append(output);
        }
        
        outputStream.RestartStrip();
    }
}
