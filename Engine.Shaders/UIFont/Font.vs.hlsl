#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerText : register(b1)
{
    float4x4 gLocal;
};

struct VSVertexFont
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
    float4 color : COLOR0;
};

struct PSVertexFont
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
    float4 color : COLOR0;
};

PSVertexFont main(VSVertexFont input)
{
    PSVertexFont output = (PSVertexFont) 0;

    float4x4 wvp = mul(gLocal, gPerFrame.OrthoViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = output.positionHomogeneous.xyz;
    output.tex = input.tex;
    output.color = input.color;

    return output;
}
