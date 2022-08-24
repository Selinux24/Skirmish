#include "..\Lib\IncVertexFormats.hlsl"

TextureCube gCubemap : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

float4 main(PSVertexPosition input) : SV_Target
{
    return gCubemap.Sample(SamplerLinear, input.positionWorld);
}
