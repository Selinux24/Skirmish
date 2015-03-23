#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorldViewProjection;
	float4x4 gShadowTransform; 
	DirectionalLight gDirLights[3];
	PointLight gPointLight;
	SpotLight gSpotLight;
	float3 gEyePositionWorld;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
	float gEnableShadows;
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
Texture2D gShadowMap;

GSVertexBillboard VSBillboard(VSVertexBillboard input)
{
	GSVertexBillboard output;

	output.centerWorld = input.positionWorld;
	output.sizeWorld = input.sizeWorld;

	return output;
}

[maxvertexcount(4)]
void GSBillboard(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard> outputStream)
{
	//Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
	float3 up = float3(0.0f, 1.0f, 0.0f);
	float3 look = gEyePositionWorld - input[0].centerWorld;
	look.y = 0.0f; // y-axis aligned, so project to xz-plane
	look = normalize(look);
	float3 right = cross(up, look);

	//Compute triangle strip vertices (quad) in world space.
	float halfWidth = 0.5f * input[0].sizeWorld.x;
	float halfHeight = 0.5f * input[0].sizeWorld.y;
	float4 v[4];
	v[0] = float4(input[0].centerWorld + halfWidth * right - halfHeight * up, 1.0f);
	v[1] = float4(input[0].centerWorld + halfWidth * right + halfHeight * up, 1.0f);
	v[2] = float4(input[0].centerWorld - halfWidth * right - halfHeight * up, 1.0f);
	v[3] = float4(input[0].centerWorld - halfWidth * right + halfHeight * up, 1.0f);

	//Transform quad vertices to world space and output them as a triangle strip.
	PSVertexBillboard gout;
	[unroll]
	for(int i = 0; i < 4; ++i)
	{
		gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
		gout.positionWorld = v[i].xyz;
		gout.shadowHomogeneous = mul(v[i], gShadowTransform);
		gout.normalWorld = up;
		gout.tex = gQuadTexC[i];
		gout.primitiveID = primID;

		outputStream.Append(gout);
	}
}

float4 PSBillboard(PSVertexBillboard input) : SV_Target
{
	float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
	float distToEye = length(toEyeWorld);
	toEyeWorld /= distToEye;

	LightInput lInput = (LightInput)0;
	lInput.toEyeWorld = toEyeWorld;
	lInput.positionWorld = input.positionWorld;
	lInput.normalWorld = input.normalWorld;
	lInput.material = gMaterial;
	lInput.dirLights = gDirLights;
	lInput.pointLight = gPointLight;
	lInput.spotLight = gSpotLight;
	lInput.enableShadows = gEnableShadows;
	lInput.shadowPosition = input.shadowHomogeneous;

	LightOutput lOutput = ComputeLights(lInput, gShadowMap);

	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw);
	clip(textureColor.a - 0.05f);

	float4 litColor = textureColor * (lOutput.ambient + lOutput.diffuse) + lOutput.specular;

	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a = gMaterial.Diffuse.a * textureColor.a;

	return litColor;
}

technique11 Billboard
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSBillboard()));
		SetGeometryShader(CompileShader(gs_5_0, GSBillboard()));
		SetPixelShader(CompileShader(ps_5_0, PSBillboard()));
	}
}
