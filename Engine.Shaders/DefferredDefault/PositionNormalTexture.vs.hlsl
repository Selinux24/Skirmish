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
PSVertexPositionNormalTexture main(VSVertexPositionNormalTexture input)
{
    float4x4 wvp = mul(gLocal, gPerFrame.ViewProjection);

    PSVertexPositionNormalTexture output = (PSVertexPositionNormalTexture) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), wvp);
    output.positionWorld = mul(float4(input.positionLocal, 1), gLocal).xyz;
    output.normalWorld = normalize(mul(input.normalLocal, (float3x3) gLocal));
    output.tex = input.tex;
    output.textureIndex = gTextureIndex;
    output.materialIndex = gMaterialIndex;
    output.tintColor = gTintColor;

    return output;
}
