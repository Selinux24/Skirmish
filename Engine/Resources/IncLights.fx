
SamplerState SamplerPoint;
SamplerState SamplerLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};
SamplerState SamplerAnisotropic
{
	Filter = ANISOTROPIC;
	MaxAnisotropy = 4;

	AddressU = WRAP;
	AddressV = WRAP;
};

SamplerComparisonState SamplerComparisonShadow
{
	Filter   = COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
	AddressU = BORDER;
	AddressV = BORDER;
	AddressW = BORDER;
	BorderColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

    ComparisonFunc = LESS;
};

static const float3 GAMMA = float3(1.0f / 2.2f, 1.0f / 2.2f, 1.0f / 2.2f);

static const int MAX_LIGHTS_DIRECTIONAL = 3;
static const int MAX_LIGHTS_POINT = 4;
static const int MAX_LIGHTS_SPOT = 4;

static const float SHADOWMAPSIZE = 2048.0f;
static const float SHADOWMAPDX = 1.0f / SHADOWMAPSIZE;
static const float2 SamplerShadowOffsets[9] =
{
	float2(-SHADOWMAPDX, -SHADOWMAPDX),		float2(0.0f, -SHADOWMAPDX),		float2(SHADOWMAPDX, -SHADOWMAPDX),
	float2(-SHADOWMAPDX, 0.0f),				float2(0.0f, 0.0f),				float2(SHADOWMAPDX, 0.0f),
	float2(-SHADOWMAPDX, +SHADOWMAPDX),		float2(0.0f, +SHADOWMAPDX),		float2(SHADOWMAPDX, +SHADOWMAPDX)
};

struct Material
{
	float4 Diffuse;
	float SpecularIntensity;
	float SpecularPower;
};
struct DirectionalLight
{
	float3 Color;
	float Ambient;
	float Diffuse;
	float3 Direction;
	float CastShadow;
	float Enabled;
	float Pad1;
	float Pad2;
};
struct PointLight
{
	float3 Color;
	float Ambient;
	float Diffuse;
	float3 Position;
    float Radius;
	float CastShadow;
	float Enabled;
	float Pad1;
};
struct SpotLight
{
	float3 Color;
	float Ambient;
	float Diffuse;
	float3 Position;
	float3 Direction;
	float Angle;
    float Radius;
	float CastShadow;
	float Enabled;
	float Pad1;
};

float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW)
{
	//Uncompress each component from [0,1] to [-1,1].
	float3 normalT = 2.0f * normalMapSample - 1.0f;
	
	//Build orthonormal basis.
	float3 N = unitNormalW;
	float3 T = normalize(tangentW - dot(tangentW, N) * N);
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);
	
	// Transform from tangent space to world space.
	return normalize(mul(normalT, TBN));
}
float CalcSphericAttenuation(float radius, float intensity, float maxDistance, float distance)
{
    float f = distance / maxDistance;
    float denom = 1.0f - (f * f);
    if (denom > 0.0f)
    {
        float d = distance / (1.0f - (f * f));
        float dn = (d / radius) + 1.0f;
        
		return intensity / (dn * dn);
    }
    else
    {
        return 0.0f;
    }
}
float CalcShadowFactor(float4 shadowPosH, Texture2D shadowMap)
{
	// Complete projection by doing division by w.
	shadowPosH.xyz /= shadowPosH.w;

	// Depth in NDC space.
	float depth = shadowPosH.z;

	// 3×3 box filter pattern. Each sample does a 4-tap PCF.
	float percentLit = 0.0f;
	[unroll]
	for(int i = 0; i < 9; ++i)
	{
		percentLit += shadowMap.SampleCmpLevelZero(SamplerComparisonShadow, shadowPosH.xy + SamplerShadowOffsets[i], depth).r;
	}

	// Average the samples.
	return percentLit / 9.0f;
}
float4 ComputeFog(float4 litColor, float distToEye, float fogStart, float fogRange, float4 fogColor)
{
	float fogLerp = saturate((distToEye - fogStart) / fogRange);

	return lerp(litColor, fogColor, fogLerp);
}

