#define GAMMA_INVERSE 1.0/2.2

float3 inverseGamma = float3(GAMMA_INVERSE, GAMMA_INVERSE, GAMMA_INVERSE);

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
    color = pow(color, inverseGamma);
    return color;
}

float3 LumaBasedReinhardToneMapping(float3 color)
{
    float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
    float toneMappedLuma = luma / (1. + luma);
    color *= toneMappedLuma / luma;
    color = pow(color, inverseGamma);
    return color;
}

float3 WhitePreservingLumaBasedReinhardToneMapping(float3 color)
{
    float white = 2.;
    float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
    float toneMappedLuma = luma * (1. + luma / (white * white)) / (1. + luma);
    color *= toneMappedLuma / luma;
    color = pow(color, inverseGamma);
    return color;
}

float3 RomBinDaHouseToneMapping(float3 color)
{
    color = exp(-1.0 / (2.72 * color + 0.15));
    color = pow(color, inverseGamma);
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
    color = pow(color, inverseGamma);
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