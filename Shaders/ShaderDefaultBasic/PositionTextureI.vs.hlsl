#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbVSPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture main(VSVertexPositionTextureI input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture)0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);

	output.positionHomogeneous = mul(instancePosition, gWorldViewProjection);
	output.positionWorld = mul(instancePosition, gWorld).xyz;
	output.tex = input.tex;
	output.tintColor = input.tintColor;
	output.textureIndex = input.textureIndex;
	output.materialIndex = input.materialIndex;

	return output;
}
