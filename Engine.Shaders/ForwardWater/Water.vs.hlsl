#include "..\Lib\IncBuiltIn.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    PerFrame gPerFrame;
};

PSVertexPosition main(VSVertexPosition input)
{
    PSVertexPosition output = (PSVertexPosition) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gPerFrame.ViewProjection);
    output.positionWorld = input.positionLocal;

    return output;
}
