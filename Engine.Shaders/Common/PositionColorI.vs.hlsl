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
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor main(VSVertexPositionColorI input)
{
	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

    Material material = GetMaterialData(gMaterialPalette, input.materialIndex + gMaterialIndex, gGlobals.MaterialPaletteWidth);

    PSVertexPositionColor output = (PSVertexPositionColor) 0;

	output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
	output.positionWorld = instancePosition.xyz;
    output.color = input.color * input.tintColor * gTintColor * material.Diffuse;

	return output;
}
