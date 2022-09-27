#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

cbuffer cbPerMesh : register(b1)
{
    float4x4 gLocal;
};

cbuffer cbPerMaterial : register(b2)
{
    float4 gTintColor;
    uint gMaterialIndex;
    uint3 PAD21;
};

/**********************************************************************************************************
POSITION COLOR
**********************************************************************************************************/
PSVertexPositionColor main(VSVertexPositionColor input)
{
    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    PSVertexPositionColor output = (PSVertexPositionColor) 0;
    
    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = mul(float4(input.positionLocal, 1), gLocal).xyz;
    output.color = input.color * gTintColor;
    output.materialIndex = gMaterialIndex;
    
    return output;
}
