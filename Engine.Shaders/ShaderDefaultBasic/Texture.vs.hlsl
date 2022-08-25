
cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorld;
    float4x4 gWorldViewProjection;
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

    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
    output.tex = input.tex;

    return output;
}
