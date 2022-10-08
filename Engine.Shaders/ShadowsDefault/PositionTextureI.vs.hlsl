
struct VSVertex
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
    row_major float4x4 localTransform : LOCALTRANSFORM;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    int materialIndex : MATERIALINDEX;
    uint instanceId : SV_INSTANCEID;
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

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), input.localTransform);
    output.depth = float4(input.positionLocal, 1);
    output.tex = input.tex;
    output.textureIndex = input.textureIndex;
    
    return output;
}
