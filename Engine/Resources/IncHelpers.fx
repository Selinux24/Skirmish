
float2 BillboardTexCoords[8] =
{
    float2(0.0f, 1.0f),
	float2(0.0f, 0.0f),
	float2(1.0f, 1.0f),
	float2(1.0f, 0.0f),
    float2(1.0f, 1.0f),
	float2(1.0f, 0.0f),
	float2(0.0f, 1.0f),
	float2(0.0f, 0.0f)
};

void BuildQuad(float3 centerWorld, float halfWidth, float halfHeight, float3 up, float3 right, float3 displacement, inout float4 vertices[4])
{
    vertices[0] = float4(centerWorld + halfWidth * right - halfHeight * up, 1.0f) + float4(displacement, 0.0f);
    vertices[1] = float4(centerWorld + halfWidth * right + halfHeight * up, 1.0f) + float4(displacement, 0.0f);
    vertices[2] = float4(centerWorld - halfWidth * right - halfHeight * up, 1.0f) + float4(displacement, 0.0f);
    vertices[3] = float4(centerWorld - halfWidth * right + halfHeight * up, 1.0f) + float4(displacement, 0.0f);
}

float3 CalcWindTranslation(float totalTime, float random, float3 pos, float3 windDirection, float windStrength)
{
    float3 vWind = sin(totalTime + (pos.x + pos.y + pos.z) * 0.1f) + (windDirection * windStrength);

    return pos + (vWind * min(1, random));
}

float Hash(float2 p)
{
    float h = dot(p, float2(127.1, 311.7));
    return frac(sin(h) * 43758.5453123);
}
float Noise(in float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    return -1.0 + 2.0 * lerp(lerp(Hash(i + float2(0.0, 0.0)), Hash(i + float2(1.0, 0.0)), u.x), lerp(Hash(i + float2(0.0, 1.0)), Hash(i + float2(1.0, 1.0)), u.x), u.y);
}