#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	PerFrame gPerFrame;
};

cbuffer cbPerMesh : register(b1)
{
	float4x4 gLocal;
};

/**********************************************************************************************************
POSITION NORMAL COLOR
**********************************************************************************************************/
PSShadowMapPosition main(VSVertexPositionNormalColor input)
{
	PSShadowMapPosition output = (PSShadowMapPosition)0;

	float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), wvp);

	return output;
}
