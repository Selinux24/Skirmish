#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncMaterials.hlsl"

cbuffer cbGlobal : register(b0)
{
    Globals gGlobal;
};

cbuffer cbPerMaterial : register(b1)
{
    float4 gTintColor;
    
    uint gMaterialIndex;
    uint gTextureCount;
    uint gNormalMapCount;
    uint PAD11;
};

Texture2D gMaterialPalette : register(t0);

struct Foliage
{
    float3 positionWorld : POSITION;
    float2 sizeWorld : SIZE;
};

struct GSFoliage
{
    float3 positionWorld : POSITION;
    float2 sizeWorld : SIZE;
    Material material;
};

GSFoliage main(Foliage input)
{
    GSFoliage output;

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobal.MaterialPaletteWidth);
    
    output.positionWorld = input.positionWorld;
    output.sizeWorld = input.sizeWorld;
    output.material = material;

    return output;
}
