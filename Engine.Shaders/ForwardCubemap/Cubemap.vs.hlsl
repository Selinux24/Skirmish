#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncMatrix.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

struct VSVertexPosition
{
    float3 positionLocal : POSITION;
};

struct PSVertexPosition
{
    float4 positionHomogeneous : SV_POSITION;
    float3 positionWorld : POSITION;
};

PSVertexPosition main(VSVertexPosition input)
{
    PSVertexPosition output;
	
    float4x4 translation = translateToMatrix(gPerFrame.EyePosition);
    float4x4 wvp = mul(translation, gPerFrame.ViewProjection);

    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), wvp).xyww;
    output.positionWorld = input.positionLocal;
	
    return output;
}
