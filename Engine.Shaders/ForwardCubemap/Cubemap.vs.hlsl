#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"
#include "..\Lib\IncMatrix.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
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
