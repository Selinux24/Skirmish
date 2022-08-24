#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

#ifndef CHANNEL_ALL
#define CHANNEL_ALL 	0
#endif
#ifndef CHANNEL_RED
#define CHANNEL_RED 	1
#endif
#ifndef CHANNEL_GREEN
#define CHANNEL_GREEN	2
#endif
#ifndef CHANNEL_BLUE
#define CHANNEL_BLUE	3
#endif
#ifndef CHANNEL_ALPHA
#define CHANNEL_ALPHA	4
#endif
#ifndef CHANNEL_NALPHA
#define CHANNEL_NALPHA	5
#endif

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float3 gEyePositionWorld;
	float PAD11;
	float4 gFogColor;
	float gFogStart;
	float gFogRange;
	float2 PAD12;
	float3 gLOD;
	float gShadowIntensity;
};

cbuffer cbPerFrame2 : register(b1)
{
	uint gChannel;
	uint3 PAD21;
};

Texture2DArray gDiffuseMapArray : register(t0);

SamplerState SamplerDiffuse : register(s0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
float4 main(PSVertexPositionTexture2 input) : SV_TARGET
{
	float4 color = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

	if(gChannel == CHANNEL_ALL)
	{
		color *= input.tintColor;
	}
	else if(gChannel == CHANNEL_RED)
	{
		//Grayscale of red channel
		color = float4(color.rrr, 1);
	}
	else if(gChannel == CHANNEL_GREEN)
	{
		//Grayscale of green channel
		color = float4(color.ggg, 1);
	}
	else if(gChannel == CHANNEL_BLUE)
	{
		//Grayscale of blue channel
		color = float4(color.bbb, 1);
	}
	else if(gChannel == CHANNEL_ALPHA)
	{
		//Grayscale of alpha channel
		color = float4(color.aaa, 1);
	}
	else if(gChannel == CHANNEL_NALPHA)
	{
		//Color channel
		color = float4(color.rgb, 1);
	}

	if (gFogRange > 0)
	{
		float distToEye = length(gEyePositionWorld - input.positionWorld);

		color = ComputeFog(color, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return color;
}
