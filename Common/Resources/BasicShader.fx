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

struct VSVertexPositionColor
{
    float3 positionLocal : POSITION;
    float4 color : COLOR0;
};
struct VSVertexPositionNormalColor
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float4 color : COLOR0;
};
struct VSVertexPositionTexture
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
};
struct VSVertexPositionNormalTexture
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float2 tex : TEXCOORD0;
};

struct PSVertexPositionColor
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float4 color : COLOR0;
};
struct PSVertexPositionNormalColor
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float4 color : COLOR0;
};
struct PSVertexPositionTexture
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
};
struct PSVertexPositionNormalTexture
{
    float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float2 tex : TEXCOORD0;
};

PSVertexPositionColor VSPositionColor(VSVertexPositionColor input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
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

		float fogLerp = saturate((distToEye - gFogStart) / gFogRange);

		litColor = lerp(input.color, gFogColor, fogLerp);
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
    
	output.normalWorld = normalize(output.normalWorld);

    return output;
}

float4 PSPositionNormalColor(PSVertexPositionNormalColor input) : SV_TARGET
{
	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;

	float distToEye = length(toEyeWorld);

	toEyeWorld /= distToEye;

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float4 A, D, S;

	[unroll]
	for(int i = 0; i < 3; ++i)
	{
		if(gDirLights[i].Padding == 1.0f)
		{
			ComputeDirectionalLight(
				gMaterial, 
				gDirLights[i],
				input.normalWorld, 
				toEyeWorld, 
				A, 
				D, 
				S);

			ambient += A;
			diffuse += D;
			spec += S;
		}
	}

	if(gPointLight.Padding == 1.0f)
	{
		ComputePointLight(
			gMaterial, 
			gPointLight,
			input.positionWorld, 
			input.normalWorld, 
			toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		spec += S;
	}

	if(gSpotLight.Padding == 1.0f)
	{
		ComputeSpotLight(
			gMaterial, 
			gSpotLight,
			input.positionWorld, 
			input.normalWorld, 
			toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		spec += S;
	}

	float4 litColor = ambient + diffuse + spec;
	
	if(gFogRange > 0)
	{
		float fogLerp = saturate((distToEye - gFogStart) / gFogRange);

		litColor = lerp(litColor, gFogColor, fogLerp);
	}

	litColor.a = gMaterial.Diffuse.a;
	
	return litColor;
}

PSVertexPositionTexture VSPositionTexture(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
    
    return output;
}

float4 PSPositionTexture(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTexture.Sample(samAnisotropic, input.tex);

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;

		float distToEye = length(toEyeWorld);

		float fogLerp = saturate((distToEye - gFogStart) / gFogRange);

		litColor = lerp(litColor, gFogColor, fogLerp);
	}

	litColor.a = gMaterial.Diffuse.a;

	return litColor;
}

PSVertexPositionNormalTexture VSPositionNormalTexture(VSVertexPositionNormalTexture input)
{
    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tex = input.tex;
    
    return output;
}

float4 PSPositionNormalTexture(PSVertexPositionNormalTexture input) : SV_TARGET
{
	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;

	float distToEye = length(toEyeWorld);

	toEyeWorld /= distToEye;

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float4 A, D, S;

	[unroll]
	for(int i = 0; i < 3; ++i)
	{
		if(gDirLights[i].Padding == 1.0f)
		{
			ComputeDirectionalLight(
				gMaterial, 
				gDirLights[i],
				input.normalWorld, 
				toEyeWorld, 
				A, 
				D, 
				S);

			ambient += A;
			diffuse += D;
			spec += S;
		}
	}

	if(gPointLight.Padding == 1.0f)
	{
		ComputePointLight(
			gMaterial, 
			gPointLight,
			input.positionWorld, 
			input.normalWorld, 
			toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		spec += S;
	}

	if(gSpotLight.Padding == 1.0f)
	{
		ComputeSpotLight(
			gMaterial, 
			gSpotLight,
			input.positionWorld, 
			input.normalWorld, 
			toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		spec += S;
	}

	float4 textureColor = gTexture.Sample(samAnisotropic, input.tex);

	float4 litColor = textureColor * (ambient + diffuse) + spec;

	if(gFogRange > 0)
	{
		float fogLerp = saturate((distToEye - gFogStart) / gFogRange);

		litColor = lerp(litColor, gFogColor, fogLerp);
	}

	litColor.a = gMaterial.Diffuse.a * textureColor.a;

	return litColor;
}

technique11 PositionColor
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionColor()));
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

technique11 PositionTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
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
