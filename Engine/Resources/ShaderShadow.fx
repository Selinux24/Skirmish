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
	float4 positionHomogeneus : SV_POSITION;
	float4 color : COLOR0;
};

struct ShadowMapTextureOut
{
	float4 positionHomogeneus : SV_POSITION;
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

	output.positionHomogeneus = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapColorOut VSSMPositionColorSkinned(VSVertexPositionColorSkinned input)
{
	ShadowMapColorOut output = (ShadowMapColorOut)0;

	float weights[4] = {0.0f, 0.0f, 0.0f, 0.0f};
	weights[0] = input.weights.x;
	weights[1] = input.weights.y;
	weights[2] = input.weights.z;
	weights[3] = 1.0f - weights[0] - weights[1] - weights[2];
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		posL += weights[i] * mul(float4(input.positionLocal, 1.0f), gBoneTransforms[input.boneIndices[i]]).xyz;
	}
	
	output.positionHomogeneus = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapColorOut VSSMPositionNormalColor(VSVertexPositionNormalColor input)
{
	ShadowMapColorOut output = (ShadowMapColorOut)0;

	output.positionHomogeneus = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapColorOut VSSMPositionNormalColorSkinned(VSVertexPositionNormalColorSkinned input)
{
	ShadowMapColorOut output = (ShadowMapColorOut)0;

	float weights[4] = {0.0f, 0.0f, 0.0f, 0.0f};
	weights[0] = input.weights.x;
	weights[1] = input.weights.y;
	weights[2] = input.weights.z;
	weights[3] = 1.0f - weights[0] - weights[1] - weights[2];
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		posL += weights[i] * mul(float4(input.positionLocal, 1.0f), gBoneTransforms[input.boneIndices[i]]).xyz;
	}
	
	output.positionHomogeneus = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.color = input.color;

	return output;
}

ShadowMapTextureOut VSSMPositionTexture(VSVertexPositionTexture input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	output.positionHomogeneus = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionTextureSkinned(VSVertexPositionTextureSkinned input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	float weights[4] = {0.0f, 0.0f, 0.0f, 0.0f};
	weights[0] = input.weights.x;
	weights[1] = input.weights.y;
	weights[2] = input.weights.z;
	weights[3] = 1.0f - weights[0] - weights[1] - weights[2];
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		posL += weights[i] * mul(float4(input.positionLocal, 1.0f), gBoneTransforms[input.boneIndices[i]]).xyz;
	}
	
	output.positionHomogeneus = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionNormalTexture(VSVertexPositionNormalTexture input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	output.positionHomogeneus = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;
	
	float weights[4] = {0.0f, 0.0f, 0.0f, 0.0f};
	weights[0] = input.weights.x;
	weights[1] = input.weights.y;
	weights[2] = input.weights.z;
	weights[3] = 1.0f - weights[0] - weights[1] - weights[2];
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		posL += weights[i] * mul(float4(input.positionLocal, 1.0f), gBoneTransforms[input.boneIndices[i]]).xyz;
	}
	
	output.positionHomogeneus = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.tex = input.tex;
	
	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureTangent(VSVertexPositionNormalTextureTangent input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	output.positionHomogeneus = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);
	output.tex = input.tex;

	return output;
}

ShadowMapTextureOut VSSMPositionNormalTextureTangentSkinned(VSVertexPositionNormalTextureTangentSkinned input)
{
	ShadowMapTextureOut output = (ShadowMapTextureOut)0;

	float weights[4] = {0.0f, 0.0f, 0.0f, 0.0f};
	weights[0] = input.weights.x;
	weights[1] = input.weights.y;
	weights[2] = input.weights.z;
	weights[3] = 1.0f - weights[0] - weights[1] - weights[2];
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		posL += weights[i] * mul(float4(input.positionLocal, 1.0f), gBoneTransforms[input.boneIndices[i]]).xyz;
	}
	
	output.positionHomogeneus = mul(float4(posL, 1.0f), gWorldViewProjection);
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

technique11 ShadowMapPositionColorSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionColorSkinned()));
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

technique11 ShadowMapPositionNormalColorSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalColorSkinned()));
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

technique11 ShadowMapPositionTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionTextureSkinned()));
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

technique11 ShadowMapPositionNormalTextureSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureSkinned()));
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

technique11 ShadowMapPositionNormalTextureTangentSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSSMPositionNormalTextureTangentSkinned()));
        SetPixelShader(NULL);

		SetRasterizerState(Depth);
    }
}