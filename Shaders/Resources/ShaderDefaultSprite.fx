#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
    float2 gResolution;
    float2 gPAD01;
};
cbuffer cbPerObject : register(b1)
{
	float4 gColor;
    float4 gSize;
    float4 gColor2;
    float gPct;
    float gTextureIndex;
    float2 gPAD11;
};

Texture2DArray gTextureArray : register(t0);

float MapScreenCoord(float positionWorld)
{
    float x = 0.5 * positionWorld.x + 0.5;
	
    float left = gSize.x / gResolution.x;
    float width = gSize.z / gResolution.x;
    return clamp((x - left) / width, 0., 1.);
}

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor VSPositionColor(VSVertexPositionColor input)
{
	PSVertexPositionColor output = (PSVertexPositionColor) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = output.positionHomogeneous.xyz;
	output.color = input.color;
    
	return output;
}

float4 PSPositionColor(PSVertexPositionColor input) : SV_TARGET
{
    return saturate(input.color * gColor);
}
float4 PSPositionColorPct(PSVertexPositionColor input) : SV_TARGET
{
    float x = MapScreenCoord(input.positionWorld.x);
    float4 tintColor = x > gPct ? gColor : gColor2;
	
    return input.color * tintColor;
}

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture VSPositionTexture(VSVertexPositionTexture input)
{
	PSVertexPositionTexture output = (PSVertexPositionTexture) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1), gWorldViewProjection);
    output.positionWorld = output.positionHomogeneous.xyz;
	output.tex = input.tex;
	output.textureIndex = gTextureIndex;
    
	return output;
}

float4 PSPositionTexture(PSVertexPositionTexture input) : SV_TARGET
{
    float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
    return saturate(color * gColor);
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
float4 PSPositionTexturePct(PSVertexPositionTexture input) : SV_TARGET
{
    float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
    float x = input.tex.x;
    float4 tintColor = x > gPct ? gColor : gColor2;
	
    return color * tintColor;
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
technique11 PositionColorPct
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSPositionColor()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSPositionColorPct()));
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
technique11 PositionTexturePct
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSPositionTexture()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSPositionTexturePct()));
    }
}