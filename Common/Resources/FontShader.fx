#include "Lights.fx"

cbuffer cbPerFrame : register (b0)
{
	DirectionalLight gDirLights[3];
	PointLight gPointLight;
	SpotLight gSpotLight;
	float3 gEyePositionWorld;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
};

cbuffer cbPerObject : register (b1)
{
	float4x4 gWorld;
	float4x4 gWorldInverse;
	float4x4 gWorldViewProjection;
	Material gMaterial;
};

Texture2D gTexture;

SamplerState samAnisotropic
{
	Filter = ANISOTROPIC;
	MaxAnisotropy = 4;

	AddressU = WRAP;
	AddressV = WRAP;
};

struct VSVertexPositionTexture
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
};

struct PSVertexPositionTexture
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
};

PSVertexPositionTexture VSFont(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
    
    return output;
}

float4 PSFont(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTexture.Sample(samAnisotropic, input.tex);

	if(litColor.r == 0.0f)
	{
		litColor.a = 0.0f;
	}
	else
	{
		litColor.a = 1.0f;
	}

	return litColor;
}

technique11 FontDrawer
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSFont()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSFont()));
	}
}