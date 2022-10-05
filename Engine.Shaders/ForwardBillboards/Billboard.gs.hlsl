#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncLights.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbBillboard : register(b1)
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

struct GSVertexBillboard
{
    float3 centerWorld : POSITION;
    float2 sizeWorld : SIZE;
    float4 tintColor : TINTCOLOR;
    Material material;
};

struct PSVertexBillboard
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    Material material;
    uint primitiveID : SV_PRIMITIVEID;
};

[maxvertexcount(4)]
void main(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard> outputStream)
{
    float3 look = gPerFrame.EyePosition - input[0].centerWorld;
    float radius = length(look);
    if ((gStartRadius == 0 || radius >= gStartRadius) && (gEndRadius == 0 || radius <= gEndRadius))
    {
        return;
    }
    
    //Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
    look.y = 0.0f; // y-axis aligned, so project to xz-plane
    look = normalize(look);
    float3 up = float3(0.0f, 1.0f, 0.0f);
    float3 right = cross(up, look);

	//Compute triangle strip vertices (quad) in world space.
    float2 halfSize = 0.5f * input[0].sizeWorld;
    float4 v[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
    BuildQuad(input[0].centerWorld, halfSize.x, halfSize.y, up, right, 0, v);

	//Transform quad vertices to world space and output them as a triangle strip.
	[unroll]
    for (int i = 0; i < 4; ++i)
    {
        PSVertexBillboard gout;
        
        gout.positionHomogeneous = mul(v[i], gPerFrame.ViewProjection);
        gout.positionWorld = v[i].xyz;
        gout.normalWorld = up;
        gout.tangentWorld = float3(1, 0, 0);
        gout.tex = BillboardTexCoords[i];
        gout.tintColor = input[0].tintColor;
        gout.material = input[0].material;
        gout.primitiveID = primID;

        outputStream.Append(gout);
    }
}
