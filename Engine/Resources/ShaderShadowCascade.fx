#include "IncVertexFormats.fx"

cbuffer cbGlobals : register(b0)
{
    uint gAnimationPaletteWidth;
    uint3 PAD01;
};
Texture2D gAnimationPalette : register(t0);

cbuffer cbVSPerFrame : register(b1)
{
    float4x4 gGSWorldViewProjection[3];
};

cbuffer cbVSPerInstance : register(b2)
{
    uint gVSAnimationOffset;
    uint3 PAD21;
};

//TEXTURE VARIABLES FOR TRANSPARENCY
Texture2DArray gPSDiffuseMapArray : register(t1);

cbuffer cbPSPerInstance : register(b5)
{
    uint gPSTextureIndex;
    uint3 PAD51;
};

struct GSShadowMap
{
    float4 position : SV_POSITION;
    uint index : SV_RENDERTARGETARRAYINDEX;
};

PSShadowMapPosition CascadedShadowGenVS(float3 positionLocal : POSITION)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = float4(positionLocal, 1.0f);

    return output;
}
PSShadowMapPosition CascadedShadowGenVSI(float3 positionLocal : POSITION, row_major float4x4 localTransform : LOCALTRANSFORM)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

    output.positionHomogeneous = mul(float4(positionLocal, 1), localTransform);
    
    return output;
}
PSShadowMapPosition CascadedShadowGenVSkinned(float3 positionLocal : POSITION, float3 weights : WEIGHTS, uint4 boneIndices : BONEINDICES)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

    float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
    ComputePositionWeights(
		gAnimationPalette,
		gVSAnimationOffset,
		gAnimationPaletteWidth,
		weights,
		boneIndices,
		positionLocal,
		positionL);
	
    output.positionHomogeneous = positionL;

    return output;
}
PSShadowMapPosition CascadedShadowGenVSkinnedI(float3 positionLocal : POSITION, row_major float4x4 localTransform : LOCALTRANSFORM, float3 weights : WEIGHTS, uint4 boneIndices : BONEINDICES, uint animationOffset : ANIMATIONOFFSET)
{
    PSShadowMapPosition output = (PSShadowMapPosition) 0;

    float4 positionL = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
    ComputePositionWeights(
		gAnimationPalette,
		animationOffset,
		gAnimationPaletteWidth,
		weights,
		boneIndices,
		positionLocal,
		positionL);

    float4 instancePosition = mul(positionL, localTransform);
	
    output.positionHomogeneous = instancePosition;
    
    return output;
}

[maxvertexcount(9)]
void CascadedShadowMapsGenGS(triangle PSShadowMapPosition input[3] : SV_Position, inout TriangleStream<GSShadowMap> outputStream)
{
    for (int iFace = 0; iFace < 3; iFace++)
    {
        GSShadowMap output;

        output.index = iFace;

        for (int v = 0; v < 3; v++)
        {
            output.position = mul(input[v].positionHomogeneous, gGSWorldViewProjection[iFace]);

            outputStream.Append(output);
        }
        outputStream.RestartStrip();
    }
}

technique11 CascadedShadowMapsGen
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, CascadedShadowGenVS()));
        SetGeometryShader(CompileShader(gs_5_0, CascadedShadowMapsGenGS()));
        SetPixelShader(NULL);
    }
}
technique11 CascadedShadowMapsGenI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, CascadedShadowGenVSI()));
        SetGeometryShader(CompileShader(gs_5_0, CascadedShadowMapsGenGS()));
        SetPixelShader(NULL);
    }
}
technique11 CascadedShadowMapsGenSkinned
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, CascadedShadowGenVSkinned()));
        SetGeometryShader(CompileShader(gs_5_0, CascadedShadowMapsGenGS()));
        SetPixelShader(NULL);
    }
}
technique11 CascadedShadowMapsGenSkinnedI
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, CascadedShadowGenVSkinnedI()));
        SetGeometryShader(CompileShader(gs_5_0, CascadedShadowMapsGenGS()));
        SetPixelShader(NULL);
    }
}
