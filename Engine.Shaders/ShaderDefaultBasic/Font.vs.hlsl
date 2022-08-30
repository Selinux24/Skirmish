#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerText : register(b1)
{
    float4x4 gLocal;
};

PSVertexFont main(VSVertexFont input)
{
    PSVertexFont output = (PSVertexFont) 0;

    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = output.positionHomogeneous.xyz;
    output.tex = input.tex;
    output.color = input.color;

    return output;
}
