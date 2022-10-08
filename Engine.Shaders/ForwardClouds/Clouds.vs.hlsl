#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncMatrix.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

struct VSVertexCloud
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
};

struct PSVertexCloud
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
    float2 tex : TEXCOORD0;
    float4 tintColor : TINTCOLOR;
    uint textureIndex : TEXTUREINDEX;
    uint materialIndex : MATERIALINDEX;
};

PSVertexCloud main(VSVertexCloud input)
{
    PSVertexCloud output = (PSVertexCloud) 0;

    float4x4 translation = translateToMatrix(gPerFrame.EyePosition);
    float4x4 wvp = mul(translation, gPerFrame.ViewProjection);
    
    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = input.positionLocal;
    output.tex = input.tex;
    output.textureIndex = 0;

    return output;
}
