
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

float2 EncodeColor(float3 rgb24)
{
	// scale up to 8-bit
	rgb24 *= 255.0f;

	// remove the 3 LSB of red and blue, and the 2 LSB of green
	int3 rgb16 = rgb24 / int3(8, 4, 8);

	// split the green at bit 3 (we'll keep the 6 bits around the split)
	float greenSplit = rgb16.g / 8.0f;

	// pack it up (capital G's are MSB, the rest are LSB)
	float2 packed;
	packed.x = rgb16.r * 8 + floor(greenSplit);		// rrrrrGGG
	packed.y = frac(greenSplit) * 256 + rgb16.b;		// gggbbbbb

														// scale down and return
	packed /= 255.0f;
	return packed;
}
float3 DecodeColor(float2 packed) {
	// scale up to 8-bit
	packed *= 255.0f;

	// round and split the packed bits
	float2 split = round(packed) / 8;	// first component at bit 3
	split.y /= 4;				// second component at bit 5

								// unpack (obfuscated yet optimized crap follows)
	float3 rgb16 = 0.0f.rrr;
	rgb16.gb = frac(split) * 256;
	rgb16.rg += floor(split) * 4;
	rgb16.r *= 2;

	// scale down and return
	rgb16 /= 255.0f;
	return rgb16;
}

inline float roll(float rnd, float min, float max)
{
	return min + (rnd * (max - min));
}

inline float RandomScalar(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0).x;
}
inline float2 RandomVector2(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0).xy;
}
inline float3 RandomVector3(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0).xyz;
}
inline float4 RandomVector4(float seed, Texture1D rndTex)
{
	return rndTex.SampleLevel(SamplerLinear, seed, 0);
}

inline float RandomScalar(float min, float max, float seed, Texture1D rndTex)
{
	float r = rndTex.SampleLevel(SamplerLinear, seed, 0).x;
	r = roll(r, min, max);

	return r;
}
inline float2 RandomVector2(float min, float max, float seed, Texture1D rndTex)
{
	float2 r = rndTex.SampleLevel(SamplerLinear, seed, 0).xy;
	r.x = roll(r.x, min, max);
	r.y = roll(r.y, min, max);

	return r;
}
inline float3 RandomVector3(float min, float max, float seed, Texture1D rndTex)
{
	float3 r = rndTex.SampleLevel(SamplerLinear, seed, 0).xyz;
	r.x = roll(r.x, min, max);
	r.y = roll(r.y, min, max);
	r.z = roll(r.z, min, max);

	return r;
}
inline float4 RandomVector4(float min, float max, float seed, Texture1D rndTex)
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
	float4 Emissive;
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float Shininess;
};
struct DirectionalLight
{
	float4 Diffuse;
	float4 Specular;
	float3 Direction;
	float CastShadow;
};
struct PointLight
{
	float4 Diffuse;
	float4 Specular;
	float3 Position;
	float Intensity;
	float Radius;
	float Pad1;
	float Pad2;
	float Pad3;
};
struct SpotLight
{
	float4 Diffuse;
	float4 Specular;
	float3 Position;
	float Angle;
	float3 Direction;
	float Intensity;
	float Radius;
	float Pad1;
	float Pad2;
	float Pad3;
};

inline Material GetMaterialData(Texture2D materialsTexture, uint materialIndex, uint paletteWidth)
{
	uint baseIndex = 4 * materialIndex;
	uint baseU = baseIndex % paletteWidth;
	uint baseV = baseIndex / paletteWidth;

	float4 mat1 = materialsTexture.Load(uint3(baseU, baseV, 0));
	float4 mat2 = materialsTexture.Load(uint3(baseU + 1, baseV, 0));
	float4 mat3 = materialsTexture.Load(uint3(baseU + 2, baseV, 0));
	float4 mat4 = materialsTexture.Load(uint3(baseU + 3, baseV, 0));

	Material mat;

	mat.Emissive = mat1;
	mat.Ambient = mat2;
	mat.Diffuse = mat3;
	mat.Specular = float4(mat4.xyz, 1.0f);
	mat.Shininess = mat4.w;

	return mat;
}

static const uint MaxSampleCount = 16;

static float2 poissonDisk[MaxSampleCount] =
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

