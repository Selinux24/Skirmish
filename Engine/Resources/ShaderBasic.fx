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
	float3 gEyePositionWorld;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPointLights[MAX_LIGHTS_POINT];
	SpotLight gSpotLights[MAX_LIGHTS_SPOT];
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
	float4x4 gShadowTransform;
};
cbuffer cbPerObject : register (b1)
{
	Material gMaterial;
};
cbuffer cbSkinned : register (b2)
{
	float4x4 gBoneTransforms[MAXBONETRANSFORMS];
};
cbuffer cbPerInstance : register (b3)
{
	float gTextureIndex;
};

Texture2DArray gTextureArray;
Texture2D gNormalMap;
Texture2D gShadowMap;

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor VSPositionColor(VSVertexPositionColor input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.color = input.color;
    
    return output;
}
PSVertexPositionColor VSPositionColorI(VSVertexPositionColorI input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.color = input.color;
    
    return output;
}
PSVertexPositionColor VSPositionColorSkinned(VSVertexPositionColorSkinned input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.color = input.color;
    
    return output;
}
PSVertexPositionColor VSPositionColorSkinnedI(VSVertexPositionColorSkinnedI input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.color = input.color;
    
    return output;
}

float4 PSPositionColor(PSVertexPositionColor input) : SV_TARGET
{
	float4 litColor = input.color * gMaterial.Diffuse;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return litColor;
}

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor VSPositionNormalColor(VSVertexPositionNormalColor input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.shadowHomogeneous = mul(float4(input.positionLocal, 1), gShadowTransform);
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.color = input.color;
    
    return output;
}
PSVertexPositionNormalColor VSPositionNormalColorI(VSVertexPositionNormalColorI input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.shadowHomogeneous = mul(instancePosition, gShadowTransform);
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.color = input.color;

    return output;
}
PSVertexPositionNormalColor VSPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionNormalWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.shadowHomogeneous = mul(positionL, gShadowTransform);
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorldInverse));
	output.color = input.color;

    return output;
}
PSVertexPositionNormalColor VSPositionNormalColorSkinnedI(VSVertexPositionNormalColorSkinnedI input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionNormalWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.shadowHomogeneous = mul(instancePosition, gShadowTransform);
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorldInverse));
	output.color = input.color;

    return output;
}

float4 PSPositionNormalColor(PSVertexPositionNormalColor input) : SV_TARGET
{
	float3 toEye = normalize(gEyePositionWorld - input.positionWorld);

	float4 litColor = ComputeAllLights(
		gDirLights, 
		gPointLights, 
		gSpotLights,
		toEye,
		input.positionWorld,
		input.normalWorld,
		gMaterial.SpecularIntensity,
		gMaterial.SpecularPower,
		input.shadowHomogeneous,
		gShadowMap);

	float4 matColor = input.color * gMaterial.Diffuse;

	litColor *= matColor;
	litColor.a = matColor.a;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return litColor;
}

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture VSPositionTexture(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}
PSVertexPositionTexture VSPositionTextureI(VSVertexPositionTextureI input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;
    
    return output;
}
PSVertexPositionTexture VSPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}
PSVertexPositionTexture VSPositionTextureSkinnedI(VSVertexPositionTextureSkinnedI input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;
    
    return output;
}

float4 PSPositionTexture(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex));

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a *= gMaterial.Diffuse.a;

	return litColor;
}
float4 PSPositionTextureRED(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex)).r;
	
	//Grayscale
	return float4(litColor.rrr, 1);
}
float4 PSPositionTextureGREEN(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex)).g;
	
	//Grayscale
	return float4(litColor.ggg, 1);
}
float4 PSPositionTextureBLUE(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex)).b;
	
	//Grayscale
	return float4(litColor.bbb, 1);
}
float4 PSPositionTextureALPHA(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex)).a;
	
	//Grayscale
	return float4(litColor.aaa, 1);
}
float4 PSPositionTextureNOALPHA(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex));

	return float4(litColor.rgb, 1);
}

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTexture VSPositionNormalTexture(VSVertexPositionNormalTexture input)
{
    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.shadowHomogeneous = mul(float4(input.positionLocal, 1), gShadowTransform);
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}
PSVertexPositionNormalTexture VSPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.shadowHomogeneous = mul(instancePosition, gShadowTransform);
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
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
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.shadowHomogeneous = mul(positionL, gShadowTransform);
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorldInverse));
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
	
	return output;
}
PSVertexPositionNormalTexture VSPositionNormalTextureSkinnedI(VSVertexPositionNormalTextureSkinnedI input)
{
	PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionNormalWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.shadowHomogeneous = mul(instancePosition, gShadowTransform);
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorldInverse));
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

float4 PSPositionNormalTexture(PSVertexPositionNormalTexture input) : SV_TARGET
{
    float4 textureColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex));

	float3 toEye = normalize(gEyePositionWorld - input.positionWorld);

	float4 litColor = ComputeAllLights(
		gDirLights, 
		gPointLights, 
		gSpotLights,
		toEye,
		input.positionWorld,
		input.normalWorld,
		gMaterial.SpecularIntensity,
		gMaterial.SpecularPower,
		input.shadowHomogeneous,
		gShadowMap);

	litColor *= textureColor;
	litColor.a = textureColor.a;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return litColor;
}

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.shadowHomogeneous = mul(float4(input.positionLocal, 1), gShadowTransform);
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tangentWorld = mul(float4(input.tangentLocal, 0), gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.shadowHomogeneous = mul(instancePosition, gShadowTransform);
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tangentWorld = mul(float4(input.tangentLocal, 0), gWorld).xyz;
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
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		positionL,
		normalL,
		tangentL);

	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.shadowHomogeneous = mul(positionL, gShadowTransform);
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorldInverse));
	output.tangentWorld = mul(tangentL, gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangentSkinnedI(VSVertexPositionNormalTextureTangentSkinnedI input)
{
	PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 tangentL = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePositionNormalTangentWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		positionL,
		normalL,
		tangentL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.shadowHomogeneous = mul(instancePosition, gShadowTransform);
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorldInverse));
	output.tangentWorld = mul(tangentL, gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

float4 PSPositionNormalTextureTangent(PSVertexPositionNormalTextureTangent input) : SV_TARGET
{
	float3 normalMapSample = gNormalMap.Sample(SamplerLinear, input.tex).rgb;
	float3 bumpedNormalW = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

	float4 textureColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex));

	float3 toEye = normalize(gEyePositionWorld - input.positionWorld);

	float4 litColor = ComputeAllLights(
		gDirLights, 
		gPointLights, 
		gSpotLights,
		toEye,
		input.positionWorld,
		bumpedNormalW,
		gMaterial.SpecularIntensity,
		gMaterial.SpecularPower,
		input.shadowHomogeneous,
		gShadowMap);

	litColor *= textureColor;
	litColor.a = textureColor.a;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return litColor;
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
