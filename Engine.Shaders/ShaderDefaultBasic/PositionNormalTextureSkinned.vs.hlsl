#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMaterials.hlsl"
#include "..\Lib\IncAnimation.hlsl"

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
    uint gAnimationOffset;
    uint gAnimationOffset2;
    float gAnimationInterpolation;
    float PAD21;
};

cbuffer cbPerMaterial : register(b3)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint gTextureIndex;
    uint2 PAD31;
};

Texture2D gMaterialPalette : register(t0);
Texture2D gAnimationPalette : register(t1);

/**********************************************************************************************************
POSITION NORMAL TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTexture2 main(VSVertexPositionNormalTextureSkinned input)
{
	PSVertexPositionNormalTexture2 output = (PSVertexPositionNormalTexture2)0;

	float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 normalL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	ComputePositionNormalWeights(
		gAnimationPalette,
		gAnimationOffset,
		gAnimationOffset2,
		gAnimationInterpolation,
		gGlobals.AnimationPaletteWidth,
		input.weights,
		input.boneIndices,
		input.positionLocal,
		input.normalLocal,
		positionL,
		normalL);

    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gGlobals.MaterialPaletteWidth);

    output.positionHomogeneous = mul(positionL, wvp);
    output.positionWorld = mul(positionL, gLocal).xyz;
    output.normalWorld = normalize(mul(normalL.xyz, (float3x3) gLocal));
	output.tex = input.tex;
	output.tintColor = gTintColor;
	output.textureIndex = gTextureIndex;
	output.material = material;

	return output;
}
