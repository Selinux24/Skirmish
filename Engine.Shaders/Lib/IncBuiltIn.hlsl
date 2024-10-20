#ifndef __BUILTIN_INCLUDED__
#define __BUILTIN_INCLUDED__

struct Globals
{
    uint MaterialPaletteWidth;
    uint AnimationPaletteWidth;
    uint2 PAD0;
};

struct PerFrame
{
    float4x4 ViewProjection;
    
    float4x4 OrthoViewProjection;
    
    float3 EyePosition;
    float PAD0;
    
    float2 ScreenResolution;
    float TotalTime;
    float ElapsedTime;

    float3 LOD;
    float ShadowIntensity;
    
    float4 FogColor;
    
    float FogStart;
    float FogRange;
    float2 PAD1;
};

#endif
