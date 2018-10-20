#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
Texture2D gCloudTexture1 : register(t0);
Texture2D gCloudTexture2 : register(t1);

cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorldViewProjection;
    float3 gColor;
	float gBrightness;
	float gFadingDistance;
	float3 PAD1;
};
cbuffer cbPerFrameStatic : register(b1)
{
	float2 gFirstTranslation;
	float2 gSecondTranslation;
};
cbuffer cbPerFramePerturbed : register(b2)
{
	float gTranslation;
	float gScale;
	float2 PAD2;
};

PSVertexPositionTexture VSClouds(VSVertexPositionTexture input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = input.positionLocal;
	output.tex = input.tex;
	output.textureIndex = 0;

	return output;
}

float4 PSClouds(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color1 = gCloudTexture1.Sample(SamplerAnisotropic, input.tex + gFirstTranslation);
	float4 color2 = gCloudTexture2.Sample(SamplerAnisotropic, input.tex + gSecondTranslation);

    float4 color = lerp(color1, color2, 0.5f) * float4(gColor, 1) * gBrightness;

	if (gFadingDistance > 0)
	{
		color.a = saturate(1 - (length(input.positionWorld) / gFadingDistance));
	}

	return color;
}

float4 PSClouds2(PSVertexPositionTexture input) : SV_TARGET
{
	input.tex.x += gTranslation;

	// Sample the texture value from the perturb texture using the translated texture coordinates.
	float4 perturbValue = gCloudTexture1.Sample(SamplerAnisotropic, input.tex) * gScale;

	// Add the texture coordinates as well as the translation value to get the perturbed texture coordinate sampling location.
	perturbValue.xy += (input.tex.xy + gTranslation);

	// Now sample the color from the cloud texture using the perturbed sampling coordinates.
    float4 color = gCloudTexture2.Sample(SamplerAnisotropic, perturbValue.xy) * float4(gColor, 1) * gBrightness;

	if (gFadingDistance > 0)
	{
		color.a = saturate(1 - (length(input.positionWorld) / gFadingDistance));
	}

	return color;
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 CloudsStatic
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSClouds()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSClouds()));
	}
}

technique11 CloudsPerturbed
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSClouds()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSClouds2()));
	}
}
