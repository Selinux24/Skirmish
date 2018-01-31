#include "IncHelpers.fx"
#include "IncMaterials.fx"

static const int MAX_LIGHTS_DIRECTIONAL = 3;
static const int MAX_LIGHTS_POINT = 16;
static const int MAX_LIGHTS_SPOT = 16;

struct HemisphericLight
{
	float4 AmbientDown;
	float4 AmbientUp;
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
    float CastShadow;
    float2 PerspectiveValues;
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
    float CastShadow;
    float2 Pad;
};

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

static float minShadowFactor = 0.2;

float PointShadowPCFDepth(float3 toPixel, float2 perspectiveValues)
{
    float3 toPixelAbs = abs(toPixel);
    float z = max(toPixelAbs.x, max(toPixelAbs.y, toPixelAbs.z));
    return (perspectiveValues.x * z + perspectiveValues.y) / z;
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
    return pow(max(0, dot(reflect(V, N), -L)), lShininess) * lSpecular;
}

inline float4 CalcAmbient(float4 ambientDown, float4 ambientUp, float3 normal)
{
	// Convert from [-1, 1] to [0, 1]
	float up = normal.y * 0.5 + 0.5;

	// Calculate the ambient value
	return float4(ambientDown.rgb + up * ambientUp.rgb, 1);
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

inline float4 LightEquation(Material k, float4 lAmbient, float globalAmbient, float4 lDiffuse, float4 pDiffuse, float4 lSpecular, float4 pSpecular, float dist)
{
	float4 emissive = k.Emissive;
	float4 ambient = lAmbient * globalAmbient;

	float4 diffuse = k.Diffuse * lDiffuse;
	float4 specular = k.Specular * lSpecular * pSpecular * dist;

	return (emissive + ambient + diffuse + specular) * pDiffuse;
}

inline float4 LightEquation2(Material k, float4 lAmbient, float globalAmbient, float4 light, float4 pDiffuse)
{
	float4 emissive = k.Emissive;
	float4 ambient = lAmbient * globalAmbient;

	return (emissive + ambient + light) * pDiffuse;
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

inline ComputeLightsOutput ComputeDirectionalLightLOD1(ComputeDirectionalLightsInput input, float dist)
{
    float3 L = normalize(-input.dirLight.Direction);
    float3 V = normalize(input.ePosition - input.pPosition);

    float cShadowFactor = 1;
    [flatten]
    if (input.dirLight.CastShadow == 1)
    {
        cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
    }

    ComputeLightsOutput output;

    output.diffuse = DiffusePass(input.dirLight.Diffuse, L, input.pNormal) * cShadowFactor;
    output.specular = SpecularBlinnPhongPass(input.dirLight.Specular, input.shininess, L, input.pNormal, V) * dist * cShadowFactor;

    return output;
}
inline ComputeLightsOutput ComputeDirectionalLightLOD2(ComputeDirectionalLightsInput input)
{
    float3 L = normalize(-input.dirLight.Direction);

    float cShadowFactor = 1;
    [flatten]
    if (input.dirLight.CastShadow == 1)
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
    if (input.dirLight.CastShadow == 1)
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

    float cShadowFactor = 1;
    [flatten]
    if (input.dirLight.CastShadow == 1)
    {
        cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
    }

    ComputeLightsOutput output;

    output.diffuse = DiffusePass(input.dirLight.Diffuse, L, input.pNormal) * cShadowFactor;
    output.specular = 0;

    return output;
}
inline ComputeLightsOutput ComputeDirectionalLight(ComputeDirectionalLightsInput input)
{
    float distToEye = length(input.ePosition - input.pPosition);

    if (distToEye < input.lod.x)
    {
        return ComputeDirectionalLightLOD1(input, 1.0f - (distToEye / input.lod.x));
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
    TextureCube<float> shadowCubic;
};

inline ComputeLightsOutput ComputePointLightLOD1(ComputePointLightsInput input, float dist)
{
    float3 L = input.pointLight.Position - input.pPosition;
    float D = length(L);
    L /= D;
    float3 V = normalize(input.ePosition - input.pPosition);

    float cShadowFactor = 1;
    [flatten]
    if (input.pointLight.CastShadow == 1)
    {
        float3 toPixel = input.pPosition - input.pointLight.Position;
        float depth = PointShadowPCFDepth(toPixel, input.pointLight.PerspectiveValues);
        cShadowFactor = max(minShadowFactor, input.shadowCubic.SampleCmpLevelZero(PCFSampler, toPixel, depth));
    }

    float attenuation = CalcSphericAttenuation(input.pointLight.Intensity, input.pointLight.Radius, D);

    ComputeLightsOutput output;

    output.diffuse = DiffusePass(input.pointLight.Diffuse, L, input.pNormal) * attenuation * cShadowFactor;
    output.specular = SpecularBlinnPhongPass(input.pointLight.Specular, input.shininess, L, input.pNormal, V) * dist * attenuation * cShadowFactor;

    return output;
}
inline ComputeLightsOutput ComputePointLightLOD2(ComputePointLightsInput input)
{
    float3 L = input.pointLight.Position - input.pPosition;
    float D = length(L);
    L /= D;

    float cShadowFactor = 1;
    [flatten]
    if (input.pointLight.CastShadow == 1)
    {
        float3 toPixel = input.pPosition - input.pointLight.Position;
        float depth = PointShadowPCFDepth(toPixel, input.pointLight.PerspectiveValues);
        cShadowFactor = max(minShadowFactor, input.shadowCubic.SampleCmpLevelZero(PCFSampler, toPixel, depth));
    }

    float attenuation = CalcSphericAttenuation(input.pointLight.Intensity, input.pointLight.Radius, D);

    ComputeLightsOutput output;

    output.diffuse = DiffusePass(input.pointLight.Diffuse, L, input.pNormal) * attenuation * cShadowFactor;
    output.specular = 0;

    return output;
}
inline ComputeLightsOutput ComputePointLight(ComputePointLightsInput input)
{
    float distToEye = length(input.ePosition - input.pPosition);

    if (distToEye < input.lod.x)
    {
        return ComputePointLightLOD1(input, 1.0f - (max(1.0f, distToEye / input.lod.x)));
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

inline ComputeLightsOutput ComputeSpotLightLOD1(ComputeSpotLightsInput input, float dist)
{
    float3 L = input.spotLight.Position - input.pPosition;
    float D = length(L);
    L /= D;
    float3 V = normalize(input.ePosition - input.pPosition);

    float attenuation = CalcSphericAttenuation(input.spotLight.Intensity, input.spotLight.Radius, D);
    attenuation *= CalcSpotCone(input.spotLight.Direction, input.spotLight.Angle, L);

    ComputeLightsOutput output;

    output.diffuse = DiffusePass(input.spotLight.Diffuse, L, input.pNormal) * attenuation;
    output.specular = SpecularBlinnPhongPass(input.spotLight.Specular, input.shininess, L, input.pNormal, V) * dist * attenuation;

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
        return ComputeSpotLightLOD1(input, 1.0f - (max(1.0f, distToEye / input.lod.x)));
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
    DirectionalLight dirLights[MAX_LIGHTS_DIRECTIONAL];
    PointLight pointLights[MAX_LIGHTS_POINT];
    SpotLight spotLights[MAX_LIGHTS_SPOT];
	HemisphericLight hemiLight;
	float Ga;
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
    TextureCubeArray<float> shadowCubic;
};

inline float4 ComputeLightsLOD1(ComputeLightsInput input, float dist)
{
    float4 lDiffuse = 0;
    float4 lSpecular = 0;

    float3 V = normalize(input.ePosition - input.pPosition);

	float4 lAmbient = CalcAmbient(input.hemiLight.AmbientDown, input.hemiLight.AmbientUp, input.pNormal);

    uint i = 0;

    for (i = 0; i < input.dirLightsCount; i++)
    {
        float3 L = normalize(-input.dirLights[i].Direction);

        float cShadowFactor = 1;
        [flatten]
        if (input.dirLights[i].CastShadow == 1)
        {
            cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
        }

        float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal);
        float4 cSpecular = SpecularBlinnPhongPass(input.dirLights[i].Specular, input.k.Shininess, L, input.pNormal, V);

        lDiffuse += (cDiffuse * cShadowFactor);
        lSpecular += (cSpecular * cShadowFactor);
    }

    for (i = 0; i < input.pointLightsCount; i++)
    {
        float3 P = input.pointLights[i].Position - input.pPosition;
        float D = length(P);
        float3 L = P / D;

        float cShadowFactor = 1;
        [flatten]
        if (input.pointLights[i].CastShadow == 1)
        {
            float3 toPixel = input.pPosition - input.pointLights[i].Position;
            float depth = PointShadowPCFDepth(toPixel, input.pointLights[i].PerspectiveValues);
            cShadowFactor = max(minShadowFactor, input.shadowCubic.SampleCmpLevelZero(PCFSampler, float4(toPixel, i), depth));
        }

        float attenuation = CalcSphericAttenuation(input.pointLights[i].Intensity, input.pointLights[i].Radius, D);

        float4 cDiffuse = DiffusePass(input.pointLights[i].Diffuse, L, input.pNormal);
        float4 cSpecular = SpecularBlinnPhongPass(input.pointLights[i].Specular, input.k.Shininess, L, input.pNormal, V);

        lDiffuse += (cDiffuse * cShadowFactor * attenuation);
        lSpecular += (cSpecular * cShadowFactor * attenuation);
    }

    for (i = 0; i < input.spotLightsCount; i++)
    {
        float3 P = input.spotLights[i].Position - input.pPosition;
        float D = length(P);
        float3 L = P / D;

        float attenuation = CalcSphericAttenuation(input.spotLights[i].Intensity, input.spotLights[i].Radius, D);
        attenuation *= CalcSpotCone(input.spotLights[i].Direction, input.spotLights[i].Angle, L);

        float4 cDiffuse = DiffusePass(input.spotLights[i].Diffuse, L, input.pNormal);
        float4 cSpecular = SpecularBlinnPhongPass(input.spotLights[i].Specular, input.k.Shininess, L, input.pNormal, V);

        lDiffuse += (cDiffuse * attenuation);
        lSpecular += (cSpecular * attenuation);
    }

	return LightEquation(input.k, lAmbient, input.Ga, lDiffuse, input.pColorDiffuse, lSpecular, input.pColorSpecular, dist);
}
inline float4 ComputeLightsLOD2(ComputeLightsInput input)
{
    float4 lDiffuse = 0;

	float4 lAmbient = CalcAmbient(input.hemiLight.AmbientDown, input.hemiLight.AmbientUp, input.pNormal);

    uint i = 0;

    for (i = 0; i < input.dirLightsCount; i++)
    {
        float3 L = normalize(-input.dirLights[i].Direction);

        float cShadowFactor = 1;
        [flatten]
        if (input.dirLights[i].CastShadow == 1)
        {
            cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
        }

        float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal);

        lDiffuse += (cDiffuse * cShadowFactor);
    }

    for (i = 0; i < input.pointLightsCount; i++)
    {
        float3 P = input.pointLights[i].Position - input.pPosition;
        float D = length(P);
        float3 L = P / D;

        float cShadowFactor = 1;
        [flatten]
        if (input.pointLights[i].CastShadow == 1)
        {
            float3 toPixel = input.pPosition - input.pointLights[i].Position;
            float depth = PointShadowPCFDepth(toPixel, input.pointLights[i].PerspectiveValues);
            cShadowFactor = max(minShadowFactor, input.shadowCubic.SampleCmpLevelZero(PCFSampler, float4(toPixel, i), depth));
        }

        float attenuation = CalcSphericAttenuation(input.pointLights[i].Intensity, input.pointLights[i].Radius, D);

        float4 cDiffuse = DiffusePass(input.pointLights[i].Diffuse, L, input.pNormal);

        lDiffuse += (cDiffuse * attenuation * cShadowFactor);
    }

    for (i = 0; i < input.spotLightsCount; i++)
    {
        float3 P = input.spotLights[i].Position - input.pPosition;
        float D = length(P);
        float3 L = P / D;

        float attenuation = CalcSphericAttenuation(input.spotLights[i].Intensity, input.spotLights[i].Radius, D);
        attenuation *= CalcSpotCone(input.spotLights[i].Direction, input.spotLights[i].Angle, L);

        float4 cDiffuse = DiffusePass(input.spotLights[i].Diffuse, L, input.pNormal);

        lDiffuse += (cDiffuse * attenuation);
    }

	return LightEquation(input.k, lAmbient, input.Ga, lDiffuse, input.pColorDiffuse, 0, 0, 0);
}
inline float4 ComputeLightsLOD3(ComputeLightsInput input)
{
    float4 lDiffuse = 0;

	float4 lAmbient = CalcAmbient(input.hemiLight.AmbientDown, input.hemiLight.AmbientUp, input.pNormal);

    uint i = 0;

    for (i = 0; i < input.dirLightsCount; i++)
    {
        float3 L = normalize(-input.dirLights[i].Direction);

        float cShadowFactor = 1;
        [flatten]
        if (input.dirLights[i].CastShadow == 1)
        {
            cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
        }

        float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal);

        lDiffuse += (cDiffuse * cShadowFactor);
    }

	return LightEquation(input.k, lAmbient, input.Ga, lDiffuse, input.pColorDiffuse, 0, 0, 0);
}
inline float4 ComputeLightsLOD4(ComputeLightsInput input)
{
    float4 lDiffuse = 0;

	float4 lAmbient = CalcAmbient(input.hemiLight.AmbientDown, input.hemiLight.AmbientUp, input.pNormal);

    uint i = 0;

    for (i = 0; i < input.dirLightsCount; i++)
    {
        float3 L = normalize(-input.dirLights[i].Direction);

        float cShadowFactor = 1;
        [flatten]
        if (input.dirLights[i].CastShadow == 1)
        {
            cShadowFactor = CalcShadowFactor(input.shadows, input.sLightPositionLD, input.sLightPositionHD, input.shadowMapLD, input.shadowMapHD);
        }

        float4 cDiffuse = DiffusePass(input.dirLights[i].Diffuse, L, input.pNormal);

        lDiffuse += (cDiffuse * cShadowFactor);
    }

	return LightEquation(input.k, lAmbient, input.Ga, lDiffuse, input.pColorDiffuse, 0, 0, 0);
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
            color = ComputeLightsLOD1(input, 1.0f - (distToEye / input.lod.x));
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
