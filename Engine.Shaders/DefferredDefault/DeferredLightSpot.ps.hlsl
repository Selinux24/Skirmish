#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerSpotLight : register(b4)
{
    SpotLight gSpotLight;
}

Texture2D gTG2Map : register(t0);
Texture2D gTG3Map : register(t1);
Texture2D gTG4Map : register(t2);
Texture2D gTG5Map : register(t3);
Texture2D gTG6Map : register(t4);
Texture2DArray<float> gShadowMapSpot : register(t5);

SamplerState SamplerPoint : register(s0)
{
    Filter = MIN_MAG_MIP_POINT;
};

struct PSLightInput
{
    float4 positionHomogeneous : SV_POSITION;
    float4 positionScreen : TEXCOORD0;
};
struct PSStencilInput
{
    float4 positionHomogeneous : SV_POSITION;
};

float4 main(PSLightInput input) : SV_TARGET
{
	//Get texture coordinates
    float4 lPosition = input.positionScreen;
    lPosition.xy /= lPosition.w;
    float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);

    float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
    
    float doLighting = tg2.w;
    if (doLighting == 0)
    {
        float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);
        float4 tg4 = gTG4Map.SampleLevel(SamplerPoint, tex, 0);
        float4 tg5 = gTG5Map.SampleLevel(SamplerPoint, tex, 0);
        float4 tg6 = gTG6Map.SampleLevel(SamplerPoint, tex, 0);

        float3 normal = tg2.xyz;
        float3 position = tg3.xyz;
        
        Material k = (Material) 0;
        k.Algorithm = (uint) tg3.w;
        k.Specular = tg4.rgb;
        k.Shininess = tg4.a;
        k.Emissive = tg5.rgb;
        k.Metallic = tg5.a;
        k.Ambient = tg6.rgb;
        k.Roughness = tg6.a;

        ComputeSpotLightsInput linput;

        linput.spotLight = gSpotLight;
        linput.material = k;
        linput.pPosition = position;
        linput.pNormal = normal;
        linput.ePosition = gPerFrame.EyePosition;
        linput.lod = gPerFrame.LOD;
        linput.shadowMap = gShadowMapSpot;
        linput.minShadowIntensity = gPerFrame.ShadowIntensity;

        ComputeLightsOutput loutput = ComputeSpotLight(linput);
        float3 diffuseSpecular = (k.Diffuse.rgb * loutput.diffuse) + (k.Specular * loutput.specular);
        
        return float4(diffuseSpecular, 1);
    }
    else
    {
        return 0;
    }
};
