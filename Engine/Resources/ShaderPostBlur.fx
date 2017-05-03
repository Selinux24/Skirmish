#include "IncLights.fx"
#include "IncVertexFormats.fx"

struct PSVertexBlur
{
	float4 hpos : SV_Position;

	float2 uv0 : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
	float2 uv3 : TEXCOORD3;

	float2 uv4 : TEXCOORD4;
	float2 uv5 : TEXCOORD5;
	float2 uv6 : TEXCOORD6;
	float2 uv7 : TEXCOORD7;
};

cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorldViewProjection;
	float2 gBlurDirection;
	float2 gPad1;
	float2 gTextureSize;
	float2 gPad2;
};

Texture2D gDiffuseMap : register(t0);

PSVertexBlur VSBlur(VSVertexPositionTexture input)
{
	PSVertexBlur output;

	output.hpos = mul(float4(input.positionLocal, 1), gWorldViewProjection);

	float2 uv = input.tex + (0.5f / gTextureSize);

	output.uv0 = uv + ((gBlurDirection * 3.5f) / gTextureSize);
	output.uv1 = uv + ((gBlurDirection * 2.5f) / gTextureSize);
	output.uv2 = uv + ((gBlurDirection * 1.5f) / gTextureSize);
	output.uv3 = uv + ((gBlurDirection * 0.5f) / gTextureSize);

	output.uv4 = uv + ((gBlurDirection * 3.5f) / gTextureSize);
	output.uv5 = uv + ((gBlurDirection * 2.5f) / gTextureSize);
	output.uv6 = uv + ((gBlurDirection * 1.5f) / gTextureSize);
	output.uv7 = uv + ((gBlurDirection * 0.5f) / gTextureSize);

	return output;
}

float4 PSBlur(PSVertexBlur IN) : SV_TARGET
{
	float4 kernel = float4(0.175f, 0.275f, 0.375f, 0.475f) * 0.5f;
	float3 rgb2lum = float3(0.30f, 0.59f, 0.11f);

	float4 output = 0;

	output += gDiffuseMap.Sample(SamplerLinear, IN.uv0) * kernel.x;
	output += gDiffuseMap.Sample(SamplerLinear, IN.uv1) * kernel.y;
	output += gDiffuseMap.Sample(SamplerLinear, IN.uv2) * kernel.z;
	output += gDiffuseMap.Sample(SamplerLinear, IN.uv3) * kernel.w;

	output += gDiffuseMap.Sample(SamplerLinear, IN.uv4) * kernel.x;
	output += gDiffuseMap.Sample(SamplerLinear, IN.uv5) * kernel.y;
	output += gDiffuseMap.Sample(SamplerLinear, IN.uv6) * kernel.z;
	output += gDiffuseMap.Sample(SamplerLinear, IN.uv7) * kernel.w;

	output.a = dot(output.rgb, rgb2lum);

	return output;
}

technique11 Blur
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSBlur()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSBlur()));
	}
}