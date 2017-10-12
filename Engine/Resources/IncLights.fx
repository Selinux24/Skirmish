
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
	AddressU = BORDER;
	AddressV = BORDER;
	BorderColor = float4(1, 1, 1, 1);

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
	packed.x = rgb16.r * 8 + floor(greenSplit); // rrrrrGGG
	packed.y = frac(greenSplit) * 256 + rgb16.b; // gggbbbbb

	// scale down and return
	packed /= 255.0f;

	return packed;
}
float3 DecodeColor(float2 packed)
{
	// scale up to 8-bit
	packed *= 255.0f;

	// round and split the packed bits
	float2 split = round(packed) / 8; // first component at bit 3
	split.y /= 4; // second component at bit 5

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

	return roll(r, min, max);
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
	float3 Pad;
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
	float3 Pad;
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
inline float CalcShadowFactor(uint shadows, float4 lightPositionLD, float4 lightPositionHD, Texture2D shadowMapLD, Texture2D shadowMapHD)
{
	uint samples = 16;
	float factor = 0.8f;
	float bias = 0.0001f;
	float poissonFactor = 3500.0f;

	float2 texL = 0.0f;
	texL.x = (+lightPositionLD.x / lightPositionLD.w * 0.5f) + 0.5f;
	texL.y = (-lightPositionLD.y / lightPositionLD.w * 0.5f) + 0.5f;
	float zL = (lightPositionLD.z / lightPositionLD.w) - bias;

	float2 texH = 0.0f;
	texH.x = (+lightPositionHD.x / lightPositionHD.w * 0.5f) + 0.5f;
	texH.y = (-lightPositionHD.y / lightPositionHD.w * 0.5f) + 0.5f;
	float zH = (lightPositionHD.z / lightPositionHD.w) - bias;

	float shadow = 0.0f;

	for (uint i = 0; i < samples; i++)
	{
		float2 stcL = texL + poissonDisk[i] / poissonFactor;
		float2 stcH = texH + poissonDisk[i] / poissonFactor;

		[flatten]
		if (shadows == 1)
		{
			if (!shadowMapLD.SampleCmpLevelZero(SamplerComparisonLessEqual, stcL, zL))
			{
				shadow += factor;
			}
		}
		[flatten]
		if (shadows == 2)
		{
			if (!shadowMapHD.SampleCmpLevelZero(SamplerComparisonLessEqual, stcH, zH))
			{
				shadow += factor;
			}
		}
		[flatten]
		if (shadows == 3)
		{
			if (!shadowMapHD.SampleCmpLevelZero(SamplerComparisonLessEqual, stcH, zH) ||
				!shadowMapLD.SampleCmpLevelZero(SamplerComparisonLessEqual, stcL, zL))
			{
				shadow += factor;
			}
		}
	}

	return 1.0f - (shadow / samples);
}

inline float CalcFogFactor(float distToEye, float fogStart, float fogRange)
{
	return saturate((distToEye - fogStart) / fogRange);
}
inline float4 ComputeFog(float4 litColor, float distToEye, float fogStart, float fogRange, float4 fogColor)
{
	float fogLerp = saturate((distToEye - fogStart) / fogRange);

	return float4(lerp(litColor.rgb, fogColor.rgb, fogLerp), litColor.a);
}

inline float4 DiffusePass(float4 lDiffuse, float3 L, float3 N)
{
	return (max(0, dot(L, N))) * lDiffuse;
}
inline float4 SpecularPhongPass(float4 lSpecular, float lShininess, float3 V, float3 R)
{
	return (pow(max(0, dot(R, V)), lShininess)) * lSpecular;
}
inline float4 SpecularBlinnPhongPass(float4 lSpecular, float lShininess, float3 L, float3 N, float3 V)
{
	return (pow(max(0, dot(N, normalize(L + V))), lShininess)) * lSpecular;
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

inline float4 HDR(float4 color, float exposure)
{
	float4 hdrColor = float4(color.rgb * exposure, color.a);

	hdrColor.r = hdrColor.r < 1.413f ? pow(abs(hdrColor.r) * 0.38317f, 1.0f / 2.2f) : 1.0f - exp(-hdrColor.r);
	hdrColor.g = hdrColor.g < 1.413f ? pow(abs(hdrColor.g) * 0.38317f, 1.0f / 2.2f) : 1.0f - exp(-hdrColor.g);
	hdrColor.b = hdrColor.b < 1.413f ? pow(abs(hdrColor.b) * 0.38317f, 1.0f / 2.2f) : 1.0f - exp(-hdrColor.b);

	return hdrColor;
}

struct ComputeLightsOutput
{
	float4 diffuse;
	float4 specular;
};

struct ComputeDirectionalLightsInput
{
	DirectionalLight dirLight;
	float3 lod;
	float shininess;
	float3 pPosition;
	float3 pNormal;
	float3 ePosition;
	float4 sLightPositionLD;
	float4 sLightPositionHD;
	uint shadows;
	Texture2D shadowMapLD;
	Texture2D shadowMapHD;
};

inline ComputeLightsOutput ComputeDirectionalLightLOD1(ComputeDirectionalLightsInput input)
{
	float3 L = normalize(-input.dirLight.Direction);
	float3 V = normalize(input.ePosition - input.pPosition);
	float3 R = 2 * dot(L, input.pNormal) * input.pNormal - L;

	float cShadowFactor = 1;
	[flatten]
    if (input.dirLight.CastShadow == 1 && input.shadows > 0)
	{
		cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
	}

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.dirLight.Diffuse, L, input.pNormal) * cShadowFactor;
	output.specular = SpecularBlinnPhongPass(input.dirLight.Specular, input.shininess, L, input.pNormal, V) * cShadowFactor;

	return output;
}
inline ComputeLightsOutput ComputeDirectionalLightLOD2(ComputeDirectionalLightsInput input)
{
	float3 L = normalize(-input.dirLight.Direction);

	float cShadowFactor = 1;
	[flatten]
    if (input.dirLight.CastShadow == 1 && input.shadows > 0)
	{
		cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
	}

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.dirLight.Diffuse, L, input.pNormal) * cShadowFactor;
	output.specular = 0;

	return output;
}
inline ComputeLightsOutput ComputeDirectionalLightLOD3(ComputeDirectionalLightsInput input)
{
	float3 L = normalize(-input.dirLight.Direction);

	float cShadowFactor = 1;
	[flatten]
    if (input.dirLight.CastShadow == 1 && input.shadows > 0)
	{
		cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
	}

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.dirLight.Diffuse, L, input.pNormal) * cShadowFactor;
	output.specular = 0;

	return output;
}
inline ComputeLightsOutput ComputeDirectionalLightLOD4(ComputeDirectionalLightsInput input)
{
	float3 L = normalize(-input.dirLight.Direction);

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.dirLight.Diffuse, L, input.pNormal);
	output.specular = 0;

	return output;
}
inline ComputeLightsOutput ComputeDirectionalLight(ComputeDirectionalLightsInput input)
{
	float distToEye = length(input.ePosition - input.pPosition);

	if (distToEye < input.lod.x)
	{
		return ComputeDirectionalLightLOD1(input);
	}
	else if (distToEye < input.lod.y)
	{
		return ComputeDirectionalLightLOD2(input);
	}
	else if (distToEye < input.lod.z)
	{
		return ComputeDirectionalLightLOD3(input);
	}
	else
	{
		return ComputeDirectionalLightLOD4(input);
	}
}

