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

GBufferPSInput VSGBPositionColor(VSVertexPositionColor input)
{
	GBufferPSInput output = (GBufferPSInput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
GBufferPSInput VSGBPositionColorI(VSVertexPositionColorI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}
GBufferPSInput VSGBPositionColorSkinned(VSVertexPositionColorSkinned input)
{
	GBufferPSInput output = (GBufferPSInput)0;

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
GBufferPSInput VSGBPositionColorSkinnedI(VSVertexPositionColorSkinnedI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

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

GBufferPSInput VSGBPositionNormalColor(VSVertexPositionNormalColor input)
{
	GBufferPSInput output = (GBufferPSInput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
GBufferPSInput VSGBPositionNormalColorI(VSVertexPositionNormalColorI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);

    return output;
}
GBufferPSInput VSGBPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
	GBufferPSInput output = (GBufferPSInput)0;

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
GBufferPSInput VSGBPositionNormalColorSkinnedI(VSVertexPositionNormalColorSkinnedI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

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

GBufferPSInput VSGBPositionTexture(VSVertexPositionTexture input)
{
	GBufferPSInput output = (GBufferPSInput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
GBufferPSInput VSGBPositionTextureI(VSVertexPositionTextureI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}
GBufferPSInput VSGBPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
	GBufferPSInput output = (GBufferPSInput)0;

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
GBufferPSInput VSGBPositionTextureSkinnedI(VSVertexPositionTextureSkinnedI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

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

GBufferPSInput VSGBPositionNormalTexture(VSVertexPositionNormalTexture input)
{
	GBufferPSInput output = (GBufferPSInput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
GBufferPSInput VSGBPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);

    return output;
}
GBufferPSInput VSGBPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	GBufferPSInput output = (GBufferPSInput)0;
	
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
GBufferPSInput VSGBPositionNormalTextureSkinnedI(VSVertexPositionNormalTextureSkinnedI input)
{
	GBufferPSInput output = (GBufferPSInput)0;

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

GBufferPSInput VSGBPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
	GBufferPSInput output = (GBufferPSInput)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}
GBufferPSInput VSGBPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
    GBufferPSInput output = (GBufferPSInput)0;

    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
    
    return output;
}
GBufferPSInput VSGBPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
	GBufferPSInput output = (GBufferPSInput)0;

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
GBufferPSInput VSGBPositionNormalTextureTangentSkinnedI(VSVertexPositionNormalTextureTangentSkinnedI input)
{
	GBufferPSInput output = (GBufferPSInput)0;

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

GBufferPSOutput PSGBGeneral(GBufferPSInput input)
{
	GBufferPSOutput output = (GBufferPSOutput)0;

    //Black color
    output.Color = 0.0f;
    output.Color.a = 0.0f;
    
	//When transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal.rgb = 0.5f;
    output.Normal.a = 0.0f; //No specular power
    
	//Max depth
    output.Depth = 1.0f;
    
	return output;
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 GBufferPositionColor
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionColorI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionColorI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionColorSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionColorSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionColorSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionColorSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}

technique11 GBufferPositionNormalColor
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalColorI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalColorI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalColorSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalColorSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalColorSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalColorSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}

technique11 GBufferPositionTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionTextureI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionTextureSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionTextureSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionTextureSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}

technique11 GBufferPositionNormalTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalTextureI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTextureI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalTextureSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalTextureSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTextureSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}

technique11 GBufferPositionNormalTextureTangent
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTextureTangent()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalTextureTangentI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTextureTangentI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalTextureTangentSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTextureTangentSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
technique11 GBufferPositionNormalTextureTangentSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSGBPositionNormalTextureTangentSkinnedI()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGBGeneral()));

		SetRasterizerState(RasterizerSolid);
	}
}
