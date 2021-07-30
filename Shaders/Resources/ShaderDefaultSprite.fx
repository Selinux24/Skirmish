#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
};
cbuffer cbPerObject : register(b1)
{
	float4 gColor;
};
cbuffer cbPerInstance : register(b2)
{
	float gTextureIndex;
    float3 gPAD21;
};

Texture2DArray gTextureArray : register(t0);

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor VSPositionColor(VSVertexPositionColor input)
{
	PSVertexPositionColor output = (PSVertexPositionColor) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.color = input.color;
    
	return output;
}

float4 PSPositionColor(PSVertexPositionColor input) : SV_TARGET
{
	return input.color * gColor;
}

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture VSPositionTexture(VSVertexPositionTexture input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
	output.positionWorld = mul(float4(input.positionLocal, 1), gWorld).xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
	return output;
}

float4 PSPositionTexture(PSVertexPositionTexture input) : SV_TARGET
{
    return saturate(gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex)) * gColor);
}
float4 PSPositionTextureRED(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
	//Grayscale of red channel
	return float4(color.rrr, 1);
}
float4 PSPositionTextureGREEN(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
	//Grayscale of green channel
	return float4(color.ggg, 1);
}
float4 PSPositionTextureBLUE(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
   	//Grayscale of blue channel
	return float4(color.bbb, 1);
}
float4 PSPositionTextureALPHA(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
   	//Grayscale of alpha channel
	return float4(color.aaa, 1);
}
float4 PSPositionTextureNOALPHA(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

   	//Color channel
	return float4(color.rgb, 1);
}

/**********************************************************************************************************
EFFECTS
**********************************************************************************************************/
technique11 PositionColor
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionColor()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionColor()));
	}
}

technique11 PositionTexture
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTexture()));
	}
}
technique11 PositionTextureRED
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureRED()));
	}
}
technique11 PositionTextureGREEN
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureGREEN()));
	}
}
technique11 PositionTextureBLUE
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureBLUE()));
	}
}
technique11 PositionTextureALPHA
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureALPHA()));
	}
}
technique11 PositionTextureNOALPHA
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PSPositionTextureNOALPHA()));
	}
}