struct ComputePointLightsInput
{
	PointLight pointLight;
	float3 lod;
	float shininess;
	float3 pPosition;
	float3 pNormal;
	float3 ePosition;
};

inline ComputeLightsOutput ComputePointLightLOD1(ComputePointLightsInput input)
{
	float3 L = input.pointLight.Position - input.pPosition;
	float D = length(L);
	L /= D;
	float3 V = normalize(input.ePosition - input.pPosition);
	float3 R = 2 * dot(L, input.pNormal) * input.pNormal - L;

	float attenuation = CalcSphericAttenuation(input.pointLight.Intensity, input.pointLight.Radius, D);

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.pointLight.Diffuse, L, input.pNormal) * attenuation;
	output.specular = SpecularBlinnPhongPass(input.pointLight.Specular, input.shininess, L, input.pNormal, V) * attenuation;

	return output;
}
inline ComputeLightsOutput ComputePointLightLOD2(ComputePointLightsInput input)
{
	float3 L = input.pointLight.Position - input.pPosition;
	float D = length(L);
	L /= D;

	float attenuation = CalcSphericAttenuation(input.pointLight.Intensity, input.pointLight.Radius, D);

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.pointLight.Diffuse, L, input.pNormal) * attenuation;
	output.specular = 0;

	return output;
}
inline ComputeLightsOutput ComputePointLight(ComputePointLightsInput input)
{
	float distToEye = length(input.ePosition - input.pPosition);

	if (distToEye < input.lod.x)
	{
		return ComputePointLightLOD1(input);
	}
	else if (distToEye < input.lod.z)
	{
		return ComputePointLightLOD2(input);
	}
	else
	{
		ComputeLightsOutput output;
		output.diffuse = 0;
		output.specular = 0;
		return output;
	}
}