float4 ComputeBaseLight(
	float3 lightColor,
	float lightAmbient,
	float lightDiffuse,
	float3 lightDirection,
	float3 toEye,
	float4 modelColor,
	float3 modelPosition,
	float3 modelNormal,
	float specularIntensity,
	float specularPower)
{                                                                                           
    float ambient = lightAmbient;
	float diffuse  = 0.0f;
	float specular = 0.0f;

	float diffuseFactor = max(0, dot(modelNormal, -lightDirection));
	[flatten]
    if (diffuseFactor > 0) 
	{
		diffuse = lightDiffuse * diffuseFactor;

        float3 lightReflection = normalize(reflect(lightDirection, modelNormal));
        
		float specularFactor = max(0, dot(toEye, lightReflection));
		[flatten]
		if (specularFactor > 0)
		{
			specularFactor = pow(specularFactor, max(1, specularPower));
            specular = specularIntensity * specularFactor;
        }
	}

    float3 litColor = modelColor.rgb * lightColor * (ambient + diffuse + specular);
	litColor = saturate(pow(litColor, GAMMA));

	return float4(litColor, modelColor.a);
}

float4 ComputeDirectionalLight(
	DirectionalLight L,
	float3 toEye,
	float4 color,
	float3 position,
	float3 normal,
	float specularIntensity,
	float specularPower,
	float4 shadowPosition,
	Texture2D shadowMap)
{
	float4 litColor = ComputeBaseLight(
		L.Color,
		L.Ambient,
		L.Diffuse,
		L.Direction,
		toEye,
		color,
		position,
		normal,
		specularIntensity,
		specularPower);

	[flatten]
	if(L.CastShadow == 1)
	{
		float shadowFactor = CalcShadowFactor(shadowPosition, shadowMap);

		litColor = float4(litColor.rgb * shadowFactor, litColor.a);
	}

	return litColor;
}

float4 ComputePointLight(
	PointLight L, 
	float3 toEye,
	float4 color,
	float3 position,
	float3 normal, 
	float specularIntensity,
	float specularPower)
{
    float3 lightDirection = position - L.Position;
	float distance = length(lightDirection);
	lightDirection /= distance;

	float4 litColor = ComputeBaseLight(
		L.Color,
		L.Ambient,
		L.Diffuse,
		lightDirection,
		toEye,
		color,
		position,
		normal,
		specularIntensity,
		specularPower);

    float attenuation = CalcSphericAttenuation(1, L.Diffuse, L.Radius, distance);

	return litColor * attenuation;
}

float4 ComputeSpotLight(
	SpotLight L,
	float3 toEye,
	float4 color,
	float3 position,
	float3 normal, 
	float specularIntensity,
	float specularPower)
{
	float4 litColor = 0;

	float3 lightDirection = position - L.Position;
	float distance = length(lightDirection);
	lightDirection /= distance;

	float spotFactor = dot(lightDirection, L.Direction);
	float lightToSurfaceAngle = degrees(acos(spotFactor));
	[flatten]
	if(lightToSurfaceAngle <= L.Angle)
	{
		litColor = ComputeBaseLight(
			L.Color,
			L.Ambient,
			L.Diffuse,
			lightDirection,
			toEye,
			color,
			position,
			normal,
			specularIntensity,
			specularPower);

		float attenuationD = CalcSphericAttenuation(1, L.Diffuse, L.Radius, distance);

		float attenuationS =  (1.0f - (1.0f - lightToSurfaceAngle) * 1.0f / (1.0f - L.Angle));

		litColor *= attenuationD * attenuationS;
	}
	
	return litColor;
}

float4 ComputeAllLights(
	DirectionalLight dirLights[MAX_LIGHTS_DIRECTIONAL],
	PointLight pointLights[MAX_LIGHTS_POINT],
	SpotLight spotLights[MAX_LIGHTS_SPOT],
	float3 toEye,
	float4 color,
	float3 position,
	float3 normal,
	float specularIntensity,
	float specularPower,
	float4 shadowPosition,
	Texture2D shadowMap)
{
	float4 litColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

	int i;

	[unroll]
	for(i = 0; i < MAX_LIGHTS_DIRECTIONAL; i++)
	{
		if(dirLights[i].Enabled == 1.0f)
		{
			litColor += ComputeDirectionalLight(
				dirLights[i],
				toEye,
				color,
				position,
				normal,
				specularIntensity,
				specularPower,
				shadowPosition,
				shadowMap);
		}
	}

	[unroll]
	for(i = 0; i < MAX_LIGHTS_POINT; i++)
	{
		if(pointLights[i].Enabled == 1.0f)
		{
			litColor += ComputePointLight(
				pointLights[i],
				toEye,
				color,
				position,
				normal,
				specularIntensity,
				specularPower);
		}
	}

	[unroll]
	for(i = 0; i < MAX_LIGHTS_SPOT; i++)
	{
		if(spotLights[i].Enabled == 1.0f)
		{
			litColor += ComputeSpotLight(
				spotLights[i],
				toEye,
				color,
				position,
				normal,
				specularIntensity,
				specularPower);
		}
	}

	return litColor;
}


