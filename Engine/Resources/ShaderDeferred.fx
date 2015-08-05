
struct RenderVertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
	float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
};

struct RenderVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 Depth : TEXCOORD1;
    float3x3 tangentToWorld : TEXCOORD2;
};

RenderVertexShaderOutput RenderVertexShaderFunction(RenderVertexShaderInput input)
{
    RenderVertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors
    output.tangentToWorld[0] = mul(input.Tangent, World);
    output.tangentToWorld[1] = mul(input.Binormal, World);
    output.tangentToWorld[2] = mul(input.Normal, World);
    
	return output;
}

struct RenderPixelShaderOutput
{
    half4 Color : SV_TARGET0;
    half4 Normal : SV_TARGET1;
    half4 Depth : SV_TARGET2;
};

RenderPixelShaderOutput RenderPixelShaderFunction(RenderVertexShaderOutput input)
{
    RenderPixelShaderOutput output;

	// read specular attributes
    float4 specularAttributes = SpecularMap.Sample(diffuseSampler, input.TexCoord);
    
	float4 color;
	color = Texture.Sample(diffuseSampler, input.TexCoord); //output Color
	color.a = specularAttributes.r; //specular Intensity
    
    // read the normal from the normal map
    float3 normalFromMap = NormalMap.Sample(diffuseSampler, input.TexCoord);
    normalFromMap = 2.0f * normalFromMap - 1.0f; //tranform to [-1,1]
    normalFromMap = mul(normalFromMap, input.tangentToWorld); //transform into world space
    normalFromMap = normalize(normalFromMap); //normalize the result
   
    output.Color = color;
    output.Normal.rgb = 0.5f * (normalFromMap + 1.0f); //output the normal, in [0,1] space
    output.Normal.a = specularAttributes.a; //specular Power
    output.Depth = input.Depth.x / input.Depth.y;
    
	return output;
}

technique11 Render
{
    pass P0
    {
        VertexShader = compile vs_5_0 RenderVertexShaderFunction();
        PixelShader = compile ps_5_0 RenderPixelShaderFunction();
    }
}