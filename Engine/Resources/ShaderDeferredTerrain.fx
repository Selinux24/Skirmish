#include "IncTerrain.fx"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbVSPerFrame : register (b1)
{
	float4x4 gVSWorld;
	float4x4 gVSWorldViewProjection;
	float gVSTextureResolution;
	float3 PAD_B1;
};

cbuffer cbPSPerObject : register (b4)
{
	float4 gPSParams;
	uint gPSMaterialIndex;
	uint3 PAD_B4;
};

PSVertexTerrain VSTerrain(VSVertexTerrain input)
{
    PSVertexTerrain output = (PSVertexTerrain)0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;
	output.normalWorld = normalize(mul(input.normalLocal, (float3x3)gVSWorld));
	output.tangentWorld = normalize(mul(input.tangentLocal, (float3x3)gVSWorld));
	output.tex0 = input.tex * gVSTextureResolution;
	output.tex1 = input.tex;
	output.color = input.color;
    
    return output;
}

GBufferPSOutput PSTerrainAlphaMap(PSVertexTerrain input)
{
	GBufferPSOutput output = (GBufferPSOutput)0;

	float4 specular;
	float3 normal;
	float4 color = AlphaMap(input, specular, normal);

	output.color = color;
	output.normal = float4(normal, 0);
	output.depth = float4(input.positionWorld, gPSMaterialIndex);

	return output;
}
GBufferPSOutput PSTerrainSlopes(PSVertexTerrain input)
{
	GBufferPSOutput output = (GBufferPSOutput)0;

	float4 specular;
	float3 normal;
	float4 color = Slopes(input, gPSParams.z, gPSParams.w, specular, normal);

	output.color = color;
	output.normal = float4(normal, 0);
	output.depth = float4(input.positionWorld, gPSMaterialIndex);

	return output;
}
GBufferPSOutput PSTerrainFull(PSVertexTerrain input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	float4 specular;
	float3 normal;
	float4 color = Full(input, gPSParams.y, gPSParams.z, gPSParams.w, specular, normal);

	output.color = color;
	output.normal = float4(normal, 0);
	output.depth = float4(input.positionWorld, gPSMaterialIndex);

    return output;
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 TerrainAlphaMapDeferred
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainAlphaMap()));
	}
}
technique11 TerrainSlopesDeferred
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainSlopes()));
	}
}
technique11 TerrainFullDeferred
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSTerrain()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSTerrainFull()));
	}
}
