#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float4x4 gLightViewProjection;
	float3 gEyePositionWorld;
	float gGlobalAmbient;
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;
	float gLightCount;
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
    float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, input.tex, 0);
    float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, input.tex, 0);

	float3 normal = tg2.xyz;
	float3 position = tg3.xyz;
	float shininess = tg2.w;

	float4 lightPosition = mul(float4(position, 1), gLightViewProjection);

	float4 diffuse = 0;
	float4 specular = 0;

	ComputeDirectionalLight(
		gDirLight, 
		shininess,
		position,
		normal,
		gEyePositionWorld,
		lightPosition,
		gShadows,
		gShadowMapStatic,
		gShadowMapDynamic,
		diffuse,
		specular);

	Material k = MaterialDefault();

	diffuse = k.Diffuse * diffuse;
	specular = k.Specular * specular;

	return diffuse + specular;
}
float4 PSPointLight(PSPointLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 lPosition = input.positionScreen;
	float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);

    float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
    float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);

	float3 normal = tg2.xyz;
	float3 position = tg3.xyz;
	float shininess = tg2.w;

	float4 lightPosition = mul(float4(position, 1), gLightViewProjection);

	float4 diffuse = 0;
	float4 specular = 0;

	ComputePointLight(
		gPointLight, 
		shininess,
		position,
		normal,
		gEyePositionWorld,
		lightPosition,
		gShadows,
		gShadowMapStatic,
		gShadowMapDynamic,
		diffuse,
		specular);

	Material k = MaterialDefault();

	diffuse = k.Diffuse * diffuse;
	specular = k.Specular * specular;

	return diffuse + specular;
}
float4 PSSpotLight(PSSpotLightInput input) : SV_TARGET
{
	input.positionScreen.xy /= input.positionScreen.w;

	//Get texture coordinates
	float4 lPosition = input.positionScreen;
	float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);

    float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
    float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);
	
	float3 normal = tg2.xyz;
	float3 position = tg3.xyz;
	float shininess = tg2.w;

	float4 lightPosition = mul(float4(position, 1), gLightViewProjection);

	float4 diffuse = 0;
	float4 specular = 0;

	ComputeSpotLight(
		gSpotLight, 
		shininess,
		position,
		normal,
		gEyePositionWorld,
		lightPosition,
		gShadows,
		gShadowMapStatic,
		gShadowMapDynamic,
		diffuse,
		specular);

	Material k = MaterialDefault();

	diffuse = k.Diffuse * diffuse;
	specular = k.Specular * specular;

	return diffuse + specular;
}
float4 PSCombineLights(PSCombineLightsInput input) : SV_TARGET
{
    float4 tg1 = gTG1Map.SampleLevel(SamplerPoint, input.tex, 0);
	float4 lmap = gLightMap.Sample(SamplerPoint, input.tex);

	float4 color = tg1;
	float4 light = saturate(lmap);

	Material k = MaterialDefault();

	float4 emissive = k.Emissive;
	float4 ambient = k.Ambient * gGlobalAmbient;

	float4 c = (emissive + ambient + light) * color;

	if(gFogRange > 0)
	{
		float4 tg3 = gTG3Map.Sample(SamplerPoint, input.tex);
		float3 position = tg3.xyz;

		float distToEye = length(gEyePositionWorld - position);

		c = ComputeFog(c, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return c;
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
