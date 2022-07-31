#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	float3 gLOD;
};
Texture2D gMaterialPalette : register(t0);

cbuffer cbPSPerFrame : register(b1)
{
	float3 gEyePositionWorld;
	float PAD11;
	float4 gFogColor;
	float gFogStart;
	float gFogRange;
	float2 PAD12;
	HemisphericLight gHemiLight;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPointLights[MAX_LIGHTS_POINT];
	SpotLight gSpotLights[MAX_LIGHTS_SPOT];
	uint3 gLightCount;
    float gShadowIntensity;
};
Texture2DArray<float> gShadowMapDir : register(t1);
Texture2DArray<float> gShadowMapSpot : register(t2);
TextureCubeArray<float> gShadowMapPoint : register(t3);

cbuffer cbPSPerObject : register(b2)
{
	bool gUseColorDiffuse;
	bool3 PAD21;
};
Texture2DArray gDiffuseMapArray : register(t4);
Texture2DArray gNormalMapArray : register(t5);

SamplerState SamplerDiffuse : register(s0);
SamplerState SamplerNormal : register(s1);

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
float4 main(PSVertexPositionNormalTextureTangent input) : SV_TARGET
{
	Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);

	float4 diffuseColor = 1;
	if (gUseColorDiffuse == true)
	{
		diffuseColor = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));
		diffuseColor *= input.tintColor;
	}

	float3 normalMap = gNormalMapArray.Sample(SamplerNormal, float3(input.tex, input.textureIndex)).rgb;
	float3 normalWorld = NormalSampleToWorldSpace(normalMap, input.normalWorld, input.tangentWorld);

	ComputeLightsInput lInput;

	lInput.material = material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normalWorld;
	lInput.objectDiffuseColor = diffuseColor;

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
