#include "IncLights.fx"
#include "IncVertexFormats.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};
cbuffer cbPerGroup : register (b1)
{
    uint gPaletteWidth;
};
cbuffer cbPerInstance : register (b2)
{
	uint3 gAnimationData;
	uint gMaterialIndex;
	float gTextureIndex;
};

Texture2DArray gDiffuseMapArray;
Texture2DArray gNormalMapArray;
Texture2DArray gSpecularMapArray;
Texture2D gAnimationPalette;

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
		gAnimationPalette,
		gAnimationData,
		gPaletteWidth,
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
		gAnimationPalette,
		input.animationData,
		gPaletteWidth,
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

GBufferPSOutput PSPositionColor(PSVertexPositionColor input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	output.color = input.color;
	output.normal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	output.depth = float4(input.positionWorld, gMaterialIndex);

    return output;
}

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSVertexPositionNormalColor VSPositionNormalColor(VSVertexPositionNormalColor input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorld));
	output.color = input.color;
    
    return output;
}
PSVertexPositionNormalColor VSPositionNormalColorI(VSVertexPositionNormalColorI input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
    float3 instanceNormal = mul(input.normalLocal, (float3x3)input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gWorld));
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
		gAnimationData,
		gPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorld));
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
		input.animationData,
		gPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

    float4 instancePosition = mul(positionL, input.localTransform);
    float3 instanceNormal = mul(normalL.xyz, (float3x3)input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gWorld));
	output.color = input.color;

    return output;
}

GBufferPSOutput PSPositionNormalColor(PSVertexPositionNormalColor input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	output.color = input.color;
	output.normal = float4(input.normalWorld, 0);
	output.depth = float4(input.positionWorld, gMaterialIndex);

    return output;
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
		gAnimationPalette,
		gAnimationData,
		gPaletteWidth,
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
		gAnimationPalette,
		input.animationData,
		gPaletteWidth,
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

GBufferPSOutput PSPositionTexture(PSVertexPositionTexture input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	output.color = gDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	output.normal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	output.depth = float4(input.positionWorld, gMaterialIndex);

    return output;
}

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTexture VSPositionNormalTexture(VSVertexPositionNormalTexture input)
{
    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorld));
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}
PSVertexPositionNormalTexture VSPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
    float3 instanceNormal = mul(input.normalLocal, (float3x3)input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gWorld));
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
		gAnimationData,
		gPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	output.positionWorld = mul(positionL, gWorld).xyz;
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorld));
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
		gAnimationPalette,
		input.animationData,
		gPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

    float4 instancePosition = mul(positionL, input.localTransform);
    float3 instanceNormal = mul(normalL.xyz, (float3x3)input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gWorld));
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

GBufferPSOutput PSPositionNormalTexture(PSVertexPositionNormalTexture input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	output.color = gDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	output.normal = float4(input.normalWorld, 0);
	output.depth = float4(input.positionWorld, gMaterialIndex);

    return output;
}

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorld));
	output.tangentWorld = normalize(mul(input.tangentLocal, (float3x3)gWorld));
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
    return output;
}
PSVertexPositionNormalTextureTangent VSPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
    float3 instanceNormal = mul(input.normalLocal, (float3x3)input.localTransform);
    float3 instanceTangent = mul(input.tangentLocal, (float3x3)input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal, (float3x3)gWorld));
	output.tangentWorld = normalize(mul(instanceTangent, (float3x3)gWorld));
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
		gAnimationData,
		gPaletteWidth,
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
	output.normalWorld = normalize(mul(normalL.xyz, (float3x3)gWorld));
	output.tangentWorld = normalize(mul(tangentL.xyz, (float3x3)gWorld));
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
		gAnimationPalette,
		input.animationData,
		gPaletteWidth,
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
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.normalWorld = normalize(mul(instanceNormal.xyz, (float3x3)gWorld));
	output.tangentWorld = normalize(mul(instanceTangent.xyz, (float3x3)gWorld));
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

GBufferPSOutput PSPositionNormalTextureTangent(PSVertexPositionNormalTextureTangent input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	float4 color = gDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	float3 normalMapSample = gNormalMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex)).rgb;
	float3 bumpedNormalW = NormalSampleToWorldSpace(normalMapSample, input.normalWorld, input.tangentWorld);

	output.color = color;
	output.normal = float4(bumpedNormalW, 0);
	output.depth = float4(input.positionWorld, gMaterialIndex);

    return output;
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
