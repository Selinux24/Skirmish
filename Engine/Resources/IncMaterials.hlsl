#define SPECULAR_ALGORITHM_PHONG 		0
#define SPECULAR_ALGORITHM_BLINNPHONG 	1
#define SPECULAR_ALGORITHM_COOKTORRANCE	2

#define ROUGHNESS_LOOK_UP 		0
#define ROUGHNESS_BECKMANN 		1
#define ROUGHNESS_GAUSSIAN 		2

struct Material
{
    float4 Ambient;
    float4 Diffuse;
    float4 Emissive;
    float4 Specular;
    float Shininess;
    
    uint Algorithm;
    uint RoughnessMode;
    float RoughnessValue;
    float ReflectionAtNormIncidence;
};

Texture2D CookTorranceTexRoughness;

inline Material GetMaterialData(Texture2D materialsTexture, uint materialIndex, uint paletteWidth)
{
    uint baseIndex = 5 * materialIndex;

    float4 mat1 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat2 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat3 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat4 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));
    baseIndex++;
    float4 mat5 = materialsTexture.Load(uint3(baseIndex % paletteWidth, baseIndex / paletteWidth, 0));

    Material mat;

    mat.Emissive = mat1;
    mat.Ambient = mat2;
    mat.Diffuse = mat3;
    mat.Specular = float4(mat4.xyz, 1.0f);
    mat.Shininess = mat4.w;
    mat.Algorithm = uint(mat5.x);
    mat.RoughnessMode = uint(mat5.y);
    mat.RoughnessValue = mat5.z;
    mat.ReflectionAtNormIncidence = mat5.w;

    return mat;
}
