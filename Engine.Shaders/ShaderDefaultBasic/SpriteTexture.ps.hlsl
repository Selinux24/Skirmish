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

Texture2DArray gTextureArray : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
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

float4 main(PSSpriteTexture input) : SV_TARGET
{
    float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, gTextureIndex));
    if (gChannel == 1)
    {
        color = float4(color.rrr, 1);
    }
    else if (gChannel == 2)
    {
        color = float4(color.ggg, 1);
    }
    else if (gChannel == 3)
    {
        color = float4(color.bbb, 1);
    }
    else if (gChannel == 4)
    {
        color = float4(color.aaa, 1);
    }
    else if (gChannel == 5)
    {
        color = float4(color.rgb, 1);
    }
    else if (gChannel == 6)
    {
        color = float4(color.rgb, 1);
    }
    
    if (gUsePct)
    {
        float pct = gDirection == 0 ? input.tex.x : input.tex.y;
        float4 tintColor = GetTintColor(pct);
    
        return EvaluateRect(input.positionWorld.xy, saturate(color * tintColor));
    }
	
    return EvaluateRect(input.positionWorld.xy, saturate(color * gColor1));
}
