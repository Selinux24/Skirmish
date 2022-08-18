#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPSPerFrame : register(b0)
{
    float3 gEyePositionWorld;
    float PAD11;
    float4 gFogColor;
    float gFogStart;
    float gFogRange;
    float2 PAD12;
    float3 gLOD;
    float PAD13;
    uint3 gLightCount;
    float gShadowIntensity;
    HemisphericLight gHemiLight;
    DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
    PointLight gPointLights[MAX_LIGHTS_POINT];
    SpotLight gSpotLights[MAX_LIGHTS_SPOT];
};

Texture2DArray<float> gShadowMapDir : register(t0);
Texture2DArray<float> gShadowMapSpot : register(t1);
TextureCubeArray<float> gShadowMapPoint : register(t2);
Texture2DArray gDiffuseMapArray : register(t3);
Texture2DArray gNormalMapArray : register(t4);

SamplerState SamplerDiffuse : register(s0);
SamplerState SamplerNormal : register(s1);

struct PSVertexPositionNormalTextureTangent2
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    Material material : MATERIAL;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
float4 main(PSVertexPositionNormalTextureTangent2 input) : SV_TARGET
{
    float4 diffuseColor = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));
	float3 normalMap = gNormalMapArray.Sample(SamplerNormal, float3(input.tex, input.textureIndex)).rgb;
	float3 normalWorld = NormalSampleToWorldSpace(normalMap, input.normalWorld, input.tangentWorld);

	ComputeLightsInput lInput;

	lInput.material = input.material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normalWorld;
    lInput.objectDiffuseColor = diffuseColor * input.tintColor;

	lInput.eyePosition = gEyePositionWorld;
	lInput.levelOfDetailRanges = gLOD;

	lInput.hemiLight = gHemiLight;
	lInput.dirLights = gDirLights;
	lInput.pointLights = gPointLights;
	lInput.spotLights = gSpotLights;
	lInput.dirLightsCount = gLightCount.x;
	lInput.pointLightsCount = gLightCount.y;
	lInput.spotLightsCount = gLightCount.z;

	lInput.shadowMapDir = gShadowMapDir;
	lInput.shadowMapPoint = gShadowMapPoint;
	lInput.shadowMapSpot = gShadowMapSpot;
    lInput.minShadowIntensity = gShadowIntensity;

	lInput.fogStart = gFogStart;
	lInput.fogRange = gFogRange;
	lInput.fogColor = gFogColor;

	return ComputeLights(lInput);
}
