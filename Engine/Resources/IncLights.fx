RasterizerState RasterizerDefault;
RasterizerState RasterizerSolid
{
	FillMode = SOLID;
	CullMode = BACK;
};
RasterizerState RasterizerWireFrame
{
	FillMode = WIREFRAME;
	CullMode = NONE;
};
RasterizerState RasterizerDepth
{
	DepthBias = 100000;
    DepthBiasClamp = 0.0f;
	SlopeScaledDepthBias = 1.0f;
};
RasterizerState RasterizerNoCull
{
    CullMode = None;
};

DepthStencilState StencilDefault;
DepthStencilState StencilEnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ZERO;
};
DepthStencilState StencilDisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};
DepthStencilState StencilLessEqualDSS
{
    DepthFunc = LESS_EQUAL;
};

BlendState BlendDefault;
BlendState BlendAdditive
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
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
SamplerState SamplerFont;
SamplerState SamplerPoint;

SamplerComparisonState SamplerShadow
{
	Filter   = COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
	AddressU = BORDER;
	AddressV = BORDER;
	AddressW = BORDER;
	BorderColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

    ComparisonFunc = LESS;
};

struct Material
{
	float4 Diffuse;
	float SpecularIntensity;
	float SpecularPower;
};

static const int MAX_LIGHTS_DIRECTIONAL = 3;
static const int MAX_LIGHTS_POINT = 4;
static const int MAX_LIGHTS_SPOT = 4;

struct DirectionalLight
{
	float3 Color;
	float Ambient;
	float Diffuse;
	float3 Direction;
	float CastShadow;
	float Enabled;
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
};
struct SpotLight
{
	float3 Color;
	float Ambient;
	float Diffuse;
	float3 Position;
	float3 Direction;
	float Spot;
    float AttenuationConstant;
	float AttenuationLinear;
	float AttenuationExp;
	float CastShadow;
	float Enabled;
};

static const float SHADOWMAPSIZE = 2048.0f;
static const float SHADOWMAPDX = 1.0f / SHADOWMAPSIZE;
static const float2 SamplerShadowOffsets[9] =
{
	float2(-SHADOWMAPDX, -SHADOWMAPDX),		float2(0.0f, -SHADOWMAPDX),		float2(SHADOWMAPDX, -SHADOWMAPDX),
	float2(-SHADOWMAPDX, 0.0f),				float2(0.0f, 0.0f),				float2(SHADOWMAPDX, 0.0f),
	float2(-SHADOWMAPDX, +SHADOWMAPDX),		float2(0.0f, +SHADOWMAPDX),		float2(SHADOWMAPDX, +SHADOWMAPDX)
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
		percentLit += shadowMap.SampleCmpLevelZero(SamplerShadow, shadowPosH.xy + SamplerShadowOffsets[i], depth).r;
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
	float3 color,
	float ambient,
	float diffuse,
	float3 direction,
	float3 toEye,
	float3 position,
	float3 normal,
	float specularIntensity,
	float specularPower)
{                                                                                           
    float4 ambientColor = float4(color * ambient, 1.0f);

	float4 diffuseColor  = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 specularColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float diffuseFactor = dot(normal, -direction);

    if (diffuseFactor > 0) 
	{
		diffuseColor = float4(color * diffuse * diffuseFactor, 1.0f);

        float3 reflectLight = normalize(reflect(direction, normal));
        
		float specFactor = dot(toEye, reflectLight);
		if (specFactor > 0)
		{
			specFactor = pow(abs(specFactor), max(1, specularPower));
            specularColor = float4(color * specularIntensity * specFactor, 1.0f);
        }
	}

    return (ambientColor + diffuseColor + specularColor);
}

float4 ComputeDirectionalLight(
	DirectionalLight L,
	float3 toEye,
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
		position,
		normal,
		specularIntensity,
		specularPower);

	if(L.CastShadow == 1)
	{
		float shadowFactor = CalcShadowFactor(shadowPosition, shadowMap);

		litColor *= shadowFactor;
	}

	return litColor;
}

float4 ComputePointLight(
	PointLight L, 
	float3 toEye,
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
	float3 position,
	float3 normal, 
	float specularIntensity,
	float specularPower)
{
	float3 lightDirection = position - L.Position;
	float spot = dot(lightDirection, L.Direction);   

	if (spot > L.Spot)
	{
		float distance = length(lightDirection);
		lightDirection /= distance;

		float attenuation =
			L.AttenuationConstant +
			L.AttenuationLinear * distance +
			L.AttenuationExp * distance * distance;

		attenuation = max(1.0f, attenuation);

		float4 litColor = ComputeBaseLight(
			L.Color,
			L.Ambient,
			L.Diffuse,
			L.Direction,
			toEye,
			position,
			normal,
			specularIntensity,
			specularPower);

		return litColor / attenuation;
	}
	else
	{
		return float4(0.0f, 0.0f, 0.0f, 0.0f);
	}
}

float4 ComputeLights(
	DirectionalLight dirLights[MAX_LIGHTS_DIRECTIONAL],
	PointLight pointLights[MAX_LIGHTS_POINT],
	SpotLight spotLights[MAX_LIGHTS_SPOT],
	float3 toEye,
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
	for(i = 0; i < MAX_LIGHTS_DIRECTIONAL; ++i)
	{
		if(dirLights[i].Enabled == 1.0f)
		{
			litColor += ComputeDirectionalLight(
				dirLights[i],
				toEye,
				position,
				normal,
				specularIntensity,
				specularPower,
				shadowPosition,
				shadowMap);
		}
	}

	[unroll]
	for(i = 0; i < MAX_LIGHTS_POINT; ++i)
	{
		if(pointLights[i].Enabled == 1.0f)
		{
			litColor += ComputePointLight(
				pointLights[i],
				toEye,
				position,
				normal,
				specularIntensity,
				specularPower);
		}
	}

	[unroll]
	for(i = 0; i < MAX_LIGHTS_SPOT; ++i)
	{
		if(spotLights[i].Enabled == 1.0f)
		{
			litColor += ComputeSpotLight(
				spotLights[i],
				toEye,
				position,
				normal,
				specularIntensity,
				specularPower);
		}
	}

	return litColor;
}


