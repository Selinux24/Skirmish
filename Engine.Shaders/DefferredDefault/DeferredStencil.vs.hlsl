#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

struct VSVertexPosition
{
    float3 positionLocal : POSITION;
};

struct PSStencilInput
{
    float4 positionHomogeneous : SV_POSITION;
};

PSStencilInput main(VSVertexPosition input)
{
    PSStencilInput output = (PSStencilInput) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gPerFrame.OrthoViewProjection);

    return output;
}
