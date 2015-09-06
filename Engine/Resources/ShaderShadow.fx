#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorldViewProjection;
};

cbuffer cbSkinned : register (b1)
{
	float4x4 gBoneTransforms[MAXBONETRANSFORMS];
};

ShadowMapOutput VSSMPositionColor(VSVertexPositionColor input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionColorI(VSVertexPositionColorI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}
ShadowMapOutput VSSMPositionColorSkinned(VSVertexPositionColorSkinned input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionColorSkinnedI(VSVertexPositionColorSkinnedI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}

ShadowMapOutput VSSMPositionNormalColor(VSVertexPositionNormalColor input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionNormalColorI(VSVertexPositionNormalColorI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);

    return output;
}
ShadowMapOutput VSSMPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionNormalColorSkinnedI(VSVertexPositionNormalColorSkinnedI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);

    return output;
}

ShadowMapOutput VSSMPositionTexture(VSVertexPositionTexture input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionTextureI(VSVertexPositionTextureI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}
ShadowMapOutput VSSMPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionTextureSkinnedI(VSVertexPositionTextureSkinnedI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}

ShadowMapOutput VSSMPositionNormalTexture(VSVertexPositionNormalTexture input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);

    return output;
}
ShadowMapOutput VSSMPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;
	
	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);
	
	return output;
}
ShadowMapOutput VSSMPositionNormalTextureSkinnedI(VSVertexPositionNormalTextureSkinnedI input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);

	return output;
}

ShadowMapOutput VSSMPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
    ShadowMapOutput output = (ShadowMapOutput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}
ShadowMapOutput VSSMPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
	output.positionHomogeneous = mul(positionL, gWorldViewProjection);

	return output;
}
ShadowMapOutput VSSMPositionNormalTextureTangentSkinnedI(VSVertexPositionNormalTextureTangentSkinnedI input)
{
	ShadowMapOutput output = (ShadowMapOutput)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);

	return output;
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
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionTextureI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinned()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionTextureSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinnedI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}

technique11 ShadowMapPositionNormalTexture
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTexture()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalTextureI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinned()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalTextureSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinnedI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}

technique11 ShadowMapPositionNormalTextureTangent
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangent()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalTextureTangentI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalTextureTangentSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinned()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}
technique11 ShadowMapPositionNormalTextureTangentSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinnedI()));
		SetGeometryShader(NULL);
        SetPixelShader(NULL);
    }
}