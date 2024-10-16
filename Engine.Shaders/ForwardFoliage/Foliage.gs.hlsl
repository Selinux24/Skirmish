#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerPatch : register(b1)
{
    float3 gPointOfView;
    float PAD12;
        
    float3 gWindDirection;
    float gWindStrength;

    float gStartRadius;
    float gEndRadius;
    uint gInstances;
    float PAD13;
    
    float3 gDelta;
    float gWindEffect;
};

Texture1D gTextureRandom : register(t0);

SamplerState SamplerPoint : register(s0)
{
    Filter = MIN_MAG_MIP_POINT;
};

static float3 up = float3(0, 1, 0);
static float3 tangent = float3(1, 0, 0);

static float4 v[16] =
{
    float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
    float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
    float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
    float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0)
};

static float3 p[4] =
{
    float3(+gDelta.x, 0.0f, +gDelta.z),
    float3(-gDelta.x, 0.0f, -gDelta.z),
    float3(+gDelta.x, 0.0f, -gDelta.z),
    float3(-gDelta.x, 0.0f, +gDelta.z)
};

inline void createPatches(uint primID, float3 position, float2 size)
{
    position.y += (size.y * (0.5f + gDelta.y));

    float len = length(gPointOfView - position);
    if ((gStartRadius > 0 && len < gStartRadius) || (gEndRadius > 0 && len > gEndRadius))
    {
        return;
    }
    
	//Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
    float3 look = gPerFrame.EyePosition - position;
    look.y = 0.0f; // y-axis aligned, so project to xz-plane
    look = normalize(look);
    float3 right = cross(up, look);

	//Compute triangle strip vertices (quad) in world space.
    float halfWidth = 0.5f * size.x;
    float halfHeight = 0.5f * size.y;

    float sRandom = gTextureRandom.SampleLevel(SamplerPoint, primID, 0).x;
    int index = 0;
    for (uint g = 0; g < gInstances; g++)
    {
        float4 tmp[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
        BuildQuad(position, halfWidth, halfHeight, up, right, p[g], tmp);

        if (gWindStrength > 0)
        {
            float time = gPerFrame.TotalTime * gWindEffect;
            tmp[1].xyz = CalcWindTranslation(time, sRandom, tmp[1].xyz, gWindDirection, gWindStrength);
            tmp[3].xyz = CalcWindTranslation(time, sRandom, tmp[3].xyz, gWindDirection, gWindStrength);
        }

        v[index++] = tmp[0];
        v[index++] = tmp[1];
        v[index++] = tmp[2];
        v[index++] = tmp[3];
    }
}

struct GSFoliage
{
    float3 positionWorld : POSITION;
    float2 sizeWorld : SIZE;
    Material material;
};

struct PSFoliage
{
    uint primitiveID : SV_PRIMITIVEID;
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
    float size : SIZE0;
    Material material;
};

[maxvertexcount(16)]
void main(point GSFoliage input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSFoliage> outputStream)
{
    createPatches(primID, input[0].positionWorld, input[0].sizeWorld);

	//Transform quad vertices to world space and output them as a triangle strip.
    PSFoliage gout;
    gout.normalWorld = up;
    gout.tangentWorld = tangent;
    gout.size = gEndRadius;
    gout.material = input[0].material;
    gout.primitiveID = primID;

	[unroll]
    for (uint i = 0; i < gInstances * 4; ++i)
    {
        gout.positionHomogeneous = mul(v[i], gPerFrame.ViewProjection);
        gout.positionWorld = v[i].xyz;
        gout.tex = BillboardTexCoords[i % 8];

        outputStream.Append(gout);

        if (i % 4 == 3)
        {
            outputStream.RestartStrip();
        }
    }
}
