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
	//Depth
    float4 depth = gDepthMap.Sample(SamplerPoint, input.tex);
	[flatten]
	if(depth.w == 1.0f)
	{
		return float4(1.0f, 1.0f, 1.0f, 0.0f);
	}

    //Normal
    float4 normal = gNormalMap.Sample(SamplerPoint, input.tex);
	[flatten]
	if(length(normal.xyz) == 0.0f)
	{
		return float4(1.0f, 1.0f, 1.0f, 0.0f);
	}

	float4 color = ComputeDirectionalLight2(
		gDirLight,
		gEyePositionWorld,
		normal.xyz);

	return color.r > 1 ? float4(10,0,0,0) : color;
}
float4 PSPointLight(PSPointLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

	//Depth
    float4 depth = gDepthMap.Sample(SamplerPoint, tex);
	[flatten]
	if(depth.w == 1.0f)
	{
		return float4(1.0f, 1.0f, 1.0f, 0.0f);
	}

    //Normal
    float4 normal = gNormalMap.Sample(SamplerPoint, tex);
	[flatten]
	if(length(normal.xyz) == 0.0f)
	{
		return float4(1.0f, 1.0f, 1.0f, 0.0f);
	}

	float4 color = ComputePointLight2(
		gPointLight,
		gEyePositionWorld,
		depth.xyz,
		normal.xyz);

	return color;
}
float4 PSSpotLight(PSSpotLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

	//Depth
    float4 depth = gDepthMap.Sample(SamplerPoint, tex);
	[flatten]
	if(depth.w == 1.0f)
	{
		return float4(1.0f, 1.0f, 1.0f, 0.0f);
	}

    //Normal
    float4 normal = gNormalMap.Sample(SamplerPoint, tex);
	[flatten]
	if(length(normal.xyz) == 0.0f)
	{
		return float4(1.0f, 1.0f, 1.0f, 0.0f);
	}

	float4 color = ComputeSpotLight2(
		gSpotLight,
		gEyePositionWorld,
		depth.xyz,
		normal.xyz);

	return color;
}
float4 PSCombineLights(PSCombineLightsInput input) : SV_TARGET
{
    float4 diffuseColor = gColorMap.Sample(SamplerPoint, input.tex);
    float4 normal = gNormalMap.Sample(SamplerPoint, input.tex);
    float4 depth = gDepthMap.Sample(SamplerPoint, input.tex);

	float4 color;

	if(depth.w == 1.0f)
	{
		color = float4(diffuseColor.rgb * normal.w, 1.0f);
	}
	else
	{
		float4 lightColor = gLightMap.Sample(SamplerPoint, input.tex);
		
		color = float4(diffuseColor.rgb * (lightColor.rgb + normal.w), 1.0f);
	}
	
	return color;
}

technique11 DeferredDirectionalLight
{
    pass P0
    {
		SetVertexShader(CompileShader(vs_5_0, VSDirectionalLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDirectionalLight()));

		SetRasterizerState(RasterizerSolid);
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
