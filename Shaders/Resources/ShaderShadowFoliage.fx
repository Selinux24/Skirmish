#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorldViewProjection;
    float3 gEyePositionWorld;
    float gStartRadius;
    float gEndRadius;
    float3 gWindDirection;
    float gWindStrength;
    float gTotalTime;
    float2 gPAD01;
    float3 gDelta;
    float gPAD02;
};
cbuffer cbPerObject : register(b1)
{
    uint gTextureCount;
    uint3 gPAD11;
};
Texture2DArray gTextureArray : register(t0);
Texture1D gTextureRandom : register(t1);

GSVertexBillboard VSFoliage(VSVertexBillboard input)
{
    GSVertexBillboard output;

    output.centerWorld = input.positionWorld;
    output.centerWorld.y += (input.sizeWorld.y * (0.5f + gDelta.y));
    output.sizeWorld = input.sizeWorld;

    return output;
}

[maxvertexcount(4)]
void GSFoliage4(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSShadowMapBillboard> outputStream)
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
        float4 v[4] =
        {
            float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0)
        };
        BuildQuad(input[0].centerWorld, halfWidth, halfHeight, up, right, 0, v);

        if (gWindStrength > 0)
        {
            float sRandom = gTextureRandom.SampleLevel(SamplerPoint, primID, 0).x;
            v[1].xyz = CalcWindTranslation(gTotalTime, sRandom, v[1].xyz, gWindDirection, gWindStrength);
            v[3].xyz = CalcWindTranslation(gTotalTime, sRandom, v[3].xyz, gWindDirection, gWindStrength);
        }

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

[maxvertexcount(8)]
void GSFoliage8(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSShadowMapBillboard> outputStream)
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
        float4 v[8] =
        {
            float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
            float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0)
        };
        float3 p[2] =
        {
            float3(+gDelta.x, 0.0f, +gDelta.z),
            float3(-gDelta.x, 0.0f, -gDelta.z)
        };

        float sRandom = gTextureRandom.SampleLevel(SamplerPoint, primID, 0).x;
        int index = 0;
        for (int g = 0; g < 2; g++)
        {
            float4 tmp[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
            BuildQuad(input[0].centerWorld, halfWidth, halfHeight, up, right, p[g], tmp);

            if (gWindStrength > 0)
            {
                tmp[1].xyz = CalcWindTranslation(gTotalTime, sRandom, tmp[1].xyz, gWindDirection, gWindStrength);
                tmp[3].xyz = CalcWindTranslation(gTotalTime, sRandom, tmp[3].xyz, gWindDirection, gWindStrength);
            }

            v[index++] = tmp[0];
            v[index++] = tmp[1];
            v[index++] = tmp[2];
            v[index++] = tmp[3];
        }

		//Transform quad vertices to world space and output them as a triangle strip.
        PSShadowMapBillboard gout;
		[unroll]
        for (int i = 0; i < 8; ++i)
        {
            gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
            gout.depth = v[i];
            gout.tex = BillboardTexCoords[i];
            gout.primitiveID = primID;

            outputStream.Append(gout);

            if (i % 4 == 3)
            {
                outputStream.RestartStrip();
            }
        }
    }
}

[maxvertexcount(16)]
void GSFoliage16(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSShadowMapBillboard> outputStream)
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
        float4 v[16] =
        {
            float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
            float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
            float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0),
            float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0)
        };
        float3 p[4] =
        {
            float3(+gDelta.x, 0.0f, +gDelta.z),
            float3(+gDelta.x, 0.0f, -gDelta.z),
            float3(-gDelta.x, 0.0f, +gDelta.z),
            float3(-gDelta.x, 0.0f, -gDelta.z)
        };

        float sRandom = gTextureRandom.SampleLevel(SamplerPoint, primID, 0).x;
        int index = 0;
        for (int g = 0; g < 4; g++)
        {
            float4 tmp[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
            BuildQuad(input[0].centerWorld, halfWidth, halfHeight, up, right, p[g], tmp);

            if (gWindStrength > 0)
            {
                tmp[1].xyz = CalcWindTranslation(gTotalTime, sRandom, tmp[1].xyz, gWindDirection, gWindStrength);
                tmp[3].xyz = CalcWindTranslation(gTotalTime, sRandom, tmp[3].xyz, gWindDirection, gWindStrength);
            }

            v[index++] = tmp[0];
            v[index++] = tmp[1];
            v[index++] = tmp[2];
            v[index++] = tmp[3];
        }

		//Transform quad vertices to world space and output them as a triangle strip.
        PSShadowMapBillboard gout;
		[unroll]
        for (int i = 0; i < 16; ++i)
        {
            gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
            gout.depth = v[i];
            gout.tex = BillboardTexCoords[i % 8];
            gout.primitiveID = primID;

            outputStream.Append(gout);

            if (i % 4 == 3)
            {
                outputStream.RestartStrip();
            }
        }
    }
}

float4 PSFoliage(PSShadowMapBillboard input) : SV_Target
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

technique11 ShadowMapFoliage4
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSFoliage()));
        SetGeometryShader(CompileShader(gs_5_0, GSFoliage4()));
        SetPixelShader(CompileShader(ps_5_0, PSFoliage()));
    }
}

technique11 ShadowMapFoliage8
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSFoliage()));
        SetGeometryShader(CompileShader(gs_5_0, GSFoliage8()));
        SetPixelShader(CompileShader(ps_5_0, PSFoliage()));
    }
}

technique11 ShadowMapFoliage16
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSFoliage()));
        SetGeometryShader(CompileShader(gs_5_0, GSFoliage4()));
        SetPixelShader(CompileShader(ps_5_0, PSFoliage()));
    }
}
