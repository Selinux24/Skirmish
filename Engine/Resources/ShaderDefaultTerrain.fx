#include "IncTerrain.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register (b0)
{
	uint gMaterialPaletteWidth;
	float3 gLOD;
};
Texture2D gMaterialPalette;

cbuffer cbVSPerFrame : register (b1)
{
	float4x4 gVSWorld;
	float4x4 gVSWorldViewProjection;
	float gVSTextureResolution;
	float3 PAD_11;
};

cbuffer cbPSPerFrame : register (b3)
{
	float4x4 gPSLightViewProjection;
	float3 gPSEyePositionWorld;
	float gPSGlobalAmbient;
	uint3 gPSLightCount;
	uint gPSShadows;
	float4 gPSFogColor;
	float gPSFogStart;
	float gPSFogRange;
	float2 PAD_33;
	DirectionalLight gPSDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPSPointLights[MAX_LIGHTS_POINT];
	SpotLight gPSSpotLights[MAX_LIGHTS_SPOT];
};
Texture2D gPSShadowMapStatic;
Texture2D gPSShadowMapDynamic;

cbuffer cbPSPerObject : register (b4)
{
	float4 gPSParams;
	bool gPSUseColorDiffuse;
	bool gPSUseColorSpecular;
	uint gPSMaterialIndex;
	uint PAD_44;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexTerrain VSTerrain(VSVertexTerrain input)
{
	PSVertexTerrain output = (PSVertexTerrain)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gVSWorld));
	output.tangentWorld = normalize(mul(input.tangentLocal, (float3x3)gVSWorld));
	output.tex0 = input.tex * gVSTextureResolution;
	output.tex1 = input.tex;
	output.color = input.color;

	return output;
}

float4 PSTerrainAlphaMap(PSVertexTerrain input) : SV_TARGET
{
	float4 specular;
	float3 normal;
	float4 color = AlphaMap(input, specular, normal);

	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	float4 lightPosition = mul(float4(input.positionWorld, 1), gPSLightViewProjection);

	ComputeLightsInput lInput;

	lInput.Ga = gPSGlobalAmbient;
	lInput.dirLights = gPSDirLights;
	lInput.pointLights = gPSPointLights;
	lInput.spotLights = gPSSpotLights;
	lInput.dirLightsCount = gPSLightCount.x;
	lInput.pointLightsCount = gPSLightCount.y;
	lInput.spotLightsCount = gPSLightCount.z;
	lInput.fogStart = gPSFogStart;
	lInput.fogRange = gPSFogRange;
	lInput.fogColor = gPSFogColor;
	lInput.k = material;
	lInput.pPosition = input.positionWorld;
	lInput.pNormal = normal;
	lInput.pColorDiffuse = color;
	lInput.pColorSpecular = 1;
	lInput.ePosition = gPSEyePositionWorld;
	lInput.sLightPosition = lightPosition;
	lInput.shadows = gPSShadows;
	lInput.shadowMapStatic = gPSShadowMapStatic;
	lInput.shadowMapDynamic = gPSShadowMapDynamic;
	lInput.lod = gLOD;

	return ComputeLights(lInput);
}

float4 PSTerrainSlopes(PSVertexTerrain input) : SV_TARGET
{
	float4 specular;
	float3 normal;
	float4 color = Slopes(input, gPSParams.z, gPSParams.w, specular, normal);

	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	float4 lightPosition = mul(float4(input.positionWorld, 1), gPSLightViewProjection);

	ComputeLightsInput lInput;

	lInput.Ga = gPSGlobalAmbient;
	lInput.dirLights = gPSDirLights;
	lInput.pointLights = gPSPointLights;
	lInput.spotLights = gPSSpotLights;
	lInput.dirLightsCount = gPSLightCount.x;
	lInput.pointLightsCount = gPSLightCount.y;
	lInput.spotLightsCount = gPSLightCount.z;
	lInput.fogStart = gPSFogStart;
	lInput.fogRange = gPSFogRange;
	lInput.fogColor = gPSFogColor;
	lInput.k = material;
	lInput.pPosition = input.positionWorld;
	lInput.pNormal = normal;
	lInput.pColorDiffuse = color;
	lInput.pColorSpecular = 1;
	lInput.ePosition = gPSEyePositionWorld;
	lInput.sLightPosition = lightPosition;
	lInput.shadows = gPSShadows;
	lInput.shadowMapStatic = gPSShadowMapStatic;
	lInput.shadowMapDynamic = gPSShadowMapDynamic;
	lInput.lod = gLOD;

	return ComputeLights(lInput);
}

float4 PSTerrainFull(PSVertexTerrain input) : SV_TARGET
{
	float4 specular;
	float3 normal;
	float4 color = Full(input, gPSParams.y, gPSParams.z, gPSParams.w, specular, normal);

	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	float4 lightPosition = mul(float4(input.positionWorld, 1), gPSLightViewProjection);

	ComputeLightsInput lInput;

	lInput.Ga = gPSGlobalAmbient;
	lInput.dirLights = gPSDirLights;
	lInput.pointLights = gPSPointLights;
	lInput.spotLights = gPSSpotLights;
	lInput.dirLightsCount = gPSLightCount.x;
	lInput.pointLightsCount = gPSLightCount.y;
	lInput.spotLightsCount = gPSLightCount.z;
	lInput.fogStart = gPSFogStart;
	lInput.fogRange = gPSFogRange;
	lInput.fogColor = gPSFogColor;
	lInput.k = material;
	lInput.pPosition = input.positionWorld;
	lInput.pNormal = normal;
	lInput.pColorDiffuse = color;
	lInput.pColorSpecular = 1;
	lInput.ePosition = gPSEyePositionWorld;
	lInput.sLightPosition = lightPosition;
	lInput.shadows = gPSShadows;
	lInput.shadowMapStatic = gPSShadowMapStatic;
	lInput.shadowMapDynamic = gPSShadowMapDynamic;
	lInput.lod = gLOD;

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