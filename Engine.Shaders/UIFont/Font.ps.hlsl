#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerFont : register(b1)
{
    float gAlpha;
    bool gUseColor;
    bool gUseRect;
    bool gFineSampling;
    
    float4 gRectangle;
};

Texture2D gTexture : register(t0);

SamplerState SamplerPointText : register(s0)
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

SamplerState SamplerLinearText : register(s1)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float4 MapFont(float4 litColor, float4 color)
{

    if (litColor.r == 0.0f)
    {
        litColor.a = 0.0f;
    }
    else if (gUseColor == true)
    {
        litColor.a *= gAlpha;
    }
    else
    {
        litColor.rgb = color.rgb;
        litColor.a *= color.a * gAlpha;
    }

    return saturate(litColor);
}

float4 main(PSVertexFont input) : SV_TARGET
{
    float4 litColor = gFineSampling ?
		gTexture.Sample(SamplerLinearText, input.tex) :
		gTexture.Sample(SamplerPointText, input.tex);
	
    if (!gUseRect)
    {
        return MapFont(litColor, input.color);
    }

    float2 pixel = MapUVToScreenPixel(input.positionWorld.xy, gPerFrame.ScreenResolution);
    if (PixelIntoRectangle(pixel, gRectangle))
    {
        return MapFont(litColor, input.color);
    }

    return 0;
}
