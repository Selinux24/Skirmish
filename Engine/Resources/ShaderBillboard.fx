#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float4x4 gLightViewProjection;
	float3 gEyePositionWorld;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPointLights[MAX_LIGHTS_POINT];
	SpotLight gSpotLights[MAX_LIGHTS_SPOT];
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
	uint gShadows;
	float gStartRadius;
	float gEndRadius;
	float3 gWindDirection;
	float gWindStrength;
	float gTotalTime;
};
cbuffer cbPerObject : register (b1)
{
	Material gMaterial;
	uint gTextureCount;
};
cbuffer cbFixed : register (b2)
{
	float2 gQuadTexC[4] = 
	{
		float2(0.0f, 1.0f),
		float2(0.0f, 0.0f),
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f)
	};
};

Texture2DArray gTextureArray;
Texture2D gShadowMapStatic;
Texture2D gShadowMapDynamic;
Texture1D gTextureRandom;

float3 CalcWindTranslation(uint primID, float3 pos)
{
	float3 vWind = sin(gTotalTime + (pos.x + pos.y + pos.z) * 0.1f) + (gWindDirection * gWindStrength);

	float sRandom = gTextureRandom.SampleLevel(SamplerLinear, primID, 0).x;

	return pos + (vWind * min(1, sRandom));
}

GSVertexBillboard VSBillboard(VSVertexBillboard input)
{
	GSVertexBillboard output;

	output.centerWorld = input.positionWorld;
	output.centerWorld.y += (input.sizeWorld.y * 0.45f);
	output.sizeWorld = input.sizeWorld;

	return output;
}

[maxvertexcount(4)]
void GSBillboard(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard> outputStream)
{
	float3 look = gEyePositionWorld - input[0].centerWorld;
	if(gEndRadius == 0 || length(look) < gEndRadius)
	{
		//Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
		look.y = 0.0f; // y-axis aligned, so project to xz-plane
		look = normalize(look);
		float3 up = float3(0.0f, 1.0f, 0.0f);
		float3 right = cross(up, look);

		//Compute triangle strip vertices (quad) in world space.
		float halfWidth = 0.5f * input[0].sizeWorld.x;
		float halfHeight = 0.5f * input[0].sizeWorld.y;
		float4 v[4] = {float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)};
		v[0] = float4(input[0].centerWorld + halfWidth * right - halfHeight * up, 1.0f);
		v[1] = float4(input[0].centerWorld + halfWidth * right + halfHeight * up, 1.0f);
		v[2] = float4(input[0].centerWorld - halfWidth * right - halfHeight * up, 1.0f);
		v[3] = float4(input[0].centerWorld - halfWidth * right + halfHeight * up, 1.0f);

		if(gWindStrength > 0)
		{
			v[1].xyz = CalcWindTranslation(primID, v[1].xyz);
			v[3].xyz = CalcWindTranslation(primID, v[3].xyz);
		}

		//Transform quad vertices to world space and output them as a triangle strip.
		PSVertexBillboard gout;
		[unroll]
		for(int i = 0; i < 4; ++i)
		{
			gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
			gout.positionWorld = mul(v[i], gWorld).xyz;
			gout.normalWorld = up;
			gout.tex = gQuadTexC[i];
			gout.primitiveID = primID;

			outputStream.Append(gout);
		}
	}
}
[maxvertexcount(4)]
void GSSMBillboard(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSShadowMapOutput> outputStream)
{
	float3 look = gEyePositionWorld - input[0].centerWorld;
	if(gEndRadius == 0 || length(look) < gEndRadius)
	{
		//Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
		look.y = 0.0f; // y-axis aligned, so project to xz-plane
		look = normalize(look);
		float3 up = float3(0.0f, 1.0f, 0.0f);
		float3 right = cross(up, look);

		//Compute triangle strip vertices (quad) in world space.
		float halfWidth = 0.5f * input[0].sizeWorld.x;
		float halfHeight = 0.5f * input[0].sizeWorld.y;
		float4 v[4] = {float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)};
		v[0] = float4(input[0].centerWorld + halfWidth * right - halfHeight * up, 1.0f);
		v[1] = float4(input[0].centerWorld + halfWidth * right + halfHeight * up, 1.0f);
		v[2] = float4(input[0].centerWorld - halfWidth * right - halfHeight * up, 1.0f);
		v[3] = float4(input[0].centerWorld - halfWidth * right + halfHeight * up, 1.0f);

		if(gWindStrength > 0)
		{
			v[1].xyz = CalcWindTranslation(primID, v[1].xyz);
			v[3].xyz = CalcWindTranslation(primID, v[3].xyz);
		}

		//Transform quad vertices to world space and output them as a triangle strip.
		PSShadowMapOutput gout;
		[unroll]
		for(int i = 0; i < 4; ++i)
		{
			gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
			gout.depth = v[i];
			gout.tex = gQuadTexC[i];
			gout.primitiveID = primID;

			outputStream.Append(gout);
		}
	}
}

float4 PSForwardBillboard(PSVertexBillboard input) : SV_Target
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw);
	clip(textureColor.a - 0.05f);

	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
	float3 toEye = normalize(toEyeWorld);

	float4 shadowPosition = mul(float4(input.positionWorld, 1), gLightViewProjection);

	float3 litColor = ComputeAllLights(
		gDirLights, 
		gPointLights, 
		gSpotLights,
		toEye,
		textureColor.rgb,
		input.positionWorld,
		input.normalWorld,
		gMaterial.SpecularIntensity,
		gMaterial.SpecularPower,
		shadowPosition,
		gShadows,
		gShadowMapStatic,
		gShadowMapDynamic);

	float distToEye = length(toEyeWorld);

	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor.rgb);
	}

	return float4(litColor, textureColor.a * (1.0f - (distToEye / gEndRadius * 0.5f)));
}
GBufferPSOutput PSDeferredBillboard(PSVertexBillboard input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw);
	clip(textureColor.a - 0.05f);

	output.color = textureColor;
	output.normal = float4(input.normalWorld, gMaterial.SpecularPower);
	output.depth = float4(input.positionWorld, gMaterial.SpecularIntensity);

    return output;
}
float4 PSSMBillboard(PSShadowMapOutput input) : SV_Target
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw);

	if(textureColor.a > 0.05f)
	{
		float depthValue = input.depth.z / input.depth.w;

		return float4(depthValue, depthValue, depthValue, 1.0f);
	}
	else
	{
		discard;

		return 0.0f;
	}
}

technique11 ForwardBillboard
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSBillboard()));
		SetGeometryShader(CompileShader(gs_5_0, GSBillboard()));
		SetPixelShader(CompileShader(ps_5_0, PSForwardBillboard()));
	}
}
technique11 DeferredBillboard
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSBillboard()));
		SetGeometryShader(CompileShader(gs_5_0, GSBillboard()));
		SetPixelShader(CompileShader(ps_5_0, PSDeferredBillboard()));
	}
}
technique11 ShadowMapBillboard
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSBillboard()));
		SetGeometryShader(CompileShader(gs_5_0, GSSMBillboard()));
		SetPixelShader(CompileShader(ps_5_0, PSSMBillboard()));
	}
}
