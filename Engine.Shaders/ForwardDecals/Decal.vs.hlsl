#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerDecal : register(b1)
{
    bool gRotation;
    uint gTextureCount;
    uint2 PAD11;
    
    float4 gTintColor;
}

float4 computeDecalRotation(float rotation)
{
    float rot = rotation % TWO_PI;
    float c = cos(rot);
    float s = sin(rot);
    
    float4 rotationMatrix = float4(c, -s, s, c);
    
    rotationMatrix *= 0.5f;
    rotationMatrix += 0.5f;
    
    return rotationMatrix;
}

struct VSVertexDecal
{
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float2 sizeWorld : SIZE;
    float startTime : START_TIME;
    float maxAge : MAX_AGE;
};

struct GSDecal
{
    float3 centerWorld : POSITION;
    float3 normalWorld : NORMAL;
    float4 rotationWorld : ROTATION;
    float2 sizeWorld : SIZE;
    float alpha : ALPHA;
};

GSDecal main(VSVertexDecal input)
{
    GSDecal output;

    //Move to zero age and normalize from 0 to 1
    float currentAge = gPerFrame.TotalTime - input.startTime;
    float normalizedAge = saturate(currentAge / input.maxAge);
    float4 rotation = 0;
    if (gRotation == true)
    {
        rotation = computeDecalRotation(input.startTime);
    }
    
    output.centerWorld = input.positionWorld;
    output.normalWorld = input.normalWorld;
    output.rotationWorld = rotation;
    output.sizeWorld = input.sizeWorld;
    output.alpha = 1 * (1.0 - normalizedAge);

    return output;
}
