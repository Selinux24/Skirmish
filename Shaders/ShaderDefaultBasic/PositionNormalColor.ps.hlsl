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

struct PSVertexPositionNormalColor2
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float4 color : COLOR0;
	Material material : MATERIAL;
};

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
float4 main(PSVertexPositionNormalColor2 input) : SV_TARGET
{
	ComputeLightsInput lInput;

	lInput.material = input.material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normalize(input.normalWorld);
	lInput.objectDiffuseColor = input.color;

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