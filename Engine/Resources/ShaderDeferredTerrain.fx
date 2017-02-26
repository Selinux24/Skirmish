#include "IncLights.fx"
#include "IncVertexFormats.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register (b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float4 gParams;
	float gTextureResolution;
	float3 PAD_B1;
};
cbuffer cbPerObject : register (b4)
{
	uint gMaterialIndex;
	uint3 PAD_B4;
};

Texture2DArray gDiffuseMapLRArray;
Texture2DArray gDiffuseMapHRArray;
Texture2DArray gNormalMapArray;
Texture2DArray gSpecularMapArray;

Texture2DArray gColorTextureArray;
Texture2D gAlphaTexture;

PSVertexTerrain VSTerrain(VSVertexTerrain input)
{
    PSVertexTerrain output = (PSVertexTerrain)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorld));
	output.tangentWorld = normalize(mul(input.tangentLocal, (float3x3)gWorld));
	output.tex0 = input.tex * gTextureResolution;
	output.tex1 = input.tex;
	output.color = input.color;
    
    return output;
}

GBufferPSOutput PSTerrain(PSVertexTerrain input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	float usage = gParams.x;
	float prop = gParams.y;
	float slope1 = gParams.z;
	float slope2 = gParams.w;

	// BY ALPHA MAP
	float3 normalMapSample1 = gNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 0)).rgb;
	float3 bumpNormalWorld1 = NormalSampleToWorldSpace(normalMapSample1, input.normalWorld, input.tangentWorld);
	float3 normalMapSample2 = gNormalMapArray.Sample(SamplerLinear, float3(input.tex0, 1)).rgb;
	float3 bumpNormalWorld2 = NormalSampleToWorldSpace(normalMapSample2, input.normalWorld, input.tangentWorld);

	float4 specularMapSample1 = gSpecularMapArray.Sample(SamplerLinear, float3(input.tex0, 0));
	float4 specularMapSample2 = gSpecularMapArray.Sample(SamplerLinear, float3(input.tex0, 1));

	float n = 0;

	float4 color1 = 0;

	[flatten]
	if (usage == 1.0f || usage == 3.0f)
	{
		float4 textureColor1 = gColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 0));
		float4 textureColor2 = gColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 1));
		float4 textureColor3 = gColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
		float4 textureColor4 = gColorTextureArray.Sample(SamplerAnisotropic, float3(input.tex0, 3));
		float4 alphaMap1 = gAlphaTexture.Sample(SamplerLinear, input.tex1);

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
				gDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)), 
				gDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)), 
				slope / slope1);
		}
		if((slope < slope2) && (slope >= slope1))
		{
			color2 = lerp(
				gDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 1)), 
				gDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2)), 
				(slope - slope1) * (1.0f / (slope2 - slope1)));
		}
		if(slope >= slope2) 
		{
			color2 = gDiffuseMapLRArray.Sample(SamplerAnisotropic, float3(input.tex0, 2));
		}

		float depthValue = input.positionHomogeneous.z / input.positionHomogeneous.w;
		if(depthValue >= 0.05f)
		{
			color2 *= gDiffuseMapHRArray.Sample(SamplerAnisotropic, float3(input.tex0, 0)) * 1.8f;
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

	float3 normal = n == 0 ? bumpNormalWorld1 : bumpNormalWorld2;
	float4 specular = n == 0 ? specularMapSample1 : specularMapSample2;

	output.color = color;
	output.normal = float4(normal, 0);
	output.depth = float4(input.positionWorld, gMaterialIndex);

    return output;
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 TerrainDeferred
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrain()));
	}
}
