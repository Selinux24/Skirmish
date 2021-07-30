#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorldViewProjection;
};

PSShadowMapPosition VSTerrain(VSVertexTerrain input)
{
	PSShadowMapPosition output = (PSShadowMapPosition) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection);

	return output;
}

technique11 TerrainShadowMap
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(NULL);
	}
}
