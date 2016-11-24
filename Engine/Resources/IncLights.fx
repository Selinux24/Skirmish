
SamplerState SamplerPoint
{
	Filter = MIN_MAG_MIP_POINT;
};
SamplerState SamplerPointParticle
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};
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
SamplerComparisonState SamplerComparisonLessEqual
{
	Filter = COMPARISON_MIN_MAG_MIP_LINEAR;
	AddressU = MIRROR;
	AddressV = MIRROR;

	ComparisonFunc = LESS_EQUAL;
};

float roll(float rnd, float min, float max)
{
   return min + (rnd * (max - min));
}

float RandomScalar(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0).x;
}
float2 RandomVector2(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0).xy;
}
float3 RandomVector3(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0).xyz;
}
float4 RandomVector4(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0);
}

float RandomScalar(float min, float max, float seed, Texture1D rndTex)
{
	float r = rndTex.SampleLevel(SamplerLinear, seed, 0).x;
	r = roll(r, min, max);

	return r;
}
float2 RandomVector2(float min, float max, float seed, Texture1D rndTex)
{
	float2 r = rndTex.SampleLevel(SamplerLinear, seed, 0).xy;
	r.x = roll(r.x, min, max);
	r.y = roll(r.y, min, max);

	return r;
}
float3 RandomVector3(float min, float max, float seed, Texture1D rndTex)
{
	float3 r = rndTex.SampleLevel(SamplerLinear, seed, 0).xyz;
	r.x = roll(r.x, min, max);
	r.y = roll(r.y, min, max);
	r.z = roll(r.z, min, max);

	return r;
}
float4 RandomVector4(float min, float max, float seed, Texture1D rndTex)
{
	float4 r = rndTex.SampleLevel(SamplerLinear, seed, 0);
	r.x = roll(r.x, min, max);
	r.y = roll(r.y, min, max);
	r.z = roll(r.z, min, max);
	r.w = roll(r.w, min, max);

	return r;
}

static const int MAX_LIGHTS_DIRECTIONAL = 3;
static const int MAX_LIGHTS_POINT = 16;
static const int MAX_LIGHTS_SPOT = 16;

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

static const uint sampleCount = 16;
static float2 poissonDisk[sampleCount] =
{
	float2(0.2770745f, 0.6951455f),
	float2(0.1874257f, -0.02561589f),
	float2(-0.3381929f, 0.8713168f),
	float2(0.5867746f, 0.1087471f),
	float2(-0.3078699f, 0.188545f),
	float2(0.7993396f, 0.4595091f),
	float2(-0.09242552f, 0.5260149f),
	float2(0.3657553f, -0.5329605f),
	float2(-0.3829718f, -0.2476171f),
	float2(-0.01085108f, -0.6966301f),
	float2(0.8404155f, -0.3543923f),
	float2(-0.5186161f, -0.7624033f),
	float2(-0.8135794f, 0.2328489f),
	float2(-0.784665f, -0.2434929f),
	float2(0.9920505f, 0.0855163f),
	float2(-0.687256f, 0.6711345f)
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
float CalcShadowFactor(float4 lightPosition, uint shadows, Texture2D shadowMapStatic, Texture2D shadowMapDynamic)
{
	float shadow = 0.0f;

    float2 tex = 0.0f;
    tex.x = (+lightPosition.x / lightPosition.w * 0.5f) + 0.5f;
    tex.y = (-lightPosition.y / lightPosition.w * 0.5f) + 0.5f;
	float z = (lightPosition.z / lightPosition.w) -  0.001f;

	for (uint i = 0; i < sampleCount; i++)
	{
		float2 stc = tex + poissonDisk[i] / 700.0f;

		if(shadows == 1)
		{
			if (!shadowMapStatic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z))
			{
				shadow += 0.8f;
			}
		}
		if(shadows == 2)
		{
			if (!shadowMapDynamic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z))
			{
				shadow += 0.8f;
			}
		}
		if(shadows == 3)
		{
			if (!shadowMapStatic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z) ||
				!shadowMapDynamic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z))
			{
				shadow += 0.8f;
			}
		}
	}

    return 1.0f - (shadow / sampleCount);
}
float3 ComputeFog(float3 litColor, float distToEye, float fogStart, float fogRange, float3 fogColor)
{
	float fogLerp = saturate((distToEye - fogStart) / fogRange);

	return lerp(litColor, fogColor, fogLerp);
}

