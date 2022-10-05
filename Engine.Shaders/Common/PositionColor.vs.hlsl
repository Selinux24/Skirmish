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

cbuffer cbPerMesh : register(b2)
{
	float4x4 gLocal;
};

cbuffer cbPerMaterial : register(b3)
{
	float4 gTintColor;
	uint gMaterialIndex;
	uint3 PAD31;
};

Texture2D gMaterialPalette : register(t0);

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor main(VSVertexPositionColor input)
{
    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

	Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionColor output = (PSVertexPositionColor) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
	output.positionWorld = mul(float4(input.positionLocal, 1), gLocal).xyz;
	output.color = input.color * gTintColor * material.Diffuse;

	return output;
}
