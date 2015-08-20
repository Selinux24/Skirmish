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
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float4 Reflect;
	float Padding;
};

struct DirectionalLight
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Direction;
	float Padding;
};
struct PointLight
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Position;
	float Range;
	float3 Attenuation;
	float Padding;
};
struct SpotLight
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Position;
	float Range;
	float3 Direction;
	float Spot;
	float3 Attenuation;
	float Padding;
};

struct LightInput
{
	float3 toEyeWorld;
	float3 positionWorld;
	float3 normalWorld;
	Material material;
	DirectionalLight dirLights[3];
	PointLight pointLight;
	SpotLight spotLight;
	float enableShadows;
	float4 shadowPosition;
};
struct LightOutput
{
	float4 ambient;
	float4 diffuse;
	float4 specular;
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

void ComputeDirectionalLight(
	Material mat, 
	DirectionalLight L,
	float3 normal, 
	float3 toEye,
	out float4 ambient,
	out float4 diffuse,
	out float4 spec)
{
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	//The light vector aims opposite the direction the light rays travel.
	float3 lightVec = -L.Direction;

	//Add ambient term.
	ambient = mat.Ambient * L.Ambient;

	//Add diffuse and specular term, provided the surface is in the line of site of the light.
	float diffuseFactor = dot(lightVec, normal);

	//Flatten to avoid dynamic branching.
	[flatten]
	if(diffuseFactor > 0.0f)
	{
		float3 v = reflect(-lightVec, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
	
		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec = specFactor * mat.Specular * L.Specular;
	}
}

void ComputePointLight(
	Material mat, 
	PointLight L, 
	float3 pos,
	float3 normal, 
	float3 toEye,
	out float4 ambient, 
	out float4 diffuse, 
	out float4 spec)
{
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	//The vector from the surface to the light.
	float3 lightVec = L.Position - pos;

	//The distance from surface to light.
	float d = length(lightVec);

	//Range test.
	if(d > L.Range)
		return;

	//Normalize the light vector.
	lightVec /= d;

	//Add diffuse and specular term, provided the surface is in the line of site of the light.
	float diffuseFactor = dot(lightVec, normal);

	//Flatten to avoid dynamic branching.
	[flatten]
	if(diffuseFactor > 0.0f)
	{
		float3 v = reflect(-lightVec, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
		
		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec = specFactor * mat.Specular * L.Specular;
	}

	//Attenuate
	float attenuation = 1.0f / dot(L.Attenuation, float3(1.0f, d, d*d));

	ambient = (mat.Ambient * L.Ambient) * attenuation;
	diffuse *= attenuation;
	spec *= attenuation;
}

void ComputeSpotLight(
	Material mat, 
	SpotLight L,
	float3 pos, 
	float3 normal, 
	float3 toEye,
	out float4 ambient, 
	out float4 diffuse, 
	out float4 spec)
{
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	//The vector from the surface to the light.
	float3 lightVec = L.Position - pos;

	//The distance from surface to light.
	float d = length(lightVec);

	//Range test.
	if( d > L.Range )
		return;

	//Normalize the light vector.
	lightVec /= d;

	//Add diffuse and specular term, provided the surface is in the line of site of the light.
	float diffuseFactor = dot(lightVec, normal);

	//Flatten to avoid dynamic branching.
	[flatten]
	if(diffuseFactor > 0.0f)
	{
		float3 v = reflect(-lightVec, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
		
		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec = specFactor * mat.Specular * L.Specular;
	}

	//Scale by spotlight factor.
	float spot = pow(max(dot(-lightVec, L.Direction), 0.0f), L.Spot);

	//Attenuate.
	float attenuation = spot / dot(L.Attenuation, float3(1.0f, d, d*d));

	//Ambient term.
	ambient = (mat.Ambient * L.Ambient) * attenuation * spot;
	diffuse *= attenuation;
	spec *= attenuation;
}

float4 ComputeDirectionalLight2(
	DirectionalLight L,
	float3 eyePosition,
	float3 normal)
{
	//The light vector aims opposite the direction the light rays travel.
	float3 lightVec = -L.Direction;

	float brightness = max(0, dot(normal, lightVec)) / (length(lightVec) * length(normal));
    brightness = clamp(brightness, 0, 1);

    return float4(brightness * L.Diffuse.rgb, 0.0f);
}

float4 ComputePointLight2(
	PointLight L,
	float3 eyePosition,
	float3 pos,
	float3 normal)
{
	//The vector from the surface to the light.
	float3 lightVec = L.Position - pos;

	//The distance from surface to light.
	float d = length(lightVec);

	//Range test.
	if(d > L.Range)
	{
		return 0.0f;
	}
	else
	{
		//Normalize the light vector.
		lightVec /= d;

		float intensity = max(0, dot(normal, lightVec));
		float4 light = intensity * (L.Diffuse * (1.0f - (d / L.Range)) * (L.Range / 10.0f));
		return float4(light.rgb, 0.0f);
	}
}

float4 ComputeSpotLight2(
	SpotLight L,
	float3 eyePosition,
	float3 pos, 
	float3 normal)
{
	//The vector from the surface to the light.
	float3 lightVec = L.Position - pos;

	//The distance from surface to light.
	float d = length(lightVec);

	//Range test.
	if( d > L.Range )
	{
		return 0.0f;
	}
	else
	{
		//Normalize the light vector.
		lightVec /= d;

		//Add diffuse and specular term, provided the surface is in the line of site of the light.
		float intensity = max(0, dot(normal, lightVec));

		//Scale by spotlight factor.
		float spot = pow(max(dot(-lightVec, L.Direction), 0.0f), L.Spot);

		float4 light = intensity * spot * (L.Diffuse * (1.0f - (d / L.Range)) * (L.Range / 10.0f));
		return float4(light.rgb, 0.0f);
	}
}

static const float SHADOWMAPSIZE = 2048.0f;
static const float SHADOWMAPDX = 1.0f / SHADOWMAPSIZE;
static const float2 SamplerShadowOffsets[9] =
{
	float2(-SHADOWMAPDX, -SHADOWMAPDX),		float2(0.0f, -SHADOWMAPDX),		float2(SHADOWMAPDX, -SHADOWMAPDX),
	float2(-SHADOWMAPDX, 0.0f),				float2(0.0f, 0.0f),				float2(SHADOWMAPDX, 0.0f),
	float2(-SHADOWMAPDX, +SHADOWMAPDX),		float2(0.0f, +SHADOWMAPDX),		float2(SHADOWMAPDX, +SHADOWMAPDX)
};

float CalcShadowFactor(float4 shadowPosH, Texture2D shadowMap)
{
	// Complete projection by doing division by w.
	shadowPosH.xyz /= shadowPosH.w;

	// Depth in NDC space.
	float depth = shadowPosH.z;

	// 3×3 box filter pattern. Each sample does a 4-tap PCF.
	float percentLit = 0.0f;
	[unroll]
	for(int i = 0; i < 4; ++i)
	{
		percentLit += shadowMap.SampleCmpLevelZero(SamplerShadow, shadowPosH.xy + SamplerShadowOffsets[i], depth).r;
	}

	// Average the samples.
	return percentLit / 4.0f;
}

LightOutput ComputeLights(LightInput input, Texture2D shadowMap)
{
	LightOutput output = (LightOutput)0;

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float3 shadow = float3(1.0f, 1.0f, 1.0f);
	if(input.enableShadows == 1)
	{
		// Only the first light casts a shadow.
		shadow[0] = CalcShadowFactor(input.shadowPosition, shadowMap);
	}

	float4 A, D, S;

	[unroll]
	for(int i = 0; i < 3; ++i)
	{
		if(input.dirLights[i].Padding == 1.0f)
		{
			ComputeDirectionalLight(
				input.material, 
				input.dirLights[i],
				input.normalWorld, 
				input.toEyeWorld, 
				A, 
				D, 
				S);

			ambient += A;
			diffuse += shadow[i] * D;
			specular += shadow[i] * S;
		}
	}

	if(input.pointLight.Padding == 1.0f)
	{
		ComputePointLight(
			input.material, 
			input.pointLight,
			input.positionWorld, 
			input.normalWorld, 
			input.toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		specular += S;
	}

	if(input.spotLight.Padding == 1.0f)
	{
		ComputeSpotLight(
			input.material, 
			input.spotLight,
			input.positionWorld, 
			input.normalWorld, 
			input.toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		specular += S;
	}

	output.ambient = ambient;
	output.diffuse = diffuse;
	output.specular = specular;

	return output;
}

float4 ComputeFog(float4 litColor, float distToEye, float fogStart, float fogRange, float4 fogColor)
{
	float fogLerp = saturate((distToEye - fogStart) / fogRange);

	return lerp(litColor, fogColor, fogLerp);
}


