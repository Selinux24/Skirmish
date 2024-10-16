
cbuffer cbPerDecal : register(b0)
{
    bool gRotation;
    uint gTextureCount;
    uint2 PAD01;
    
    float4 gTintColor;
}

Texture2DArray gTextureArray : register(t0);

SamplerState SamplerPointDecal : register(s0)
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct PSDecal
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float4 rotationWorld : ROTATION;
    float alpha : ALPHA;
    float2 tex : TEXCOORD0;
    uint primitiveID : SV_PRIMITIVEID;
};

float4 main(PSDecal input) : SV_Target
{
    float2 tex = input.tex;

    if (gRotation)
    {
        float4 rot = (input.rotationWorld * 2.0f) - 1.0f;
        tex -= 0.5f;
        tex = mul(tex, float2x2(rot));
        tex *= sqrt(2.0f);
        tex += 0.5f;
    }
    
    float3 uvw = float3(tex, input.primitiveID % gTextureCount);
    float4 color = gTextureArray.Sample(SamplerPointDecal, uvw);
    color *= gTintColor;
    color.a *= input.alpha;
	
    return color;
}
