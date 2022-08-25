#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorldViewProjection;
	float gTime;
	float2 gTextureSize;
	float gEffectIntensity;

	float gBlurDirections;
	float gBlurQuality;
	float gBlurSize;
	float gVignetteOuter;
	float gVignetteInner;
	float gBloomIntensity;
	uint gToneMappingTone;
};

struct PSVertexEmpty
{
	float4 hpos : SV_Position;
	float2 uv : TEXCOORD0;
};

Texture2D gDiffuseMap : register(t0);
Texture2D gTexture1 : register(t1);
Texture2D gTexture2 : register(t2);

SamplerState SamplerLinear : register(s0)
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

#ifndef GAMMA_INVERSE
#define GAMMA_INVERSE 1.0/2.2
#endif

float3 inverseGamma = GAMMA_INVERSE.rrr;

float3 LinearToneMapping(float3 color)
{
	float exposure = 1.;
	color = clamp(exposure * color, 0., 1.);
	color = pow(color, inverseGamma);
	return color;
}

float3 SimpleReinhardToneMapping(float3 color)
{
	float exposure = 1.5;
	color *= exposure / (1. + color / exposure);
	color = pow(saturate(color), inverseGamma);
	return color;
}

float3 LumaBasedReinhardToneMapping(float3 color)
{
	float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
	float toneMappedLuma = luma / (1. + luma);
	color *= toneMappedLuma / luma;
	color = pow(saturate(color), inverseGamma);
	return color;
}

float3 WhitePreservingLumaBasedReinhardToneMapping(float3 color)
{
	float white = 2.;
	float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
	float toneMappedLuma = luma * (1. + luma / (white * white)) / (1. + luma);
	color *= toneMappedLuma / luma;
	color = pow(saturate(color), inverseGamma);
	return color;
}

float3 RomBinDaHouseToneMapping(float3 color)
{
	color = exp(-1.0 / (2.72 * color + 0.15));
	color = pow(saturate(color), inverseGamma);
	return color;
}

float3 FilmicToneMapping(float3 color)
{
	color = max(float3(0., 0., 0.), color - float3(0.004, 0.004, 0.004));
	color = (color * (6.2 * color + .5)) / (color * (6.2 * color + 1.7) + 0.06);
	return color;
}

float3 Uncharted2ToneMapping(float3 color)
{
	float A = 0.15;
	float B = 0.50;
	float C = 0.10;
	float D = 0.20;
	float E = 0.02;
	float F = 0.30;
	float W = 11.2;
	float exposure = 2.;
	color *= exposure;
	color = ((color * (A * color + C * B) + D * E) / (color * (A * color + B) + D * F)) - E / F;
	float white = ((W * (A * W + C * B) + D * E) / (W * (A * W + B) + D * F)) - E / F;
	color /= white;
	color = pow(saturate(color), inverseGamma);
	return color;
}

float GetVignette(float vOutter, float vInner, float2 uv)
{
	// Center of Screen
	float2 center = float2(0.5, 0.5);
	// Distance  between center and the current Uv. Multiplyed by 1.414213 to fit in the range of 0.0 to 1.0 
	float dist = distance(center, uv) * 1.414213;
	// Generate the Vignette with Clamp which go from outer Viggnet ring to inner vignette ring with smooth steps
	return clamp((vOutter - dist) / (vOutter - vInner), 0.0, 1.0);
}

float3 CalcBlur(float2 uv, Texture2D gDiffuseMap, float blurSize)
{
	float4 sum = 0;

	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x - 4.0 * blurSize, uv.y)) * 0.05;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x - 3.0 * blurSize, uv.y)) * 0.09;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x - 2.0 * blurSize, uv.y)) * 0.12;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x - blurSize, uv.y)) * 0.15;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y)) * 0.16;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x + blurSize, uv.y)) * 0.15;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x + 2.0 * blurSize, uv.y)) * 0.12;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x + 3.0 * blurSize, uv.y)) * 0.09;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x + 4.0 * blurSize, uv.y)) * 0.05;

	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y - 4.0 * blurSize)) * 0.05;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y - 3.0 * blurSize)) * 0.09;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y - 2.0 * blurSize)) * 0.12;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y - blurSize)) * 0.15;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y)) * 0.16;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y + blurSize)) * 0.15;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y + 2.0 * blurSize)) * 0.12;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y + 3.0 * blurSize)) * 0.09;
	sum += gDiffuseMap.Sample(SamplerLinear, float2(uv.x, uv.y + 4.0 * blurSize)) * 0.05;

	return sum.rgb;
}

float3 CalcGaussianBlur(float2 uv, Texture2D gDiffuseMap, float3 color, float Directions, float Quality, float2 Radius)
{
	float3 output = color;

	// Gaussian blur calculations
	for (float d = 0.0; d < TWO_PI; d += TWO_PI / Directions)
	{
		for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
		{
			float2 suv = uv + float2(cos(d), sin(d)) * Radius * i;
			output += gDiffuseMap.Sample(SamplerLinear, suv).rgb;
		}
	}

	// Output to screen
	output /= Quality * Directions - 15.0;

	return output;
}

float CalcGrain(float2 uv, float iTime)
{
	return frac(sin(dot(uv, float2(17.0, 180.))) * 2500. + iTime);
}

