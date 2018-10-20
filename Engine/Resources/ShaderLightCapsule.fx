#include "IncCommon.hlsl"

/////////////////////////////////////////////////////////////////////////////
// constants
/////////////////////////////////////////////////////////////////////////////
cbuffer cbCapsuleLightDomain : register(b0)
{
    float4x4 LightProjection : packoffset(c0);
    float HalfSegmentLen : packoffset(c4);
    float CapsuleRange : packoffset(c4.y);
}

cbuffer cbCapsuleLightPixel : register(b1)
{
    float3 CapsuleLightPos : packoffset(c0);
    float CapsuleLightRangeRcp : packoffset(c0.w);
    float3 CapsuleDir : packoffset(c1);
    float CapsuleLen : packoffset(c1.w);
    float3 CapsuleColor : packoffset(c2);
}

/////////////////////////////////////////////////////////////////////////////
// Vertex shader
/////////////////////////////////////////////////////////////////////////////
float4 CapsuleLightVS() : SV_Position
{
    return float4(0.0, 0.0, 0.0, 1.0);
}

/////////////////////////////////////////////////////////////////////////////
// Hull shader
/////////////////////////////////////////////////////////////////////////////
struct HS_CONSTANT_DATA_OUTPUT
{
    float Edges[4] : SV_TessFactor;
    float Inside[2] : SV_InsideTessFactor;
};

HS_CONSTANT_DATA_OUTPUT CapsuleLightConstantHS()
{
    HS_CONSTANT_DATA_OUTPUT Output;
	
    float tessFactor = 18.0;
    Output.Edges[0] = Output.Edges[1] = Output.Edges[2] = Output.Edges[3] = tessFactor;
    Output.Inside[0] = Output.Inside[1] = tessFactor;

    return Output;
}

struct HS_OUTPUT
{
    float4 CapsuleDir : POSITION;
};

static const float4 CapsuelDir[2] =
{
    float4(1.0, 1.0, 1.0, 1.0),
	float4(-1.0, 1.0, -1.0, 1.0)
};

[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[patchconstantfunc("CapsuleLightConstantHS")]
HS_OUTPUT CapsuleLightHS(uint PatchID : SV_PrimitiveID)
{
    HS_OUTPUT Output;

    Output.CapsuleDir = CapsuelDir[PatchID];

    return Output;
}

/////////////////////////////////////////////////////////////////////////////
// Domain Shader shader
/////////////////////////////////////////////////////////////////////////////
struct DS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 cpPos : TEXCOORD0;
};

#define CylinderPortion 0.2
#define SpherePortion   (1.0 - CylinderPortion)
#define ExpendAmount    (1.0 + CylinderPortion)

[domain("quad")]
DS_OUTPUT CapsuleLightDS(HS_CONSTANT_DATA_OUTPUT input, float2 UV : SV_DomainLocation, const OutputPatch<HS_OUTPUT, 4> quad)
{
	// Transform the UV's into clip-space
    float2 posClipSpace = UV.xy * float2(2.0, -2.0) + float2(-1.0, 1.0);

	// Find the vertex offsets based on the UV
    float2 posClipSpaceAbs = abs(posClipSpace.xy);
    float maxLen = max(posClipSpaceAbs.x, posClipSpaceAbs.y);

	// Force the cone vertices to the mesh edge
    float2 posClipSpaceNoCylAbs = saturate(posClipSpaceAbs * ExpendAmount);
    float maxLenNoCapsule = max(posClipSpaceNoCylAbs.x, posClipSpaceNoCylAbs.y);
    float2 posClipSpaceNoCyl = sign(posClipSpace.xy) * posClipSpaceNoCylAbs;

	// Convert the positions to half sphere with the cone vertices on the edge
    float3 halfSpherePos = normalize(float3(posClipSpaceNoCyl.xy, 1.0 - maxLenNoCapsule));

	// Find the offsets for the cone vertices (0 for cone base)
    float cylinderOffsetZ = saturate((maxLen * ExpendAmount - 1.0) / CylinderPortion);

	// Apply the range
    halfSpherePos *= CapsuleRange;

	// Offset the cone vertices to thier final position
    float4 posLS = float4(halfSpherePos.xy, halfSpherePos.z + HalfSegmentLen - cylinderOffsetZ * HalfSegmentLen, 1.0);

	// Move the vertex to the selected capsule side
    posLS *= quad[0].CapsuleDir;

	// Transform all the way to projected space and generate the UV coordinates
    DS_OUTPUT Output;
    Output.Position = mul(posLS, LightProjection);
    Output.cpPos = Output.Position.xy / Output.Position.w;

    return Output;
}

/////////////////////////////////////////////////////////////////////////////
// Pixel shader
/////////////////////////////////////////////////////////////////////////////
float3 CalcCapsule(float3 position, Material material)
{
    float3 ToEye = EyePosition - position;
   
   // Find the shortest distance between the pixel and capsules segment
    float3 ToCapsuleStart = position - CapsuleLightPos;
    float DistOnLine = dot(ToCapsuleStart, CapsuleDir) / CapsuleLen;
    DistOnLine = saturate(DistOnLine) * CapsuleLen;
    float3 PointOnLine = CapsuleLightPos + CapsuleDir * DistOnLine;
    float3 ToLight = PointOnLine - position;
    float DistToLight = length(ToLight);
   
   // Phong diffuse
    ToLight /= DistToLight; // Normalize
    float NDotL = saturate(dot(ToLight, material.normal));
    float3 finalColor = material.diffuseColor.rgb * NDotL;
   
   // Blinn specular
    ToEye = normalize(ToEye);
    float3 HalfWay = normalize(ToEye + ToLight);
    float NDotH = saturate(dot(HalfWay, material.normal));
    finalColor += pow(NDotH, material.specPow) * material.specIntensity;
   
   // Attenuation
    float DistToLightNorm = 1.0 - saturate(DistToLight * CapsuleLightRangeRcp);
    float Attn = DistToLightNorm * DistToLightNorm;
    finalColor *= CapsuleColor.rgb * Attn;
   
    return finalColor;
}

float4 CapsuleLightPS(DS_OUTPUT In) : SV_TARGET
{
	// Unpack the GBuffer
    SURFACE_DATA gbd = UnpackGBuffer_Loc(In.Position.xy);
	
	// Convert the data into the material structure
    Material mat;
    MaterialFromGBuffer(gbd, mat);

	// Reconstruct the world position
    float3 position = CalcWorldPos(In.cpPos, gbd.LinearDepth);

	// Calculate the light contribution
    float3 finalColor = CalcCapsule(position, mat);

	// Return the final color
    return float4(finalColor, 1.0);
}

technique11 CapsuleLight
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, CapsuleLightVS()));
        SetHullShader(CompileShader(hs_5_0, CapsuleLightHS()));
        SetDomainShader(CompileShader(ds_5_0, CapsuleLightDS()));
        SetPixelShader(CompileShader(ps_5_0, CapsuleLightPS()));
    }
}
