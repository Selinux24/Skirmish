#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncHelpers.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
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
float4 main(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));
	color *= input.tintColor;

	color = GetChannel(color, gChannel);

	return color;
}
