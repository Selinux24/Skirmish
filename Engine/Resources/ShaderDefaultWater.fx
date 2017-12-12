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
    float3 gPSBaseColor;
    float3 gPSWaterColor;
    float4 gPSWaveParams;
    float gPSAmbient;
    float gPSFogRange;
    float gPSFogStart;
    float3 gPSFogColor;
    float gPSTotalTime;
    uint3 gPSIters;
    uint gPSLightCount;
    DirectionalLight gPSDirLights[MAX_LIGHTS_DIRECTIONAL];
};

static const float2x2 octaveMatrix = float2x2(1.6, 1.2, -1.6, 1.2);
static const float waterShinniness = 60.0f;
static const float specularPassNRM = (waterShinniness + 8.0) / (PI * 8.0);
static const float positionEpsilonNRM = (0.1f / 10000.0f);

// Sea geometry
float Octave(float2 uv, float choppy)
{
    uv += Noise(uv);
    float2 wv = 1.0 - abs(sin(uv));
    float2 swv = abs(cos(uv));
    wv = lerp(wv, swv, wv);
    float v = 1.0 - pow(max(0, wv.x * wv.y), 0.65);
    return pow(max(0, v), choppy);
}
float Map(float3 position, float time, uint iterations)
{
    float freq = gPSWaveParams.w;
    float amp = gPSWaveParams.x;
    float choppy = gPSWaveParams.y;
    float2 uv = position.xz;

    float h = 0.0;
    for (uint i = 0; i < iterations; i++)
    {
        float d = Octave((uv + time) * freq, choppy) + Octave((uv - time) * freq, choppy);

        h += d * amp;
        uv = mul(uv, octaveMatrix);
        freq *= 1.9;
        amp *= 0.22;
        choppy = lerp(choppy, 1.0, 0.2);
    }
    return position.y - h;
}
float3 GetNormal(float3 position, float epsilon, float time)
{
    float3 n;
    n.y = Map(position, time, gPSIters.z);
    n.x = Map(position + float3(epsilon, 0, 0), time, gPSIters.z) - n.y;
    n.z = Map(position + float3(0, 0, epsilon), time, gPSIters.z) - n.y;
    n.y = epsilon;
    return normalize(n);
}
float3 HeightMapTracing(float3 eyePos, float3 eyeDir, float time)
{
    float3 p = 0;

    float tm = 0.0;
    float tx = 1000.0;
    float hx = Map(eyePos + eyeDir * tx, time, gPSIters.y);
    if (hx > 0.0)
    {
        return tx;
    }

    float hm = Map(eyePos + eyeDir * tm, time, gPSIters.y);
    float tmid = 0.0;
    for (uint i = 0; i < gPSIters.x; i++)
    {
        tmid = lerp(tm, tx, hm / (hm - hx));
        p = eyePos + eyeDir * tmid;
        float hmid = Map(p, time, gPSIters.y);
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

// Water lighting
void GetLightColor(DirectionalLight light, float3 normal, float3 eyeDir, out float3 diffusePass, out float3 specularPass)
{
    float3 lightDir = normalize(-light.Direction);

    diffusePass = DiffusePass(light.Diffuse, -lightDir, normal).rgb;
    specularPass = SpecularBlinnPhongPass(light.Specular, waterShinniness, lightDir, normal, eyeDir).rgb;
}
float3 GetSkyColor(float3 eyeDir)
{
    eyeDir.y = max(eyeDir.y, 0.0);
    return float3(pow(1.0 - eyeDir.y, 2.0), 1.0 - eyeDir.y, 0.6 + (1.0 - eyeDir.y) * 0.4);
}
float3 GetSeaColor(float3 position, float3 normal, float3 eyeDir, float ambient, float3 diffuse, float3 specular, float epsilon)
{
    float3 refracted = (gPSBaseColor + pow(diffuse * 0.4 + float3(0.6, 0.6, 0.6), 80.0) * gPSWaterColor * 0.12) * clamp(ambient * 1.5f, 0.1, 1);
    float3 reflected = GetSkyColor(reflect(eyeDir, normal)) * clamp(ambient * 2, 0.1, 1);

    float attenuation = max(1.0 - epsilon, 0.0) * 0.18;
    float fresnel = clamp(1.0 - dot(normal, eyeDir), 0.0, 1.0);
    fresnel = pow(fresnel, 6.0) * 0.65;
    
    float3 color = lerp(refracted, reflected, fresnel);
    color += gPSWaterColor * (position.y - gPSWaveParams.x) * attenuation;
    color += specular * specularPassNRM;

    return color;
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
    float3 eyeDir = eyePos - input.positionWorld;
    float distToEye = length(eyeDir);
    eyeDir /= distToEye;

    // Get the current time    
    float time = (1.0f + gPSTotalTime * gPSWaveParams.z);
    
    // Get the geometry (position and normal)
    float3 hmPosition = HeightMapTracing(eyePos, -eyeDir, time);
    float3 toPosition = hmPosition - eyePos;
    float epsilon = dot(toPosition, toPosition) * positionEpsilonNRM;
    float3 hmNormal = GetNormal(hmPosition, epsilon, time);

    float fog = 0;
    if (gPSFogRange > 0)
    {
        fog = CalcFogFactor(distToEye, gPSFogStart, gPSFogRange);
    }

    if (fog >= 1)
    {
        return float4(gPSFogColor, 1);
    }
    else
    {
        // Do light color
        float3 lDiffuse = 0;
        float3 lSpecular = 0;
        if (gPSLightCount > 0)
        {
            for (uint i = 0; i < gPSLightCount.x; i++)
            {
                float3 diffuse = 0;
                float3 specular = 0;
                GetLightColor(gPSDirLights[i], hmNormal, eyeDir, diffuse, specular);
                lDiffuse += diffuse;
                lSpecular += specular;
            }
        }

        // Do sea color
        float3 color = GetSeaColor(hmPosition, hmNormal, eyeDir, gPSAmbient, saturate(lDiffuse), lSpecular, epsilon);

        return float4(lerp(color, gPSFogColor, fog), 1.0f);
    }
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
