#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerLight : register(b1)
{
    float4x4 gLocal;
};

struct VSVertexPosition
{
    float3 positionLocal : POSITION;
};

struct PSLightInput
{
    float4 positionHomogeneous : SV_POSITION;
    float4 positionScreen : TEXCOORD0;
};

PSLightInput main(VSVertexPosition input)
{
    PSLightInput output = (PSLightInput) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), mul(gLocal, gPerFrame.ViewProjection));
    output.positionScreen = output.positionHomogeneous;

    return output;
}
