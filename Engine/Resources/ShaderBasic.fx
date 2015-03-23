#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldInverse;
	float4x4 gWorldViewProjection;
	float4x4 gShadowTransform; 
	DirectionalLight gDirLights[3];
	PointLight gPointLight;
	SpotLight gSpotLight;
	float3 gEyePositionWorld;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
	float gEnableShadows;
};

cbuffer cbPerObject : register (b1)
{
	float gTextureIndex;
	Material gMaterial;
};

cbuffer cbSkinned : register (b2)
{
	float4x4 gBoneTransforms[96];
};

Texture2DArray gTextureArray;
Texture2D gNormalMap;
Texture2D gShadowMap;

PSVertexPositionColor VSPositionColor(VSVertexPositionColor input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.color = input.color;
    
    return output;
}

PSVertexPositionColor VSPositionColorSkinned(VSVertexPositionColorSkinned input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);

	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.positionWorld = mul(float4(posL, 1.0f), gWorld).xyz;
	output.color = input.color;
    
    return output;
}

float4 PSPositionColor(PSVertexPositionColor input) : SV_TARGET
{
	float4 litColor = input.color;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return litColor;
}

PSVertexPositionNormalColor VSPositionNormalColor(VSVertexPositionNormalColor input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.color = input.color;
	output.shadowHomogeneous = mul(float4(input.positionLocal, 1.0f), gShadowTransform);
    
	output.normalWorld = normalize(output.normalWorld);

    return output;
}

PSVertexPositionNormalColor VSPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	float3 normalL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionNormalWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		posL,
		normalL);

	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.positionWorld = mul(float4(posL, 1.0f), gWorld).xyz;
	output.normalWorld = normalize(mul(normalL, (float3x3)gWorldInverse));
	output.color = input.color;
	output.shadowHomogeneous = mul(float4(posL, 1.0f), gShadowTransform);
    
	output.normalWorld = normalize(output.normalWorld);

    return output;
}

float4 PSPositionNormalColor(PSVertexPositionNormalColor input) : SV_TARGET
{
	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
	float distToEye = length(toEyeWorld);
	toEyeWorld /= distToEye;

	LightInput lInput = (LightInput)0;
	lInput.toEyeWorld = toEyeWorld;
	lInput.positionWorld = input.positionWorld;
	lInput.normalWorld = input.normalWorld;
	lInput.material = gMaterial;
	lInput.dirLights = gDirLights;
	lInput.pointLight = gPointLight;
	lInput.spotLight = gSpotLight;
	lInput.enableShadows = gEnableShadows;
	lInput.shadowPosition = input.shadowHomogeneous;

	LightOutput lOutput = ComputeLights(lInput, gShadowMap);

	float4 litColor = input.color * (lOutput.ambient + lOutput.diffuse) + lOutput.specular;

	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a = gMaterial.Diffuse.a * input.color.a;

	return litColor;
}

PSVertexPositionTexture VSPositionTexture(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}

PSVertexPositionTexture VSPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);

	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.positionWorld = mul(float4(posL, 1.0f), gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
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

PSVertexPositionNormalTexture VSPositionNormalTexture(VSVertexPositionNormalTexture input)
{
    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
	output.shadowHomogeneous = mul(float4(input.positionLocal, 1.0f), gShadowTransform);
    
    return output;
}

PSVertexPositionNormalTexture VSPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	float3 normalL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionNormalWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		posL,
		normalL);
	
	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.positionWorld = mul(float4(posL, 1.0f), gWorld).xyz;
	output.normalWorld = normalize(mul(normalL, (float3x3)gWorldInverse));
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
	output.shadowHomogeneous = mul(float4(posL, 1.0f), gShadowTransform);
	
	return output;
}

