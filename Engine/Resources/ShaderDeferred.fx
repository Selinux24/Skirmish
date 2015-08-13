#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float3 gEyePositionWorld;
	float gPadding;
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;
};

Texture2D gColorMap : register(t0);
Texture2D gNormalMap : register(t1);
Texture2D gDepthMap : register(t2);
Texture2D gLightMap : register(t3);

SamplerState SampleTypePoint : register(s0);

struct PSDirectionalLightInput
{
    float4 positionHomogeneous : SV_POSITION;
    float2 tex : TEXCOORD0;
};
struct PSPointLightInput
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION0;
	float4 positionScreen : TEXCOORD0;
};
struct PSSpotLightInput
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION0;
	float4 positionScreen : TEXCOORD0;
};
struct PSCombineLightsInput
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

PSDirectionalLightInput VSDirectionalLight(VSVertexPositionTexture input)
{
    PSDirectionalLightInput output = (PSDirectionalLightInput)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.tex = input.tex;
    
    return output;
}
PSPointLightInput VSPointLight(VSVertexPosition input)
{
    PSPointLightInput output = (PSPointLightInput)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
    output.positionScreen = output.positionHomogeneous;
    
    return output;
}
PSSpotLightInput VSSpotLight(VSVertexPosition input)
{
    PSSpotLightInput output = (PSSpotLightInput)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
    output.positionScreen = output.positionHomogeneous;
    
    return output;
}
PSCombineLightsInput VSCombineLights(VSVertexPositionTexture input)
{
    PSDirectionalLightInput output = (PSDirectionalLightInput)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.tex = input.tex;

    return output;
}

float4 PSDirectionalLight(PSDirectionalLightInput input) : SV_TARGET
{
	float4 outputColor = 0.0f;

    //Color
    float4 color = gColorMap.Sample(SampleTypePoint, input.tex);
    //Normal
    float4 normal = gNormalMap.Sample(SampleTypePoint, input.tex);
	//Depth
    float4 depth = gDepthMap.Sample(SampleTypePoint, input.tex);

	if(length(normal) != 0.0f)
	{
		//Get the light direction
		float3 lightDir = -gDirLight.Direction;

		//Calculate the amount of light on this pixel
		float lightIntensity = saturate(dot(normal.xyz, lightDir));

		//Determine the final amount of diffuse color based on the color of the pixel combined with the light intensity.
		outputColor = saturate(color * lightIntensity);
		outputColor.a = 1.0f;
	}
	else
	{
		outputColor = color;
	}

	return outputColor;
}
float4 PSPointLight(PSPointLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 texCoord = 0.5f * (float2(position.x, -position.y) + 1);

    //Color
    float4 color = gColorMap.Sample(SampleTypePoint, texCoord);
    //Normal
    float4 normal = gNormalMap.Sample(SampleTypePoint, texCoord);
	//Depth
    float4 depth = gDepthMap.Sample(SampleTypePoint, texCoord);

	if(depth.w == 1.0f)
	{
		return color;
	}

	if(length(normal.xyz) == 0.0f)
	{
		return color;
	}

	Material mat = (Material)0;
	mat.Ambient = float4(1.0f, 1.0f, 1.0f, 1.0f);
	mat.Diffuse = float4(1.0f, 1.0f, 1.0f, 1.0f);
	mat.Specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float3 toEyeWorld = gEyePositionWorld - depth.xyz;
	float distToEye = length(toEyeWorld);
	toEyeWorld /= distToEye;

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

	ComputePointLight(
		mat, 
		gPointLight, 
		depth.xyz,
		normal.xyz, 
		toEyeWorld,
		ambient, 
		diffuse, 
		specular);

	return color * (ambient + diffuse) + specular;
}
float4 PSSpotLight(PSSpotLightInput input) : SV_TARGET
{
	float4 outputColor = 0.0f;

	//Get texture coordinates
	float4 position = input.positionScreen;
    position.xy /= position.w;
	float2 texCoord = 0.5f * (float2(position.x, -position.y) + 1);

    //Color
    float4 color = gColorMap.Sample(SampleTypePoint, texCoord);
    //Normal
    float4 normal = gNormalMap.Sample(SampleTypePoint, texCoord);
	//Depth
    float4 depth = gDepthMap.Sample(SampleTypePoint, texCoord);

	if(length(normal) != 0.0f)
	{
		outputColor = color;
	}
	else
	{
		outputColor = color;
	}

	return outputColor;
}
float4 PSCombineLights(PSCombineLightsInput input) : SV_TARGET
{
    float4 diffuseColor = gColorMap.Sample(SampleTypePoint, input.tex);
    float4 depth = gDepthMap.Sample(SampleTypePoint, input.tex);
    float4 lightColor = gLightMap.Sample(SampleTypePoint, input.tex);

	if(depth.w == 1.0f)
	{
		return diffuseColor;
	}

	return float4(diffuseColor.rgb * lightColor.rgb, 1.0f);
}

technique11 DeferredDirectionalLight
{
    pass P0
    {
		SetVertexShader(CompileShader(vs_5_0, VSDirectionalLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDirectionalLight()));

		//SetRasterizerState(RasterizerSolid);
    }
}
technique11 DeferredPointLight
{
    pass P0
    {
		SetVertexShader(CompileShader(vs_5_0, VSPointLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPointLight()));
    }
}
technique11 DeferredSpotLight
{
    pass P0
    {
		SetVertexShader(CompileShader(vs_5_0, VSSpotLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSSpotLight()));
    }
}
technique11 DeferredCombineLights
{
    pass P0
    {
		SetVertexShader(CompileShader(vs_5_0, VSCombineLights()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSCombineLights()));

		SetRasterizerState(RasterizerSolid);
    }
}
