#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorldViewProjection;
};

cbuffer cbSkinned : register (b1)
{
	float4x4 gBoneTransforms[96];
};

struct ShadowMapColorOut
{
	float4 positionHomogeneous : SV_POSITION;
	float4 color : COLOR0;
};

struct ShadowMapTextureOut
{
	float4 positionHomogeneous : SV_POSITION;
	float2 tex : TEXCOORD;
};

RasterizerState Depth
{
	DepthBias = 10000;
    DepthBiasClamp = 0.0f;
	SlopeScaledDepthBias = 1.0f;
};

ShadowMapColorOut VSSMPositionColor(VSVertexPositionColor input)
{
	ShadowMapColorOut output = (ShadowMapColorOut)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapColorOut VSSMPositionColorI(VSVertexPositionColorI input)
{
    ShadowMapColorOut output = (ShadowMapColorOut)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.color = input.color;
    
    return output;
}

ShadowMapColorOut VSSMPositionColorSkinned(VSVertexPositionColorSkinned input)
{
	ShadowMapColorOut output = (ShadowMapColorOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);
	
	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapColorOut VSSMPositionColorSkinnedI(VSVertexPositionColorSkinnedI input)
{
    ShadowMapColorOut output = (ShadowMapColorOut)0;

    float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);

    float4 instancePosition = mul(float4(posL, 1), input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.color = input.color;
    
    return output;
}

ShadowMapColorOut VSSMPositionNormalColor(VSVertexPositionNormalColor input)
{
	ShadowMapColorOut output = (ShadowMapColorOut)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapColorOut VSSMPositionNormalColorI(VSVertexPositionNormalColorI input)
{
    ShadowMapColorOut output = (ShadowMapColorOut)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.color = input.color;

    return output;
}

ShadowMapColorOut VSSMPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
	ShadowMapColorOut output = (ShadowMapColorOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);
	
	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapColorOut VSSMPositionNormalColorSkinnedI(VSVertexPositionNormalColorSkinnedI input)
{
    ShadowMapColorOut output = (ShadowMapColorOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);

    float4 instancePosition = mul(float4(posL, 1), input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.color = input.color;

    return output;
}

ShadowMapTextureOut VSSMPositionTexture(VSVertexPositionTexture input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionTextureI(VSVertexPositionTextureI input)
{
    ShadowMapTextureOut output = (ShadowMapTextureOut)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.tex = input.tex;
    
    return output;
}

ShadowMapTextureOut VSSMPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);
	
	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionTextureSkinnedI(VSVertexPositionTextureSkinnedI input)
{
    ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);

    float4 instancePosition = mul(float4(posL, 1), input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.tex = input.tex;
    
    return output;
}

ShadowMapTextureOut VSSMPositionNormalTexture(VSVertexPositionNormalTexture input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
    ShadowMapTextureOut output = (ShadowMapTextureOut)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.tex = input.tex;

    return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);
	
	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.tex = input.tex;
	
	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureSkinnedI(VSVertexPositionNormalTextureSkinnedI input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);

    float4 instancePosition = mul(float4(posL, 1), input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
    ShadowMapTextureOut output = (ShadowMapTextureOut)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.tex = input.tex;
    
    return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);
	
	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureTangentSkinnedI(VSVertexPositionNormalTextureTangentSkinnedI input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gBoneTransforms,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		posL);

    float4 instancePosition = mul(float4(posL, 1), input.localTransform);
	
	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

technique11 ShadowMapPositionColor
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColor()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionColorI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionColorSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorSkinned()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionColorSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorSkinnedI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalColor
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColor()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalColorI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalColorSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorSkinned()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalColorSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorSkinnedI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionTexture
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTexture()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionTextureI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinned()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionTextureSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinnedI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTexture
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTexture()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTextureI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinned()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTextureSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinnedI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTextureTangent
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangent()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTextureTangentI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTextureTangentSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinned()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}

technique11 ShadowMapPositionNormalTextureTangentSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinnedI()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}