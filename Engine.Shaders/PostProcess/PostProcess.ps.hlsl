#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncHelpers.hlsl"

#ifndef GAMMA_INVERSE
#define GAMMA_INVERSE 1.0/2.2
#endif

#ifndef EFFECT_EMPTY
#define EFFECT_EMPTY 0
#endif
#ifndef EFFECT_GRAYSCALE
#define EFFECT_GRAYSCALE 1
#endif
#ifndef EFFECT_SEPIA
#define EFFECT_SEPIA 2
#endif
#ifndef EFFECT_VIGNETTE
#define EFFECT_VIGNETTE 3
#endif
#ifndef EFFECT_BLUR
#define EFFECT_BLUR 4
#endif
#ifndef EFFECT_BLURVIGNETTE
#define EFFECT_BLURVIGNETTE 5
#endif
#ifndef EFFECT_BLOOM
#define EFFECT_BLOOM 6
#endif
#ifndef EFFECT_GRAIN
#define EFFECT_GRAIN 7
#endif
#ifndef EFFECT_TONEMAPPING
#define EFFECT_TONEMAPPING 8
#endif

#ifndef MAX_EFFECTS
#define MAX_EFFECTS 8
#endif

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerPassData : register(b1)
{
    float gGrayscaleIntensity;
    float gSpeiaIntensity;
    float gGrainIntensity;
    
    float gBlurIntensity;
    float gBlurDirections;
    float gBlurQuality;
    float gBlurSize;

    float gVignetteIntensity;
    float gVignetteOuter;
    float gVignetteInner;
	
    float gBlurVignetteIntensity;
    float gBlurVignetteDirections;
    float gBlurVignetteQuality;
    float gBlurVignetteSize;
    float gBlurVignetteOuter;
    float gBlurVignetteInner;
	
    float gBloomIntensity;
    float gBloomForce;
    float gBloomDirections;
    float gBloomQuality;
    float gBloomSize;

    float gToneMappingIntensity;
    uint gToneMappingTone;
    uint PAD11;
};

cbuffer cbPerPass : register(b2)
{
    uint gEffects[MAX_EFFECTS];
};

Texture2D gDiffuseMap : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

inline float3 LinearToneMapping(float3 color)
{
    float exposure = 1.;
    color = clamp(exposure * color, 0., 1.);
    color = pow(color, GAMMA_INVERSE.rrr);
    return color;
}
inline float3 SimpleReinhardToneMapping(float3 color)
{
    float exposure = 1.5;
    color *= exposure / (1. + color / exposure);
    color = pow(saturate(color), GAMMA_INVERSE.rrr);
    return color;
}
inline float3 LumaBasedReinhardToneMapping(float3 color)
{
    float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
    float toneMappedLuma = luma / (1. + luma);
    color *= toneMappedLuma / luma;
    color = pow(saturate(color), GAMMA_INVERSE.rrr);
    return color;
}
inline float3 WhitePreservingLumaBasedReinhardToneMapping(float3 color)
{
    float white = 2.;
    float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
    float toneMappedLuma = luma * (1. + luma / (white * white)) / (1. + luma);
    color *= toneMappedLuma / luma;
    color = pow(saturate(color), GAMMA_INVERSE.rrr);
    return color;
}
inline float3 RomBinDaHouseToneMapping(float3 color)
{
    color = exp(-1.0 / (2.72 * color + 0.15));
    color = pow(saturate(color), GAMMA_INVERSE.rrr);
    return color;
}
inline float3 FilmicToneMapping(float3 color)
{
    color = max(float3(0., 0., 0.), color - float3(0.004, 0.004, 0.004));
    color = (color * (6.2 * color + .5)) / (color * (6.2 * color + 1.7) + 0.06);
    return color;
}
inline float3 Uncharted2ToneMapping(float3 color)
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
    color = pow(saturate(color), GAMMA_INVERSE.rrr);
    return color;
}

inline float GetVignette(float vOutter, float vInner, float2 uv)
{
	// Center of Screen
    float2 center = float2(0.5, 0.5);
	// Distance  between center and the current Uv. Multiplyed by 1.414213 to fit in the range of 0.0 to 1.0 
    float dist = distance(center, uv) * 1.414213;
	// Generate the Vignette with Clamp which go from outer Viggnet ring to inner vignette ring with smooth steps
    return clamp((vOutter - dist) / (vOutter - vInner), 0.0, 1.0);
}
inline float3 CalcBlur(float2 uv, Texture2D gDiffuseMap, float blurSize)
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
inline float3 CalcGaussianBlur(float2 uv, Texture2D gDiffuseMap, float3 color, float Directions, float Quality, float2 Radius)
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
inline float CalcGrain(float2 uv, float iTime)
{
    return frac(sin(dot(uv, float2(17.0, 180.))) * 2500. + iTime);
}

