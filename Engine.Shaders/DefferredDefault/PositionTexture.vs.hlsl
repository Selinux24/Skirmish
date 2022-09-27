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
    uint gTextureIndex;
    uint2 PAD21;
};

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionTexture main(VSVertexPositionTexture input)
{
    PSVertexPositionTexture output = (PSVertexPositionTexture) 0;

    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = mul(float4(input.positionLocal, 1), gLocal).xyz;
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;
    output.materialIndex = gMaterialIndex;
    output.tintColor = gTintColor;

    return output;
}
