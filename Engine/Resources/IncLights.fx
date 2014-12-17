RasterizerState Solid
{
	FillMode = SOLID;
	CullMode = BACK;
};

RasterizerState WireFrame
{
	FillMode = WIREFRAME;
	CullMode = NONE;
};

SamplerState samLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

SamplerState samAnisotropic
{
	Filter = ANISOTROPIC;
	MaxAnisotropy = 4;

	AddressU = WRAP;
	AddressV = WRAP;
};

SamplerState samFont;

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
	float3 Att;
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
	float3 Att;
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
};
struct LightOutput
{
	float4 ambient;
	float4 diffuse;
	float4 specular;
};

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

	//Ambient term.
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

	//Attenuate
	float att = 1.0f / dot(L.Att, float3(1.0f, d, d*d));
	diffuse *= att;
	spec *= att;
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

	//Ambient term.
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

	//Scale by spotlight factor and attenuate.
	float spot = pow(max(dot(-lightVec, L.Direction), 0.0f), L.Spot);

	//Scale by spotlight factor and attenuate.
	float att = spot / dot(L.Att, float3(1.0f, d, d*d));
	ambient *= spot;
	diffuse *= att;
	spec *= att;
}

LightOutput ComputeLights(LightInput input)
{
	LightOutput output = (LightOutput)0;

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

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
			diffuse += D;
			specular += S;
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