PSVertexEmpty VSEmpty(VSVertexPositionTexture input)
{
	PSVertexEmpty output;

	output.hpos = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.uv = input.tex;

	return output;
}

float4 PSEmpty(PSVertexEmpty input) : SV_TARGET
{
	return gDiffuseMap.Sample(SamplerLinear, input.uv);
}
float4 PSCombine(PSVertexEmpty input) : SV_TARGET
{
	float3 output1 = gTexture1.Sample(SamplerLinear, input.uv).rgb;
	float4 output2 = gTexture2.Sample(SamplerLinear, input.uv);

	float3 mix = (output1 * (1 - output2.a)) + (output2.rgb * output2.a);
	return float4(saturate(mix), 1);
}
float4 PSGrayscale(PSVertexEmpty input) : SV_TARGET
{
	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float greyScale = .5;
	float3 output = dot(color.rgb, greyScale.rrr).rrr;

	return float4(lerp(color.rgb, output, gEffectIntensity), color.a);
}
float4 PSSepia(PSVertexEmpty input) : SV_TARGET
{
	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float3 sepia = float3(1.2, 1.0, 0.8);
	float grey = dot(color.rgb, float3(0.299, 0.587, 0.114));
	float3 output = grey.rrr * sepia;
	output = lerp(color.rgb, output, 0.75);

	return float4(lerp(color.rgb, output, gEffectIntensity), color.a);
}
float4 PSVignette(PSVertexEmpty input) : SV_TARGET
{
	float vOutter = gVignetteOuter;
	float vInner = gVignetteInner;

	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float vig = GetVignette(vOutter, vInner, input.uv);

	// Multiply the Vignette with the texture color
	float3 output = color.rgb * vig;
	output = lerp(color.rgb, output, gEffectIntensity);

	return float4(output, color.a);
}
float4 PSBlur(PSVertexEmpty input) : SV_TARGET
{
	float directions = gBlurDirections;
	float quality = gBlurQuality;
	float2 radius = gBlurSize / gTextureSize;

	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float3 gau = CalcGaussianBlur(input.uv, gDiffuseMap, color.rgb, directions, quality, radius);

	float3 output = lerp(color.rgb, gau, gEffectIntensity);

	return float4(output, color.a);
}
float4 PSBlurVignette(PSVertexEmpty input) : SV_TARGET
{
	float directions = gBlurDirections;
	float quality = gBlurQuality;
	float2 radius = gBlurSize / gTextureSize;
	float vOutter = gVignetteOuter;
	float vInner = gVignetteInner;

	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float3 gau = CalcGaussianBlur(input.uv, gDiffuseMap, color.rgb, directions, quality, radius);
	float vig = GetVignette(vOutter, vInner, input.uv);

	// Lerp Gaussian Blur and texture color by the Vignette value
	float3 output = lerp(gau, color.rgb, vig);
	output = lerp(color.rgb, output, gEffectIntensity);

	return float4(output, color.a);
}
float4 PSBloom(PSVertexEmpty input) : SV_TARGET
{
	float intensity = gBloomIntensity;
	float directions = gBlurDirections;
	float quality = gBlurQuality;
	float2 radius = gBlurSize / gTextureSize;

	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float3 blur = CalcGaussianBlur(input.uv, gDiffuseMap, color.rgb, directions, quality, radius);

	//Bloom intensity
	float3 output = blur * intensity + color.rgb;
	output = lerp(color.rgb, output, gEffectIntensity);

	return float4(output, color.a);
}
float4 PSGrain(PSVertexEmpty input) : SV_TARGET
{
	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float3 grain = CalcGrain(input.uv, gTime);

	float3 output = lerp(color.rgb, grain, .1);

	return float4(lerp(color.rgb, output, gEffectIntensity), color.a);
}
float4 PSToneMapping(PSVertexEmpty input) : SV_TARGET
{
	uint toneMap = gToneMappingTone;

	float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);
	if (gEffectIntensity == 0)
	{
		return color;
	}

	float3 output = color.rgb;

	if (toneMap == 1)
		output = LinearToneMapping(color.rgb);
	if (toneMap == 2)
		output = SimpleReinhardToneMapping(color.rgb);
	if (toneMap == 3)
		output = LumaBasedReinhardToneMapping(color.rgb);
	if (toneMap == 4)
		output = WhitePreservingLumaBasedReinhardToneMapping(color.rgb);
	if (toneMap == 5)
		output = RomBinDaHouseToneMapping(color.rgb);
	if (toneMap == 6)
		output = FilmicToneMapping(color.rgb);
	if (toneMap == 7)
		output = Uncharted2ToneMapping(color.rgb);

	return float4(lerp(color.rgb, output, gEffectIntensity), color.a);
}

technique11 Empty
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSEmpty()));
	}
}
technique11 Combine
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSCombine()));
	}
}
technique11 Grayscale
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGrayscale()));
	}
}
technique11 Sepia
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSSepia()));
	}
}
technique11 Vignette
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSVignette()));
	}
}
technique11 Blur
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSBlur()));
	}
}
technique11 BlurVignette
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSBlurVignette()));
	}
}
technique11 Bloom
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSBloom()));
	}
}
technique11 Grain
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSGrain()));
	}
}
technique11 ToneMapping
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSToneMapping()));
	}
}
