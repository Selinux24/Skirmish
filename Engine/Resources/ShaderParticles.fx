#include "IncLights.fx"
#include "IncVertexFormats.fx"

#define PT_EMITTER 0
#define PT_FLARE 1

cbuffer cbPerFrame : register (b0)
{
	float4x4 gWorld;
	float4x4 gWorldViewProjection;
	float3 gEyePositionWorld;
	float gFogStart;
	float gFogRange;
	float4 gFogColor;

	float3 gPosition;
	float4x4 gRotation;
	float gElapsedTime;
};
cbuffer cbPerEmitter : register (b1)
{
	float gEmissionRate;

	float4 gParticleOrbit;
	float gParticleEllipsoid;
	float3 gParticlePosition;
	float3 gParticlePositionVariance;
	float3 gParticleVelocity;
	float3 gParticleVelocityVariance;
	float3 gParticleAcceleration;
	float3 gParticleAccelerationVariance;
	float4 gParticleColorStart;
	float4 gParticleColorStartVariance;
	float4 gParticleColorEnd;
	float4 gParticleColorEndVariance;
	float gParticleEnergyMax;
	float gParticleEnergyMin;
	float gParticleSizeStartMax;
	float gParticleSizeStartMin;
	float gParticleSizeEndMax;
	float gParticleSizeEndMin;
	float gParticleRotationPerParticleSpeedMin;
	float gParticleRotationPerParticleSpeedMax;
	float3 gParticleRotationAxis;
	float3 gParticleRotationAxisVariance;
	float gParticleRotationSpeedMin;
	float gParticleRotationSpeedMax;

	uint gTextureCount;
};
cbuffer cbFixed : register (b2)
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

float4 RandomVector4(float offset)
{
	//Use game time plus offset to sample random texture.
	float u = (gElapsedTime + offset);
	
	return gTextureRandom.SampleLevel(SamplerLinear, u, 0);
}
float GenerateScalar(float min, float max)
{
	return min + (max - min) * RandomVector4(0.0f).x;
}
float4 GenerateColor(float4 base, float4 variance)
{
	return base + variance * RandomVector4(0.0f);
}
float3 GenerateVectorInRect(float3 base, float3 variance)
{
	return base * variance * RandomVector4(0.0f).xyz;
}
float3 GenerateVectorInEllipsoid(float3 center, float3 scale)
{
	float3 res = float3(1,1,1);

	while (length(res) > 1.0f)
    {
        res = RandomVector4(0.0f).xyz;
    }

	res *= scale;
	res += center;

	return res;
}

/***********************************************
 STREAM OUT
***********************************************/
VSVertexParticle VSBasicStreamOut(VSVertexParticle input)
{
    return input;
}
[maxvertexcount(2)]
void GSStreamOutBasic(point VSVertexParticle input[1], inout PointStream<VSVertexParticle> ptStream)
{
	input[0].age += gElapsedTime;
	
	if(input[0].type == PT_EMITTER)
	{
		if(input[0].age > gEmissionRate)
		{
			input[0].age = 0.0f;

			//Initializes a new particle
			float3 position = gParticleEllipsoid == 0.0f ? GenerateVectorInRect(gParticlePosition, gParticlePositionVariance) : GenerateVectorInEllipsoid(gParticlePosition, gParticlePositionVariance);
			float3 velocity = GenerateVectorInRect(gParticleVelocity, gParticleVelocityVariance);
			float3 acceleration = GenerateVectorInRect(gParticleAcceleration, gParticleAccelerationVariance);
			float4 colorStart = GenerateColor(gParticleColorStart, gParticleColorStartVariance);
			float4 colorEnd = GenerateColor(gParticleColorEnd, gParticleColorEndVariance);
			float rotationPP = GenerateScalar(gParticleRotationPerParticleSpeedMin, gParticleRotationPerParticleSpeedMax);
			float3 rotationAxis = GenerateVectorInRect(gParticleRotationAxis, gParticleRotationAxisVariance);
			float rotation = GenerateScalar(gParticleRotationSpeedMin, gParticleRotationSpeedMax);
			float angle = GenerateScalar(0, rotationPP);
			float energy = GenerateScalar(gParticleEnergyMax, gParticleEnergyMin);
			float sizeStart = GenerateScalar(gParticleSizeStartMax, gParticleSizeStartMin);
			float sizeEnd = GenerateScalar(gParticleSizeEndMax, gParticleSizeEndMin);

			if(gParticleOrbit.x != 0.0f) position = mul(float4(position, 1), gRotation).xyz;
			if(gParticleOrbit.y != 0.0f) velocity = mul(float4(velocity, 1), gRotation).xyz;
			if(gParticleOrbit.z != 0.0f) acceleration = mul(float4(acceleration, 1), gRotation).xyz;

			if (rotation != 0.0f && length(rotationAxis) != 0.0f)
            {
				rotationAxis = mul(float4(rotationAxis, 1), gRotation).xyz;
            }

			position += gPosition;

			VSVertexParticle p;
			p.position = position;
			p.velocity = velocity;
			p.acceleration = acceleration;
			p.colorStart = colorStart;
			p.colorEnd = colorEnd;
			p.color = colorStart;
			p.rotationParticleSpeed = rotationPP;
			p.rotationAxis = rotationAxis;
			p.rotationSpeed = rotation;
			p.angle = angle;
			p.energyStart = energy;
			p.energy = energy;
			p.sizeStart = sizeStart;
			p.sizeEnd = sizeEnd;
			p.size = sizeStart;
			p.age = 0.0f;
			p.type = PT_FLARE;

			ptStream.Append(p);
		}
		
		ptStream.Append(input[0]);
	}
	else
	{
		input[0].energy -= gElapsedTime;

		if(input[0].energy > 0.0f)
		{
			//Updates particle state

			/*if (output.rotationSpeed != 0.0f && length(output.rotationAxis) != 0.0f)
			{
				Matrix pRotation;
				Matrix.RotationAxis(ref p.RotationAxis, p.RotationSpeed * gameTime.ElapsedSeconds, out pRotation);

				Vector3.TransformCoordinate(ref p.Velocity, ref pRotation, out p.Velocity);
				Vector3.TransformCoordinate(ref p.Acceleration, ref pRotation, out p.Acceleration);
			}*/

			float3 velocity = input[0].velocity + input[0].acceleration * gElapsedTime;
			float3 position = input[0].position + input[0].velocity * gElapsedTime;
			float angle = input[0].angle + input[0].rotationParticleSpeed * gElapsedTime;

			float percent = 1.0f - (input[0].energy / input[0].energyStart);
			float4 color = input[0].colorStart + (input[0].colorEnd - input[0].colorStart) * percent;
			float size = input[0].sizeStart + (input[0].sizeEnd - input[0].sizeStart) * percent;

			input[0].position = position;
			input[0].velocity = velocity;
			input[0].angle = angle;
			input[0].color = color;
			input[0].size = size;

			ptStream.Append(input[0]);
		}
	}
}

