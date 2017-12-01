
struct Material
{
    float4 Emissive;
    float4 Ambient;
    float4 Diffuse;
    float4 Specular;
    float Shininess;
};

inline Material GetMaterialData(Texture2D materialsTexture, uint materialIndex, uint paletteWidth)
{
    uint baseIndex = 4 * materialIndex;
    uint baseU = baseIndex % paletteWidth;
    uint baseV = baseIndex / paletteWidth;

    float4 mat1 = materialsTexture.Load(uint3(baseU, baseV, 0));
    float4 mat2 = materialsTexture.Load(uint3(baseU + 1, baseV, 0));
    float4 mat3 = materialsTexture.Load(uint3(baseU + 2, baseV, 0));
    float4 mat4 = materialsTexture.Load(uint3(baseU + 3, baseV, 0));

    Material mat;

    mat.Emissive = mat1;
    mat.Ambient = mat2;
    mat.Diffuse = mat3;
    mat.Specular = float4(mat4.xyz, 1.0f);
    mat.Shininess = mat4.w;

    return mat;
}
