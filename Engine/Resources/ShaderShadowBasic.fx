#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbGlobals : register (b0)
{
    uint gAnimationPaletteWidth;
};
Texture2D gAnimationPalette;

cbuffer cbVSPerFrame : register (b1)
{
	float4x4 gVSWorldViewProjection;
};

cbuffer cbVSPerInstance : register (b2)
{
	uint gVSAnimationOffset;
	uint PAD21;
	uint PAD22;
	uint PAD23;
};

Texture2DArray gPSDiffuseMapArray;

cbuffer cbPSPerInstance : register (b5)
{
	uint gPSTextureIndex;
	bool PAD51;
	bool PAD52;
	bool PAD53;
};

PSShadowMapPosition VSSMPositionColor(VSVertexPositionColor input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gVSWorldViewProjection);

	return output;
}
PSShadowMapPosition VSSMPositionColorI(VSVertexPositionColorI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
    
    return output;
}
PSShadowMapPosition VSSMPositionColorSkinned(VSVertexPositionColorSkinned input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

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

	return output;
}
PSShadowMapPosition VSSMPositionColorSkinnedI(VSVertexPositionColorSkinnedI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

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
    
    return output;
}

PSShadowMapPosition VSSMPositionNormalColor(VSVertexPositionNormalColor input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gVSWorldViewProjection);

	return output;
}
PSShadowMapPosition VSSMPositionNormalColorI(VSVertexPositionNormalColorI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);

    return output;
}
PSShadowMapPosition VSSMPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

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

	return output;
}
PSShadowMapPosition VSSMPositionNormalColorSkinnedI(VSVertexPositionNormalColorSkinnedI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

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

    return output;
}

PSShadowMapPositionTexture VSSMPositionTexture(VSVertexPositionTexture input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gVSWorldViewProjection);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionTextureI(VSVertexPositionTextureI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

    return output;
}
PSShadowMapPositionTexture VSSMPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionTextureSkinnedI(VSVertexPositionTextureSkinnedI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

    return output;
}

PSShadowMapPositionTexture VSSMPositionNormalTexture(VSVertexPositionNormalTexture input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gVSWorldViewProjection);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

    return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;
	
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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureSkinnedI(VSVertexPositionNormalTextureSkinnedI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

PSShadowMapPositionTexture VSSMPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gVSWorldViewProjection);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gVSWorldViewProjection);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

    return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureTangentSkinnedI(VSVertexPositionNormalTextureTangentSkinnedI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture)0;

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
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

float4 PSDefault(PSShadowMapPositionTexture input) : SV_Target
{
	float4 textureColor = gPSDiffuseMapArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

	if (textureColor.a > 0.8f)
	{
		float depthValue = input.depth.z / input.depth.w;

		return float4(depthValue, depthValue, depthValue, 1.0f);
	}
	else
	{
		discard;

		return 0.0f;
	}
}

technique11 ShadowMapPositionColor
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColor()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionColorI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionColorSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorSkinned()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionColorSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorSkinnedI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}

technique11 ShadowMapPositionNormalColor
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColor()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalColorI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalColorSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorSkinned()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalColorSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorSkinnedI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}

technique11 ShadowMapPositionTexture
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionTextureI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionTextureSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}

technique11 ShadowMapPositionNormalTexture
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}

technique11 ShadowMapPositionNormalTextureTangent
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangent()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureTangentI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureTangentSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureTangentSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}