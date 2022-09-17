#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerEmitter : register(b1)
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

static float2 quadTexC[4] =
{
    float2(0.0f, 1.0f),
    float2(0.0f, 0.0f),
    float2(1.0f, 1.0f),
    float2(1.0f, 0.0f)
};

struct GSParticle
{
    float3 centerWorld : POSITION;
    float2 sizeWorld : SIZE;
    float4 color : COLOR;
    float4 rotationWorld : ROTATION;
};

struct PSParticle
{
    uint primitiveID : SV_PRIMITIVEID;
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float4 rotationWorld : ROTATION;
    float2 tex : TEXCOORD0;
    float4 color : COLOR0;
};

[maxvertexcount(4)]
void main(point GSParticle input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSParticle> outputStream)
{
    float3 centerWorld = input[0].centerWorld;
    float2 sizeWorld = input[0].sizeWorld;
    float4 color = input[0].color;
    float4 rotationWorld = input[0].rotationWorld;

	//Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
    float3 look = gPerFrame.EyePosition - centerWorld;
    look.y = 0.0f; // y-axis aligned, so project to xz-plane
    look = normalize(look);
    float3 up = float3(0.0f, 1.0f, 0.0f);
    float3 right = cross(up, look);

	//Compute triangle strip vertices (quad) in world space.
    float halfWidth = 0.5f * sizeWorld.x;
    float halfHeight = 0.5f * sizeWorld.y;
    float4 v[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
    v[0] = float4(centerWorld + halfWidth * right - halfHeight * up, 1.0f);
    v[1] = float4(centerWorld + halfWidth * right + halfHeight * up, 1.0f);
    v[2] = float4(centerWorld - halfWidth * right - halfHeight * up, 1.0f);
    v[3] = float4(centerWorld - halfWidth * right + halfHeight * up, 1.0f);

	//Transform quad vertices to world space and output them as a triangle strip.
    PSParticle gout;
    gout.primitiveID = primID;
    gout.color = color;
    gout.rotationWorld = rotationWorld;

    [unroll]
    for (int i = 0; i < 4; ++i)
    {
        gout.positionHomogeneous = mul(v[i], gPerFrame.ViewProjection);
        gout.positionWorld = v[i].xyz;
        gout.tex = quadTexC[i];

        outputStream.Append(gout);
    }
}
