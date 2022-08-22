#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

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
    float gShadowIntensity;
    float4 gFogColor;
    float gFogStart;
    float gFogRange;
    float gStartRadius;
    float gEndRadius;
    float3 gEyePositionWorld;
    float PAD11;
    uint gMaterialIndex;
    uint gTextureCount;
    uint gNormalMapCount;
    uint PAD12;
};
Texture2DArray<float> gShadowMapDir : register(t2);
Texture2DArray<float> gShadowMapSpot : register(t3);
TextureCubeArray<float> gShadowMapPoint : register(t4);
Texture2DArray gTextureArray : register(t5);
Texture2DArray gNormalMapArray : register(t6);

GSVertexBillboard VSBillboard(VSVertexBillboard input)
{
    GSVertexBillboard output;

    output.centerWorld = input.positionWorld;
    output.sizeWorld = input.sizeWorld;

    return output;
}

[maxvertexcount(4)]
void GSBillboard(point GSVertexBillboard input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSVertexBillboard> outputStream)
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

float4 PSForwardBillboard(PSVertexBillboard input) : SV_Target
{
    float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);

    float4 diffuseColor = gTextureArray.Sample(SamplerLinear, uvw);
    clip(diffuseColor.a - 0.01f);

    float3 normalWorld = normalize(input.normalWorld);
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
    lInput.minShadowIntensity = gShadowIntensity;

    lInput.fogStart = gFogStart;
    lInput.fogRange = gFogRange;
    lInput.fogColor = gFogColor;

    return ComputeLights(lInput);
}

technique11 ForwardBillboard
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSBillboard()));
        SetGeometryShader(CompileShader(gs_5_0, GSBillboard()));
        SetPixelShader(CompileShader(ps_5_0, PSForwardBillboard()));
    }
}
