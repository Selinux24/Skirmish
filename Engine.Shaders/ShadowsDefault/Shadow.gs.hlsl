
cbuffer cbPerLight : register(b0)
{
    float4x4 gFromLightViewProjection[6];
    uint gFaceCount;
    uint3 PAD01;
};

struct PSShadowMapPosition
{
    float4 positionHomogeneous : SV_POSITION;
};

struct GSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(18)]
void main(triangle PSShadowMapPosition input[3] : SV_Position, inout TriangleStream<GSShadowMap> outputStream)
{
    GSShadowMap output;
    
    for (uint iFace = 0; iFace < gFaceCount; iFace++)
    {
        output.index = iFace;

        for (uint v = 0; v < 3; v++)
        {
            output.positionHomogeneous = mul(input[v].positionHomogeneous, gFromLightViewProjection[iFace]);
            
            outputStream.Append(output);
        }
        
        outputStream.RestartStrip();
    }
}
