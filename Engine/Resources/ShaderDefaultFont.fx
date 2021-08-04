#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

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
	float2 gResolution;
	float4 gRectangle;
	bool gUseRect;
};

Texture2D gTexture : register(t0);

PSVertexFont VSFont(VSVertexFont input)
{
	PSVertexFont output = (PSVertexFont)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = output.positionHomogeneous.xyz;
	output.tex = input.tex;
	output.color = input.color;

	return output;
}

float4 MapFont(float4 litColor, float4 color) {

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

float2 MapScreenCoord(float2 positionWorld) {

	float2 p = 0.5 * float2(positionWorld.x, -positionWorld.y) + 0.5f;

	return float2(gResolution.x * p.x, gResolution.y * p.y);
}

bool CoordIntoRectangle(float2 coord) {
	return (
		coord.x >= gRectangle.x &&
		coord.x <= gRectangle.z &&
		coord.y >= gRectangle.x + gRectangle.y &&
		coord.y <= gRectangle.z + gRectangle.w);
}

float4 PSFont(PSVertexFont input) : SV_TARGET
{
	if (!gUseRect)
	{
		float4 litColor = gTexture.Sample(SamplerText, input.tex);

		return MapFont(litColor, input.color);
	}

	float2 coord = MapScreenCoord(input.positionWorld.xy);
	if (CoordIntoRectangle(coord))
	{
		float4 litColor = gTexture.Sample(SamplerText, input.tex);

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