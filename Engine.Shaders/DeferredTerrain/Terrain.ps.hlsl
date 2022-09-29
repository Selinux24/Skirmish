#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncLights.hlsl"

#define MODE_ALHPAMAP      0
#define MODE_SLOPES        1
#define MODE_FULL          2

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbTerrain : register(b1)
{
    float4 gTintColor;

    uint gMaterialIndex;
    uint gMode;
    uint2 PAD51;

    float gTextureResolution;
    float gProp;
    float gSlope1;
    float gSlope2;
};

Texture2D gAlphaTexture : register(t0);
Texture2DArray gNormalMapArray : register(t1);
Texture2DArray gColorTextureArray : register(t2);
Texture2DArray gDiffuseMapLRArray : register(t3);
Texture2DArray gDiffuseMapHRArray : register(t4);

SamplerState SamplerDiffuse : register(s0);
SamplerState SamplerNormal : register(s1);

inline float4 AlphaMap(float4 color, float3 normalWorld, float3 tangentWorld, float2 tex0, float2 tex1, out float3 normal)
{
    float4 alphaMap = gAlphaTexture.Sample(SamplerDiffuse, tex1);

    float3 normalMapSample1 = gNormalMapArray.Sample(SamplerNormal, float3(tex0, 0)).rgb;
    float3 normalMapSample2 = gNormalMapArray.Sample(SamplerNormal, float3(tex0, 1)).rgb;
    float3 normalMapSample = lerp(normalMapSample1, normalMapSample2, alphaMap.b);
    normal = NormalSampleToWorldSpace(normalMapSample, normalWorld, tangentWorld);

    float4 textureColor1 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 0));
    float4 textureColor2 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 1));
    float4 textureColor3 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 2));
    float4 textureColor4 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 3));

    float4 mapColor = lerp(textureColor1, textureColor2, alphaMap.r);
    mapColor = lerp(mapColor, textureColor3, alphaMap.g);
    mapColor = lerp(mapColor, textureColor4, alphaMap.b);

    return saturate(mapColor * color);
}

inline float4 Slopes(float4 positionHomogeneous, float4 color, float3 normalWorld, float3 tangentWorld, float2 tex0, float2 tex1, out float3 normal)
{
    float4 alphaMap = gAlphaTexture.Sample(SamplerDiffuse, tex1);

    float3 normalMapSample1 = gNormalMapArray.Sample(SamplerNormal, float3(tex0, 0)).rgb;
    float3 normalMapSample2 = gNormalMapArray.Sample(SamplerNormal, float3(tex0, 1)).rgb;
    float3 normalMapSample = lerp(normalMapSample1, normalMapSample2, alphaMap.b);
    normal = NormalSampleToWorldSpace(normalMapSample, normalWorld, tangentWorld);

    float4 mapColor = 0;

	// BY SLOPE. Determine which texture to use based on height.
    float slope = 1.0f - normalWorld.y;
    if (slope < gSlope1)
    {
        mapColor = lerp(
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 0)),
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 1)),
			slope / gSlope1);
    }
    if ((slope < gSlope2) && (slope >= gSlope1))
    {
        mapColor = lerp(
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 1)),
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 2)),
			(slope - gSlope1) * (1.0f / (gSlope2 - gSlope1)));
    }
    if (slope >= gSlope2)
    {
        mapColor = gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 2));
    }

    float depthValue = positionHomogeneous.z / positionHomogeneous.w;
    if (depthValue >= 0.05f)
    {
        mapColor *= gDiffuseMapHRArray.Sample(SamplerDiffuse, float3(tex0, 0)) * 1.8f;
    }

    return saturate(mapColor * color);
}

inline float4 Full(float4 positionHomogeneous, float4 color, float3 normalWorld, float3 tangentWorld, float2 tex0, float2 tex1, out float3 normal)
{
    float4 alphaMap = gAlphaTexture.Sample(SamplerDiffuse, tex1);

    float3 normalMapSample1 = gNormalMapArray.Sample(SamplerNormal, float3(tex0, 0)).rgb;
    float3 normalMapSample2 = gNormalMapArray.Sample(SamplerNormal, float3(tex0, 1)).rgb;
    float3 normalMapSample = lerp(normalMapSample1, normalMapSample2, alphaMap.b);
    normal = NormalSampleToWorldSpace(normalMapSample, normalWorld, tangentWorld);

	// BY ALPHA MAP
    float4 textureColor1 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 0));
    float4 textureColor2 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 1));
    float4 textureColor3 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 2));
    float4 textureColor4 = gColorTextureArray.Sample(SamplerDiffuse, float3(tex0, 3));

    float4 alphaMap1 = gAlphaTexture.Sample(SamplerDiffuse, tex1);

    float4 color1 = lerp(textureColor1, textureColor2, alphaMap1.r);
    color1 = lerp(color1, textureColor3, alphaMap1.g);
    color1 = lerp(color1, textureColor4, alphaMap1.b);

	// BY SLOPE. Determine which texture to use based on height.
    float4 color2 = 0;
    float slope = 1.0f - normalWorld.y;
    if (slope < gSlope1)
    {
        color2 = lerp(
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 0)),
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 1)),
			slope / gSlope1);
    }
    if ((slope < gSlope2) && (slope >= gSlope1))
    {
        color2 = lerp(
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 1)),
			gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 2)),
			(slope - gSlope1) * (1.0f / (gSlope2 - gSlope1)));
    }
    if (slope >= gSlope2)
    {
        color2 = gDiffuseMapLRArray.Sample(SamplerDiffuse, float3(tex0, 2));
    }

    float depthValue = positionHomogeneous.z / positionHomogeneous.w;
    if (depthValue >= 0.05f)
    {
        color2 *= gDiffuseMapHRArray.Sample(SamplerDiffuse, float3(tex0, 0)) * 1.8f;
    }

    return saturate(((color1 * gProp) + (color2 * (1.0f - gProp))) * color);
}

GBufferPSOutput main(PSVertexTerrain2 input)
{
    float3 normal;
    float4 diffuse;
    if (gMode == MODE_ALHPAMAP)
    {
        diffuse = AlphaMap(input.color, input.normalWorld, input.tangentWorld, input.tex0, input.tex1, normal);
    }
    else if (gMode == MODE_SLOPES)
    {
        diffuse = Slopes(input.positionHomogeneous, input.color, input.normalWorld, input.tangentWorld, input.tex0, input.tex1, normal);
    }
    else if (gMode == MODE_FULL)
    {
        diffuse = Full(input.positionHomogeneous, input.color, input.normalWorld, input.tangentWorld, input.tex0, input.tex1, normal);
    }
    else
    {
        normal = float3(0, 1, 0);
        diffuse = input.color;
    }

    GBufferPSOutput output = (GBufferPSOutput) 0;

    output.color = diffuse * gTintColor * input.material.Diffuse;
    output.normal = float4(normal, 1);
    output.depth = float4(input.positionWorld, input.material.Algorithm);
    output.mat1 = float4(input.material.Specular, input.material.Shininess);
    output.mat2 = float4(input.material.Emissive, input.material.Metallic);
    output.mat3 = float4(input.material.Ambient, input.material.Roughness);
    
    return output;
}
