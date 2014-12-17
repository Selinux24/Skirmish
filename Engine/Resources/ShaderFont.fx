#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float4 gColor;
};

Texture2D gTexture;

PSVertexPositionTexture VSFont(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
    
    return output;
}

float4 PSFont(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTexture.Sample(samFont, input.tex);

	if(litColor.a != 0.0f)
	{
		litColor.a = 1.0f;
		litColor = litColor * gColor;
	}

	return litColor;
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