#include "IncLights.fx"
#include "IncVertexFormats.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register (b0)
{
	uint gMaterialPaletteWidth;
	uint gAnimationPaletteWidth;
};
Texture2D gMaterialPalette;
Texture2D gAnimationPalette;

cbuffer cbVSPerFrame : register (b1)
{
	float4x4 gVSWorld;
	float4x4 gVSWorldViewProjection;
};

cbuffer cbVSPerInstance : register (b2)
{
	uint gVSAnimationOffset;
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
	float PAD1;
	float PAD2;
	DirectionalLight gPSDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPSPointLights[MAX_LIGHTS_POINT];
	SpotLight gPSSpotLights[MAX_LIGHTS_SPOT];
};
Texture2D gPSShadowMapStatic;
Texture2D gPSShadowMapDynamic;

cbuffer cbPSPerObject : register (b4)
{
	bool gPSUseColorDiffuse;
	bool gPSUseColorSpecular;
};
Texture2DArray gPSDiffuseMapArray;
Texture2DArray gPSNormalMapArray;
Texture2DArray gPSSpecularMapArray;

cbuffer cbPSPerInstance : register (b5)
{
	uint gPSMaterialIndex;
	uint gPSTextureIndex;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor VSPositionColor(VSVertexPositionColor input)
{
	PSVertexPositionColor output = (PSVertexPositionColor)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.color = input.color;

	return output;
}
PSVertexPositionColor VSPositionColorI(VSVertexPositionColorI input)
{
	PSVertexPositionColor output = (PSVertexPositionColor)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.color = input.color;

	return output;
}
PSVertexPositionColor VSPositionColorSkinned(VSVertexPositionColorSkinned input)
{
	PSVertexPositionColor output = (PSVertexPositionColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	output.positionHomogeneous = mul(positionL, gVSWorldViewProjection);
	output.positionWorld = mul(positionL, gVSWorld).xyz;
	output.color = input.color;

	return output;
}
PSVertexPositionColor VSPositionColorSkinnedI(VSVertexPositionColorSkinnedI input)
{
	PSVertexPositionColor output = (PSVertexPositionColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionWeights(
		gAnimationPalette,
		input.animationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	float4 instancePosition = mul(positionL, input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.color = input.color;

	return output;
}

float4 PSPositionColor(PSVertexPositionColor input) : SV_TARGET
{
	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	float4 matColor = input.color * material.Diffuse;

	if (gPSFogRange > 0)
	{
		float distToEye = length(gPSEyePositionWorld - input.positionWorld);

		matColor = ComputeFog(matColor, distToEye, gPSFogStart, gPSFogRange, gPSFogColor);
	}

	return matColor;
}

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor VSPositionNormalColor(VSVertexPositionNormalColor input)
{
	PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gVSWorld));
	output.color = input.color;

	return output;
}
PSVertexPositionNormalColor VSPositionNormalColorI(VSVertexPositionNormalColorI input)
{
	PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
	float3 instanceNormal = mul(input.normalLocal, (float3x3)input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gVSWorld));
	output.color = input.color;

	return output;
}
PSVertexPositionNormalColor VSPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
	PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionNormalWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

	output.positionHomogeneous = mul(positionL, gVSWorldViewProjection);
	output.positionWorld = mul(positionL, gVSWorld).xyz;
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gVSWorld));
	output.color = input.color;

	return output;
}
PSVertexPositionNormalColor VSPositionNormalColorSkinnedI(VSVertexPositionNormalColorSkinnedI input)
{
	PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionNormalWeights(
		gAnimationPalette,
		input.animationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

	float4 instancePosition = mul(positionL, input.localTransform);
	float3 instanceNormal = mul(normalL.xyz, (float3x3)input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gVSWorld));
	output.color = input.color;

	return output;
}

