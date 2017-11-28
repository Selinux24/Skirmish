#include "IncHelpers.fx"
#include "IncLights.fx"
#include "IncVertexFormats.fx"

cbuffer cbVSPerFrame : register(b1)
{
    float4x4 gVSWorld;
    float4x4 gVSWorldViewProjection;
};

cbuffer cbPSPerFrame : register(b3)
{
    float3 gPSEyePositionWorld;
    float3 gPSLightDirection;
    float3 gPSBaseColor;
    float3 gPSWaterColor;
    float4 gPSWaveParams;
    float gPSTotalTime;
    uint3 gPSIters;
};

static const float2x2 octave_m = float2x2(1.6, 1.2, -1.2, 1.6);

// lighting
float diffuse(float3 n, float3 l)
{
    return max(0, dot(n, l)) * 0.4 + 0.6;
}
float specular(float3 n, float3 l, float3 e, float s)
{
    float nrm = (s + 8.0) / (PI * 8.0);
    return pow(max(dot(reflect(e, n), l), 0.0), s) * nrm;
}

float3 getSkyColor(float3 eyeDir)
{
    eyeDir.y = max(eyeDir.y, 0.0);
    return float3(pow(1.0 - eyeDir.y, 2.0), 1.0 - eyeDir.y, 0.6 + (1.0 - eyeDir.y) * 0.4);
}

// sea
float sea_octave(float2 uv, float choppy)
{
    uv += Noise(uv);
    float2 wv = 1.0 - abs(sin(uv));
    float2 swv = abs(cos(uv));
    wv = lerp(wv, swv, wv);
    float v = 1.0 - pow(max(0, wv.x * wv.y), 0.65);
    return pow(max(0, v), choppy);
}

float map(float3 position, float time)
{
    float freq = gPSWaveParams.w;
    float amp = gPSWaveParams.x;
    float choppy = gPSWaveParams.y;
    float2 uv = position.xz;
    uv.x *= 0.75;

    float d, h = 0.0;
    for (uint i = 0; i < gPSIters.y; i++)
    {
        d = sea_octave((uv + time) * freq, choppy);
        d += sea_octave((uv - time) * freq, choppy);
        h += d * amp;
        uv = mul(uv, octave_m);
        freq *= 1.9;
        amp *= 0.22;
        choppy = lerp(choppy, 1.0, 0.2);
    }
    return position.y - h;
}

float map_detailed(float3 position, float time)
{
    float freq = gPSWaveParams.w;
    float amp = gPSWaveParams.x;
    float choppy = gPSWaveParams.y;
    float2 uv = position.xz;
    uv.x *= 0.75;

    float d, h = 0.0;
    for (uint i = 0; i < gPSIters.z; i++)
    {
        d = sea_octave((uv + time) * freq, choppy);
        d += sea_octave((uv - time) * freq, choppy);
        h += d * amp;
        uv = mul(uv, octave_m);
        freq *= 1.9;
        amp *= 0.22;
        choppy = lerp(choppy, 1.0, 0.2);
    }
    return position.y - h;
}

float3 getSeaColor(float3 position, float3 normal, float3 lightDir, float3 eyeDir, float epsilon)
{
    float fresnel = clamp(1.0 - dot(normal, -eyeDir), 0.0, 1.0);
    fresnel = pow(fresnel, 3.0) * 0.65;
        
    float3 reflected = getSkyColor(reflect(eyeDir, normal));
    float3 refracted = gPSBaseColor + pow(diffuse(normal, lightDir), 80.0) * gPSWaterColor * 0.12;
    
    float3 color = lerp(refracted, reflected, fresnel);
    
    float atten = max(1.0 - epsilon, 0.0);
    color += gPSWaterColor * (position.y - gPSWaveParams.x) * 0.18 * atten;
    
    float spec = specular(normal, lightDir, eyeDir, 60.0);
    color += float3(spec, spec, spec);
    
    return color;
}

float3 getNormal(float3 postion, float epsilon, float time)
{
    float3 n;
    n.y = map_detailed(postion, time);
    n.x = map_detailed(float3(postion.x + epsilon, postion.y, postion.z), time) - n.y;
    n.z = map_detailed(float3(postion.x, postion.y, postion.z + epsilon), time) - n.y;
    n.y = epsilon;
    return normalize(n);
}

float3 heightMapTracing(float3 eyePos, float3 eyeDir, float time)
{
    float3 p = 0;

    float tm = 0.0;
    float tx = 1000.0;
    float hx = map(eyePos + eyeDir * tx, time);
    if (hx > 0.0)
    {
        return tx;
    }

    float hm = map(eyePos + eyeDir * tm, time);
    float tmid = 0.0;
    for (uint i = 0; i < gPSIters.x; i++)
    {
        tmid = lerp(tm, tx, hm / (hm - hx));
        p = eyePos + eyeDir * tmid;
        float hmid = map(p, time);
        if (hmid < 0.0)
        {
            tx = tmid;
            hx = hmid;
        }
        else
        {
            tm = tmid;
            hm = hmid;
        }
    }

    return p;
}

PSVertexPosition VSWater(VSVertexPosition input)
{
    PSVertexPosition output = (PSVertexPosition) 0;

    output.positionHomogeneous = mul(float4(input.positionLocal, 1), gVSWorldViewProjection);
    output.positionWorld = mul(float4(input.positionLocal, 1), gVSWorld).xyz;

    return output;
}

float4 PSWater(PSVertexPosition input) : SV_TARGET
{
    float3 eyePos = gPSEyePositionWorld;
    float3 eyeDir = -normalize(eyePos - input.positionWorld);

    // time    
    float time = (1.0f + gPSTotalTime * gPSWaveParams.z);
    
    // tracing
    float3 hmPosition = heightMapTracing(eyePos, eyeDir, time);
    float3 toPosition = hmPosition - eyePos;
    float epsilon = dot(toPosition, toPosition) * 0.00001f;
    float3 normal = getNormal(hmPosition, epsilon, time);

    float3 color = lerp(
        getSkyColor(eyeDir),
        getSeaColor(hmPosition, normal, normalize(gPSLightDirection), eyeDir, epsilon),
    	pow(smoothstep(0.0f, -0.05f, eyeDir.y), 0.3f));
        
    return float4(color, 1.0f);
}

technique11 Water
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSWater()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PSWater()));
    }
}
