#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

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

GSVertexBillboard2 main(VSVertexBillboard input)
{
    GSVertexBillboard2 output;

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobal.MaterialPaletteWidth);
    
    output.centerWorld = input.positionWorld;
    output.sizeWorld = input.sizeWorld;
    output.tintColor = gTintColor;
    output.material = material;

    return output;
}