float4 PSPositionNormalColor(PSVertexPositionNormalColor input) : SV_TARGET
{
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
	lInput.pNormal = input.normalWorld;
	lInput.pColorDiffuse = input.color;
	lInput.pColorSpecular = 1;
	lInput.ePosition = gPSEyePositionWorld;
	lInput.sLightPosition = lightPosition;
	lInput.shadows = gPSShadows;
	lInput.shadowMapStatic = gPSShadowMapStatic;
	lInput.shadowMapDynamic = gPSShadowMapDynamic;

	return ComputeLights(lInput);
}

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture VSPositionTexture(VSVertexPositionTexture input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSVertexPositionTexture VSPositionTextureI(VSVertexPositionTextureI input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}
PSVertexPositionTexture VSPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	output.positionHomogeneous = mul(positionL, gVSWorldViewProjection);
	output.positionWorld = mul(positionL, gVSWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSVertexPositionTexture VSPositionTextureSkinnedI(VSVertexPositionTextureSkinnedI input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionWeights(
		gAnimationPalette,
		input.animationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	float4 instancePosition = mul(positionL, input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

float4 PSPositionTexture(PSVertexPositionTexture input) : SV_TARGET
{
	float4 textureColor = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

	if (gPSFogRange > 0)
	{
		float distToEye = length(gPSEyePositionWorld - input.positionWorld);

		textureColor = ComputeFog(textureColor, distToEye, gPSFogStart, gPSFogRange, gPSFogColor);
	}

	return textureColor;
}
float4 PSPositionTextureRED(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

	//Grayscale of red channel
	return float4(color.rrr, 1);
}
float4 PSPositionTextureGREEN(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

	//Grayscale of green channel
	return float4(color.ggg, 1);
}
float4 PSPositionTextureBLUE(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

	//Grayscale of blue channel
	return float4(color.bbb, 1);
}
float4 PSPositionTextureALPHA(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

	//Grayscale of alpha channel
	return float4(color.aaa, 1);
}
float4 PSPositionTextureNOALPHA(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

	//Color channel
	return float4(color.rgb, 1);
}

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTexture VSPositionNormalTexture(VSVertexPositionNormalTexture input)
{
	PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSVertexPositionNormalTexture VSPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
	PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
	float3 instanceNormal = mul(input.normalLocal, (float3x3)input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}
PSVertexPositionNormalTexture VSPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionNormalWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

	output.positionHomogeneous = mul(positionL, gVSWorldViewProjection);
	output.positionWorld = mul(positionL, gVSWorld).xyz;
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSVertexPositionNormalTexture VSPositionNormalTextureSkinnedI(VSVertexPositionNormalTextureSkinnedI input)
{
	PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionNormalWeights(
		gAnimationPalette,
		input.animationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

	float4 instancePosition = mul(positionL, input.localTransform);
	float3 instanceNormal = mul(normalL.xyz, (float3x3)input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

float4 PSPositionNormalTexture(PSVertexPositionNormalTexture input) : SV_TARGET
{
	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	float4 diffuseMap = 1;
	if (gPSUseColorDiffuse == true)
	{
		diffuseMap = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	}

	float4 specularMap = 1;
	if (gPSUseColorSpecular == true)
	{
		specularMap = gPSSpecularMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	}

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
	lInput.pNormal = input.normalWorld;
	lInput.pColorDiffuse = diffuseMap;
	lInput.pColorSpecular = specularMap;
	lInput.ePosition = gPSEyePositionWorld;
	lInput.sLightPosition = lightPosition;
	lInput.shadows = gPSShadows;
	lInput.shadowMapStatic = gPSShadowMapStatic;
	lInput.shadowMapDynamic = gPSShadowMapDynamic;

	return ComputeLights(lInput);
}

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
	PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gVSWorld));
	output.tangentWorld = normalize(mul(input.tangentLocal, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
	PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
	float3 instanceNormal = mul(input.normalLocal, (float3x3)input.localTransform);
	float3 instanceTangent = mul(input.tangentLocal, (float3x3)input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gVSWorld));
	output.tangentWorld = normalize(mul(instanceTangent, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
	PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 tangentL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionNormalTangentWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		positionL,
		normalL,
		tangentL);

	output.positionHomogeneous = mul(positionL, gVSWorldViewProjection);
	output.positionWorld = mul(positionL, gVSWorld).xyz;
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gVSWorld));
	output.tangentWorld = normalize(mul(tangentL.xyz, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangentSkinnedI(VSVertexPositionNormalTextureTangentSkinnedI input)
{
	PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 tangentL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionNormalTangentWeights(
		gAnimationPalette,
		input.animationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		positionL,
		normalL,
		tangentL);

	float4 instancePosition = mul(positionL, input.localTransform);
	float3 instanceNormal = mul(normalL.xyz, (float3x3)input.localTransform);
	float3 instanceTangent = mul(tangentL.xyz, (float3x3)input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.positionWorld = mul(instancePosition, gVSWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal.xyz, (float3x3)gVSWorld));
	output.tangentWorld = normalize(mul(instanceTangent.xyz, (float3x3)gVSWorld));
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

float4 PSPositionNormalTextureTangent(PSVertexPositionNormalTextureTangent input) : SV_TARGET
{
	Material material = GetMaterialData(gMaterialPalette, gPSMaterialIndex, gMaterialPaletteWidth);

	float4 diffuseMap = 1;
	if (gPSUseColorDiffuse == true)
	{
		diffuseMap = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	}

	float4 specularMap = 1;
	if (gPSUseColorSpecular == true)
	{
		specularMap = gPSSpecularMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	}

	float3 normalMap = gPSNormalMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex)).rgb;
	float3 normalWorld = NormalSampleToWorldSpace(normalMap, input.normalWorld, input.tangentWorld);

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
	lInput.pNormal = normalWorld;
	lInput.pColorDiffuse = diffuseMap;
	lInput.pColorSpecular = specularMap;
	lInput.ePosition = gPSEyePositionWorld;
	lInput.sLightPosition = lightPosition;
	lInput.shadows = gPSShadows;
	lInput.shadowMapStatic = gPSShadowMapStatic;
	lInput.shadowMapDynamic = gPSShadowMapDynamic;

	return ComputeLights(lInput);
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 PositionColor
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}
technique11 PositionColorI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionColorI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}
technique11 PositionColorSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionColorSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}
technique11 PositionColorSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionColorSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}

technique11 PositionNormalColor
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalColor()));
	}
}
technique11 PositionNormalColorI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalColorI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalColor()));
	}
}
technique11 PositionNormalColorSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalColorSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalColor()));
	}
}
technique11 PositionNormalColorSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalColorSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalColor()));
	}
}

