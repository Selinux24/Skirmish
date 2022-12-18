#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncGBuffer.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbCombineLights : register(b1)
{
    HemisphericLight gHemiLight;
}

Texture2D gTG1Map : register(t0);
Texture2D gTG2Map : register(t1);
Texture2D gTG3Map : register(t2);
Texture2D gTG4Map : register(t3);
Texture2D gTG5Map : register(t4);
Texture2D gTG6Map : register(t5);

Texture2D gLightMap : register(t6);

SamplerState SamplerPoint : register(s0)
{
    Filter = MIN_MAG_MIP_POINT;
};

struct PSLightInput
{
    float4 positionHomogeneous : SV_POSITION;
    float4 positionScreen : TEXCOORD0;
};

float4 main(PSLightInput input) : SV_TARGET
{
	//Get texture coordinates
    float4 lPosition = input.positionScreen;
    lPosition.xy /= lPosition.w;
    float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);
    
    GBuffer gBuffer;
    gBuffer.color = gTG1Map.SampleLevel(SamplerPoint, tex, 0);
    gBuffer.normal = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
    gBuffer.depth = gTG3Map.SampleLevel(SamplerPoint, tex, 0);
    gBuffer.mat1 = gTG4Map.SampleLevel(SamplerPoint, tex, 0);
    gBuffer.mat2 = gTG5Map.SampleLevel(SamplerPoint, tex, 0);
    gBuffer.mat3 = gTG6Map.SampleLevel(SamplerPoint, tex, 0);
    
    float3 position;
    float3 normal;
    float4 albedo;
    float doLighting;
    Material k;
    UnPack(gBuffer, position, normal, albedo, doLighting, k);
    
    if (!doLighting)
    {
        return albedo;
    }

    float3 diffuseSpecular = gLightMap.SampleLevel(SamplerPoint, tex, 0).rgb;

    float3 lAmbient = CalcAmbientHemispheric(gHemiLight.AmbientDown, gHemiLight.AmbientRange, normal);

    float3 light = DeferredLightEquation(k, lAmbient, diffuseSpecular);
    float4 color = float4(light, 1) * albedo;

    if (gPerFrame.FogRange > 0)
    {
        float distToEye = length(gPerFrame.EyePosition - position);

        color = ComputeFog(color, distToEye, gPerFrame.FogStart, gPerFrame.FogRange, gPerFrame.FogColor);
    }

    return saturate(color);
};
