#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldInverse;
	float4x4 gWorldViewProjection;
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
	Material gMaterial;
};

cbuffer cbSkinned : register (b2)
{
	float4x4 gBoneTransforms[96];
};

Texture2D gTexture;

PSVertexPositionColor VSPositionColor(VSVertexPositionColor input)
{
    PSVertexPositionColor output = (PSVertexPositionColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.color = input.color;
    
    return output;
}

float4 PSPositionColor(PSVertexPositionColor input) : SV_TARGET
{
	float4 litColor = input.color;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	return litColor;
}

PSVertexPositionNormalColor VSPositionNormalColor(VSVertexPositionNormalColor input)
{
    PSVertexPositionNormalColor output = (PSVertexPositionNormalColor)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.color = input.color;
    
	output.normalWorld = normalize(output.normalWorld);

    return output;
}

float4 PSPositionNormalColor(PSVertexPositionNormalColor input) : SV_TARGET
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

	LightOutput lOutput = ComputeLights(lInput);

	float4 litColor = lOutput.ambient + lOutput.diffuse + lOutput.specular;
	
	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a = gMaterial.Diffuse.a;
	
	return litColor;
}

PSVertexPositionTexture VSPositionTexture(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
    
    return output;
}

float4 PSPositionTexture(PSVertexPositionTexture input) : SV_TARGET
{
    float4 litColor = gTexture.Sample(samAnisotropic, input.tex);

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a *= gMaterial.Diffuse.a;

	return litColor;
}

PSVertexPositionNormalTexture VSPositionNormalTexture(VSVertexPositionNormalTexture input)
{
    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gWorldInverse));
	output.tex = input.tex;
    
    return output;
}

float4 PSPositionNormalTexture(PSVertexPositionNormalTexture input) : SV_TARGET
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

	LightOutput lOutput = ComputeLights(lInput);

	float4 textureColor = gTexture.Sample(samAnisotropic, input.tex);

	float4 litColor = textureColor * (lOutput.ambient + lOutput.diffuse) + lOutput.specular;

	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a = gMaterial.Diffuse.a * textureColor.a;

	return litColor;
}

PSVertexPositionNormalTexture VSPositionNormalTextureSkinned(VSVertexPositionNormalTextureSkinned input)
{
	PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture)0;
	
	float weights[4] = {0.0f, 0.0f, 0.0f, 0.0f};
	weights[0] = input.weights.x;
	weights[1] = input.weights.y;
	weights[2] = input.weights.z;
	weights[3] = 1.0f - weights[0] - weights[1] - weights[2];
	
	float3 posL = float3(0.0f, 0.0f, 0.0f);
	float3 normalL = float3(0.0f, 0.0f, 0.0f);
	
	for(int i = 0; i < 4; ++i)
	{
		posL += weights[i] * mul(float4(input.positionLocal, 1.0f), gBoneTransforms[input.boneIndices[i]]).xyz;
		normalL += weights[i] * mul(input.normalLocal, (float3x3)gBoneTransforms[input.boneIndices[i]]);
	}
	
	output.positionHomogeneous = mul(float4(posL, 1.0f), gWorldViewProjection);
	output.positionWorld = mul(float4(posL, 1.0f), gWorld).xyz;
	output.normalWorld = normalize(mul(normalL, (float3x3)gWorldInverse));
	output.tex = input.tex;
	
	return output;
}

float4 PSPositionNormalTextureSkinned(PSVertexPositionNormalTexture input) : SV_TARGET
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

	LightOutput lOutput = ComputeLights(lInput);

	float4 textureColor = gTexture.Sample(samAnisotropic, input.tex);

	float4 litColor = textureColor * (lOutput.ambient + lOutput.diffuse) + lOutput.specular;

	if(gFogRange > 0)
	{
		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor);
	}

	litColor.a = gMaterial.Diffuse.a * textureColor.a;

	return litColor;
}

technique11 PositionColor
{
	pass P0
	{
		SetRasterizerState(Solid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}

technique11 PositionNormalColor
{
	pass P0
	{
		SetRasterizerState(Solid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalColor()));
	}
}

technique11 PositionTexture
{
	pass P0
	{
		SetRasterizerState(Solid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}

technique11 PositionNormalTexture
{
	pass P0
	{
		SetRasterizerState(Solid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTexture()));
	}
}

technique11 PositionNormalTextureSkinned
{
	pass P0
	{
		SetRasterizerState(Solid);
		SetVertexShader(CompileShader(vs_5_0, VSPositionNormalTextureSkinned()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionNormalTextureSkinned()));
	}
}
