
cbuffer cbPerMesh : register(b0)
{
    float4x4 gLocal;
};

cbuffer cbPerMaterial : register(b1)
{
    uint gTextureIndex;
    uint3 PAD21;
};

struct VSVertex
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
};

struct PSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
};

PSShadowMap main(VSVertex input)
{
    PSShadowMap output = (PSShadowMap) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gLocal);
    output.depth = float4(input.positionLocal, 1);
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;

    return output;
}
