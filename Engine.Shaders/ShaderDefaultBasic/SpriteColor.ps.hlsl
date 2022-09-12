#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerObject : register(b1)
{
    float4 gSize;
    float4 gColor1;
    float4 gColor2;
    float4 gColor3;
    float4 gColor4;
    bool gUsePct;
    float3 gPct;
    uint gDirection;
    uint gChannel;
    uint gTextureIndex;
    bool gUseRect;
    float4 gRectangle;
};

float MapScreenCoordX(float x, float4 rectPixels, float2 screenPixels)
{
    float p = 0.5 * x + 0.5;
	
    float left = rectPixels.x / screenPixels.x;
    float width = rectPixels.z / screenPixels.x;
    return clamp((p - left) / width, 0., 1.);
}
float MapScreenCoordY(float y, float4 rectPixels, float2 screenPixels)
{
    float p = 0.5 * -y + 0.5;

    float top = rectPixels.y / screenPixels.y;
    float height = rectPixels.w / screenPixels.y;
    return clamp((p - top) / height, 0., 1.);
}
float4 GetTintColor(float value)
{
    if (value <= gPct.x)
        return gColor1;
    if (value <= gPct.y)
        return gColor2;
    if (value <= gPct.z)
        return gColor3;
    return gColor4;
}
float4 EvaluateRect(float2 uv, float4 color)
{
    if (!gUseRect)
    {
        return color;
    }
    
    float2 pixel = MapUVToScreenPixel(uv, gPerFrame.ScreenResolution);
    if (PixelIntoRectangle(pixel, gRectangle))
    {
        return color;
    }
	
    return 0;
}

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/

float4 main(PSSpriteColor input) : SV_TARGET
{
    if (gUsePct)
    {
        float pct = gDirection == 0 ? MapScreenCoordX(input.positionWorld.x, gSize, gPerFrame.ScreenResolution) : MapScreenCoordY(input.positionWorld.y, gSize, gPerFrame.ScreenResolution);
        float4 tintColor = GetTintColor(pct);
 
        return EvaluateRect(input.positionWorld.xy, saturate(input.color * tintColor));
    }
    
    return EvaluateRect(input.positionWorld.xy, saturate(input.color * gColor1));
}
