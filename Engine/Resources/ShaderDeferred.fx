#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float3 gEyePositionWorld;
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
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
	//Color
    float4 diffuseColor = gColorMap.Sample(SamplerPoint, input.tex);

	//Depth
    float4 depth = gDepthMap.Sample(SamplerPoint, input.tex);
	[flatten]
	if(depth.w == 1.0f)
	{
		return diffuseColor;
	}

    //Normal
    float4 normal = gNormalMap.Sample(SamplerPoint, input.tex);
	[flatten]
	if(length(normal.xyz) == 0.0f)
	{
		return diffuseColor;
	}

	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	float4 color = ComputeDirectionalLight(
		gDirLight,
		toEye,
		depth.xyz,
		normal.xyz,
		1,
		normal.w);

	color = diffuseColor * color;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - depth.xyz;
		float distToEye = length(toEyeWorld);

		color = ComputeFog(color, distToEye, gFogStart, gFogRange, gFogColor);
	}

	color.a = diffuseColor.a;

	return color;
}
float4 PSPointLight(PSPointLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

	//Color
    float4 diffuseColor = gColorMap.Sample(SamplerPoint, tex);

	//Depth
    float4 depth = gDepthMap.Sample(SamplerPoint, tex);
	[flatten]
	if(depth.w == 1.0f)
	{
		return diffuseColor;
	}

    //Normal
    float4 normal = gNormalMap.Sample(SamplerPoint, tex);
	[flatten]
	if(length(normal.xyz) == 0.0f)
	{
		return diffuseColor;
	}

	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	float4 color = ComputePointLight(
		gPointLight,
		toEye,
		depth.xyz,
		normal.xyz,
		1,
		normal.w);

	color = diffuseColor * color;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - depth.xyz;
		float distToEye = length(toEyeWorld);

		color = ComputeFog(color, distToEye, gFogStart, gFogRange, gFogColor);
	}

	color.a = diffuseColor.a;

	return color;
}
float4 PSSpotLight(PSSpotLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

	//Color
    float4 diffuseColor = gColorMap.Sample(SamplerPoint, tex);

	//Depth
    float4 depth = gDepthMap.Sample(SamplerPoint, tex);
	[flatten]
	if(depth.w == 1.0f)
	{
		return diffuseColor;
	}

    //Normal
    float4 normal = gNormalMap.Sample(SamplerPoint, tex);
	[flatten]
	if(length(normal.xyz) == 0.0f)
	{
		return diffuseColor;
	}

	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	float4 color = ComputeSpotLight(
		gSpotLight,
		toEye,
		depth.xyz,
		normal.xyz,
		1,
		normal.w);

	color = diffuseColor * color;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - depth.xyz;
		float distToEye = length(toEyeWorld);

		color = ComputeFog(color, distToEye, gFogStart, gFogRange, gFogColor);
	}

	color.a = diffuseColor.a;

	return color;
}
float4 PSCombineLights(PSCombineLightsInput input) : SV_TARGET
{
	return gLightMap.Sample(SamplerPoint, input.tex);
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
