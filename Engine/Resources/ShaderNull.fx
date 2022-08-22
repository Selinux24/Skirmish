#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorldViewProjection;
};

float4 VSNull(VSVertexPosition input) : SV_POSITION
{
    return mul(float4(input.positionLocal, 1), gWorldViewProjection);
}

technique11 Null
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSNull()));
        SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}