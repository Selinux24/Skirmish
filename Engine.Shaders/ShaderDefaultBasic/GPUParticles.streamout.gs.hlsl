#include "..\Lib\IncLights.hlsl"
#include "..\Lib\IncVertexFormats.hlsl"

#define PT_EMITTER 0
#define PT_FLARE 1

cbuffer cbPerFrame : register(b0)
{
    float gTotalTime;
    float gElapsedTime;
    float gEmissionRate;
    float gVelocitySensitivity;

    float2 gHorizontalVelocity;
    float2 gVerticalVelocity;

    float4 gRandomValues;
};

[maxvertexcount(2)]
void main(point VSVertexGPUParticle input[1], inout PointStream<VSVertexGPUParticle> ptStream)
{
    if (input[0].type == PT_EMITTER)
    {
        if (input[0].emissionTime > 0)
        {
            input[0].maxAge -= gElapsedTime;
            input[0].emissionTime -= gElapsedTime;

            if (input[0].maxAge <= 0)
            {
                input[0].maxAge = gEmissionRate;

				//Adds a new particle
                float3 velocity = input[0].velocity * gVelocitySensitivity;
                float horizontalVelocity = lerp(gHorizontalVelocity.x, gHorizontalVelocity.y, gRandomValues.x);
                float horizontalAngle = PI * gRandomValues.y;

                velocity.x += horizontalVelocity * cos(horizontalAngle);
                velocity.z += horizontalVelocity * sin(horizontalAngle);
                velocity.y += lerp(gVerticalVelocity.x, gVerticalVelocity.y, gRandomValues.z);

                VSVertexGPUParticle p;
			
                p.position = input[0].position;
                p.velocity = velocity;
                p.random = gRandomValues;
                p.maxAge = gTotalTime;
                p.type = PT_FLARE;
                p.emissionTime = 0;

                ptStream.Append(p);
            }
	
			//Emitter in
            ptStream.Append(input[0]);
        }
    }
    else
    {
        if (input[0].maxAge > 0)
        {
			//Flares only remains if have enougth energy
            ptStream.Append(input[0]);
        }
    }
}

GeometryShader gsStreamOut = ConstructGSWithSO(CompileShader(gs_5_0, main()), "POSITION.xyz; VELOCITY.xyz; RANDOM.xyzw; MAX_AGE.x; TYPE.x; EMISSION_TIME.x");
