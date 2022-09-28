#include "..\Lib\IncBuiltIn.hlsl"
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

    float4 tg1 = gTG1Map.SampleLevel(SamplerPoint, tex, 0);
    float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);

    float doLighting = tg2.w;
    if (doLighting == 0)
    {
        float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);
        float4 tg4 = gTG4Map.SampleLevel(SamplerPoint, tex, 0);
        float4 tg5 = gTG5Map.SampleLevel(SamplerPoint, tex, 0);
        float4 tg6 = gTG6Map.SampleLevel(SamplerPoint, tex, 0);
        float4 lmap = gLightMap.Sample(SamplerPoint, tex);

        float4 albedo = tg1;
        float3 position = tg3.xyz;
        float3 normal = tg2.xyz;
        float3 diffuseSpecular = lmap.rgb;
        
        Material k = (Material) 0;
        k.Algorithm = (uint) tg3.w;
        k.Specular = tg4.rgb;
        k.Shininess = tg4.a;
        k.Emissive = tg5.rgb;
        k.Metallic = tg5.a;
        k.Ambient = tg6.rgb;
        k.Roughness = tg6.a;

        float3 lAmbient = CalcAmbientHemispheric(gHemiLight.AmbientDown, gHemiLight.AmbientRange, normal);

        float3 light = DeferredLightEquation(k, lAmbient, diffuseSpecular);
        float4 color = float4(light, 1) * albedo;

        if (gPerFrame.FogRange > 0)
        {
            float distToEye = length(gPerFrame.EyePosition - position);

            color = ComputeFog(color, distToEye, gPerFrame.FogStart, gPerFrame.FogRange, gPerFrame.FogColor);
        }

        return saturate(color);
    }
    else
    {
        return tg1;
    }
};
