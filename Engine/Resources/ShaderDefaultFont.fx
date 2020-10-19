#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"

SamplerState SamplerText
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
	AddressW = WRAP;
	MipLODBias = 0;
	MaxAnisotropy = 1;
	ComparisonFunc = ALWAYS;
	BorderColor = float4(0, 0, 0, 0);
	MinLOD = 0;
    MaxLOD = FLOAT_MAX;
};

cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
    float gAlpha;
    bool gUseColor;
};

Texture2D gTexture : register(t0);

PSVertexFont VSFont(VSVertexFont input)
{
    PSVertexFont output = (PSVertexFont) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
    output.color = input.color;
    
	return output;
}

float4 PSFont(PSVertexFont input) : SV_TARGET
{
    float4 litColor = gTexture.Sample(SamplerText, input.tex);

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
        litColor.rgb = input.color.rgb;
        litColor.a *= input.color.a * gAlpha;
    }

    return saturate(litColor);
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