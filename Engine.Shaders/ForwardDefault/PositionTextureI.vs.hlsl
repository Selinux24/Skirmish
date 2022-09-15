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
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture2 main(VSVertexPositionTextureI input)
{
	PSVertexPositionTexture2 output = (PSVertexPositionTexture2)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    uint materialIndex = input.materialIndex >= 0 ? input.materialIndex : gMaterialIndex;
    Material material = GetMaterialData(gMaterialPalette, materialIndex, gGlobals.MaterialPaletteWidth);

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
    output.positionWorld = instancePosition.xyz;
	output.tex = input.tex;
    output.tintColor = input.tintColor * gTintColor * material.Diffuse;
	output.textureIndex = input.textureIndex;
	
	return output;
}
