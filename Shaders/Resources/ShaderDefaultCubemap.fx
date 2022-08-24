#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorldViewProjection;
};

TextureCube gCubemap : register(t0);

SamplerState SamplerLinear : register(s0)
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

PSVertexPosition VSCubic(VSVertexPosition input)
{
    PSVertexPosition output;
	
    output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
    output.positionWorld = input.positionLocal;
	
    return output;
}

float4 PSForwardCubic(PSVertexPosition input) : SV_Target
{
    return gCubemap.Sample(SamplerLinear, input.positionWorld);
}

technique11 ForwardCubemap
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSCubic()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSForwardCubic()));
    }
}
