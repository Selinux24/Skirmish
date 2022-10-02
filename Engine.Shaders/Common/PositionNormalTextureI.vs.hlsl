#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbGlobals : register(b0)
{
    Globals gGlobals;
};

cbuffer cbPerFrame : register(b1)
{
    PerFrame gPerFrame;
};

cbuffer cbPerMaterial : register(b2)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint3 PAD21;
};

Texture2D gMaterialPalette : register(t0);

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTexture2 main(VSVertexPositionNormalTextureI input)
{
    float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
    float3 instanceNormal = mul(input.normalLocal, (float3x3) input.localTransform);

    uint materialIndex = input.materialIndex >= 0 ? input.materialIndex : gMaterialIndex;
    Material material = GetMaterialData(gMaterialPalette, materialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionNormalTexture2 output = (PSVertexPositionNormalTexture2) 0;

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
    output.positionWorld = instancePosition.xyz;
    output.normalWorld = normalize(instanceNormal);
    output.tex = input.tex;
    output.tintColor = input.tintColor * gTintColor;
    output.textureIndex = input.textureIndex;
    output.material = material;

    return output;
}
