#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldInverse;
	float4x4 gWorldViewProjection;
};

cbuffer cbPerDirectional : register (b1)
{
	DirectionalLight gDirLight;
};

cbuffer cbPerPoint : register (b2)
{
	PointLight gPointLight;
};

cbuffer cbPerSpot : register (b3)
{
	SpotLight gSpotLight;
};

Texture2D gColorMap : register(t0);
Texture2D gNormalMap : register(t1);
Texture2D gDepthMap : register(t2);

SamplerState SampleTypePoint : register(s0);

struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

PixelInputType VSDeferred(VSVertexPositionTexture input)
{
    PixelInputType output = (PixelInputType)0;

	float4 pos = 0.0f;
	pos.xyz = input.positionLocal;
	pos.w = 1.0f;

	output.position = mul(pos, gWorldViewProjection);
    output.tex = input.tex;
    
    return output;
}

float4 PSDirectionalLight(PixelInputType input) : SV_TARGET
{
	float4 outputColor = 0.0f;

    //Color
    float4 color = gColorMap.Sample(SampleTypePoint, input.tex);
    //Normal
    float4 normal = gNormalMap.Sample(SampleTypePoint, input.tex);
	//Depth
    float4 depth = gDepthMap.Sample(SampleTypePoint, input.tex);

	if(length(normal) != 0.0f)
	{
		//Get the light direction
		float3 lightDir = -gDirLight.Direction;

		//Calculate the amount of light on this pixel
		float lightIntensity = saturate(dot(normal.xyz, lightDir));

		//Determine the final amount of diffuse color based on the color of the pixel combined with the light intensity.
		outputColor = saturate(color * lightIntensity);
		
		outputColor.a = 1.0f;
	}
	else
	{
		outputColor = color;
	}

	return outputColor;
}
float4 PSPointLight(PixelInputType input) : SV_TARGET
{
	float4 outputColor = 0.0f;

    //Color
    float4 color = gColorMap.Sample(SampleTypePoint, input.tex);
    //Normal
    float4 normal = gNormalMap.Sample(SampleTypePoint, input.tex);
	//Depth
    float4 depth = gDepthMap.Sample(SampleTypePoint, input.tex);

	if(length(normal) != 0.0f)
	{
		//Obtain screen position
		float4 position;
		position.xy = input.position.xy / input.position.w;
		position.z = depth.r;
		position.w = 1.0f;
	
		outputColor = color;
	}
	else
	{
		outputColor = color;
	}

	return outputColor;
}
float4 PSSpotLight(PixelInputType input) : SV_TARGET
{
	float4 outputColor = 0.0f;

    //Color
    float4 color = gColorMap.Sample(SampleTypePoint, input.tex);
    //Normal
    float4 normal = gNormalMap.Sample(SampleTypePoint, input.tex);
	//Depth
    float4 depth = gDepthMap.Sample(SampleTypePoint, input.tex);

	if(length(normal) != 0.0f)
	{
		//Obtain screen position
		float4 position;
		position.xy = input.position.xy / input.position.w;
		position.z = depth.r;
		position.w = 1.0f;
	
		outputColor = color;
	}
	else
	{
		outputColor = color;
	}

	return outputColor;
}

technique11 DeferredDirectionalLight
{
    pass P0
    {
        VertexShader = compile vs_5_0 VSDeferred();
        PixelShader = compile ps_5_0 PSDirectionalLight();
    }
}
technique11 DeferredPointLight
{
    pass P0
    {
        VertexShader = compile vs_5_0 VSDeferred();
        PixelShader = compile ps_5_0 PSPointLight();
    }
}
technique11 DeferredSpotLight
{
    pass P0
    {
        VertexShader = compile vs_5_0 VSDeferred();
        PixelShader = compile ps_5_0 PSSpotLight();
    }
}