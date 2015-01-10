#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorldViewProjection;
};

TextureCube gCubemap;

RasterizerState NoCull
{
    CullMode = None;
};
DepthStencilState LessEqualDSS
{
    DepthFunc = LESS_EQUAL;
};

PSVertexPosition VSCubic(VSVertexPosition input)
{
	PSVertexPosition output;
	
	output.positionHomogeneus = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
	output.positionLocal = input.positionLocal;
	
	return output;
}

float4 PSCubic(PSVertexPosition input) : SV_Target
{
	return gCubemap.Sample(SamplerLinear, input.positionLocal);
}

technique11 Cubemap
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSCubic()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSCubic()));
        
        SetRasterizerState(NoCull);
        SetDepthStencilState(LessEqualDSS, 0);
    }
}
