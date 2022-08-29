#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorld;
    float4x4 gWorldViewProjection;
};

PSVertexFont main(VSVertexFont input)
{
    PSVertexFont output = (PSVertexFont) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = output.positionHomogeneous.xyz;
    output.tex = input.tex;
    output.color = input.color;

    return output;
}
