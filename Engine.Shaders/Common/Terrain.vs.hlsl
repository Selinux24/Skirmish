#include "..\Lib\IncBuiltIn.hlsl"
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

struct VSVertexTerrain
{
    float3 positionLocal : POSITION;
    float3 normalLocal : NORMAL;
    float3 tangentLocal : TANGENT;
    float2 tex : TEXCOORD0;
    float4 color : COLOR0;
};

struct PSVertexTerrain
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float3 normalWorld : NORMAL;
    float3 tangentWorld : TANGENT;
    float2 tex0 : TEXCOORD0;
    float2 tex1 : TEXCOORD1;
    float4 color : COLOR0;
    Material material;
};

/**********************************************************************************************************
POSITION NORMAL TEXTURE TANGENT
**********************************************************************************************************/
PSVertexTerrain main(VSVertexTerrain input)
{
    PSVertexTerrain output = (PSVertexTerrain) 0;

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
