#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float gViewportHeight;
	float3 gEyePositionWorld;
	float gTotalTime;
	uint gTextureCount;

	float gMaxDuration;
	float gMaxDurationRandomness;
	float gEndVelocity;
	float3 gGravity;
	float2 gStartSize;
	float2 gEndSize;
	float4 gMinColor;
	float4 gMaxColor;
	float2 gRotateSpeed;
};
cbuffer cbFixed : register (b1)
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

float3 ComputeParticlePosition(float3 position, float3 velocity, float age, float normalizedAge)
{
    float startVelocity = length(velocity);
    float endVelocity = startVelocity * gEndVelocity;
    float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge * 0.5f;
     
    float3 p = (normalize(velocity) * velocityIntegral) + (gGravity * age * normalizedAge);
    
    return position + p;
}
float2 ComputeParticleSize(float randomValue, float normalizedAge)
{
    float startSize = lerp(gStartSize.x, gStartSize.y, randomValue);
    float endSize = lerp(gEndSize.x, gEndSize.y, randomValue);
    
    float size = lerp(startSize, endSize, normalizedAge);
    
    return float2(size, size);
}
float4 ComputeParticleColor(float randomValue, float normalizedAge)
{
    float4 color = lerp(gMinColor, gMaxColor, randomValue);
    
    color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7f;

	return color;
}
float4 ComputeParticleRotation(float randomValue, float age)
{    
    float rotateSpeed = lerp(gRotateSpeed.x, gRotateSpeed.y, randomValue);
    
    float rotation = rotateSpeed * age;

    float c = cos(rotation);
    float s = sin(rotation);
    
    float4 rotationMatrix = float4(c, -s, s, c);
    
    rotationMatrix *= 0.5f;
    rotationMatrix += 0.5f;
    
    return rotationMatrix;
}

GSCPUParticle VSParticle(VSVertexCPUParticle input)
{
	GSCPUParticle output;

	float age = gTotalTime - input.maxAge;
	age *= 1.0f + input.color.x * gMaxDurationRandomness;
	float normalizedAge = saturate(age / gMaxDuration);

	output.centerWorld = ComputeParticlePosition(input.positionWorld, input.velocityWorld, age, normalizedAge);
	output.sizeWorld = ComputeParticleSize(input.color.y, normalizedAge);
	output.color = ComputeParticleColor(input.color.z, normalizedAge);
	output.rotationWorld = ComputeParticleRotation(input.color.w, age);

	output.centerWorld.y += (output.sizeWorld.y * 0.5f);

	return output;
}

[maxvertexcount(4)]
void GSParticle(point GSCPUParticle input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSCPUParticle> outputStream)
{
	float3 centerWorld = input[0].centerWorld;
	float2 sizeWorld = input[0].sizeWorld;
	float4 color = input[0].color;
	float4 rotationWorld = input[0].rotationWorld;

	//Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
	float3 look = gEyePositionWorld - centerWorld;
	look.y = 0.0f; // y-axis aligned, so project to xz-plane
	look = normalize(look);
	float3 up = float3(0.0f, 1.0f, 0.0f);
	float3 right = cross(up, look);

	//Compute triangle strip vertices (quad) in world space.
	float halfWidth = 0.5f * sizeWorld.x;
	float halfHeight = 0.5f * sizeWorld.y;
	float4 v[4] = {float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)};
	v[0] = float4(centerWorld + halfWidth * right - halfHeight * up, 1.0f);
	v[1] = float4(centerWorld + halfWidth * right + halfHeight * up, 1.0f);
	v[2] = float4(centerWorld - halfWidth * right - halfHeight * up, 1.0f);
	v[3] = float4(centerWorld - halfWidth * right + halfHeight * up, 1.0f);

	//Transform quad vertices to world space and output them as a triangle strip.
	PSCPUParticle gout;
	[unroll]
	for(int i = 0; i < 4; ++i)
	{
		gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
		gout.positionWorld = mul(v[i], gWorld).xyz;
		gout.tex = gQuadTexC[i];
		gout.color = color;
		gout.rotationWorld = rotationWorld;
		gout.primitiveID = primID;

		outputStream.Append(gout);
	}
}

float4 PSForwardParticle(PSCPUParticle input) : SV_Target
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	return gTextureArray.Sample(SamplerLinear, uvw) * input.color;
}

technique11 ForwardParticle
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSParticle()));
		SetGeometryShader(CompileShader(gs_5_0, GSParticle()));
		SetPixelShader(CompileShader(ps_5_0, PSForwardParticle()));
	}
}