inline float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 normalW, float3 tangentW)
{
	//Uncompress each component from [0,1] to [-1,1].
	float3 normalT = (2.0f * normalMapSample) - 1.0f;

	float3 binormalW = cross(normalW, tangentW);

	return normalize((normalT.x * tangentW) + (normalT.y * binormalW) + (normalT.z * normalW));
}
inline float CalcShadowFactor(float4 lightPosition, uint shadows, Texture2D shadowMapStatic, Texture2D shadowMapDynamic)
{
	uint samples = 16;
	float factor = 0.8f;
	float bias = 0.0001f;
	float poissonFactor = 3500.0f;

	float2 tex = 0.0f;
	tex.x = (+lightPosition.x / lightPosition.w * 0.5f) + 0.5f;
	tex.y = (-lightPosition.y / lightPosition.w * 0.5f) + 0.5f;
	float z = (lightPosition.z / lightPosition.w) - bias;

	float shadow = 0.0f;

	for (uint i = 0; i < samples; i++)
	{
		float2 stc = tex + poissonDisk[i] / poissonFactor;

		[flatten]
		if (shadows == 1)
		{
			if (!shadowMapStatic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z))
			{
				shadow += factor;
			}
		}
		[flatten]
		if (shadows == 2)
		{
			if (!shadowMapDynamic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z))
			{
				shadow += factor;
			}
		}
		[flatten]
		if (shadows == 3)
		{
			if (!shadowMapStatic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z) ||
				!shadowMapDynamic.SampleCmpLevelZero(SamplerComparisonLessEqual, stc, z))
			{
				shadow += factor;
			}
		}
	}

	return 1.0f - (shadow / samples);
}
inline float4 ComputeFog(float4 litColor, float distToEye, float fogStart, float fogRange, float4 fogColor)
{
	float fogLerp = saturate((distToEye - fogStart) / fogRange);

	return lerp(litColor, fogColor, fogLerp);
}

inline void Phong(float4 lDiffuse, float4 lSpecular, float lShininess, float3 L, float3 N, float3 V, float3 R, out float4 diffuse, out float4 specular)
{
	diffuse = (max(0, dot(L, N))) * lDiffuse;
	specular = (pow(max(0, dot(R, V)), lShininess)) * lSpecular;
}
inline void BlinnPhong(float4 lDiffuse, float4 lSpecular, float lShininess, float3 L, float3 N, float3 V, float3 R, out float4 diffuse, out float4 specular)
{
	diffuse = (max(0, dot(L, N))) * lDiffuse;
	specular = (pow(max(0, dot(N, normalize(L + V))), lShininess)) * lSpecular;
}

inline float CalcSphericAttenuation(float intensity, float radius, float distance)
{
	float attenuation = 0.0f;

	float f = distance / radius;
	float denom = max(1.0f - (f * f), 0.0f);
	if (denom > 0.0f)
	{
		float d = distance / (1.0f - (f * f));
		float dn = (d / intensity) + 1.0f;

		attenuation = 1.0f / (dn * dn);
	}

	return attenuation;
}
inline float CalcSpotCone(float3 lightDirection, float spotAngle, float3 L)
{
	float minCos = cos(spotAngle);
	float maxCos = (minCos + 1.0f) * 0.5f;
	float cosAngle = dot(lightDirection, -L);
	return smoothstep(minCos, maxCos, cosAngle);
}

inline void ComputeDirectionalLight(
	DirectionalLight dirLight,
	float shininess,
	float3 pPosition,
	float3 pNormal,
	float3 ePosition,
	float4 sLightPosition,
	uint shadows,
	Texture2D shadowMapStatic,
	Texture2D shadowMapDynamic,
	out float4 diffuse,
	out float4 specular)
{
	float3 L = normalize(-dirLight.Direction);
	float3 V = normalize(ePosition - pPosition);
	float3 R = 2 * dot(L, pNormal) * pNormal - L;

	float cShadowFactor = 1;
	[flatten]
	if (dirLight.CastShadow == 1)
	{
		cShadowFactor = CalcShadowFactor(sLightPosition, shadows, shadowMapStatic, shadowMapDynamic);
	}

	BlinnPhong(dirLight.Diffuse, dirLight.Specular, shininess, L, pNormal, V, R, diffuse, specular);

	diffuse *= cShadowFactor;
	specular *= cShadowFactor;
}

inline void ComputePointLight(
	PointLight pointLight,
	float shininess,
	float3 pPosition,
	float3 pNormal,
	float3 ePosition,
	out float4 diffuse,
	out float4 specular)
{
	float D = length(pointLight.Position - pPosition);
	float3 L = normalize(pointLight.Position - pPosition);
	float3 V = normalize(ePosition - pPosition);
	float3 R = 2 * dot(L, pNormal) * pNormal - L;

	BlinnPhong(pointLight.Diffuse, pointLight.Specular, shininess, L, pNormal, V, R, diffuse, specular);

	float attenuation = CalcSphericAttenuation(pointLight.Intensity, pointLight.Radius, D);

	diffuse *= attenuation;
	specular *= attenuation;
}

