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

    float gStartRadius;
    float gEndRadius;
    float2 PAD12;
};

Texture2D gMaterialPalette : register(t0);

struct VSVertexBillboard
{
    float3 positionWorld : POSITION;
    float2 sizeWorld : SIZE;
};

struct GSVertexBillboard
{
    float3 centerWorld : POSITION;
    float2 sizeWorld : SIZE;
    float4 tintColor : TINTCOLOR;
    Material material;
};

GSVertexBillboard main(VSVertexBillboard input)
{
    GSVertexBillboard output;

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobal.MaterialPaletteWidth);
    
    output.centerWorld = input.positionWorld;
    output.sizeWorld = input.sizeWorld;
    output.tintColor = gTintColor;
    output.material = material;

    return output;
}
