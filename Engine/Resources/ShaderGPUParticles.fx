#include "IncLights.fx"
#include "IncVertexFormats.fx"

#define PT_EMITTER 0
#define PT_FLARE 1

cbuffer cbPerFrame : register (b0)
{
	float4x4 gViewProjection;
	float3 gEyePositionWorld;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;
};
cbuffer cbPerEmission : register (b1)
{
	uint gTextureCount;
	float gEnergyMin;
	float gEnergyMax;

	float4 gParticleOrbit;

	float gSizeStartMin;
	float gSizeStartMax;
	float gSizeEndMin;
	float gSizeEndMax;

	float4 gParticleColorStart;
	float4 gParticleColorStartVariance;
	float4 gParticleColorEnd;
	float4 gParticleColorEndVariance;

	float3 gParticlePosition;
	float3 gParticlePositionVariance;
	float3 gParticleVelocity;
	float3 gParticleVelocityVariance;
	float3 gParticleAcceleration;
	float3 gParticleAccelerationVariance;
};
cbuffer cbPerUpdate : register (b2)
{
	float gTotalTime;
	float gElapsedTime;
	float gEmissionRate;
};
cbuffer cbFixed : register (b3)
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

float GenerateScalar(float min, float max, float offset)
{
	return RandomScalar(min, max, offset, gTextureRandom);
}
float2 GenerateVector2(float2 base, float2 variance, float offset)
{
	float2 rnd = RandomVector2(-1, 1, offset, gTextureRandom);

	float2 res;
	res.x = base.x + (variance.x * rnd.x);
	res.y = base.y + (variance.y * rnd.y);
	return res;
}
float3 GenerateVector3(float3 base, float3 variance, float offset)
{
	float3 rnd = RandomVector3(-1, 1, offset, gTextureRandom);

	float3 res;
	res.x = base.x + (variance.x * rnd.x);
	res.y = base.y + (variance.y * rnd.y);
	res.z = base.z + (variance.z * rnd.z);
	return res;
}
float4 GenerateVector4(float4 base, float4 variance, float offset)
{
	float4 rnd = RandomVector4(-1, 1, offset, gTextureRandom);

	float4 res;
	res.x = base.x + (variance.x * rnd.x);
	res.y = base.y + (variance.y * rnd.y);
	res.z = base.z + (variance.z * rnd.z);
	res.w = base.w + (variance.w * rnd.w);
	return res;
}
float3 GenerateEllipsoidVector3(float3 center, float3 scale, float offset)
{
	float3 res = RandomVector3(offset++, gTextureRandom);
	while(length(res)>1)
	{
		res = RandomVector3(offset++, gTextureRandom);
	}
	return (res * scale) + center;
}

/***********************************************************
 * STREAM OUT                                              *
 ***********************************************************/

VSVertexGPUParticle VSStreamOut(VSVertexGPUParticle input)
{
    return input;
}

[maxvertexcount(2)]
void GSStreamOut(point VSVertexGPUParticle input[1], inout PointStream<VSVertexGPUParticle> ptStream)
{
	if(input[0].type == PT_EMITTER)
	{
		input[0].energy += gElapsedTime;
	
		if(input[0].energy > gEmissionRate)
		{
			float seed = gTotalTime + input[0].energy;

			//Initializes a new particle
			float4 colorStart = GenerateVector4(gParticleColorStart, gParticleColorStartVariance, seed++);
			float4 colorEnd = GenerateVector4(gParticleColorEnd, gParticleColorEndVariance, seed++);
			float sizeStart = GenerateScalar(gSizeStartMin, gSizeStartMax, seed++);
			float sizeEnd = GenerateScalar(gSizeEndMin, gSizeEndMax, seed++);

			float3 position;
			if (gParticleOrbit.w == 0.0f)
			{
				position = GenerateVector3(gParticlePosition, gParticlePositionVariance, seed++);
			}
			else
			{
				position = GenerateEllipsoidVector3(gParticlePosition, gParticlePositionVariance, seed++);
			}
			float3 velocity = GenerateVector3(gParticleVelocity, gParticleVelocityVariance, seed++);
			float3 acceleration = GenerateVector3(gParticleAcceleration, gParticleAccelerationVariance, seed++);

			float energy = GenerateScalar(gEnergyMin, gEnergyMax, seed++);

			VSVertexGPUParticle p;
			p.position = position;
			p.velocity = velocity;
			p.acceleration = acceleration;
			p.size = float2(sizeStart, sizeStart);
			p.sizeStart = float2(sizeStart, sizeStart);
			p.sizeEnd = float2(sizeEnd, sizeEnd);
			p.color = colorStart;
			p.colorStart = colorStart;
			p.colorEnd = colorEnd;
			p.energy = energy;
			p.energyStart = energy;
			p.type = PT_FLARE;

			ptStream.Append(p);
			
			input[0].energy = 0.0f;
		}
		
		ptStream.Append(input[0]);
	}
	else
	{
		input[0].energy -= gElapsedTime;

		if(input[0].energy > 0)
		{
			//Update state
			float percent = 1.0f - (input[0].energy / input[0].energyStart);

			input[0].velocity += input[0].acceleration * gElapsedTime;
            input[0].position += input[0].velocity * gElapsedTime;
			input[0].color = input[0].colorStart + (input[0].colorEnd - input[0].colorStart) * percent;
			input[0].size = input[0].sizeStart + (input[0].sizeEnd - input[0].sizeStart) * percent;

			ptStream.Append(input[0]);
		}
	}
}

