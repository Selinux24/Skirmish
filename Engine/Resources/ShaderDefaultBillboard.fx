#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbGlobal : register (b0)
{
	uint gMaterialPaletteWidth;
	float3 gLOD;
};
Texture2D gMaterialPalette;
Texture1D gTextureRandom;

cbuffer cbFixed : register (b1)
{
	float2 gQuadTexCL[4] =
	{
		float2(0.0f, 1.0f),
		float2(0.0f, 0.0f),
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f)
	};
	float2 gQuadTexCR[4] =
	{
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f),
		float2(0.0f, 1.0f),
		float2(0.0f, 0.0f)
	};
};

cbuffer cbPerFrame : register (b2)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float4x4 gLightViewProjectionLD;
	float4x4 gLightViewProjectionHD;
	float3 gEyePositionWorld;
	float gGlobalAmbient;
	uint3 gLightCount;
	uint gShadows;
	float4 gFogColor;
	float gFogStart;
	float gFogRange;
	float gStartRadius;
	float gEndRadius;
	float3 gWindDirection;
	float gWindStrength;
	float gTotalTime;
	uint gMaterialIndex;
	uint gTextureCount;
	uint gUVToggleByPID;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPointLights[MAX_LIGHTS_POINT];
	SpotLight gSpotLights[MAX_LIGHTS_SPOT];
};
Texture2DArray gTextureArray;
Texture2D gShadowMapLD;
Texture2D gShadowMapHD;

float3 CalcWindTranslation(uint primID, float3 pos, float3 windDirection, float windStrength)
{
	float3 vWind = sin(gTotalTime + (pos.x + pos.y + pos.z) * 0.1f) + (windDirection * windStrength);

	float sRandom = gTextureRandom.SampleLevel(SamplerLinear, primID, 0).x;

	return pos + (vWind * min(1, sRandom));
}

GSVertexBillboard VSBillboard(VSVertexBillboard input)
{
	GSVertexBillboard output;

	output.centerWorld = input.positionWorld;
	output.centerWorld.y += (input.sizeWorld.y * 0.40f);
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
			v[1].xyz = CalcWindTranslation(primID, v[1].xyz, gWindDirection, gWindStrength);
			v[3].xyz = CalcWindTranslation(primID, v[3].xyz, gWindDirection, gWindStrength);
		}

		//Transform quad vertices to world space and output them as a triangle strip.
		PSVertexBillboard gout;
		[unroll]
		for(int i = 0; i < 4; ++i)
		{
			gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
			gout.positionWorld = mul(v[i], gWorld).xyz;
			gout.normalWorld = up;

			if (gUVToggleByPID == 0 || fmod(primID, 2) < 1)
			{
				gout.tex = gQuadTexCL[i];
			}
			else
			{
				gout.tex = gQuadTexCR[i];
			}

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

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);

	float4 lightPositionLD = mul(float4(input.positionWorld, 1), gLightViewProjectionLD);
	float4 lightPositionHD = mul(float4(input.positionWorld, 1), gLightViewProjectionHD);

	ComputeLightsInput lInput;

	lInput.Ga = 0;
	lInput.dirLights = gDirLights;
	lInput.pointLights = gPointLights;
	lInput.spotLights = gSpotLights;
	lInput.dirLightsCount = gLightCount.x;
	lInput.pointLightsCount = gLightCount.y;
	lInput.spotLightsCount = gLightCount.z;
	lInput.fogStart = gFogStart;
	lInput.fogRange = gFogRange;
	lInput.fogColor = gFogColor;
	lInput.k = material;
	lInput.pPosition = input.positionWorld;
	lInput.pNormal = input.normalWorld;
	lInput.pColorDiffuse = textureColor;
	lInput.pColorSpecular = 1;
	lInput.ePosition = gEyePositionWorld;
	lInput.sLightPositionLD = lightPositionLD;
	lInput.sLightPositionHD = lightPositionHD;
	lInput.shadows = gShadows;
	lInput.shadowMapLD = gShadowMapLD;
	lInput.shadowMapHD = gShadowMapHD;
	lInput.lod = gLOD;

	float4 litColor = ComputeLights(lInput);

	float distToEye = length(gEyePositionWorld - input.positionWorld);

	float alpha = saturate(textureColor.a * (1.0f - (distToEye / gEndRadius) + 0.5f));

	return float4(litColor.rgb, alpha);
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
