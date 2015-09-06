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

Texture2D gTG1Map : register(t0);
Texture2D gTG2Map : register(t1);
Texture2D gTG3Map : register(t2);
Texture2D gTG4Map : register(t3);
Texture2D gShadowMap : register(t4);
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
	float4 litColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

    float4 diffuseColor = gTG1Map.Sample(SamplerPoint, input.tex); //Color
    float4 depth = gTG3Map.Sample(SamplerPoint, input.tex); //Depth
    float4 normal = gTG2Map.Sample(SamplerPoint, input.tex); //Normal
    float4 shadow = gTG4Map.Sample(SamplerPoint, input.tex); //Shadow positions

	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	litColor = ComputeDirectionalLight(
		gDirLight,
		toEye,
		depth.xyz,
		normal.xyz,
		shadow.w,
		normal.w,
		float4(shadow.xyz, 1),
		gShadowMap);

	litColor *= diffuseColor;
	litColor.a = 1;

	return litColor;
}
float4 PSPointLight(PSPointLightInput input) : SV_TARGET
{
	float4 litColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

    float4 diffuseColor = gTG1Map.Sample(SamplerPoint, tex); //Color
    float4 depth = gTG3Map.Sample(SamplerPoint, tex); //Depth
    float4 normal = gTG2Map.Sample(SamplerPoint, tex); //Normal
	
	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	litColor = ComputePointLight(
		gPointLight,
		toEye,
		depth.xyz,
		normal.xyz,
		1,
		normal.w);

	litColor *= diffuseColor;
	litColor.a = 1;

	return litColor;
}
float4 PSSpotLight(PSSpotLightInput input) : SV_TARGET
{
	float4 litColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 position = input.positionScreen;
	float2 tex = 0.5f * (float2(position.x, -position.y) + 1);

    float4 diffuseColor = gTG1Map.Sample(SamplerPoint, tex); //Color
    float4 depth = gTG3Map.Sample(SamplerPoint, tex); //Depth
    float4 normal = gTG2Map.Sample(SamplerPoint, tex); //Normal
	
	float3 toEye = normalize(gEyePositionWorld - depth.xyz);

	litColor = ComputeSpotLight(
		gSpotLight,
		toEye,
		depth.xyz,
		normal.xyz,
		1,
		normal.w);

	litColor *= diffuseColor;
	litColor.a = 1;

	return litColor;
}
float4 PSCombineLights(PSCombineLightsInput input) : SV_TARGET
{
    float4 depth = gTG3Map.Sample(SamplerPoint, input.tex);
	float4 litColor = gLightMap.Sample(SamplerPoint, input.tex);

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - depth.xyz;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return litColor;
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
