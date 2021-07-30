#include "..\Lib\IncCommon.hlsl"

TextureCube<float> PointShadowMapTexture : register(t4);

/////////////////////////////////////////////////////////////////////////////
// constants
/////////////////////////////////////////////////////////////////////////////
cbuffer cbPointLightDomain : register(b0)
{
    float4x4 LightProjection : packoffset(c0);
}

cbuffer cbPointLightPixel : register(b1)
{
    float3 PointLightPos : packoffset(c0);
    float PointLightRangeRcp : packoffset(c0.w);
    float3 PointColor : packoffset(c1);
    float2 LightPerspectiveValues : packoffset(c2);
}

/////////////////////////////////////////////////////////////////////////////
// Vertex shader
/////////////////////////////////////////////////////////////////////////////
float4 PointLightVS() : SV_Position
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

HS_CONSTANT_DATA_OUTPUT PointLightConstantHS()
{
    HS_CONSTANT_DATA_OUTPUT Output;
	
    float tessFactor = 18.0;
    Output.Edges[0] = Output.Edges[1] = Output.Edges[2] = Output.Edges[3] = tessFactor;
    Output.Inside[0] = Output.Inside[1] = tessFactor;

    return Output;
}

struct HS_OUTPUT
{
    float3 HemiDir : POSITION;
};

static const float3 HemilDir[2] =
{
    float3(1.0, 1.0, 1.0),
	float3(-1.0, 1.0, -1.0)
};

[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[patchconstantfunc("PointLightConstantHS")]
HS_OUTPUT PointLightHS(uint PatchID : SV_PrimitiveID)
{
    HS_OUTPUT Output;

    Output.HemiDir = HemilDir[PatchID];

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

[domain("quad")]
DS_OUTPUT PointLightDS(HS_CONSTANT_DATA_OUTPUT input, float2 UV : SV_DomainLocation, const OutputPatch<HS_OUTPUT, 4> quad)
{
	// Transform the UV's into clip-space
    float2 posClipSpace = UV.xy * 2.0 - 1.0;

	// Find the absulate maximum distance from the center
    float2 posClipSpaceAbs = abs(posClipSpace.xy);
    float maxLen = max(posClipSpaceAbs.x, posClipSpaceAbs.y);

	// Generate the final position in clip-space
    float3 normDir = normalize(float3(posClipSpace.xy, (maxLen - 1.0)) * quad[0].HemiDir);
    float4 posLS = float4(normDir.xyz, 1.0);
	
	// Transform all the way to projected space
    DS_OUTPUT Output;
    Output.Position = mul(posLS, LightProjection);

	// Store the clip space position
    Output.PositionXYW = Output.Position.xyw;

    return Output;
}

/////////////////////////////////////////////////////////////////////////////
// Pixel shader
/////////////////////////////////////////////////////////////////////////////
float PointShadowPCF(float3 ToPixel)
{
    float3 ToPixelAbs = abs(ToPixel);
    float Z = max(ToPixelAbs.x, max(ToPixelAbs.y, ToPixelAbs.z));
    float Depth = (LightPerspectiveValues.x * Z + LightPerspectiveValues.y) / Z;
    return PointShadowMapTexture.SampleCmpLevelZero(PCFSampler, ToPixel, Depth);
}

float3 CalcPoint(float3 position, Material material, bool bUseShadow)
{
    float3 ToLight = PointLightPos - position;
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

    float shadowAtt;
    if (bUseShadow)
    {
		// Find the shadow attenuation for the pixels world position
        shadowAtt = PointShadowPCF(position - PointLightPos);
    }
    else
    {
		// No shadow attenuation
        shadowAtt = 1.0;
    }

   // Attenuation
    float DistToLightNorm = 1.0 - saturate(DistToLight * PointLightRangeRcp);
    float Attn = DistToLightNorm * DistToLightNorm;
    finalColor *= PointColor.rgb * Attn * shadowAtt;
   
    return finalColor;
}

float4 PointLightCommonPS(DS_OUTPUT In, bool bUseShadow) : SV_TARGET
{
	// Unpack the GBuffer
    SURFACE_DATA gbd = UnpackGBuffer_Loc(In.Position.xy);
	
	// Convert the data into the material structure
    Material mat;
    MaterialFromGBuffer(gbd, mat);

	// Reconstruct the world position
    float3 position = CalcWorldPos(In.PositionXYW.xy / In.PositionXYW.z, gbd.LinearDepth);

	// Calculate the light contribution
    float3 finalColor = CalcPoint(position, mat, bUseShadow);

	// return the final color
    return float4(finalColor, 1.0);
}

float4 PointLightPS(DS_OUTPUT In) : SV_TARGET
{
    return PointLightCommonPS(In, false);
}

float4 PointLightShadowPS(DS_OUTPUT In) : SV_TARGET
{
    return PointLightCommonPS(In, true);
}

technique11 PointLight
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, PointLightVS()));
        SetHullShader(CompileShader(hs_5_0, PointLightHS()));
        SetDomainShader(CompileShader(ds_5_0, PointLightDS()));
        SetPixelShader(CompileShader(ps_5_0, PointLightPS()));
    }
}

technique11 PointLightShadow
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, PointLightVS()));
        SetHullShader(CompileShader(hs_5_0, PointLightHS()));
        SetDomainShader(CompileShader(ds_5_0, PointLightDS()));
        SetPixelShader(CompileShader(ps_5_0, PointLightShadowPS()));
    }
}