float4 PSPositionNormalTexture(PSVertexPositionNormalTexture input) : SV_TARGET
{
	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
	float distToEye = length(toEyeWorld);
	toEyeWorld /= distToEye;

	LightInput lInput = (LightInput)0;
	lInput.toEyeWorld = toEyeWorld;
	lInput.positionWorld = input.positionWorld;
	lInput.normalWorld = input.normalWorld;
	lInput.material = gMaterial;
	lInput.dirLights = gDirLights;
	lInput.pointLight = gPointLight;
	lInput.spotLight = gSpotLight;
	lInput.enableShadows = gEnableShadows;
	lInput.shadowPosition = input.shadowHomogeneous;

	LightOutput lOutput = ComputeLights(lInput, gShadowMap);

    float4 textureColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex));

	float4 litColor = textureColor * (lOutput.ambient + lOutput.diffuse) + lOutput.specular;

	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a = gMaterial.Diffuse.a * textureColor.a;

	return litColor;
}

PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1.0f), gWorld).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tangentWorld = mul(input.tangentLocal, (float3x3)gWorld);
	output.tex = input.tex;
	output.shadowHomogeneous = mul(float4(input.positionLocal, 1.0f), gShadowTransform);
	output.textureIndex = gTextureIndex;
    
    return output;
}

PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	float3 normalL = float3(0.0f, 0.0f, 0.0f);
	float3 tangentL = float3(0.0f, 0.0f, 0.0f);

	ComputePositionNormalTangentWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		input.tangentLocal,
		posL,
		normalL,
		tangentL);

	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.positionWorld = mul(float4(posL, 1.0f), gWorld).xyz;
	output.normalWorld = normalize(mul(normalL, (float3x3)gWorldInverse));
	output.tangentWorld = mul(tangentL, (float3x3)gWorld);
	output.tex = input.tex;
	output.shadowHomogeneous = mul(float4(posL, 1.0f), gShadowTransform);
	output.textureIndex = gTextureIndex;
    
    return output;
}

float4 PSPositionNormalTextureTangent(PSVertexPositionNormalTextureTangent input) : SV_TARGET
{
	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
	float distToEye = length(toEyeWorld);
	toEyeWorld /= distToEye;

	float3 normalMapSample = gNormalMap.Sample(SamplerLinear, input.tex).rgb;
	float3 bumpedNormalW = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

	LightInput lInput = (LightInput)0;
	lInput.toEyeWorld = toEyeWorld;
	lInput.positionWorld = input.positionWorld;
	lInput.normalWorld = bumpedNormalW;
	lInput.material = gMaterial;
	lInput.dirLights = gDirLights;
	lInput.pointLight = gPointLight;
	lInput.spotLight = gSpotLight;
	lInput.enableShadows = gEnableShadows;
	lInput.shadowPosition = input.shadowHomogeneous;

	LightOutput lOutput = ComputeLights(lInput, gShadowMap);

    float4 textureColor = gTextureArray.Sample(SamplerAnisotropic, float3(input.tex, input.textureIndex));

	float4 litColor = textureColor * (lOutput.ambient + lOutput.diffuse) + lOutput.specular;

	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a = gMaterial.Diffuse.a * textureColor.a;

	return litColor;
}

technique11 PositionColor
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}

technique11 PositionColorSkinned
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionColorSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}

technique11 PositionNormalColor
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalColor()));
	}
}

technique11 PositionNormalColorSkinned
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalColorSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalColor()));
	}
}

technique11 PositionTexture
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}

technique11 PositionTextureRED
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureRED()));
	}
}

technique11 PositionTextureGREEN
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureGREEN()));
	}
}

technique11 PositionTextureBLUE
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureBLUE()));
	}
}

technique11 PositionTextureSkinned
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}

technique11 PositionNormalTexture
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTexture()));
	}
}

technique11 PositionNormalTextureSkinned
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTexture()));
	}
}

technique11 PositionNormalTextureTangent
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureTangent()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTextureTangent()));
	}
}

technique11 PositionNormalTextureTangentSkinned
{
	pass P0
	{
		SetRasterizerState(RasterizerSolid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureTangentSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTextureTangent()));
	}
}
