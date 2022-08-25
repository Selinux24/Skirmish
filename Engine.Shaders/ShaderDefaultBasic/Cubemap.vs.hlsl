#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorld;
    float4x4 gWorldViewProjection;
};

PSVertexPosition main(VSVertexPosition input)
{
    PSVertexPosition output;
	
    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
    output.positionWorld = input.positionLocal;
	
    return output;
}
