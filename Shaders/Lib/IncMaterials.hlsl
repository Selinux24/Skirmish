#ifndef __MATERIALS_INCLUDED__
#define __MATERIALS_INCLUDED__

#ifndef SPECULAR_ALGORITHM_PHONG
#define SPECULAR_ALGORITHM_PHONG 		0
#endif
#ifndef SPECULAR_ALGORITHM_BLINNPHONG
#define SPECULAR_ALGORITHM_BLINNPHONG 	1
#endif
#ifndef SPECULAR_ALGORITHM_COOKTORRANCE
#define SPECULAR_ALGORITHM_COOKTORRANCE	2
#endif

#ifndef MATERIAL_STRIDE
#define MATERIAL_STRIDE 8
#endif

struct Material
{
    float4 Diffuse : DIFFUSE;
    float3 Specular : SPECULAR;
    float Shininess : SHININESS;
    float3 Emissive : EMISSIVE;
    float Metallic : METALLIC;
    float3 Ambient : AMBIENT;
    float Roughness : ROUGHNESS;
    
    uint Algorithm : ALGORITHM;
};

Texture2D CookTorranceTexRoughness;

inline Material GetMaterialData(Texture2D materialsTexture, uint materialIndex, uint paletteWidth)
{
    uint baseIndex = MATERIAL_STRIDE * materialIndex;

    float4 mat1 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat2 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat3 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat4 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat5 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat6 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat7 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat8 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));

    Material mat;

    mat.Algorithm = uint(mat1.r);

    mat.Metallic = clamp(mat2.r, 0.0, 1.0);
    mat.Roughness = clamp(mat2.g, 0.01, 1.0);

    mat.Diffuse = mat5;
    
    mat.Emissive = mat6.rgb;

    mat.Ambient = mat7.rgb;

    mat.Specular = mat8.rgb;
    mat.Shininess = mat8.a;

    return mat;
}

#endif
