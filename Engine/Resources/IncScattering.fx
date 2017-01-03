
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

void vertexPhase(
	float3 positionLocal, float3 lightDirection, float4 backColor, 
	float4 sphereRadii, float4 scatteringCoeffs, float4 invWaveLength, float4 misc,
	out float4 colorM, out float4 colorR, out float3 rayPos)
{
	float earthRadius = misc.x;
	float atmosphereRadius = misc.y;
	float scale = misc.z;
	float scaleOverScaleDepth = misc.w;
   
	float outerRadius = sphereRadii.x;
	float outerRadiusSqr = sphereRadii.y;
	float innerRadius = sphereRadii.z;
	float innerRadiusSqr = sphereRadii.w;
   
	float rayleighBrightness = scatteringCoeffs.x;
	float rayleigh4PI = scatteringCoeffs.y;
	float mieBrightness = scatteringCoeffs.z;
	float mie4PI = scatteringCoeffs.w;

	// Get the ray from the camera to the vertex, and its length (which is the far point of the ray passing through the atmosphere).
	float3 rayDir = positionLocal; //Assuming one radius sphere in 0 coordinate center
	float rayLen = atmosphereRadius / earthRadius;
	float3 rayStart = float3(0, 1, 0);
	rayPos = -rayDir * rayLen;

	float3 color = backColor.rgb;
	colorM = backColor;
	colorR = backColor;

	if(positionLocal.y >= -0.1f)
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
			float lightAngle = dot(lightDirection, samplePoint) / sampleHeight;
			float cameraAngle = dot(rayDir, samplePoint) / sampleHeight;

			float scaleFromCamera = vernierScale(cameraAngle);
			float scaleFromLight = vernierScale(lightAngle);

			float scatterFactor = -(scatteringOffset + sampleDepth * (scaleFromLight - scaleFromCamera));
			float3 attenuation = 0;
			attenuation.x = exp(scatterFactor * (invWaveLength.x * rayleigh4PI + mie4PI));
			attenuation.y = exp(scatterFactor * (invWaveLength.y * rayleigh4PI + mie4PI));
			attenuation.z = exp(scatterFactor * (invWaveLength.z * rayleigh4PI + mie4PI));

			color += attenuation * (sampleDepth * scaledLength);

			samplePoint += sampleRay;
		}

		color = saturate(color);
		colorM = float4(color * mieBrightness, 1.0f);
		colorR = float4(color * (invWaveLength.xyz * rayleighBrightness), 1.0f);
	}
}

float4 pixelPhase(
	float3 lightDirection, float3 viewDirection, float4 colorR, float4 colorM)
{
	float fMiePhase = mieScale(lightDirection, viewDirection);
   
	float4 color = colorR + fMiePhase * colorM;
	color.a = color.b;

	return color;
}