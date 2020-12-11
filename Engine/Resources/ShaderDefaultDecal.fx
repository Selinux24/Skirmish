#include "IncLights.hlsl"
#include "IncVertexFormats.hlsl"

cbuffer cbPerFrame : register(b0)
{
    float4x4 gWorld;
    float4x4 gWorldViewProjection;
    uint gTextureCount;
    float gTotalTime;
};
cbuffer cbFixed : register(b1)
{
    float2 gQuadTexC[4] =
    {
        float2(0.0f, 1.0f),
		float2(0.0f, 0.0f),
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f)
    };
};

Texture2DArray gTextureArray : register(t0);

float4 ComputeParticleRotation(float rotation)
{
    float rot = rotation % TWO_PI;
    float c = cos(rot);
    float s = sin(rot);
    
    float4 rotationMatrix = float4(c, -s, s, c);
    
    rotationMatrix *= 0.5f;
    rotationMatrix += 0.5f;
    
    return rotationMatrix;
}

GSDecal VSDecals(VSVertexDecal input)
{
    GSDecal output;

    //Move to zero age and normalize from 0 to 1
    float currentAge = gTotalTime - input.startTime;
    float normalizedAge = saturate(currentAge / input.maxAge);

    output.centerWorld = input.positionWorld;
    output.normalWorld = input.normalWorld;
    output.rotationWorld = 0;
    output.sizeWorld = input.sizeWorld;
    output.alpha = 1 * (1.0 - normalizedAge);

    return output;
}
GSDecal VSDecalsRotated(VSVertexDecal input)
{
    GSDecal output;

    //Move to zero age and normalize from 0 to 1
    float currentAge = gTotalTime - input.startTime;
    float normalizedAge = saturate(currentAge / input.maxAge);

    output.centerWorld = input.positionWorld;
    output.normalWorld = input.normalWorld;
    output.rotationWorld = ComputeParticleRotation(input.startTime);
    output.sizeWorld = input.sizeWorld;
    output.alpha = 1 * (1.0 - normalizedAge);

    return output;
}

[maxvertexcount(4)]
void GSDecals(point GSDecal input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSDecal> outputStream)
{
    float3 centerWorld = input[0].centerWorld;
    float3 normalWorld = input[0].normalWorld;
    float2 sizeWorld = input[0].sizeWorld;
    float4 rotationWorld = input[0].rotationWorld;
    float alpha = input[0].alpha;
    
	//Compute the local coordinate system of the sprite relative to the normal world
    normalWorld = normalize(normalWorld);
    float3 unit = abs(normalWorld.y) > 0.9998 ? float3(1, 0, 0) : float3(0, 1, 0);
    float3 right = normalize(cross(unit, normalWorld));
    float3 up = normalize(cross(normalWorld, right));

	//Compute triangle strip vertices (quad) in world space.
    float halfWidth = 0.5f * sizeWorld.x;
    float halfHeight = 0.5f * sizeWorld.y;
    float4 v[4] = { float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0) };
    v[0] = float4(centerWorld + halfWidth * right - halfHeight * up, 1.0f);
    v[1] = float4(centerWorld + halfWidth * right + halfHeight * up, 1.0f);
    v[2] = float4(centerWorld - halfWidth * right - halfHeight * up, 1.0f);
    v[3] = float4(centerWorld - halfWidth * right + halfHeight * up, 1.0f);

	//Transform quad vertices to world space and output them as a triangle strip.
    PSDecal gout;
	[unroll]
    for (int i = 0; i < 4; ++i)
    {
        gout.positionHomogeneous = mul(v[i], gWorldViewProjection);
        gout.positionWorld = mul(v[i], gWorld).xyz;
        gout.rotationWorld = rotationWorld;
        gout.alpha = alpha;
        gout.tex = gQuadTexC[i];
        gout.primitiveID = primID;

        outputStream.Append(gout);
    }
}

float4 PSDecals(PSDecal input) : SV_Target
{
    float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
    float4 color = gTextureArray.Sample(SamplerPointParticle, uvw);
    color.a *= input.alpha;
	
    return color;
}
float4 PSDecalsRotated(PSDecal input) : SV_Target
{
    float2 tex = input.tex;
    float4 rot = (input.rotationWorld * 2.0f) - 1.0f;
    
    tex -= 0.5f;
    tex = mul(tex, float2x2(rot));
    tex *= sqrt(2.0f);
    tex += 0.5f;
    
    float3 uvw = float3(tex, input.primitiveID % gTextureCount);
    float4 color = gTextureArray.Sample(SamplerPointParticle, uvw);
    color.a *= input.alpha;
	
    return color;
}

technique11 Decal
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDecals()));
        SetGeometryShader(CompileShader(gs_5_0, GSDecals()));
        SetPixelShader(CompileShader(ps_5_0, PSDecals()));
    }
}
technique11 DecalRotated
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDecalsRotated()));
        SetGeometryShader(CompileShader(gs_5_0, GSDecals()));
        SetPixelShader(CompileShader(ps_5_0, PSDecalsRotated()));
    }
}
