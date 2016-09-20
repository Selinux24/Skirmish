#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float4x4 gLightViewProjection;
	float3 gEyePositionWorld;
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
	uint gShadows;
};

Texture2D gTG1Map : register(t0);
Texture2D gTG2Map : register(t1);
Texture2D gTG3Map : register(t2);
Texture2D gShadowMapStatic : register(t3);
Texture2D gShadowMapDynamic : register(t4);
Texture2D gLightMap : register(t5);

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
    float4 diffuseColor = gTG1Map.Sample(SamplerPoint, input.tex); //Color
    float4 depth = gTG3Map.Sample(SamplerPoint, input.tex); //Depth
    float4 normal = gTG2Map.Sample(SamplerPoint, input.tex); //Normal

	float4 lPosition = mul(float4(depth.xyz, 1), gLightViewProjection);

	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	float3 litColor = ComputeDirectionalLight(
		gDirLight,
		toEye,
		diffuseColor.rgb,
		depth.xyz,
		normal.xyz,
		depth.w,
		normal.w,
		lPosition,
		gShadows,
		gShadowMapStatic,
		gShadowMapDynamic);

	return float4(litColor, diffuseColor.a);
}
float4 PSPointLight(PSPointLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

    float4 diffuseColor = gTG1Map.Sample(SamplerPoint, tex); //Color
    float4 normal = gTG2Map.Sample(SamplerPoint, tex); //Normal
    float4 depth = gTG3Map.Sample(SamplerPoint, tex); //Depth
	
	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	float3 litColor = ComputePointLight(
		gPointLight,
		toEye,
		diffuseColor.rgb,
		depth.xyz,
		normal.xyz,
		depth.w,
		normal.w);

	return float4(litColor, diffuseColor.a);
}
float4 PSSpotLight(PSSpotLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

    float4 diffuseColor = gTG1Map.Sample(SamplerPoint, tex); //Color
    float4 normal = gTG2Map.Sample(SamplerPoint, tex); //Normal
    float4 depth = gTG3Map.Sample(SamplerPoint, tex); //Depth
	
	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	float3 litColor = ComputeSpotLight(
		gSpotLight,
		toEye,
		diffuseColor.rgb,
		depth.xyz,
		normal.xyz,
		depth.w,
		normal.w);

	return float4(litColor, diffuseColor.a);
}
float4 PSCombineLights(PSCombineLightsInput input) : SV_TARGET
{
    float4 depth = gTG3Map.Sample(SamplerPoint, input.tex);
	float4 color = gLightMap.Sample(SamplerPoint, input.tex);

	float3 litColor = color.rgb;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - depth.xyz;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor.rgb);
	}

	return float4(litColor, color.a);
}

technique11 DeferredDirectionalLight
{
    pass P0
    {
		SetVertexShader(CompileShader(vs_5_0, VSDirectionalLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDirectionalLight()));
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
    }
}