inline float4 grayscale(float4 color, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    float greyScale = .5;
    float3 output = dot(color.rgb, greyScale.rrr).rrr;

    return float4(lerp(color.rgb, output, intensity), color.a);
}
inline float4 sepia(float4 color, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    float3 sepia = float3(1.2, 1.0, 0.8);
    float grey = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float3 output = grey.rrr * sepia;
    output = lerp(color.rgb, output, 0.75);

    return float4(lerp(color.rgb, output, intensity), color.a);
}
inline float4 vignette(float4 color, float2 uv, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    float vOutter = gVignetteOuter;
    float vInner = gVignetteInner;
    float vig = GetVignette(vOutter, vInner, uv);

	// Multiply the Vignette with the texture color
    float3 output = color.rgb * vig;
    output = lerp(color.rgb, output, intensity);

    return float4(output, color.a);
}
inline float4 blur(float4 color, float2 uv, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    float directions = gBlurDirections;
    float quality = gBlurQuality;
    float2 radius = gBlurSize / gPerFrame.ScreenResolution;
    float3 gau = CalcGaussianBlur(uv, gDiffuseMap, color.rgb, directions, quality, radius);

    float3 output = lerp(color.rgb, gau, intensity);

    return float4(output, color.a);
}
inline float4 blurVignette(float4 color, float2 uv, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    float directions = gBlurVignetteDirections;
    float quality = gBlurVignetteQuality;
    float2 radius = gBlurVignetteSize / gPerFrame.ScreenResolution;
    float vOutter = gBlurVignetteOuter;
    float vInner = gBlurVignetteInner;

    float3 gau = CalcGaussianBlur(uv, gDiffuseMap, color.rgb, directions, quality, radius);
    float vig = GetVignette(vOutter, vInner, uv);

	// Lerp Gaussian Blur and texture color by the Vignette value
    float3 output = lerp(gau, color.rgb, vig);
    output = lerp(color.rgb, output, intensity);

    return float4(output, color.a);
}
inline float4 bloom(float4 color, float2 uv, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    float force = gBloomForce;
    float directions = gBloomDirections;
    float quality = gBloomQuality;
    float2 radius = gBloomSize / gPerFrame.ScreenResolution;

    float3 blur = CalcGaussianBlur(uv, gDiffuseMap, color.rgb, directions, quality, radius);

	//Bloom intensity
    float3 output = blur * force + color.rgb;

    return float4(lerp(color.rgb, output, intensity), color.a);
}
inline float4 grain(float4 color, float2 uv, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    float3 grain = CalcGrain(uv, gPerFrame.TotalTime);

    float3 output = lerp(color.rgb, grain, .1);

    return float4(lerp(color.rgb, output, intensity), color.a);
}
inline float4 toneMapping(float4 color, float intensity)
{
    if (intensity <= 0)
    {
        return color;
    }
    
    uint toneMap = gToneMappingTone;

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

    return float4(lerp(color.rgb, output, intensity), color.a);
}

struct PSVertexEmpty
{
    float4 hpos : SV_Position;
    float2 uv : TEXCOORD0;
};

float4 main(PSVertexEmpty input) : SV_TARGET
{
    float4 color = gDiffuseMap.Sample(SamplerLinear, input.uv);

	[unroll]
    for (uint i = 0; i < MAX_EFFECTS; i++)
    {
        uint effect = gEffects[i];

        if (effect == EFFECT_EMPTY)
        {
            break;
        }
        
        if (effect == EFFECT_GRAYSCALE)
        {
            color = grayscale(color, gGrayscaleIntensity);
        }
        
        if (effect == EFFECT_SEPIA)
        {
            color = sepia(color, gSpeiaIntensity);
        }
        
        if (effect == EFFECT_VIGNETTE)
        {
            color = vignette(color, input.uv, gVignetteIntensity);
        }
        
        if (effect == EFFECT_BLUR)
        {
            color = blur(color, input.uv, gBlurIntensity);
        }
        
        if (effect == EFFECT_BLURVIGNETTE)
        {
            color = blurVignette(color, input.uv, gBlurVignetteIntensity);
        }
        
        if (effect == EFFECT_BLOOM)
        {
            color = bloom(color, input.uv, gBloomIntensity);
        }
        
        if (effect == EFFECT_GRAIN)
        {
            color = grain(color, input.uv, gGrainIntensity);
        }
        
        if (effect == EFFECT_TONEMAPPING)
        {
            color = toneMapping(color, gToneMappingIntensity);
        }
    }

    return color;
}
