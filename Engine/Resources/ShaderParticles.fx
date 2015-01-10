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

VSVertexParticle VSStreamOut(VSVertexParticle vin)
{
	return vin;
}

[maxvertexcount(2)]
void GSStreamOutFire(point VSVertexParticle gin[1], inout PointStream<VSVertexParticle> ptStream)
{
	gin[0].age += gElapsedTime;
	
	if(gin[0].type == PT_EMITTER)
	{
		if(gin[0].age > gEmitterAge)
		{
			float3 vRandom = normalize(RandomVector3(0.0f));
			vRandom.x *= 0.33f;
			vRandom.z *= 0.33f;

			VSVertexParticle p;
			p.positionWorld = gin[0].positionWorld;
			p.velocityWorld = vRandom * 1.5f * gin[0].sizeWorld.x;
			p.sizeWorld = gin[0].sizeWorld;
			p.age = 0.0f;
			p.type = PT_FLARE;

			ptStream.Append(p);
			
			gin[0].age = 0.0f;
		}
		
		ptStream.Append(gin[0]);
	}
	else
	{
		if(gin[0].age <= gMaximumAge)
		{
			ptStream.Append(gin[0]);
		}
	}
}

[maxvertexcount(6)]
void GSStreamOutRain(point VSVertexParticle gin[1], inout PointStream<VSVertexParticle> ptStream)
{
	gin[0].age += gElapsedTime;
	
	if(gin[0].type == PT_EMITTER)
	{
		if(gin[0].age > gEmitterAge)
		{
			for(int i = 0; i < 5; ++i)
			{
				float3 vRandom = 35.0f * RandomVector3((float)i / 5.0f);
				vRandom.y = (gEyePositionWorld.y - (gAccelerationWorld.y * gMaximumAge)) * 0.5f;

				VSVertexParticle p;
				p.positionWorld = gEyePositionWorld.xyz + vRandom;
				p.velocityWorld = gAccelerationWorld;
				p.sizeWorld = gin[0].sizeWorld;
				p.age = 0.0f;
				p.type = PT_FLARE;

				ptStream.Append(p);
			}

			gin[0].age = 0.0f;
		}

		ptStream.Append(gin[0]);
	}
	else
	{
		if(gin[0].age <= gMaximumAge)
		{
			ptStream.Append(gin[0]);
		}
	}
}

GSParticleFire VSDrawFire(VSVertexParticle vin)
{
	float t = vin.age;
	float opacity = 1.0f - smoothstep(0.0f, 1.0f, t / 1.0f);
	
	GSParticleFire vout;
	vout.positionWorld = 0.5f * t * t * gAccelerationWorld + t * vin.velocityWorld + vin.positionWorld;
	vout.color = float4(1.0f, 1.0f, 1.0f, opacity);
	vout.sizeWorld = vin.sizeWorld;
	vout.type  = vin.type;
	
	return vout;
}

GSParticleRain VSDrawRain(VSVertexParticle vin)
{
	float t = vin.age;

	GSParticleRain vout;
	vout.positionWorld = 0.5f * t * t * gAccelerationWorld + t * vin.velocityWorld + vin.positionWorld;
	vout.type  = vin.type;
	
	return vout;
}

[maxvertexcount(4)]
void GSDrawFire(point GSParticleFire gin[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSParticleFire> triStream)
{
	if(gin[0].type != PT_EMITTER)
	{
		float3 look  = normalize(gEyePositionWorld.xyz - gin[0].positionWorld);
		float3 right = normalize(cross(float3(0, 1, 0), look));
		float3 up    = cross(look, right);

		float halfWidth  = 0.5f * gin[0].sizeWorld.x;
		float halfHeight = 0.5f * gin[0].sizeWorld.y;
	
		float4 v[4];
		v[0] = float4(gin[0].positionWorld + halfWidth * right - halfHeight * up, 1.0f);
		v[1] = float4(gin[0].positionWorld + halfWidth * right + halfHeight * up, 1.0f);
		v[2] = float4(gin[0].positionWorld - halfWidth * right - halfHeight * up, 1.0f);
		v[3] = float4(gin[0].positionWorld - halfWidth * right + halfHeight * up, 1.0f);
		
		PSParticleFire gout;
		[unroll]
		for(int i = 0; i < 4; ++i)
		{
			gout.positionHomogeneus = mul(v[i], gWorldViewProjection);
			gout.tex = gQuadTexC[i];
			gout.color = gin[0].color;
			gout.primitiveID = primID;
			
			triStream.Append(gout);
		}
	}
}

[maxvertexcount(2)]
void GSDrawRain(point GSParticleRain gin[1], uint primID : SV_PrimitiveID, inout LineStream<PSParticleRain> lineStream)
{
	if( gin[0].type != PT_EMITTER )
	{
		float3 p0 = gin[0].positionWorld;
		float3 p1 = gin[0].positionWorld + 0.07f * gAccelerationWorld;
		
		PSParticleRain v0;
		v0.positionHomogeneus = mul(float4(p0, 1.0f), gWorldViewProjection);
		v0.tex = float2(0.0f, 0.0f);
		v0.primitiveID = primID;
		lineStream.Append(v0);
		
		PSParticleRain v1;
		v1.positionHomogeneus = mul(float4(p1, 1.0f), gWorldViewProjection);
		v1.tex  = float2(1.0f, 1.0f);
		v1.primitiveID = primID;
		lineStream.Append(v1);
	}
}

float4 PSDrawFire(PSParticleFire pin) : SV_TARGET
{
	float3 uvw = float3(pin.tex, pin.primitiveID % gTextureCount);

	return gTextureArray.Sample(SamplerLinear, uvw) * pin.color;
}

float4 PSDrawRain(PSParticleRain pin) : SV_TARGET
{
	float3 uvw = float3(pin.tex, pin.primitiveID % gTextureCount);

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
