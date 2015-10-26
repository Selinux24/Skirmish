#include "IncLights.fx"
#include "IncVertexFormats.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldInverse;
	float4x4 gWorldViewProjection;
	float4x4 gLightViewProjection;
	float3 gEyePositionWorld;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPointLights[MAX_LIGHTS_POINT];
	SpotLight gSpotLights[MAX_LIGHTS_SPOT];
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
};
cbuffer cbPerObject : register (b1)
{
	Material gMaterial;
};

Texture2DArray gTextureArray;
Texture2D gNormalMap;
Texture2D gShadowMap;

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexTerrain VSTerrainForward(VSVertexTerrain input)
{
    PSVertexTerrain output = (PSVertexTerrain)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tangentWorld = mul(float4(input.tangentLocal, 0), gWorld).xyz;
	output.tex = input.tex;
	output.color = input.color;
    
    return output;
}
ShadowMapOutput VSTerrainShadowMap(VSVertexTerrain input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}

float4 PSTerrainForward(PSVertexTerrain input) : SV_TARGET
{
	float3 normalWorld = 0.0f;
	float4 textureColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, 0));

	float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
	if(depthValue < 0.75f)
	{
		normalWorld = input.normalWorld;
	}
	else
	{
		float3 normalMapSample = gNormalMap.Sample(SamplerLinear, input.tex).rgb;
		normalWorld = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

		textureColor *= gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, 1)) * 1.8f;
	}

	textureColor = saturate(textureColor * input.color * 2.0f);

	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
	float3 toEye = normalize(toEyeWorld);

	float4 shadowPosition = mul(float4(input.positionWorld, 1), gLightViewProjection);

	float3 litColor = ComputeAllLights(
		gDirLights, 
		gPointLights, 
		gSpotLights,
		toEye,
		textureColor.rgb,
		input.positionWorld,
		normalWorld,
		gMaterial.SpecularIntensity,
		gMaterial.SpecularPower,
		shadowPosition,
		gShadowMap);

	if(gFogRange > 0)
	{
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor.rgb);
	}

	return float4(litColor, textureColor.a);
}
GBufferPSOutput PSTerrainDeferred(PSVertexTerrain input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	float3 normal = 0.0f;
	float4 color = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, 0));

	float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
	if(depthValue < 0.75f)
	{
		normal = input.normalWorld;
	}
	else
	{
		float3 normalMapSample = gNormalMap.Sample(SamplerLinear, input.tex).rgb;
		normal = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

		color *= gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, 1)) * 1.8f;
	}

	color = saturate(color * input.color * 2.0f);

	output.color = color;
	output.normal = float4(normal.xyz, gMaterial.SpecularPower);
	output.depth = float4(input.positionWorld, gMaterial.SpecularIntensity);

    return output;
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 TerrainForward
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrainForward()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainForward()));
	}
}

technique11 TerrainDeferred
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrainForward()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainDeferred()));
	}
}

technique11 TerrainShadowMap
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrainShadowMap()));
		SetGeometryShader(NULL);
		SetPixelShader(NULL);
	}
}
