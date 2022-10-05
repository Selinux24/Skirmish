#ifndef __GBUFFER_INCLUDED__
#define __GBUFFER_INCLUDED__

#include "IncMaterials.hlsl"

struct GBuffer
{
    float4 color : SV_TARGET0;
    float4 normal : SV_TARGET1;
    float4 depth : SV_TARGET2;
    float4 mat1 : SV_TARGET3;
    float4 mat2 : SV_TARGET4;
    float4 mat3 : SV_TARGET5;
};

inline GBuffer Pack(float3 position, float3 normal, float4 diffuse, bool doLighting, Material material)
{
    GBuffer output = (GBuffer) 0;
    
    output.color = diffuse * material.Diffuse;
    output.normal = float4(normal, doLighting ? 1 : 0);
    output.depth = float4(position, material.Algorithm);
    output.mat1 = float4(material.Specular, material.Shininess);
    output.mat2 = float4(material.Emissive, material.Metallic);
    output.mat3 = float4(material.Ambient, material.Roughness);
    
    return output;
}

inline void UnPack(GBuffer gBuffer, out float3 position, out float3 normal, out float4 albedo, out bool doLighting, out Material material)
{
    position = 0;
    normal = 0;
    doLighting = false;
    material = (Material) 0;
    
    albedo = gBuffer.color;
    float4 tgNormal = gBuffer.normal;

    doLighting = tgNormal.w != 0;
    if (!doLighting)
    {
        return;
    }

    float4 tgDepth = gBuffer.depth;
    float4 tgMat1 = gBuffer.mat1;
    float4 tgMat2 = gBuffer.mat2;
    float4 tgMat3 = gBuffer.mat3;
    
    position = tgDepth.xyz;
    normal = tgNormal.xyz;

    material.Algorithm = tgDepth.w;
    material.Diffuse = float4(1, 1, 1, 1);
    material.Specular = tgMat1.rgb;
    material.Shininess = tgMat1.a;
    material.Emissive = tgMat2.rgb;
    material.Metallic = tgMat2.a;
    material.Ambient = tgMat3.rgb;
    material.Roughness = tgMat3.a;
}

#endif
