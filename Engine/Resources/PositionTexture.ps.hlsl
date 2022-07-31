#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPSGlobals : register(b0)
{
	uint gMaterialPaletteWidth;
	uint3 PAD01;
};
Texture2D gMaterialPalette : register(t0);

cbuffer cbPSPerFrame : register(b1)
{
	float3 gEyePositionWorld;
	float PAD11;
	float4 gFogColor;
	float gFogStart;
	float gFogRange;
	float2 PAD12;
};
Texture2DArray gDiffuseMapArray : register(t5);

cbuffer cbPSPerInstance : register(b2)
{
	uint gChannel;
	uint3 PAD21;
};

SamplerState SamplerDiffuse : register(s0);

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
float4 GrayscaleRed(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

	//Grayscale of red channel
	return float4(color.rrr, 1);
}
float4 GrayscaleGreen(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

	//Grayscale of green channel
	return float4(color.ggg, 1);
}
float4 GrayscaleBlue(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

	//Grayscale of blue channel
	return float4(color.bbb, 1);
}
float4 GrayscaleAlpha(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

	//Grayscale of alpha channel
	return float4(color.aaa, 1);
}
float4 NoAlpha(PSVertexPositionTexture input) : SV_TARGET
{
	float4 color = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));

	//Color channel
	return float4(color.rgb, 1);
}
float4 main(PSVertexPositionTexture input) : SV_TARGET
{
	if(gChannel == 0)
	{
		Material material = GetMaterialData(gMaterialPalette, input.materialIndex, gMaterialPaletteWidth);


		float4 textureColor = gDiffuseMapArray.Sample(SamplerDiffuse, float3(input.tex, input.textureIndex));
		textureColor *= input.tintColor * material.Diffuse;

		if (gFogRange > 0)
		{
			float distToEye = length(gEyePositionWorld - input.positionWorld);

			textureColor = ComputeFog(textureColor, distToEye, gFogStart, gFogRange, gFogColor);
		}

		return textureColor;
	}
	
	if(gChannel == 1)
	{
		return GrayscaleRed(input);
	}

	if(gChannel == 2)
	{
		return GrayscaleGreen(input);
	}

	if(gChannel == 3)
	{
		return GrayscaleBlue(input);
	}

	if(gChannel == 4)
	{
		return GrayscaleAlpha(input);
	}

	if(gChannel == 5)
	{
		return NoAlpha(input);
	}

	return float4(0,0,0,1);
}
