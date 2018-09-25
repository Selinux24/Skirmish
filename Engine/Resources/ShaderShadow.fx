///////////////////////////////////////////////////////////////////
// Spot shadow map generation
///////////////////////////////////////////////////////////////////
cbuffer cbSpotShadowGenVS : register(b0)
{
    float4x4 ShadowMat : packoffset(c0);
}

float4 SpotShadowGenVS(float4 Pos : POSITION) : SV_Position
{
    return mul(Pos, ShadowMat);
}

///////////////////////////////////////////////////////////////////
// Point shadow map generation
///////////////////////////////////////////////////////////////////
float4 PointShadowGenVS(float4 Pos : POSITION) : SV_Position
{
    return Pos;
}

cbuffer cbuffercbShadowMapCubeGS : register(b0)
{
    float4x4 CubeViewProj[6] : packoffset(c0);
};

struct GS_OUTPUT
{
    float4 Pos : SV_POSITION;
    uint RTIndex : SV_RenderTargetArrayIndex;
};

[maxvertexcount(18)]
void PointShadowGenGS(triangle float4 InPos[3] : SV_Position, inout TriangleStream<GS_OUTPUT> OutStream)
{
    for (int iFace = 0; iFace < 6; iFace++)
    {
        GS_OUTPUT output;

        output.RTIndex = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.Pos = mul(InPos[v], CubeViewProj[iFace]);
            OutStream.Append(output);
        }
        OutStream.RestartStrip();
    }
}

///////////////////////////////////////////////////////////////////
// Cascaded shadow maps generation
///////////////////////////////////////////////////////////////////
cbuffer cbuffercbShadowMapCubeGS : register(b0)
{
    float4x4 CascadeViewProj[3] : packoffset(c0);
};

[maxvertexcount(9)]
void CascadedShadowMapsGenGS(triangle float4 InPos[3] : SV_Position, inout TriangleStream<GS_OUTPUT> OutStream)
{
    for (int iFace = 0; iFace < 3; iFace++)
    {
        GS_OUTPUT output;

        output.RTIndex = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.Pos = mul(InPos[v], CascadeViewProj[iFace]);
            OutStream.Append(output);
        }
        OutStream.RestartStrip();
    }
}

technique11 SpotShadowGen
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, SpotShadowGenVS()));
        SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}

technique11 PointShadowGen
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, PointShadowGenVS()));
        SetGeometryShader(CompileShader(gs_5_0, PointShadowGenGS()));
        SetPixelShader(NULL);
    }
}

technique11 CascadedShadowMapsGen
{
    pass P0
    {
        SetVertexShader(NULL);
        SetGeometryShader(CompileShader(gs_5_0, CascadedShadowMapsGenGS()));
        SetPixelShader(NULL);
    }
}

