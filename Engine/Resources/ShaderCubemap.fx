#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorldViewProjection;
	float3 gEyePositionWorld;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPointLights[MAX_LIGHTS_POINT];
	SpotLight gSpotLights[MAX_LIGHTS_SPOT];
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
};

TextureCube gCubemap;
Texture2D gShadowMap;

PSVertexPosition VSCubic(VSVertexPosition input)
{
	PSVertexPosition output;
	
	output.positionHomogeneous = mul(float4(input.positionLocal, 1.0f), gWorldViewProjection).xyww;
	output.positionLocal = input.positionLocal;
	
	return output;
}

float4 PSForwardCubic(PSVertexPosition input) : SV_Target
{
    float4 textureColor = gCubemap.Sample(SamplerLinear, input.positionLocal);

	float3 toEyeWorld = gEyePositionWorld - input.positionLocal;
	float3 toEye = normalize(toEyeWorld);

	float3 litColor = ComputeAllLights(
		gDirLights, 
		gPointLights, 
		gSpotLights,
		toEye,
		textureColor.rgb,
		input.positionLocal,
		float3(0.0f, 0.0f, 0.0f),
		0.0f,
		0.0f,
		float4(0.0f, 0.0f, 0.0f, 1.0f),
		gShadowMap);

	if(gFogRange > 0)
	{
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor.rgb);
	}

	return float4(litColor, textureColor.a);
}
GBufferPSOutput PSDeferredCubic(PSVertexPosition input)
{
    GBufferPSOutput output = (GBufferPSOutput)0;

	float4 color = gCubemap.Sample(SamplerLinear, input.positionLocal);

	output.color = color;
	output.normal = float4(0.0f, 0.0f, 0.0f, 0.0f);
	output.depth.xyz = input.positionLocal;
	output.depth.w = input.positionHomogeneous.z / input.positionHomogeneous.w;
	output.shadow = float4(0.0f, 0.0f, 0.0f, 1.0f);

    return output;
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
technique11 DeferredCubemap
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSCubic()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSDeferredCubic()));
    }
}
