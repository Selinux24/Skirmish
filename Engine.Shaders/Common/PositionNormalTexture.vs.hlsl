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

cbuffer cbPerMesh : register(b2)
{
    float4x4 gLocal;
};

cbuffer cbPerMaterial : register(b3)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint gTextureIndex;
    uint2 PAD31;
};

Texture2D gMaterialPalette : register(t0);

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTexture2 main(VSVertexPositionNormalTexture input)
{
    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionNormalTexture2 output = (PSVertexPositionNormalTexture2) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = mul(float4(input.positionLocal, 1), gLocal).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3) gLocal));
	output.tex = input.tex;
	output.tintColor = gTintColor;
	output.textureIndex = gTextureIndex;
	output.material = material;

	return output;
}
