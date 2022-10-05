#include "..\Lib\IncBuiltIn.hlsl"

cbuffer cbPerFrame : register(b0)
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

struct GSDecal
{
    float3 centerWorld : POSITION;
    float3 normalWorld : NORMAL;
    float4 rotationWorld : ROTATION;
    float2 sizeWorld : SIZE;
    float alpha : ALPHA;
};

struct PSDecal
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float4 rotationWorld : ROTATION;
    float alpha : ALPHA;
    float2 tex : TEXCOORD0;
    uint primitiveID : SV_PRIMITIVEID;
};

[maxvertexcount(4)]
void main(point GSDecal input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSDecal> outputStream)
{
    float3 centerWorld = input[0].centerWorld;
    float3 normalWorld = input[0].normalWorld;
    float2 sizeWorld = input[0].sizeWorld;
    float4 rotationWorld = input[0].rotationWorld;
    float alpha = input[0].alpha;
    
	//Compute the local coordinate system of the sprite relative to the normal world
    normalWorld = normalize(normalWorld);
    float3 unit = abs(normalWorld.y) > 0.9998 ? float3(1, 0, 0) : float3(0, 1, 0);
    float3 right = normalize(cross(unit, normalWorld));
    float3 up = normalize(cross(normalWorld, right));

	//Compute triangle strip vertices (quad) in world space.
    float halfWidth = 0.5f * sizeWorld.x;
    float halfHeight = 0.5f * sizeWorld.y;
    float4 v[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
    v[0] = float4(centerWorld + halfWidth * right - halfHeight * up, 1.0f);
    v[1] = float4(centerWorld + halfWidth * right + halfHeight * up, 1.0f);
    v[2] = float4(centerWorld - halfWidth * right - halfHeight * up, 1.0f);
    v[3] = float4(centerWorld - halfWidth * right + halfHeight * up, 1.0f);

	//Transform quad vertices to world space and output them as a triangle strip.
    PSDecal gout;
	[unroll]
    for (int i = 0; i < 4; ++i)
    {
        gout.positionHomogeneous = mul(v[i], gPerFrame.ViewProjection);
        gout.positionWorld = v[i].xyz;
        gout.rotationWorld = rotationWorld;
        gout.alpha = alpha;
        gout.tex = quadTexC[i];
        gout.primitiveID = primID;

        outputStream.Append(gout);
    }
}
