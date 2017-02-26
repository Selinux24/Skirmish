#include "IncLights.fx"
#include "IncVertexFormats.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register (b0)
{
    uint gMaterialPaletteWidth;
	uint3 PAD_B0;
};
Texture2D gMaterialPalette;

cbuffer cbVSPerFrame : register (b1)
{
	float4x4 gVSWorld;
	float4x4 gVSWorldViewProjection;
	float gVSTextureResolution;
	float3 PAD_B1;
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
	float2 PAD_B3;
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
	uint PAD_B4;
};
Texture2DArray gPSNormalMapArray;
Texture2DArray gPSSpecularMapArray;
Texture2DArray gPSColorTextureArray;
Texture2D gPSAlphaTexture;
Texture2DArray gPSDiffuseMapLRArray;
Texture2DArray gPSDiffuseMapHRArray;

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

float4 PSTerrain(PSVertexTerrain input) : SV_TARGET
{
	float usage = gPSParams.x;
	float prop = gPSParams.y;
	float slope1 = gPSParams.z;
	float slope2 = gPSParams.w;

	float3 normalMapSample1 = gPSNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 0)).rgb;
	float3 bumpNormalWorld1 = NormalSampleToWorldSpace(normalMapSample1, input.normalWorld, input.tangentWorld);
	float3 normalMapSample2 = gPSNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 1)).rgb;
	float3 bumpNormalWorld2 = NormalSampleToWorldSpace(normalMapSample2, input.normalWorld, input.tangentWorld);

	float4 specularMapSample1 = gPSSpecularMapArray.Sample(SamplerLinear, float3(input.tex0, 0));
	float4 specularMapSample2 = gPSSpecularMapArray.Sample(SamplerLinear, float3(input.tex0, 1));

	float n = 0;

	float4 color1 = 0;

	[flatten]
	if (usage == 1.0f || usage == 3.0f)
	{
		// BY ALPHA MAP
		float4 textureColor1 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 0));
		float4 textureColor2 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 1));
		float4 textureColor3 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
		float4 textureColor4 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 3));

		float4 alphaMap1 = gPSAlphaTexture.Sample(SamplerLinear, input.tex1);

		color1 = lerp(textureColor1, textureColor2, alphaMap1.r);
		color1 = lerp(color1, textureColor3, alphaMap1.g);
		color1 = lerp(color1, textureColor4, alphaMap1.b);

		n = alphaMap1.b > 0.5f ? 1 : 0;
	}

	float4 color2 = 0;

	[flatten]
	if (usage == 2.0f || usage == 3.0f)
	{
		// BY SLOPE. Determine which texture to use based on height.
		float slope = 1.0f - input.normalWorld.y;
		if(slope < slope1)
		{
			color2 = lerp(
				gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)),
				gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)),
				slope / slope1);
		}
		if((slope < slope2) && (slope >= slope1))
		{
			color2 = lerp(
				gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)),
				gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2)),
				(slope - slope1) * (1.0f / (slope2 - slope1)));
		}
		if(slope >= slope2) 
		{
			color2 = gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
		}

		float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
		if(depthValue >= 0.05f)
		{
			color2 *= gPSDiffuseMapHRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)) * 1.8f;
		}
	}

	float4 color = 0;

	if (usage == 1.0f)
	{
		color = saturate(color1 * input.color * 2.0f);
	}
	if( usage == 2.0f)
	{
		color = saturate(color2 * input.color * 2.0f);
	}
	if (usage == 3.0f)
	{
		color = saturate(((color1 * prop) + (color2 * (1.0f-prop))) * input.color * 2.0f);
	}

	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	float4 lightPosition = mul(float4(input.positionWorld, 1), gPSLightViewProjection);

	float4 litColor = ComputeLights(
		gPSGlobalAmbient,
		gPSDirLights,
		gPSPointLights, 
		gPSSpotLights,
		gPSLightCount.x,
		gPSLightCount.y,
		gPSLightCount.z,
		gPSFogStart,
		gPSFogRange,
		gPSFogColor,
		material,
		input.positionWorld,
		n == 0 ? bumpNormalWorld1 : bumpNormalWorld2,
		color,
		n == 0 ? specularMapSample1 : specularMapSample2,
		gPSUseColorDiffuse,
		gPSUseColorSpecular,
		gPSEyePositionWorld,
		lightPosition,
		gPSShadows,
		gPSShadowMapStatic,
		gPSShadowMapDynamic);

	return litColor;
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 TerrainForward
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrain()));
	}
}
