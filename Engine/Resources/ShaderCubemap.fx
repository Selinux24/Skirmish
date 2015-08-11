#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorldViewProjection;
};

TextureCube gCubemap;

PSVertexPosition VSCubic(VSVertexPosition input)
{
	PSVertexPosition output;
	
	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
	output.positionLocal = input.positionLocal;
	
	return output;
}

float4 PSForwardCubic(PSVertexPosition input) : SV_Target
{
	return gCubemap.Sample(SamplerLinear, input.positionLocal);
}
GBufferPSOutput PSDeferredCubic(PSVertexPosition input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	output.color = gCubemap.Sample(SamplerLinear, input.positionLocal);
	output.normal = 0.0f;
	output.depth = input.positionHomogeneous.z / input.positionHomogeneous.w;

    return output;
}

technique11 ForwardCubemap
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSCubic()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSForwardCubic()));
        
        SetRasterizerState(RasterizerNoCull);
        SetDepthStencilState(StencilLessEqualDSS, 0);
    }
}
technique11 DeferredCubemap
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSCubic()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSDeferredCubic()));
        
        SetRasterizerState(RasterizerNoCull);
        SetDepthStencilState(StencilLessEqualDSS, 0);
    }
}
