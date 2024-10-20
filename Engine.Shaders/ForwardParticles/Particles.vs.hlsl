#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerEmitter : register(b0)
{
    float gMaxDuration;
    float gMaxDurationRandomness;
    float gTotalTime;
    float gElapsedTime;

    bool gRotation;
    float2 gRotateSpeed;
    uint gTextureCount;

    float3 gGravity;
    float gEndVelocity;

    float2 gStartSize;
    float2 gEndSize;
    float4 gMinColor;
    float4 gMaxColor;
}

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
    
    color.a *= normalizedAge * (1 - normalizedAge) * (1 - normalizedAge) * 6.7f;

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

struct VSParticle
{
    float3 position : POSITION;
    float3 velocity : VELOCITY;
    float4 random : RANDOM;
    float maxAge : MAX_AGE;
};

struct GSParticle
{
    float3 centerWorld : POSITION;
    float2 sizeWorld : SIZE;
    float4 color : COLOR;
    float4 rotationWorld : ROTATION;
};

GSParticle main(VSParticle input)
{
    GSParticle output;

    float age = gTotalTime - input.maxAge;
    age *= 1.0f + input.random.x * gMaxDurationRandomness;
    float normalizedAge = saturate(age / gMaxDuration);

    output.centerWorld = ComputeParticlePosition(input.position, input.velocity, age, normalizedAge);
    output.sizeWorld = ComputeParticleSize(input.random.y, normalizedAge);
    output.color = ComputeParticleColor(input.random.z, normalizedAge);
    output.rotationWorld = ComputeParticleRotation(input.random.w, age);

    output.centerWorld.y += (output.sizeWorld.y * 0.5f);

    return output;
}
