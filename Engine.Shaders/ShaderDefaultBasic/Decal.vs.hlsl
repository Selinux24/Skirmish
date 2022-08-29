#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorld;
    float4x4 gWorldViewProjection;
};

cbuffer cbPerDecal : register(b1)
{
    float gTotalTime;
    bool gRotation;
    uint gTextureCount;
    uint PAD11;
    
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

GSDecal main(VSVertexDecal input)
{
    GSDecal output;

    //Move to zero age and normalize from 0 to 1
    float currentAge = gTotalTime - input.startTime;
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
