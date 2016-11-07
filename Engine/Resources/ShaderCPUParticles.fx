#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float3 gEyePositionWorld;
	float gTotalTime;
	uint gTextureCount;

	float gViewportHeight;
	float gDuration;
	float gDurationRandomness;
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
    float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge / 2;
     
    position += normalize(velocity) * velocityIntegral * gDuration;
    position += gGravity * age * normalizedAge;
    
    return position;
}
float ComputeParticleSize(float4 projectedPosition, float randomValue, float normalizedAge)
{
    float startSize = lerp(gStartSize.x, gStartSize.y, randomValue);
    float endSize = lerp(gEndSize.x, gEndSize.y, randomValue);
    
    float size = lerp(startSize, endSize, normalizedAge);
    
    return size * gWorldViewProjection._m11 / projectedPosition.w * gViewportHeight * 0.5f;
}
float4 ComputeParticleColor(float4 projectedPosition, float randomValue, float normalizedAge)
{
    float4 color = lerp(gMinColor, gMaxColor, randomValue);
    
    color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7;

	return color;
}
float4 ComputeParticleRotation(float randomValue, float age)
{    
    float rotateSpeed = lerp(gRotateSpeed.x, gRotateSpeed.y, randomValue);
    
    float rotation = rotateSpeed * age;

    float c = cos(rotation);
    float s = sin(rotation);
    
    float4 rotationMatrix = float4(c, -s, s, c);
    
    rotationMatrix *= 0.5;
    rotationMatrix += 0.5;
    
    return rotationMatrix;
}

GSCPUParticle VSParticle(VSVertexCPUParticle input)
{
	GSCPUParticle output;

	output.positionWorld = input.positionWorld;
	output.velocityWorld = input.velocityWorld;
	output.color = input.color;
	output.energy = input.energy;

	return output;
}

[maxvertexcount(4)]
void GSParticle(point GSCPUParticle input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSCPUParticle> outputStream)
{
	float age = gTotalTime - input[0].energy;
	age *= 1 + input[0].color.x * gDurationRandomness;
	float normalizedAge = saturate(age / gDuration);

	float3 centerWorld = ComputeParticlePosition(input[0].positionWorld, input[0].velocityWorld, age, normalizedAge);
	float4 projPosition = mul(float4(centerWorld, 1), gWorldViewProjection);
	float2 sizeWorld = ComputeParticleSize(projPosition, input[0].color.y, normalizedAge);
	float4 color = ComputeParticleColor(projPosition, input[0].color.z, normalizedAge);
	float4 rotationWorld = ComputeParticleRotation(input[0].color.w, age);

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
		gout.rotationWorld = rotationWorld;
		gout.tex = gQuadTexC[i];
		gout.color = color;
		gout.primitiveID = primID;

		outputStream.Append(gout);
	}
}

float4 PSForwardParticle(PSCPUParticle input) : SV_Target
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw) * input.color;
	clip(textureColor.a - 0.05f);

	return textureColor;
}
GBufferPSOutput PSDeferredParticle(PSCPUParticle input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw) * input.color;
	clip(textureColor.a - 0.05f);

	output.color = textureColor;
	output.normal = float4(0, 0, 0, 0);
	output.depth = float4(input.positionWorld, 0);

    return output;
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
technique11 DeferredParticle
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSParticle()));
		SetGeometryShader(CompileShader(gs_5_0, GSParticle()));
		SetPixelShader(CompileShader(ps_5_0, PSDeferredParticle()));
	}
}