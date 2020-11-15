#define SPECULAR_ALGORITHM_PHONG 		0
#define SPECULAR_ALGORITHM_BLINNPHONG 	1
#define SPECULAR_ALGORITHM_COOKTORRANCE	2

#define MATERIAL_STRIDE 8

struct Material
{
    uint Algorithm;

    float4 Diffuse;
    float3 Emissive;
    float3 Ambient;
    float3 Specular;
    
    float Shininess;
    float Metallic;
    float Roughness;
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
