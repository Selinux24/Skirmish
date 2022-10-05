#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
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

Texture2DArray<float> gShadowMapDir : register(t0);
Texture2DArray<float> gShadowMapSpot : register(t1);
TextureCubeArray<float> gShadowMapPoint : register(t2);
Texture2DArray gDiffuseMapArray : register(t3);

SamplerState SamplerDiffuse : register(s0);

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
float4 main(PSVertexPositionNormalTexture input) : SV_TARGET
{
    float4 diffuseColor = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

	ComputeLightsInput lInput;

	lInput.material = input.material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normalize(input.normalWorld);
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
