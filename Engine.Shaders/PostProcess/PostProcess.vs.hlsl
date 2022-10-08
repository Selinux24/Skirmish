#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerFrame : register(b0)
{
	PerFrame gPerFrame;
};

struct VSVertexPositionTexture
{
	float3 positionLocal : POSITION;
	float2 tex : TEXCOORD0;
};

struct PSVertexEmpty
{
	float4 hpos : SV_Position;
	float2 uv : TEXCOORD0;
};

PSVertexEmpty main(VSVertexPositionTexture input)
{
	PSVertexEmpty output;

	output.hpos = mul(float4(input.positionLocal, 1), gPerFrame.OrthoViewProjection);
	output.uv = input.tex;

	return output;
}