struct ComputeSpotLightsInput
{
	SpotLight spotLight;
	float3 lod;
	float shininess;
	float3 pPosition;
	float3 pNormal;
	float3 ePosition;
};

inline ComputeLightsOutput ComputeSpotLightLOD1(ComputeSpotLightsInput input)
{
	float3 L = input.spotLight.Position - input.pPosition;
	float D = length(L);
	L /= D;
	float3 V = normalize(input.ePosition - input.pPosition);
	float3 R = 2 * dot(L, input.pNormal) * input.pNormal - L;

	float attenuation = CalcSphericAttenuation(input.spotLight.Intensity, input.spotLight.Radius, D);
	attenuation *= CalcSpotCone(input.spotLight.Direction, input.spotLight.Angle, L);

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.spotLight.Diffuse, L, input.pNormal) * attenuation;
	output.specular = SpecularBlinnPhongPass(input.spotLight.Specular, input.shininess, L, input.pNormal, V) * attenuation;

	return output;
}
inline ComputeLightsOutput ComputeSpotLightLOD2(ComputeSpotLightsInput input)
{
	float3 L = input.spotLight.Position - input.pPosition;
	float D = length(L);
	L /= D;

	float attenuation = CalcSphericAttenuation(input.spotLight.Intensity, input.spotLight.Radius, D);
	attenuation *= CalcSpotCone(input.spotLight.Direction, input.spotLight.Angle, L);

	ComputeLightsOutput output;

	output.diffuse = DiffusePass(input.spotLight.Diffuse, L, input.pNormal) * attenuation;
	output.specular = 0;

	return output;
}
inline ComputeLightsOutput ComputeSpotLight(ComputeSpotLightsInput input)
{
	float distToEye = length(input.ePosition - input.pPosition);

	if (distToEye < input.lod.x)
	{
		return ComputeSpotLightLOD1(input);
	}
	else if (distToEye < input.lod.z)
	{
		return ComputeSpotLightLOD2(input);
	}
	else
	{
		ComputeLightsOutput output;
		output.diffuse = 0;
		output.specular = 0;
		return output;
	}
}

