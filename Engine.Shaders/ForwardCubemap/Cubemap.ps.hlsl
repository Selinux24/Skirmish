
TextureCube gCubemap : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
};

float4 main(PSVertex input) : SV_Target
{
    return gCubemap.Sample(SamplerLinear, input.positionWorld);
}
