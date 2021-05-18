#include "IncVertexFormats.hlsl"
#include "IncPostProcessing.hlsl"

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
