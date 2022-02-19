#include "..\Lib\IncTerrain.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	float3 gLOD;
};
Texture2D gMaterialPalette : register(t0);

cbuffer cbVSPerFrame : register(b1)
{
	float4x4 gVSWorld;
	float4x4 gVSWorldViewProjection;
	float gVSTextureResolution;
	float3 PAD_11;
};

cbuffer cbPSPerFrame : register(b3)
{
	HemisphericLight gPSHemiLight;
	DirectionalLight gPSDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPSPointLights[MAX_LIGHTS_POINT];
	SpotLight gPSSpotLights[MAX_LIGHTS_SPOT];
	uint3 gPSLightCount;
    float gPSShadowIntensity;
	float4 gPSFogColor;
	float gPSFogStart;
	float gPSFogRange;
	float2 PAD32;
	float3 gPSEyePositionWorld;
	float PAD33;
};
Texture2DArray<float> gPSShadowMapDir : register(t2);
Texture2DArray<float> gPSShadowMapSpot : register(t3);
TextureCubeArray<float> gPSShadowMapPoint : register(t4);

cbuffer cbPSPerObject : register(b4)
{
	float4 gPSParams;
	bool gPSUseColorDiffuse;
	uint gPSMaterialIndex;
	uint2 PAD_41;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexTerrain VSTerrain(VSVertexTerrain input)
{
	PSVertexTerrain output = (PSVertexTerrain) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3) gVSWorld));
	output.tangentWorld = normalize(mul(input.tangentLocal, (float3x3) gVSWorld));
	output.tex0 = input.tex * gVSTextureResolution;
	output.tex1 = input.tex;
	output.color = input.color;

	return output;
}

float4 PSTerrainAlphaMap(PSVertexTerrain input) : SV_TARGET
{
	float3 normal;
	float4 color = AlphaMap(input, normal);

	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	ComputeLightsInput lInput;

    lInput.material = material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normal;
    lInput.objectDiffuseColor = color;

	lInput.eyePosition = gPSEyePositionWorld;
	lInput.levelOfDetailRanges = gLOD;

	lInput.hemiLight = gPSHemiLight;
	lInput.dirLights = gPSDirLights;
	lInput.pointLights = gPSPointLights;
	lInput.spotLights = gPSSpotLights;
	lInput.dirLightsCount = gPSLightCount.x;
	lInput.pointLightsCount = gPSLightCount.y;
	lInput.spotLightsCount = gPSLightCount.z;

	lInput.shadowMapDir = gPSShadowMapDir;
    lInput.shadowMapPoint = gPSShadowMapPoint;
	lInput.shadowMapSpot = gPSShadowMapSpot;
    lInput.minShadowIntensity = gPSShadowIntensity;

	lInput.fogStart = gPSFogStart;
	lInput.fogRange = gPSFogRange;
	lInput.fogColor = gPSFogColor;

	return ComputeLights(lInput);
}

float4 PSTerrainSlopes(PSVertexTerrain input) : SV_TARGET
{
	float3 normal;
	float4 color = Slopes(input, gPSParams.z, gPSParams.w, normal);

	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	ComputeLightsInput lInput;

    lInput.material = material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normal;
    lInput.objectDiffuseColor = color;

	lInput.eyePosition = gPSEyePositionWorld;
	lInput.levelOfDetailRanges = gLOD;

	lInput.hemiLight = gPSHemiLight;
	lInput.dirLights = gPSDirLights;
	lInput.pointLights = gPSPointLights;
	lInput.spotLights = gPSSpotLights;
	lInput.dirLightsCount = gPSLightCount.x;
	lInput.pointLightsCount = gPSLightCount.y;
	lInput.spotLightsCount = gPSLightCount.z;

	lInput.shadowMapDir = gPSShadowMapDir;
    lInput.shadowMapPoint = gPSShadowMapPoint;
	lInput.shadowMapSpot = gPSShadowMapSpot;
    lInput.minShadowIntensity = gPSShadowIntensity;

	lInput.fogStart = gPSFogStart;
	lInput.fogRange = gPSFogRange;
	lInput.fogColor = gPSFogColor;

	return ComputeLights(lInput);
}

float4 PSTerrainFull(PSVertexTerrain input) : SV_TARGET
{
	float3 normal;
	float4 color = Full(input, gPSParams.y, gPSParams.z, gPSParams.w, normal);

	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	ComputeLightsInput lInput;

    lInput.material = material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normal;
    lInput.objectDiffuseColor = color;

	lInput.eyePosition = gPSEyePositionWorld;
	lInput.levelOfDetailRanges = gLOD;

	lInput.hemiLight = gPSHemiLight;
	lInput.dirLights = gPSDirLights;
	lInput.pointLights = gPSPointLights;
	lInput.spotLights = gPSSpotLights;
	lInput.dirLightsCount = gPSLightCount.x;
	lInput.pointLightsCount = gPSLightCount.y;
	lInput.spotLightsCount = gPSLightCount.z;

	lInput.shadowMapDir = gPSShadowMapDir;
    lInput.shadowMapPoint = gPSShadowMapPoint;
	lInput.shadowMapSpot = gPSShadowMapSpot;
    lInput.minShadowIntensity = gPSShadowIntensity;
	
	lInput.fogStart = gPSFogStart;
	lInput.fogRange = gPSFogRange;
	lInput.fogColor = gPSFogColor;

	return ComputeLights(lInput);
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 TerrainAlphaMapForward
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainAlphaMap()));
	}
}
technique11 TerrainSlopesForward
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainSlopes()));
	}
}
technique11 TerrainFullForward
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainFull()));
	}
}