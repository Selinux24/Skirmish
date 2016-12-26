#include "IncVertexFormats.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float4 gSphereRadii;
	float4 gScatteringCoeffs;
	float4 gInvWaveLength;
	float4 gMisc;
	float4 gBackColor;
	float3 gLightDirection;
};

static const uint gSamples = 2;

float vernierScale(float fCos)
{
	float x = 1.0 - fCos;
	float x5 = x * 5.25;
	float x5p6 = (-6.80 + x5);
	float xnew = (3.83 + x * x5p6);
	float xfinal = (0.459 + x * xnew);
	float xfinal2 = -0.00287 + x * xfinal;
	float outx = exp( xfinal2 ); 
	return 0.25 * outx;
}
float mieScale(float3 lightDirection, float3 viewDirection)
{
	float fCos = dot(lightDirection, viewDirection) / length(viewDirection);
	float fCos2 = fCos * fCos;
    
	float g = -0.991f;
	float g2 = -0.991f * -0.991f;

	return (1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(abs(1.0 + g2 - 2.0*g*fCos), 1.5));
}

PSVertexSkyScattering VSScattering(VSVertexPosition input)
{
	float3 positionLocal = mul(float4(input.positionLocal, 1), gWorld).xyz;

	float earthRadius = gMisc.x;
	float atmosphereRadius = gMisc.y;
	float scale = gMisc.z;
	float scaleOverScaleDepth = gMisc.w;
   
	float outerRadius = gSphereRadii.x;
	float outerRadiusSqr = gSphereRadii.y;
	float innerRadius = gSphereRadii.z;
	float innerRadiusSqr = gSphereRadii.w;
   
	float rayleighBrightness = gScatteringCoeffs.x;
	float rayleigh4PI = gScatteringCoeffs.y;
	float mieBrightness = gScatteringCoeffs.z;
	float mie4PI = gScatteringCoeffs.w;

	// Get the ray from the camera to the vertex, and its length (which is the far point of the ray passing through the atmosphere).
	float3 rayDir = input.positionLocal; //Assuming one radius sphere in 0 coordinate center
	float rayLen = atmosphereRadius / earthRadius;
	float3 rayPos = -rayDir * rayLen;
	float3 rayStart = float3(0, 1, 0);

	float3 color = gBackColor.rgb;
	float4 colorM = gBackColor;
	float4 colorR = gBackColor;

	if(input.positionLocal.y >= 0)
	{
		// Calculate ray's scattering offset.
		float scatteringOffset = (exp(scaleOverScaleDepth * (innerRadius - 1))) * vernierScale(dot(rayDir, rayStart));

		// Initialize the scattering loop variables.
		float sampleLength = rayLen * 0.5f;
		float scaledLength = sampleLength * scale;
		float3 sampleRay = rayDir * sampleLength;
		float3 samplePoint = rayStart + sampleRay * 0.5;

		// Now loop through the sample rays
		for(uint i = 0; i < gSamples; i++)
		{
			float sampleHeight = length(samplePoint);
			float sampleDepth = exp(scaleOverScaleDepth * (innerRadius - sampleHeight));
			float lightAngle = dot(gLightDirection, samplePoint) / sampleHeight;
			float cameraAngle = dot(rayDir, samplePoint) / sampleHeight;

			float scaleFromCamera = vernierScale(cameraAngle);
			float scaleFromLight = vernierScale(lightAngle);

			float scatterFactor = -(scatteringOffset + sampleDepth * (scaleFromLight - scaleFromCamera));
			float3 attenuation = 0;
			attenuation.x = exp(scatterFactor * (gInvWaveLength.x * rayleigh4PI + mie4PI));
			attenuation.y = exp(scatterFactor * (gInvWaveLength.y * rayleigh4PI + mie4PI));
			attenuation.z = exp(scatterFactor * (gInvWaveLength.z * rayleigh4PI + mie4PI));

			color += attenuation * (sampleDepth * scaledLength);

			samplePoint += sampleRay;
		}

		color = saturate(color);
		colorM = float4(color * mieBrightness, 1.0f);
		colorR = float4(color * (gInvWaveLength.xyz * rayleighBrightness), 1.0f);
	}

    PSVertexSkyScattering output = (PSVertexSkyScattering)0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = positionLocal;
	output.colorM = colorM;
	output.colorR = colorR;
	output.direction = rayPos;

	return output;
}

float4 PSScattering(PSVertexSkyScattering input) : SV_TARGET
{
	float fMiePhase = mieScale(gLightDirection, input.direction);
   
	float4 color = input.colorR + fMiePhase * input.colorM;
	color.a = color.b;
  
	return color;
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
