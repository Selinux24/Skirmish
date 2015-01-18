#include "IncLights.fx"
#include "IncVertexFormats.fx"

#define PC_FIRE 1
#define PC_RAIN 2

#define PT_EMITTER 0
#define PT_FLARE 1

cbuffer cbPerFrame : register (b0)
{
	float gMaximumAge;
	float gEmitterAge;
	float gTotalTime;
	float gElapsedTime;
	float3 gAccelerationWorld;
	float3 gEyePositionWorld;
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	uint gTextureCount;
};

cbuffer cbFixed : register (b1)
{
	float2 gQuadTexC[4] = 
	{
		float2(0.0f, 1.0f),
		float2(0.0f, 0.0f),
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f)
	};
};

Texture2DArray gTextureArray;
Texture1D gTextureRandom;

float3 RandomVector3(float offset)
{
	//Use game time plus offset to sample random texture.
	float u = (gTotalTime + offset);
	
	return gTextureRandom.SampleLevel(SamplerLinear, u, 0).xyz;
}

VSVertexParticle VSStreamOut(VSVertexParticle input)
{
    return input;
}

[maxvertexcount(2)]
void GSStreamOutFire(point VSVertexParticle input[1], inout PointStream<VSVertexParticle> ptStream)
{
	input[0].age += gElapsedTime;
	
	if(input[0].type == PT_EMITTER)
	{
		if(input[0].age > gEmitterAge)
		{
			float3 vRandom = normalize(RandomVector3(0.0f));
			vRandom.x *= 0.33f;
			vRandom.z *= 0.33f;

			VSVertexParticle p;
			p.positionWorld = input[0].positionWorld;
			p.velocityWorld = vRandom * 1.5f * input[0].sizeWorld.x;
			p.sizeWorld = input[0].sizeWorld;
			p.age = 0.0f;
			p.type = PT_FLARE;

			ptStream.Append(p);
			
			input[0].age = 0.0f;
		}
		
		ptStream.Append(input[0]);
	}
	else
	{
		if(input[0].age <= gMaximumAge)
		{
			ptStream.Append(input[0]);
		}
	}
}

[maxvertexcount(6)]
void GSStreamOutRain(point VSVertexParticle input[1], inout PointStream<VSVertexParticle> ptStream)
{
	input[0].age += gElapsedTime;
	
	if(input[0].type == PT_EMITTER)
	{
		if(input[0].age > gEmitterAge)
		{
			for(int i = 0; i < 5; ++i)
			{
				float3 vRandom = 35.0f * RandomVector3((float)i / 5.0f);
				vRandom.y = (gEyePositionWorld.y - (gAccelerationWorld.y * gMaximumAge)) * 0.5f;

				VSVertexParticle p;
				p.positionWorld = gEyePositionWorld.xyz + vRandom;
				p.velocityWorld = gAccelerationWorld;
				p.sizeWorld = input[0].sizeWorld;
				p.age = 0.0f;
				p.type = PT_FLARE;

				ptStream.Append(p);
			}

			input[0].age = 0.0f;
		}

		ptStream.Append(input[0]);
	}
	else
	{
		if(input[0].age <= gMaximumAge)
		{
			ptStream.Append(input[0]);
		}
	}
}

GSParticleFire VSDrawFire(VSVertexParticle input)
{
	float t = input.age;
	float opacity = 1.0f - smoothstep(0.0f, 1.0f, t / 1.0f);
	
	float3 pos = 0.5f * t * t * gAccelerationWorld + t * input.velocityWorld + input.positionWorld;

	GSParticleFire output;
	output.positionWorld = mul(float4(pos, 1), gWorld).xyz;
	output.color = float4(1.0f, 1.0f, 1.0f, opacity);
	output.sizeWorld = input.sizeWorld;
	output.type  = input.type;
	
	return output;
}

GSParticleRain VSDrawRain(VSVertexParticle input)
{
	float t = input.age;

	float3 pos = 0.5f * t * t * gAccelerationWorld + t * input.velocityWorld + input.positionWorld;

	GSParticleRain output;
	output.positionWorld = mul(float4(pos, 1), gWorld).xyz;
	output.type  = input.type;
	
	return output;
}

