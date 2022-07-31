#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPSGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	uint3 PAD01;
};
Texture2D gMaterialPalette : register(t0);

cbuffer cbPSPerFrame : register(b1)
{
	float3 gEyePositionWorld;
	float PAD11;
	float4 gFogColor;
	float gFogStart;
	float gFogRange;
	float2 PAD12;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
float4 main(PSVertexPositionColor input) : SV_TARGET
{
	Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);

	float4 matColor = input.color * material.Diffuse;

	if (gFogRange > 0)
	{
		float distToEye = length(gEyePositionWorld - input.positionWorld);

		matColor = ComputeFog(matColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return matColor;
}
