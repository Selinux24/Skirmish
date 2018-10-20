#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"

cbuffer cbGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	float3 gLOD;
};
cbuffer cbPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float3 gEyePositionWorld;
	float PAD11;
};
cbuffer cbPerDirLight : register(b2)
{
	DirectionalLight gDirLight;
}
cbuffer cbPerPointLight : register(b3)
{
	PointLight gPointLight;
}
cbuffer cbPerSpotLight : register(b4)
{
	SpotLight gSpotLight;
}
cbuffer cbCombineLights : register(b5)
{
	HemisphericLight gHemiLight;
	float4 gFogColor;
	float gFogStart;
	float gFogRange;
	float PAD51;
	float PAD52;
}

Texture2D gTG1Map : register(t0);
Texture2D gTG2Map : register(t1);
Texture2D gTG3Map : register(t2);
Texture2D gLightMap : register(t3);
Texture2D gMaterialPalette : register(t4);
Texture2DArray<float> gShadowMapDir : register(t5);
Texture2DArray<float> gShadowMapSpot : register(t6);
TextureCubeArray<float> gShadowMapPoint : register(t7);

struct PSLightInput
{
	float4 positionHomogeneous : SV_POSITION;
	float4 positionScreen : TEXCOORD0;
};
struct PSStencilInput
{
	float4 positionHomogeneous : SV_POSITION;
};

PSLightInput VSLight(VSVertexPosition input)
{
	PSLightInput output = (PSLightInput) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionScreen = output.positionHomogeneous;

	return output;
}
PSStencilInput VSStencil(VSVertexPosition input)
{
	PSStencilInput output = (PSStencilInput) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);

	return output;
}

float4 PSDirectionalLight(PSLightInput input) : SV_TARGET
{
	//Get texture coordinates
	float4 lPosition = input.positionScreen;
	lPosition.xy /= lPosition.w;
	float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);

	float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
	float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);

	float3 normal = tg2.xyz;
	float doLighting = tg2.a;
	float3 position = tg3.xyz;
	float materialIndex = tg3.w;

	if (doLighting == 0)
	{
		Material k = GetMaterialData(gMaterialPalette, materialIndex, gMaterialPaletteWidth);

		ComputeDirectionalLightsInput linput;

		linput.dirLight = gDirLight;
		linput.lod = gLOD;
		linput.shininess = k.Shininess;
        linput.specular = k.Specular;
		linput.pPosition = position;
		linput.pNormal = normal;
		linput.ePosition = gEyePositionWorld;
		linput.shadowMap = gShadowMapDir;

		ComputeLightsOutput loutput = ComputeDirectionalLight(linput);

		return (k.Diffuse * loutput.diffuse) + (k.Specular * loutput.specular);
	}
	else
	{
		return 0;
	}
}
float4 PSPointLight(PSLightInput input) : SV_TARGET
{
	//Get texture coordinates
	float4 lPosition = input.positionScreen;
	lPosition.xy /= lPosition.w;
	float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);

	float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
	float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);

	float3 normal = tg2.xyz;
	float doLighting = tg2.a;
	float3 position = tg3.xyz;
	float materialIndex = tg3.w;

	if (doLighting == 0)
	{
		Material k = GetMaterialData(gMaterialPalette, materialIndex, gMaterialPaletteWidth);

		ComputePointLightsInput linput;

		linput.pointLight = gPointLight;
		linput.shininess = k.Shininess;
		linput.pPosition = position;
		linput.pNormal = normal;
		linput.ePosition = gEyePositionWorld;
		linput.lod = gLOD;
        linput.shadowMapPoint = gShadowMapPoint;

		ComputeLightsOutput loutput = ComputePointLight(linput);

		return (k.Diffuse * loutput.diffuse) + (k.Specular * loutput.specular);
	}
	else
	{
		return 0;
	}
}
float4 PSSpotLight(PSLightInput input) : SV_TARGET
{
	//Get texture coordinates
	float4 lPosition = input.positionScreen;
	lPosition.xy /= lPosition.w;
	float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);

	float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
	float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);

	float3 normal = tg2.xyz;
	float doLighting = tg2.a;
	float3 position = tg3.xyz;
	float materialIndex = tg3.w;

	if (doLighting == 0)
	{
		Material k = GetMaterialData(gMaterialPalette, materialIndex, gMaterialPaletteWidth);

		ComputeSpotLightsInput linput;

		linput.spotLight = gSpotLight;
		linput.shininess = k.Shininess;
		linput.pPosition = position;
		linput.pNormal = normal;
		linput.ePosition = gEyePositionWorld;
		linput.lod = gLOD;
        linput.shadowMap = gShadowMapSpot;

		ComputeLightsOutput loutput = ComputeSpotLight(linput);

		return (k.Diffuse * loutput.diffuse) + (k.Specular * loutput.specular);
	}
	else
	{
		return 0;
	}
};
float4 PSCombineLights(PSLightInput input) : SV_TARGET
{
	//Get texture coordinates
	float4 lPosition = input.positionScreen;
	lPosition.xy /= lPosition.w;
	float2 tex = 0.5f * (float2(lPosition.x, -lPosition.y) + 1);

	float4 tg1 = gTG1Map.SampleLevel(SamplerPoint, tex, 0);
	float4 tg2 = gTG2Map.SampleLevel(SamplerPoint, tex, 0);
	float4 tg3 = gTG3Map.SampleLevel(SamplerPoint, tex, 0);
	float4 lmap = gLightMap.Sample(SamplerPoint, tex);

	float doLighting = tg2.w;
	if (doLighting == 0)
	{
		float4 color = tg1;
		float3 normal = tg2.xyz;
		float3 position = tg3.xyz;
		float materialIndex = tg3.w;
		float4 light = lmap;

		float3 lAmbient = CalcAmbient(gHemiLight.AmbientDown, gHemiLight.AmbientRange, normal);

		Material k = GetMaterialData(gMaterialPalette, materialIndex, gMaterialPaletteWidth);

		color = LightEquation(k, lAmbient, light, color);

		if (gFogRange > 0)
		{
			float distToEye = length(gEyePositionWorld - position);

			color = ComputeFog(color, distToEye, gFogStart, gFogRange, gFogColor);
		}

		return saturate(color);
	}
	else
	{
		return tg1;
	}
};

technique11 DeferredDirectionalLight
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSDirectionalLight()));
	}
}
technique11 DeferredPointStencil
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSStencil()));
		SetGeometryShader(NULL);
		SetPixelShader(NULL);
	}
}
technique11 DeferredPointLight
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPointLight()));
	}
}
technique11 DeferredSpotStencil
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSStencil()));
		SetGeometryShader(NULL);
		SetPixelShader(NULL);
	}
}
technique11 DeferredSpotLight
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSSpotLight()));
	}
}
technique11 DeferredCombineLights
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSLight()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSCombineLights()));
	}
}
