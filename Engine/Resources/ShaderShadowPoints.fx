#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbGlobals : register(b0)
{
	uint gAnimationPaletteWidth;
};
Texture2D gAnimationPalette : register(t0);

cbuffer cbGSPerFrame : register(b1)
{
	float4x4 gGSWorldViewProjection[6];
};

cbuffer cbVSPerInstance : register(b2)
{
    uint gVSAnimationOffset;
	uint3 PAD21;
};

Texture2DArray gPSDiffuseMapArray : register(t1);

cbuffer cbPSPerInstance : register(b5)
{
	uint gPSTextureIndex;
    uint3 PAD51;
};

PSShadowMapPosition VSSMPositionColor(VSVertexPositionColor input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);

	return output;
}
PSShadowMapPosition VSSMPositionColorI(VSVertexPositionColorI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), input.localTransform);
    
	return output;
}
PSShadowMapPosition VSSMPositionColorSkinned(VSVertexPositionColorSkinned input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
    output.positionHomogeneous = positionL;

	return output;
}
PSShadowMapPosition VSSMPositionColorSkinnedI(VSVertexPositionColorSkinnedI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

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
	
    output.positionHomogeneous = instancePosition;
    
	return output;
}

PSShadowMapPosition VSSMPositionNormalColor(VSVertexPositionNormalColor input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);

	return output;
}
PSShadowMapPosition VSSMPositionNormalColorI(VSVertexPositionNormalColorI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = instancePosition;

	return output;
}
PSShadowMapPosition VSSMPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
    output.positionHomogeneous = positionL;

	return output;
}
PSShadowMapPosition VSSMPositionNormalColorSkinnedI(VSVertexPositionNormalColorSkinnedI input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

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
	
    output.positionHomogeneous = instancePosition;

	return output;
}

PSShadowMapPositionTexture VSSMPositionTexture(VSVertexPositionTexture input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionTextureI(VSVertexPositionTextureI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = instancePosition;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
    output.positionHomogeneous = positionL;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionTextureSkinnedI(VSVertexPositionTextureSkinnedI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

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
	
    output.positionHomogeneous = instancePosition;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

PSShadowMapPositionTexture VSSMPositionNormalTexture(VSVertexPositionNormalTexture input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureI(VSVertexPositionNormalTextureI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = instancePosition;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;
	
	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
    output.positionHomogeneous = positionL;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureSkinnedI(VSVertexPositionNormalTextureSkinnedI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

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
	
    output.positionHomogeneous = instancePosition;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

PSShadowMapPositionTexture VSSMPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

    output.positionHomogeneous = float4(input.positionLocal, 1.0f);
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureTangentI(VSVertexPositionNormalTextureTangentI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    output.positionHomogeneous = instancePosition;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		positionL);
	
    output.positionHomogeneous = positionL;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = gPSTextureIndex;

	return output;
}
PSShadowMapPositionTexture VSSMPositionNormalTextureTangentSkinnedI(VSVertexPositionNormalTextureTangentSkinnedI input)
{
	PSShadowMapPositionTexture output = (PSShadowMapPositionTexture) 0;

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
	
    output.positionHomogeneous = instancePosition;
	output.depth = output.positionHomogeneous;
	output.tex = input.tex;
	output.textureIndex = input.textureIndex;

	return output;
}

struct GSShadowMap
{
    float4 position : SV_POSITION;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(18)]
void GSPointShadowMap(triangle PSShadowMapPosition input[3] : SV_Position, inout TriangleStream<GSShadowMap> outputStream)
{
    for (int iFace = 0; iFace < 6; iFace++)
    {
        GSShadowMap output;

        output.index = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.position = mul(input[v].positionHomogeneous, gGSWorldViewProjection[iFace]);
            
            outputStream.Append(output);
        }
        outputStream.RestartStrip();
    }
}

struct GSShadowMapTexture
{
    float4 position : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint textureIndex : TEXTUREINDEX;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

[maxvertexcount(18)]
void GSPointShadowMapTexture(triangle PSShadowMapPositionTexture input[3] : SV_Position, inout TriangleStream<GSShadowMapTexture> outputStream)
{
    for (int iFace = 0; iFace < 6; iFace++)
    {
        GSShadowMapTexture output;

        output.index = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.position = mul(input[v].positionHomogeneous, gGSWorldViewProjection[iFace]);
            output.depth = output.position;
            output.tex = input[v].tex;
            output.textureIndex = input[v].textureIndex;
            
            outputStream.Append(output);
        }
        outputStream.RestartStrip();
    }
}

float4 PSDefault(GSShadowMapTexture input) : SV_Target
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
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}
technique11 ShadowMapPositionColorI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}
technique11 ShadowMapPositionColorSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorSkinned()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}
technique11 ShadowMapPositionColorSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorSkinnedI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}

technique11 ShadowMapPositionNormalColor
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColor()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}
technique11 ShadowMapPositionNormalColorI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}
technique11 ShadowMapPositionNormalColorSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorSkinned()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}
technique11 ShadowMapPositionNormalColorSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorSkinnedI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMap()));
		SetPixelShader(NULL);
	}
}

technique11 ShadowMapPositionTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionTexture()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionTextureI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionTextureSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinned()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionTextureSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinnedI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}

technique11 ShadowMapPositionNormalTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTexture()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinned()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinnedI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}

technique11 ShadowMapPositionNormalTextureTangent
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangent()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureTangentI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureTangentSkinned
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinned()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}
technique11 ShadowMapPositionNormalTextureTangentSkinnedI
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinnedI()));
        SetGeometryShader(CompileShader(gs_5_0, GSPointShadowMapTexture()));
		SetPixelShader(CompileShader(ps_5_0, PSDefault()));
	}
}