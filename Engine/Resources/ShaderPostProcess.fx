#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"
#include "IncPostProcessing.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorldViewProjection;
    float2 gTextureSize;
    float2 gPad1;
    
    //Blur parameters
    float gBlurDirections;
    float gBlurQuality;
    float gBlurSize;
    float gPad2;
    
    //Blur vignette
    float gVignetteOuter;
    float gVignetteInner;
    float2 gPad3;

    //Blur bloom
    float gBloomIntensity;
    float gBloomBlurSize;
    float2 gPad4;
    
    //Tone mapping
    uint gToneMappingTone;
    uint3 gPad5;
};

struct PSVertexEmpty
{
    float4 hpos : SV_Position;
    float2 uv : TEXCOORD0;
};

Texture2D gDiffuseMap : register(t0);

PSVertexEmpty VSEmpty(VSVertexPositionTexture input)
{
    PSVertexEmpty output;

    output.hpos = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.uv = input.tex;

    return output;
}

float4 PSEmpty(PSVertexEmpty input) : SV_TARGET
{
    float4 output = gDiffuseMap.Sample(SamplerLinear, input.uv);
    output.a = 1;
    return output;
}
float4 PSGrayscale(PSVertexEmpty input) : SV_TARGET
{
    float3 color = gDiffuseMap.Sample(SamplerLinear, input.uv).rgb;
    float3 greyScale = float3(.5, .5, .5);
    float d = dot(color, greyScale);
    return float4(d, d, d, 1);
}
float4 PSSepia(PSVertexEmpty input) : SV_TARGET
{
    float3 color = gDiffuseMap.Sample(SamplerLinear, input.uv).rgb;
    float3 sepia = float3(1.2, 1.0, 0.8);
    float grey = dot(color, float3(0.299, 0.587, 0.114));
    float3 sepiaColour = float3(grey, grey, grey) * sepia;
    
    color = lerp(color, sepiaColour, 0.75);
    
    return float4(color, 1);
}
float4 PSBlur(PSVertexEmpty input) : SV_TARGET
{
    float Directions = gBlurDirections;
    float Quality = gBlurQuality;
    float Size = gBlurSize;
    
    float2 Radius = Size / gTextureSize;
    
    float4 output = gDiffuseMap.Sample(SamplerLinear, input.uv);
    
    // Gaussian blur calculations
    for (float d = 0.0; d < TWO_PI; d += TWO_PI / Directions)
    {
        for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
        {
            float2 uv = input.uv + float2(cos(d), sin(d)) * Radius * i;
            output += gDiffuseMap.Sample(SamplerLinear, uv);
        }
    }
    
    // Output to screen
    output /= Quality * Directions - 15.0;
    output.a = 1;
    
    return output;
}
float4 PSVignette(PSVertexEmpty input) : SV_TARGET
{
    float vOutter = gVignetteOuter;
    float vInner = gVignetteInner;
    
    float4 output = gDiffuseMap.Sample(SamplerLinear, input.uv);
    output.a = 1;
    
    // Center of Screen
    float2 center = float2(0.5, 0.5);
    // Distance  between center and the current Uv. Multiplyed by 1.414213 to fit in the range of 0.0 to 1.0 
    float dist = distance(center, input.uv) * 1.414213;
	// Generate the Vignette with Clamp which go from outer Viggnet ring to inner vignette ring with smooth steps
    float vig = clamp((vOutter - dist) / (vOutter - vInner), 0.0, 1.0);
	// Multiply the Vignette with the texture color
    output *= vig;
    
    return output;
}
float4 PSBloom(PSVertexEmpty input) : SV_TARGET
{
    float intensity = gBloomIntensity;
    float blurSize = gBloomBlurSize;
    
    float4 sum = float4(0, 0, 0, 0);
    
    // Blur
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x - 4.0 * blurSize, input.uv.y)) * 0.05;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x - 3.0 * blurSize, input.uv.y)) * 0.09;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x - 2.0 * blurSize, input.uv.y)) * 0.12;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x - blurSize, input.uv.y)) * 0.15;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y)) * 0.16;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x + blurSize, input.uv.y)) * 0.15;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x + 2.0 * blurSize, input.uv.y)) * 0.12;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x + 3.0 * blurSize, input.uv.y)) * 0.09;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x + 4.0 * blurSize, input.uv.y)) * 0.05;
    
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y - 4.0 * blurSize)) * 0.05;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y - 3.0 * blurSize)) * 0.09;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y - 2.0 * blurSize)) * 0.12;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y - blurSize)) * 0.15;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y)) * 0.16;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y + blurSize)) * 0.15;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y + 2.0 * blurSize)) * 0.12;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y + 3.0 * blurSize)) * 0.09;
    sum += gDiffuseMap.Sample(SamplerLinear, float2(input.uv.x, input.uv.y + 4.0 * blurSize)) * 0.05;
    
    //Bloom intensity
    float4 output = sum * intensity + gDiffuseMap.Sample(SamplerLinear, input.uv);
    output.a = 1;
    
    return output;
}
float4 PSToneMapping(PSVertexEmpty input) : SV_TARGET
{
    uint toneMap = gToneMappingTone;
    
    float3 color = gDiffuseMap.Sample(SamplerLinear, input.uv).rgb;
    
    if (toneMap == 1)
        color = LinearToneMapping(color);
    if (toneMap == 2)
        color = SimpleReinhardToneMapping(color);
    if (toneMap == 3)
        color = LumaBasedReinhardToneMapping(color);
    if (toneMap == 4)
        color = WhitePreservingLumaBasedReinhardToneMapping(color);
    if (toneMap == 5)
        color = RomBinDaHouseToneMapping(color);
    if (toneMap == 6)
        color = FilmicToneMapping(color);
    if (toneMap == 7)
        color = Uncharted2ToneMapping(color);
        
    return float4(color, 1.);
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
technique11 Blur
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSBlur()));
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
technique11 Bloom
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSEmpty()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSBloom()));
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
