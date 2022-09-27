#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

/**********************************************************************************************************
BUFFERS & VARIABLES
**********************************************************************************************************/
cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

/**********************************************************************************************************
POSITION TEXTURE
**********************************************************************************************************/
PSVertexPositionNormalTextureTangent main(VSVertexPositionNormalTextureTangentI input)
{
    PSVertexPositionNormalTextureTangent output = (PSVertexPositionNormalTextureTangent) 0;

	float4 instancePosition = mul(float4(input.positionLocal, 1), input.localTransform);
    float3 instanceNormal = mul(input.normalLocal, (float3x3) input.localTransform);
    float3 instanceTangent = mul(input.tangentLocal, (float3x3) input.localTransform);

    output.positionHomogeneous = mul(instancePosition, gPerFrame.ViewProjection);
    output.positionWorld = instancePosition.xyz;
    output.normalWorld = normalize(instanceNormal);
    output.tangentWorld = normalize(instanceTangent);
    output.tex = input.tex;
	output.textureIndex = input.textureIndex;
    output.materialIndex = input.materialIndex;
    output.tintColor = input.tintColor;
	
	return output;
}
