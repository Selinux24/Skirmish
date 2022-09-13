#ifndef __SPRITES_INCLUDED__
#define __SPRITES_INCLUDED__

#include "IncBuiltIn.hlsl"
#include "IncHelpers.hlsl"

struct VSSpriteColor
{
    float3 positionLocal : POSITION;
    float4 color : COLOR0;
};
struct PSSpriteColor
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float4 color : COLOR0;
};

struct VSSpriteTexture
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
};
struct PSSpriteTexture
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
};

inline float MapScreenCoordX(float x, float4 rectPixels, float2 screenPixels)
{
    float p = 0.5 * x + 0.5;

    float left = rectPixels.x / screenPixels.x;
    float width = rectPixels.z / screenPixels.x;
    return clamp((p - left) / width, 0., 1.);
}
inline float MapScreenCoordY(float y, float4 rectPixels, float2 screenPixels)
{
    float p = 0.5 * -y + 0.5;

    float top = rectPixels.y / screenPixels.y;
    float height = rectPixels.w / screenPixels.y;
    return clamp((p - top) / height, 0., 1.);
}
inline float4 GetTintColor(float value, float3 pct, float4 color1, float4 color2, float4 color3, float4 color4)
{
    if (value <= pct.x)
        return color1;
    if (value <= pct.y)
        return color2;
    if (value <= pct.z)
        return color3;
    return color4;
}
inline float4 EvaluateRect(float2 uv, float4 color, float2 screen, float4 rect)
{
    float2 pixel = MapUVToScreenPixel(uv, screen);
    if (PixelIntoRectangle(pixel, rect))
    {
        return color;
    }

    return 0;
}

#endif