[maxvertexcount(4)]
void GSDrawFire(point GSParticleFire input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSParticleFire> triStream)
{
	if(input[0].type != PT_EMITTER)
	{
		float3 look  = normalize(gEyePositionWorld.xyz - input[0].positionWorld);
		float3 right = normalize(cross(float3(0, 1, 0), look));
		float3 up    = cross(look, right);

		float halfWidth  = 0.5f * input[0].sizeWorld.x;
		float halfHeight = 0.5f * input[0].sizeWorld.y;
	
		float4 v[4];
		v[0] = float4(input[0].positionWorld + halfWidth * right - halfHeight * up, 1.0f);
		v[1] = float4(input[0].positionWorld + halfWidth * right + halfHeight * up, 1.0f);
		v[2] = float4(input[0].positionWorld - halfWidth * right - halfHeight * up, 1.0f);
		v[3] = float4(input[0].positionWorld - halfWidth * right + halfHeight * up, 1.0f);
		
		PSParticleFire output;
		[unroll]
		for(int i = 0; i < 4; ++i)
		{
			v[i].y += halfHeight;

			output.positionHomogeneous = mul(v[i], gWorldViewProjection);
			output.tex = gQuadTexC[i];
			output.color = input[0].color;
			output.primitiveID = primID;
			
			triStream.Append(output);
		}
	}
}

[maxvertexcount(2)]
void GSDrawRain(point GSParticleRain input[1], uint primID : SV_PrimitiveID, inout LineStream<PSParticleRain> lineStream)
{
	if( input[0].type != PT_EMITTER )
	{
		float3 p0 = input[0].positionWorld;
		float3 p1 = input[0].positionWorld + 0.07f * gAccelerationWorld;
		
		PSParticleRain v0;
		v0.positionHomogeneous = mul(float4(p0, 1.0f), gWorldViewProjection);
		v0.tex = float2(0.0f, 0.0f);
		v0.primitiveID = primID;
		lineStream.Append(v0);
		
		PSParticleRain v1;
		v1.positionHomogeneous = mul(float4(p1, 1.0f), gWorldViewProjection);
		v1.tex  = float2(1.0f, 1.0f);
		v1.primitiveID = primID;
		lineStream.Append(v1);
	}
}

float4 PSDrawFire(PSParticleFire input) : SV_TARGET
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);

	return gTextureArray.Sample(SamplerLinear, uvw) * input.color;
}

float4 PSDrawRain(PSParticleRain input) : SV_TARGET
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);

	return gTextureArray.Sample(SamplerLinear, uvw);
}

GeometryShader gsStreamOutFire = ConstructGSWithSO(CompileShader(gs_5_0, GSStreamOutFire()), "POSITION.xyz; VELOCITY.xyz; SIZE.xy; AGE.x; TYPE.x");

technique11 FireStreamOut
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSStreamOut()));
        SetGeometryShader(gsStreamOutFire);
        SetPixelShader(NULL);
        SetDepthStencilState(StencilDisableDepth, 0);
    }
}

GeometryShader gsStreamOutRain = ConstructGSWithSO(CompileShader(gs_5_0, GSStreamOutRain()), "POSITION.xyz; VELOCITY.xyz; SIZE.xy; AGE.x; TYPE.x");

technique11 RainStreamOut
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSStreamOut()));
        SetGeometryShader(gsStreamOutRain);
        SetPixelShader(NULL);
        SetDepthStencilState(StencilDisableDepth, 0);
    }
}

technique11 FireDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawFire()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawFire()));
        SetPixelShader(CompileShader(ps_5_0, PSDrawFire()));

        SetBlendState(BlendAdditive, float4(0.0f, 0.0f, 0.0f, 0.0f), 0xffffffff);
        SetDepthStencilState(StencilNoDepthWrites, 0);
    }
}

technique11 RainDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawRain()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawRain()));
        SetPixelShader(CompileShader(ps_5_0, PSDrawRain()));

        SetDepthStencilState(StencilNoDepthWrites, 0);
    }
}
