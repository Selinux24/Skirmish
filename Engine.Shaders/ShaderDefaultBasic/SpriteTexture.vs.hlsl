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
POSITION TEXTURE
**********************************************************************************************************/
PSSpriteTexture main(VSSpriteTexture input)
{
    PSSpriteTexture output = (PSSpriteTexture) 0;

    float4x4 wvp = mul(gLocal, gPerFrame.OrthoViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = output.positionHomogeneous.xyz;
    output.tex = input.tex;
    
    return output;
}
