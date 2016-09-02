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
	float2 gSlopeRanges;
};
cbuffer cbPerObject : register (b1)
{
	Material gMaterial;
};

Texture2DArray gTextureLRArray;
Texture2DArray gTextureHRArray;
Texture2DArray gNormalMapArray;
Texture2D gShadowMapStatic;
Texture2D gShadowMapDynamic;

Texture2DArray gColorTextureArray;
Texture2D gAlphaTexture;

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
	output.tex0 = input.tex0;
	output.tex1 = input.tex1;
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
	// BY ALPHA MAP
	float3 normalMapSample1 = gNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 0)).rgb;
	float3 bumpNormalWorld1 = NormalSampleToWorldSpace(normalMapSample1, input.normalWorld, input.tangentWorld);

	float3 normalMapSample2 = gNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 1)).rgb;
	float3 bumpNormalWorld2 = NormalSampleToWorldSpace(normalMapSample2, input.normalWorld, input.tangentWorld);

    float4 textureColor1 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 0));
    float4 textureColor2 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 1));
    float4 textureColor3 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 2));
    float4 textureColor4 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 3));

	float4 alphaMap1 = gAlphaTexture.Sample(SamplerLinear, input.tex1);

	float4 color1 = lerp(textureColor1, textureColor2, alphaMap1.r);
	color1 = lerp(color1, textureColor3, alphaMap1.g);
	color1 = lerp(color1, textureColor4, alphaMap1.b);

	// BY SLOPE. Determine which texture to use based on height.
	float4 color2 = 0;
	float slope = 1.0f - input.normalWorld.y;
    if(slope < gSlopeRanges.x)
    {
        color2 = lerp(
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)), 
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)), 
			slope / gSlopeRanges.x);
    }
    if((slope < gSlopeRanges.y) && (slope >= gSlopeRanges.x))
    {
        color2 = lerp(
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)), 
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2)), 
			(slope - gSlopeRanges.x) * (1.0f / (gSlopeRanges.y - gSlopeRanges.x)));
    }
    if(slope >= gSlopeRanges.y) 
    {
        color2 = gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
    }

	float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
	if(depthValue >= 0.05f)
	{
		color2 *= gTextureHRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)) * 1.8f;
	}

	float4 color = saturate(((color1 * 0.70f) + (color2 * 0.30f)) * input.color * 2.0f);

	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
	float3 toEye = normalize(toEyeWorld);

	float4 shadowPosition = mul(float4(input.positionWorld, 1), gLightViewProjection);

	float3 litColor = ComputeAllLights(
		gDirLights, 
		gPointLights, 
		gSpotLights,
		toEye,
		color.rgb,
		input.positionWorld,
		bumpNormalWorld1,
		gMaterial.SpecularIntensity,
		gMaterial.SpecularPower,
		shadowPosition,
		gShadowMapStatic,
		gShadowMapDynamic);

	if(gFogRange > 0)
	{
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor.rgb);
	}

	return float4(litColor, color.a);
}
GBufferPSOutput PSTerrainDeferred(PSVertexTerrain input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	// BY ALPHA MAP
	float3 normalMapSample1 = gNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 0)).rgb;
	float3 bumpNormalWorld1 = NormalSampleToWorldSpace(normalMapSample1, input.normalWorld, input.tangentWorld);

	float3 normalMapSample2 = gNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 1)).rgb;
	float3 bumpNormalWorld2 = NormalSampleToWorldSpace(normalMapSample2, input.normalWorld, input.tangentWorld);

    float4 textureColor1 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 0));
    float4 textureColor2 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 1));
    float4 textureColor3 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 2));
    float4 textureColor4 = gColorTextureArray.Sample(SamplerLinear, float3(input.tex0, 3));
	float4 alphaMap1 = gAlphaTexture.Sample(SamplerLinear, input.tex1);

	float4 color1 = lerp(textureColor1, textureColor2, alphaMap1.r);
	color1 = lerp(color1, textureColor3, alphaMap1.g);
	color1 = lerp(color1, textureColor4, alphaMap1.b);

	// BY SLOPE. Determine which texture to use based on height.
	float4 color2 = 0;
	float slope = 1.0f - input.normalWorld.y;
    if(slope < gSlopeRanges.x)
    {
        color2 = lerp(
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)), 
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)), 
			slope / gSlopeRanges.x);
    }
    if((slope < gSlopeRanges.y) && (slope >= gSlopeRanges.x))
    {
        color2 = lerp(
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)), 
			gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2)), 
			(slope - gSlopeRanges.x) * (1.0f / (gSlopeRanges.y - gSlopeRanges.x)));
    }
    if(slope >= gSlopeRanges.y) 
    {
        color2 = gTextureLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
    }

	float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
	if(depthValue >= 0.05f)
	{
		color2 *= gTextureHRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)) * 1.8f;
	}

	float4 color = saturate(((color1 * 0.70f) + (color2 * 0.30f)) * input.color * 2.0f);

	output.color = color;
	output.normal = float4(bumpNormalWorld1.xyz, gMaterial.SpecularPower);
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
