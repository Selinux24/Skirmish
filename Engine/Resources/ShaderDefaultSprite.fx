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
    bool gUseRect;
    float gPAD01;
    float4 gRectangle;
};
cbuffer cbPerObject : register(b1)
{
    float4 gSize;
    float4 gColor1;
    float4 gColor2;
    float4 gColor3;
    float4 gColor4;
    float3 gPct;
    int gDirection;
    float gTextureIndex;
    float3 gPAD11;
};

Texture2DArray gTextureArray : register(t0);

float MapScreenCoordX(float x, float4 rectPixels, float2 screenPixels)
{
    float p = 0.5 * x + 0.5;
	
    float left = rectPixels.x / screenPixels.x;
    float width = rectPixels.z / screenPixels.x;
    return clamp((p - left) / width, 0., 1.);
}
float MapScreenCoordY(float y, float4 rectPixels, float2 screenPixels)
{
    float p = 0.5 * -y + 0.5;

    float top = rectPixels.y / screenPixels.y;
    float height = rectPixels.w / screenPixels.y;
    return clamp((p - top) / height, 0., 1.);
}
float4 GetTintColor(float value)
{
    if (value <= gPct.x) return gColor1;
    if (value <= gPct.y) return gColor2;
    if (value <= gPct.z) return gColor3;
    return gColor4;
}
float4 EvaluateRect(float2 uv, float4 color)
{
    if (!gUseRect)
    {
        return color;
    }
    
    float2 pixel = MapUVToScreenPixel(uv, gResolution);
    if (PixelIntoRectangle(pixel, gRectangle))
    {
        return color;
    }
	
    return 0;
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
    return EvaluateRect(input.positionWorld.xy, saturate(input.color * gColor1));
}
float4 PSPositionColorPct(PSVertexPositionColor input) : SV_TARGET
{
    float pct = gDirection == 0 ? 
		MapScreenCoordX(input.positionWorld.x, gSize, gResolution) : 
		MapScreenCoordY(input.positionWorld.y, gSize, gResolution);
    float4 tintColor = GetTintColor(pct);
	
    return EvaluateRect(input.positionWorld.xy, saturate(input.color * tintColor));
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
	
    return EvaluateRect(input.positionWorld.xy, saturate(color * gColor1));
}
float4 PSPositionTextureRED(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
	//Grayscale of red channel
    return EvaluateRect(input.positionWorld.xy, float4(color.rrr, 1));
}
float4 PSPositionTextureGREEN(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
	//Grayscale of green channel
    return EvaluateRect(input.positionWorld.xy, float4(color.ggg, 1));
}
float4 PSPositionTextureBLUE(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
   	//Grayscale of blue channel
    return EvaluateRect(input.positionWorld.xy, float4(color.bbb, 1));
}
float4 PSPositionTextureALPHA(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
   	//Grayscale of alpha channel
    return EvaluateRect(input.positionWorld.xy, float4(color.aaa, 1));
}
float4 PSPositionTextureNOALPHA(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));

   	//Color channel
    return EvaluateRect(input.positionWorld.xy, float4(color.rgb, 1));
}
float4 PSPositionTexturePct(PSVertexPositionTexture input) : SV_TARGET
{
    float4 color = gTextureArray.Sample(SamplerLinear, float3(input.tex, input.textureIndex));
	
    float pct = gDirection == 0 ? input.tex.x : input.tex.y;
    float4 tintColor = GetTintColor(pct);

    return EvaluateRect(input.positionWorld.xy, saturate(color * tintColor));
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