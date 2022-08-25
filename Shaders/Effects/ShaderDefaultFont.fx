#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

SamplerState SamplerPointText
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

SamplerState SamplerLinearText
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorld;
    float4x4 gWorldViewProjection;
    float gAlpha;
    bool gUseColor;
    float2 gResolution;
    float4 gRectangle;
    bool gUseRect;
    bool gFineSampling;
};

Texture2D gTexture : register(t0);

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

PSVertexFont VSFont(VSVertexFont input)
{
    PSVertexFont output = (PSVertexFont) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = output.positionHomogeneous.xyz;
    output.tex = input.tex;
    output.color = input.color;

    return output;
}

float4 PSFont(PSVertexFont input) : SV_TARGET
{
    float4 litColor = gFineSampling ?
		gTexture.Sample(SamplerLinearText, input.tex) :
		gTexture.Sample(SamplerPointText, input.tex);
	
    if (!gUseRect)
    {
        return MapFont(litColor, input.color);
    }

    float2 pixel = MapUVToScreenPixel(input.positionWorld.xy, gResolution);
    if (PixelIntoRectangle(pixel, gRectangle))
    {
        return MapFont(litColor, input.color);
    }

    return 0;
}

technique11 FontDrawer
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSFont()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSFont()));
    }
}