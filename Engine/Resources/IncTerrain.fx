#include "IncLights.fx"
#include "IncVertexFormats.fx"

Texture2D gPSAlphaTexture;
Texture2DArray gPSNormalMapArray;
Texture2DArray gPSSpecularMapArray;
Texture2DArray gPSColorTextureArray;
Texture2DArray gPSDiffuseMapLRArray;
Texture2DArray gPSDiffuseMapHRArray;

inline float4 AlphaMap(PSVertexTerrain input, out float4 specular, out float3 normal)
{
    float4 alphaMap = gPSAlphaTexture.Sample(SamplerAnisotropic, input.tex1);
	uint index = alphaMap.b > 0.5f ? 1 : 0;

    float3 normalMapSample = gPSNormalMapArray.Sample(SamplerAnisotropic, float3(input.tex0, index)).rgb;
	normal = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

    specular = gPSSpecularMapArray.Sample(SamplerAnisotropic, float3(input.tex0, index));

	float4 textureColor1 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 0));
	float4 textureColor2 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 1));
	float4 textureColor3 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
	float4 textureColor4 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 3));

	float4 color = lerp(textureColor1, textureColor2, alphaMap.r);
	color = lerp(color, textureColor3, alphaMap.g);
	color = lerp(color, textureColor4, alphaMap.b);

	return saturate(color * input.color);
}

inline float4 Slopes(PSVertexTerrain input, float slope1, float slope2, out float4 specular, out float3 normal)
{
    float4 alphaMap = gPSAlphaTexture.Sample(SamplerAnisotropic, input.tex1);
	uint index = alphaMap.b > 0.5f ? 1 : 0;

    float3 normalMapSample = gPSNormalMapArray.Sample(SamplerAnisotropic, float3(input.tex0, index)).rgb;
	normal = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

    specular = gPSSpecularMapArray.Sample(SamplerAnisotropic, float3(input.tex0, index));

	float4 color = 0;

	// BY SLOPE. Determine which texture to use based on height.
	float slope = 1.0f - input.normalWorld.y;
	if (slope < slope1)
	{
		color = lerp(
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)),
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)),
			slope / slope1);
	}
	if ((slope < slope2) && (slope >= slope1))
	{
		color = lerp(
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)),
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2)),
			(slope - slope1) * (1.0f / (slope2 - slope1)));
	}
	if (slope >= slope2)
	{
		color = gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
	}

	float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
	if (depthValue >= 0.05f)
	{
		color *= gPSDiffuseMapHRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)) * 1.8f;
	}

	return saturate(color * input.color);
}

inline float4 Full(PSVertexTerrain input, float prop, float slope1, float slope2, out float4 specular, out float3 normal)
{
    float4 alphaMap = gPSAlphaTexture.Sample(SamplerAnisotropic, input.tex1);
	uint index = alphaMap.b > 0.5f ? 1 : 0;

    float3 normalMapSample = gPSNormalMapArray.Sample(SamplerAnisotropic, float3(input.tex0, index)).rgb;
	normal = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

    specular = gPSSpecularMapArray.Sample(SamplerAnisotropic, float3(input.tex0, index));

	// BY ALPHA MAP
	float4 textureColor1 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 0));
	float4 textureColor2 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 1));
	float4 textureColor3 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
	float4 textureColor4 = gPSColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 3));

    float4 alphaMap1 = gPSAlphaTexture.Sample(SamplerAnisotropic, input.tex1);

	float4 color1 = lerp(textureColor1, textureColor2, alphaMap1.r);
	color1 = lerp(color1, textureColor3, alphaMap1.g);
	color1 = lerp(color1, textureColor4, alphaMap1.b);

	// BY SLOPE. Determine which texture to use based on height.
	float4 color2 = 0;
	float slope = 1.0f - input.normalWorld.y;
	if (slope < slope1)
	{
		color2 = lerp(
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)),
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)),
			slope / slope1);
	}
	if ((slope < slope2) && (slope >= slope1))
	{
		color2 = lerp(
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)),
			gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2)),
			(slope - slope1) * (1.0f / (slope2 - slope1)));
	}
	if (slope >= slope2)
	{
		color2 = gPSDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
	}

	float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
	if (depthValue >= 0.05f)
	{
		color2 *= gPSDiffuseMapHRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)) * 1.8f;
	}

	return saturate(((color1 * prop) + (color2 * (1.0f - prop))) * input.color);
}
