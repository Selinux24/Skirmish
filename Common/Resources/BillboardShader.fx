#include "Lights.fx"

cbuffer cbPerFrame : register (b0)
{
	DirectionalLight gDirLights[3];
	PointLight gPointLight;
	SpotLight gSpotLight;
	float3 gEyePositionWorld;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
};

cbuffer cbPerObject : register (b1)
{
	float4x4 gWorldViewProjection;
	Material gMaterial;
};

cbuffer cbFixed
{
	float2 gTexC[4] =
	{
		float2(0.0f, 1.0f),
		float2(0.0f, 0.0f),
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f)
	};
};

Texture2DArray gTreeMapArray;

SamplerState samLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct VertexIn
{
	float3 positionWorld : POSITION;
	float2 sizeWorld : SIZE;
};
struct VertexOut
{
	float3 centerWorld : POSITION;
	float2 sizeWorld : SIZE;
};
struct GeoOut
{
	float4 positionHomogeneous : SV_POSITION;
	float3 positionWorld : POSITION;
	float3 normalWorld : NORMAL;
	float2 tex : TEXCOORD;
	uint primitiveID : SV_PrimitiveID;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	vout.centerWorld = vin.positionWorld;
	vout.sizeWorld = vin.sizeWorld;

	return vout;
}

[maxvertexcount(4)]
void GS(point VertexOut input[1], uint primID : SV_PrimitiveID, inout TriangleStream<GeoOut> outputStream)
{
	// Compute the local coordinate system of the sprite relative to the world space such 
	// that the billboard is aligned with the y-axis and faces the eye.
	float3 up = float3(0.0f, 1.0f, 0.0f);
	float3 look = gEyePositionWorld - input[0].centerWorld;
	look.y = 0.0f; // y-axis aligned, so project to xz-plane
	look = normalize(look);
	float3 right = cross(up, look);

	// Compute triangle strip vertices (quad) in world space.
	float halfWidth = 0.5f * input[0].sizeWorld.x;
	float halfHeight = 0.5f * input[0].sizeWorld.y;
	float4 v[4];
	v[0] = float4(input[0].centerWorld + halfWidth * right - halfHeight * up, 1.0f);
	v[1] = float4(input[0].centerWorld + halfWidth * right + halfHeight * up, 1.0f);
	v[2] = float4(input[0].centerWorld - halfWidth * right - halfHeight * up, 1.0f);
	v[3] = float4(input[0].centerWorld - halfWidth * right + halfHeight * up, 1.0f);

	// Transform quad vertices to world space and output them as a triangle strip.
	GeoOut gout;
	[unroll]
	for(int i = 0; i < 4; ++i)
	{
		gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
		gout.positionWorld = v[i].xyz;
		gout.normalWorld = up;
		gout.tex = gTexC[i];
		gout.primitiveID = primID;

		outputStream.Append(gout);
	}
}

float4 PS(GeoOut input) : SV_Target
{
	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;

	float distToEye = length(toEyeWorld);

	toEyeWorld /= distToEye;

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float4 A, D, S;

	[unroll]
	for(int i = 0; i < 3; ++i)
	{
		if(gDirLights[i].Padding == 1.0f)
		{
			ComputeDirectionalLight(
				gMaterial, 
				gDirLights[i],
				input.normalWorld, 
				toEyeWorld, 
				A, 
				D, 
				S);

			ambient += A;
			diffuse += D;
			spec += S;
		}
	}

	if(gPointLight.Padding == 1.0f)
	{
		ComputePointLight(
			gMaterial, 
			gPointLight,
			input.positionWorld, 
			input.normalWorld, 
			toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		spec += S;
	}

	if(gSpotLight.Padding == 1.0f)
	{
		ComputeSpotLight(
			gMaterial, 
			gSpotLight,
			input.positionWorld, 
			input.normalWorld, 
			toEyeWorld, 
			A, 
			D, 
			S);

		ambient += A;
		diffuse += D;
		spec += S;
	}

	float3 uvw = float3(input.tex, input.primitiveID%4);
	float4 textureColor = gTreeMapArray.Sample(samLinear, uvw);
	clip(textureColor.a - 0.05f);

	float4 litColor = textureColor * (ambient + diffuse) + spec;

	if(gFogRange > 0)
	{
		float fogLerp = saturate((distToEye - gFogStart) / gFogRange);

		litColor = lerp(litColor, gFogColor, fogLerp);
	}

	litColor.a = gMaterial.Diffuse.a * textureColor.a;

	return litColor;
}

technique11 Billboard
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(CompileShader(gs_5_0, GS()));
		SetPixelShader(CompileShader(ps_5_0, PS()));
	}
}