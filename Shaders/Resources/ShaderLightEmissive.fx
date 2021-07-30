//--------------------------------------------------------------------------------------
// Constant Buffers
//--------------------------------------------------------------------------------------
cbuffer cbEmissive : register(b2)
{
    matrix g_mWorldViewProjection : packoffset(c0);
    float4 g_color : packoffset(c4);
}

//--------------------------------------------------------------------------------------
// shader input/output structure
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float4 Position : POSITION; // vertex position 
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION; // vertex position 
};

VS_OUTPUT RenderEmissiveVS(VS_INPUT input)
{
    VS_OUTPUT Output;
    
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul(input.Position, g_mWorldViewProjection);
    
    return Output;
}

float4 RenderEmissivePS(VS_OUTPUT In) : SV_TARGET0
{
    return g_color;
}

technique11 SpotShadowGen
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, RenderEmissiveVS()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, RenderEmissivePS()));
    }
}
