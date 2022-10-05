#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

struct VSVertex
{
    float3 positionLocal : POSITION;
};

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
};

PSVertex main(VSVertex input)
{
    PSVertex output = (PSVertex) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gPerFrame.ViewProjection);
    output.positionWorld = input.positionLocal;

    return output;
}
