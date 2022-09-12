#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

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

static float4 v[8] =
{
    float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
    float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0)
};
static float3 p[2] =
{
    float3(+gDelta.x, 0.0f, +gDelta.z),
    float3(-gDelta.x, 0.0f, -gDelta.z)
};

[maxvertexcount(8)]
void main(point GSVertexBillboard2 input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard2> outputStream)
{
    float3 position = input[0].centerWorld;
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

    float sRandom = gTextureRandom.SampleLevel(SamplerPoint, primID, 0).x;
    int index = 0;
    for (int g = 0; g < 2; g++)
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

	//Transform quad vertices to world space and output them as a triangle strip.
    PSVertexBillboard2 gout;
	[unroll]
    for (int i = 0; i < 8; ++i)
    {
        gout.positionHomogeneous = mul(v[i], gPerFrame.ViewProjection);
        gout.positionWorld = v[i].xyz;
        gout.normalWorld = up;
        gout.tangentWorld = float3(1, 0, 0);
        gout.tex = BillboardTexCoords[i];
        gout.tintColor = input[0].tintColor;
        gout.material = input[0].material;
        gout.primitiveID = primID;

        outputStream.Append(gout);

        if (i % 4 == 3)
        {
            outputStream.RestartStrip();
        }
    }
}
