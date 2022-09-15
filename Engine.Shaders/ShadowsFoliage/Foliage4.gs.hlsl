#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerMaterial : register(b1)
{
    float4 gTintColor;
    
    uint gMaterialIndex;
    uint gTextureCount;
    uint gNormalMapCount;
    uint PAD11;

    float gStartRadius;
    float gEndRadius;
    float2 PAD12;
};

cbuffer cbPerPatch : register(b2)
{
    float3 gWindDirection;
    float gWindStrength;
    
    float3 gDelta;
    float gWindEffect;
};

Texture1D gTextureRandom : register(t0);

SamplerState SamplerPoint : register(s0)
{
    Filter = MIN_MAG_MIP_POINT;
};

struct Foliage
{
    float3 positionWorld : POSITION;
    float2 sizeWorld : SIZE;
};

struct PSFoliage
{
    float4 positionHomogeneous : SV_POSITION;
    float4 depth : TEXCOORD0;
    float2 tex : TEXCOORD1;
    uint primitiveID : SV_PRIMITIVEID;
};

[maxvertexcount(4)]
void main(point Foliage input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSFoliage> outputStream)
{
    float3 position = input[0].positionWorld;
    position.y += (input[0].sizeWorld.y * (0.5f + gDelta.y));

    float3 look = gPerFrame.EyePosition - position;
    float len = length(look);
    if ((gStartRadius > 0 && len < gStartRadius) || (gEndRadius > 0 && len > gEndRadius))
    {
        return;
    }
    
    //Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
    look.y = 0.0f; // y-axis aligned, so project to xz-plane
    look = normalize(look);
    float3 up = float3(0.0f, 1.0f, 0.0f);
    float3 right = cross(up, look);

	//Compute triangle strip vertices (quad) in world space.
    float halfWidth = 0.5f * input[0].sizeWorld.x;
    float halfHeight = 0.5f * input[0].sizeWorld.y;
    float4 v[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
    BuildQuad(position, halfWidth, halfHeight, up, right, 0, v);

    if (gWindStrength > 0)
    {
        float sRandom = gTextureRandom.SampleLevel(SamplerPoint, primID, 0).x;
        float time = gPerFrame.TotalTime * gWindEffect;
        v[1].xyz = CalcWindTranslation(time, sRandom, v[1].xyz, gWindDirection, gWindStrength);
        v[3].xyz = CalcWindTranslation(time, sRandom, v[3].xyz, gWindDirection, gWindStrength);
    }

	//Transform quad vertices to world space and output them as a triangle strip.
    PSFoliage gout;
	[unroll]
    for (int i = 0; i < 4; ++i)
    {
        gout.positionHomogeneous = mul(v[i], gPerFrame.ViewProjection);
        gout.depth = v[i];
        gout.tex = BillboardTexCoords[i];
        gout.primitiveID = primID;

        outputStream.Append(gout);
    }
}
