
cbuffer cbPerLight : register(b0)
{
    float4x4 gFromLightViewProjection[6];
    uint gFaceCount;
    uint3 PAD01;
};

struct PSShadowMapPositionTexture
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
};

struct GSShadowMapTexture
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(18)]
void main(triangle PSShadowMapPositionTexture input[3] : SV_Position, inout TriangleStream<GSShadowMapTexture> outputStream)
{
    for (uint iFace = 0; iFace < gFaceCount; iFace++)
    {
        GSShadowMapTexture output;

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
