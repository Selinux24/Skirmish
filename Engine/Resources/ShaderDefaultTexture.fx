#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorldViewProjection;
    float gTextureIndex;
};

Texture2DArray gTexture : register(t0);

struct VSVertex
{
    float3 positionLocal : POSITION;
    float2 tex : TEXCOORD0;
};

struct PSVertex
{
    float4 positionHomogeneous : SV_POSITION;
    float2 tex : TEXCOORD0;
};

PSVertex VSTexture(VSVertex input)
{
    PSVertex output = (PSVertex) 0;

	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
    output.tex = input.tex;

    return output;
}

float4 PSTexture(PSVertex input) : SV_TARGET
{
    return gTexture.Sample(SamplerLinear, float3(input.tex, gTextureIndex));
}

technique11 SimpleTexture
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSTexture()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSTexture()));
    }
}