GeometryShader gsStreamOutBasic = ConstructGSWithSO(CompileShader(gs_5_0, GSStreamOutBasic()), "POSITION.xyz; VELOCITY.xyz; ACCELERATION.xyz; COLOR_START.rgba; COLOR_END.rgba; COLOR.rgba; ROTATION_PARTICLE_SPEED.x; ROTATION_AXIS.xyz; ROTATION_SPEED.x; ANGLE.x; ENERGY_START.x; ENERGY.x; SIZE_START.x; SIZE_END.x; SIZE.x; AGE.x; TYPE.x");

technique11 ParticleStreamOut
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSBasicStreamOut()));
        SetGeometryShader(gsStreamOutBasic);
        SetPixelShader(NULL);
    }
}

/***********************************************
 DRAW
***********************************************/
GSParticleBasic VSDrawBasic(VSVertexParticle input)
{
	//Updates particle state
	GSParticleBasic output;
	output.positionWorld = input.position;
	output.color = input.color;
	output.sizeWorld = float2(input.size, input.size);
	output.type  = input.type;
	
	return output;
}
[maxvertexcount(4)]
void GSDrawBasic(point GSParticleBasic input[1], uint primID : SV_PrimitiveID, inout TriangleStream<PSParticleSolid> triStream)
{
	if(input[0].type != PT_EMITTER)
	{
		float3 look  = normalize(gEyePositionWorld.xyz - input[0].positionWorld);
		float3 right = normalize(cross(float3(0, 1, 0), look));
		float3 up    = cross(look, right);

		float2 halfSize  = 0.5f * input[0].sizeWorld;
		float4 v[4];
		v[0] = float4(input[0].positionWorld + halfSize.x * right - halfSize.y * up, 1.0f);
		v[1] = float4(input[0].positionWorld + halfSize.x * right + halfSize.y * up, 1.0f);
		v[2] = float4(input[0].positionWorld - halfSize.x * right - halfSize.y * up, 1.0f);
		v[3] = float4(input[0].positionWorld - halfSize.x * right + halfSize.y * up, 1.0f);
		
		PSParticleSolid output;
		[unroll]
		for(int i = 0; i < 4; ++i)
		{
			v[i].y += halfSize.y;

			output.positionHomogeneous = mul(v[i], gWorldViewProjection);
			output.positionWorld = mul(v[i], gWorld).xyz;
			output.tex = gQuadTexC[i];
			output.color = input[0].color;
			output.primitiveID = primID;

			triStream.Append(output);
		}
	}
}
float4 PSDrawBasic(PSParticleSolid input) : SV_TARGET
{
	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 textureColor = gTextureArray.Sample(SamplerLinear, uvw);

	float3 litColor = (textureColor * input.color).rgb;

	if(gFogRange > 0)
	{
		float3 toEyeWorld = gEyePositionWorld - input.positionWorld;
		float distToEye = length(toEyeWorld);

		litColor = ComputeFog(litColor, distToEye, gFogStart, gFogRange, gFogColor.rgb);
	}

	return float4(litColor, textureColor.a);
}
GBufferPSOutput PSDeferredDrawBasic(PSParticleSolid input)
{
	GBufferPSOutput output = (GBufferPSOutput)0;

	float3 uvw = float3(input.tex, input.primitiveID % gTextureCount);
	float4 color = gTextureArray.Sample(SamplerLinear, uvw);
	float3 litColor = (color * input.color).rgb;
	
	output.color = float4(litColor, color.a);
	output.normal = 0.0f;
	output.depth = float4(input.positionHomogeneous.xyz, 0.0f);
	
	return output;
}

technique11 ParticleDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawBasic()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawBasic()));
        SetPixelShader(CompileShader(ps_5_0, PSDrawBasic()));
    }
}
technique11 DeferredParticleDraw
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VSDrawBasic()));
        SetGeometryShader(CompileShader(gs_5_0, GSDrawBasic()));
        SetPixelShader(CompileShader(ps_5_0, PSDeferredDrawBasic()));
    }
}