GeometryShader gsStreamOut = ConstructGSWithSO(CompileShader(gs_5_0, GSStreamOut()), "POSITION.xyz; COLOR0.rgba; COLOR_START.rgba; COLOR_END.rgba; VELOCITY.xyz; ACCELERATION.xyz; SIZE.xy; SIZE_START.xy; SIZE_END.xy; ENERGY.x; ENERGY_START.x; TYPE.x");

technique11 ParticleStreamOut
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSStreamOut()));
        SetGeometryShader(gsStreamOut);
        SetPixelShader(NULL);
    }
}

/***********************************************************
 * DRAWING FROM STREAM OUT                                 *
 ***********************************************************/

GSGPUParticle VSDrawParticle(VSVertexGPUParticle input)
{
	GSGPUParticle output;
	output.positionWorld = input.position;
	output.color = input.color;
	output.sizeWorld = input.size;
	output.type  = input.type;
	
	return output;
}

[maxvertexcount(4)]
void GSDrawSolid(point GSGPUParticle input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSGPUParticle> triStream)
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
		
		PSGPUParticle output;
		[unroll]
		for(int i = 0; i < 4; ++i)
		{
			v[i].y += halfHeight;

			output.positionHomogeneous = mul(v[i], gViewProjection);
			output.positionWorld = v[i].xyz;
			output.tex = gQuadTexC[i];
			output.color = input[0].color;
			output.primitiveID = primID;
			
			triStream.Append(output);
		}
	}
}
[maxvertexcount(2)]
void GSDrawLine(point GSGPUParticle input[1], uint primID : SV_PrimitiveID, inout LineStream<PSGPUParticle> lineStream)
{
	if( input[0].type != PT_EMITTER )
	{
		float3 p0 = input[0].positionWorld;
		float3 p1 = input[0].positionWorld + 0.07f;
		
		PSGPUParticle v0;
		v0.positionHomogeneous = mul(float4(p0, 1.0f), gViewProjection);
		v0.positionWorld = p0;
		v0.color = input[0].color;
		v0.tex = float2(0.0f, 0.0f);
		v0.primitiveID = primID;
		lineStream.Append(v0);
		
		PSGPUParticle v1;
		v1.positionHomogeneous = mul(float4(p1, 1.0f), gViewProjection);
		v1.positionWorld = p1;
		v1.color = input[0].color;
		v1.tex  = float2(1.0f, 1.0f);
		v1.primitiveID = primID;
		lineStream.Append(v1);
	}
}

float4 PSDrawParticle(PSGPUParticle input) : SV_TARGET
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);

	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw) * input.color;

	float3 litColor = textureColor.rgb;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor.rgb);
	}

	return float4(litColor, textureColor.a);
}
GBufferPSOutput PSDeferredDrawParticle(PSGPUParticle input)
{
	GBufferPSOutput output = (GBufferPSOutput)0;

	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw) * input.color;
	float3 color = textureColor.rgb;
	
	output.color = float4(color, textureColor.a);
	output.normal = 0.0f;
	output.depth = float4(input.positionHomogeneous.xyz, 0.0f);
	
	return output;
}

technique11 SolidDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawParticle()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawSolid()));
        SetPixelShader(CompileShader(ps_5_0, PSDrawParticle()));
    }
}
technique11 LineDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawParticle()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawLine()));
        SetPixelShader(CompileShader(ps_5_0, PSDrawParticle()));
    }
}

technique11 DeferredSolidDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawParticle()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawSolid()));
        SetPixelShader(CompileShader(ps_5_0, PSDeferredDrawParticle()));
    }
}
technique11 DeferredLineDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawParticle()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawLine()));
        SetPixelShader(CompileShader(ps_5_0, PSDeferredDrawParticle()));
    }
}