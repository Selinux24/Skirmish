cbuffer cbPerFrame
{
	float4x4 gWorldViewProjection;
};
 
TextureCube gCubemap;

SamplerState samTriLinearSam
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexIn
{
	float3 positionLocal : POSITION;
};
struct VertexOut
{
	float4 positionHomogeneus : SV_POSITION;
    float3 positionLocal : POSITION;
};

RasterizerState NoCull
{
    CullMode = None;
};
DepthStencilState LessEqualDSS
{
    DepthFunc = LESS_EQUAL;
};

VertexOut VS(VertexIn input)
{
	VertexOut output;
	
	output.positionHomogeneus = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
	output.positionLocal = input.positionLocal;
	
	return output;
}

float4 PS(VertexOut input) : SV_Target
{
	return gCubemap.Sample(samTriLinearSam, input.positionLocal);
}

technique11 Cubemap
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VS()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PS()));
        
        SetRasterizerState(NoCull);
        SetDepthStencilState(LessEqualDSS, 0);
    }
}
