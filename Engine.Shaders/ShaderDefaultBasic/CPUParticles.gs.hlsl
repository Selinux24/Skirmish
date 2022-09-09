#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerEmitter : register(b0)
{
    float gTotalTime;
    float gMaxDuration;
    float gMaxDurationRandomness;
    float PAD01;

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

cbuffer cbPerFrame : register(b1)
{
    PerFrame gPerFrame;
};

static float2 quadTexC[4] =
{
    float2(0.0f, 1.0f),
    float2(0.0f, 0.0f),
    float2(1.0f, 1.0f),
    float2(1.0f, 0.0f)
};

[maxvertexcount(4)]
void main(point GSCPUParticle input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSCPUParticle> outputStream)
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
    PSCPUParticle gout;
	[unroll]
    for (int i = 0; i < 4; ++i)
    {
        gout.positionHomogeneous = mul(v[i], gPerFrame.ViewProjection);
        gout.positionWorld = v[i].xyz;
        gout.tex = quadTexC[i];
        gout.color = color;
        gout.rotationWorld = rotationWorld;
        gout.primitiveID = primID;

        outputStream.Append(gout);
    }
}
