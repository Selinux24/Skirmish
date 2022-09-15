#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
	PerFrame gPerFrame;
};

cbuffer cbDirectionals : register(b1)
{
	uint gDirLightsCount;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
};

cbuffer cbPerWater : register(b2)
{
	float gWaveHeight;
	float gWaveChoppy;
	float gWaveSpeed;
	float gWaveFrequency;

	float4 gWaterColor;

	float3 gBaseColor;
	float PAD21;

	uint gSteps;
	uint gGeometryIterations;
	uint gColorIterations;
	uint PAD22;
};

static const float2x2 octaveMatrix = float2x2(1.6, 1.2, -1.6, 1.2);
static const float waterShinniness = 60.0f;
static const float specularPassNRM = (waterShinniness + 8.0) / (PI * 8.0);
static const float positionEpsilonNRM = (0.1f / 10000.0f);

// Sea geometry
float Octave(float2 uv, float choppy)
{
	uv += Noise(uv);
	float2 wv = 1.0 - abs(sin(uv));
	float2 swv = abs(cos(uv));
	wv = lerp(wv, swv, wv);
	float v = 1.0 - pow(max(0, wv.x * wv.y), 0.65);
	return pow(max(0, v), choppy);
}
float Map(float3 position, float time, uint iterations)
{
	float freq = gWaveFrequency;
	float amp = gWaveHeight;
	float choppy = gWaveSpeed;
	float2 uv = position.xz;

	float h = 0.0;
	for (uint i = 0; i < iterations; i++)
	{
		float d = Octave((uv + time) * freq, choppy) + Octave((uv - time) * freq, choppy);

		h += d * amp;
		uv = mul(uv, octaveMatrix);
		freq *= 1.9;
		amp *= 0.22;
		choppy = lerp(choppy, 1.0, 0.2);
	}
	return position.y - h;
}
float3 GetNormal(float3 position, float epsilon, float time)
{
	float3 n;
	n.y = Map(position, time, gColorIterations);
	n.x = Map(position + float3(epsilon, 0, 0), time, gColorIterations) - n.y;
	n.z = Map(position + float3(0, 0, epsilon), time, gColorIterations) - n.y;
	n.y = epsilon;
	return normalize(n);
}
float3 HeightMapTracing(float3 eyePos, float3 eyeDir, float time)
{
	float3 p = 0;

	float tx = 1000.0;
	float hx = Map(eyePos + eyeDir * tx, time, gGeometryIterations);
	if (hx > 0.0)
	{
		return tx;
	}

	float tm = 0.0;
	float hm = Map(eyePos + eyeDir * tm, time, gGeometryIterations);
	float tmid = 0.0;
	for (uint i = 0; i < gSteps; i++)
	{
		tmid = lerp(tm, tx, hm / (hm - hx));
		p = eyePos + eyeDir * tmid;
		float hmid = Map(p, time, gGeometryIterations);
		if (hmid < 0.0)
		{
			tx = tmid;
			hx = hmid;
		}
		else
		{
			tm = tmid;
			hm = hmid;
		}
	}

	return p;
}

// Water lighting
void GetLightColor(DirectionalLight light, float3 normal, float3 eyeDir, out float3 diffusePass, out float3 specularPass)
{
	float3 lightDir = normalize(-light.DirToLight);

	Material mat = (Material)0;
	mat.Shininess = waterShinniness;

	diffusePass = DiffusePass(normal, -lightDir, light.Diffuse).rgb;
	specularPass = SpecularPassBlinnPhong(normal, eyeDir, lightDir, 0, mat).rgb;
}
float3 GetSkyColor(float3 eyeDir)
{
	eyeDir.y = max(eyeDir.y, 0.0);

	return float3(pow(1.0 - eyeDir.y, 2.0), 1.0 - eyeDir.y, 0.6 + (1.0 - eyeDir.y) * 0.4);
}
float3 GetSeaColor(float3 position, float3 normal, float3 eyeDir, float3 diffuse, float3 specular, float epsilon)
{
	float ambient = 0.3333;

	float3 refracted = (gBaseColor + pow(diffuse * 0.4 + float3(0.6, 0.6, 0.6), 80.0) * gWaterColor.rgb * 0.12) * clamp(ambient * 1.5f, 0.1, 1);
	float3 reflected = GetSkyColor(reflect(eyeDir, normal)) * clamp(ambient * 2, 0.1, 1);

	float attenuation = max(1.0 - epsilon, 0.0) * 0.18;
	float fresnel = clamp(1.0 - dot(normal, eyeDir), 0.0, 1.0);
	fresnel = pow(fresnel, 6.0) * 0.65;

	float3 color = lerp(refracted, reflected, fresnel);
	color += gWaterColor.rgb * (position.y - gWaveHeight) * attenuation;
	color += specular * specularPassNRM;

	return color;
}
float GetSeaAlpha(float distToEye, float alpha)
{
	float trDistance = (1 - alpha) * 500;

	return min(((distToEye / trDistance) * (1 - alpha)) + alpha, 1);
}

float4 main(PSVertexPosition input) : SV_TARGET
{
	float3 eyePos = gPerFrame.EyePosition;
	float3 eyeDir = eyePos - input.positionWorld;
	float distToEye = length(eyeDir);
	eyeDir /= distToEye;

	// Get the current time    
	float time = (1.0f + gPerFrame.TotalTime * gWaveSpeed);

	// Move position to origin level for tracing
	eyePos.y -= input.positionWorld.y;
	// Get the geometry (position and normal)
	float3 hmPosition = HeightMapTracing(eyePos, -eyeDir, time);
	float3 toPosition = hmPosition - eyePos;
	float epsilon = dot(toPosition, toPosition) * positionEpsilonNRM;
	float3 hmNormal = GetNormal(hmPosition, epsilon, time);

	float fog = 0;
	if (gPerFrame.FogRange > 0)
	{
		fog = CalcFogFactor(distToEye, gPerFrame.FogStart, gPerFrame.FogRange);
	}

	if (fog >= 1)
	{
		return float4(gPerFrame.FogColor.rgb, 1);
	}
	else
	{
		// Do light color
		float3 lDiffuse = 0;
		float3 lSpecular = 0;
		if (gDirLightsCount > 0)
		{
			for (uint i = 0; i < gDirLightsCount; i++)
			{
				float3 diffuse = 0;
				float3 specular = 0;
				GetLightColor(gDirLights[i], hmNormal, eyeDir, diffuse, specular);
				lDiffuse += diffuse;
				lSpecular += specular;
			}
		}

		// Do sea color
		float3 color = GetSeaColor(hmPosition, hmNormal, eyeDir, saturate(lDiffuse), lSpecular, epsilon);
		float alpha = GetSeaAlpha(distToEye, gWaterColor.a);

		return float4(lerp(color, gPerFrame.FogColor.rgb, fog), alpha);
	}
}