inline void ComputeSpotLight(
	SpotLight spotLight,
	float shininess,
	float3 pPosition,
	float3 pNormal,
	float3 ePosition,
	out float4 diffuse,
	out float4 specular)
{
	float3 L = spotLight.Position - pPosition;
	float D = length(L);
	L /= D;
	float3 V = normalize(ePosition - pPosition);
	float3 R = 2 * dot(L, pNormal) * pNormal - L;

	BlinnPhong(spotLight.Diffuse, spotLight.Specular, shininess, L, pNormal, V, R, diffuse, specular);

	float attenuation = CalcSphericAttenuation(spotLight.Intensity, spotLight.Radius, D);
	attenuation *= CalcSpotCone(spotLight.Direction, spotLight.Angle, L);

	diffuse *= attenuation;
	specular *= attenuation;
}

inline float4 ComputeLights(
	float4 Ga,
	DirectionalLight dirLights[MAX_LIGHTS_DIRECTIONAL],
	PointLight pointLights[MAX_LIGHTS_POINT],
	SpotLight spotLights[MAX_LIGHTS_SPOT],
	uint dirLightsCount,
	uint pointLightsCount,
	uint spotLightsCount,
	float fogStart,
	float fogRange,
	float4 fogColor,
	Material k,
	float3 pPosition,
	float3 pNormal,
	float4 pColorDiffuse,
	float4 pColorSpecular,
	bool useColorDiffuse,
	bool useColorSpecular,
	float3 ePosition,
	float4 sLightPosition,
	uint shadows,
	Texture2D shadowMapStatic,
	Texture2D shadowMapDynamic)
{
	float4 lDiffuse = 0;
	float4 lSpecular = 0;

	float3 L = 0;
	float3 V = 0;
	float3 R = 0;

	float4 cDiffuse = 0;
	float4 cSpecular = 0;
	float cShadowFactor = 0;

	float D = 0;
	float S = 0;
	float attenuation = 0;

	uint i = 0;

	for (i = 0; i < dirLightsCount; i++)
	{
		L = normalize(-dirLights[i].Direction);
		V = normalize(ePosition - pPosition);
		R = 2 * dot(L, pNormal) * pNormal - L;

		cShadowFactor = dirLights[i].CastShadow * CalcShadowFactor(sLightPosition, shadows, shadowMapStatic, shadowMapDynamic);

		BlinnPhong(dirLights[i].Diffuse, dirLights[i].Specular, k.Shininess, L, pNormal, V, R, cDiffuse, cSpecular);
		lDiffuse += cDiffuse * cShadowFactor;
		lSpecular += cSpecular * cShadowFactor;
	}

	for (i = 0; i < pointLightsCount; i++)
	{
		D = length(pointLights[i].Position - pPosition);
		L = normalize(pointLights[i].Position - pPosition);
		V = normalize(ePosition - pPosition);
		R = 2 * dot(L, pNormal) * pNormal - L;

		BlinnPhong(pointLights[i].Diffuse, pointLights[i].Specular, k.Shininess, L, pNormal, V, R, cDiffuse, cSpecular);

		attenuation = CalcSphericAttenuation(pointLights[i].Intensity, pointLights[i].Radius, D);

		lDiffuse += (cDiffuse * attenuation);
		lSpecular += (cSpecular * attenuation);
	}

	for (i = 0; i < spotLightsCount; i++)
	{
		D = length(spotLights[i].Position - pPosition);
		L = normalize(spotLights[i].Position - pPosition);
		V = normalize(ePosition - pPosition);
		R = 2 * dot(L, pNormal) * pNormal - L;

		BlinnPhong(spotLights[i].Diffuse, spotLights[i].Specular, k.Shininess, L, pNormal, V, R, cDiffuse, cSpecular);

		attenuation = CalcSphericAttenuation(spotLights[i].Intensity, spotLights[i].Radius, D);
		attenuation *= CalcSpotCone(spotLights[i].Direction, spotLights[i].Angle, L);

		lDiffuse += (cDiffuse * attenuation);
		lSpecular += (cSpecular * attenuation);
	}

	float4 emissive = k.Emissive;
	float4 ambient = k.Ambient * Ga;

	float4 diffuse = k.Diffuse * lDiffuse;
	float4 specular = k.Specular * lSpecular * (useColorSpecular == true ? pColorSpecular : 1);

	float4 color = (emissive + ambient + diffuse + specular) * (useColorDiffuse == true ? pColorDiffuse : 1);

	if (fogRange > 0)
	{
		float distToEye = length(ePosition - pPosition);

		color = ComputeFog(color, distToEye, fogStart, fogRange, fogColor);
	}

	return saturate(color);
}