struct ComputeLightsInput
{
	float4 Ga;
	DirectionalLight dirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight pointLights[MAX_LIGHTS_POINT];
	SpotLight spotLights[MAX_LIGHTS_SPOT];
	uint dirLightsCount;
	uint pointLightsCount;
	uint spotLightsCount;
	float3 lod;
	float fogStart;
	float fogRange;
	float4 fogColor;
	Material k;
	float3 pPosition;
	float3 pNormal;
	float4 pColorDiffuse;
	float4 pColorSpecular;
	float3 ePosition;
	float4 sLightPositionLD;
	float4 sLightPositionHD;
	uint shadows;
	Texture2D shadowMapLD;
	Texture2D shadowMapHD;
};

inline float4 ComputeLightsLOD1(ComputeLightsInput input)
{
	float4 lDiffuse = 0;
	float4 lSpecular = 0;

	uint i = 0;

	for (i = 0; i < input.dirLightsCount; i++)
	{
		float3 L = normalize(-input.dirLights[i].Direction);
		float3 V = normalize(input.ePosition - input.pPosition);
		float3 R = 2 * dot(L, input.pNormal) * input.pNormal - L;

		float cShadowFactor = 1;
        if (input.dirLights[i].CastShadow == 1 && input.shadows > 0)
		{
			cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
		}

		float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal) * cShadowFactor;
		float4 cSpecular = SpecularBlinnPhongPass(input.dirLights[i].Specular, input.k.Shininess, L, input.pNormal, V) * cShadowFactor;

		lDiffuse += cDiffuse * cShadowFactor;
		lSpecular += cSpecular * cShadowFactor;
	}

	for (i = 0; i < input.pointLightsCount; i++)
	{
		float D = length(input.pointLights[i].Position - input.pPosition);
		float3 L = normalize(input.pointLights[i].Position - input.pPosition);
		float3 V = normalize(input.ePosition - input.pPosition);
		float3 R = 2 * dot(L, input.pNormal) * input.pNormal - L;

		float attenuation = CalcSphericAttenuation(input.pointLights[i].Intensity, input.pointLights[i].Radius, D);

		float4 cDiffuse = DiffusePass(input.pointLights[i].Diffuse, L, input.pNormal) * attenuation;
		float4 cSpecular = SpecularBlinnPhongPass(input.pointLights[i].Specular, input.k.Shininess, L, input.pNormal, V) * attenuation;

		lDiffuse += (cDiffuse * attenuation);
		lSpecular += (cSpecular * attenuation);
	}

	for (i = 0; i < input.spotLightsCount; i++)
	{
		float D = length(input.spotLights[i].Position - input.pPosition);
		float3 L = normalize(input.spotLights[i].Position - input.pPosition);
		float3 V = normalize(input.ePosition - input.pPosition);
		float3 R = 2 * dot(L, input.pNormal) * input.pNormal - L;

		float attenuation = CalcSphericAttenuation(input.spotLights[i].Intensity, input.spotLights[i].Radius, D);
		attenuation *= CalcSpotCone(input.spotLights[i].Direction, input.spotLights[i].Angle, L);

		float4 cDiffuse = DiffusePass(input.spotLights[i].Diffuse, L, input.pNormal) * attenuation;
		float4 cSpecular = SpecularBlinnPhongPass(input.spotLights[i].Specular, input.k.Shininess, L, input.pNormal, V) * attenuation;

		lDiffuse += (cDiffuse * attenuation);
		lSpecular += (cSpecular * attenuation);
	}

	float4 emissive = input.k.Emissive;
	float4 ambient = input.k.Ambient * input.Ga;

	float4 diffuse = input.k.Diffuse * lDiffuse;
	float4 specular = input.k.Specular * lSpecular * input.pColorSpecular;

	float4 color = (emissive + ambient + diffuse + specular) * input.pColorDiffuse;

	return saturate(color);
}
inline float4 ComputeLightsLOD2(ComputeLightsInput input)
{
	float4 lDiffuse = 0;

	uint i = 0;

	for (i = 0; i < input.dirLightsCount; i++)
	{
		float3 L = normalize(-input.dirLights[i].Direction);

		float cShadowFactor = 1;
        if (input.dirLights[i].CastShadow == 1 && input.shadows > 0)
		{
			cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
		}

		float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal) * cShadowFactor;

		lDiffuse += cDiffuse;
	}

	for (i = 0; i < input.pointLightsCount; i++)
	{
		float D = length(input.pointLights[i].Position - input.pPosition);
		float3 L = normalize(input.pointLights[i].Position - input.pPosition);

		float attenuation = CalcSphericAttenuation(input.pointLights[i].Intensity, input.pointLights[i].Radius, D);

		float4 cDiffuse = DiffusePass(input.pointLights[i].Diffuse, L, input.pNormal) * attenuation;

		lDiffuse += cDiffuse;
	}

	for (i = 0; i < input.spotLightsCount; i++)
	{
		float D = length(input.spotLights[i].Position - input.pPosition);
		float3 L = normalize(input.spotLights[i].Position - input.pPosition);

		float attenuation = CalcSphericAttenuation(input.spotLights[i].Intensity, input.spotLights[i].Radius, D);
		attenuation *= CalcSpotCone(input.spotLights[i].Direction, input.spotLights[i].Angle, L);

		float4 cDiffuse = DiffusePass(input.spotLights[i].Diffuse, L, input.pNormal) * attenuation;

		lDiffuse += cDiffuse;
	}

	float4 emissive = input.k.Emissive;
	float4 ambient = input.k.Ambient * input.Ga;

	float4 diffuse = input.k.Diffuse * lDiffuse;

	float4 color = (emissive + ambient + diffuse) * input.pColorDiffuse;

	return saturate(color);
}
inline float4 ComputeLightsLOD3(ComputeLightsInput input)
{
	float4 lDiffuse = 0;

	uint i = 0;

	for (i = 0; i < input.dirLightsCount; i++)
	{
		float3 L = normalize(-input.dirLights[i].Direction);

		float cShadowFactor = 1;
        if (input.dirLights[i].CastShadow == 1 && input.shadows > 0)
		{
			cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
		}

		float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal) * cShadowFactor;

		lDiffuse += cDiffuse;
	}

	float4 emissive = input.k.Emissive;
	float4 ambient = input.k.Ambient * input.Ga;

	float4 diffuse = input.k.Diffuse * lDiffuse;

	float4 color = (emissive + ambient + diffuse) * input.pColorDiffuse;

	return saturate(color);
}
inline float4 ComputeLightsLOD4(ComputeLightsInput input)
{
	float4 lDiffuse = 0;

	uint i = 0;

	for (i = 0; i < input.dirLightsCount; i++)
	{
		float3 L = normalize(-input.dirLights[i].Direction);

		float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal);

		lDiffuse += cDiffuse;
	}

	float4 emissive = input.k.Emissive;
	float4 ambient = input.k.Ambient * input.Ga;

	float4 diffuse = input.k.Diffuse * lDiffuse;

	float4 color = (emissive + ambient + diffuse) * input.pColorDiffuse;

	return saturate(color);
}
inline float4 ComputeLights(ComputeLightsInput input)
{
	float distToEye = length(input.ePosition - input.pPosition);

	float fog = 0;
	if (input.fogRange > 0)
	{
		fog = CalcFogFactor(distToEye, input.fogStart, input.fogRange);
	}

	if (fog >= 1)
	{
		return input.fogColor;
	}
	else
	{
		float4 color = 0;
		if (distToEye < input.lod.x)
		{
			color = ComputeLightsLOD1(input);
		}
		else if (distToEye < input.lod.y)
		{
			color = ComputeLightsLOD2(input);
		}
		else if (distToEye < input.lod.z)
		{
			color = ComputeLightsLOD3(input);
		}
		else
		{
			color = ComputeLightsLOD4(input);
		}

		return float4(lerp(color.rgb, input.fogColor.rgb, fog), color.a);
	}
}
