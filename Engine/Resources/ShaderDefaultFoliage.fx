#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"

cbuffer cbGlobal : register(b0)
{
    uint gMaterialPaletteWidth;
    float3 gLOD;
};
Texture2D gMaterialPalette : register(t0);
Texture1D gTextureRandom : register(t1);

cbuffer cbPerFrame : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	HemisphericLight gPSHemiLight;
	DirectionalLight gDirLights[MAX_LIGHTS_DIRECTIONAL];
	PointLight gPointLights[MAX_LIGHTS_POINT];
	SpotLight gSpotLights[MAX_LIGHTS_SPOT];
	uint3 gLightCount;
	uint PAD11;
	float4 gFogColor;
	float gFogStart;
	float gFogRange;
	float gStartRadius;
	float gEndRadius;
    float3 gWindDirection;
	float gWindStrength;
	float3 gDelta;
	float gTotalTime;
	float3 gEyePositionWorld;
    float PAD12;
    uint gMaterialIndex;
    uint gTextureCount;
    uint gNormalMapCount;
    uint PAD13;
};
Texture2DArray<float> gShadowMapDir : register(t2);
Texture2DArray<float> gShadowMapSpot : register(t3);
TextureCubeArray<float> gShadowMapPoint : register(t4);
Texture2DArray gTextureArray : register(t5);
Texture2DArray gNormalMapArray : register(t6);

GSVertexBillboard VSFoliage(VSVertexBillboard input)
{
    GSVertexBillboard output;

    output.centerWorld = input.positionWorld;
    output.centerWorld.y += (input.sizeWorld.y * (0.5f + gDelta.y));
    output.sizeWorld = input.sizeWorld;

    return output;
}

[maxvertexcount(4)]
void GSFoliage4(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard> outputStream)
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

        if (gWindStrength > 0)
        {
            float sRandom = gTextureRandom.SampleLevel(SamplerPoint, primID, 0).x;
            v[1].xyz = CalcWindTranslation(gTotalTime, sRandom, v[1].xyz, gWindDirection, gWindStrength);
            v[3].xyz = CalcWindTranslation(gTotalTime, sRandom, v[3].xyz, gWindDirection, gWindStrength);
        }

		//Transform quad vertices to world space and output them as a triangle strip.
        PSVertexBillboard gout;
		[unroll]
        for (int i = 0; i < 4; ++i)
        {
            gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
            gout.positionWorld = mul(v[i], gWorld).xyz;
            gout.normalWorld = up;
            gout.tangentWorld = float3(1, 0, 0);
            gout.tex = BillboardTexCoords[i];
            gout.primitiveID = primID;

            outputStream.Append(gout);
        }
    }
}

[maxvertexcount(8)]
void GSFoliage8(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard> outputStream)
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
        PSVertexBillboard gout;
		[unroll]
        for (int i = 0; i < 8; ++i)
        {
            gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
            gout.positionWorld = mul(v[i], gWorld).xyz;
            gout.normalWorld = up;
            gout.tangentWorld = float3(1, 0, 0);
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
void GSFoliage16(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard> outputStream)
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
        PSVertexBillboard gout;
		[unroll]
        for (int i = 0; i < 16; ++i)
        {
            gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
            gout.positionWorld = mul(v[i], gWorld).xyz;
            gout.normalWorld = up;
            gout.tangentWorld = float3(1, 0, 0);
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

float4 PSFoliage(PSVertexBillboard input) : SV_Target
{
    float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);

    float4 diffuseColor = gTextureArray.Sample(SamplerLinear, uvw);

    float distToEye = length(gEyePositionWorld - input.positionWorld);
    float falloff = saturate(distToEye / gEndRadius);
    clip(diffuseColor.a - max(0.01f, falloff));

    float3 normalWorld = input.normalWorld;
    if (gNormalMapCount > 0)
    {
        float3 normalMap = gNormalMapArray.Sample(SamplerLinear, uvw).rgb;
        normalWorld = NormalSampleToWorldSpace(normalMap, input.normalWorld, input.tangentWorld);
    }

    Material material = GetMaterialData(gMaterialPalette, gMaterialIndex, gMaterialPaletteWidth);

    ComputeLightsInput lInput;

    lInput.material = material;
	lInput.objectPosition = input.positionWorld;
	lInput.objectNormal = normalWorld;
    lInput.objectDiffuseColor = diffuseColor;

	lInput.eyePosition = gEyePositionWorld;
	lInput.levelOfDetailRanges = gLOD;

	lInput.hemiLight = gPSHemiLight;
	lInput.dirLights = gDirLights;
    lInput.pointLights = gPointLights;
    lInput.spotLights = gSpotLights;
    lInput.dirLightsCount = gLightCount.x;
    lInput.pointLightsCount = gLightCount.y;
    lInput.spotLightsCount = gLightCount.z;

	lInput.shadowMapDir = gShadowMapDir;
    lInput.shadowMapPoint = gShadowMapPoint;
	lInput.shadowMapSpot = gShadowMapSpot;

	lInput.fogStart = gFogStart;
	lInput.fogRange = gFogRange;
	lInput.fogColor = gFogColor;

    return ComputeLights(lInput);
}

technique11 ForwardFoliage4
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSFoliage()));
        SetGeometryShader(CompileShader(gs_5_0, GSFoliage4()));
        SetPixelShader(CompileShader(ps_5_0, PSFoliage()));
    }
}

technique11 ForwardFoliage8
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSFoliage()));
        SetGeometryShader(CompileShader(gs_5_0, GSFoliage8()));
        SetPixelShader(CompileShader(ps_5_0, PSFoliage()));
    }
}

technique11 ForwardFoliage16
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSFoliage()));
        SetGeometryShader(CompileShader(gs_5_0, GSFoliage16()));
        SetPixelShader(CompileShader(ps_5_0, PSFoliage()));
    }
}
