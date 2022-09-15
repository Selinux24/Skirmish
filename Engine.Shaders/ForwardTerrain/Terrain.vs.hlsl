#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

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

cbuffer cbTerrain : register(b2)
{
    float4 gTintColor;

    uint gMaterialIndex;
    uint gMode;
    uint2 PAD21;

    float gTextureResolution;
    float gProp;
    float gSlope1;
    float gSlope2;
};

Texture2D gMaterialPalette : register(t0);

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexTerrain2 main(VSVertexTerrain input)
{
    PSVertexTerrain2 output = (PSVertexTerrain2) 0;

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobals.MaterialPaletteWidth);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gPerFrame.ViewProjection);
    output.positionWorld = input.positionLocal;
    output.normalWorld = normalize(input.normalLocal);
    output.tangentWorld = normalize(input.tangentLocal);
    output.tex0 = input.tex * gTextureResolution;
    output.tex1 = input.tex;
    output.color = input.color * gTintColor;
    output.material = material;

    return output;
}
