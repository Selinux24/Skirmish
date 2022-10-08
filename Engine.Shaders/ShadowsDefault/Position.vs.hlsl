
cbuffer cbPerMesh : register(b0)
{
    float4x4 gLocal;
};

struct VSVertex
{
    float3 positionLocal : POSITION;
};

struct PSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
};

PSShadowMap main(VSVertex input)
{
    PSShadowMap output = (PSShadowMap) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gLocal);

    return output;
}
