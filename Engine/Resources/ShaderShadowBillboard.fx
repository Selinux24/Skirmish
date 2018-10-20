#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
	float4x4 gWorldViewProjection;
	float3 gEyePositionWorld;
	float gStartRadius;
	float gEndRadius;
    float3 gPAD01;
};
cbuffer cbPerObject : register(b1)
{
	uint gTextureCount;
    uint3 gPAD11;
};
Texture2DArray gTextureArray : register(t0);
Texture1D gTextureRandom : register(t1);

GSVertexBillboard VSBillboard(VSVertexBillboard input)
{
	GSVertexBillboard output;

	output.centerWorld = input.positionWorld;
	output.sizeWorld = input.sizeWorld;

	return output;
}

[maxvertexcount(4)]
void GSBillboard(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSShadowMapBillboard> outputStream)
{
    float3 look = gEyePositionWorld - input[0].centerWorld;
    if (gEndRadius == 0 || length(look) < gEndRadius)
    {
		//Compute the local coordinate system of the sprite relative to the world space such that the billboard is aligned with the y-axis and faces the eye.
        look.y = 0.0f; // y-axis aligned, so project to xz-plane
        look = normalize(look);
        float3 up = float3(0.0f, 1.0f, 0.0f);
        float3 right = cross(up, look);

		//Compute triangle strip vertices (quad) in world space.
        float halfWidth = 0.5f * input[0].sizeWorld.x;
        float halfHeight = 0.5f * input[0].sizeWorld.y;
        float4 v[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
        BuildQuad(input[0].centerWorld, halfWidth, halfHeight, up, right, 0, v);

		//Transform quad vertices to world space and output them as a triangle strip.
        PSShadowMapBillboard gout;
		[unroll]
        for (int i = 0; i < 4; ++i)
        {
            gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
            gout.depth = v[i];
            gout.tex = BillboardTexCoords[i];
            gout.primitiveID = primID;

            outputStream.Append(gout);
        }
    }
}

float4 PSBillboard(PSShadowMapBillboard input) : SV_Target
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw);

	if (textureColor.a > 0.8f)
	{
		float depthValue = input.depth.z / input.depth.w;

		return float4(depthValue, depthValue, depthValue, 1.0f);
	}
	else
	{
		discard;

		return 0.0f;
	}
}

technique11 ShadowMapBillboard
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VSBillboard()));
		SetGeometryShader(CompileShader(gs_5_0, GSBillboard()));
		SetPixelShader(CompileShader(ps_5_0, PSBillboard()));
	}
}
