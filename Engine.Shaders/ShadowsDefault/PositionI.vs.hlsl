
struct VSVertex
{
    float3 positionLocal : POSITION;
    row_major float4x4 localTransform : LOCALTRANSFORM;
    int materialIndex : MATERIALINDEX;
    uint instanceId : SV_INSTANCEID;
};

struct PSShadowMap
{
    float4 positionHomogeneous : SV_POSITION;
};

PSShadowMap main(VSVertex input)
{
    PSShadowMap output = (PSShadowMap) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), input.localTransform);
    
    return output;
}
