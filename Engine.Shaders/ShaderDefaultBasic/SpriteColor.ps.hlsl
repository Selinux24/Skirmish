#include "..\Lib\IncSprites.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerSprite : register(b1)
{
    float4x4 gLocal;
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

/**********************************************************************************************************
SPRITE COLOR
**********************************************************************************************************/
float4 main(PSSpriteColor input) : SV_TARGET
{
    float4 color = input.color;
    if (gUsePct)
    {
        float pct = gDirection == 0 ? MapScreenCoordX(input.positionWorld.x, gSize, gPerFrame.ScreenResolution) : MapScreenCoordY(input.positionWorld.y, gSize, gPerFrame.ScreenResolution);
        float4 tintColor = GetTintColor(pct, gPct, gColor1, gColor2, gColor3, gColor4);
 
        color = saturate(color * tintColor);
    }
    else 
    {
        color = saturate(color * gColor1);
    }
    
    color = GetChannel(color, gChannel);

    return EvaluateRect(input.positionWorld.xy, color, gPerFrame.ScreenResolution, gRectangle);
}
