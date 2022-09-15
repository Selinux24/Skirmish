#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncMatrix.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

struct VSVertex
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
};

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float2 tex : TEXCOORD0;
};

PSVertex main(VSVertex input)
{
    PSVertex output = (PSVertex) 0;

    float4x4 translation = translateToMatrix(gPerFrame.EyePosition);
    float4x4 wvp = mul(translation, gPerFrame.ViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), wvp).xyww;
    output.tex = input.tex;

    return output;
}