technique11 PositionTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}
technique11 PositionTextureRED
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureRED()));
	}
}
technique11 PositionTextureGREEN
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureGREEN()));
	}
}
technique11 PositionTextureBLUE
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureBLUE()));
	}
}
technique11 PositionTextureALPHA
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureALPHA()));
	}
}
technique11 PositionTextureNOALPHA
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureNOALPHA()));
	}
}
technique11 PositionTextureI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}
technique11 PositionTextureREDI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureRED()));
	}
}
technique11 PositionTextureGREENI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureGREEN()));
	}
}
technique11 PositionTextureBLUEI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureBLUE()));
	}
}
technique11 PositionTextureALPHAI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureALPHA()));
	}
}
technique11 PositionTextureNOALPHAI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureNOALPHA()));
	}
}
technique11 PositionTextureSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}
technique11 PositionTextureSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}

technique11 PositionNormalTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTexture()));
	}
}
technique11 PositionNormalTextureI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTexture()));
	}
}
technique11 PositionNormalTextureSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTexture()));
	}
}
technique11 PositionNormalTextureSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTexture()));
	}
}

technique11 PositionNormalTextureTangent
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureTangent()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTextureTangent()));
	}
}
technique11 PositionNormalTextureTangentI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureTangentI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTextureTangent()));
	}
}
technique11 PositionNormalTextureTangentSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureTangentSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTextureTangent()));
	}
}
technique11 PositionNormalTextureTangentSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureTangentSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTextureTangent()));
	}
}
