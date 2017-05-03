#include "IncScattering.fx"
#include "IncLights.fx"
#include "IncVertexFormats.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorldViewProjection;
	float4 gSphereRadii;
	float4 gScatteringCoeffs;
	float4 gInvWaveLength;
	float4 gMisc;
	float4 gBackColor;
	float3 gLightDirection;
	float gHDRExposure;
};

PSVertexSkyScattering VSScattering(VSVertexPosition input)
{
	float4 colorM;
	float4 colorR;
	float3 rayPos;
	vertexPhase(
		input.positionLocal, gLightDirection, gBackColor,
		gSphereRadii, gScatteringCoeffs, gInvWaveLength, gMisc,
		colorM, colorR, rayPos);

	PSVertexSkyScattering output = (PSVertexSkyScattering) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = input.positionLocal;
	output.colorM = colorM;
	output.colorR = colorR;
	output.direction = rayPos;

	return output;
}

float4 PSScattering(PSVertexSkyScattering input) : SV_TARGET
{
	return HDR(pixelPhase(gLightDirection, input.direction, input.colorR, input.colorM), gHDRExposure);
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 SkyScattering
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSScattering()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSScattering()));
	}
}
