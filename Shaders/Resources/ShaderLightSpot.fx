#include "..\Lib\IncGBufferLights.hlsl"

Texture2D<float> SpotShadowMapTexture : register(t4);

/////////////////////////////////////////////////////////////////////////////
// constants
/////////////////////////////////////////////////////////////////////////////
cbuffer cbSpotLightDomain : register(b0)
{
    float4x4 LightProjection : packoffset(c0);
    float SinAngle : packoffset(c4);
    float CosAngle : packoffset(c4.y);
}

cbuffer cbSpotLightPixel : register(b1)
{
    float3 SpotLightPos : packoffset(c0);
    float SpotLightRangeRcp : packoffset(c0.w);
    float3 SpotDirToLight : packoffset(c1);
    float SpotCosOuterCone : packoffset(c1.w);
    float3 SpotColor : packoffset(c2);
    float SpotCosConeAttRange : packoffset(c2.w);
    float4x4 ToShadowmap : packoffset(c3);
}

/////////////////////////////////////////////////////////////////////////////
// Vertex shader
/////////////////////////////////////////////////////////////////////////////
float4 SpotLightVS() : SV_Position
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

HS_CONSTANT_DATA_OUTPUT SpotLightConstantHS()
{
    HS_CONSTANT_DATA_OUTPUT Output;
	
    float tessFactor = 18.0;
    Output.Edges[0] = Output.Edges[1] = Output.Edges[2] = Output.Edges[3] = tessFactor;
    Output.Inside[0] = Output.Inside[1] = tessFactor;

    return Output;
}

struct HS_OUTPUT
{
    float3 Position : POSITION;
};

[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[patchconstantfunc("SpotLightConstantHS")]
HS_OUTPUT SpotLightHS()
{
    HS_OUTPUT Output;

    Output.Position = float3(0.0, 0.0, 0.0);

    return Output;
}

/////////////////////////////////////////////////////////////////////////////
// Domain Shader shader
/////////////////////////////////////////////////////////////////////////////
struct DS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 PositionXYW : TEXCOORD0;
};

#define CylinderPortion 0.2
#define ExpendAmount    (1.0 + CylinderPortion)

[domain("quad")]
DS_OUTPUT SpotLightDS(HS_CONSTANT_DATA_OUTPUT input, float2 UV : SV_DomainLocation, const OutputPatch<HS_OUTPUT, 4> quad)
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

	// Scale the sphere to the size of the cones rounded base
    halfSpherePos = normalize(float3(halfSpherePos.xy * SinAngle, CosAngle));

	// Find the offsets for the cone vertices (0 for cone base)
    float cylinderOffsetZ = saturate((maxLen * ExpendAmount - 1.0) / CylinderPortion);

	// Offset the cone vertices to thier final position
    float4 posLS = float4(halfSpherePos.xy * (1.0 - cylinderOffsetZ), halfSpherePos.z - cylinderOffsetZ * CosAngle, 1.0);

	// Transform all the way to projected space and generate the UV coordinates
    DS_OUTPUT Output;
    Output.Position = mul(posLS, LightProjection);
    Output.PositionXYW = Output.Position.xyw;

    return Output;
}

/////////////////////////////////////////////////////////////////////////////
// Pixel shader
/////////////////////////////////////////////////////////////////////////////

// Shadow PCF calculation helper function
float SpotShadowPCF(float3 position)
{
	// Transform the world position to shadow projected space
    float4 posShadowMap = mul(float4(position, 1.0), ToShadowmap);

	// Transform the position to shadow clip space
    float3 UVD = posShadowMap.xyz / posShadowMap.w;

	// Convert to shadow map UV values
    UVD.xy = 0.5 * UVD.xy + 0.5;
    UVD.y = 1.0 - UVD.y;

	// Compute the hardware PCF value
    return SpotShadowMapTexture.SampleCmpLevelZero(PCFSampler, UVD.xy, UVD.z);
}

float3 CalcSpot(float3 position, Material material, bool bUseShadow)
{
    float3 ToLight = SpotLightPos - position;
    float3 ToEye = EyePosition - position;
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

	// Cone attenuation
    float cosAng = dot(SpotDirToLight, ToLight);
    float conAtt = saturate((cosAng - SpotCosOuterCone) / SpotCosConeAttRange);
    conAtt *= conAtt;
   
    float shadowAtt;
    if (bUseShadow)
    {
		// Find the shadow attenuation for the pixels world position
        shadowAtt = SpotShadowPCF(position);
    }
    else
    {
		// No shadow attenuation
        shadowAtt = 1.0;
    }

	// Attenuation
    float DistToLightNorm = 1.0 - saturate(DistToLight * SpotLightRangeRcp);
    float Attn = DistToLightNorm * DistToLightNorm;
    finalColor *= SpotColor.rgb * Attn * conAtt * shadowAtt;
   
	// Return the fianl color
    return finalColor;
}

float4 SpotLightCommonPS(DS_OUTPUT In, bool bUseShadow) : SV_TARGET
{
	// Unpack the GBuffer
    SURFACE_DATA gbd = UnpackGBuffer_Loc(In.Position.xy);
	
	// Convert the data into the material structure
    Material mat;
    MaterialFromGBuffer(gbd, mat);

	// Reconstruct the world position
    float3 position = CalcWorldPos(In.PositionXYW.xy / In.PositionXYW.z, gbd.LinearDepth);

	// Calculate the light contribution
    float3 finalColor = CalcSpot(position, mat, bUseShadow);

	// Return the final color
    return float4(finalColor, 1.0);
}

float4 SpotLightPS(DS_OUTPUT In) : SV_TARGET
{
    return SpotLightCommonPS(In, false);
}

float4 SpotLightShadowPS(DS_OUTPUT In) : SV_TARGET
{
    return SpotLightCommonPS(In, true);
}

technique11 SpotLight
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, SpotLightVS()));
        SetHullShader(CompileShader(hs_5_0, SpotLightHS()));
        SetDomainShader(CompileShader(ds_5_0, SpotLightDS()));
        SetPixelShader(CompileShader(ps_5_0, SpotLightPS()));
    }
}

technique11 SpotLightShadow
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, SpotLightVS()));
        SetHullShader(CompileShader(hs_5_0, SpotLightHS()));
        SetDomainShader(CompileShader(ds_5_0, SpotLightDS()));
        SetPixelShader(CompileShader(ps_5_0, SpotLightShadowPS()));
    }
}
