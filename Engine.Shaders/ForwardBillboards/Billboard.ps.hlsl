#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbHemispheric : register(b1)
{
    HemisphericLight gHemiLight;
};

cbuffer cbDirectionals : register(b2)
{
    uint gDirLightsCount;
    DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
};

cbuffer cbSpots : register(b3)
{
    uint gSpotLightsCount;
    SpotLight gSpotLights[MAX_LIGHTS_SPOT];
};

cbuffer cbPoints : register(b4)
{
    uint gPointLightsCount;
    PointLight gPointLights[MAX_LIGHTS_POINT];
};

cbuffer cbPerMaterial : register(b5)
{
    float4 gTintColor;
    
    uint gMaterialIndex;
    uint gTextureCount;
    uint gNormalMapCount;
    uint PAD51;

    float gStartRadius;
    float gEndRadius;
    float2 PAD52;
};

Texture1D gTextureRandom : register(t0);
Texture2DArray<float> gShadowMapDir : register(t1);
Texture2DArray<float> gShadowMapSpot : register(t2);
TextureCubeArray<float> gShadowMapPoint : register(t3);
Texture2DArray gTextureArray : register(t4);
Texture2DArray gNormalMapArray : register(t5);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

struct PSVertexBillboard
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    Material material;
    uint primitiveID : SV_PRIMITIVEID;
};

float4 main(PSVertexBillboard input) : SV_Target
{
    float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);

    float4 diffuseColor = gTextureArray.Sample(SamplerLinear, uvw);
    clip(diffuseColor.a - 0.01f);

    float3 normalWorld = normalize(input.normalWorld);
    if (gNormalMapCount > 0)
    {
        float3 normalMap = gNormalMapArray.Sample(SamplerLinear, uvw).rgb;
        normalWorld = NormalSampleToWorldSpace(normalMap, input.normalWorld, input.tangentWorld);
    }

    ComputeLightsInput lInput;

    lInput.material = input.material;
    lInput.objectPosition = input.positionWorld;
    lInput.objectNormal = normalWorld;
    lInput.objectDiffuseColor = diffuseColor * input.tintColor;

    lInput.eyePosition = gPerFrame.EyePosition;
    lInput.levelOfDetailRanges = gPerFrame.LOD;

    lInput.hemiLight = gHemiLight;
    
    lInput.dirLightsCount = gDirLightsCount;
    lInput.dirLights = gDirLights;
	
    lInput.pointLightsCount = gPointLightsCount;
    lInput.pointLights = gPointLights;
	
    lInput.spotLightsCount = gSpotLightsCount;
    lInput.spotLights = gSpotLights;

    lInput.shadowMapDir = gShadowMapDir;
    lInput.shadowMapPoint = gShadowMapPoint;
    lInput.shadowMapSpot = gShadowMapSpot;
    lInput.minShadowIntensity = gPerFrame.ShadowIntensity;

    lInput.fogStart = gPerFrame.FogStart;
    lInput.fogRange = gPerFrame.FogRange;
    lInput.fogColor = gPerFrame.FogColor;

    return ComputeLights(lInput);
}
