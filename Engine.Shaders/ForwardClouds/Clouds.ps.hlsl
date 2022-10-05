#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerCloud : register(b0)
{
    bool gPerturbed;
    float gTranslation;
    float gScale;
    float gFadingDistance;

    float2 gFirstTranslation;
    float2 gSecondTranslation;

    float3 gColor;
    float gBrightness;
};

Texture2D gCloudTexture1 : register(t0);
Texture2D gCloudTexture2 : register(t1);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};
SamplerState SamplerAnisotropic : register(s1)
{
    Filter = ANISOTROPIC;
    MaxAnisotropy = 4;
    AddressU = WRAP;
    AddressV = WRAP;
};

inline float RandomScalar(float seed, Texture1D rndTex)
{
    return rndTex.SampleLevel(SamplerLinear, seed, 0).x;
}
inline float2 RandomVector2(float seed, Texture1D rndTex)
{
    return rndTex.SampleLevel(SamplerLinear, seed, 0).xy;
}
inline float3 RandomVector3(float seed, Texture1D rndTex)
{
    return rndTex.SampleLevel(SamplerLinear, seed, 0).xyz;
}
inline float4 RandomVector4(float seed, Texture1D rndTex)
{
    return rndTex.SampleLevel(SamplerLinear, seed, 0);
}

inline float RandomScalar(float min, float max, float seed, Texture1D rndTex)
{
    float r = rndTex.SampleLevel(SamplerLinear, seed, 0).x;

    return roll(r, min, max);
}
inline float2 RandomVector2(float min, float max, float seed, Texture1D rndTex)
{
    float2 r = rndTex.SampleLevel(SamplerLinear, seed, 0).xy;
    r.x = roll(r.x, min, max);
    r.y = roll(r.y, min, max);

    return r;
}
inline float3 RandomVector3(float min, float max, float seed, Texture1D rndTex)
{
    float3 r = rndTex.SampleLevel(SamplerLinear, seed, 0).xyz;
    r.x = roll(r.x, min, max);
    r.y = roll(r.y, min, max);
    r.z = roll(r.z, min, max);

    return r;
}
inline float4 RandomVector4(float min, float max, float seed, Texture1D rndTex)
{
    float4 r = rndTex.SampleLevel(SamplerLinear, seed, 0);
    r.x = roll(r.x, min, max);
    r.y = roll(r.y, min, max);
    r.z = roll(r.z, min, max);
    r.w = roll(r.w, min, max);

    return r;
}

struct PSVertexCloud
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
};

float4 main(PSVertexCloud input) : SV_TARGET
{
    float4 color;
    if (gPerturbed)
    {
        input.tex.x += gTranslation;

        // Sample the texture value from the perturb texture using the translated texture coordinates.
        float4 perturbValue = gCloudTexture1.Sample(SamplerAnisotropic, input.tex) * gScale;

        // Add the texture coordinates as well as the translation value to get the perturbed texture coordinate sampling location.
        perturbValue.xy += (input.tex.xy + gTranslation);

        // Now sample the color from the cloud texture using the perturbed sampling coordinates.
        color = gCloudTexture2.Sample(SamplerAnisotropic, perturbValue.xy) * float4(gColor, 1) * gBrightness;
    }
    else 
    {
        float4 color1 = gCloudTexture1.Sample(SamplerAnisotropic, input.tex + gFirstTranslation);
        float4 color2 = gCloudTexture2.Sample(SamplerAnisotropic, input.tex + gSecondTranslation);

        color = lerp(color1, color2, 0.5f) * float4(gColor, 1) * gBrightness;
    }

    if (gFadingDistance > 0)
    {
        color.a = saturate(1 - (length(input.positionWorld) / gFadingDistance));
    }

    return color;
}