float3 ComputeBaseLight(
	float3 lightColor,
	float lightAmbient,
	float lightDiffuse,
	float3 lightDirection,
	float3 toEye,
	float3 modelColor,
	float3 modelPosition,
	float3 modelNormal,
	float specularIntensity,
	float specularPower,
	float shadowFactor)
{                                                                                           
    float ambient = lightAmbient;
	float diffuse  = 0.0f;
	float specular = 0.0f;

	float diffuseFactor = max(0, dot(modelNormal, -lightDirection));
	[flatten]
    if (diffuseFactor > 0) 
	{
		diffuse = lightDiffuse * diffuseFactor * shadowFactor;

        float3 lightReflection = normalize(reflect(lightDirection, modelNormal));
        
		float specularFactor = max(0, dot(toEye, lightReflection));
		[flatten]
		if (specularFactor > 0)
		{
			specularFactor = pow(specularFactor, max(1, specularPower));
            specular = specularIntensity * specularFactor;
        }
	}

    float3 litColor = modelColor * lightColor * (ambient + diffuse + specular);

	return litColor;
}

float3 ComputeDirectionalLight(
	DirectionalLight L,
	float3 toEye,
	float3 color,
	float3 position,
	float3 normal,
	float specularIntensity,
	float specularPower,
	float4 lightPosition,
	uint shadows,
	Texture2D shadowMapStatic,
	Texture2D shadowMapDynamic)
{
	float shadowFactor = 1.0f;

	[flatten]
	if(L.CastShadow == 1)
	{
		shadowFactor = CalcShadowFactor(lightPosition, shadows, shadowMapStatic, shadowMapDynamic);
	}

	float3 litColor = ComputeBaseLight(
		L.Color,
		L.Ambient,
		L.Diffuse,
		L.Direction,
		toEye,
		color,
		position,
		normal,
		specularIntensity,
		specularPower,
		shadowFactor);

	return litColor;
}

float3 ComputePointLight(
	PointLight L, 
	float3 toEye,
	float3 color,
	float3 position,
	float3 normal, 
	float specularIntensity,
	float specularPower)
{
    float3 lightDirection = position - L.Position;
	float distance = length(lightDirection);
	lightDirection /= distance;

	float3 litColor = ComputeBaseLight(
		L.Color,
		L.Ambient,
		L.Diffuse,
		lightDirection,
		toEye,
		color,
		position,
		normal,
		specularIntensity,
		specularPower,
		1.0f);

    float attenuation = CalcSphericAttenuation(1, L.Diffuse, L.Radius, distance);

	return litColor * attenuation;
}

float3 ComputeSpotLight(
	SpotLight L,
	float3 toEye,
	float3 color,
	float3 position,
	float3 normal, 
	float specularIntensity,
	float specularPower)
{
	float3 litColor = 0;

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
			specularPower,
			1.0f);

		float attenuationD = CalcSphericAttenuation(1, L.Diffuse, L.Radius, distance);

		float attenuationS =  (1.0f - (1.0f - lightToSurfaceAngle) * 1.0f / (1.0f - L.Angle));

		litColor *= attenuationD * attenuationS;
	}
	
	return litColor;
}

float3 ComputeAllLights(
	DirectionalLight dirLights[MAX_LIGHTS_DIRECTIONAL],
	PointLight pointLights[MAX_LIGHTS_POINT],
	SpotLight spotLights[MAX_LIGHTS_SPOT],
	float3 toEye,
	float3 color,
	float3 position,
	float3 normal,
	float specularIntensity,
	float specularPower,
	float4 lightPosition,
	uint shadows,
	Texture2D shadowMapStatic,
	Texture2D shadowMapDynamic)
{
	float3 litColor = 0;

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
				lightPosition,
				shadows,
				shadowMapStatic,
				shadowMapDynamic);
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


