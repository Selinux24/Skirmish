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

Texture2DArray gTextureArray : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

/**********************************************************************************************************
SPRITE TEXTURE
**********************************************************************************************************/
float4 main(PSSpriteTexture input) : SV_TARGET
{
    float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, gTextureIndex));
    if (gUsePct)
    {
        float pct = gDirection == 0 ? input.tex.x : input.tex.y;
        float4 tintColor = GetTintColor(pct, gPct, gColor1, gColor2, gColor3, gColor4);

        color = saturate(color * tintColor);
    }
    else
    {
        color = saturate(color * gColor1);
    }

    color = GetChannel(color, gChannel);

    if (gUseRect)
    {
        return EvaluateRect(input.positionWorld.xy, color, gPerFrame.ScreenResolution, gRectangle);
    }
    
    return color;
}
